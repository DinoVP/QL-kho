<script setup>
import { ref, computed } from 'vue'
import { 
  MagnifyingGlassIcon, PlusIcon, PencilSquareIcon, 
  TrashIcon, EyeIcon, XMarkIcon, UserGroupIcon, LinkIcon 
} from '@heroicons/vue/24/outline'

// === 1. STATE: ĐÃ DỌN SẠCH DỮ LIỆU ĐỐI TÁC CHỜ API ===
const partners = ref([])

// === 2. STATE: BỘ LỌC TÌM KIẾM ===
const searchQuery = ref('')
const filterType = ref('')

const filteredPartners = computed(() => {
  return partners.value.filter(p => {
    const matchSearch = p.code.toLowerCase().includes(searchQuery.value.toLowerCase()) || 
                        p.name.toLowerCase().includes(searchQuery.value.toLowerCase())
    const matchType = filterType.value === '' || p.type === filterType.value
    return matchSearch && matchType
  })
})

// === 3. LOGIC MODAL: THÊM / SỬA ĐỐI TÁC ===
const showModal = ref(false)
const modalMode = ref('add') 
const formData = ref({ id: null, code: '', name: '', type: 'Khách hàng', phone: '', email: '', address: '', status: 'active', assignedSkus: [] })

const openModal = (mode, partner = null) => {
  modalMode.value = mode
  if (partner) formData.value = { ...partner } 
  else formData.value = { id: null, code: '', name: '', type: 'Khách hàng', phone: '', email: '', address: '', status: 'active', assignedSkus: [] }
  showModal.value = true
}
const closeModal = () => showModal.value = false

// Tự động sinh mã phân biệt KH và NCC
const generateCode = (type) => {
  const prefix = type === 'Nhà cung cấp' ? 'NCC' : 'KH'
  const count = partners.value.filter(p => p.code.startsWith(prefix)).length
  return `${prefix}${(count + 1).toString().padStart(3, '0')}`
}

const handleSubmit = () => {
  if (modalMode.value === 'add') {
    partners.value.push({
      ...formData.value, id: Date.now(),
      code: generateCode(formData.value.type) // Ép cứng luôn dùng hàm tự sinh, không lấy từ form
    })
    alert('Thêm đối tác thành công!')
  } else {
    const idx = partners.value.findIndex(p => p.id === formData.value.id)
    if (idx !== -1) partners.value[idx] = { ...formData.value }
    alert('Cập nhật thành công!')
  }
  closeModal()
}

const handleDelete = (id, name) => {
  if (confirm(`Xóa đối tác "${name}" có thể ảnh hưởng đến lịch sử giao dịch. Tiếp tục xóa?`)) {
    partners.value = partners.value.filter(p => p.id !== id)
  }
}

// === 4. LOGIC MODAL: GÁN HÀNG CHO NHÀ CUNG CẤP ===
const showAssignModal = ref(false)
const currentNcc = ref(null)
const selectedSkus = ref([])

// Giữ lại mock data sản phẩm để sếp test chức năng gán hàng
const mockAllProducts = ref([
  { sku: 'SKU-001', name: 'Laptop Dell XPS 15 9520' },
  { sku: 'SKU-002', name: 'Bột giặt OMO Matic 3.6kg' },
  { sku: 'SKU-003', name: 'Màn hình LG 27UP850-W' },
  { sku: 'SKU-004', name: 'Bàn phím cơ Keychron K8' }
])

const openAssignModal = (ncc) => {
  currentNcc.value = ncc
  selectedSkus.value = [...ncc.assignedSkus]
  showAssignModal.value = true
}

const closeAssignModal = () => {
  showAssignModal.value = false
  currentNcc.value = null
}

const handleSaveAssign = () => {
  const idx = partners.value.findIndex(p => p.id === currentNcc.value.id)
  if (idx !== -1) {
    partners.value[idx].assignedSkus = [...selectedSkus.value]
  }
  alert(`Đã gán ${selectedSkus.value.length} mặt hàng cho nhà cung cấp ${currentNcc.value.name}!`)
  closeAssignModal()
}
</script>

<template>
  <div class="space-y-5 md:space-y-6 animate-fade-in pb-10 px-0 md:px-1 relative">
    
    <div class="flex flex-col sm:flex-row sm:items-center justify-between gap-4">
      <div>
        <h2 class="text-xl md:text-2xl font-bold text-gray-800">Quản lý Đối tác</h2>
        <p class="text-xs md:text-sm text-gray-500 mt-1">Quản lý Khách hàng và Nhà cung cấp (Gán hàng theo NCC)</p>
      </div>
      <button @click="openModal('add')" class="bg-primary-600 hover:bg-primary-700 text-white px-4 py-2.5 rounded-lg flex items-center gap-2 text-sm font-semibold transition-colors shadow-sm">
        <PlusIcon class="w-5 h-5" /> Thêm Đối tác
      </button>
    </div>

    <div class="bg-white p-3 md:p-4 rounded-xl border border-gray-200 flex flex-col sm:flex-row items-center gap-3 md:gap-4 shadow-sm">
      <div class="relative w-full sm:flex-1">
        <div class="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none"><MagnifyingGlassIcon class="w-5 h-5 text-gray-400" /></div>
        <input v-model="searchQuery" type="text" class="block w-full pl-10 pr-3 py-2 border border-gray-200 rounded-lg text-sm outline-none focus:ring-1 focus:ring-primary-500" placeholder="Tìm kiếm theo mã, tên đối tác...">
      </div>
      <select v-model="filterType" class="w-full sm:w-auto border border-gray-200 rounded-lg text-sm px-4 py-2 outline-none focus:ring-1 focus:ring-primary-500 cursor-pointer">
        <option value="">Tất cả Loại đối tác</option>
        <option value="Nhà cung cấp">Nhà cung cấp (NCC)</option>
        <option value="Khách hàng">Khách hàng (KH)</option>
      </select>
    </div>

    <div class="bg-white rounded-xl border border-gray-200 shadow-sm overflow-hidden">
      <div class="w-full overflow-x-auto custom-scrollbar">
        <table class="min-w-[1000px] w-full divide-y divide-gray-200">
          <thead class="bg-gray-50">
            <tr>
              <th class="px-5 py-3.5 text-left text-xs font-bold text-gray-500 uppercase tracking-wider">Mã Đối tác</th>
              <th class="px-5 py-3.5 text-left text-xs font-bold text-gray-500 uppercase tracking-wider">Tên Đối tác</th>
              <th class="px-5 py-3.5 text-left text-xs font-bold text-gray-500 uppercase tracking-wider">Phân loại</th>
              <th class="px-5 py-3.5 text-left text-xs font-bold text-gray-500 uppercase tracking-wider">Liên hệ</th>
              <th class="px-5 py-3.5 text-center text-xs font-bold text-gray-500 uppercase tracking-wider">Trạng thái</th>
              <th class="px-5 py-3.5 text-right text-xs font-bold text-gray-500 uppercase tracking-wider">Thao tác</th>
            </tr>
          </thead>
          
          <tbody class="divide-y divide-gray-100 bg-white">
            <tr v-if="filteredPartners.length === 0">
              <td colspan="6" class="px-6 py-16 text-center">
                <UserGroupIcon class="w-12 h-12 text-gray-300 mx-auto mb-3" />
                <h3 class="text-base font-semibold text-gray-700">Chưa có dữ liệu đối tác</h3>
                <p class="text-sm text-gray-500 mt-1">Bấm "Thêm đối tác" để bắt đầu thiết lập.</p>
              </td>
            </tr>

            <tr v-for="partner in filteredPartners" :key="partner.id" class="hover:bg-gray-50 transition-colors">
              <td class="px-5 py-3 text-sm font-bold text-primary-700">{{ partner.code }}</td>
              <td class="px-5 py-3 text-sm font-bold text-gray-900">{{ partner.name }}</td>
              <td class="px-5 py-3">
                <span v-if="partner.type === 'Nhà cung cấp'" class="text-[10px] font-bold px-2 py-1 rounded bg-amber-100 text-amber-700 border border-amber-200 uppercase">Nhà cung cấp</span>
                <span v-else class="text-[10px] font-bold px-2 py-1 rounded bg-blue-100 text-blue-700 border border-blue-200 uppercase">Khách hàng</span>
              </td>
              <td class="px-5 py-3">
                <div class="flex flex-col">
                  <span class="text-sm font-medium text-gray-700">{{ partner.phone || '—' }}</span>
                  <span class="text-xs text-gray-500">{{ partner.email || '—' }}</span>
                </div>
              </td>
              <td class="px-5 py-3 text-center">
                <span v-if="partner.status === 'active'" class="text-xs font-bold px-2.5 py-1 rounded bg-green-100 text-green-700">Đang giao dịch</span>
                <span v-else class="text-xs font-bold px-2.5 py-1 rounded bg-red-100 text-red-700">Ngừng giao dịch</span>
              </td>
              <td class="px-5 py-3 text-right space-x-2 whitespace-nowrap">
                <button v-if="partner.type === 'Nhà cung cấp'" @click="openAssignModal(partner)" class="p-1.5 text-emerald-600 hover:bg-emerald-50 rounded border border-transparent hover:border-emerald-200 transition-colors" title="Gán Sản phẩm cung cấp">
                  <LinkIcon class="w-5 h-5 inline-block mr-1" /> <span class="text-xs font-bold">{{ partner.assignedSkus.length }} Món</span>
                </button>

                <button @click="openModal('view', partner)" class="p-1.5 text-blue-600 hover:bg-blue-50 rounded" title="Xem chi tiết"><EyeIcon class="w-5 h-5" /></button>
                <button @click="openModal('edit', partner)" class="p-1.5 text-amber-600 hover:bg-amber-50 rounded" title="Chỉnh sửa"><PencilSquareIcon class="w-5 h-5" /></button>
                <button @click="handleDelete(partner.id, partner.name)" class="p-1.5 text-red-600 hover:bg-red-50 rounded" title="Xóa"><TrashIcon class="w-5 h-5" /></button>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>

    <Teleport to="body">
      <div v-if="showModal" class="fixed inset-0 z-50 flex items-center justify-center px-4 bg-slate-900/50 backdrop-blur-sm animate-fade-in">
        <div class="bg-white rounded-xl shadow-2xl w-full max-w-lg overflow-hidden transform transition-all">
          <div class="px-4 md:px-6 py-4 border-b border-gray-100 flex items-center justify-between bg-gray-50">
            <h3 class="text-lg font-bold text-gray-800">{{ modalMode === 'add' ? 'Thêm Đối tác' : (modalMode === 'edit' ? 'Cập nhật Đối tác' : 'Chi tiết Đối tác') }}</h3>
            <button @click="closeModal" class="text-gray-400 hover:text-red-500 p-1"><XMarkIcon class="w-6 h-6" /></button>
          </div>

          <div class="p-4 md:p-6 max-h-[80vh] overflow-y-auto">
            <form @submit.prevent="handleSubmit" class="space-y-4">
              
              <div class="grid grid-cols-1 sm:grid-cols-2 gap-4">
                <div>
                  <label class="block text-xs font-bold text-gray-700 mb-1">Loại đối tác <span class="text-red-500">*</span></label>
                  <select v-model="formData.type" :disabled="modalMode !== 'add'" class="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm disabled:bg-gray-100 cursor-pointer">
                    <option>Khách hàng</option>
                    <option>Nhà cung cấp</option>
                  </select>
                </div>
                
                <div v-if="modalMode === 'add'">
                  <label class="block text-xs font-bold text-gray-700 mb-1">Mã Đối tác</label>
                  <div class="w-full border border-gray-200 border-dashed rounded-lg px-3 py-2 text-sm bg-gray-50 text-gray-400 italic">
                    Hệ thống tự động sinh
                  </div>
                </div>
                <div v-else>
                  <label class="block text-xs font-bold text-gray-700 mb-1">Mã Đối tác</label>
                  <input v-model="formData.code" disabled type="text" class="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm bg-gray-100 text-gray-500 cursor-not-allowed">
                </div>
              </div>

              <div>
                <label class="block text-xs font-bold text-gray-700 mb-1">Tên Đối tác (Công ty / Cá nhân) <span class="text-red-500">*</span></label>
                <input v-model="formData.name" :disabled="modalMode === 'view'" required type="text" class="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm disabled:bg-gray-100">
              </div>

              <div class="grid grid-cols-1 sm:grid-cols-2 gap-4">
                <div>
                  <label class="block text-xs font-bold text-gray-700 mb-1">Số điện thoại</label>
                  <input v-model="formData.phone" :disabled="modalMode === 'view'" type="tel" class="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm disabled:bg-gray-100">
                </div>
                <div>
                  <label class="block text-xs font-bold text-gray-700 mb-1">Email</label>
                  <input v-model="formData.email" :disabled="modalMode === 'view'" type="email" class="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm disabled:bg-gray-100">
                </div>
              </div>

              <div>
                <label class="block text-xs font-bold text-gray-700 mb-1">Địa chỉ</label>
                <input v-model="formData.address" :disabled="modalMode === 'view'" type="text" class="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm disabled:bg-gray-100">
              </div>

              <div>
                <label class="block text-xs font-bold text-gray-700 mb-1">Trạng thái giao dịch</label>
                <select v-model="formData.status" :disabled="modalMode === 'view'" class="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm disabled:bg-gray-100 cursor-pointer">
                  <option value="active">Đang giao dịch</option>
                  <option value="inactive">Ngừng giao dịch</option>
                </select>
              </div>

              <div class="mt-6 pt-4 border-t border-gray-100 flex justify-end gap-3">
                <button type="button" @click="closeModal" class="px-4 py-2 border border-gray-300 rounded-lg text-sm hover:bg-gray-50">{{ modalMode === 'view' ? 'Đóng' : 'Hủy bỏ' }}</button>
                <button v-if="modalMode !== 'view'" type="submit" class="px-4 py-2 bg-primary-600 text-white rounded-lg text-sm hover:bg-primary-700 shadow-sm">Lưu thay đổi</button>
              </div>
            </form>
          </div>
        </div>
      </div>
    </Teleport>

    <Teleport to="body">
      <div v-if="showAssignModal" class="fixed inset-0 z-50 flex items-center justify-center px-4 bg-slate-900/50 backdrop-blur-sm animate-fade-in">
        <div class="bg-white rounded-xl shadow-2xl w-full max-w-lg overflow-hidden transform transition-all">
          <div class="px-4 md:px-6 py-4 border-b border-emerald-100 bg-emerald-50 flex items-center justify-between">
            <div>
              <h3 class="text-lg font-bold text-emerald-800 flex items-center gap-2"><LinkIcon class="w-5 h-5"/> Cấu hình Hàng hóa NCC</h3>
              <p class="text-xs text-emerald-600 mt-1">Đang gán cho: <strong>{{ currentNcc?.name }}</strong></p>
            </div>
            <button @click="closeAssignModal" class="text-gray-400 hover:text-red-500 p-1"><XMarkIcon class="w-6 h-6" /></button>
          </div>

          <div class="p-4 md:p-6">
            <p class="text-sm text-gray-600 mb-4">Vui lòng chọn (tick) các mặt hàng mà nhà cung cấp này có thể giao:</p>
            
            <div class="space-y-2 border border-gray-200 rounded-lg p-3 max-h-60 overflow-y-auto custom-scrollbar">
              <label v-for="product in mockAllProducts" :key="product.sku" class="flex items-start gap-3 p-2 hover:bg-gray-50 rounded cursor-pointer transition-colors border border-transparent hover:border-gray-200">
                <input type="checkbox" :value="product.sku" v-model="selectedSkus" class="mt-1 w-4 h-4 text-emerald-600 border-gray-300 rounded focus:ring-emerald-500 cursor-pointer">
                <div class="flex flex-col">
                  <span class="text-sm font-bold text-gray-800">{{ product.name }}</span>
                  <span class="text-xs text-gray-500">Mã SKU: {{ product.sku }}</span>
                </div>
              </label>
            </div>
            
            <div class="mt-2 text-xs font-medium text-emerald-600">Đã chọn: {{ selectedSkus.length }} mặt hàng</div>

            <div class="mt-6 pt-4 border-t border-gray-100 flex justify-end gap-3">
              <button type="button" @click="closeAssignModal" class="px-4 py-2 border border-gray-300 rounded-lg text-sm hover:bg-gray-50">Hủy</button>
              <button @click="handleSaveAssign" class="px-4 py-2 bg-emerald-600 text-white rounded-lg text-sm font-bold hover:bg-emerald-700 shadow-sm flex items-center gap-2">
                <LinkIcon class="w-4 h-4"/> Lưu Gán hàng
              </button>
            </div>
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