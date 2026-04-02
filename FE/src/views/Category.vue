<script setup>
import { ref, computed, onMounted } from 'vue'
import { MagnifyingGlassIcon, PlusIcon, PencilSquareIcon, TrashIcon, ListBulletIcon, XMarkIcon } from '@heroicons/vue/24/outline'
import { uiLogger } from '@/utils/logger' 

const API_URL = 'https://localhost:7139/api/Categories'

const getAuthHeaders = () => ({
  'Content-Type': 'application/json',
  'Authorization': 'Bearer ' + (localStorage.getItem('authToken') || '')
})

const categories = ref([])
const isLoading = ref(false)

const searchQuery = ref('')
const filterType = ref('')

const fetchCategories = async () => {
  isLoading.value = true
  try {
    const res = await fetch(API_URL, { headers: { 'Authorization': 'Bearer ' + (localStorage.getItem('authToken') || '') } })
    if (res.ok) {
      categories.value = await res.json()
    }
  } catch (error) {
    console.error('Lỗi tải danh mục:', error)
  } finally {
    isLoading.value = false
  }
}

const filteredCategories = computed(() => {
  return categories.value.filter(c => {
    const matchSearch = (c.code || '').toLowerCase().includes(searchQuery.value.toLowerCase()) || 
                        (c.name || '').toLowerCase().includes(searchQuery.value.toLowerCase())
    const matchType = filterType.value === '' || c.type === filterType.value
    return matchSearch && matchType
  })
})

const showModal = ref(false)
const modalMode = ref('add') 
const formData = ref({ id: '', code: '', name: '', type: 'Nhóm sản phẩm', desc: '', status: 'active' })

const openModal = (mode, item = null) => {
  modalMode.value = mode
  if (item) formData.value = { ...item }
  else formData.value = { id: '', code: '', name: '', type: 'Nhóm sản phẩm', desc: '', status: 'active' }
  showModal.value = true
}

const closeModal = () => showModal.value = false

const handleSubmit = async () => {
  // SỬA LỖI 400: Ép kiểu Id về string "" thay vì số 0
  const payload = {
    id: formData.value.id || "", 
    code: formData.value.code || "", 
    name: formData.value.name,
    type: formData.value.type,
    desc: formData.value.desc || "", // Gửi chuỗi rỗng nếu desc null
    status: formData.value.status
  }

  try {
    if (modalMode.value === 'add') {
      const res = await fetch(API_URL, {
        method: 'POST',
        headers: getAuthHeaders(),
        body: JSON.stringify(payload)
      })
      if (res.ok) {
        uiLogger.log('API_CALL', '/categories', `Thêm danh mục mới: ${payload.name}`)
        await fetchCategories()
      } else { 
        const err = await res.json()
        alert('Lỗi: ' + err.message) 
      }
    } else {
      const res = await fetch(`${API_URL}/${payload.id}`, {
        method: 'PUT',
        headers: getAuthHeaders(),
        body: JSON.stringify(payload)
      })
      if (res.ok) {
        uiLogger.log('API_CALL', '/categories', `Cập nhật danh mục: ${payload.name}`)
        await fetchCategories()
      } else { 
        const err = await res.json()
        alert('Lỗi: ' + err.message) 
      }
    }
  } catch (error) {
    console.error('Lỗi lưu danh mục:', error)
  } finally {
    closeModal()
  }
}

const handleDelete = async (id, name) => {
  if (confirm(`Xóa danh mục "${name}" có thể làm lỗi các mặt hàng đang dùng. Chắc chắn xóa?`)) {
    try {
      const res = await fetch(`${API_URL}/${id}`, { 
        method: 'DELETE',
        headers: { 'Authorization': 'Bearer ' + (localStorage.getItem('authToken') || '') }
      })
      if (res.ok) {
        uiLogger.log('CLICK', '/categories', `Đã xóa danh mục: ${name}`)
        await fetchCategories()
      } else {
        alert('Không thể xóa! (Danh mục này có thể đang được gán cho sản phẩm)')
      }
    } catch (error) {
      console.error('Lỗi xóa danh mục:', error)
    }
  }
}

onMounted(() => {
  fetchCategories()
})
</script>

<template>
  <div class="space-y-5 md:space-y-6 animate-fade-in pb-10 px-0 md:px-1 relative">
    <div class="flex flex-col sm:flex-row sm:items-center justify-between gap-4">
      <div>
        <h2 class="text-xl md:text-2xl font-bold text-gray-800">Danh mục chung</h2>
        <p class="text-xs md:text-sm text-gray-500 mt-1">Quản lý từ điển hệ thống: Nhóm hàng, Đơn vị tính, Thương hiệu...</p>
      </div>
      <button @click="openModal('add')" class="bg-primary-600 hover:bg-primary-700 text-white px-4 py-2.5 rounded-lg flex items-center gap-2 text-sm font-semibold shadow-sm transition-colors">
        <PlusIcon class="w-5 h-5" /> Thêm Danh mục
      </button>
    </div>

    <div class="bg-white p-3 md:p-4 rounded-xl border border-gray-200 flex flex-col sm:flex-row items-center gap-3 md:gap-4 shadow-sm">
      <div class="relative w-full sm:flex-1">
        <div class="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none"><MagnifyingGlassIcon class="w-5 h-5 text-gray-400" /></div>
        <input v-model="searchQuery" type="text" class="block w-full pl-10 pr-3 py-2 border border-gray-200 rounded-lg text-sm outline-none focus:ring-1 focus:ring-primary-500 transition-all" placeholder="Tìm tên danh mục, mã...">
      </div>
      <select v-model="filterType" class="w-full sm:w-auto border border-gray-200 rounded-lg text-sm px-4 py-2 outline-none cursor-pointer focus:ring-1 focus:ring-primary-500">
        <option value="">Tất cả Loại</option>
        <option>Nhóm sản phẩm</option>
        <option>Đơn vị tính</option>
        <option>Thương hiệu</option>
      </select>
    </div>

    <div class="bg-white rounded-xl border border-gray-200 shadow-sm overflow-hidden">
      <div class="w-full overflow-x-auto custom-scrollbar">
        <table class="min-w-[850px] w-full divide-y divide-gray-200">
          <thead class="bg-gray-50">
            <tr>
              <th class="px-5 py-3.5 text-left text-xs font-bold text-gray-500 uppercase tracking-wider">Mã DM</th>
              <th class="px-5 py-3.5 text-left text-xs font-bold text-gray-500 uppercase tracking-wider">Tên Danh mục</th>
              <th class="px-5 py-3.5 text-left text-xs font-bold text-gray-500 uppercase tracking-wider">Loại</th>
              <th class="px-5 py-3.5 text-left text-xs font-bold text-gray-500 uppercase tracking-wider">Mô tả</th>
              <th class="px-5 py-3.5 text-left text-xs font-bold text-gray-500 uppercase tracking-wider">Trạng thái</th>
              <th class="px-5 py-3.5 text-right text-xs font-bold text-gray-500 uppercase tracking-wider">Thao tác</th>
            </tr>
          </thead>
          <tbody class="divide-y divide-gray-100 bg-white">
            <tr v-if="isLoading">
              <td colspan="6" class="px-6 py-12 text-center text-gray-500">Đang tải dữ liệu...</td>
            </tr>
            <tr v-else-if="filteredCategories.length === 0">
              <td colspan="6" class="px-6 py-16 text-center">
                <ListBulletIcon class="w-12 h-12 text-gray-300 mx-auto mb-3" />
                <h3 class="text-base font-semibold text-gray-700">Chưa có dữ liệu danh mục</h3>
              </td>
            </tr>
            <tr v-else v-for="item in filteredCategories" :key="item.id" class="hover:bg-gray-50 transition-colors">
              <td class="px-5 py-3 text-sm font-bold text-primary-700">{{ item.code }}</td>
              <td class="px-5 py-3 text-sm font-bold">{{ item.name }}</td>
              <td class="px-5 py-3"><span class="text-xs font-medium px-2.5 py-1 rounded bg-slate-100 border text-slate-700">{{ item.type }}</span></td>
              <td class="px-5 py-3 text-sm text-gray-500 truncate max-w-[200px]">{{ item.desc || '—' }}</td>
              <td class="px-5 py-3">
                <span v-if="item.status==='active'" class="text-[11px] font-bold px-2.5 py-1 bg-green-100 text-green-700 rounded-full border border-green-200">Đang dùng</span>
                <span v-else class="text-[11px] font-bold px-2.5 py-1 bg-gray-100 text-gray-500 rounded-full border border-gray-200">Tạm ẩn</span>
              </td>
              <td class="px-5 py-3 text-right">
                <button @click="openModal('edit', item)" class="p-1.5 text-amber-600 hover:bg-amber-50 rounded transition-colors"><PencilSquareIcon class="w-5 h-5" /></button>
                <button @click="handleDelete(item.id, item.name)" class="p-1.5 text-red-600 hover:bg-red-50 rounded transition-colors"><TrashIcon class="w-5 h-5" /></button>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>

    <Teleport to="body">
      <div v-if="showModal" class="fixed inset-0 z-50 flex items-center justify-center px-4 bg-slate-900/50 backdrop-blur-sm animate-fade-in">
        <div class="bg-white rounded-xl shadow-2xl w-full max-w-lg overflow-hidden">
          <div class="px-6 py-4 border-b flex justify-between bg-gray-50">
            <h3 class="text-lg font-bold text-gray-800">{{ modalMode === 'add' ? 'Thêm Danh mục mới' : 'Cập nhật Danh mục' }}</h3>
            <button @click="closeModal" class="text-gray-400 hover:text-red-500 transition-colors"><XMarkIcon class="w-6 h-6"/></button>
          </div>
          <div class="p-6">
            <form @submit.prevent="handleSubmit" class="space-y-4">
              
              <div class="grid grid-cols-1 sm:grid-cols-2 gap-4">
                <div v-if="modalMode === 'edit'">
                  <label class="block text-xs font-bold text-gray-700 mb-1">Mã Danh mục</label>
                  <input v-model="formData.code" disabled class="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm bg-gray-100 text-gray-500 cursor-not-allowed outline-none">
                </div>

                <div :class="modalMode === 'add' ? 'col-span-2' : ''">
                  <label class="block text-xs font-bold text-gray-700 mb-1">Loại *</label>
                  <select v-model="formData.type" :disabled="modalMode === 'edit'" class="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm focus:ring-1 focus:ring-primary-500 outline-none cursor-pointer disabled:bg-gray-100 disabled:cursor-not-allowed">
                    <option>Nhóm sản phẩm</option>
                    <option>Đơn vị tính</option>
                    <option>Thương hiệu</option>
                  </select>
                </div>
              </div>
              
              <div>
                <label class="block text-xs font-bold text-gray-700 mb-1">Tên danh mục *</label>
                <input v-model="formData.name" required class="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm focus:ring-1 focus:ring-primary-500 outline-none">
              </div>
              
              <div>
                <label class="block text-xs font-bold text-gray-700 mb-1">{{ formData.type === 'Đơn vị tính' ? 'Đơn vị quy đổi cơ sở' : 'Mô tả thêm' }}</label>
                <textarea v-model="formData.desc" rows="2" class="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm focus:ring-1 focus:ring-primary-500 outline-none" :placeholder="formData.type === 'Đơn vị tính' ? 'VD: Cái, Hộp, Chai...' : ''"></textarea>
              </div>
              
              <div>
                <label class="block text-xs font-bold text-gray-700 mb-1">Trạng thái sử dụng</label>
                <select v-model="formData.status" class="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm focus:ring-1 focus:ring-primary-500 outline-none cursor-pointer">
                  <option value="active">Đang sử dụng</option>
                  <option value="inactive">Tạm ẩn (Không hiện khi nhập hàng)</option>
                </select>
              </div>
              
              <div class="mt-6 pt-4 border-t flex justify-end gap-3">
                <button type="button" @click="closeModal" class="px-5 py-2 border border-gray-300 rounded-lg text-sm font-medium hover:bg-gray-50 transition-colors">Hủy bỏ</button>
                <button type="submit" class="px-5 py-2 bg-primary-600 text-white rounded-lg text-sm font-bold hover:bg-primary-700 shadow-sm transition-colors">Lưu Danh mục</button>
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
.custom-scrollbar::-webkit-scrollbar-thumb { background: #cbd5e1; border-radius: 10px; }
.custom-scrollbar::-webkit-scrollbar-thumb:hover { background: #94a3b8; }
.animate-fade-in { animation: fadeIn 0.2s ease-out; }
@keyframes fadeIn { from { opacity: 0; transform: translateY(5px); } to { opacity: 1; transform: translateY(0); } }
</style>