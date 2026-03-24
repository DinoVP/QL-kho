<script setup>
import { ref } from 'vue'
import { 
  MagnifyingGlassIcon, PlusIcon, PencilSquareIcon, 
  TrashIcon, EyeIcon, XMarkIcon, UserCircleIcon 
} from '@heroicons/vue/24/outline'

// === 1. STATE: DỮ LIỆU ĐÃ XÓA TRỐNG CHỜ API ===
const users = ref([])

const showModal = ref(false)
const modalMode = ref('add') 
// Bổ sung thêm trường email và phone vào form mặc định
const formData = ref({ 
  id: null, 
  code: '', 
  name: '', 
  username: '', 
  email: '', 
  phone: '', 
  role: 'Nhân viên Kho', 
  status: 'active' 
})

// Mở Modal
const openModal = (mode, user = null) => {
  modalMode.value = mode
  if (user) {
    formData.value = { ...user } 
  } else {
    // Reset form sạch sẽ khi thêm mới
    formData.value = { 
      id: null, code: '', name: '', username: '', 
      email: '', phone: '', role: 'Nhân viên Kho', status: 'active' 
    }
  }
  showModal.value = true
}

// Đóng Modal
const closeModal = () => {
  showModal.value = false
}

// Hàm sinh mã tự động
const generateAutoCode = (role) => {
  let prefix = 'NV' 
  if (role === 'Đội ngũ IT') prefix = 'IT'
  else if (role === 'Giám đốc Chi nhánh') prefix = 'GD'
  else if (role === 'Quản lý Kho') prefix = 'QL'

  const count = users.value.filter(u => u.code.startsWith(prefix)).length
  return `${prefix}${(count + 1).toString().padStart(3, '0')}`
}

// Xử lý Lưu form
const handleSubmit = () => {
  if (modalMode.value === 'add') {
    const newUser = {
      ...formData.value,
      id: Date.now(),
      code: formData.value.code.trim() !== '' ? formData.value.code : generateAutoCode(formData.value.role)
    }
    users.value.push(newUser)
    alert(`Thêm thành công tài khoản: ${newUser.code}`)
  } 
  else if (modalMode.value === 'edit') {
    const index = users.value.findIndex(u => u.id === formData.value.id)
    if (index !== -1) {
      users.value[index] = { ...formData.value }
    }
    alert('Cập nhật thông tin thành công!')
  }
  closeModal()
}

// Xử lý Xóa
const handleDelete = (userId, userName) => {
  const isConfirm = confirm(`Sếp có chắc chắn muốn xóa nhân sự "${userName}" không?`)
  if (isConfirm) {
    users.value = users.value.filter(u => u.id !== userId)
  }
}

// Lấy màu sắc Tag hiển thị cho Role
const getRoleStyle = (role) => {
  switch(role) {
    case 'Đội ngũ IT': return 'bg-purple-100 text-purple-700 border-purple-200'
    case 'Giám đốc Chi nhánh': return 'bg-rose-100 text-rose-700 border-rose-200'
    case 'Quản lý Kho': return 'bg-blue-100 text-blue-700 border-blue-200'
    default: return 'bg-slate-100 text-slate-700 border-slate-200'
  }
}
</script>

<template>
  <div class="space-y-5 md:space-y-6 animate-fade-in pb-10 px-0 md:px-1 relative">
    
    <div class="flex flex-col sm:flex-row sm:items-center justify-between gap-4">
      <div>
        <h2 class="text-xl md:text-2xl font-bold text-gray-800">Quản lý Nhân sự</h2>
        <p class="text-xs md:text-sm text-gray-500 mt-1">Phân quyền, cấp tài khoản và thông tin liên lạc (Email/SĐT)</p>
      </div>
      <button 
        @click="openModal('add')"
        class="bg-primary-600 hover:bg-primary-700 text-white px-4 py-2.5 rounded-lg flex items-center gap-2 text-sm font-semibold transition-colors shadow-sm active:scale-95 justify-center"
      >
        <PlusIcon class="w-5 h-5" /> Thêm nhân sự
      </button>
    </div>

    <div class="bg-white p-3 md:p-4 rounded-xl border border-gray-200 flex flex-col sm:flex-row items-center gap-3 md:gap-4 shadow-sm">
      <div class="relative w-full sm:flex-1 max-w-none sm:max-w-md">
        <div class="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none">
          <MagnifyingGlassIcon class="w-5 h-5 text-gray-400" />
        </div>
        <input type="text" class="block w-full pl-10 pr-3 py-2 border border-gray-200 rounded-lg text-sm focus:ring-1 focus:ring-primary-500 outline-none" placeholder="Tìm theo mã NV, tên, email hoặc SĐT...">
      </div>
    </div>

    <div class="bg-white rounded-xl border border-gray-200 shadow-sm overflow-hidden">
      <div class="w-full overflow-x-auto custom-scrollbar">
        <table class="min-w-[1050px] w-full divide-y divide-gray-200">
          <thead class="bg-gray-50">
            <tr>
              <th class="px-5 py-3.5 text-left text-xs font-bold text-gray-500 uppercase tracking-wider">Mã NV</th>
              <th class="px-5 py-3.5 text-left text-xs font-bold text-gray-500 uppercase tracking-wider">Nhân viên</th>
              <th class="px-5 py-3.5 text-left text-xs font-bold text-gray-500 uppercase tracking-wider">Liên hệ (Email/SĐT)</th>
              <th class="px-5 py-3.5 text-left text-xs font-bold text-gray-500 uppercase tracking-wider">Chức vụ (Vai trò)</th>
              <th class="px-5 py-3.5 text-left text-xs font-bold text-gray-500 uppercase tracking-wider">Trạng thái</th>
              <th class="px-5 py-3.5 text-right text-xs font-bold text-gray-500 uppercase tracking-wider">Thao tác</th>
            </tr>
          </thead>
          
          <tbody class="divide-y divide-gray-100 bg-white">
            <tr v-if="users.length === 0">
              <td colspan="6" class="px-6 py-16 text-center">
                <div class="flex flex-col items-center justify-center">
                  <UserCircleIcon class="w-12 h-12 text-gray-300 mb-3" />
                  <h3 class="text-base font-semibold text-gray-700">Chưa có dữ liệu nhân sự</h3>
                  <p class="text-sm text-gray-500 mt-1">Hệ thống đang chờ dữ liệu. Bấm "Thêm nhân sự" để tạo tài khoản mới.</p>
                </div>
              </td>
            </tr>

            <tr v-for="user in users" :key="user.id" class="hover:bg-gray-50 transition-colors">
              <td class="px-5 py-3 text-sm font-bold text-gray-700">{{ user.code }}</td>
              <td class="px-5 py-3">
                <div class="flex flex-col">
                  <span class="text-sm font-bold text-gray-900">{{ user.name }}</span>
                  <span class="text-xs text-gray-500">@{{ user.username }}</span>
                </div>
              </td>
              <td class="px-5 py-3">
                <div class="flex flex-col">
                  <span class="text-sm text-gray-700">{{ user.email || '—' }}</span>
                  <span class="text-xs text-gray-500">{{ user.phone || '—' }}</span>
                </div>
              </td>
              <td class="px-5 py-3">
                <span :class="['text-xs font-bold px-2.5 py-1 rounded border', getRoleStyle(user.role)]">
                  {{ user.role }}
                </span>
              </td>
              <td class="px-5 py-3">
                <span v-if="user.status === 'active'" class="text-xs font-bold px-2.5 py-1 rounded bg-green-100 text-green-700 border border-green-200">Hoạt động</span>
                <span v-else class="text-xs font-bold px-2.5 py-1 rounded bg-red-100 text-red-700 border border-red-200">Đã khóa</span>
              </td>
              <td class="px-5 py-3 text-right space-x-2 whitespace-nowrap">
                <button @click="openModal('view', user)" class="p-1.5 text-blue-600 hover:bg-blue-50 rounded transition-colors" title="Xem chi tiết"><EyeIcon class="w-5 h-5" /></button>
                <button @click="openModal('edit', user)" class="p-1.5 text-amber-600 hover:bg-amber-50 rounded transition-colors" title="Chỉnh sửa"><PencilSquareIcon class="w-5 h-5" /></button>
                <button @click="handleDelete(user.id, user.name)" class="p-1.5 text-red-600 hover:bg-red-50 rounded transition-colors" title="Xóa"><TrashIcon class="w-5 h-5" /></button>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>

    <Teleport to="body">
      <div v-if="showModal" class="fixed inset-0 z-50 flex items-center justify-center px-4 bg-slate-900/50 backdrop-blur-sm animate-fade-in">
        <div class="bg-white rounded-xl shadow-2xl w-full max-w-lg overflow-hidden transform transition-all">
          
          <div class="px-4 md:px-6 py-4 border-b border-gray-100 flex items-center justify-between bg-gray-50/50">
            <h3 class="text-lg font-bold text-gray-800">
              <span v-if="modalMode === 'add'">Thêm Nhân sự mới</span>
              <span v-else-if="modalMode === 'edit'">Chỉnh sửa Thông tin</span>
              <span v-else>Chi tiết Nhân sự</span>
            </h3>
            <button @click="closeModal" class="text-gray-400 hover:text-red-500 hover:bg-red-50 p-1 rounded-lg transition-colors">
              <XMarkIcon class="w-6 h-6" />
            </button>
          </div>

          <div class="p-4 md:p-6 max-h-[80vh] overflow-y-auto">
            <form @submit.prevent="handleSubmit" class="space-y-4">
              
              <div class="grid grid-cols-1 sm:grid-cols-2 gap-4">
                <div>
                  <label class="block text-xs font-bold text-gray-700 mb-1">Mã NV (Tự động sinh)</label>
                  <input v-model="formData.code" :disabled="modalMode === 'view'" type="text" class="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm focus:ring-1 focus:ring-primary-500 outline-none disabled:bg-gray-100 disabled:text-gray-500" placeholder="VD: Để trống tự tạo">
                </div>
                <div>
                  <label class="block text-xs font-bold text-gray-700 mb-1">Tên đăng nhập <span class="text-red-500">*</span></label>
                  <input v-model="formData.username" :disabled="modalMode === 'view'" type="text" required class="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm focus:ring-1 focus:ring-primary-500 outline-none disabled:bg-gray-100 disabled:text-gray-500">
                </div>
              </div>

              <div>
                <label class="block text-xs font-bold text-gray-700 mb-1">Họ và Tên <span class="text-red-500">*</span></label>
                <input v-model="formData.name" :disabled="modalMode === 'view'" type="text" required class="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm focus:ring-1 focus:ring-primary-500 outline-none disabled:bg-gray-100 disabled:text-gray-500">
              </div>

              <div class="grid grid-cols-1 sm:grid-cols-2 gap-4">
                <div>
                  <label class="block text-xs font-bold text-gray-700 mb-1">Email <span class="text-red-500">*</span></label>
                  <input v-model="formData.email" :disabled="modalMode === 'view'" type="email" required class="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm focus:ring-1 focus:ring-primary-500 outline-none disabled:bg-gray-100 disabled:text-gray-500" placeholder="example@gmail.com">
                </div>
                <div>
                  <label class="block text-xs font-bold text-gray-700 mb-1">Số điện thoại</label>
                  <input v-model="formData.phone" :disabled="modalMode === 'view'" type="tel" class="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm focus:ring-1 focus:ring-primary-500 outline-none disabled:bg-gray-100 disabled:text-gray-500" placeholder="09xxxxxxxxx">
                </div>
              </div>

              <div class="grid grid-cols-1 sm:grid-cols-2 gap-4">
                <div>
                  <label class="block text-xs font-bold text-gray-700 mb-1">Chức vụ (Vai trò)</label>
                  <select v-model="formData.role" :disabled="modalMode === 'view'" class="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm focus:ring-1 focus:ring-primary-500 outline-none disabled:bg-gray-100 disabled:text-gray-500 cursor-pointer">
                    <option value="Đội ngũ IT">Đội ngũ IT (Admin)</option>
                    <option value="Giám đốc Chi nhánh">Giám đốc Chi nhánh</option>
                    <option value="Quản lý Kho">Quản lý Kho</option>
                    <option value="Nhân viên Kho">Nhân viên Kho</option>
                  </select>
                </div>
                <div>
                  <label class="block text-xs font-bold text-gray-700 mb-1">Trạng thái</label>
                  <select v-model="formData.status" :disabled="modalMode === 'view'" class="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm focus:ring-1 focus:ring-primary-500 outline-none disabled:bg-gray-100 disabled:text-gray-500 cursor-pointer">
                    <option value="active">Đang hoạt động</option>
                    <option value="inactive">Đã khóa</option>
                  </select>
                </div>
              </div>

              <div class="mt-6 pt-4 border-t border-gray-100 flex justify-end gap-3">
                <button type="button" @click="closeModal" class="px-4 py-2 border border-gray-300 text-gray-700 rounded-lg text-sm font-semibold hover:bg-gray-50 transition-colors">
                  {{ modalMode === 'view' ? 'Đóng' : 'Hủy bỏ' }}
                </button>
                <button v-if="modalMode !== 'view'" type="submit" class="px-4 py-2 bg-primary-600 text-white rounded-lg text-sm font-semibold hover:bg-primary-700 shadow-sm transition-colors">
                  Lưu thay đổi
                </button>
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
.custom-scrollbar:hover::-webkit-scrollbar-thumb { background: #cbd5e1; }
</style>