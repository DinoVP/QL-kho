<script setup>
import { ref, computed, onMounted } from 'vue'
import { MagnifyingGlassIcon, PlusIcon, PencilSquareIcon, TrashIcon, EyeIcon, XMarkIcon, CubeIcon, PhotoIcon, ScaleIcon } from '@heroicons/vue/24/outline'
import { uiLogger } from '@/utils/logger'

const API_URL = 'https://localhost:7139/api/Products'
const CAT_API_URL = 'https://localhost:7139/api/Categories'

const products = ref([])
const isLoading = ref(false)

const categoryList = ref([])
const brandList = ref([])
const unitList = ref([])

const getAuthHeaders = () => ({
  'Content-Type': 'application/json',
  'Authorization': 'Bearer ' + (localStorage.getItem('authToken') || '')
})

const fetchDropdownData = async () => {
  try {
    const res = await fetch(CAT_API_URL, { headers: { 'Authorization': 'Bearer ' + (localStorage.getItem('authToken') || '') } })
    if (res.ok) {
      const data = await res.json()
      categoryList.value = data.filter(i => i.type === 'Nhóm sản phẩm').map(i => i.name)
      brandList.value = data.filter(i => i.type === 'Thương hiệu').map(i => i.name)
      unitList.value = data.filter(i => i.type === 'Đơn vị tính').map(i => i.name)
    }
  } catch (error) { console.error('Lỗi load danh mục:', error) }
}

const fetchProducts = async () => {
  isLoading.value = true
  try {
    const res = await fetch(API_URL, { headers: { 'Authorization': 'Bearer ' + (localStorage.getItem('authToken') || '') } })
    if (res.ok) products.value = await res.json()
  } catch (error) { console.error('Lỗi load sản phẩm:', error) }
  finally { isLoading.value = false }
}

const searchQuery = ref('')
const filterCategory = ref('')
const filterStatus = ref('')

const filteredProducts = computed(() => {
  return products.value.filter(p => {
    const matchSearch = (p.sku || '').toLowerCase().includes(searchQuery.value.toLowerCase()) || 
                        (p.name || '').toLowerCase().includes(searchQuery.value.toLowerCase())
    const matchCategory = filterCategory.value === '' || p.category === filterCategory.value
    const matchStatus = filterStatus.value === '' || p.status === filterStatus.value
    return matchSearch && matchCategory && matchStatus
  })
})

const showModal = ref(false)
const modalMode = ref('add') 
const formData = ref({ 
  id: 0, name: '', category: '', brand: '', unit: '', 
  packSize: '', weight: 0, status: 'active' 
})

const openModal = (mode, product = null) => {
  modalMode.value = mode
  if (product) {
    formData.value = { ...product } 
  } else {
    formData.value = { 
      id: 0, name: '', category: '', brand: '', unit: '', 
      packSize: '', weight: 0, status: 'active' 
    }
  }
  showModal.value = true
}

const closeModal = () => showModal.value = false

const handleSubmit = async () => {
  if (!formData.value.category || !formData.value.brand || !formData.value.unit) {
    alert("Vui lòng chọn đầy đủ Danh mục, Thương hiệu, Đơn vị tính!"); return;
  }

  try {
    const method = modalMode.value === 'add' ? 'POST' : 'PUT'
    const url = modalMode.value === 'add' ? API_URL : `${API_URL}/${formData.value.id}`
    const payload = { ...formData.value, id: formData.value.id || 0 }

    const res = await fetch(url, { method: method, headers: getAuthHeaders(), body: JSON.stringify(payload) })

    if (res.ok) {
      uiLogger.log('API_CALL', '/products', `${modalMode.value === 'add' ? 'Thêm' : 'Sửa'} Sản phẩm: ${formData.value.name}`)
      await fetchProducts()
      closeModal()
    } else {
      const err = await res.json()
      alert('Lỗi: ' + (err.message || 'Hệ thống từ chối do trùng lặp dữ liệu'))
    }
  } catch (error) { alert('Lỗi kết nối máy chủ!') }
}

const handleDelete = async (id, name) => {
  if (confirm(`Sếp có chắc chắn muốn xóa Sản phẩm "${name}" không?`)) {
    try {
      const res = await fetch(`${API_URL}/${id}`, { method: 'DELETE', headers: getAuthHeaders() })
      if (res.ok) {
        uiLogger.log('CLICK', '/products', `Xóa Sản phẩm: ${name}`)
        await fetchProducts()
      } else { alert('Lỗi xóa sản phẩm') }
    } catch (error) { console.error('Lỗi xóa:', error) }
  }
}

onMounted(() => {
  fetchDropdownData()
  fetchProducts()
})
</script>

<template>
  <div class="space-y-5 md:space-y-6 animate-fade-in pb-10 px-0 md:px-1 relative">
    
    <div class="flex flex-col sm:flex-row sm:items-center justify-between gap-4">
      <div>
        <h2 class="text-xl md:text-2xl font-bold text-gray-800">Danh mục Sản phẩm (SKU)</h2>
        <p class="text-xs md:text-sm text-gray-500 mt-1">Quản lý mã hàng, quy cách đóng gói và khối lượng</p>
      </div>
      <div class="flex gap-2">
        <button @click="openModal('add')" class="bg-primary-600 hover:bg-primary-700 text-white px-4 py-2.5 rounded-lg flex items-center gap-2 text-sm font-semibold transition-colors shadow-sm">
          <PlusIcon class="w-5 h-5" /> Thêm Sản Phẩm Mới
        </button>
      </div>
    </div>

    <div class="bg-white p-3 md:p-4 rounded-xl border border-gray-200 flex flex-col lg:flex-row items-center gap-3 md:gap-4 shadow-sm">
      <div class="relative w-full lg:flex-1">
        <div class="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none"><MagnifyingGlassIcon class="w-5 h-5 text-gray-400" /></div>
        <input v-model="searchQuery" type="text" class="block w-full pl-10 pr-3 py-2 border border-gray-200 rounded-lg text-sm outline-none focus:ring-1 focus:ring-primary-500" placeholder="Tìm kiếm theo mã SKU, Tên sản phẩm...">
      </div>
      
      <div class="flex flex-col sm:flex-row gap-3 w-full lg:w-auto">
        <select v-model="filterCategory" class="w-full sm:w-auto border border-gray-200 rounded-lg text-sm px-4 py-2 outline-none cursor-pointer">
          <option value="">Tất cả danh mục</option>
          <option v-for="cat in categoryList" :key="cat" :value="cat">{{ cat }}</option>
        </select>
        <select v-model="filterStatus" class="w-full sm:w-auto border border-gray-200 rounded-lg text-sm px-4 py-2 outline-none cursor-pointer">
          <option value="">Tất cả Trạng thái</option>
          <option value="active">Đang kinh doanh</option>
          <option value="inactive">Ngừng kinh doanh</option>
        </select>
      </div>
    </div>

    <div class="bg-white rounded-xl border border-gray-200 shadow-sm overflow-hidden">
      <div class="w-full overflow-x-auto custom-scrollbar">
        <table class="min-w-[1000px] w-full divide-y divide-gray-200">
          <thead class="bg-gray-50">
            <tr>
              <th class="px-5 py-3.5 text-left text-xs font-bold text-gray-500 uppercase tracking-wider w-16">Ảnh</th>
              <th class="px-5 py-3.5 text-left text-xs font-bold text-gray-500 uppercase tracking-wider">Mã & Tên Sản phẩm</th>
              <th class="px-5 py-3.5 text-left text-xs font-bold text-gray-500 uppercase tracking-wider">Phân loại</th>
              <th class="px-5 py-3.5 text-left text-xs font-bold text-gray-500 uppercase tracking-wider">Quy cách & Cân nặng</th>
              <th class="px-5 py-3.5 text-center text-xs font-bold text-gray-500 uppercase tracking-wider">Trạng thái</th>
              <th class="px-5 py-3.5 text-right text-xs font-bold text-gray-500 uppercase tracking-wider">Thao tác</th>
            </tr>
          </thead>
          
          <tbody class="divide-y divide-gray-100 bg-white">
            <tr v-if="isLoading"><td colspan="6" class="px-6 py-12 text-center text-gray-500">Đang tải sản phẩm...</td></tr>
            <tr v-else-if="filteredProducts.length === 0">
              <td colspan="6" class="px-6 py-16 text-center">
                <CubeIcon class="w-12 h-12 text-gray-300 mx-auto mb-3" />
                <h3 class="text-base font-semibold text-gray-700">Chưa có sản phẩm nào</h3>
              </td>
            </tr>

            <tr v-for="product in filteredProducts" :key="product.id" class="hover:bg-gray-50 transition-colors">
              <td class="px-5 py-3"><PhotoIcon class="w-8 h-8 text-gray-300" /></td>
              <td class="px-5 py-3">
                <div class="flex flex-col">
                  <span class="text-sm font-bold text-primary-700">{{ product.sku }}</span>
                  <span class="text-sm font-bold text-gray-900 mt-0.5">{{ product.name }}</span>
                </div>
              </td>
              <td class="px-5 py-3">
                <div class="flex flex-col gap-1">
                  <span class="text-[10px] bg-gray-100 px-2 py-0.5 rounded text-gray-600 border w-fit">{{ product.category }}</span>
                  <span class="text-[10px] bg-gray-100 px-2 py-0.5 rounded text-gray-600 border w-fit">{{ product.brand }}</span>
                </div>
              </td>
              <td class="px-5 py-3">
                <div class="flex flex-col space-y-1">
                  <div class="flex items-center gap-1 text-xs text-gray-600"><CubeIcon class="w-3.5 h-3.5"/> ĐVT: <strong class="text-indigo-600">{{ product.unit }}</strong></div>
                  <div class="flex items-center gap-1 text-xs text-gray-600"><CubeIcon class="w-3.5 h-3.5"/> Quy cách: {{ product.packSize || 'N/A' }}</div>
                  <div class="flex items-center gap-1 text-xs text-gray-600"><ScaleIcon class="w-3.5 h-3.5"/> Trọng lượng: {{ product.weight || 0 }} kg</div>
                </div>
              </td>
              <td class="px-5 py-3 text-center">
                <span :class="product.status === 'active' ? 'bg-green-100 text-green-700 border-green-200' : 'bg-red-100 text-red-700 border-red-200'" class="text-[10px] font-bold px-2 py-1 rounded border uppercase">
                  {{ product.status === 'active' ? 'Đang KD' : 'Ngừng KD' }}
                </span>
              </td>
              <td class="px-5 py-3 text-right space-x-2">
                <button @click="openModal('edit', product)" class="p-1.5 text-amber-600 hover:bg-amber-50 rounded"><PencilSquareIcon class="w-5 h-5" /></button>
                <button @click="handleDelete(product.id, product.name)" class="p-1.5 text-red-600 hover:bg-red-50 rounded"><TrashIcon class="w-5 h-5" /></button>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>

    <Teleport to="body">
      <div v-if="showModal" class="fixed inset-0 z-50 flex items-center justify-center px-4 bg-slate-900/50 backdrop-blur-sm animate-fade-in">
        <div class="bg-white rounded-xl shadow-2xl w-full max-w-2xl overflow-hidden transform transition-all flex flex-col">
          
          <div class="px-6 py-4 border-b flex items-center justify-between bg-gray-50 shrink-0">
            <h3 class="text-lg font-bold text-gray-800">{{ modalMode === 'add' ? 'Thêm Sản phẩm mới' : 'Cập nhật Sản phẩm' }}</h3>
            <button @click="closeModal" class="text-gray-400 hover:text-red-500"><XMarkIcon class="w-6 h-6" /></button>
          </div>

          <div class="p-6 overflow-y-auto flex-1 custom-scrollbar">
            <form @submit.prevent="handleSubmit" class="space-y-6">
              
              <div class="grid grid-cols-1 md:grid-cols-2 gap-6">
                <div class="space-y-4">
                  <h4 class="text-sm font-bold text-primary-600 border-b pb-2 flex items-center gap-2"><CubeIcon class="w-4 h-4"/> 1. Thông tin Cơ bản</h4>
                  
                  <div>
                    <label class="block text-xs font-bold mb-1">Tên Sản phẩm <span class="text-red-500">*</span></label>
                    <input v-model="formData.name" type="text" required class="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm focus:ring-1 focus:ring-primary-500" placeholder="VD: Nước tăng lực Redbull">
                  </div>

                  <div class="grid grid-cols-2 gap-4">
                    <div>
                      <label class="block text-xs font-bold mb-1">Danh mục <span class="text-red-500">*</span></label>
                      <select v-model="formData.category" required class="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm cursor-pointer">
                        <option value="" disabled selected>-- Chọn --</option>
                        <option v-for="cat in categoryList" :key="cat" :value="cat">{{ cat }}</option>
                      </select>
                    </div>
                    <div>
                      <label class="block text-xs font-bold mb-1">Thương hiệu <span class="text-red-500">*</span></label>
                      <select v-model="formData.brand" required class="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm cursor-pointer">
                        <option value="" disabled selected>-- Chọn --</option>
                        <option v-for="b in brandList" :key="b" :value="b">{{ b }}</option>
                      </select>
                    </div>
                  </div>
                  
                  <div>
                    <label class="block text-xs font-bold mb-1">Trạng thái kinh doanh</label>
                    <select v-model="formData.status" class="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm">
                      <option value="active">Đang kinh doanh</option>
                      <option value="inactive">Ngừng kinh doanh</option>
                    </select>
                  </div>
                </div>

                <div class="space-y-4 bg-slate-50 p-4 rounded-xl border border-slate-200">
                  <h4 class="text-sm font-bold text-indigo-600 border-b border-indigo-100 pb-2 flex items-center gap-2"><ScaleIcon class="w-4 h-4"/> 2. Đóng gói & Tải trọng</h4>
                  
                  <div>
                    <label class="block text-xs font-bold mb-1">Đơn vị tính cơ bản <span class="text-red-500">*</span></label>
                    <select v-model="formData.unit" required class="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm cursor-pointer">
                      <option value="" disabled selected>-- Chọn ĐVT --</option>
                      <option v-for="u in unitList" :key="u" :value="u">{{ u }}</option>
                    </select>
                  </div>

                  <div>
                    <label class="block text-xs font-bold mb-1">Quy cách chuyển đổi</label>
                    <input v-model="formData.packSize" type="text" class="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm" placeholder="VD: 1 Thùng = 24 Lon">
                  </div>

                  <div>
                    <label class="block text-xs font-bold mb-1">Trọng lượng (kg) / Sản phẩm</label>
                    <div class="relative">
                      <input v-model.number="formData.weight" type="number" step="0.01" min="0" class="w-full border border-gray-300 rounded-lg pl-3 pr-8 py-2 text-sm" placeholder="VD: 1.5">
                      <div class="absolute inset-y-0 right-0 pr-3 flex items-center pointer-events-none text-xs text-gray-500 font-bold">kg</div>
                    </div>
                    <p class="text-[10px] text-gray-500 mt-1 italic">Hệ thống dùng số liệu này để tính tải trọng an toàn của Kệ kho</p>
                  </div>
                </div>
              </div>

            </form>
          </div>

          <div class="px-6 py-4 border-t flex justify-end gap-3 shrink-0 bg-white">
            <button type="button" @click="closeModal" class="px-4 py-2 border border-gray-300 rounded-lg text-sm font-semibold hover:bg-gray-50">Hủy bỏ</button>
            <button @click="handleSubmit" class="px-4 py-2 bg-primary-600 text-white rounded-lg text-sm font-semibold hover:bg-primary-700 shadow-sm">Lưu Sản phẩm</button>
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
.custom-scrollbar::-webkit-scrollbar-thumb:hover { background: #94a3b8; }
.animate-fade-in { animation: fadeIn 0.2s ease-out; }
@keyframes fadeIn { from { opacity: 0; transform: translateY(5px); } to { opacity: 1; transform: translateY(0); } }
</style>