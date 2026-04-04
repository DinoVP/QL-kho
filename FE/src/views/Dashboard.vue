<script setup>
import { ref, computed, onMounted } from 'vue'
import { 
  ChartBarIcon, ArrowTrendingUpIcon, ArrowTrendingDownIcon, 
  CubeIcon, CurrencyDollarIcon, PresentationChartLineIcon,
  ExclamationCircleIcon, ChartPieIcon, BuildingStorefrontIcon,
  DocumentArrowDownIcon, DocumentArrowUpIcon, PresentationChartBarIcon,
  MapPinIcon, StarIcon
} from '@heroicons/vue/24/outline'

const STOCK_API = 'https://localhost:7139/api/Stock'
const INBOUND_API = 'https://localhost:7139/api/Inbound'
const OUTBOUND_API = 'https://localhost:7139/api/Outbound'
const PROD_API = 'https://localhost:7139/api/Products'
const BRANCH_API = 'https://localhost:7139/api/Branches'
const LOC_API = 'https://localhost:7139/api/Locations' // THÊM API VỊ TRÍ ĐỂ TÍNH REAL DATA

const getAuthHeaders = () => ({ 'Content-Type': 'application/json', 'Authorization': 'Bearer ' + (localStorage.getItem('authToken') || '') })

// === 1. LẤY THÔNG TIN ĐĂNG NHẬP ===
const currentUserRole = ref((localStorage.getItem('role') || 'ql_kho').toLowerCase().trim()) 
const myBranchId = ref(parseInt(localStorage.getItem('branchId')) || 1) 
const myWarehouseId = ref(parseInt(localStorage.getItem('warehouseId')) || 1) 

const viewScope = computed(() => {
    const role = currentUserRole.value
    if (role.includes('admin') || role.includes('giam_doc') || role.includes('giám đốc')) return 'ALL'
    if (role.includes('gd_chi_nhanh') || role.includes('chi nhánh')) return 'BRANCH'
    return 'WAREHOUSE'
})

const selectedBranch = ref('')
const selectedWarehouse = ref('')
const allBranches = ref([])
const allWarehouses = ref([])

const onBranchChange = () => selectedWarehouse.value = ''

const isLoading = ref(false)
const rawData = ref({ stocks: [], inbounds: [], outbounds: [], products: [], locations: [] })

const fetchData = async () => {
    isLoading.value = true
    try {
        const headers = getAuthHeaders()
        
        if (viewScope.value === 'ALL' || viewScope.value === 'BRANCH') {
            try {
                const branchRes = await fetch(BRANCH_API, { headers })
                if (branchRes.ok) {
                    const branches = await branchRes.json()
                    allBranches.value = viewScope.value === 'ALL' ? branches : branches.filter(b => (b.id||b.Id) === myBranchId.value)
                    
                    let whList = []
                    for (const b of allBranches.value) {
                        const bId = b.id || b.Id;
                        const whRes = await fetch(`${BRANCH_API}/${bId}/warehouses-detail`, { headers })
                        if(whRes.ok) {
                            const whData = await whRes.json()
                            whList = [...whList, ...whData.map(w => ({ id: w.warehouseId, name: w.whname, branchId: bId }))]
                        }
                    }
                    allWarehouses.value = whList
                }
            } catch(e) { console.error("Lỗi lấy danh sách Kho:", e) }
        }

        const [stockRes, inbRes, outRes, prodRes, locRes] = await Promise.all([
            fetch(STOCK_API, { headers }), fetch(INBOUND_API, { headers }), 
            fetch(OUTBOUND_API, { headers }), fetch(PROD_API, { headers }),
            fetch(LOC_API, { headers })
        ])
        
        if (prodRes.ok) rawData.value.products = await prodRes.json()
        if (locRes.ok) rawData.value.locations = await locRes.json()
        if (stockRes.ok) rawData.value.stocks = await stockRes.json()
        if (inbRes.ok) rawData.value.inbounds = await inbRes.json()
        if (outRes.ok) rawData.value.outbounds = await outRes.json()
        else rawData.value.outbounds = [] 

    } catch (error) { console.error('Lỗi tải Dashboard:', error) }
    finally { isLoading.value = false }
}

const validWarehouseIds = computed(() => {
    if (viewScope.value === 'WAREHOUSE') return [myWarehouseId.value]
    let validWhs = allWarehouses.value
    if (viewScope.value === 'BRANCH') validWhs = validWhs.filter(w => w.branchId === myBranchId.value)
    if (selectedBranch.value) validWhs = validWhs.filter(w => w.branchId == selectedBranch.value)
    if (selectedWarehouse.value) return [parseInt(selectedWarehouse.value)]
    return validWhs.map(w => w.id)
})

const formatCurrency = (val) => new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(val)

const dashboardStats = computed(() => {
    const whIds = validWarehouseIds.value;
    const myStocks = rawData.value.stocks.filter(s => whIds.includes(parseInt(s.warehouseId)))
    const myInbounds = rawData.value.inbounds.filter(i => whIds.includes(parseInt(i.warehouseId)))
    const myOutbounds = rawData.value.outbounds.filter(o => whIds.includes(parseInt(o.warehouseId)))

    let totalValue = 0;
    let totalItems = 0;
    let alertCount = 0;

    myStocks.forEach(s => {
        const prod = rawData.value.products.find(p => p.id === (s.variantId || s.VariantId)) || {}
        const price = prod.price || prod.Price || 0
        totalValue += (s.qty || 0) * price
        totalItems += (s.qty || 0)

        const convRate = prod.conversionRate || prod.ConversionRate || 24
        const minStock = prod.minStock || prod.MinStock || 10
        if (s.qty === 0 || ((s.qty || 0) * convRate) <= minStock) alertCount++
    })

    const currentMonth = new Date().getMonth()
    const currentYear = new Date().getFullYear()

    const inThisMonth = myInbounds.filter(i => {
        const d = new Date(i.date)
        return i.status === 'completed' && d.getMonth() === currentMonth && d.getFullYear() === currentYear
    }).length

    const outThisMonth = myOutbounds.filter(o => {
        const d = new Date(o.date)
        return o.status === 'completed' && d.getMonth() === currentMonth && d.getFullYear() === currentYear
    }).length

    const pendingOrders = myOutbounds.filter(o => o.status === 'pending').length

    return { totalValue: formatCurrency(totalValue), totalItems, inboundCount: inThisMonth, outboundCount: outThisMonth, pendingOrders, activeAlerts: alertCount }
})

// === BIỂU ĐỒ CỘT (THẬT) ===
const last6Months = computed(() => {
    const months = []
    for(let i=5; i>=0; i--) {
        const d = new Date()
        d.setMonth(d.getMonth() - i)
        months.push({ month: d.getMonth(), year: d.getFullYear(), label: `T${d.getMonth()+1}/${d.getFullYear().toString().slice(2)}` })
    }
    return months
})

const chartData = computed(() => {
    const whIds = validWarehouseIds.value;
    const myInbounds = rawData.value.inbounds.filter(i => i.status === 'completed' && whIds.includes(parseInt(i.warehouseId)))
    const myOutbounds = rawData.value.outbounds.filter(o => o.status === 'completed' && whIds.includes(parseInt(o.warehouseId)))

    return last6Months.value.map(m => {
        const inCount = myInbounds.filter(i => new Date(i.date).getMonth() === m.month && new Date(i.date).getFullYear() === m.year).length
        const outCount = myOutbounds.filter(o => new Date(o.date).getMonth() === m.month && new Date(o.date).getFullYear() === m.year).length
        return { label: m.label, in: inCount, out: outCount }
    })
})

const maxChartValue = computed(() => {
    if(chartData.value.length === 0) return 10;
    const maxIn = Math.max(...chartData.value.map(d => d.in))
    const maxOut = Math.max(...chartData.value.map(d => d.out))
    return Math.max(maxIn, maxOut, 10) 
})

// === BIỂU ĐỒ TRÒN (THẬT TỪ BẢNG ITM_PRODUCTS VÀ WMS_STOCKBALANCES) ===
const categoryData = computed(() => {
    const whIds = validWarehouseIds.value;
    const myStocks = rawData.value.stocks.filter(s => whIds.includes(parseInt(s.warehouseId)));
    const map = {};
    let total = 0;
    
    myStocks.forEach(s => {
        const prod = rawData.value.products.find(p => p.id === (s.variantId || s.VariantId)) || {};
        const cat = prod.categoryName || prod.CategoryName || 'Chung';
        if (!map[cat]) map[cat] = 0;
        map[cat] += (s.qty || 0);
        total += (s.qty || 0);
    });

    if (total === 0) return [];
    
    const colors = ['#3b82f6', '#10b981', '#f59e0b', '#8b5cf6', '#ef4444', '#ec4899'];
    let currentAngle = 0;
    
    return Object.keys(map).map((cat, idx) => {
        const percentage = (map[cat] / total) * 100;
        const startAngle = currentAngle;
        const endAngle = currentAngle + percentage;
        currentAngle = endAngle;
        return { label: cat, value: map[cat], percentage: percentage.toFixed(1), color: colors[idx % colors.length], startAngle, endAngle };
    }).sort((a, b) => b.value - a.value);
})

const pieGradient = computed(() => {
    if (categoryData.value.length === 0) return 'conic-gradient(#e5e7eb 0% 100%)';
    const stops = categoryData.value.map(d => `${d.color} ${d.startAngle}% ${d.endAngle}%`).join(', ');
    return `conic-gradient(${stops})`;
})

// === THANH TIẾN TRÌNH: TỈ LỆ LẤP ĐẦY KHO (THẬT TỪ BẢNG WMS_LOCATIONS) ===
const capacityData = computed(() => {
    const whIds = validWarehouseIds.value;
    
    // Đếm tổng số vị trí kệ (Locations) thuộc các kho đang xem
    const validLocs = rawData.value.locations.filter(l => {
        const wId = l.warehouseId || l.WarehouseId || 1; // Giả định nếu API trả về warehouseId
        return whIds.includes(parseInt(wId));
    });
    const totalLocations = validLocs.length;

    // Đếm số vị trí kệ ĐANG ĐƯỢC SỬ DỤNG
    const myStocks = rawData.value.stocks.filter(s => s.qty > 0 && s.locationId && whIds.includes(parseInt(s.warehouseId)));
    const usedLocationIds = new Set(myStocks.map(s => s.locationId));
    const usedLocations = usedLocationIds.size;

    let percentage = 0;
    if (totalLocations > 0) percentage = Math.min((usedLocations / totalLocations) * 100, 100);
    
    let colorClass = 'bg-emerald-500';
    if (percentage > 70) colorClass = 'bg-amber-500';
    if (percentage > 90) colorClass = 'bg-red-500';

    return { current: usedLocations, max: totalLocations, percentage: percentage.toFixed(1), colorClass };
})

// === TOP SẢN PHẨM XUẤT KHO (THẬT TỪ BẢNG SAL_DELIVERYLINES / OUTBOUND) ===
const topOutboundProducts = computed(() => {
    const whIds = validWarehouseIds.value;
    const myOutbounds = rawData.value.outbounds.filter(o => o.status === 'completed' && whIds.includes(parseInt(o.warehouseId)));
    
    const map = {};
    myOutbounds.forEach(out => {
        (out.items || []).forEach(item => {
            const vId = item.variantId || item.VariantId;
            if(!map[vId]) map[vId] = 0;
            map[vId] += (item.qty || 0);
        });
    });

    return Object.keys(map).map(vId => {
        const prod = rawData.value.products.find(p => p.id == vId || p.Id == vId) || {};
        return {
            sku: prod.sku || prod.Sku,
            name: prod.name || prod.Name,
            qty: map[vId]
        };
    }).sort((a,b) => b.qty - a.qty).slice(0, 4);
})

onMounted(() => fetchData())
</script>

<template>
  <div class="space-y-6 animate-fade-in pb-10 px-0 md:px-1">
    
    <div class="flex flex-col md:flex-row md:items-center justify-between gap-4 bg-white p-4 md:p-5 rounded-2xl border border-gray-100 shadow-sm">
      <div>
        <h2 class="text-xl md:text-2xl font-bold text-gray-800 flex items-center gap-2">
            Tổng quan Hệ thống
            <span v-if="viewScope === 'ALL'" class="bg-primary-100 text-primary-800 text-[10px] px-2 py-0.5 rounded border border-primary-200 uppercase font-black tracking-wider">Cấp: Toàn công ty</span>
            <span v-else-if="viewScope === 'BRANCH'" class="bg-blue-100 text-blue-800 text-[10px] px-2 py-0.5 rounded border border-blue-200 uppercase font-black tracking-wider">Cấp: Chi Nhánh</span>
            <span v-else class="bg-emerald-100 text-emerald-800 text-[10px] px-2 py-0.5 rounded border border-emerald-200 uppercase font-black tracking-wider">Cấp: Nội bộ Kho {{ myWarehouseId }}</span>
        </h2>
        <p class="text-xs md:text-sm text-gray-500 mt-1">Dữ liệu thời gian thực đồng bộ từ Database</p>
      </div>

      <div v-if="viewScope === 'ALL' || viewScope === 'BRANCH'" class="flex flex-col sm:flex-row gap-2">
        <select v-if="viewScope === 'ALL'" v-model="selectedBranch" @change="onBranchChange" class="border border-gray-200 rounded-lg text-sm px-3 py-2 outline-none focus:ring-1 focus:ring-primary-500 bg-gray-50 font-semibold cursor-pointer">
            <option value="">Tất cả Chi Nhánh</option>
            <option v-for="b in allBranches" :key="b.id||b.Id" :value="b.id||b.Id">{{ b.name||b.Name }}</option>
        </select>
        <select v-model="selectedWarehouse" class="border border-gray-200 rounded-lg text-sm px-3 py-2 outline-none focus:ring-1 focus:ring-primary-500 bg-gray-50 font-semibold cursor-pointer">
            <option value="">Tất cả các Kho</option>
            <option v-for="wh in allWarehouses.filter(w => !selectedBranch || w.branchId == selectedBranch)" :key="wh.id" :value="wh.id">{{ wh.name }}</option>
        </select>
      </div>
    </div>

    <div v-if="isLoading" class="text-center py-10"><span class="text-gray-500 font-bold">Đang tải dữ liệu báo cáo...</span></div>
    
    <template v-else>
        <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4 md:gap-6">
            <div class="bg-white p-5 rounded-2xl border border-gray-100 shadow-sm flex flex-col relative overflow-hidden group hover:shadow-md transition-shadow">
                <div class="absolute -right-4 -top-4 w-16 h-16 bg-blue-50 rounded-full group-hover:scale-150 transition-transform duration-500"></div>
                <div class="flex items-center justify-between mb-4 relative z-10"><h3 class="text-sm font-semibold text-gray-500 uppercase tracking-wider">Tổng Giá Trị Tồn</h3><CurrencyDollarIcon class="w-6 h-6 text-blue-500" /></div>
                <p class="text-2xl font-extrabold text-gray-800 relative z-10 truncate" :title="dashboardStats.totalValue">{{ dashboardStats.totalValue }}</p>
                <p class="text-xs font-medium text-blue-600 mt-2 relative z-10 flex items-center gap-1">Đang chứa {{ dashboardStats.totalItems }} đơn vị hàng</p>
            </div>
            
            <div class="bg-white p-5 rounded-2xl border border-gray-100 shadow-sm flex flex-col relative overflow-hidden group hover:shadow-md transition-shadow">
                <div class="absolute -right-4 -top-4 w-16 h-16 bg-emerald-50 rounded-full group-hover:scale-150 transition-transform duration-500"></div>
                <div class="flex items-center justify-between mb-4 relative z-10"><h3 class="text-sm font-semibold text-gray-500 uppercase tracking-wider">Nhập Kho (Tháng này)</h3><DocumentArrowDownIcon class="w-6 h-6 text-emerald-500" /></div>
                <p class="text-3xl font-extrabold text-gray-800 relative z-10">{{ dashboardStats.inboundCount }}</p>
                <p class="text-xs font-medium text-emerald-600 mt-2 relative z-10">Phiếu nhập đã hoàn thành</p>
            </div>
            
            <div class="bg-white p-5 rounded-2xl border border-gray-100 shadow-sm flex flex-col relative overflow-hidden group hover:shadow-md transition-shadow">
                <div class="absolute -right-4 -top-4 w-16 h-16 bg-amber-50 rounded-full group-hover:scale-150 transition-transform duration-500"></div>
                <div class="flex items-center justify-between mb-4 relative z-10"><h3 class="text-sm font-semibold text-gray-500 uppercase tracking-wider">Xuất Kho (Tháng này)</h3><DocumentArrowUpIcon class="w-6 h-6 text-amber-500" /></div>
                <p class="text-3xl font-extrabold text-gray-800 relative z-10">{{ dashboardStats.outboundCount }}</p>
                <p class="text-xs font-medium text-amber-600 mt-2 relative z-10 flex items-center gap-1">{{ dashboardStats.pendingOrders }} đơn đang chờ xuất kho</p>
            </div>
            
            <div class="bg-white p-5 rounded-2xl border border-gray-100 shadow-sm flex flex-col relative overflow-hidden group hover:shadow-md transition-shadow">
                <div class="absolute -right-4 -top-4 w-16 h-16 bg-red-50 rounded-full group-hover:scale-150 transition-transform duration-500"></div>
                <div class="flex items-center justify-between mb-4 relative z-10"><h3 class="text-sm font-semibold text-gray-500 uppercase tracking-wider">Cảnh báo Tồn Kho</h3><ExclamationCircleIcon class="w-6 h-6 text-red-500" /></div>
                <p class="text-3xl font-extrabold text-red-600 relative z-10">{{ dashboardStats.activeAlerts }}</p>
                <p class="text-xs font-medium text-red-500 mt-2 relative z-10 animate-pulse">Sản phẩm Cạn kho / Hết hạn</p>
            </div>
        </div>

        <div class="bg-white rounded-2xl border border-gray-100 shadow-sm p-5 md:p-6 mt-6">
            <div class="flex items-center justify-between mb-6">
                <h3 class="text-base font-bold text-gray-800 flex items-center gap-2"><PresentationChartBarIcon class="w-5 h-5 text-indigo-500"/> Biểu đồ Lượng Giao Dịch (6 tháng gần nhất)</h3>
                <div class="flex gap-4 text-xs font-bold text-gray-500 uppercase tracking-wider">
                    <span class="flex items-center gap-1"><div class="w-3 h-3 bg-gradient-to-t from-emerald-500 to-emerald-300 rounded-sm shadow-sm"></div> Nhập Kho</span>
                    <span class="flex items-center gap-1"><div class="w-3 h-3 bg-gradient-to-t from-amber-500 to-amber-300 rounded-sm shadow-sm"></div> Xuất Kho</span>
                </div>
            </div>

            <div class="w-full h-56 sm:h-64 flex items-end justify-around border-b-2 border-gray-200 pb-2 relative mt-4">
                <div class="absolute left-0 w-full h-full flex flex-col justify-between z-0 pointer-events-none opacity-30">
                    <div class="w-full border-t border-dashed border-gray-400 flex items-start"><span class="text-[10px] text-gray-600 -mt-2 bg-white px-1 ml-2">{{ maxChartValue }} phiếu</span></div>
                    <div class="w-full border-t border-dashed border-gray-400 flex items-start"><span class="text-[10px] text-gray-600 -mt-2 bg-white px-1 ml-2">{{ Math.round(maxChartValue/2) }} phiếu</span></div>
                    <div class="w-full border-t border-dashed border-gray-400"></div>
                </div>

                <div v-for="(col, idx) in chartData" :key="idx" class="flex flex-col items-center gap-1 z-10 w-[14%] relative group">
                    <div class="flex items-end justify-center gap-1 sm:gap-2 w-full h-48">
                        <div class="w-1/3 sm:w-8 bg-gradient-to-t from-emerald-500 to-emerald-300 rounded-t-md shadow-sm transition-all duration-500 hover:brightness-110 relative" :style="`height: ${(col.in / maxChartValue) * 100}%`">
                            <span class="absolute -top-6 left-1/2 -translate-x-1/2 bg-emerald-800 text-white text-[10px] font-bold px-1.5 py-0.5 rounded opacity-0 group-hover:opacity-100 transition-opacity">{{ col.in }}</span>
                        </div>
                        <div class="w-1/3 sm:w-8 bg-gradient-to-t from-amber-500 to-amber-300 rounded-t-md shadow-sm transition-all duration-500 hover:brightness-110 relative" :style="`height: ${(col.out / maxChartValue) * 100}%`">
                            <span class="absolute -top-6 left-1/2 -translate-x-1/2 bg-amber-800 text-white text-[10px] font-bold px-1.5 py-0.5 rounded opacity-0 group-hover:opacity-100 transition-opacity">{{ col.out }}</span>
                        </div>
                    </div>
                    <span class="text-[10px] sm:text-xs font-bold text-gray-600 mt-2">{{ col.label }}</span>
                </div>
            </div>
        </div>

        <div class="grid grid-cols-1 lg:grid-cols-3 gap-6 mt-6">
            
            <div class="bg-white rounded-2xl border border-gray-100 shadow-sm p-5 md:p-6 flex flex-col items-center">
                <h3 class="text-base font-bold text-gray-800 mb-6 flex items-center gap-2 w-full text-left"><ChartPieIcon class="w-5 h-5 text-indigo-500"/> Cơ cấu Nhóm hàng (Tồn)</h3>
                
                <div v-if="categoryData.length === 0" class="flex-1 flex flex-col items-center justify-center text-gray-400 italic">Kho đang trống</div>
                <template v-else>
                    <div class="relative w-40 h-40 rounded-full flex items-center justify-center shadow-md transition-all duration-700" :style="{ background: pieGradient }">
                        <div class="w-24 h-24 bg-white rounded-full absolute flex flex-col items-center justify-center shadow-inner">
                            <span class="text-[10px] font-bold text-gray-400 uppercase tracking-widest">Tổng</span>
                            <span class="text-xl font-black text-gray-800">{{ dashboardStats.totalItems }}</span>
                        </div>
                    </div>
                    <div class="w-full mt-6 space-y-2">
                        <div v-for="(cat, idx) in categoryData" :key="idx" class="flex items-center justify-between text-sm">
                            <div class="flex items-center gap-2 font-semibold text-gray-600">
                                <div class="w-3 h-3 rounded-full" :style="{ backgroundColor: cat.color }"></div> {{ cat.label }}
                            </div>
                            <span class="font-bold text-gray-800">{{ cat.percentage }}%</span>
                        </div>
                    </div>
                </template>
            </div>

            <div class="bg-white rounded-2xl border border-gray-100 shadow-sm p-5 md:p-6 flex flex-col justify-between">
                <h3 class="text-base font-bold text-gray-800 flex items-center gap-2 w-full text-left"><MapPinIcon class="w-5 h-5 text-indigo-500"/> Tỉ lệ Lấp đầy Kho (Real Data)</h3>
                <div class="flex flex-col items-center justify-center flex-1 py-4">
                    <template v-if="capacityData.max === 0">
                        <div class="text-center text-gray-400 italic text-sm mt-4 flex flex-col items-center">
                            <BuildingStorefrontIcon class="w-10 h-10 mb-2 text-gray-200"/>
                            Kho chưa được thiết lập Vị trí Kệ (Locations).<br>Hệ thống không thể tính % lấp đầy.
                        </div>
                    </template>
                    <template v-else>
                        <div class="relative w-full h-8 bg-gray-100 rounded-full overflow-hidden shadow-inner mb-4 border border-gray-200">
                            <div class="h-full transition-all duration-1000 ease-out" :class="capacityData.colorClass" :style="`width: ${capacityData.percentage}%`"></div>
                            <span class="absolute inset-0 flex items-center justify-center text-xs font-black text-gray-800 mix-blend-overlay">{{ capacityData.percentage }}% Lấp đầy</span>
                        </div>
                        <p class="text-sm text-gray-500 font-medium text-center">Đang dùng <strong class="text-gray-800">{{ capacityData.current }}</strong> / tổng <strong class="text-gray-800">{{ capacityData.max }}</strong> vị trí kệ</p>
                    </template>
                </div>
            </div>

            <div class="bg-white rounded-2xl border border-gray-100 shadow-sm p-5 md:p-6 flex flex-col">
                <h3 class="text-base font-bold text-gray-800 mb-4 flex items-center gap-2 w-full text-left"><StarIcon class="w-5 h-5 text-amber-500"/> Top Xuất Kho</h3>
                
                <div v-if="topOutboundProducts.length === 0" class="flex-1 flex flex-col items-center justify-center text-gray-400 italic text-sm">Chưa có dữ liệu Xuất kho</div>
                <div v-else class="space-y-3 mt-2">
                    <div v-for="(prod, idx) in topOutboundProducts" :key="idx" class="flex items-center gap-3 p-2 hover:bg-gray-50 rounded-lg transition-colors border border-transparent hover:border-gray-100">
                        <div class="w-8 h-8 rounded bg-indigo-50 text-indigo-600 flex items-center justify-center font-black text-sm shrink-0 border border-indigo-100">#{{ idx + 1 }}</div>
                        <div class="flex-1 min-w-0">
                            <p class="text-sm font-bold text-gray-800 truncate">{{ prod.name }}</p>
                            <p class="text-[10px] text-gray-500">{{ prod.sku }}</p>
                        </div>
                        <div class="text-right">
                            <p class="text-sm font-black text-amber-600">{{ prod.qty }}</p>
                            <p class="text-[9px] font-bold text-gray-400 uppercase">Đơn vị</p>
                        </div>
                    </div>
                </div>
            </div>

        </div>
    </template>
  </div>
</template>

<style scoped>
.animate-fade-in { animation: fadeIn 0.4s ease-out; }
@keyframes fadeIn { from { opacity: 0; transform: translateY(10px); } to { opacity: 1; transform: translateY(0); } }
</style>