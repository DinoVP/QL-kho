<script setup>
import { ref, computed } from 'vue'
import { 
  MagnifyingGlassIcon, DocumentTextIcon, FunnelIcon, 
  ArrowDownCircleIcon, ArrowUpCircleIcon, ArrowsRightLeftIcon, ShieldExclamationIcon, ClipboardDocumentCheckIcon
} from '@heroicons/vue/24/outline'

// === 1. STATE CHÍNH: TRỐNG CHỜ API ===
const transactions = ref([])

// === 2. BỘ LỌC ===
const searchQuery = ref('')
const filterType = ref('')
const filterWarehouse = ref('')

const filteredTransactions = computed(() => {
  return transactions.value.filter(t => {
    const matchSearch = t.refCode.toLowerCase().includes(searchQuery.value.toLowerCase()) || 
                        t.sku.toLowerCase().includes(searchQuery.value.toLowerCase()) ||
                        t.name.toLowerCase().includes(searchQuery.value.toLowerCase())
    const matchType = filterType.value === '' || t.type.includes(filterType.value)
    const matchWarehouse = filterWarehouse.value === '' || t.warehouse.includes(filterWarehouse.value)
    
    return matchSearch && matchType && matchWarehouse
  }).sort((a, b) => b.id - a.id) 
})

// === 3. HÀM RENDER MÀU SẮC ===
const getTypeDisplay = (type) => {
  if (type.includes('Nhập kho')) return { class: 'bg-emerald-100 text-emerald-700 border-emerald-200', icon: ArrowDownCircleIcon }
  if (type.includes('Xuất kho')) return { class: 'bg-amber-100 text-amber-700 border-amber-200', icon: ArrowUpCircleIcon }
  if (type.includes('Điều chuyển')) return { class: 'bg-blue-100 text-blue-700 border-blue-200', icon: ArrowsRightLeftIcon }
  if (type.includes('Hàng lỗi')) return { class: 'bg-red-100 text-red-700 border-red-200', icon: ShieldExclamationIcon }
  if (type.includes('Kiểm kê')) return { class: 'bg-purple-100 text-purple-700 border-purple-200', icon: ClipboardDocumentCheckIcon }
  return { class: 'bg-gray-100 text-gray-700 border-gray-200', icon: DocumentTextIcon }
}

const formatQtyChange = (qty) => qty > 0 ? `+${qty}` : qty

const getQtyColorClass = (qty) => {
  if (qty > 0) return 'text-emerald-600 bg-emerald-50'
  if (qty < 0) return 'text-amber-600 bg-amber-50'
  return 'text-gray-500 bg-gray-50'
}

const handleExportExcel = () => {
  alert('Hệ thống đang chờ API để xuất dữ liệu giao dịch ra Excel.')
}
</script>

<template>
  <div class="space-y-5 md:space-y-6 animate-fade-in pb-10 px-0 md:px-1 relative">
    
    <div class="flex flex-col sm:flex-row sm:items-center justify-between gap-4">
      <div>
        <h2 class="text-xl md:text-2xl font-bold text-gray-800">Sổ Giao Dịch (Transaction Ledger)</h2>
        <p class="text-xs md:text-sm text-gray-500 mt-1">Lịch sử mọi biến động số lượng hàng hóa trong tất cả các kho</p>
      </div>
      <button @click="handleExportExcel" class="bg-white border border-emerald-200 text-emerald-700 px-4 py-2.5 rounded-lg text-sm font-bold hover:bg-emerald-50 transition-colors shadow-sm flex items-center gap-2">
        <DocumentTextIcon class="w-5 h-5"/> Xuất Excel
      </button>
    </div>

    <div class="bg-white p-3 md:p-4 rounded-xl border border-gray-200 flex flex-col lg:flex-row items-center gap-3 md:gap-4 shadow-sm">
      <div class="relative w-full lg:flex-1">
        <div class="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none"><MagnifyingGlassIcon class="w-5 h-5 text-gray-400" /></div>
        <input v-model="searchQuery" type="text" class="block w-full pl-10 pr-3 py-2 border border-gray-200 rounded-lg text-sm outline-none focus:ring-1 focus:ring-primary-500" placeholder="Tìm theo mã chứng từ, mã SKU, tên hàng...">
      </div>
      <div class="flex flex-col sm:flex-row gap-3 w-full lg:w-auto">
        <select v-model="filterType" class="w-full sm:w-auto border border-gray-200 rounded-lg text-sm px-4 py-2 outline-none focus:ring-1 focus:ring-primary-500 cursor-pointer">
          <option value="">Tất cả Loại giao dịch</option><option value="Nhập kho">Nhập kho</option><option value="Xuất kho">Xuất kho</option><option value="Điều chuyển">Điều chuyển</option><option value="Hàng lỗi">Hàng lỗi</option><option value="Kiểm kê">Kiểm kê (Bù trừ)</option>
        </select>
        <select v-model="filterWarehouse" class="w-full sm:w-auto border border-gray-200 rounded-lg text-sm px-4 py-2 outline-none focus:ring-1 focus:ring-primary-500 cursor-pointer">
          <option value="">Tất cả Kho</option><option value="Miền Bắc">Tổng kho Miền Bắc</option><option value="Miền Nam">Chi nhánh Miền Nam</option><option value="Đà Nẵng">Trung tâm Đà Nẵng</option>
        </select>
        <button class="w-full sm:w-auto bg-gray-50 border border-gray-200 text-gray-600 px-4 py-2 rounded-lg flex items-center justify-center gap-2 text-sm hover:bg-gray-100 transition-colors">
          <FunnelIcon class="w-4 h-4" /> Lọc Ngày
        </button>
      </div>
    </div>

    <div class="bg-white rounded-xl border border-gray-200 shadow-sm overflow-hidden">
      <div class="w-full overflow-x-auto custom-scrollbar">
        <table class="min-w-[1100px] w-full divide-y divide-gray-200">
          <thead class="bg-slate-50">
            <tr>
              <th class="px-5 py-3.5 text-left text-xs font-bold text-slate-500 uppercase tracking-wider">Thời gian</th>
              <th class="px-5 py-3.5 text-left text-xs font-bold text-slate-500 uppercase tracking-wider">Mã Chứng Từ</th>
              <th class="px-5 py-3.5 text-left text-xs font-bold text-slate-500 uppercase tracking-wider">Loại Giao Dịch</th>
              <th class="px-5 py-3.5 text-left text-xs font-bold text-slate-500 uppercase tracking-wider">Sản phẩm (SKU)</th>
              <th class="px-5 py-3.5 text-left text-xs font-bold text-slate-500 uppercase tracking-wider">Thuộc Kho</th>
              <th class="px-5 py-3.5 text-right text-xs font-bold text-slate-500 uppercase tracking-wider">Biến động (SL)</th>
              <th class="px-5 py-3.5 text-left text-xs font-bold text-slate-500 uppercase tracking-wider">Người thực hiện</th>
            </tr>
          </thead>
          <tbody class="divide-y divide-gray-100 bg-white">
            <tr v-if="filteredTransactions.length === 0">
              <td colspan="7" class="px-6 py-16 text-center">
                <DocumentTextIcon class="w-12 h-12 text-gray-300 mx-auto mb-3" />
                <h3 class="text-base font-semibold text-gray-700">Chưa có giao dịch nào phát sinh</h3>
              </td>
            </tr>
            <tr v-for="t in filteredTransactions" :key="t.id" class="hover:bg-slate-50">
              <td class="px-5 py-4 text-sm font-medium text-gray-500 whitespace-nowrap">{{ t.date }}</td>
              <td class="px-5 py-4"><a href="#" class="text-sm font-bold text-primary-600 hover:text-primary-800 hover:underline">{{ t.refCode }}</a></td>
              <td class="px-5 py-4"><span :class="['text-[10px] font-bold px-2.5 py-1 rounded border flex items-center gap-1 w-max', getTypeDisplay(t.type).class]"><component :is="getTypeDisplay(t.type).icon" class="w-4 h-4" />{{ t.type }}</span></td>
              <td class="px-5 py-4"><div class="flex flex-col"><span class="text-sm font-bold text-gray-900">{{ t.name }}</span><span class="text-xs text-gray-500">Mã: {{ t.sku }}</span></div></td>
              <td class="px-5 py-4 text-sm font-medium text-gray-700">{{ t.warehouse }}</td>
              <td class="px-5 py-4 text-right"><span :class="['text-base font-bold px-3 py-1 rounded-md', getQtyColorClass(t.qtyChange)]">{{ formatQtyChange(t.qtyChange) }} <span class="text-xs font-normal opacity-80">{{ t.unit }}</span></span></td>
              <td class="px-5 py-4 text-sm text-gray-600">{{ t.actor }}</td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>
  </div>
</template>

<style scoped>
.custom-scrollbar::-webkit-scrollbar { height: 6px; width: 6px; }
.custom-scrollbar::-webkit-scrollbar-track { background: transparent; }
.custom-scrollbar::-webkit-scrollbar-thumb { background: #cbd5e1; border-radius: 10px; }
</style>