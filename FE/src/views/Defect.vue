<script setup>
import { ref, computed } from 'vue'
import { 
  MagnifyingGlassIcon, PlusIcon, PencilSquareIcon, 
  TrashIcon, XMarkIcon, ShieldExclamationIcon 
} from '@heroicons/vue/24/outline'

// === 1. STATE CHÍNH: TRỐNG CHỜ API ===
const defects = ref([])
const mockProducts = ref([]) // Chờ API danh sách sản phẩm

const searchQuery = ref('')
const filterStatus = ref('')

const filteredDefects = computed(() => {
  return defects.value.filter(d => {
    const matchSearch = d.code.toLowerCase().includes(searchQuery.value.toLowerCase()) || 
                        d.name.toLowerCase().includes(searchQuery.value.toLowerCase())
    const matchStatus = filterStatus.value === '' || d.status === filterStatus.value
    return matchSearch && matchStatus
  }).sort((a, b) => b.id - a.id)
})

const showModal = ref(false)
const modalMode = ref('add') 
const formData = ref({ id: null, code: '', date: '', sku: '', name: '', unit: '', qty: 1, reason: '', reporter: '', status: 'pending' })

const generateCode = () => `HL${(defects.value.length + 1).toString().padStart(4, '0')}`
const getToday = () => new Date().toISOString().split('T')[0]

const openModal = (mode, defect = null) => {
  modalMode.value = mode
  if (defect) formData.value = { ...defect } 
  else formData.value = { id: null, code: '', date: getToday(), sku: '', name: '', unit: '', qty: 1, reason: '', reporter: '', status: 'pending' }
  showModal.value = true
}

const closeModal = () => showModal.value = false

const getStatusBadge = (status) => {
  switch(status) {
    case 'pending': return { text: 'Chờ xử lý', class: 'bg-amber-100 text-amber-700 border-amber-200' }
    case 'destroyed': return { text: 'Đã tiêu hủy', class: 'bg-red-100 text-red-700 border-red-200' }
    case 'returned': return { text: 'Trả NCC', class: 'bg-blue-100 text-blue-700 border-blue-200' }
    default: return { text: 'Khác', class: 'bg-gray-100 text-gray-500' }
  }
}

const handleProductChange = () => {
  const prod = mockProducts.value.find(p => p.sku === formData.value.sku)
  if (prod) { formData.value.name = prod.name; formData.value.unit = prod.unit; }
}

const handleSubmit = () => {
  if (!formData.value.sku) return alert('Vui lòng chọn Sản phẩm bị lỗi!')
  if (modalMode.value === 'add') {
    defects.value.push({ ...formData.value, id: Date.now(), code: generateCode() })
    alert('Ghi nhận hàng lỗi thành công!')
  } else {
    const idx = defects.value.findIndex(d => d.id === formData.value.id)
    if (idx !== -1) defects.value[idx] = { ...formData.value }
    alert('Cập nhật trạng thái thành công!')
  }
  closeModal()
}

const handleDelete = (id, code) => {
  if (confirm(`Sếp có chắc chắn muốn xóa báo cáo lỗi [${code}] không?`)) {
    defects.value = defects.value.filter(d => d.id !== id)
  }
}
</script>

<template>
  <div class="space-y-5 md:space-y-6 animate-fade-in pb-10 px-0 md:px-1 relative">
    <div class="flex flex-col sm:flex-row sm:items-center justify-between gap-4">
      <div>
        <h2 class="text-xl md:text-2xl font-bold text-gray-800">Quản lý Hàng Lỗi / Hư hỏng</h2>
        <p class="text-xs md:text-sm text-gray-500 mt-1">Ghi nhận hàng hóa bị hỏng hóc để loại trừ khỏi tồn kho khả dụng</p>
      </div>
      <button @click="openModal('add')" class="bg-red-600 hover:bg-red-700 text-white px-4 py-2.5 rounded-lg flex items-center gap-2 text-sm font-semibold shadow-sm">
        <PlusIcon class="w-5 h-5" /> Báo cáo Hàng lỗi
      </button>
    </div>

    <div class="bg-white p-3 md:p-4 rounded-xl border border-gray-200 flex flex-col sm:flex-row items-center gap-3 md:gap-4 shadow-sm">
      <div class="relative w-full sm:flex-1">
        <div class="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none"><MagnifyingGlassIcon class="w-5 h-5 text-gray-400" /></div>
        <input v-model="searchQuery" type="text" class="block w-full pl-10 pr-3 py-2 border border-gray-200 rounded-lg text-sm outline-none focus:ring-1 focus:ring-primary-500" placeholder="Tìm theo mã phiếu, tên sản phẩm...">
      </div>
      <select v-model="filterStatus" class="w-full sm:w-auto border border-gray-200 rounded-lg text-sm px-4 py-2 outline-none focus:ring-1 focus:ring-primary-500 cursor-pointer">
        <option value="">Tất cả Trạng thái</option>
        <option value="pending">Chờ xử lý</option>
        <option value="destroyed">Đã tiêu hủy</option>
        <option value="returned">Trả Nhà cung cấp</option>
      </select>
    </div>

    <div class="bg-white rounded-xl border border-gray-200 shadow-sm overflow-hidden">
      <div class="w-full overflow-x-auto custom-scrollbar">
        <table class="min-w-[1000px] w-full divide-y divide-gray-200">
          <thead class="bg-gray-50">
            <tr>
              <th class="px-5 py-3.5 text-left text-xs font-bold text-gray-500 uppercase tracking-wider">Mã Phiếu</th>
              <th class="px-5 py-3.5 text-left text-xs font-bold text-gray-500 uppercase tracking-wider">Sản phẩm lỗi</th>
              <th class="px-5 py-3.5 text-center text-xs font-bold text-gray-500 uppercase tracking-wider">Số lượng</th>
              <th class="px-5 py-3.5 text-left text-xs font-bold text-gray-500 uppercase tracking-wider">Nguyên nhân</th>
              <th class="px-5 py-3.5 text-left text-xs font-bold text-gray-500 uppercase tracking-wider">Người báo cáo</th>
              <th class="px-5 py-3.5 text-center text-xs font-bold text-gray-500 uppercase tracking-wider">Trạng thái</th>
              <th class="px-5 py-3.5 text-right text-xs font-bold text-gray-500 uppercase tracking-wider">Thao tác</th>
            </tr>
          </thead>
          <tbody class="divide-y divide-gray-100 bg-white">
            <tr v-if="filteredDefects.length === 0">
              <td colspan="7" class="px-6 py-16 text-center">
                <ShieldExclamationIcon class="w-12 h-12 text-gray-300 mx-auto mb-3" />
                <h3 class="text-base font-semibold text-gray-700">Chưa có báo cáo hàng lỗi nào</h3>
              </td>
            </tr>
            <tr v-for="defect in filteredDefects" :key="defect.id" class="hover:bg-gray-50">
              <td class="px-5 py-3 text-sm font-bold text-red-600">{{ defect.code }}</td>
              <td class="px-5 py-3"><div class="flex flex-col"><span class="text-sm font-bold text-gray-900">{{ defect.name }}</span><span class="text-xs text-gray-500">Mã: {{ defect.sku }}</span></div></td>
              <td class="px-5 py-3 text-sm text-center font-bold text-gray-900">{{ defect.qty }} <span class="text-xs font-normal text-gray-500">{{ defect.unit }}</span></td>
              <td class="px-5 py-3 text-sm text-gray-700 max-w-[250px] truncate" :title="defect.reason">{{ defect.reason }}</td>
              <td class="px-5 py-3 text-sm text-gray-600">{{ defect.reporter }}</td>
              <td class="px-5 py-3 text-center"><span :class="['text-[10px] font-bold px-2 py-1 rounded border uppercase tracking-wider', getStatusBadge(defect.status).class]">{{ getStatusBadge(defect.status).text }}</span></td>
              <td class="px-5 py-3 text-right space-x-2">
                <button @click="openModal('edit', defect)" class="p-1.5 text-amber-600 hover:bg-amber-50 rounded" title="Cập nhật xử lý"><PencilSquareIcon class="w-5 h-5" /></button>
                <button @click="handleDelete(defect.id, defect.code)" class="p-1.5 text-red-600 hover:bg-red-50 rounded" title="Xóa báo cáo"><TrashIcon class="w-5 h-5" /></button>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>

    <Teleport to="body">
      <div v-if="showModal" class="fixed inset-0 z-50 flex items-center justify-center px-4 bg-slate-900/50 backdrop-blur-sm animate-fade-in">
        <div class="bg-white rounded-xl shadow-2xl w-full max-w-2xl overflow-hidden flex flex-col">
          <div class="px-6 py-4 border-b border-red-100 flex items-center justify-between bg-red-50 shrink-0">
            <h3 class="text-lg font-bold text-red-800 flex items-center gap-2"><ShieldExclamationIcon class="w-6 h-6 text-red-600"/> {{ modalMode === 'add' ? 'Báo cáo Hàng Lỗi' : `Cập nhật xử lý: ${formData.code}` }}</h3>
            <button @click="closeModal" class="text-gray-400 hover:text-red-500"><XMarkIcon class="w-6 h-6" /></button>
          </div>
          <div class="p-6 overflow-y-auto flex-1 custom-scrollbar">
            <form @submit.prevent="handleSubmit" class="space-y-5">
              <div class="grid grid-cols-1 sm:grid-cols-2 gap-4">
                <div><label class="block text-xs font-bold mb-1">Mã Phiếu</label><input type="text" disabled class="w-full border rounded-lg px-3 py-2 text-sm bg-gray-100 italic" :placeholder="modalMode === 'add' ? 'Tự động sinh' : formData.code"></div>
                <div><label class="block text-xs font-bold mb-1">Ngày ghi nhận *</label><input v-model="formData.date" type="date" required class="w-full border rounded-lg px-3 py-2 text-sm focus:ring-red-500"></div>
              </div>
              <div class="bg-slate-50 p-4 rounded-lg border border-slate-200">
                <label class="block text-xs font-bold mb-1">Sản phẩm bị lỗi *</label>
                <select v-model="formData.sku" @change="handleProductChange" :disabled="modalMode === 'edit'" required class="w-full border rounded-lg px-3 py-2 text-sm focus:ring-red-500 mb-4 disabled:bg-gray-100">
                  <option value="" disabled>-- Chọn Sản phẩm --</option>
                  <option v-for="prod in mockProducts" :key="prod.sku" :value="prod.sku">{{ prod.sku }} - {{ prod.name }}</option>
                </select>
                <div class="grid grid-cols-1 sm:grid-cols-2 gap-4">
                  <div>
                    <label class="block text-xs font-bold mb-1">Số lượng lỗi *</label>
                    <div class="flex items-center">
                      <input v-model.number="formData.qty" type="number" min="1" required class="w-full border rounded-l-lg px-3 py-2 text-sm focus:ring-red-500 font-bold text-red-600">
                      <span class="bg-gray-100 border-y border-r px-3 py-2 text-sm rounded-r-lg text-gray-600">{{ formData.unit || 'ĐVT' }}</span>
                    </div>
                  </div>
                  <div><label class="block text-xs font-bold mb-1">Người báo cáo *</label><input v-model="formData.reporter" type="text" required class="w-full border rounded-lg px-3 py-2 text-sm focus:ring-red-500"></div>
                </div>
              </div>
              <div><label class="block text-xs font-bold mb-1">Nguyên nhân / Tình trạng lỗi *</label><textarea v-model="formData.reason" rows="3" required class="w-full border rounded-lg px-3 py-2 text-sm focus:ring-red-500" placeholder="Mô tả..."></textarea></div>
              <div class="border-t pt-4">
                <label class="block text-xs font-bold mb-2">Trạng thái xử lý *</label>
                <div class="flex flex-wrap gap-4">
                  <label class="flex items-center gap-2 cursor-pointer"><input type="radio" v-model="formData.status" value="pending" class="text-amber-600 focus:ring-amber-500"><span class="text-sm font-medium text-amber-700">Chờ xử lý</span></label>
                  <label class="flex items-center gap-2 cursor-pointer"><input type="radio" v-model="formData.status" value="returned" class="text-blue-600 focus:ring-blue-500"><span class="text-sm font-medium text-blue-700">Trả Nhà Cung Cấp</span></label>
                  <label class="flex items-center gap-2 cursor-pointer"><input type="radio" v-model="formData.status" value="destroyed" class="text-red-600 focus:ring-red-500"><span class="text-sm font-medium text-red-700">Đã tiêu hủy</span></label>
                </div>
              </div>
            </form>
          </div>
          <div class="px-6 py-4 border-t flex justify-end gap-3 bg-white shrink-0">
            <button type="button" @click="closeModal" class="px-5 py-2.5 border rounded-lg text-sm font-semibold hover:bg-gray-50">Hủy bỏ</button>
            <button type="submit" @click="handleSubmit" class="px-5 py-2.5 bg-red-600 text-white rounded-lg text-sm font-bold hover:bg-red-700 shadow-md">Lưu Báo Cáo</button>
          </div>
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