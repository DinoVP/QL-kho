<script setup>
import { ref, computed } from 'vue'
import { 
  MagnifyingGlassIcon, PlusIcon, PencilSquareIcon, 
  TrashIcon, MapPinIcon, XMarkIcon, AdjustmentsVerticalIcon
} from '@heroicons/vue/24/outline'

// === 1. STATE CHÍNH: TRỐNG CHỜ API ===
const locations = ref([])

// === 2. BỘ LỌC TÌM KIẾM ===
const searchQuery = ref('')
const filterWarehouse = ref('')
const filterStatus = ref('')

const filteredLocations = computed(() => {
  return locations.value.filter(loc => {
    const matchSearch = loc.code.toLowerCase().includes(searchQuery.value.toLowerCase()) || 
                        loc.zone.toLowerCase().includes(searchQuery.value.toLowerCase()) ||
                        loc.rack.toLowerCase().includes(searchQuery.value.toLowerCase())
    const matchWarehouse = filterWarehouse.value === '' || loc.warehouse.includes(filterWarehouse.value)
    const matchStatus = filterStatus.value === '' || loc.status === filterStatus.value
    
    return matchSearch && matchWarehouse && matchStatus
  }).sort((a, b) => a.id - b.id)
})

// === 3. LOGIC MODAL TẠO / SỬA VỊ TRÍ ===
const showModal = ref(false)
const modalMode = ref('add') 

const formData = ref({ 
  id: null, code: '', warehouse: 'Tổng kho Miền Bắc', zone: '', rack: '', 
  tier: 1, bin: 1, type: 'Tiêu chuẩn', capacity: 100, currentQty: 0, status: 'empty' 
})

const generateCode = () => {
  const whPrefix = formData.value.warehouse === 'Tổng kho Miền Bắc' ? 'HN' : (formData.value.warehouse === 'Chi nhánh Miền Nam' ? 'HCM' : 'DN')
  const zoneStr = formData.value.zone ? formData.value.zone.replace('Dãy ', '') : 'X'
  const rackStr = formData.value.rack ? formData.value.rack.replace('Kệ ', '') : '00'
  return `${whPrefix}-${zoneStr}-${rackStr}-T${formData.value.tier}-O${formData.value.bin}`
}

const openModal = (mode, loc = null) => {
  modalMode.value = mode
  if (loc) {
    formData.value = { ...loc } 
  } else {
    formData.value = { id: null, code: '', warehouse: 'Tổng kho Miền Bắc', zone: 'Dãy A', rack: 'Kệ 01', tier: 1, bin: 1, type: 'Tiêu chuẩn', capacity: 100, currentQty: 0, status: 'empty' }
    formData.value.code = generateCode()
  }
  showModal.value = true
}

const closeModal = () => showModal.value = false

const updateAutoCode = () => {
  if (modalMode.value === 'add') formData.value.code = generateCode()
}

const getStatusBadge = (status) => {
  switch(status) {
    case 'empty': return { text: 'Trống (Trống)', class: 'bg-gray-100 text-gray-700 border-gray-300' }
    case 'partial': return { text: 'Đang chứa', class: 'bg-blue-100 text-blue-700 border-blue-300' }
    case 'full': return { text: 'Đầy (100%)', class: 'bg-red-100 text-red-700 border-red-300' }
    case 'maintenance': return { text: 'Bảo trì / Khóa', class: 'bg-amber-100 text-amber-700 border-amber-300' }
    default: return { text: 'Khác', class: 'bg-gray-100 text-gray-500' }
  }
}

const getCapacityPercent = (current, max) => {
  if (max === 0) return 0
  const pct = Math.round((current / max) * 100)
  return pct > 100 ? 100 : pct
}

const getProgressBarColor = (pct) => {
  if (pct === 0) return 'bg-gray-200'
  if (pct < 80) return 'bg-blue-500'
  if (pct < 100) return 'bg-amber-500'
  return 'bg-red-500'
}

const handleSubmit = () => {
  if (modalMode.value === 'add') {
    if (formData.value.currentQty === 0) formData.value.status = 'empty'
    else if (formData.value.currentQty >= formData.value.capacity) formData.value.status = 'full'
    else formData.value.status = 'partial'

    locations.value.push({ ...formData.value, id: Date.now() })
    alert('Thêm Vị trí lưu kho thành công!')
  } else {
    const idx = locations.value.findIndex(l => l.id === formData.value.id)
    if (idx !== -1) {
      if (formData.value.currentQty === 0 && formData.value.status !== 'maintenance') formData.value.status = 'empty'
      else if (formData.value.currentQty >= formData.value.capacity && formData.value.status !== 'maintenance') formData.value.status = 'full'
      else if (formData.value.status !== 'maintenance') formData.value.status = 'partial'
      
      locations.value[idx] = { ...formData.value }
    }
    alert('Cập nhật Vị trí lưu kho thành công!')
  }
  closeModal()
}

const handleDelete = (id, code) => {
  if (confirm(`Sếp có chắc chắn muốn xóa Vị trí [${code}] không?`)) {
    locations.value = locations.value.filter(l => l.id !== id)
  }
}
</script>

<template>
  <div class="space-y-5 md:space-y-6 animate-fade-in pb-10 px-0 md:px-1 relative">
    <div class="flex flex-col sm:flex-row sm:items-center justify-between gap-4">
      <div>
        <h2 class="text-xl md:text-2xl font-bold text-gray-800">Vị trí Lưu Kho (Bin Locations)</h2>
        <p class="text-xs md:text-sm text-gray-500 mt-1">Danh sách chi tiết và thông số sức chứa của từng ô kệ trong kho</p>
      </div>
      <div class="flex gap-2">
        <button class="bg-white border border-gray-300 text-gray-700 px-4 py-2.5 rounded-lg text-sm font-semibold hover:bg-gray-50 transition-colors shadow-sm flex items-center gap-2">
          <AdjustmentsVerticalIcon class="w-5 h-5"/> Sinh mã tự động
        </button>
        <button @click="openModal('add')" class="bg-slate-800 hover:bg-slate-900 text-white px-4 py-2.5 rounded-lg flex items-center gap-2 text-sm font-semibold transition-colors shadow-sm">
          <PlusIcon class="w-5 h-5" /> Thêm Vị Trí Mới
        </button>
      </div>
    </div>

    <div class="bg-white p-3 md:p-4 rounded-xl border border-gray-200 flex flex-col md:flex-row items-center gap-3 md:gap-4 shadow-sm">
      <div class="relative w-full md:flex-1">
        <div class="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none"><MagnifyingGlassIcon class="w-5 h-5 text-gray-400" /></div>
        <input v-model="searchQuery" type="text" class="block w-full pl-10 pr-3 py-2 border border-gray-200 rounded-lg text-sm outline-none focus:ring-1 focus:ring-slate-500" placeholder="Tìm theo mã vị trí, Dãy, Kệ...">
      </div>
      
      <div class="flex flex-col sm:flex-row gap-3 w-full md:w-auto">
        <select v-model="filterWarehouse" class="w-full sm:w-auto border border-gray-200 rounded-lg text-sm px-4 py-2 outline-none focus:ring-1 focus:ring-slate-500 cursor-pointer">
          <option value="">Tất cả Kho</option><option value="Miền Bắc">Tổng kho Miền Bắc</option><option value="Miền Nam">Chi nhánh Miền Nam</option><option value="Đà Nẵng">Trung tâm Đà Nẵng</option>
        </select>
        <select v-model="filterStatus" class="w-full sm:w-auto border border-gray-200 rounded-lg text-sm px-4 py-2 outline-none focus:ring-1 focus:ring-slate-500 cursor-pointer">
          <option value="">Tất cả Trạng thái</option><option value="empty">Trống</option><option value="partial">Đang chứa</option><option value="full">Đầy (Full)</option><option value="maintenance">Bảo trì / Khóa</option>
        </select>
      </div>
    </div>

    <div class="bg-white rounded-xl border border-gray-200 shadow-sm overflow-hidden">
      <div class="w-full overflow-x-auto custom-scrollbar">
        <table class="min-w-[1050px] w-full divide-y divide-gray-200">
          <thead class="bg-gray-50">
            <tr>
              <th class="px-5 py-3.5 text-left text-xs font-bold text-gray-500 uppercase tracking-wider">Mã Vị Trí (Bin Code)</th>
              <th class="px-5 py-3.5 text-left text-xs font-bold text-gray-500 uppercase tracking-wider">Thuộc Kho</th>
              <th class="px-5 py-3.5 text-center text-xs font-bold text-gray-500 uppercase tracking-wider">Tọa độ (Dãy-Kệ-Tầng-Ô)</th>
              <th class="px-5 py-3.5 text-center text-xs font-bold text-gray-500 uppercase tracking-wider">Phân loại</th>
              <th class="px-5 py-3.5 text-left text-xs font-bold text-gray-500 uppercase tracking-wider">Sức chứa (Capacity)</th>
              <th class="px-5 py-3.5 text-center text-xs font-bold text-gray-500 uppercase tracking-wider">Trạng thái</th>
              <th class="px-5 py-3.5 text-right text-xs font-bold text-gray-500 uppercase tracking-wider">Thao tác</th>
            </tr>
          </thead>
          <tbody class="divide-y divide-gray-100 bg-white">
            <tr v-if="filteredLocations.length === 0">
              <td colspan="7" class="px-6 py-16 text-center">
                <MapPinIcon class="w-12 h-12 text-gray-300 mx-auto mb-3" />
                <h3 class="text-base font-semibold text-gray-700">Chưa có vị trí lưu kho nào</h3>
                <p class="text-sm text-gray-500 mt-1">Hệ thống đang chờ API đổ dữ liệu vị trí.</p>
              </td>
            </tr>
            <tr v-for="loc in filteredLocations" :key="loc.id" class="hover:bg-gray-50">
              <td class="px-5 py-3 text-sm font-bold text-slate-800 flex items-center gap-2"><MapPinIcon class="w-4 h-4 text-gray-400" />{{ loc.code }}</td>
              <td class="px-5 py-3 text-sm font-medium text-gray-600">{{ loc.warehouse }}</td>
              <td class="px-5 py-3 text-center">
                <div class="inline-flex gap-1">
                  <span class="bg-gray-100 text-gray-700 px-2 py-0.5 rounded text-xs font-bold border border-gray-200">{{ loc.zone }}</span>
                  <span class="bg-gray-100 text-gray-700 px-2 py-0.5 rounded text-xs font-bold border border-gray-200">{{ loc.rack }}</span>
                  <span class="bg-gray-100 text-gray-700 px-2 py-0.5 rounded text-xs font-bold border border-gray-200">T{{ loc.tier }}</span>
                  <span class="bg-gray-100 text-gray-700 px-2 py-0.5 rounded text-xs font-bold border border-gray-200">Ô {{ loc.bin }}</span>
                </div>
              </td>
              <td class="px-5 py-3 text-center"><span class="text-xs font-semibold px-2 py-1 rounded-full border border-gray-200 bg-white text-gray-600 shadow-sm">{{ loc.type }}</span></td>
              <td class="px-5 py-3">
                <div class="flex items-center gap-3">
                  <div class="w-full bg-gray-200 rounded-full h-2.5 overflow-hidden border border-gray-300">
                    <div class="h-2.5 rounded-full" :class="getProgressBarColor(getCapacityPercent(loc.currentQty, loc.capacity))" :style="{ width: `${getCapacityPercent(loc.currentQty, loc.capacity)}%` }"></div>
                  </div>
                  <span class="text-xs font-bold text-gray-700 whitespace-nowrap w-16 text-right">{{ loc.currentQty }} / {{ loc.capacity }}</span>
                </div>
              </td>
              <td class="px-5 py-3 text-center"><span :class="['text-[10px] font-bold px-2.5 py-1 rounded border uppercase tracking-wider whitespace-nowrap', getStatusBadge(loc.status).class]">{{ getStatusBadge(loc.status).text }}</span></td>
              <td class="px-5 py-3 text-right space-x-2 whitespace-nowrap">
                <button @click="openModal('edit', loc)" class="p-1.5 text-amber-600 hover:bg-amber-50 rounded" title="Chỉnh sửa"><PencilSquareIcon class="w-5 h-5" /></button>
                <button @click="handleDelete(loc.id, loc.code)" class="p-1.5 text-red-600 hover:bg-red-50 rounded" title="Xóa"><TrashIcon class="w-5 h-5" /></button>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>

    <Teleport to="body">
      <div v-if="showModal" class="fixed inset-0 z-50 flex items-center justify-center px-4 bg-slate-900/50 backdrop-blur-sm animate-fade-in">
        <div class="bg-white rounded-xl shadow-2xl w-full max-w-2xl overflow-hidden flex flex-col max-h-[90vh]">
          <div class="px-6 py-4 border-b border-gray-100 flex items-center justify-between bg-gray-50 shrink-0">
            <h3 class="text-lg font-bold text-gray-800 flex items-center gap-2"><MapPinIcon class="w-6 h-6 text-slate-800"/> {{ modalMode === 'add' ? 'Thêm Vị Trí Lưu Kho Mới' : `Cập nhật Vị Trí: ${formData.code}` }}</h3>
            <button @click="closeModal" class="text-gray-400 hover:text-red-500"><XMarkIcon class="w-6 h-6" /></button>
          </div>
          <div class="p-6 overflow-y-auto flex-1 custom-scrollbar">
            <form @submit.prevent="handleSubmit" class="space-y-6">
              <div class="grid grid-cols-1 sm:grid-cols-2 gap-4">
                <div><label class="block text-xs font-bold mb-1">Mã Vị Trí *</label><input v-model="formData.code" type="text" required class="w-full border rounded-lg px-3 py-2 text-sm font-bold text-slate-700"></div>
                <div><label class="block text-xs font-bold mb-1">Thuộc Kho *</label>
                  <select v-model="formData.warehouse" @change="updateAutoCode" required class="w-full border rounded-lg px-3 py-2 text-sm cursor-pointer">
                    <option value="Tổng kho Miền Bắc">Tổng kho Miền Bắc</option><option value="Chi nhánh Miền Nam">Chi nhánh Miền Nam</option><option value="Trung tâm Đà Nẵng">Trung tâm Đà Nẵng</option>
                  </select>
                </div>
              </div>
              <div class="bg-slate-50 p-4 rounded-lg border border-slate-200">
                <h4 class="text-sm font-bold mb-3 border-b pb-2">Tọa độ vật lý</h4>
                <div class="grid grid-cols-2 sm:grid-cols-4 gap-4">
                  <div><label class="block text-xs font-bold mb-1">Dãy *</label><input v-model="formData.zone" @input="updateAutoCode" type="text" required class="w-full border rounded-lg px-3 py-2 text-sm"></div>
                  <div><label class="block text-xs font-bold mb-1">Kệ *</label><input v-model="formData.rack" @input="updateAutoCode" type="text" required class="w-full border rounded-lg px-3 py-2 text-sm"></div>
                  <div><label class="block text-xs font-bold mb-1">Tầng *</label><input v-model.number="formData.tier" @input="updateAutoCode" type="number" min="1" required class="w-full border rounded-lg px-3 py-2 text-sm text-center"></div>
                  <div><label class="block text-xs font-bold mb-1">Ô *</label><input v-model.number="formData.bin" @input="updateAutoCode" type="number" min="1" required class="w-full border rounded-lg px-3 py-2 text-sm text-center"></div>
                </div>
              </div>
              <div class="grid grid-cols-1 sm:grid-cols-2 gap-4">
                <div><label class="block text-xs font-bold mb-1">Phân loại vị trí *</label>
                  <select v-model="formData.type" required class="w-full border rounded-lg px-3 py-2 text-sm cursor-pointer">
                    <option>Tiêu chuẩn</option><option>Kho lạnh</option><option>Dễ cháy nổ</option><option>Hàng cồng kềnh</option>
                  </select>
                </div>
                <div><label class="block text-xs font-bold mb-1">Sức chứa tối đa *</label><input v-model.number="formData.capacity" type="number" min="1" required class="w-full border rounded-lg px-3 py-2 text-sm"></div>
              </div>
              <div class="border-t pt-4">
                <label class="block text-xs font-bold mb-2">Hiện trạng thực tế</label>
                <div class="grid grid-cols-1 sm:grid-cols-2 gap-4 items-center">
                  <div><label class="block text-xs font-medium text-gray-500 mb-1">Số lượng đang chứa</label><input v-model.number="formData.currentQty" type="number" min="0" :max="formData.capacity" class="w-full border rounded-lg px-3 py-2 text-sm font-bold text-blue-700"></div>
                  <div class="pt-4"><label class="flex items-center gap-2 cursor-pointer p-2 rounded border border-amber-200 bg-amber-50"><input type="checkbox" :checked="formData.status === 'maintenance'" @change="e => formData.status = e.target.checked ? 'maintenance' : 'empty'" class="w-4 h-4 text-amber-600 rounded"><span class="text-sm font-bold text-amber-700">Khóa để Bảo trì</span></label></div>
                </div>
              </div>
            </form>
          </div>
          <div class="px-6 py-4 border-t flex justify-end gap-3 bg-white shrink-0">
            <button type="button" @click="closeModal" class="px-5 py-2.5 border rounded-lg text-sm font-semibold hover:bg-gray-50">Hủy bỏ</button>
            <button type="submit" @click="handleSubmit" class="px-5 py-2.5 bg-slate-800 text-white rounded-lg text-sm font-bold hover:bg-slate-900 shadow-md"><MapPinIcon class="w-5 h-5 inline mr-1"/> Lưu</button>
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