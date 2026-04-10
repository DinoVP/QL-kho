<script setup>
import { ref, computed, onMounted, watch } from 'vue'
import { 
  MagnifyingGlassIcon, ArchiveBoxIcon, MapPinIcon,
  ExclamationTriangleIcon, EyeIcon, XMarkIcon, 
  DocumentTextIcon, ArrowPathIcon, ArrowsRightLeftIcon,
  ChevronLeftIcon, ChevronRightIcon
} from '@heroicons/vue/24/outline'

const STOCK_API = 'https://localhost:7139/api/Stock'
const PROD_API = 'https://localhost:7139/api/Products'
const LOC_API = 'https://localhost:7139/api/Locations'
const BRANCH_API = 'https://localhost:7139/api/Branches' 

const getAuthHeaders = () => ({
  'Content-Type': 'application/json',
  'Authorization': 'Bearer ' + (localStorage.getItem('authToken') || '')
})

const myWarehouseId = ref(parseInt(localStorage.getItem('warehouseId')) || null)

const inventoryList = ref([])
const productsList = ref([])
const locationsList = ref([])
const warehousesList = ref([])
const isLoading = ref(false)

// =======================================================================
// 1. TẠO CHUỖI QUY ĐỔI GỐC TỪ SẢN PHẨM
// =======================================================================
const getUnitChain = (prod) => {
    let units = [{ name: prod.unit || prod.Unit || 'SL', rate: 1, rel: 1 }];
    const conversions = prod.conversions || prod.Conversions || [];
    
    if (conversions.length > 0) {
        const sortedConvs = [...conversions].sort((a,b) => a.rate - b.rate);
        let prevRate = 1;
        sortedConvs.forEach(c => {
            units.push({ name: c.altUnit, rate: c.rate, rel: c.rate / prevRate });
            prevRate = c.rate;
        });
    } else if ((prod.conversionRate || prod.ConversionRate) > 1) {
        const cr = prod.conversionRate || prod.ConversionRate;
        units.push({ name: 'Thùng/Kiện', rate: cr, rel: cr });
    }
    return units.sort((a,b) => a.rate - b.rate);
}

// =======================================================================
// 2. PHÂN RÃ THỰC TẾ (Ví dụ: 1 Pallet + 1 Thùng to)
// =======================================================================
const formatPhysicalStock = (qty, units, baseUnit) => {
    if (qty <= 0) return `0 ${baseUnit}`;
    let remainingQty = qty;
    let result = [];
    const sortedPacks = [...(units || [])].filter(u => u.rate > 1).sort((a, b) => b.rate - a.rate);
    
    for (const pack of sortedPacks) {
        const count = Math.floor(remainingQty / pack.rate);
        if (count > 0) {
            result.push(`${count} ${pack.name}`);
            remainingQty %= pack.rate;
        }
    }
    if (remainingQty > 0 || result.length === 0) {
        result.push(`${remainingQty} ${baseUnit}`);
    }
    return result.join(' + ');
}

// =======================================================================
// 3. QUY ĐỔI TƯƠNG ĐƯƠNG (Ví dụ: 1.5 Pallet = 3 Thùng to = 600 SL)
// =======================================================================
const formatEquivalentStock = (qty, units, baseUnit) => {
    if (qty <= 0) return `0 ${baseUnit}`;
    let parts = [];
    const reversedUnits = [...(units || [])].sort((a, b) => b.rate - a.rate);
    for (const u of reversedUnits) {
        let eq = Number((qty / u.rate).toFixed(2));
        parts.push(`${eq} ${u.name}`);
    }
    return parts.join(' = ');
}

// =======================================================================
// 4. CHUỖI CÔNG THỨC GỐC ĐỂ HIỂN THỊ DÒNG QUY CÁCH
// =======================================================================
const getFormulaChain = (product) => {
    const sortedPacks = [...(product.units || [])].filter(u => u.rate > 1).sort((a, b) => b.rate - a.rate);
    if (sortedPacks.length === 0) {
        if (product.packSize && product.packSize !== 'N/A') return product.packSize;
        return `Hàng lẻ (Không cấu hình đa cấp)`;
    }
    let result = [`1 ${sortedPacks[0].name}`];
    const topRate = sortedPacks[0].rate;
    for (let i = 1; i < sortedPacks.length; i++) {
        result.push(`${topRate / sortedPacks[i].rate} ${sortedPacks[i].name}`);
    }
    result.push(`${topRate} ${product.baseUnit}`);
    return result.join(' = ');
}

const fetchData = async () => {
  isLoading.value = true
  try {
    const headers = getAuthHeaders()
    try {
        const branchRes = await fetch(BRANCH_API, { headers })
        if (branchRes.ok) {
            const branches = await branchRes.json()
            let allWh = []
            for (const b of branches) {
                const bId = b.id || b.Id;
                const whRes = await fetch(`${BRANCH_API}/${bId}/warehouses-detail`, { headers })
                if(whRes.ok) {
                    const whData = await whRes.json()
                    allWh = [...allWh, ...whData.map(w => ({ id: w.warehouseId, name: w.whname, branchId: bId }))]
                }
            }
            warehousesList.value = allWh
        }
    } catch(e) { console.error("Lỗi tải danh sách Kho", e) }

    const [stockRes, prodRes, locRes] = await Promise.all([
      fetch(STOCK_API, { headers }), fetch(PROD_API, { headers }), fetch(LOC_API, { headers })
    ])
    
    if (prodRes.ok) productsList.value = await prodRes.json()
    if (locRes.ok) locationsList.value = await locRes.json()
    
    if (stockRes.ok) {
      const rawStocks = await stockRes.json()
      const mappedStocks = rawStocks.map(s => {
        const prod = productsList.value.find(p => p.id === s.variantId || p.Id === s.variantId) || {};
        const loc = locationsList.value.find(l => l.id === s.locationId || l.Id === s.locationId) || {};
        const whId = s.warehouseId || loc.warehouseId || 1;
        const wh = warehousesList.value.find(w => w.id === whId) || {};
        
        const rawConv = prod.quyCach || prod.QuyCach || prod.conversionRate || prod.ConversionRate;
        const convRate = (!rawConv || rawConv === 'N/A' || rawConv === 'null') ? 1 : (parseInt(rawConv) || 1);
        const isPendingZone = !s.locationId; 
        
        const baseUnit = prod.unit || prod.Unit || 'SL';
        const units = getUnitChain(prod);

        return {
          id: s.id, variantId: s.variantId, 
          qty: s.qty || 0, // Dữ liệu gốc trả về là Base Qty (SL Lẻ)
          qtyPending: isPendingZone ? (s.qty || 0) : 0, 
          qtyRack: !isPendingZone ? (s.qty || 0) : 0,   
          
          nsx: s.nsx || '', hsd: s.hsd || '', sku: prod.sku || prod.Sku || 'N/A', name: prod.name || prod.Name || 'Sản phẩm không xác định',
          unit: baseUnit, baseUnit: baseUnit, units: units, conversionRate: convRate, packSize: prod.packSize || prod.PackSize || '',
          
          category: prod.categoryName || prod.CategoryName || 'Chung', minStock: prod.minStock || prod.MinStock || 10, 
          locationId: s.locationId, locationCode: loc.code || loc.Code || 'Khu Chờ Nhập', warehouseId: whId, warehouse: wh.name || 'Kho Tổng' 
        }
      })
      inventoryList.value = mappedStocks.filter(item => !myWarehouseId.value || item.warehouseId === myWarehouseId.value)
    }
  } catch (error) { console.error('Lỗi tải dữ liệu tồn kho:', error) } 
  finally { isLoading.value = false }
}

const totalSkus = computed(() => { const uniqueSkus = new Set(inventoryList.value.map(item => item.sku)); return uniqueSkus.size; })
const getAbsQty = (item) => item.qty || 0; 

const lowStockCount = computed(() => inventoryList.value.filter(item => item.qty > 0 && getAbsQty(item) <= item.minStock).length)
const outOfStockCount = computed(() => inventoryList.value.filter(item => item.qty === 0).length)

const searchQuery = ref(''); const filterWarehouse = ref(''); const filterStatus = ref('')
const currentPage = ref(1); const itemsPerPage = ref(10); 

const getStockStatus = (qty, minStock) => { if (qty === 0) return 'out'; if (qty <= minStock) return 'low'; return 'ok' }
const checkExpiryStatus = (hsd) => {
  if (!hsd) return { text: 'Không thời hạn', class: 'bg-gray-100 text-gray-600 border-gray-200', code: 'none' }
  const today = new Date(); today.setHours(0,0,0,0); const expiryDate = new Date(hsd);
  const diffDays = Math.ceil(Math.abs(expiryDate - today) / (1000 * 60 * 60 * 24));
  if (expiryDate < today) return { text: 'Đã hết hạn', class: 'bg-red-100 text-red-700 border-red-200', code: 'expired' }
  if (diffDays <= 30) return { text: 'Sắp hết hạn', class: 'bg-amber-100 text-amber-700 border-amber-200', code: 'warning' }
  return { text: 'Còn hạn', class: 'bg-emerald-100 text-emerald-700 border-emerald-200', code: 'valid' }
}

const baseFilteredInventory = computed(() => {
  return inventoryList.value.filter(item => {
    const matchSearch = item.sku.toLowerCase().includes(searchQuery.value.toLowerCase()) || item.name.toLowerCase().includes(searchQuery.value.toLowerCase()) || item.locationCode.toLowerCase().includes(searchQuery.value.toLowerCase())
    const matchWarehouse = filterWarehouse.value === '' || item.warehouse.includes(filterWarehouse.value)
    let matchStatus = true
    if (filterStatus.value !== '') { matchStatus = getStockStatus(item.qty, item.minStock) === filterStatus.value || checkExpiryStatus(item.hsd).code === filterStatus.value }
    return matchSearch && matchWarehouse && matchStatus
  })
})

const displayInventory = computed(() => {
    const grouped = {};
    baseFilteredInventory.value.forEach(item => {
        if (!grouped[item.sku]) {
            grouped[item.sku] = { ...item };
            grouped[item.sku].rackLocations = new Set();
            if (item.locationId) grouped[item.sku].rackLocations.add(item.locationCode);
        } else {
            grouped[item.sku].qty += item.qty; 
            grouped[item.sku].qtyPending += item.qtyPending; 
            grouped[item.sku].qtyRack += item.qtyRack;       
            if (item.locationId) grouped[item.sku].rackLocations.add(item.locationCode);
            if (grouped[item.sku].hsd !== item.hsd) grouped[item.sku].hsd = 'Nhiều hạn sử dụng';
            if (grouped[item.sku].nsx !== item.nsx) grouped[item.sku].nsx = 'Nhiều ngày SX';
        }
    });
    return Object.values(grouped).map(item => { item.rackLocationList = Array.from(item.rackLocations); return item; });
})

const totalPages = computed(() => { if (!displayInventory.value.length) return 1; return Math.ceil(displayInventory.value.length / itemsPerPage.value); })
const paginatedInventory = computed(() => { const start = (currentPage.value - 1) * itemsPerPage.value; return displayInventory.value.slice(start, start + itemsPerPage.value); })
watch([searchQuery, filterWarehouse, filterStatus, itemsPerPage], () => { currentPage.value = 1; })
const totalStockQuantity = computed(() => baseFilteredInventory.value.reduce((sum, s) => sum + getAbsQty(s), 0))

const showModal = ref(false); const selectedProduct = ref(null); const mockLedger = ref([])
const openStockLedger = (item) => { selectedProduct.value = item; showModal.value = true }
const closeModal = () => { showModal.value = false; selectedProduct.value = null }

onMounted(() => fetchData())
</script>

<template>
  <div class="space-y-5 md:space-y-6 animate-fade-in pb-10 px-0 md:px-1 relative">
    
    <div class="flex flex-col sm:flex-row sm:items-center justify-between gap-4">
      <div>
        <h2 class="text-xl md:text-2xl font-bold text-gray-800">Tồn Kho Thời Gian Thực</h2>
        <p class="text-xs md:text-sm text-gray-500 mt-1">Theo dõi số lượng hàng hóa và các cảnh báo cạn kho</p>
        <p v-if="myWarehouseId" class="text-[10px] font-bold text-indigo-600 bg-indigo-50 inline-block px-2 py-1 rounded mt-2 border border-indigo-200 uppercase">Kho ID: {{ myWarehouseId }}</p>
      </div>
      <div class="flex gap-2">
        <button @click="fetchData" class="bg-white border border-gray-300 text-gray-700 px-4 py-2.5 rounded-lg text-sm font-semibold hover:bg-gray-50 transition-colors shadow-sm flex items-center gap-2">
          <ArrowPathIcon class="w-4 h-4" :class="{'animate-spin': isLoading}"/> Làm mới
        </button>
        <button class="bg-white border border-emerald-200 text-emerald-700 px-4 py-2.5 rounded-lg text-sm font-semibold hover:bg-emerald-50 transition-colors shadow-sm flex items-center gap-2">
          <DocumentTextIcon class="w-5 h-5"/> Xuất Báo Cáo
        </button>
      </div>
    </div>

    <div class="grid grid-cols-1 sm:grid-cols-3 gap-4">
      <div class="bg-white p-4 rounded-xl border border-gray-200 shadow-sm flex items-center gap-4">
        <div class="w-12 h-12 rounded-full bg-blue-50 flex items-center justify-center text-blue-600 shrink-0"><ArchiveBoxIcon class="w-6 h-6" /></div>
        <div><p class="text-sm font-medium text-gray-500">Mã Sản phẩm (SKU) có trong kho</p><p class="text-2xl font-bold text-gray-800">{{ totalSkus }}</p></div>
      </div>
      <div class="bg-white p-4 rounded-xl border border-amber-200 shadow-sm flex items-center gap-4 relative overflow-hidden">
        <div class="absolute right-0 top-0 w-2 h-full bg-amber-400"></div>
        <div class="w-12 h-12 rounded-full bg-amber-50 flex items-center justify-center text-amber-600 shrink-0"><ExclamationTriangleIcon class="w-6 h-6" /></div>
        <div><p class="text-sm font-medium text-amber-700">Cảnh báo sắp hết</p><p class="text-2xl font-bold text-gray-800">{{ lowStockCount }} <span class="text-xs font-normal text-gray-500">Mã SKU</span></p></div>
      </div>
      <div class="bg-white p-4 rounded-xl border border-red-200 shadow-sm flex items-center gap-4 relative overflow-hidden">
        <div class="absolute right-0 top-0 w-2 h-full bg-red-500"></div>
        <div class="w-12 h-12 rounded-full bg-red-50 flex items-center justify-center text-red-600 shrink-0"><XMarkIcon class="w-6 h-6" /></div>
        <div><p class="text-sm font-medium text-red-700">Đã HẾT HÀNG</p><p class="text-2xl font-bold text-gray-800">{{ outOfStockCount }} <span class="text-xs font-normal text-gray-500">Mã SKU</span></p></div>
      </div>
    </div>

    <div class="bg-white p-3 md:p-4 rounded-xl border border-gray-200 flex flex-col md:flex-row items-center justify-between gap-3 md:gap-4 shadow-sm">
      <div class="flex flex-col sm:flex-row gap-3 w-full md:w-auto flex-1">
        <div class="relative w-full md:max-w-md">
            <div class="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none"><MagnifyingGlassIcon class="w-5 h-5 text-gray-400" /></div>
            <input v-model="searchQuery" type="text" class="block w-full pl-10 pr-3 py-2 border border-gray-200 rounded-lg text-sm outline-none focus:ring-1 focus:ring-primary-500" placeholder="Tìm mã SKU, tên SP, vị trí Kệ...">
        </div>
        <select v-if="!myWarehouseId" v-model="filterWarehouse" class="w-full sm:w-auto border border-gray-200 rounded-lg text-sm px-4 py-2 outline-none focus:ring-1 focus:ring-primary-500 cursor-pointer">
            <option value="">Tất cả Kho Chi Nhánh</option>
            <option v-for="wh in warehousesList" :key="wh.id" :value="wh.name">{{ wh.name }}</option>
        </select>
        <select v-model="filterStatus" class="w-full sm:w-auto border border-gray-200 rounded-lg text-sm px-4 py-2 outline-none focus:ring-1 focus:ring-primary-500 cursor-pointer">
            <option value="">Tất cả Trạng thái</option>
            <option value="ok">Đang có hàng</option><option value="low">Sắp hết hàng</option><option value="out">Đã hết hàng</option>
        </select>
      </div>
    </div>

    <div class="bg-white rounded-xl border border-gray-200 shadow-sm overflow-hidden flex flex-col">
      <div class="w-full overflow-x-auto custom-scrollbar">
        <table class="min-w-[1300px] w-full divide-y divide-gray-200">
          <thead class="bg-gray-50">
            <tr>
              <th class="px-4 py-3.5 text-left text-xs font-bold text-gray-500 uppercase tracking-wider">Mã SKU & Sản phẩm</th>
              <th class="px-4 py-3.5 text-center text-xs font-bold text-amber-700 uppercase tracking-wider min-w-[220px]">Bãi Chờ (Chưa lên kệ)</th>
              <th class="px-4 py-3.5 text-left text-xs font-bold text-indigo-700 uppercase tracking-wider min-w-[280px]">Trên Kệ (Đã sắp xếp)</th>
              <th class="px-4 py-3.5 text-center text-xs font-bold text-gray-500 uppercase tracking-wider w-32">NSX - HSD</th>
              <th class="px-4 py-3.5 text-left text-xs font-bold text-primary-700 uppercase tracking-wider min-w-[350px]">Tồn kho Tổng</th>
              <th class="px-4 py-3.5 text-center text-xs font-bold text-emerald-700 uppercase tracking-wider w-24">Tổng SL Lẻ</th>
              <th class="px-4 py-3.5 text-right text-xs font-bold text-gray-500 uppercase tracking-wider w-24">Thao tác</th>
            </tr>
          </thead>
          <tbody class="divide-y divide-gray-100 bg-white">
            <tr v-if="isLoading"><td colspan="7" class="px-6 py-12 text-center text-gray-500 font-medium">Đang tải dữ liệu tồn kho...</td></tr>
            <tr v-else-if="paginatedInventory.length === 0">
              <td colspan="7" class="px-6 py-16 text-center">
                <ArchiveBoxIcon class="w-12 h-12 text-gray-300 mx-auto mb-3" />
                <h3 class="text-base font-semibold text-gray-700">Không có dữ liệu tồn kho</h3>
              </td>
            </tr>
            <tr v-for="item in paginatedInventory" :key="item.id" class="hover:bg-gray-50 transition-colors">
              
              <td class="px-4 py-3">
                <div class="flex flex-col">
                    <span class="text-sm font-bold text-gray-700">{{ item.sku }}</span>
                    <span class="text-sm font-bold text-gray-900">{{ item.name }}</span>
                    <span class="text-[10px] uppercase font-bold text-indigo-600 mt-0.5 w-fit bg-indigo-50 px-1 rounded border border-indigo-100">Kho: {{ item.warehouse }}</span>
                </div>
              </td>
              
              <td class="px-4 py-3 text-center border-l border-gray-100">
                <div v-if="item.qtyPending > 0" class="flex flex-col items-center w-full">
                    <span class="font-bold text-amber-800 text-[12px] bg-amber-100 px-2 py-1.5 rounded-t-lg border border-amber-200 border-b-0 text-center shadow-sm w-full">
                        {{ formatPhysicalStock(item.qtyPending, item.units, item.baseUnit) }}
                    </span>
                    <span v-if="item.units.length > 1" class="text-[10px] text-amber-700 bg-amber-50 px-2 py-1 rounded-b-lg border border-amber-200 text-center shadow-sm w-full font-semibold">
                        {{ formatEquivalentStock(item.qtyPending, item.units, item.baseUnit) }}
                    </span>
                </div>
                <span v-else class="text-gray-300">-</span>
              </td>

              <td class="px-4 py-3 border-l border-gray-100">
                <div v-if="item.qtyRack > 0" class="flex flex-col items-start w-full gap-1">
                    <div class="w-full">
                        <span class="font-bold text-indigo-800 text-[12px] bg-indigo-100 px-2 py-1.5 rounded-t-lg border border-indigo-200 border-b-0 shadow-sm block w-full">
                            {{ formatPhysicalStock(item.qtyRack, item.units, item.baseUnit) }}
                        </span>
                        <span v-if="item.units.length > 1" class="text-[10px] text-indigo-600 bg-indigo-50 px-2 py-1 rounded-b-lg border border-indigo-200 shadow-sm block w-full font-semibold">
                            {{ formatEquivalentStock(item.qtyRack, item.units, item.baseUnit) }}
                        </span>
                    </div>
                    <div class="flex flex-wrap gap-1 mt-1">
                        <span v-for="(loc, idx) in item.rackLocationList" :key="idx" class="bg-gray-100 border border-gray-300 text-gray-700 px-2 py-0.5 rounded text-[10px] font-bold flex items-center gap-1"><MapPinIcon class="w-3 h-3 text-indigo-600"/> {{ loc }}</span>
                    </div>
                </div>
                <div v-else class="text-center text-gray-300">-</div>
              </td>

              <td class="px-4 py-3 text-center border-l border-gray-100">
                <span v-if="item.nsx || item.hsd" class="text-[10px] font-medium text-gray-600">
                  <span class="block">NSX: {{ item.nsx || '--' }}</span>
                  <span class="block" :class="checkExpiryStatus(item.hsd).code !== 'valid' ? 'text-amber-600 font-bold' : ''">HSD: {{ item.hsd || '--' }}</span>
                </span>
                <span v-else class="text-xs text-gray-400 italic">---</span>
              </td>

              <td class="px-4 py-3 text-left border-l border-gray-100">
                <div class="flex flex-col gap-1.5 w-full">
                    <div class="w-full">
                        <span class="text-[13px] block font-bold text-blue-900 bg-blue-100 px-3 py-2 rounded-t-lg border border-blue-200 border-b-0 shadow-sm w-full">
                            <ArchiveBoxIcon class="w-4 h-4 inline-block mr-1 text-blue-600 align-text-bottom"/>
                            {{ formatPhysicalStock(getAbsQty(item), item.units, item.baseUnit) }}
                        </span>
                        <span v-if="item.units.length > 1" class="text-[11px] block text-blue-700 bg-blue-50 px-3 py-1.5 rounded-b-lg border border-blue-200 shadow-sm w-full font-bold">
                            {{ formatEquivalentStock(getAbsQty(item), item.units, item.baseUnit) }}
                        </span>
                    </div>
                    <div class="flex items-start gap-1.5 text-[11px] text-gray-500 font-medium ml-2">
                        <ArrowsRightLeftIcon class="w-3.5 h-3.5 mt-0.5 text-gray-400 shrink-0"/> 
                        <span>Quy cách: {{ getFormulaChain(item) }}</span>
                    </div>
                </div>
              </td>

              <td class="px-4 py-3 text-center bg-emerald-50/20 border-l border-emerald-100">
                <span class="font-bold text-emerald-700 block text-xl">{{ getAbsQty(item) }}</span>
                <span class="text-[10px] text-emerald-600 block uppercase font-bold">{{ item.baseUnit }}</span>
              </td>
              
              <td class="px-4 py-3 text-right border-l border-gray-100">
                <button @click="openStockLedger(item)" class="px-3 py-1.5 text-blue-600 hover:bg-blue-50 rounded bg-white border border-blue-200 shadow-sm font-medium text-xs flex items-center justify-center gap-1 ml-auto transition-colors w-full">
                  <EyeIcon class="w-4 h-4" /> Thẻ kho
                </button>
              </td>
            </tr>
          </tbody>
          <tfoot class="bg-gray-50 font-bold border-t border-gray-200">
            <tr>
              <td colspan="5" class="px-4 py-4 text-right uppercase text-gray-600">Tổng toàn bộ Sản phẩm lẻ (SL) của Kho này theo bộ lọc:</td>
              <td class="px-4 py-4 text-center text-emerald-700 text-2xl bg-emerald-50/30">{{ totalStockQuantity }}</td>
              <td></td>
            </tr>
          </tfoot>
        </table>
      </div>
      
      <div class="flex items-center justify-between px-4 py-3 bg-white border-t border-gray-200 sm:px-6 mt-auto">
          <div class="flex items-center gap-4">
              <span class="text-sm text-gray-700">
                  Hiển thị
                  <select v-model="itemsPerPage" class="border border-gray-300 rounded mx-1 px-2 py-1 text-sm outline-none cursor-pointer focus:ring-1 focus:ring-primary-500">
                      <option :value="10">10</option>
                      <option :value="20">20</option>
                      <option :value="50">50</option>
                  </select>
                  / {{ displayInventory.length }} sản phẩm
              </span>
          </div>
          
          <div class="flex items-center space-x-2">
              <button @click="currentPage--" :disabled="currentPage === 1" class="px-3 py-1.5 text-sm font-medium bg-white border border-gray-300 rounded-md text-gray-700 disabled:opacity-50 disabled:cursor-not-allowed hover:bg-gray-50 transition-colors flex items-center gap-1"><ChevronLeftIcon class="w-4 h-4" /> Trước</button>
              <span class="text-sm font-semibold text-primary-700 bg-primary-50 border border-primary-100 px-3 py-1.5 rounded-md">Trang {{ currentPage }} / {{ totalPages }}</span>
              <button @click="currentPage++" :disabled="currentPage === totalPages" class="px-3 py-1.5 text-sm font-medium bg-white border border-gray-300 rounded-md text-gray-700 disabled:opacity-50 disabled:cursor-not-allowed hover:bg-gray-50 transition-colors flex items-center gap-1">Sau <ChevronRightIcon class="w-4 h-4" /></button>
          </div>
      </div>
      </div>

    <Teleport to="body">
      <div v-if="showModal" class="fixed inset-0 z-50 flex items-center justify-center px-4 bg-slate-900/50 backdrop-blur-sm animate-fade-in">
        <div class="bg-white rounded-xl shadow-2xl w-full max-w-3xl overflow-hidden flex flex-col max-h-[90vh]">
          <div class="px-6 py-4 border-b border-gray-100 flex items-center justify-between bg-gray-50 shrink-0">
            <div>
              <h3 class="text-lg font-bold text-gray-800 flex items-center gap-2">Thẻ Kho (Lịch sử giao dịch)</h3>
              <p class="text-sm text-gray-500 mt-1">Sản phẩm: <span class="font-bold text-primary-700">{{ selectedProduct?.sku }} - {{ selectedProduct?.name }}</span></p>
            </div>
            <button @click="closeModal" class="text-gray-400 hover:text-red-500"><XMarkIcon class="w-6 h-6" /></button>
          </div>
          <div class="p-6 overflow-y-auto flex-1 custom-scrollbar">
            <div class="border border-gray-200 rounded-lg overflow-hidden">
              <table class="w-full text-sm text-left">
                <thead class="bg-slate-100 text-xs uppercase font-bold text-slate-600 border-b border-slate-200">
                  <tr><th class="px-4 py-3">Ngày giao dịch</th><th class="px-4 py-3">Loại phiếu</th><th class="px-4 py-3">Mã chứng từ</th><th class="px-4 py-3 text-right">Biến động</th><th class="px-4 py-3 text-right">Tồn cuối</th></tr>
                </thead>
                <tbody class="divide-y divide-gray-100">
                  <tr v-if="mockLedger.length === 0"><td colspan="5" class="px-4 py-8 text-center text-gray-400 italic">Chưa có giao dịch nào phát sinh.</td></tr>
                </tbody>
              </table>
            </div>
          </div>
          <div class="px-6 py-4 border-t flex justify-end bg-white shrink-0"><button @click="closeModal" class="px-5 py-2.5 bg-slate-800 text-white rounded-lg text-sm font-bold hover:bg-slate-900 shadow-md">Đóng Thẻ Kho</button></div>
        </div>
      </div>
    </Teleport>
  </div>
</template>

<style scoped>
.custom-scrollbar::-webkit-scrollbar { height: 6px; width: 6px; }
.custom-scrollbar::-webkit-scrollbar-track { background: transparent; }
.custom-scrollbar::-webkit-scrollbar-thumb { background: #cbd5e1; border-radius: 10px; }
.animate-fade-in { animation: fadeIn 0.2s ease-out; }
@keyframes fadeIn { from { opacity: 0; transform: translateY(5px); } to { opacity: 1; transform: translateY(0); } }
</style>