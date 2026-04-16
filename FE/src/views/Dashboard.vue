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
const LOC_API = 'https://localhost:7139/api/Locations'

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
            fetch(STOCK_API, { headers }).catch(()=>({ok:false})), 
            fetch(INBOUND_API, { headers }).catch(()=>({ok:false})), 
            fetch(OUTBOUND_API, { headers }).catch(()=>({ok:false})), 
            fetch(PROD_API, { headers }).catch(()=>({ok:false})),
            fetch(LOC_API, { headers }).catch(()=>({ok:false}))
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

const formatCurrency = (val) => new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(val || 0)

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
        // Dùng importPrice cho chuẩn với module Mua Hàng
        const price = prod.importPrice || prod.ImportPrice || prod.price || prod.Price || 0
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

// === BIỂU ĐỒ CỘT ===
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

// === BIỂU ĐỒ TRÒN ===
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
    if (categoryData.value.length === 0) return 'conic-gradient(#f3f4f6 0% 100%)';
    const stops = categoryData.value.map(d => `${d.color} ${d.startAngle}% ${d.endAngle}%`).join(', ');
    return `conic-gradient(${stops})`;
})

// === THANH TIẾN TRÌNH LẤP ĐẦY KHO ===
const capacityData = computed(() => {
    const whIds = validWarehouseIds.value;
    
    const validLocs = rawData.value.locations.filter(l => {
        const wId = l.warehouseId || l.WarehouseId || 1; 
        return whIds.includes(parseInt(wId));
    });
    const totalLocations = validLocs.length;

    const myStocks = rawData.value.stocks.filter(s => s.qty > 0 && s.locationId && whIds.includes(parseInt(s.warehouseId)));
    const usedLocationIds = new Set(myStocks.map(s => s.locationId));
    const usedLocations = usedLocationIds.size;

    let percentage = 0;
    if (totalLocations > 0) percentage = Math.min((usedLocations / totalLocations) * 100, 100);
    
    let colorClass = 'from-emerald-400 to-emerald-500';
    let textClass = 'text-emerald-800';
    if (percentage > 70) { colorClass = 'from-amber-400 to-amber-500'; textClass = 'text-amber-900'; }
    if (percentage > 90) { colorClass = 'from-red-500 to-red-600'; textClass = 'text-white'; }

    return { current: usedLocations, max: totalLocations, percentage: percentage.toFixed(1), colorClass, textClass };
})

// === TOP SẢN PHẨM XUẤT KHO ===
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
  <div class="space-y-6 animate-fade-in pb-10 px-0 md:px-2 max-w-7xl mx-auto">
    
    <div class="flex flex-col md:flex-row md:items-center justify-between gap-4 bg-white/70 backdrop-blur-xl p-5 md:p-6 rounded-3xl border border-white shadow-[0_4px_20px_-4px_rgba(0,0,0,0.05)]">
      <div>
        <h2 class="text-2xl md:text-3xl font-extrabold text-gray-900 tracking-tight flex items-center gap-3">
            Tổng quan Hệ thống
            <span v-if="viewScope === 'ALL'" class="bg-gradient-to-r from-blue-600 to-indigo-600 text-white text-[10px] px-2.5 py-1 rounded-md uppercase font-black tracking-widest shadow-md">Toàn công ty</span>
            <span v-else-if="viewScope === 'BRANCH'" class="bg-gradient-to-r from-teal-500 to-emerald-500 text-white text-[10px] px-2.5 py-1 rounded-md uppercase font-black tracking-widest shadow-md">Chi Nhánh</span>
            <span v-else class="bg-gradient-to-r from-purple-500 to-pink-500 text-white text-[10px] px-2.5 py-1 rounded-md uppercase font-black tracking-widest shadow-md">Kho Số {{ myWarehouseId }}</span>
        </h2>
        <p class="text-sm text-gray-500 mt-1 font-medium">Báo cáo trực quan dữ liệu thời gian thực</p>
      </div>

      <div v-if="viewScope === 'ALL' || viewScope === 'BRANCH'" class="flex flex-col sm:flex-row gap-3">
        <div v-if="viewScope === 'ALL'" class="relative">
            <select v-model="selectedBranch" @change="onBranchChange" class="appearance-none w-full border border-gray-200 rounded-xl pl-4 pr-10 py-2.5 outline-none focus:ring-2 focus:ring-blue-500 bg-white font-semibold text-gray-700 shadow-sm cursor-pointer transition-all hover:border-blue-300">
                <option value="">🏠 Tất cả Chi Nhánh</option>
                <option v-for="b in allBranches" :key="b.id||b.Id" :value="b.id||b.Id">{{ b.name||b.Name }}</option>
            </select>
            <div class="pointer-events-none absolute inset-y-0 right-0 flex items-center px-3 text-gray-500"><svg class="h-4 w-4" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 9l-7 7-7-7"></path></svg></div>
        </div>
        
        <div class="relative">
            <select v-model="selectedWarehouse" class="appearance-none w-full border border-gray-200 rounded-xl pl-4 pr-10 py-2.5 outline-none focus:ring-2 focus:ring-blue-500 bg-white font-semibold text-gray-700 shadow-sm cursor-pointer transition-all hover:border-blue-300">
                <option value="">📦 Tất cả các Kho</option>
                <option v-for="wh in allWarehouses.filter(w => !selectedBranch || w.branchId == selectedBranch)" :key="wh.id" :value="wh.id">{{ wh.name }}</option>
            </select>
            <div class="pointer-events-none absolute inset-y-0 right-0 flex items-center px-3 text-gray-500"><svg class="h-4 w-4" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 9l-7 7-7-7"></path></svg></div>
        </div>
      </div>
    </div>

    <div v-if="isLoading" class="flex flex-col items-center justify-center py-20 opacity-60">
        <div class="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600 mb-4"></div>
        <span class="text-gray-500 font-bold tracking-widest uppercase text-sm">Đang tải khối dữ liệu...</span>
    </div>
    
    <template v-else>
        <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-5 md:gap-6">
            <div class="relative bg-white p-6 rounded-3xl border border-gray-100 shadow-[0_8px_30px_rgb(0,0,0,0.04)] hover:shadow-[0_8px_30px_rgb(59,130,246,0.12)] transition-all duration-300 overflow-hidden group">
                <div class="absolute -right-8 -top-8 w-32 h-32 bg-gradient-to-br from-blue-50 to-blue-100 rounded-full opacity-50 group-hover:scale-[1.5] transition-transform duration-700 ease-out"></div>
                <div class="relative z-10 flex items-center justify-between mb-4">
                    <div class="w-12 h-12 bg-gradient-to-br from-blue-500 to-indigo-600 text-white rounded-2xl flex items-center justify-center shadow-lg shadow-blue-500/30">
                        <CurrencyDollarIcon class="w-6 h-6" />
                    </div>
                </div>
                <div class="relative z-10">
                    <h3 class="text-xs font-bold text-gray-400 mb-1 uppercase tracking-widest">Tổng Giá Trị Tồn</h3>
                    <p class="text-2xl xl:text-3xl font-black text-gray-900 tracking-tight truncate" :title="dashboardStats.totalValue">{{ dashboardStats.totalValue }}</p>
                </div>
                <div class="relative z-10 mt-5 border-t border-gray-100 pt-4">
                    <p class="text-xs font-semibold text-gray-500 flex items-center gap-1.5">Đang quản lý <strong class="text-blue-600 bg-blue-50 px-2 py-0.5 rounded-md">{{ dashboardStats.totalItems }}</strong> sản phẩm</p>
                </div>
            </div>
            
            <div class="relative bg-white p-6 rounded-3xl border border-gray-100 shadow-[0_8px_30px_rgb(0,0,0,0.04)] hover:shadow-[0_8px_30px_rgb(16,185,129,0.12)] transition-all duration-300 overflow-hidden group">
                <div class="absolute -right-8 -top-8 w-32 h-32 bg-gradient-to-br from-emerald-50 to-emerald-100 rounded-full opacity-50 group-hover:scale-[1.5] transition-transform duration-700 ease-out"></div>
                <div class="relative z-10 flex items-center justify-between mb-4">
                    <div class="w-12 h-12 bg-gradient-to-br from-emerald-400 to-teal-500 text-white rounded-2xl flex items-center justify-center shadow-lg shadow-emerald-500/30">
                        <DocumentArrowDownIcon class="w-6 h-6" />
                    </div>
                </div>
                <div class="relative z-10">
                    <h3 class="text-xs font-bold text-gray-400 mb-1 uppercase tracking-widest">Nhập Kho (Tháng)</h3>
                    <p class="text-3xl xl:text-4xl font-black text-gray-900 tracking-tight">{{ dashboardStats.inboundCount }} <span class="text-sm font-semibold text-gray-400">Phiếu</span></p>
                </div>
                <div class="relative z-10 mt-5 border-t border-gray-100 pt-4 flex justify-between items-center">
                    <p class="text-xs font-semibold text-gray-500">Giao dịch đã hoàn thành</p>
                    <span class="flex h-2 w-2 rounded-full bg-emerald-500 animate-pulse"></span>
                </div>
            </div>
            
            <div class="relative bg-white p-6 rounded-3xl border border-gray-100 shadow-[0_8px_30px_rgb(0,0,0,0.04)] hover:shadow-[0_8px_30px_rgb(245,158,11,0.12)] transition-all duration-300 overflow-hidden group">
                <div class="absolute -right-8 -top-8 w-32 h-32 bg-gradient-to-br from-amber-50 to-amber-100 rounded-full opacity-50 group-hover:scale-[1.5] transition-transform duration-700 ease-out"></div>
                <div class="relative z-10 flex items-center justify-between mb-4">
                    <div class="w-12 h-12 bg-gradient-to-br from-amber-400 to-orange-500 text-white rounded-2xl flex items-center justify-center shadow-lg shadow-amber-500/30">
                        <DocumentArrowUpIcon class="w-6 h-6" />
                    </div>
                </div>
                <div class="relative z-10">
                    <h3 class="text-xs font-bold text-gray-400 mb-1 uppercase tracking-widest">Xuất Kho (Tháng)</h3>
                    <p class="text-3xl xl:text-4xl font-black text-gray-900 tracking-tight">{{ dashboardStats.outboundCount }} <span class="text-sm font-semibold text-gray-400">Phiếu</span></p>
                </div>
                <div class="relative z-10 mt-5 border-t border-gray-100 pt-4">
                    <p class="text-xs font-semibold text-gray-500 flex items-center gap-1.5"><strong class="text-amber-600 bg-amber-50 px-2 py-0.5 rounded-md">{{ dashboardStats.pendingOrders }}</strong> đơn đang chờ xuất</p>
                </div>
            </div>
            
            <div class="relative bg-white p-6 rounded-3xl border border-gray-100 shadow-[0_8px_30px_rgb(0,0,0,0.04)] hover:shadow-[0_8px_30px_rgb(239,68,68,0.12)] transition-all duration-300 overflow-hidden group">
                <div class="absolute -right-8 -top-8 w-32 h-32 bg-gradient-to-br from-red-50 to-red-100 rounded-full opacity-50 group-hover:scale-[1.5] transition-transform duration-700 ease-out"></div>
                <div class="relative z-10 flex items-center justify-between mb-4">
                    <div class="w-12 h-12 bg-gradient-to-br from-red-500 to-rose-500 text-white rounded-2xl flex items-center justify-center shadow-lg shadow-red-500/30">
                        <ExclamationCircleIcon class="w-6 h-6" />
                    </div>
                </div>
                <div class="relative z-10">
                    <h3 class="text-xs font-bold text-gray-400 mb-1 uppercase tracking-widest">Cảnh Báo Kho</h3>
                    <p class="text-3xl xl:text-4xl font-black text-red-600 tracking-tight">{{ dashboardStats.activeAlerts }} <span class="text-sm font-semibold text-red-400">SKU</span></p>
                </div>
                <div class="relative z-10 mt-5 border-t border-gray-100 pt-4">
                    <p class="text-xs font-semibold text-red-500 bg-red-50 py-1.5 px-3 rounded-lg w-fit inline-block">Hết hàng / Cạn định mức</p>
                </div>
            </div>
        </div>

        <div class="bg-white rounded-3xl border border-gray-100 shadow-[0_4px_20px_-4px_rgba(0,0,0,0.03)] p-6 md:p-8 mt-6">
            <div class="flex flex-col sm:flex-row items-start sm:items-center justify-between mb-8 gap-4">
                <h3 class="text-lg font-bold text-gray-800 flex items-center gap-2"><PresentationChartBarIcon class="w-6 h-6 text-blue-500"/> Lưu Lượng Giao Dịch (6 tháng qua)</h3>
                <div class="flex gap-5 text-xs font-bold text-gray-500 uppercase tracking-widest bg-gray-50 px-4 py-2 rounded-xl border border-gray-100">
                    <span class="flex items-center gap-2"><div class="w-3 h-3 bg-emerald-500 rounded-full shadow-sm"></div> Nhập Kho</span>
                    <span class="flex items-center gap-2"><div class="w-3 h-3 bg-amber-500 rounded-full shadow-sm"></div> Xuất Kho</span>
                </div>
            </div>

            <div class="w-full h-64 sm:h-72 flex items-end justify-around border-b-2 border-gray-100 pb-2 relative mt-4">
                <div class="absolute left-0 w-full h-full flex flex-col justify-between z-0 pointer-events-none opacity-20">
                    <div class="w-full border-t border-dashed border-gray-500 flex items-start"><span class="text-[10px] text-gray-600 font-bold -mt-2.5 bg-white px-2 ml-1 rounded-full">{{ maxChartValue }}</span></div>
                    <div class="w-full border-t border-dashed border-gray-500 flex items-start"><span class="text-[10px] text-gray-600 font-bold -mt-2.5 bg-white px-2 ml-1 rounded-full">{{ Math.round(maxChartValue/2) }}</span></div>
                    <div class="w-full border-t border-solid border-gray-300"></div>
                </div>

                <div v-for="(col, idx) in chartData" :key="idx" class="flex flex-col items-center gap-2 z-10 w-[14%] relative group cursor-pointer">
                    <div class="flex items-end justify-center gap-2 sm:gap-3 w-full h-56 relative">
                        <div class="w-1/3 sm:w-10 bg-gradient-to-t from-emerald-600 to-emerald-400 rounded-t-xl shadow-md transition-all duration-300 group-hover:opacity-80" :style="`height: ${(col.in / maxChartValue) * 100}%`"></div>
                        <div class="w-1/3 sm:w-10 bg-gradient-to-t from-amber-500 to-orange-400 rounded-t-xl shadow-md transition-all duration-300 group-hover:opacity-80" :style="`height: ${(col.out / maxChartValue) * 100}%`"></div>
                        
                        <div class="absolute -top-14 left-1/2 -translate-x-1/2 bg-gray-900 text-white text-xs font-bold px-3 py-2 rounded-lg opacity-0 group-hover:opacity-100 transition-all duration-200 pointer-events-none shadow-xl whitespace-nowrap z-50 transform group-hover:-translate-y-2">
                            <p class="text-center border-b border-gray-700 pb-1 mb-1">{{ col.label }}</p>
                            <span class="text-emerald-400">Nhập: {{ col.in }}</span> | <span class="text-amber-400">Xuất: {{ col.out }}</span>
                            <div class="absolute -bottom-1 left-1/2 -translate-x-1/2 w-2 h-2 bg-gray-900 rotate-45"></div>
                        </div>
                    </div>
                    <span class="text-xs font-bold text-gray-500 mt-2 bg-gray-50 px-3 py-1 rounded-lg">{{ col.label }}</span>
                </div>
            </div>
        </div>

        <div class="grid grid-cols-1 lg:grid-cols-3 gap-6 mt-6">
            
            <div class="bg-white rounded-3xl border border-gray-100 shadow-[0_4px_20px_-4px_rgba(0,0,0,0.03)] p-6 flex flex-col items-center hover:shadow-lg transition-shadow duration-300">
                <h3 class="text-sm font-bold text-gray-500 uppercase tracking-widest mb-6 w-full text-left">Tỷ trọng Danh mục</h3>
                
                <div v-if="categoryData.length === 0" class="flex-1 flex flex-col items-center justify-center text-gray-400 italic">Kho đang trống</div>
                <template v-else>
                    <div class="relative w-48 h-48 rounded-full flex items-center justify-center shadow-lg transition-all duration-700 hover:scale-105" :style="{ background: pieGradient }">
                        <div class="w-32 h-32 bg-white rounded-full absolute flex flex-col items-center justify-center shadow-[inset_0_4px_10px_rgba(0,0,0,0.1)] z-10">
                            <span class="text-[10px] font-bold text-gray-400 uppercase tracking-widest mb-1">Tổng SL</span>
                            <span class="text-2xl font-black text-gray-800">{{ dashboardStats.totalItems }}</span>
                        </div>
                    </div>
                    <div class="w-full mt-8 space-y-3">
                        <div v-for="(cat, idx) in categoryData" :key="idx" class="flex items-center justify-between text-sm bg-gray-50/50 px-3 py-2 rounded-lg border border-gray-100 hover:bg-gray-50 transition-colors">
                            <div class="flex items-center gap-3 font-semibold text-gray-700">
                                <div class="w-4 h-4 rounded-full shadow-sm" :style="{ backgroundColor: cat.color }"></div> {{ cat.label }}
                            </div>
                            <span class="font-black text-gray-900 bg-white px-2 py-0.5 rounded shadow-sm border border-gray-100">{{ cat.percentage }}%</span>
                        </div>
                    </div>
                </template>
            </div>

            <div class="bg-white rounded-3xl border border-gray-100 shadow-[0_4px_20px_-4px_rgba(0,0,0,0.03)] p-6 flex flex-col justify-between hover:shadow-lg transition-shadow duration-300">
                <h3 class="text-sm font-bold text-gray-500 uppercase tracking-widest w-full text-left">Không gian Kho chứa</h3>
                
                <div class="flex flex-col items-center justify-center flex-1 py-6">
                    <template v-if="capacityData.max === 0">
                        <div class="text-center text-gray-400 text-sm flex flex-col items-center bg-gray-50 p-6 rounded-2xl border border-dashed border-gray-200">
                            <BuildingStorefrontIcon class="w-12 h-12 mb-3 text-gray-300"/>
                            <span class="font-semibold text-gray-600 mb-1">Chưa cấu hình Vị trí Kệ</span>
                            <span>Hệ thống không thể tính % lấp đầy không gian.</span>
                        </div>
                    </template>
                    <template v-else>
                        <CubeIcon class="w-16 h-16 mb-6" :class="capacityData.textClass" />
                        
                        <div class="relative w-full h-10 bg-gray-100 rounded-2xl overflow-hidden shadow-[inset_0_2px_4px_rgba(0,0,0,0.06)] mb-5 border border-gray-200">
                            <div class="absolute h-full transition-all duration-1000 ease-out bg-gradient-to-r progress-stripes shadow-[inset_0_2px_4px_rgba(255,255,255,0.3)]" 
                                 :class="capacityData.colorClass" 
                                 :style="`width: ${capacityData.percentage}%`">
                            </div>
                            <span class="absolute inset-0 flex items-center justify-center text-sm font-black text-gray-800 drop-shadow-md mix-blend-overlay">{{ capacityData.percentage }}% Đã sử dụng</span>
                        </div>
                        
                        <div class="flex items-center justify-between w-full px-4 py-3 bg-gray-50 rounded-xl border border-gray-100">
                            <div class="text-center">
                                <p class="text-[10px] uppercase font-bold text-gray-400 mb-1">Đang Dùng</p>
                                <p class="text-xl font-black text-gray-800">{{ capacityData.current }}</p>
                            </div>
                            <div class="h-8 w-px bg-gray-300"></div>
                            <div class="text-center">
                                <p class="text-[10px] uppercase font-bold text-gray-400 mb-1">Tổng Vị Trí Kệ</p>
                                <p class="text-xl font-black text-gray-800">{{ capacityData.max }}</p>
                            </div>
                        </div>
                    </template>
                </div>
            </div>

            <div class="bg-white rounded-3xl border border-gray-100 shadow-[0_4px_20px_-4px_rgba(0,0,0,0.03)] p-6 flex flex-col hover:shadow-lg transition-shadow duration-300">
                <h3 class="text-sm font-bold text-gray-500 uppercase tracking-widest mb-6 w-full text-left">Top Sản Phẩm Bán Chạy</h3>
                
                <div v-if="topOutboundProducts.length === 0" class="flex-1 flex flex-col items-center justify-center text-gray-400 italic text-sm">
                    <StarIcon class="w-12 h-12 mb-3 text-gray-200"/>
                    Chưa có dữ liệu Xuất kho
                </div>
                <div v-else class="space-y-4">
                    <div v-for="(prod, idx) in topOutboundProducts" :key="idx" class="flex items-center gap-4 p-3 bg-gray-50/50 hover:bg-white rounded-2xl transition-all border border-gray-100 hover:border-amber-200 hover:shadow-md group">
                        
                        <div class="w-10 h-10 rounded-xl flex items-center justify-center font-black text-base shrink-0 shadow-sm transition-colors"
                             :class="idx === 0 ? 'bg-gradient-to-br from-amber-400 to-yellow-500 text-white' : (idx === 1 ? 'bg-gradient-to-br from-gray-300 to-gray-400 text-white' : (idx === 2 ? 'bg-gradient-to-br from-orange-300 to-orange-400 text-white' : 'bg-white border border-gray-200 text-gray-500'))">
                            #{{ idx + 1 }}
                        </div>
                        
                        <div class="flex-1 min-w-0">
                            <p class="text-sm font-bold text-gray-800 truncate group-hover:text-amber-600 transition-colors">{{ prod.name }}</p>
                            <p class="text-[10px] font-semibold text-gray-400 mt-0.5 uppercase tracking-wider">{{ prod.sku }}</p>
                        </div>
                        
                        <div class="text-right">
                            <p class="text-lg font-black text-gray-900 group-hover:text-amber-600 transition-colors">{{ prod.qty }}</p>
                            <p class="text-[9px] font-bold text-gray-400 uppercase tracking-widest">Đã Xuất</p>
                        </div>
                    </div>
                </div>
            </div>

        </div>
    </template>
  </div>
</template>

<style scoped>
.animate-fade-in { animation: fadeIn 0.5s cubic-bezier(0.4, 0, 0.2, 1); }
@keyframes fadeIn { 
    from { opacity: 0; transform: translateY(15px); } 
    to { opacity: 1; transform: translateY(0); } 
}

/* CSS cho thanh sọc xéo chạy xịn sò */
.progress-stripes {
    background-image: linear-gradient(
        45deg, 
        rgba(255,255,255,0.2) 25%, 
        transparent 25%, 
        transparent 50%, 
        rgba(255,255,255,0.2) 50%, 
        rgba(255,255,255,0.2) 75%, 
        transparent 75%, 
        transparent
    );
    background-size: 1.5rem 1.5rem;
    animation: progress-stripes 1.5s linear infinite;
}
@keyframes progress-stripes {
    from { background-position: 1.5rem 0; }
    to { background-position: 0 0; }
}
</style>