<script setup>
import { ref } from 'vue'
import { MagnifyingGlassIcon, PlusIcon, PencilSquareIcon, TrashIcon, ListBulletIcon, XMarkIcon } from '@heroicons/vue/24/outline'

// ĐÃ XÓA SẠCH DATA MẪU CHỜ API
const categories = ref([])

const showModal = ref(false)
const modalMode = ref('add') 
const formData = ref({ id: null, code: '', name: '', type: 'Nhóm sản phẩm', desc: '', status: 'active' })

const openModal = (mode, item = null) => {
  modalMode.value = mode
  if (item) formData.value = { ...item }
  else formData.value = { id: null, code: '', name: '', type: 'Nhóm sản phẩm', desc: '', status: 'active' }
  showModal.value = true
}

const closeModal = () => showModal.value = false
const generateAutoCode = () => `DM${(categories.value.length + 1).toString().padStart(3, '0')}`

const handleSubmit = () => {
  if (modalMode.value === 'add') {
    categories.value.push({ ...formData.value, id: Date.now(), code: formData.value.code || generateAutoCode() })
  } else {
    const idx = categories.value.findIndex(c => c.id === formData.value.id)
    if (idx !== -1) categories.value[idx] = { ...formData.value }
  }
  closeModal()
}

const handleDelete = (id, name) => {
  if (confirm(`Xóa danh mục "${name}" có thể làm lỗi các mặt hàng đang dùng. Chắc chắn xóa?`)) {
    categories.value = categories.value.filter(c => c.id !== id)
  }
}
</script>

<template>
  <div class="space-y-5 md:space-y-6 animate-fade-in pb-10 px-0 md:px-1 relative">
    <div class="flex flex-col sm:flex-row sm:items-center justify-between gap-4">
      <div>
        <h2 class="text-xl md:text-2xl font-bold text-gray-800">Danh mục chung</h2>
        <p class="text-xs md:text-sm text-gray-500 mt-1">Quản lý từ điển hệ thống: Nhóm hàng, Đơn vị tính, Thương hiệu...</p>
      </div>
      <button @click="openModal('add')" class="bg-primary-600 hover:bg-primary-700 text-white px-4 py-2.5 rounded-lg flex items-center gap-2 text-sm font-semibold shadow-sm">
        <PlusIcon class="w-5 h-5" /> Thêm Danh mục
      </button>
    </div>

    <div class="bg-white p-3 md:p-4 rounded-xl border border-gray-200 flex flex-col sm:flex-row items-center gap-3 md:gap-4 shadow-sm">
      <div class="relative w-full sm:flex-1">
        <div class="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none"><MagnifyingGlassIcon class="w-5 h-5 text-gray-400" /></div>
        <input type="text" class="block w-full pl-10 pr-3 py-2 border border-gray-200 rounded-lg text-sm outline-none" placeholder="Tìm tên danh mục, mã...">
      </div>
      <select class="w-full sm:w-auto border border-gray-200 rounded-lg text-sm px-4 py-2 outline-none">
        <option value="">Tất cả Loại</option><option>Nhóm sản phẩm</option><option>Đơn vị tính</option><option>Thương hiệu</option>
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
            <tr v-if="categories.length === 0">
              <td colspan="6" class="px-6 py-16 text-center">
                <ListBulletIcon class="w-12 h-12 text-gray-300 mx-auto mb-3" />
                <h3 class="text-base font-semibold text-gray-700">Chưa có dữ liệu danh mục</h3>
              </td>
            </tr>
            <tr v-for="item in categories" :key="item.id" class="hover:bg-gray-50">
              <td class="px-5 py-3 text-sm font-bold text-primary-700">{{ item.code }}</td>
              <td class="px-5 py-3 text-sm font-bold">{{ item.name }}</td>
              <td class="px-5 py-3"><span class="text-xs px-2.5 py-1 rounded bg-slate-100 border">{{ item.type }}</span></td>
              <td class="px-5 py-3 text-sm text-gray-500 truncate max-w-[200px]">{{ item.desc || '—' }}</td>
              <td class="px-5 py-3"><span v-if="item.status==='active'" class="text-xs font-bold px-2 py-1 bg-green-100 text-green-700 rounded">Đang dùng</span><span v-else class="text-xs font-bold px-2 py-1 bg-gray-100 text-gray-500 rounded">Tạm ẩn</span></td>
              <td class="px-5 py-3 text-right">
                <button @click="openModal('edit', item)" class="p-1.5 text-amber-600 hover:bg-amber-50 rounded"><PencilSquareIcon class="w-5 h-5" /></button>
                <button @click="handleDelete(item.id, item.name)" class="p-1.5 text-red-600 hover:bg-red-50 rounded"><TrashIcon class="w-5 h-5" /></button>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>

    <Teleport to="body">
      <div v-if="showModal" class="fixed inset-0 z-50 flex items-center justify-center px-4 bg-slate-900/50 backdrop-blur-sm animate-fade-in">
        <div class="bg-white rounded-xl shadow-2xl w-full max-w-lg overflow-hidden">
          <div class="px-6 py-4 border-b flex justify-between bg-gray-50"><h3 class="text-lg font-bold">{{ modalMode === 'add' ? 'Thêm' : 'Sửa' }} Danh mục</h3><button @click="closeModal" class="text-gray-400"><XMarkIcon class="w-6 h-6"/></button></div>
          <div class="p-6">
            <form @submit.prevent="handleSubmit" class="space-y-4">
              <div class="grid grid-cols-2 gap-4">
                <div><label class="block text-xs font-bold mb-1">Mã</label><input v-model="formData.code" class="w-full border rounded-lg px-3 py-2 text-sm"></div>
                <div><label class="block text-xs font-bold mb-1">Loại *</label>
                  <select v-model="formData.type" class="w-full border rounded-lg px-3 py-2 text-sm">
                    <option>Nhóm sản phẩm</option><option>Đơn vị tính</option><option>Thương hiệu</option>
                  </select>
                </div>
              </div>
              <div><label class="block text-xs font-bold mb-1">Tên *</label><input v-model="formData.name" required class="w-full border rounded-lg px-3 py-2 text-sm"></div>
              <div><label class="block text-xs font-bold mb-1">Mô tả</label><textarea v-model="formData.desc" rows="2" class="w-full border rounded-lg px-3 py-2 text-sm"></textarea></div>
              <div><label class="block text-xs font-bold mb-1">Trạng thái</label>
                <select v-model="formData.status" class="w-full border rounded-lg px-3 py-2 text-sm"><option value="active">Đang dùng</option><option value="inactive">Tạm ẩn</option></select>
              </div>
              <div class="mt-4 flex justify-end gap-3"><button type="button" @click="closeModal" class="px-4 py-2 border rounded-lg text-sm">Hủy</button><button type="submit" class="px-4 py-2 bg-primary-600 text-white rounded-lg text-sm">Lưu</button></div>
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