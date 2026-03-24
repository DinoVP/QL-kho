<script setup>
import { ref, computed } from 'vue'
import { MagnifyingGlassIcon, PlusIcon, PencilSquareIcon, TrashIcon, EyeIcon, XMarkIcon, CubeIcon, PhotoIcon } from '@heroicons/vue/24/outline'

// === 1. STATE: DỮ LIỆU ĐÃ XÓA TRỐNG CHỜ API ===
const products = ref([])

// === 2. STATE: BỘ LỌC (TÌM KIẾM) ===
const searchQuery = ref('')
const filterCategory = ref('')
const filterStatus = ref('')

// === 3. LOGIC LỌC DỮ LIỆU THÔNG MINH ===
const filteredProducts = computed(() => {
  return products.value.filter(p => {
    // Lọc theo từ khóa (Mã SKU hoặc Tên)
    const matchSearch = p.sku.toLowerCase().includes(searchQuery.value.toLowerCase()) || 
                        p.name.toLowerCase().includes(searchQuery.value.toLowerCase())
    // Lọc theo Danh mục
    const matchCategory = filterCategory.value === '' || p.category === filterCategory.value
    // Lọc theo Trạng thái
    const matchStatus = filterStatus.value === '' || p.status === filterStatus.value
    
    return matchSearch && matchCategory && matchStatus
  })
})

// === 4. LOGIC MODAL & CRUD ===
const showModal = ref(false)
const modalMode = ref('add') 
const formData = ref({ 
  id: null, sku: '', name: '', category: 'Hàng Điện tử', brand: '', unit: 'Cái', 
  minStock: 0, maxStock: 0, price: 0, desc: '', status: 'active' 
})

const openModal = (mode, product = null) => {
  modalMode.value = mode
  if (product) {
    formData.value = { ...product } 
  } else {
    formData.value = { 
      id: null, sku: '', name: '', category: 'Hàng Điện tử', brand: '', unit: 'Cái', 
      minStock: 10, maxStock: 100, price: 0, desc: '', status: 'active' 
    }
  }
  showModal.value = true
}

const closeModal = () => showModal.value = false

const generateSKU = () => {
  const count = products.value.length
  return `SKU-${(count + 1).toString().padStart(3, '0')}`
}

const handleSubmit = () => {
  if (modalMode.value === 'add') {
    products.value.push({
      ...formData.value,
      id: Date.now(),
      sku: formData.value.sku.trim() !== '' ? formData.value.sku : generateSKU()
    })
    alert('Thêm sản phẩm thành công!')
  } else if (modalMode.value === 'edit') {
    const index = products.value.findIndex(p => p.id === formData.value.id)
    if (index !== -1) products.value[index] = { ...formData.value }
    alert('Cập nhật sản phẩm thành công!')
  }
  closeModal()
}

const handleDelete = (id, name) => {
  if (confirm(`Sếp có chắc chắn muốn xóa SKU "${name}" không?`)) {
    products.value = products.value.filter(p => p.id !== id)
  }
}

const formatCurrency = (value) => {
  return new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(value)
}
</script>

<template>
  <div class="space-y-5 md:space-y-6 animate-fade-in pb-10 px-0 md:px-1 relative">
    
    <div class="flex flex-col sm:flex-row sm:items-center justify-between gap-4">
      <div>
        <h2 class="text-xl md:text-2xl font-bold text-gray-800">Danh mục Sản phẩm (SKU)</h2>
        <p class="text-xs md:text-sm text-gray-500 mt-1">Quản lý mã hàng, định mức tồn kho và giá vốn</p>
      </div>
      <div class="flex gap-2">
        <button class="bg-white border border-gray-300 text-gray-700 px-4 py-2.5 rounded-lg text-sm font-semibold hover:bg-gray-50 transition-colors shadow-sm">
          Nhập Excel
        </button>
        <button @click="openModal('add')" class="bg-primary-600 hover:bg-primary-700 text-white px-4 py-2.5 rounded-lg flex items-center gap-2 text-sm font-semibold transition-colors shadow-sm">
          <PlusIcon class="w-5 h-5" /> Thêm SKU
        </button>
      </div>
    </div>

    <div class="bg-white p-3 md:p-4 rounded-xl border border-gray-200 flex flex-col lg:flex-row items-center gap-3 md:gap-4 shadow-sm">
      <div class="relative w-full lg:flex-1">
        <div class="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none">
          <MagnifyingGlassIcon class="w-5 h-5 text-gray-400" />
        </div>
        <input v-model="searchQuery" type="text" class="block w-full pl-10 pr-3 py-2 border border-gray-200 rounded-lg text-sm focus:ring-1 focus:ring-primary-500 outline-none" placeholder="Tìm kiếm theo mã SKU, tên sản phẩm...">
      </div>
      
      <div class="flex flex-col sm:flex-row gap-3 w-full lg:w-auto">
        <select v-model="filterCategory" class="w-full sm:w-auto border border-gray-200 rounded-lg text-sm px-4 py-2 outline-none focus:ring-1 focus:ring-primary-500 cursor-pointer">
          <option value="">Tất cả danh mục</option>
          <option value="Hàng Điện tử">Hàng Điện tử</option>
          <option value="Hàng Tiêu dùng">Hàng Tiêu dùng</option>
        </select>
        <select v-model="filterStatus" class="w-full sm:w-auto border border-gray-200 rounded-lg text-sm px-4 py-2 outline-none focus:ring-1 focus:ring-primary-500 cursor-pointer">
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
              <th class="px-5 py-3.5 text-right text-xs font-bold text-gray-500 uppercase tracking-wider">Định mức (Min-Max)</th>
              <th class="px-5 py-3.5 text-right text-xs font-bold text-gray-500 uppercase tracking-wider">Giá tham khảo</th>
              <th class="px-5 py-3.5 text-left text-xs font-bold text-gray-500 uppercase tracking-wider">Trạng thái</th>
              <th class="px-5 py-3.5 text-right text-xs font-bold text-gray-500 uppercase tracking-wider">Thao tác</th>
            </tr>
          </thead>
          
          <tbody class="divide-y divide-gray-100 bg-white">
            <tr v-if="products.length === 0">
              <td colspan="8" class="px-6 py-16 text-center">
                <CubeIcon class="w-12 h-12 text-gray-300 mx-auto mb-3" />
                <h3 class="text-base font-semibold text-gray-700">Kho chưa có sản phẩm nào</h3>
                <p class="text-sm text-gray-500 mt-1">Bấm "Thêm SKU" hoặc đợi đồng bộ dữ liệu.</p>
              </td>
            </tr>
            <tr v-else-if="filteredProducts.length === 0">
              <td colspan="8" class="px-6 py-12 text-center">
                <p class="text-sm font-medium text-gray-500">Không tìm thấy sản phẩm nào phù hợp với bộ lọc hiện tại.</p>
              </td>
            </tr>

            <tr v-for="product in filteredProducts" :key="product.id" class="hover:bg-gray-50 transition-colors">
              <td class="px-5 py-3">
                <div class="w-10 h-10 rounded border border-gray-200 bg-gray-50 flex items-center justify-center text-gray-400">
                  <PhotoIcon class="w-5 h-5" />
                </div>
              </td>
              <td class="px-5 py-3 text-sm font-bold text-primary-700">{{ product.sku }}</td>
              <td class="px-5 py-3">
                <div class="flex flex-col">
                  <span class="text-sm font-bold text-gray-900 truncate max-w-[250px]" :title="product.name">{{ product.name }}</span>
                  <span class="text-xs text-gray-500">{{ product.category }} | {{ product.brand }}</span>
                </div>
              </td>
              <td class="px-5 py-3 text-sm text-center font-medium text-gray-700">{{ product.unit }}</td>
              <td class="px-5 py-3 text-right">
                <div class="text-sm font-medium text-gray-700">
                  <span class="text-orange-600">{{ product.minStock }}</span> - <span class="text-blue-600">{{ product.maxStock }}</span>
                </div>
              </td>
              <td class="px-5 py-3 text-sm font-medium text-gray-900 text-right">{{ formatCurrency(product.price) }}</td>
              <td class="px-5 py-3">
                <span v-if="product.status === 'active'" class="text-[10px] font-bold px-2 py-1 rounded bg-green-100 text-green-700 uppercase">Đang kinh doanh</span>
                <span v-else class="text-[10px] font-bold px-2 py-1 rounded bg-red-100 text-red-700 uppercase">Ngừng bán</span>
              </td>
              <td class="px-5 py-3 text-right space-x-2 whitespace-nowrap">
                <button @click="openModal('view', product)" class="p-1.5 text-blue-600 hover:bg-blue-50 rounded" title="Xem chi tiết"><EyeIcon class="w-5 h-5" /></button>
                <button @click="openModal('edit', product)" class="p-1.5 text-amber-600 hover:bg-amber-50 rounded" title="Chỉnh sửa"><PencilSquareIcon class="w-5 h-5" /></button>
                <button @click="handleDelete(product.id, product.name)" class="p-1.5 text-red-600 hover:bg-red-50 rounded" title="Xóa"><TrashIcon class="w-5 h-5" /></button>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>

    <Teleport to="body">
      <div v-if="showModal" class="fixed inset-0 z-50 flex items-center justify-center px-4 bg-slate-900/50 backdrop-blur-sm animate-fade-in">
        <div class="bg-white rounded-xl shadow-2xl w-full max-w-2xl overflow-hidden transform transition-all">
          
          <div class="px-4 md:px-6 py-4 border-b border-gray-100 flex items-center justify-between bg-gray-50">
            <h3 class="text-lg font-bold text-gray-800">
              <span v-if="modalMode === 'add'">Thêm SKU mới</span>
              <span v-else-if="modalMode === 'edit'">Cập nhật Sản phẩm</span>
              <span v-else>Chi tiết Sản phẩm</span>
            </h3>
            <button @click="closeModal" class="text-gray-400 hover:text-red-500 p-1"><XMarkIcon class="w-6 h-6" /></button>
          </div>

          <div class="p-4 md:p-6 max-h-[75vh] overflow-y-auto custom-scrollbar">
            <form @submit.prevent="handleSubmit" class="space-y-6">
              
              <div>
                <h4 class="text-sm font-bold text-primary-600 mb-3 border-b border-gray-100 pb-2">1. Thông tin cơ bản</h4>
                <div class="grid grid-cols-1 sm:grid-cols-2 gap-4">
                  <div>
                    <label class="block text-xs font-bold text-gray-700 mb-1">Mã SKU</label>
                    <input v-model="formData.sku" :disabled="modalMode === 'view'" type="text" class="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm disabled:bg-gray-100" placeholder="VD: SKU-001 (Để trống tự sinh)">
                  </div>
                  <div>
                    <label class="block text-xs font-bold text-gray-700 mb-1">Tên Sản phẩm <span class="text-red-500">*</span></label>
                    <input v-model="formData.name" :disabled="modalMode === 'view'" type="text" required class="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm disabled:bg-gray-100">
                  </div>
                </div>

                <div class="grid grid-cols-1 sm:grid-cols-3 gap-4 mt-4">
                  <div>
                    <label class="block text-xs font-bold text-gray-700 mb-1">Danh mục</label>
                    <select v-model="formData.category" :disabled="modalMode === 'view'" class="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm disabled:bg-gray-100">
                      <option>Hàng Điện tử</option><option>Hàng Tiêu dùng</option>
                    </select>
                  </div>
                  <div>
                    <label class="block text-xs font-bold text-gray-700 mb-1">Thương hiệu</label>
                    <input v-model="formData.brand" :disabled="modalMode === 'view'" type="text" class="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm disabled:bg-gray-100" placeholder="VD: Dell, Sony...">
                  </div>
                  <div>
                    <label class="block text-xs font-bold text-gray-700 mb-1">Đơn vị tính</label>
                    <select v-model="formData.unit" :disabled="modalMode === 'view'" class="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm disabled:bg-gray-100">
                      <option>Cái</option><option>Chiếc</option><option>Hộp</option><option>Thùng</option><option>Túi</option>
                    </select>
                  </div>
                </div>
              </div>

              <div>
                <h4 class="text-sm font-bold text-primary-600 mb-3 border-b border-gray-100 pb-2">2. Định mức & Đơn giá</h4>
                <div class="grid grid-cols-1 sm:grid-cols-3 gap-4">
                  <div>
                    <label class="block text-xs font-bold text-gray-700 mb-1">Tồn tối thiểu (Min) <span class="text-red-500">*</span></label>
                    <input v-model.number="formData.minStock" :disabled="modalMode === 'view'" type="number" required min="0" class="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm disabled:bg-gray-100">
                  </div>
                  <div>
                    <label class="block text-xs font-bold text-gray-700 mb-1">Tồn tối đa (Max) <span class="text-red-500">*</span></label>
                    <input v-model.number="formData.maxStock" :disabled="modalMode === 'view'" type="number" required min="0" class="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm disabled:bg-gray-100">
                  </div>
                  <div>
                    <label class="block text-xs font-bold text-gray-700 mb-1">Giá trị tham khảo (VNĐ)</label>
                    <input v-model.number="formData.price" :disabled="modalMode === 'view'" type="number" min="0" class="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm disabled:bg-gray-100">
                  </div>
                </div>
              </div>

              <div class="grid grid-cols-1 sm:grid-cols-2 gap-4">
                <div>
                  <label class="block text-xs font-bold text-gray-700 mb-1">Ghi chú thêm</label>
                  <textarea v-model="formData.desc" :disabled="modalMode === 'view'" rows="2" class="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm disabled:bg-gray-100"></textarea>
                </div>
                <div>
                  <label class="block text-xs font-bold text-gray-700 mb-1">Trạng thái kinh doanh</label>
                  <select v-model="formData.status" :disabled="modalMode === 'view'" class="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm disabled:bg-gray-100">
                    <option value="active">Đang kinh doanh (Nhập/Xuất bình thường)</option>
                    <option value="inactive">Ngừng kinh doanh (Khóa giao dịch)</option>
                  </select>
                </div>
              </div>

              <div class="mt-8 pt-4 border-t border-gray-100 flex justify-end gap-3">
                <button type="button" @click="closeModal" class="px-4 py-2 border border-gray-300 text-gray-700 rounded-lg text-sm font-semibold hover:bg-gray-50">
                  {{ modalMode === 'view' ? 'Đóng' : 'Hủy bỏ' }}
                </button>
                <button v-if="modalMode !== 'view'" type="submit" class="px-4 py-2 bg-primary-600 text-white rounded-lg text-sm font-semibold hover:bg-primary-700 shadow-sm">
                  Lưu Sản phẩm
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
.custom-scrollbar::-webkit-scrollbar { height: 6px; width: 6px; }
.custom-scrollbar::-webkit-scrollbar-track { background: transparent; }
.custom-scrollbar::-webkit-scrollbar-thumb { background: #cbd5e1; border-radius: 10px; }
.custom-scrollbar:hover::-webkit-scrollbar-thumb { background: #94a3b8; }
</style>