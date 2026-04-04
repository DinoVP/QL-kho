<script setup>
import { ref, computed, onMounted } from 'vue'
import { 
  MagnifyingGlassIcon, ArchiveBoxIcon, MapPinIcon,
  ExclamationTriangleIcon, CheckCircleIcon,
  EyeIcon, XMarkIcon, DocumentTextIcon, ArrowPathIcon
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
    
    if (prodRes.ok) {
      productsList.value = await prodRes.json()
      // MẸO CHECK DỮ LIỆU BE: Mở F12 (Console) trên trình duyệt để xem C# trả về biến tên gì
      console.log("Dữ liệu Sản phẩm từ BE trả về:", productsList.value[0])
    }
    if (locRes.ok) locationsList.value = await locRes.json()
    
    if (stockRes.ok) {
      const rawStocks = await stockRes.json()
      
      const mappedStocks = rawStocks.map(s => {
        const prod = productsList.value.find(p => p.id === s.variantId || p.Id === s.variantId) || {};
        const loc = locationsList.value.find(l => l.id === s.locationId || l.Id === s.locationId) || {};
        
        const whId = s.warehouseId || loc.warehouseId || 1;
        const wh = warehousesList.value.find(w => w.id === whId) || {};
        
        // HỨNG DỮ LIỆU TỪ BACKEND ĐA DẠNG:
        // Bắt cả quyCach, QuyCach, conversionRate...
        const rawConv = prod.quyCach || prod.QuyCach || prod.conversionRate || prod.ConversionRate;
        const convRate = (!rawConv || rawConv === 'N/A' || rawConv === 'null') ? 1 : (parseInt(rawConv) || 1);

        const isPendingZone = !s.locationId; 

        return {
          id: s.id, variantId: s.variantId, 
          qty: s.qty || 0, 
          qtyPending: isPendingZone ? (s.qty || 0) : 0, 
          qtyRack: !isPendingZone ? (s.qty || 0) : 0,   
          
          nsx: s.nsx || '', hsd: s.hsd || '',
          sku: prod.sku || prod.Sku || 'N/A', name: prod.name || prod.Name || 'Sản phẩm không xác định',
          
          unit: prod.unit || prod.Unit || 'Thùng', 
          baseUnit: 'Đơn vị nhỏ', // Tên chung sau quy đổi
          conversionRate: convRate, 
          
          category: prod.categoryName || prod.CategoryName || 'Chung',
          minStock: prod.minStock || prod.MinStock || 10, 
          locationId: s.locationId, 
          locationCode: loc.code || loc.Code || 'Khu Chờ Nhập',
          warehouseId: whId, 
          warehouse: wh.name || 'Kho Tổng' 
        }
      })

      inventoryList.value = mappedStocks.filter(item => !myWarehouseId.value || item.warehouseId === myWarehouseId.value)
    }
  } catch (error) { console.error('Lỗi tải dữ liệu:', error) }
  finally { isLoading.value = false }
}

const totalSkus = computed(() => {
    const uniqueSkus = new Set(inventoryList.value.map(item => item.sku));
    return uniqueSkus.size;
})

const lowStockCount = computed(() => inventoryList.value.filter(item => item.qty > 0 && (item.qty * item.conversionRate) <= item.minStock).length)
const outOfStockCount = computed(() => inventoryList.value.filter(item => item.qty === 0).length)

const searchQuery = ref(''); const filterWarehouse = ref(''); const filterStatus = ref('')

const getStockStatus = (qty, minStock, convRate) => {
  const totalItems = qty * convRate;
  if (qty === 0) return 'out'; if (totalItems <= minStock) return 'low'; return 'ok'
}

const checkExpiryStatus = (hsd) => {
  if (!hsd) return { text: 'Không thời hạn', class: 'bg-gray-100 text-gray-600 border-gray-200', code: 'none' }
  const today = new Date(); today.setHours(0,0,0,0);
  const expiryDate = new Date(hsd);
  const diffTime = Math.abs(expiryDate - today);
  const diffDays = Math.ceil(diffTime / (1000 * 60 * 60 * 24));

  if (expiryDate < today) return { text: 'Đã hết hạn', class: 'bg-red-100 text-red-700 border-red-200', code: 'expired' }
  if (diffDays <= 30) return { text: 'Sắp hết hạn', class: 'bg-amber-100 text-amber-700 border-amber-200', code: 'warning' }
  return { text: 'Còn hạn', class: 'bg-emerald-100 text-emerald-700 border-emerald-200', code: 'valid' }
}

const baseFilteredInventory = computed(() => {
  return inventoryList.value.filter(item => {
    const matchSearch = item.sku.toLowerCase().includes(searchQuery.value.toLowerCase()) || 
                        item.name.toLowerCase().includes(searchQuery.value.toLowerCase()) ||
                        item.locationCode.toLowerCase().includes(searchQuery.value.toLowerCase())
    const matchWarehouse = filterWarehouse.value === '' || item.warehouse.includes(filterWarehouse.value)
    let matchStatus = true
    if (filterStatus.value !== '') { matchStatus = getStockStatus(item.qty, item.minStock, item.conversionRate) === filterStatus.value || checkExpiryStatus(item.hsd).code === filterStatus.value }
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

    return Object.values(grouped).map(item => {
        item.rackLocationList = Array.from(item.rackLocations);
        return item;
    });
})

const totalStockQuantity = computed(() => baseFilteredInventory.value.reduce((sum, s) => sum + ((s.qty || 0) * (s.conversionRate || 1)), 0))

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
        <div><p class="text-sm font-medium text-amber-700">Cảnh báo sắp hết</p><p class="text-2xl font-bold text-gray-800">{{ lowStockCount }} <span class="text-xs font-normal text-gray-500">Lô</span></p></div>
      </div>
      <div class="bg-white p-4 rounded-xl border border-red-200 shadow-sm flex items-center gap-4 relative overflow-hidden">
        <div class="absolute right-0 top-0 w-2 h-full bg-red-500"></div>
        <div class="w-12 h-12 rounded-full bg-red-50 flex items-center justify-center text-red-600 shrink-0"><XMarkIcon class="w-6 h-6" /></div>
        <div><p class="text-sm font-medium text-red-700">Đã HẾT HÀNG</p><p class="text-2xl font-bold text-gray-800">{{ outOfStockCount }} <span class="text-xs font-normal text-gray-500">Lô</span></p></div>
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

    <div class="bg-white rounded-xl border border-gray-200 shadow-sm overflow-hidden">
      <div class="w-full overflow-x-auto custom-scrollbar">
        <table class="min-w-[1250px] w-full divide-y divide-gray-200">
          <thead class="bg-gray-50">
            <tr>
              <th class="px-4 py-3.5 text-left text-xs font-bold text-gray-500 uppercase tracking-wider">Mã SKU</th>
              <th class="px-4 py-3.5 text-left text-xs font-bold text-gray-500 uppercase tracking-wider">Tên Sản phẩm</th>
              <th class="px-4 py-3.5 text-center text-xs font-bold text-amber-700 uppercase tracking-wider w-32">SL Bãi Chờ</th>
              <th class="px-4 py-3.5 text-left text-xs font-bold text-indigo-700 uppercase tracking-wider min-w-[160px]">Trên Kệ (SL & Vị trí)</th>
              <th class="px-4 py-3.5 text-center text-xs font-bold text-gray-500 uppercase tracking-wider">NSX - HSD</th>
              <th class="px-4 py-3.5 text-center text-xs font-bold text-gray-500 uppercase tracking-wider">Tổng Tồn</th>
              <th class="px-4 py-3.5 text-center text-xs font-bold text-gray-500 uppercase tracking-wider">Quy Cách</th>
              <th class="px-4 py-3.5 text-center text-xs font-bold text-emerald-700 uppercase tracking-wider">Tổng Quy Đổi</th>
              <th class="px-4 py-3.5 text-center text-xs font-bold text-gray-500 uppercase tracking-wider">Định mức</th>
              <th class="px-4 py-3.5 text-right text-xs font-bold text-gray-500 uppercase tracking-wider">Thao tác</th>
            </tr>
          </thead>
          <tbody class="divide-y divide-gray-100 bg-white">
            <tr v-if="isLoading"><td colspan="10" class="px-6 py-12 text-center text-gray-500 font-medium">Đang tải dữ liệu tồn kho...</td></tr>
            <tr v-else-if="displayInventory.length === 0">
              <td colspan="10" class="px-6 py-16 text-center">
                <ArchiveBoxIcon class="w-12 h-12 text-gray-300 mx-auto mb-3" />
                <h3 class="text-base font-semibold text-gray-700">Không có dữ liệu tồn kho</h3>
              </td>
            </tr>
            <tr v-for="item in displayInventory" :key="item.id" class="hover:bg-gray-50 transition-colors">
              <td class="px-4 py-3 text-sm font-bold text-gray-700">{{ item.sku }}</td>
              <td class="px-4 py-3"><div class="flex flex-col"><span class="text-sm font-bold text-gray-900">{{ item.name }}</span><span class="text-[10px] uppercase font-bold text-indigo-600 mt-0.5">Kho: {{ item.warehouse }}</span></div></td>
              
              <td class="px-4 py-3 text-center">
                <div v-if="item.qtyPending > 0" class="flex flex-col items-center">
                    <span class="font-bold text-amber-600 text-lg">{{ item.qtyPending }}</span>
                    <span class="text-[10px] text-amber-600/70 font-bold uppercase">{{ item.unit }}</span>
                </div>
                <span v-else class="text-gray-300">-</span>
              </td>

              <td class="px-4 py-3">
                <div v-if="item.qtyRack > 0" class="flex flex-col">
                    <span class="font-bold text-indigo-600 text-lg">{{ item.qtyRack }} <span class="text-[10px] text-indigo-500 uppercase font-bold">{{ item.unit }}</span></span>
                    <div class="flex flex-wrap gap-1 mt-1.5">
                        <span v-for="(loc, idx) in item.rackLocationList" :key="idx" class="bg-indigo-100 border border-indigo-200 text-indigo-700 px-1.5 py-0.5 rounded text-[10px] font-bold flex items-center gap-0.5">
                            <MapPinIcon class="w-3 h-3"/> {{ loc }}
                        </span>
                    </div>
                </div>
                <div v-else class="text-center text-gray-300">-</div>
              </td>

              <td class="px-4 py-3 text-center">
                <span v-if="item.nsx || item.hsd" class="text-[10px] font-medium text-gray-600">
                  <span class="block">NSX: {{ item.nsx || '---' }}</span>
                  <span class="block" :class="checkExpiryStatus(item.hsd).code !== 'valid' ? 'text-amber-600 font-bold' : ''">HSD: {{ item.hsd || '---' }}</span>
                </span>
                <span v-else class="text-xs text-gray-400 italic">---</span>
              </td>

              <td class="px-4 py-3 text-center">
                <div class="flex flex-col items-center">
                  <span class="font-bold text-gray-800 text-lg">{{ item.qty }}</span>
                  <span class="text-[10px] text-gray-500 block uppercase font-bold">{{ item.unit }}</span>
                </div>
              </td>

              <td class="px-4 py-3 text-center border-l border-r border-gray-100">
                <div class="flex flex-col items-center">
                  <span v-if="item.conversionRate > 1" class="font-bold text-amber-700 bg-amber-50 px-2 py-0.5 rounded border border-amber-200 text-[10px] shadow-sm w-max">
                    {{ item.conversionRate }} {{ item.baseUnit }}/{{ item.unit }}
                  </span>
                  
                  <span v-else class="text-gray-300 text-xs italic">-</span>
                </div>
              </td>

              <td class="px-4 py-3 text-center bg-emerald-50/10">
                <span class="font-bold text-emerald-700 block text-lg">{{ item.qty * item.conversionRate }}</span>
                <span class="text-[10px] text-emerald-600 block uppercase font-bold">{{ item.conversionRate > 1 ? item.baseUnit : item.unit }}</span>
              </td>
              
              <td class="px-4 py-3 text-center text-sm text-gray-500 font-medium">{{ item.minStock }}</td>
              
              <td class="px-4 py-3 text-right space-x-2">
                <button @click="openStockLedger(item)" class="px-3 py-1.5 text-blue-600 hover:bg-blue-50 rounded bg-white border border-blue-200 shadow-sm font-medium text-xs flex items-center gap-1 ml-auto transition-colors"><EyeIcon class="w-4 h-4" /> Thẻ kho</button>
              </td>
            </tr>
          </tbody>
          <tfoot class="bg-gray-50 font-bold border-t border-gray-200">
            <tr>
              <td colspan="7" class="px-4 py-4 text-right uppercase text-gray-600">Tổng toàn bộ tồn kho của Kho này (Tổng số cái/chiếc):</td>
              <td class="px-4 py-4 text-center text-emerald-700 text-2xl">{{ totalStockQuantity }}</td>
              <td colspan="2"></td>
            </tr>
          </tfoot>
        </table>
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