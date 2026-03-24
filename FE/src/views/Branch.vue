<script setup>
import { ref } from 'vue'
import { MagnifyingGlassIcon, PlusIcon, PencilSquareIcon, TrashIcon, EyeIcon, XMarkIcon, BuildingOfficeIcon } from '@heroicons/vue/24/outline'

// ĐÃ XÓA SẠCH DATA MẪU CHỜ API
const branches = ref([])

const showModal = ref(false)
const modalMode = ref('add') 
const formData = ref({ id: null, code: '', name: '', address: '', manager: '', email: '', phone: '', warehouseCount: 1, status: 'active' })

const openModal = (mode, branch = null) => {
  modalMode.value = mode
  if (branch) formData.value = { ...branch } 
  else formData.value = { id: null, code: '', name: '', address: '', manager: '', email: '', phone: '', warehouseCount: 1, status: 'active' }
  showModal.value = true
}

const closeModal = () => showModal.value = false

const generateAutoCode = () => {
  const count = branches.value.filter(b => b.code.startsWith('CN')).length
  return `CN${(count + 1).toString().padStart(3, '0')}`
}

const handleSubmit = () => {
  if (modalMode.value === 'add') {
    branches.value.push({
      ...formData.value, id: Date.now(),
      code: formData.value.code.trim() !== '' ? formData.value.code : generateAutoCode()
    })
  } else {
    const index = branches.value.findIndex(b => b.id === formData.value.id)
    if (index !== -1) branches.value[index] = { ...formData.value }
  }
  closeModal()
}

const handleDelete = (branchId, branchName) => {
  if (confirm(`Sếp có chắc chắn muốn xóa Chi nhánh "${branchName}" không?`)) {
    branches.value = branches.value.filter(b => b.id !== branchId)
  }
}
</script>

<template>
  <div class="space-y-5 md:space-y-6 animate-fade-in pb-10 px-0 md:px-1 relative">
    <div class="flex flex-col sm:flex-row sm:items-center justify-between gap-4">
      <div>
        <h2 class="text-xl md:text-2xl font-bold text-gray-800">Mạng lưới Chi nhánh</h2>
        <p class="text-xs md:text-sm text-gray-500 mt-1">Quản lý danh sách chi nhánh, thông tin liên hệ và kho trực thuộc</p>
      </div>
      <button @click="openModal('add')" class="bg-primary-600 hover:bg-primary-700 text-white px-4 py-2.5 rounded-lg flex items-center gap-2 text-sm font-semibold transition-colors shadow-sm">
        <PlusIcon class="w-5 h-5" /> Thêm Chi nhánh
      </button>
    </div>

    <div class="bg-white p-3 md:p-4 rounded-xl border border-gray-200 flex flex-col sm:flex-row items-center gap-3 md:gap-4 shadow-sm">
      <div class="relative w-full sm:flex-1 max-w-none sm:max-w-md">
        <div class="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none"><MagnifyingGlassIcon class="w-5 h-5 text-gray-400" /></div>
        <input type="text" class="block w-full pl-10 pr-3 py-2 border border-gray-200 rounded-lg text-sm focus:ring-1 focus:ring-primary-500 outline-none" placeholder="Tìm theo mã CN, tên, email hoặc SĐT...">
      </div>
    </div>

    <div class="bg-white rounded-xl border border-gray-200 shadow-sm overflow-hidden">
      <div class="w-full overflow-x-auto custom-scrollbar">
        <table class="min-w-[1000px] w-full divide-y divide-gray-200">
          <thead class="bg-gray-50">
            <tr>
              <th class="px-5 py-3.5 text-left text-xs font-bold text-gray-500 uppercase tracking-wider">Mã CN</th>
              <th class="px-5 py-3.5 text-left text-xs font-bold text-gray-500 uppercase tracking-wider">Tên Chi nhánh</th>
              <th class="px-5 py-3.5 text-left text-xs font-bold text-gray-500 uppercase tracking-wider">Người Quản lý</th>
              <th class="px-5 py-3.5 text-left text-xs font-bold text-gray-500 uppercase tracking-wider">Liên hệ (Email/SĐT)</th>
              <th class="px-5 py-3.5 text-center text-xs font-bold text-gray-500 uppercase tracking-wider">SL Kho</th>
              <th class="px-5 py-3.5 text-left text-xs font-bold text-gray-500 uppercase tracking-wider">Trạng thái</th>
              <th class="px-5 py-3.5 text-right text-xs font-bold text-gray-500 uppercase tracking-wider">Thao tác</th>
            </tr>
          </thead>
          <tbody class="divide-y divide-gray-100 bg-white">
            <tr v-if="branches.length === 0">
              <td colspan="7" class="px-6 py-16 text-center">
                <BuildingOfficeIcon class="w-12 h-12 text-gray-300 mx-auto mb-3" />
                <h3 class="text-base font-semibold text-gray-700">Chưa có dữ liệu Chi nhánh</h3>
                <p class="text-sm text-gray-500 mt-1">Hệ thống đang chờ dữ liệu từ API.</p>
              </td>
            </tr>
            <tr v-for="branch in branches" :key="branch.id" class="hover:bg-gray-50 transition-colors">
              <td class="px-5 py-3 text-sm font-bold text-primary-700">{{ branch.code }}</td>
              <td class="px-5 py-3 text-sm font-bold text-gray-900">{{ branch.name }}</td>
              <td class="px-5 py-3 text-sm text-gray-700">{{ branch.manager || '—' }}</td>
              <td class="px-5 py-3">
                <div class="flex flex-col"><span class="text-sm text-gray-700">{{ branch.email || '—' }}</span><span class="text-xs text-gray-500">{{ branch.phone || '—' }}</span></div>
              </td>
              <td class="px-5 py-3 text-sm text-center font-bold text-gray-700"><span class="bg-slate-100 text-slate-600 px-2 py-1 rounded">{{ branch.warehouseCount }}</span></td>
              <td class="px-5 py-3">
                <span v-if="branch.status === 'active'" class="text-xs font-bold px-2.5 py-1 rounded bg-green-100 text-green-700">Hoạt động</span>
                <span v-else class="text-xs font-bold px-2.5 py-1 rounded bg-red-100 text-red-700">Đóng cửa</span>
              </td>
              <td class="px-5 py-3 text-right space-x-2 whitespace-nowrap">
                <button @click="openModal('view', branch)" class="p-1.5 text-blue-600 hover:bg-blue-50 rounded" title="Xem chi tiết"><EyeIcon class="w-5 h-5" /></button>
                <button @click="openModal('edit', branch)" class="p-1.5 text-amber-600 hover:bg-amber-50 rounded" title="Chỉnh sửa"><PencilSquareIcon class="w-5 h-5" /></button>
                <button @click="handleDelete(branch.id, branch.name)" class="p-1.5 text-red-600 hover:bg-red-50 rounded" title="Xóa"><TrashIcon class="w-5 h-5" /></button>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>

    <Teleport to="body">
      <div v-if="showModal" class="fixed inset-0 z-50 flex items-center justify-center px-4 bg-slate-900/50 backdrop-blur-sm animate-fade-in">
        <div class="bg-white rounded-xl shadow-2xl w-full max-w-lg overflow-hidden">
          <div class="px-4 md:px-6 py-4 border-b border-gray-100 flex items-center justify-between bg-gray-50/50">
            <h3 class="text-lg font-bold text-gray-800">{{ modalMode === 'add' ? 'Thêm Chi nhánh' : (modalMode === 'edit' ? 'Cập nhật' : 'Chi tiết') }}</h3>
            <button @click="closeModal" class="text-gray-400 hover:text-red-500"><XMarkIcon class="w-6 h-6" /></button>
          </div>
          <div class="p-4 md:p-6 max-h-[80vh] overflow-y-auto">
            <form @submit.prevent="handleSubmit" class="space-y-4">
              <div class="grid grid-cols-1 sm:grid-cols-2 gap-4">
                <div><label class="block text-xs font-bold text-gray-700 mb-1">Mã CN</label><input v-model="formData.code" :disabled="modalMode === 'view'" type="text" class="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm disabled:bg-gray-100"></div>
                <div><label class="block text-xs font-bold text-gray-700 mb-1">Tên Chi nhánh *</label><input v-model="formData.name" :disabled="modalMode === 'view'" required type="text" class="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm disabled:bg-gray-100"></div>
              </div>
              <div><label class="block text-xs font-bold text-gray-700 mb-1">Địa chỉ</label><input v-model="formData.address" :disabled="modalMode === 'view'" type="text" class="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm disabled:bg-gray-100"></div>
              <div class="grid grid-cols-1 sm:grid-cols-2 gap-4">
                <div><label class="block text-xs font-bold text-gray-700 mb-1">Email *</label><input v-model="formData.email" :disabled="modalMode === 'view'" required type="email" class="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm disabled:bg-gray-100"></div>
                <div><label class="block text-xs font-bold text-gray-700 mb-1">SĐT</label><input v-model="formData.phone" :disabled="modalMode === 'view'" type="tel" class="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm disabled:bg-gray-100"></div>
              </div>
              <div class="grid grid-cols-1 sm:grid-cols-2 gap-4">
                <div><label class="block text-xs font-bold text-gray-700 mb-1">Người quản lý</label><input v-model="formData.manager" :disabled="modalMode === 'view'" type="text" class="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm disabled:bg-gray-100"></div>
                <div><label class="block text-xs font-bold text-gray-700 mb-1">Số lượng Kho</label><input v-model.number="formData.warehouseCount" :disabled="modalMode === 'view'" type="number" min="0" class="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm disabled:bg-gray-100"></div>
              </div>
              <div>
                <label class="block text-xs font-bold text-gray-700 mb-1">Trạng thái</label>
                <select v-model="formData.status" :disabled="modalMode === 'view'" class="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm disabled:bg-gray-100">
                  <option value="active">Hoạt động</option><option value="inactive">Đóng cửa</option>
                </select>
              </div>
              <div class="mt-6 pt-4 border-t border-gray-100 flex justify-end gap-3">
                <button type="button" @click="closeModal" class="px-4 py-2 border border-gray-300 rounded-lg text-sm hover:bg-gray-50">{{ modalMode === 'view' ? 'Đóng' : 'Hủy' }}</button>
                <button v-if="modalMode !== 'view'" type="submit" class="px-4 py-2 bg-primary-600 text-white rounded-lg text-sm hover:bg-primary-700">Lưu thay đổi</button>
              </div>
            </form>
          </div>
        </div>
      </div>
    </Teleport>
  </div>
</template>

<style scoped>
.custom-scrollbar::-webkit-scrollbar { height: 6px; }
.custom-scrollbar::-webkit-scrollbar-track { background: transparent; }
.custom-scrollbar::-webkit-scrollbar-thumb { background: #e2e8f0; border-radius: 10px; }
</style>