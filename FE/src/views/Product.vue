<script setup>
import { ref, computed, onMounted } from 'vue'
import { MagnifyingGlassIcon, PlusIcon, PencilSquareIcon, TrashIcon, EyeIcon, XMarkIcon, CubeIcon, PhotoIcon } from '@heroicons/vue/24/outline'
import { uiLogger } from '@/utils/logger'
import { useAuth } from '@/composables/useAuth'

const { currentUserRole } = useAuth()

const API_URL = 'https://localhost:7139/api/Products'
const CAT_API_URL = 'https://localhost:7139/api/Categories'

// --- STATE QUẢN LÝ DỮ LIỆU ---
const products = ref([])
const isLoading = ref(false)

// Data lấy từ bên trang Danh mục qua
const categoryList = ref([])
const brandList = ref([])
const unitList = ref([])

const getAuthHeaders = () => ({
  'Content-Type': 'application/json',
  'Authorization': 'Bearer ' + (localStorage.getItem('authToken') || '')
})

// --- 1. LẤY DỮ LIỆU TỪ TRANG DANH MỤC ---
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

// --- 2. LẤY DANH SÁCH SẢN PHẨM ---
const fetchProducts = async () => {
  isLoading.value = true
  try {
    const res = await fetch(API_URL, { headers: { 'Authorization': 'Bearer ' + (localStorage.getItem('authToken') || '') } })
    if (res.ok) products.value = await res.json()
  } catch (error) { console.error('Lỗi load sản phẩm:', error) }
  finally { isLoading.value = false }
}

// --- BỘ LỌC TÌM KIẾM ---
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

// --- MODAL & FORM ---
const showModal = ref(false)
const modalMode = ref('add') 
const formData = ref({ 
  id: 0, sku: '', name: '', category: '', brand: '', unit: '', 
  minStock: 10, maxStock: 100, price: 0, desc: '', status: 'active' 
})

const openModal = (mode, product = null) => {
  modalMode.value = mode
  if (product) {
    formData.value = { ...product } 
  } else {
    // Để trống toàn bộ dropdown để ép người dùng phải chọn
    formData.value = { 
      id: 0, sku: '', name: '', 
      category: '', 
      brand: '', 
      unit: '', 
      minStock: 10, maxStock: 100, price: 0, desc: '', status: 'active' 
    }
  }
  showModal.value = true
}

const closeModal = () => showModal.value = false

const handleSubmit = async () => {
  // KIỂM TRA RÀNG BUỘC (Bắt buộc chọn Danh mục, Thương hiệu, ĐVT)
  if (!formData.value.category || !formData.value.brand || !formData.value.unit) {
    alert("Sếp vui lòng chọn đầy đủ Danh mục, Thương hiệu và Đơn vị tính nhé!");
    return;
  }

  try {
    const method = modalMode.value === 'add' ? 'POST' : 'PUT'
    const url = modalMode.value === 'add' ? API_URL : `${API_URL}/${formData.value.id}`
    
    // Ép kiểu chuẩn trước khi gửi xuống C# để không bị lỗi 400
    const payload = {
      ...formData.value,
      id: formData.value.id || 0,
      price: formData.value.price || 0,
      desc: formData.value.desc || ""
    }

    const res = await fetch(url, {
      method: method,
      headers: getAuthHeaders(),
      body: JSON.stringify(payload)
    })

    if (res.ok) {
      uiLogger.log('API_CALL', '/products', `${modalMode.value === 'add' ? 'Thêm' : 'Sửa'} SKU: ${formData.value.name}`)
      await fetchProducts()
      closeModal()
    } else {
      const err = await res.json()
      // Bắt lỗi chi tiết của C# nếu có
      if (err.errors) {
        alert('Dữ liệu không hợp lệ: Vui lòng kiểm tra lại các ô nhập liệu.')
      } else {
        alert('Lỗi: ' + (err.message || 'Không xác định'))
      }
    }
  } catch (error) { 
    console.error('Lỗi lưu:', error) 
    alert('Lỗi kết nối máy chủ!')
  }
}

const handleDelete = async (id, name) => {
  if (confirm(`Sếp có chắc chắn muốn xóa SKU "${name}" không?`)) {
    try {
      const res = await fetch(`${API_URL}/${id}`, { 
        method: 'DELETE', 
        headers: { 'Authorization': 'Bearer ' + (localStorage.getItem('authToken') || '') } 
      })
      if (res.ok) {
        uiLogger.log('CLICK', '/products', `Xóa SKU: ${name}`)
        await fetchProducts()
      } else {
        const err = await res.json()
        alert('Lỗi xóa: ' + (err.message || 'Không thể xóa sản phẩm này'))
      }
    } catch (error) { console.error('Lỗi xóa:', error) }
  }
}

const formatCurrency = (value) => new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(value)

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
        <p class="text-xs md:text-sm text-gray-500 mt-1">Quản lý mã hàng, định mức tồn kho và giá vốn</p>
      </div>
      <div class="flex gap-2">
        <button @click="openModal('add')" class="bg-primary-600 hover:bg-primary-700 text-white px-4 py-2.5 rounded-lg flex items-center gap-2 text-sm font-semibold transition-colors shadow-sm">
          <PlusIcon class="w-5 h-5" /> Thêm SKU
        </button>
      </div>
    </div>

    <div class="bg-white p-3 md:p-4 rounded-xl border border-gray-200 flex flex-col lg:flex-row items-center gap-3 md:gap-4 shadow-sm">
      <div class="relative w-full lg:flex-1">
        <div class="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none"><MagnifyingGlassIcon class="w-5 h-5 text-gray-400" /></div>
        <input v-model="searchQuery" type="text" class="block w-full pl-10 pr-3 py-2 border border-gray-200 rounded-lg text-sm outline-none focus:ring-1 focus:ring-primary-500" placeholder="Tìm kiếm theo mã SKU, tên sản phẩm...">
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
        <table class="min-w-[1100px] w-full divide-y divide-gray-200">
          <thead class="bg-gray-50">
            <tr>
              <th class="px-5 py-3.5 text-left text-xs font-bold text-gray-500 uppercase tracking-wider w-16">Ảnh</th>
              <th class="px-5 py-3.5 text-left text-xs font-bold text-gray-500 uppercase tracking-wider">Mã SKU</th>
              <th class="px-5 py-3.5 text-left text-xs font-bold text-gray-500 uppercase tracking-wider">Thông tin Sản phẩm</th>
              <th class="px-5 py-3.5 text-center text-xs font-bold text-gray-500 uppercase tracking-wider">ĐVT</th>
              <th class="px-5 py-3.5 text-right text-xs font-bold text-gray-500 uppercase tracking-wider">Giá tham khảo</th>
              <th class="px-5 py-3.5 text-left text-xs font-bold text-gray-500 uppercase tracking-wider">Trạng thái</th>
              <th class="px-5 py-3.5 text-right text-xs font-bold text-gray-500 uppercase tracking-wider">Thao tác</th>
            </tr>
          </thead>
          
          <tbody class="divide-y divide-gray-100 bg-white">
            <tr v-if="isLoading"><td colspan="7" class="px-6 py-12 text-center text-gray-500">Đang tải sản phẩm...</td></tr>
            <tr v-else-if="filteredProducts.length === 0">
              <td colspan="7" class="px-6 py-16 text-center">
                <CubeIcon class="w-12 h-12 text-gray-300 mx-auto mb-3" />
                <h3 class="text-base font-semibold text-gray-700">Chưa có sản phẩm nào</h3>
              </td>
            </tr>

            <tr v-for="product in filteredProducts" :key="product.id" class="hover:bg-gray-50 transition-colors">
              <td class="px-5 py-3"><PhotoIcon class="w-8 h-8 text-gray-300" /></td>
              <td class="px-5 py-3 text-sm font-bold text-primary-700">{{ product.sku }}</td>
              <td class="px-5 py-3">
                <div class="flex flex-col">
                  <span class="text-sm font-bold text-gray-900">{{ product.name }}</span>
                  <span class="text-xs text-gray-500">{{ product.category }} | {{ product.brand }}</span>
                </div>
              </td>
              <td class="px-5 py-3 text-sm text-center font-medium text-gray-700">{{ product.unit }}</td>
              <td class="px-5 py-3 text-sm font-medium text-gray-900 text-right">{{ formatCurrency(product.price) }}</td>
              <td class="px-5 py-3">
                <span :class="product.status === 'active' ? 'bg-green-100 text-green-700' : 'bg-red-100 text-red-700'" class="text-[10px] font-bold px-2 py-1 rounded uppercase">
                  {{ product.status === 'active' ? 'Đang kinh doanh' : 'Ngừng bán' }}
                </span>
              </td>
              <td class="px-5 py-3 text-right space-x-2">
                <button @click="openModal('view', product)" class="p-1.5 text-blue-600 hover:bg-blue-50 rounded"><EyeIcon class="w-5 h-5" /></button>
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
        <div class="bg-white rounded-xl shadow-2xl w-full max-w-2xl overflow-hidden transform transition-all">
          <div class="px-4 md:px-6 py-4 border-b flex items-center justify-between bg-gray-50">
            <h3 class="text-lg font-bold text-gray-800">{{ modalMode === 'add' ? 'Thêm SKU mới' : 'Chi tiết Sản phẩm' }}</h3>
            <button @click="closeModal" class="text-gray-400 hover:text-red-500"><XMarkIcon class="w-6 h-6" /></button>
          </div>

          <div class="p-4 md:p-6 max-h-[75vh] overflow-y-auto">
            <form @submit.prevent="handleSubmit" class="space-y-6">
              <div>
                <h4 class="text-sm font-bold text-primary-600 mb-3 border-b pb-2">1. Thông kho & Vị trí</h4>
                <div class="grid grid-cols-1 sm:grid-cols-2 gap-4">
                  <div v-if="modalMode !== 'add'">
                    <label class="block text-xs font-bold mb-1">Mã SKU</label>
                    <input v-model="formData.sku" disabled type="text" class="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm bg-gray-100 text-gray-500 cursor-not-allowed outline-none">
                  </div>
                  <div :class="modalMode === 'add' ? 'col-span-2' : ''">
                    <label class="block text-xs font-bold mb-1">Tên Sản phẩm *</label>
                    <input v-model="formData.name" :disabled="modalMode === 'view'" type="text" required class="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm focus:ring-1 focus:ring-primary-500 outline-none">
                  </div>
                </div>

                <div class="grid grid-cols-1 sm:grid-cols-3 gap-4 mt-4">
                  <div>
                    <label class="block text-xs font-bold mb-1">Danh mục *</label>
                    <select v-model="formData.category" required :disabled="modalMode === 'view'" class="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm cursor-pointer disabled:bg-gray-100">
                      <option value="" disabled selected>-- Chọn Danh mục --</option>
                      <option v-for="cat in categoryList" :key="cat" :value="cat">{{ cat }}</option>
                    </select>
                  </div>
                  <div>
                    <label class="block text-xs font-bold mb-1">Thương hiệu *</label>
                    <select v-model="formData.brand" required :disabled="modalMode === 'view'" class="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm cursor-pointer disabled:bg-gray-100">
                      <option value="" disabled selected>-- Chọn Thương hiệu --</option>
                      <option v-for="b in brandList" :key="b" :value="b">{{ b }}</option>
                    </select>
                  </div>
                  <div>
                    <label class="block text-xs font-bold mb-1">Đơn vị tính *</label>
                    <select v-model="formData.unit" required :disabled="modalMode === 'view'" class="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm cursor-pointer disabled:bg-gray-100">
                      <option value="" disabled selected>-- Chọn ĐVT --</option>
                      <option v-for="u in unitList" :key="u" :value="u">{{ u }}</option>
                    </select>
                  </div>
                </div>
              </div>

              <div>
                <h4 class="text-sm font-bold text-primary-600 mb-3 border-b pb-2">2. Đơn giá & Ghi chú</h4>
                <div class="grid grid-cols-1 sm:grid-cols-2 gap-4">
                  <div>
                    <label class="block text-xs font-bold mb-1">Giá tham khảo (VNĐ)</label>
                    <input v-model.number="formData.price" 
                           :disabled="modalMode === 'view' || currentUserRole === 'ql_kho' || currentUserRole === 'nv_kho'" 
                           type="number" 
                           class="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm focus:ring-1 focus:ring-primary-500 outline-none disabled:bg-gray-100 disabled:cursor-not-allowed">
                    <p v-if="currentUserRole === 'ql_kho' || currentUserRole === 'nv_kho'" class="text-[10px] text-red-500 mt-1 italic">Tài khoản Kho không có quyền sửa giá</p>
                  </div>
                  <div>
                    <label class="block text-xs font-bold mb-1">Trạng thái kinh doanh</label>
                    <select v-model="formData.status" :disabled="modalMode === 'view'" class="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm disabled:bg-gray-100">
                      <option value="active">Đang kinh doanh</option>
                      <option value="inactive">Ngừng kinh doanh</option>
                    </select>
                  </div>
                </div>
                <div class="mt-4">
                    <label class="block text-xs font-bold mb-1">Mô tả sản phẩm</label>
                    <textarea v-model="formData.desc" :disabled="modalMode === 'view'" rows="2" class="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm disabled:bg-gray-100"></textarea>
                </div>
              </div>

              <div class="mt-8 pt-4 border-t flex justify-end gap-3">
                <button type="button" @click="closeModal" class="px-4 py-2 border border-gray-300 rounded-lg text-sm font-semibold hover:bg-gray-50">{{ modalMode === 'view' ? 'Đóng' : 'Hủy bỏ' }}</button>
                <button v-if="modalMode !== 'view'" type="submit" class="px-4 py-2 bg-primary-600 text-white rounded-lg text-sm font-semibold hover:bg-primary-700">Lưu Sản phẩm</button>
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