<script setup>
import { ref, computed } from 'vue'
import { 
  MagnifyingGlassIcon, ArchiveBoxIcon, 
  ExclamationTriangleIcon, CheckCircleIcon,
  EyeIcon, XMarkIcon, DocumentTextIcon
} from '@heroicons/vue/24/outline'

// === 1. STATE CHÍNH: TRỐNG CHỜ API ===
const inventoryList = ref([])

// === 2. TÍNH TOÁN THỐNG KÊ NHANH ===
const totalSkus = computed(() => inventoryList.value.length)
const lowStockCount = computed(() => inventoryList.value.filter(item => item.qty > 0 && item.qty <= item.minStock).length)
const outOfStockCount = computed(() => inventoryList.value.filter(item => item.qty === 0).length)

// === 3. BỘ LỌC ===
const searchQuery = ref('')
const filterWarehouse = ref('')
const filterStatus = ref('')

const getStockStatus = (qty, minStock) => {
  if (qty === 0) return 'out'
  if (qty <= minStock) return 'low'
  return 'ok'
}

const filteredInventory = computed(() => {
  return inventoryList.value.filter(item => {
    const matchSearch = item.sku.toLowerCase().includes(searchQuery.value.toLowerCase()) || 
                        item.name.toLowerCase().includes(searchQuery.value.toLowerCase())
    const matchWarehouse = filterWarehouse.value === '' || item.warehouse.includes(filterWarehouse.value)
    
    let matchStatus = true
    if (filterStatus.value !== '') {
      matchStatus = getStockStatus(item.qty, item.minStock) === filterStatus.value
    }
    
    return matchSearch && matchWarehouse && matchStatus
  })
})

// === 4. LOGIC MODAL "THẺ KHO" ===
const showModal = ref(false)
const selectedProduct = ref(null)
const mockLedger = ref([]) // Trống chờ API đổ lịch sử giao dịch của 1 SKU

const openStockLedger = (item) => {
  selectedProduct.value = item
  showModal.value = true
}
const closeModal = () => {
  showModal.value = false
  selectedProduct.value = null
}
</script>

<template>
  <div class="space-y-5 md:space-y-6 animate-fade-in pb-10 px-0 md:px-1 relative">
    
    <div class="flex flex-col sm:flex-row sm:items-center justify-between gap-4">
      <div>
        <h2 class="text-xl md:text-2xl font-bold text-gray-800">Tồn Kho Thời Gian Thực</h2>
        <p class="text-xs md:text-sm text-gray-500 mt-1">Theo dõi số lượng hàng hóa và các cảnh báo cạn kho</p>
      </div>
      <button class="bg-white border border-gray-300 text-gray-700 px-4 py-2.5 rounded-lg text-sm font-semibold hover:bg-gray-50 transition-colors shadow-sm flex items-center gap-2">
        <DocumentTextIcon class="w-5 h-5"/> Xuất Báo Cáo
      </button>
    </div>

    <div class="grid grid-cols-1 sm:grid-cols-3 gap-4">
      <div class="bg-white p-4 rounded-xl border border-gray-200 shadow-sm flex items-center gap-4">
        <div class="w-12 h-12 rounded-full bg-blue-50 flex items-center justify-center text-blue-600 shrink-0"><ArchiveBoxIcon class="w-6 h-6" /></div>
        <div><p class="text-sm font-medium text-gray-500">Tổng Mã SKU</p><p class="text-2xl font-bold text-gray-800">{{ totalSkus }}</p></div>
      </div>
      <div class="bg-white p-4 rounded-xl border border-amber-200 shadow-sm flex items-center gap-4 relative overflow-hidden">
        <div class="absolute right-0 top-0 w-2 h-full bg-amber-400"></div>
        <div class="w-12 h-12 rounded-full bg-amber-50 flex items-center justify-center text-amber-600 shrink-0"><ExclamationTriangleIcon class="w-6 h-6" /></div>
        <div><p class="text-sm font-medium text-amber-700">Cảnh báo sắp hết</p><p class="text-2xl font-bold text-gray-800">{{ lowStockCount }} <span class="text-xs font-normal text-gray-500">SKU</span></p></div>
      </div>
      <div class="bg-white p-4 rounded-xl border border-red-200 shadow-sm flex items-center gap-4 relative overflow-hidden">
        <div class="absolute right-0 top-0 w-2 h-full bg-red-500"></div>
        <div class="w-12 h-12 rounded-full bg-red-50 flex items-center justify-center text-red-600 shrink-0"><XMarkIcon class="w-6 h-6" /></div>
        <div><p class="text-sm font-medium text-red-700">Đã HẾT HÀNG</p><p class="text-2xl font-bold text-gray-800">{{ outOfStockCount }} <span class="text-xs font-normal text-gray-500">SKU</span></p></div>
      </div>
    </div>

    <div class="bg-white p-3 md:p-4 rounded-xl border border-gray-200 flex flex-col md:flex-row items-center gap-3 md:gap-4 shadow-sm">
      <div class="relative w-full md:flex-1">
        <div class="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none"><MagnifyingGlassIcon class="w-5 h-5 text-gray-400" /></div>
        <input v-model="searchQuery" type="text" class="block w-full pl-10 pr-3 py-2 border border-gray-200 rounded-lg text-sm outline-none focus:ring-1 focus:ring-primary-500" placeholder="Tìm theo mã SKU, tên sản phẩm...">
      </div>
      <div class="flex flex-col sm:flex-row gap-3 w-full md:w-auto">
        <select v-model="filterWarehouse" class="w-full sm:w-auto border border-gray-200 rounded-lg text-sm px-4 py-2 outline-none focus:ring-1 focus:ring-primary-500 cursor-pointer">
          <option value="">Tất cả Kho</option><option value="Miền Bắc">Tổng kho Miền Bắc</option><option value="Miền Nam">Chi nhánh Miền Nam</option><option value="Đà Nẵng">Trung tâm Đà Nẵng</option>
        </select>
        <select v-model="filterStatus" class="w-full sm:w-auto border border-gray-200 rounded-lg text-sm px-4 py-2 outline-none focus:ring-1 focus:ring-primary-500 cursor-pointer">
          <option value="">Tất cả Trạng thái</option><option value="ok">Đang có hàng</option><option value="low">Sắp hết hàng</option><option value="out">Đã hết hàng</option>
        </select>
      </div>
    </div>

    <div class="bg-white rounded-xl border border-gray-200 shadow-sm overflow-hidden">
      <div class="w-full overflow-x-auto custom-scrollbar">
        <table class="min-w-[1000px] w-full divide-y divide-gray-200">
          <thead class="bg-gray-50">
            <tr>
              <th class="px-5 py-3.5 text-left text-xs font-bold text-gray-500 uppercase tracking-wider">Mã SKU</th>
              <th class="px-5 py-3.5 text-left text-xs font-bold text-gray-500 uppercase tracking-wider">Tên Sản phẩm</th>
              <th class="px-5 py-3.5 text-left text-xs font-bold text-gray-500 uppercase tracking-wider">Vị trí Kho</th>
              <th class="px-5 py-3.5 text-right text-xs font-bold text-gray-500 uppercase tracking-wider bg-blue-50/50">Tồn kho hiện tại</th>
              <th class="px-5 py-3.5 text-right text-xs font-bold text-gray-500 uppercase tracking-wider">Định mức (Min)</th>
              <th class="px-5 py-3.5 text-center text-xs font-bold text-gray-500 uppercase tracking-wider">Tình trạng</th>
              <th class="px-5 py-3.5 text-right text-xs font-bold text-gray-500 uppercase tracking-wider">Thao tác</th>
            </tr>
          </thead>
          <tbody class="divide-y divide-gray-100 bg-white">
            <tr v-if="filteredInventory.length === 0">
              <td colspan="7" class="px-6 py-16 text-center">
                <ArchiveBoxIcon class="w-12 h-12 text-gray-300 mx-auto mb-3" />
                <h3 class="text-base font-semibold text-gray-700">Không có dữ liệu tồn kho</h3>
                <p class="text-sm text-gray-500 mt-1">Đang chờ API đồng bộ dữ liệu.</p>
              </td>
            </tr>
            <tr v-for="item in filteredInventory" :key="item.id" class="hover:bg-gray-50 transition-colors">
              <td class="px-5 py-3 text-sm font-bold text-gray-700">{{ item.sku }}</td>
              <td class="px-5 py-3"><div class="flex flex-col"><span class="text-sm font-bold text-gray-900">{{ item.name }}</span><span class="text-xs text-gray-500">{{ item.category }}</span></div></td>
              <td class="px-5 py-3 text-sm font-medium text-gray-700">{{ item.warehouse }}</td>
              <td class="px-5 py-3 text-right bg-blue-50/20">
                <span class="text-lg font-bold" :class="{'text-red-600': item.qty === 0, 'text-amber-600': item.qty > 0 && item.qty <= item.minStock, 'text-emerald-600': item.qty > item.minStock}">{{ item.qty }}</span>
                <span class="text-xs text-gray-500 ml-1">{{ item.unit }}</span>
              </td>
              <td class="px-5 py-3 text-sm text-right text-gray-500">{{ item.minStock }}</td>
              <td class="px-5 py-3 text-center">
                <span v-if="item.qty === 0" class="text-[10px] font-bold px-2 py-1 rounded bg-red-100 text-red-700 uppercase flex items-center justify-center gap-1 w-max mx-auto"><XMarkIcon class="w-3 h-3"/> Hết hàng</span>
                <span v-else-if="item.qty <= item.minStock" class="text-[10px] font-bold px-2 py-1 rounded bg-amber-100 text-amber-700 uppercase flex items-center justify-center gap-1 w-max mx-auto"><ExclamationTriangleIcon class="w-3 h-3"/> Sắp hết</span>
                <span v-else class="text-[10px] font-bold px-2 py-1 rounded bg-emerald-100 text-emerald-700 uppercase flex items-center justify-center gap-1 w-max mx-auto"><CheckCircleIcon class="w-3 h-3"/> Tốt</span>
              </td>
              <td class="px-5 py-3 text-right space-x-2">
                <button @click="openStockLedger(item)" class="px-3 py-1.5 text-blue-600 hover:bg-blue-50 rounded bg-white border border-blue-200 shadow-sm font-medium text-xs flex items-center gap-1 ml-auto transition-colors"><EyeIcon class="w-4 h-4" /> Thẻ kho</button>
              </td>
            </tr>
          </tbody>
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
                  <tr v-for="(log, idx) in mockLedger" :key="idx" class="hover:bg-slate-50">
                    <td class="px-4 py-3 text-gray-600">{{ log.date }}</td>
                    <td class="px-4 py-3">
                      <span v-if="log.type === 'Nhập kho'" class="text-emerald-600 font-bold bg-emerald-50 px-2 py-1 rounded text-xs">{{ log.type }}</span>
                      <span v-else class="text-amber-600 font-bold bg-amber-50 px-2 py-1 rounded text-xs">{{ log.type }}</span>
                    </td>
                    <td class="px-4 py-3 font-medium text-gray-800">{{ log.refCode }}</td>
                    <td class="px-4 py-3 text-right font-bold" :class="log.qtyChange.startsWith('+') ? 'text-emerald-600' : 'text-amber-600'">{{ log.qtyChange }}</td>
                    <td class="px-4 py-3 text-right font-bold text-gray-900">{{ log.balance }}</td>
                  </tr>
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
</style>