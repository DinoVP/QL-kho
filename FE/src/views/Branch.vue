<script setup>
import { ref, computed, onMounted } from 'vue'
import { 
  MagnifyingGlassIcon, PlusIcon, PencilSquareIcon, TrashIcon, EyeIcon, 
  XMarkIcon, BuildingOfficeIcon, IdentificationIcon, UserIcon, CheckCircleIcon, 
  XCircleIcon, ArchiveBoxIcon, ExclamationTriangleIcon, UserGroupIcon, UserMinusIcon, MapPinIcon,
  CubeIcon, ChartBarSquareIcon
} from '@heroicons/vue/24/outline'

const API_URL = 'https://localhost:7139/api/Branches'
const STOCK_API = 'https://localhost:7139/api/Stock' // Gọi thêm API tồn kho để đếm

const getAuthHeaders = () => ({ 
    'Content-Type': 'application/json', 
    'Authorization': 'Bearer ' + (localStorage.getItem('authToken') || '') 
})

const branches = ref([])
const isLoading = ref(false)
const searchQuery = ref('')

const toast = ref({ show: false, message: '', type: 'success' })
const showToast = (message, type = 'success') => {
  toast.value = { show: true, message, type }
  setTimeout(() => { toast.value.show = false }, 3000) 
}

const filteredBranches = computed(() => {
  if (!searchQuery.value) return branches.value
  const q = searchQuery.value.toLowerCase()
  return branches.value.filter(b => 
    b.code?.toLowerCase().includes(q) || 
    b.name?.toLowerCase().includes(q) || 
    b.managerName?.toLowerCase().includes(q) ||
    b.address?.toLowerCase().includes(q)
  )
})

// ====================================================================
// QUẢN LÝ KHO CỦA CHI NHÁNH (ĐÃ CỘNG TỒN KHO)
// ====================================================================
const isWhModalOpen = ref(false)
const selectedBranchId = ref(null) 
const selectedBranchName = ref('')
const branchWarehouses = ref([])
const isWhLoading = ref(false)

const openWhModal = async (branch) => {
  selectedBranchId.value = branch.id || branch.Id
  selectedBranchName.value = branch.name || branch.Name
  isWhModalOpen.value = true
  isWhLoading.value = true
  
  try {
    const res = await fetch(`${API_URL}/${selectedBranchId.value}/warehouses-detail`, { headers: getAuthHeaders() })
    if (res.ok) {
        let whList = await res.json()
        
        // TẢI TỒN KHO ĐỂ TÍNH TOÁN CHO TỪNG KHO VẬT LÝ
        try {
            const stockRes = await fetch(STOCK_API, { headers: getAuthHeaders() })
            const allStocks = stockRes.ok ? await stockRes.json() : []
            
            whList = whList.map(wh => {
                const whId = wh.warehouseId || wh.WarehouseId || wh.id || wh.Id
                // Lọc những hàng đang nằm trong Kho này
                const whStocks = allStocks.filter(s => (s.warehouseId || s.WarehouseId) === whId)
                
                // Đếm số loại mặt hàng (SKU khác nhau)
                const uniqueItems = new Set(whStocks.map(s => s.variantId || s.VariantId)).size
                // Tổng số lượng tất cả hàng
                const totalQty = whStocks.reduce((sum, s) => sum + Number(s.qty || s.Qty || s.quantity || s.Quantity || 0), 0)
                
                return { ...wh, itemCount: uniqueItems, totalQuantity: totalQty }
            })
        } catch (e) { console.error("Không đếm được kho", e) }

        branchWarehouses.value = whList
    }
  } catch (err) { 
    showToast('Lỗi tải danh sách kho', 'error') 
  } finally { 
    isWhLoading.value = false 
  }
}

const deleteWarehouse = async (whId, whName) => {
  if (!confirm(`Sếp có chắc chắn xóa kho "${whName}"? Nhân sự trong kho sẽ tự động được gỡ (vẫn giữ chi nhánh).`)) return;
  try {
    const res = await fetch(`${API_URL}/warehouses/${whId}`, { method: 'DELETE', headers: getAuthHeaders() });
    let data; 
    try { data = await res.json(); } catch(e) { data = { message: 'Lỗi phản hồi' } }
    if (res.ok) {
      showToast(data.message || 'Xóa kho thành công!', 'success');
      fetchBranches();
      openWhModal({id: selectedBranchId.value, name: selectedBranchName.value});
    } else { 
      showToast(data.message, 'error'); 
    }
  } catch (err) { showToast('Lỗi kết nối server', 'error'); }
};

const clearWarehouseEmployees = async (whId, whName) => {
  if (!confirm(`CẢNH BÁO: Gỡ TẤT CẢ nhân viên khỏi kho "${whName}"?`)) return;
  try {
    const res = await fetch(`${API_URL}/warehouses/${whId}/clear-employees`, { method: 'PUT', headers: getAuthHeaders() });
    let data; 
    try { data = await res.json(); } catch(e) { data = { message: 'Lỗi phản hồi' } }
    if (res.ok) {
      showToast(data.message || 'Đã gỡ tất cả nhân sự khỏi kho!', 'success');
      openWhModal({id: selectedBranchId.value, name: selectedBranchName.value});
    } else { 
      showToast(data.message, 'error'); 
    }
  } catch (err) { showToast('Lỗi kết nối server', 'error'); }
};

// ====================================================================
// XEM CHI TIẾT NHÂN SỰ CỦA 1 KHO
// ====================================================================
const isWhEmpModalOpen = ref(false)
const currentWhName = ref('')
const whEmployees = ref([])
const isWhEmpLoading = ref(false)

const openWhEmpModal = async (wh) => {
  currentWhName.value = wh.whname || wh.Whname
  isWhEmpModalOpen.value = true
  isWhEmpLoading.value = true
  try {
    const res = await fetch(`${API_URL}/warehouses/${wh.warehouseId || wh.WarehouseId}/employees`, { headers: getAuthHeaders() })
    if (res.ok) whEmployees.value = await res.json()
  } catch(e) { 
    showToast('Lỗi tải danh sách NV', 'error') 
  } finally { 
    isWhEmpLoading.value = false 
  }
}

// ====================================================================
// TẠO NHANH KHO
// ====================================================================
const isCreateWhModalOpen = ref(false)
const whFormData = ref({ name: '', address: '' })

const openCreateWhModal = () => {
  whFormData.value = { name: '', address: '' }
  isCreateWhModalOpen.value = true
}

const submitCreateWh = async () => {
  if (!whFormData.value.name || !whFormData.value.address) { 
    showToast('Nhập đủ Tên và Địa chỉ kho sếp ơi!', 'error'); 
    return 
  }
  try {
    const res = await fetch(`${API_URL}/${selectedBranchId.value}/warehouses`, {
      method: 'POST', 
      headers: getAuthHeaders(), 
      body: JSON.stringify(whFormData.value)
    })
    let data; 
    try { data = await res.json(); } catch(e) { data = { message: 'Lỗi server' } }
    
    if (res.ok) {
      showToast(data.message || 'Tạo Kho mới thành công!', 'success')
      isCreateWhModalOpen.value = false
      openWhModal({id: selectedBranchId.value, name: selectedBranchName.value})
      fetchBranches()
    } else { 
      showToast(data.message, 'error') 
    }
  } catch (err) { showToast('Lỗi kết nối Server', 'error') }
}

// ====================================================================
// QUẢN LÝ CHI NHÁNH CHÍNH (ĐÃ CỘNG TỒN KHO)
// ====================================================================
const showModal = ref(false)
const modalMode = ref('add') 
const formData = ref({ 
  id: null, code: '', name: '', address: '', 
  managerId: null, managerName: '', email: '', phone: '', 
  status: 'active', warehouseCount: 0 
})

const openModal = (mode, branch = null) => {
  modalMode.value = mode
  if (branch) formData.value = { ...branch } 
  else formData.value = { id: null, code: '', name: '', address: '', managerId: null, managerName: '', email: '', phone: '', status: 'active', warehouseCount: 0 }
  showModal.value = true
}

const closeModal = () => showModal.value = false

const isManagerModalOpen = ref(false)
const availableManagers = ref([])
const managerSearchQuery = ref('')

const filteredManagers = computed(() => {
  if (!managerSearchQuery.value) return availableManagers.value
  const q = managerSearchQuery.value.toLowerCase()
  return availableManagers.value.filter(m => m.managerName.toLowerCase().includes(q) || m.empCode.toLowerCase().includes(q))
})

const openManagerModal = async () => {
  if (modalMode.value === 'view') return
  isManagerModalOpen.value = true; managerSearchQuery.value = ''
  try {
    const res = await fetch(`${API_URL}/available-managers`, { headers: getAuthHeaders() })
    if (res.ok) availableManagers.value = await res.json()
  } catch (err) { showToast('Lỗi tải danh sách giám đốc', 'error') }
}

const selectManager = (manager) => {
  formData.value.managerId = manager.managerId
  formData.value.managerName = manager.managerName
  formData.value.email = manager.email || ''   
  formData.value.phone = manager.phone || ''   
  isManagerModalOpen.value = false
}

const clearManager = () => {
  formData.value.managerId = null
  formData.value.managerName = ''
  formData.value.email = ''
  formData.value.phone = ''
}

const fetchBranches = async () => {
  isLoading.value = true
  try {
    const res = await fetch(API_URL, { headers: getAuthHeaders() })
    if (!res.ok) throw new Error("Lỗi fetch")
    let branchesData = await res.json()

    // 1. Fetch Tồn Kho
    let allStocks = []
    try {
        const stockRes = await fetch(STOCK_API, { headers: getAuthHeaders() })
        if (stockRes.ok) allStocks = await stockRes.json()
    } catch (e) { console.warn("Chưa tải được tồn kho", e) }

    // 2. Chạy vòng lặp nhặt Kho để đếm số liệu cho Chi nhánh
    const branchPromises = branchesData.map(async (b) => {
        const bId = b.id || b.Id
        try {
            const whRes = await fetch(`${API_URL}/${bId}/warehouses-detail`, { headers: getAuthHeaders() })
            const whList = whRes.ok ? await whRes.json() : []
            
            // Ép chuẩn số lượng Kho nếu Backend quên đếm
            b.warehouseCount = whList.length
            
            // Tìm tất cả Tồn Kho thuộc các Kho của Chi Nhánh này
            const whIds = whList.map(w => w.warehouseId || w.WarehouseId || w.id || w.Id)
            const bStocks = allStocks.filter(s => whIds.includes(s.warehouseId || s.WarehouseId))
            
            // Tính số loại SP và Tổng Số Lượng
            b.itemCount = new Set(bStocks.map(s => s.variantId || s.VariantId)).size
            b.totalQuantity = bStocks.reduce((sum, s) => sum + Number(s.qty || s.Qty || s.quantity || s.Quantity || 0), 0)
        } catch(e) {
            b.itemCount = 0; b.totalQuantity = 0; b.warehouseCount = 0;
        }
        return b
    })

    branches.value = await Promise.all(branchPromises)
  } catch (err) { showToast('Lỗi kết nối Backend', 'error') } 
  finally { isLoading.value = false }
}

const handleSubmit = async () => {
  if (!formData.value.name) { showToast('Vui lòng nhập Tên Chi nhánh!', 'error'); return; }
  const method = modalMode.value === 'add' ? 'POST' : 'PUT'
  const url = modalMode.value === 'add' ? API_URL : `${API_URL}/${formData.value.id}`
  try {
    const res = await fetch(url, { method, headers: getAuthHeaders(), body: JSON.stringify(formData.value) })
    let data; 
    try { data = await res.json(); } catch(e) { data = { message: 'Lỗi hệ thống' } }
    
    if (res.ok) { 
      showToast(data.message || 'Lưu thành công!', 'success'); 
      closeModal(); 
      fetchBranches(); 
    } 
    else { showToast(data.message, 'error') }
  } catch (err) { showToast('Lỗi kết nối Server', 'error') }
}

const handleDelete = async (branchId, branchName) => {
  if (confirm(`Sếp có chắc chắn muốn xóa Chi nhánh "${branchName}" không?`)) {
    try {
      const res = await fetch(`${API_URL}/${branchId}`, { method: 'DELETE', headers: getAuthHeaders() })
      let data = {};
      try { data = await res.json(); } catch(e) { data = { message: 'Không thể xóa! Có lỗi hệ thống từ máy chủ.' }; }

      if (res.ok) { 
        showToast(data.message || 'Xóa thành công!', 'success'); 
        fetchBranches(); 
      } else { 
        showToast(data.message, 'error') 
      }
    } catch (err) { 
      showToast('Lỗi kết nối đến máy chủ!', 'error') 
    }
  }
}

onMounted(() => fetchBranches())
</script>

<template>
  <div class="space-y-5 md:space-y-6 animate-fade-in pb-10 px-0 md:px-1 relative">
    
    <Transition name="slide-fade">
      <div v-if="toast.show" class="fixed top-6 right-6 z-[100] flex items-center p-4 mb-4 text-gray-900 bg-white rounded-lg shadow-xl border-l-4" :class="toast.type === 'success' ? 'border-green-500' : 'border-red-500'">
        <div class="inline-flex items-center justify-center w-8 h-8 rounded-lg" :class="toast.type === 'success' ? 'text-green-500 bg-green-100' : 'text-red-500 bg-red-100'">
          <CheckCircleIcon v-if="toast.type === 'success'" class="w-6 h-6" />
          <XCircleIcon v-else class="w-6 h-6" />
        </div>
        <div class="ml-3 text-sm font-medium mr-4">{{ toast.message }}</div>
      </div>
    </Transition>

    <div class="flex flex-col sm:flex-row sm:items-center justify-between gap-4">
      <div>
        <h2 class="text-xl md:text-2xl font-bold text-gray-800 flex items-center gap-2">
          <BuildingOfficeIcon class="w-8 h-8 text-primary-600"/> 
          Hệ thống Chi nhánh
        </h2>
        <p class="text-xs md:text-sm text-gray-500 mt-1">Quản lý danh sách chi nhánh, thông tin liên hệ và kho trực thuộc</p>
      </div>
      <button @click="openModal('add')" class="bg-primary-600 hover:bg-primary-700 text-white px-4 py-2.5 rounded-lg flex items-center gap-2 text-sm font-semibold transition-colors shadow-md">
        <PlusIcon class="w-5 h-5" /> Thêm Chi nhánh
      </button>
    </div>

    <div class="bg-white p-3 md:p-4 rounded-xl border border-gray-200 flex flex-col sm:flex-row items-center gap-3 md:gap-4 shadow-sm">
      <div class="relative w-full sm:flex-1 max-w-none sm:max-w-md">
        <div class="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none">
          <MagnifyingGlassIcon class="w-5 h-5 text-gray-400" />
        </div>
        <input v-model="searchQuery" type="text" class="block w-full pl-10 pr-3 py-2 border border-gray-200 rounded-lg text-sm focus:ring-1 focus:ring-primary-500 outline-none" placeholder="Tìm theo mã CN, tên, giám đốc, địa chỉ...">
      </div>
    </div>

    <div class="bg-white rounded-xl border border-gray-200 shadow-sm overflow-hidden">
      <div class="w-full overflow-x-auto custom-scrollbar">
        <table class="min-w-[1200px] w-full divide-y divide-gray-200">
          <thead class="bg-gray-50">
            <tr>
              <th class="px-5 py-3.5 text-left text-xs font-bold text-gray-500 uppercase tracking-wider">Mã CN</th>
              <th class="px-5 py-3.5 text-left text-xs font-bold text-gray-500 uppercase tracking-wider">Tên & Địa chỉ CN</th>
              <th class="px-5 py-3.5 text-left text-xs font-bold text-gray-500 uppercase tracking-wider">Giám đốc CN</th>
              <th class="px-5 py-3.5 text-left text-xs font-bold text-gray-500 uppercase tracking-wider">Liên hệ</th>
              <th class="px-5 py-3.5 text-center text-xs font-bold text-gray-500 uppercase tracking-wider">Hàng hóa (Tổng)</th>
              <th class="px-5 py-3.5 text-center text-xs font-bold text-gray-500 uppercase tracking-wider">SL Kho</th>
              <th class="px-5 py-3.5 text-left text-xs font-bold text-gray-500 uppercase tracking-wider">Trạng thái</th>
              <th class="px-5 py-3.5 text-right text-xs font-bold text-gray-500 uppercase tracking-wider">Thao tác</th>
            </tr>
          </thead>
          <tbody class="divide-y divide-gray-100 bg-white">
            <tr v-if="isLoading">
              <td colspan="8" class="px-6 py-10 text-center text-gray-500">Đang tải dữ liệu...</td>
            </tr>
            <tr v-else-if="filteredBranches.length === 0">
              <td colspan="8" class="px-6 py-16 text-center">
                <BuildingOfficeIcon class="w-12 h-12 text-gray-300 mx-auto mb-3" />
                <h3 class="text-base font-semibold text-gray-700">Chưa có dữ liệu Chi nhánh</h3>
                <p class="text-sm text-gray-500 mt-1">Bấm "Thêm Chi nhánh" để tạo mới.</p>
              </td>
            </tr>
            <tr v-else v-for="branch in filteredBranches" :key="branch.id" class="hover:bg-gray-50 transition-colors">
              <td class="px-5 py-3 text-sm font-bold text-primary-700">{{ branch.code }}</td>
              
              <td class="px-5 py-3">
                <div class="text-sm font-bold text-gray-900">{{ branch.name }}</div>
                <div class="text-xs text-gray-500 mt-0.5 line-clamp-1" :title="branch.address"><MapPinIcon class="w-3.5 h-3.5 inline text-gray-400 mr-1"/>{{ branch.address || '—' }}</div>
              </td>
              
              <td class="px-5 py-3">
                <div class="flex items-center gap-1.5 text-sm text-gray-700">
                  <IdentificationIcon class="w-4 h-4 text-gray-400" />
                  <span :class="!branch.managerName ? 'italic text-gray-400' : 'font-medium'">
                    {{ branch.managerName || 'Chưa bổ nhiệm' }}
                  </span>
                </div>
              </td>
              
              <td class="px-5 py-3">
                <div class="text-xs text-gray-500" v-if="branch.email || branch.phone">
                  <div v-if="branch.email">{{ branch.email }}</div>
                  <div v-if="branch.phone">{{ branch.phone }}</div>
                </div>
                <div class="text-xs text-gray-400 italic" v-else>—</div>
              </td>

              <td class="px-5 py-3 text-center">
                <div class="flex justify-center gap-4 text-xs">
                  <div class="flex flex-col items-center" title="Số loại mặt hàng">
                    <CubeIcon class="w-4 h-4 text-blue-500 mb-0.5"/>
                    <span class="font-bold text-gray-700">{{ branch.itemCount || 0 }}</span>
                  </div>
                  <div class="flex flex-col items-center" title="Tổng số lượng tồn">
                    <ChartBarSquareIcon class="w-4 h-4 text-emerald-500 mb-0.5"/>
                    <span class="font-bold text-gray-700">{{ branch.totalQuantity || 0 }}</span>
                  </div>
                </div>
              </td>
              
              <td class="px-5 py-3 text-center font-bold text-gray-700">
                <span class="bg-indigo-50 text-indigo-700 border border-indigo-200 px-3 py-1 rounded-full text-xs">
                  {{ branch.warehouseCount || 0 }}
                </span>
              </td>
              
              <td class="px-5 py-3">
                <span v-if="branch.status === 'active'" class="text-xs font-bold px-2.5 py-1 rounded bg-green-100 text-green-700">Hoạt động</span>
                <span v-else class="text-xs font-bold px-2.5 py-1 rounded bg-red-100 text-red-700">Đóng cửa</span>
              </td>
              
              <td class="px-5 py-3 text-right space-x-2 whitespace-nowrap">
                <button @click="openWhModal(branch)" class="p-1.5 text-purple-600 hover:bg-purple-50 rounded" title="Quản lý Kho">
                  <ArchiveBoxIcon class="w-5 h-5" />
                </button>
                <button @click="openModal('view', branch)" class="p-1.5 text-blue-600 hover:bg-blue-50 rounded" title="Xem chi tiết">
                  <EyeIcon class="w-5 h-5" />
                </button>
                <button @click="openModal('edit', branch)" class="p-1.5 text-amber-600 hover:bg-amber-50 rounded" title="Chỉnh sửa">
                  <PencilSquareIcon class="w-5 h-5" />
                </button>
                <button @click="handleDelete(branch.id || branch.Id, branch.name || branch.Name)" class="p-1.5 text-red-600 hover:bg-red-50 rounded" title="Xóa">
                  <TrashIcon class="w-5 h-5" />
                </button>
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
            <h3 class="text-lg font-bold text-gray-800">
              {{ modalMode === 'add' ? 'Thêm Chi nhánh' : (modalMode === 'edit' ? 'Cập nhật Chi nhánh' : 'Chi tiết Chi nhánh') }}
            </h3>
            <button @click="closeModal" class="text-gray-400 hover:text-red-500 transition-colors"><XMarkIcon class="w-6 h-6" /></button>
          </div>
          
          <div class="p-4 md:p-6 max-h-[80vh] overflow-y-auto">
            <form @submit.prevent="handleSubmit" class="space-y-4">
              
              <div class="grid grid-cols-1 gap-4 mb-4" v-if="modalMode !== 'add'">
                <div>
                  <label class="block text-xs font-bold text-gray-700 mb-1">Mã CN</label>
                  <input v-model="formData.code" disabled type="text" class="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm bg-gray-100 font-bold text-blue-700">
                </div>
              </div>

              <div class="grid grid-cols-1 sm:grid-cols-2 gap-4">
                <div>
                  <label class="block text-xs font-bold text-gray-700 mb-1">Tên Chi nhánh *</label>
                  <input v-model="formData.name" :disabled="modalMode === 'view'" required type="text" class="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm disabled:bg-gray-100">
                </div>
                <div>
                  <label class="block text-xs font-bold text-gray-700 mb-1">Số lượng Kho <span class="font-normal text-gray-400">(Tự động đếm)</span></label>
                  <input v-model.number="formData.warehouseCount" disabled type="number" class="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm bg-gray-100 text-gray-600 font-bold cursor-not-allowed">
                </div>
              </div>
              
              <div>
                <label class="block text-xs font-bold text-gray-700 mb-1">Địa chỉ quản lý (Văn phòng)</label>
                <input v-model="formData.address" :disabled="modalMode === 'view'" type="text" class="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm disabled:bg-gray-100">
              </div>
              
              <div class="grid grid-cols-1 sm:grid-cols-2 gap-4">
                <div>
                  <label class="block text-xs font-bold text-gray-700 mb-1">Email (Theo Giám đốc)</label>
                  <input v-model="formData.email" readonly type="email" class="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm bg-gray-100 cursor-not-allowed text-gray-500" placeholder="Chưa có">
                </div>
                <div>
                  <label class="block text-xs font-bold text-gray-700 mb-1">SĐT (Theo Giám đốc)</label>
                  <input v-model="formData.phone" readonly type="tel" class="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm bg-gray-100 cursor-not-allowed text-gray-500" placeholder="Chưa có">
                </div>
              </div>
              
              <div class="grid grid-cols-1 sm:grid-cols-2 gap-4">
                <div>
                  <label class="block text-xs font-bold text-gray-700 mb-1">Giám đốc Chi nhánh</label>
                  <div class="flex items-center gap-2">
                    <div class="relative cursor-pointer flex-1" @click="openManagerModal">
                      <input :value="formData.managerName" readonly type="text" class="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm cursor-pointer disabled:bg-gray-100 bg-white" placeholder="Nhấn để chọn...">
                      <div class="absolute inset-y-0 right-0 pr-2 flex items-center pointer-events-none">
                        <IdentificationIcon class="w-4 h-4 text-gray-400"/>
                      </div>
                    </div>
                    <button v-if="formData.managerId && modalMode !== 'view'" type="button" @click="clearManager" class="p-2 text-red-500 hover:bg-red-50 rounded-lg border border-red-100 transition-colors" title="Gỡ Giám đốc khỏi Chi nhánh này">
                      <XMarkIcon class="w-5 h-5"/>
                    </button>
                  </div>
                </div>
                
                <div>
                  <label class="block text-xs font-bold text-gray-700 mb-1">Trạng thái</label>
                  <select v-model="formData.status" :disabled="modalMode === 'view'" class="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm disabled:bg-gray-100">
                    <option value="active">Hoạt động</option>
                    <option value="inactive">Đóng cửa</option>
                  </select>
                </div>
              </div>
              
              <div class="mt-6 pt-4 border-t border-gray-100 flex justify-end gap-3">
                <button type="button" @click="closeModal" class="px-4 py-2 border border-gray-300 rounded-lg text-sm hover:bg-gray-50 transition-colors">
                  {{ modalMode === 'view' ? 'Đóng' : 'Hủy' }}
                </button>
                <button v-if="modalMode !== 'view'" type="submit" class="px-4 py-2 bg-primary-600 text-white rounded-lg text-sm hover:bg-primary-700 transition-colors shadow-sm">
                  Lưu dữ liệu
                </button>
              </div>
            </form>
          </div>
        </div>
      </div>
    </Teleport>

    <Teleport to="body">
      <div v-if="isManagerModalOpen" class="fixed inset-0 z-[100] flex items-center justify-center px-4 bg-slate-900/60 backdrop-blur-sm animate-fade-in-fast">
        <div class="bg-white rounded-lg shadow-xl w-full max-w-sm overflow-hidden">
          <div class="px-4 py-3 border-b flex justify-between items-center bg-primary-600 text-white">
            <h4 class="font-bold text-sm flex items-center gap-2"><IdentificationIcon class="w-4 h-4" /> Chọn GĐ Chi nhánh</h4>
            <button @click="isManagerModalOpen = false" class="hover:text-red-200 transition-colors"><XMarkIcon class="w-5 h-5" /></button>
          </div>
          <div class="p-3">
            <div class="relative mb-3">
              <MagnifyingGlassIcon class="absolute left-2.5 top-2 w-4 h-4 text-gray-400" />
              <input v-model="managerSearchQuery" type="text" class="w-full border border-gray-300 rounded-lg pl-8 pr-2 py-1.5 text-sm focus:ring-primary-500" placeholder="Tìm tên, mã NV...">
            </div>
            <div class="max-h-60 overflow-y-auto space-y-1 custom-scrollbar pr-1">
              <div v-if="filteredManagers.length === 0" class="text-center text-sm text-gray-500 py-4 italic">Không có GĐ nào rảnh tay.</div>
              <button 
                v-else v-for="m in filteredManagers" :key="m.managerId" @click="selectManager(m)"
                class="w-full text-left flex items-center gap-3 p-2.5 rounded-lg hover:bg-primary-50 border border-transparent hover:border-primary-100 transition-all"
              >
                <div class="bg-gray-100 p-1.5 rounded-full text-gray-500"><UserIcon class="w-5 h-5" /></div>
                <div>
                  <div class="text-sm font-bold text-gray-800">{{ m.managerName }}</div>
                  <div class="text-xs text-gray-500">Mã NV: <span class="font-medium">{{ m.empCode }}</span> | SĐT: {{ m.phone || 'N/A' }}</div>
                </div>
              </button>
            </div>
          </div>
        </div>
      </div>
    </Teleport>

    <Teleport to="body">
      <div v-if="isWhModalOpen" class="fixed inset-0 z-[70] flex items-center justify-center bg-slate-900/50 backdrop-blur-sm p-4 animate-fade-in">
        <div class="bg-white rounded-xl shadow-2xl w-full max-w-5xl overflow-hidden">
          <div class="p-4 border-b flex justify-between items-center bg-purple-50">
            <div>
              <h3 class="font-bold text-purple-800 text-lg flex items-center gap-2">
                <ArchiveBoxIcon class="w-6 h-6"/> Quản lý Kho - {{ selectedBranchName }}
              </h3>
              <p class="text-xs text-purple-600 italic mt-1">Gợi ý: Xóa kho sẽ tự động gỡ nhân viên khỏi kho đó (vẫn giữ nhân viên ở chi nhánh).</p>
            </div>
            <div class="flex items-center gap-3">
              <button @click="openCreateWhModal" class="px-3 py-2 bg-purple-600 hover:bg-purple-700 text-white text-sm font-bold rounded-lg transition-colors flex items-center gap-1 shadow-sm">
                <PlusIcon class="w-4 h-4"/> Tạo Kho Mới
              </button>
              <button @click="isWhModalOpen = false" class="text-gray-400 hover:text-red-500 text-2xl leading-none transition-colors">&times;</button>
            </div>
          </div>
          
          <div class="p-6 max-h-[70vh] overflow-y-auto custom-scrollbar">
            <div v-if="isWhLoading" class="text-center py-10 text-gray-500">Đang tải dữ liệu kho...</div>
            
            <div v-else-if="branchWarehouses.length === 0" class="text-center py-10">
              <ExclamationTriangleIcon class="w-12 h-12 text-amber-500 mx-auto mb-3" />
              <p class="font-bold text-gray-700 text-lg">Chi nhánh này chưa có Kho vật lý nào!</p>
              <button @click="openCreateWhModal" class="mt-4 px-5 py-2.5 bg-primary-600 text-white rounded-lg font-bold hover:bg-primary-700 shadow-md transition-transform active:scale-95">
                 + Khởi tạo Kho đầu tiên ngay
              </button>
            </div>

            <div v-else class="overflow-hidden border border-gray-200 rounded-lg shadow-sm">
              <table class="w-full text-sm text-left">
                <thead class="bg-gray-50 font-bold text-gray-700 border-b">
                  <tr>
                    <th class="p-3">Tên Kho</th>
                    <th class="p-3">Địa chỉ thực tế</th>
                    <th class="p-3 text-center">Hàng hóa (Của Kho này)</th>
                    <th class="p-3 text-center">Nhân sự</th>
                    <th class="p-3 text-center">Thao tác</th>
                  </tr>
                </thead>
                <tbody class="divide-y divide-gray-100">
                  <tr v-for="wh in branchWarehouses" :key="wh.warehouseId" 
                      :class="['transition-colors', (wh.hasManager && wh.employeeCount >= 10) ? 'bg-gray-100 opacity-60 grayscale' : 'hover:bg-purple-50']">
                    
                    <td class="p-3 font-bold text-gray-800">{{ wh.whname || wh.Whname }}</td>
                    
                    <td class="p-3">
                      <div class="text-xs text-gray-500 flex items-start gap-1">
                        <MapPinIcon class="w-3.5 h-3.5 mt-0.5 text-gray-400 shrink-0"/> 
                        <span class="line-clamp-2" :title="wh.whAddress || wh.WhAddress">{{ wh.whAddress || wh.WhAddress || 'Chưa cập nhật' }}</span>
                      </div>
                    </td>
                    
                    <td class="p-3 text-center">
                      <div class="flex justify-center gap-3 text-xs">
                        <div class="flex flex-col items-center" title="Số loại mặt hàng">
                          <CubeIcon class="w-4 h-4 text-blue-500 mb-0.5"/>
                          <span class="font-bold text-gray-700">{{ wh.itemCount || 0 }}</span>
                        </div>
                        <div class="flex flex-col items-center" title="Tổng số lượng tồn">
                          <ChartBarSquareIcon class="w-4 h-4 text-emerald-500 mb-0.5"/>
                          <span class="font-bold text-gray-700">{{ wh.totalQuantity || 0 }}</span>
                        </div>
                      </div>
                    </td>

                    <td class="p-3 text-center">
                      <div class="flex items-center justify-center gap-1 font-bold text-primary-700">
                        {{ wh.employeeCount || 0 }} / 10
                      </div>
                      <div class="text-[10px] text-gray-400 uppercase font-medium mt-0.5">
                        QL: <span :class="wh.hasManager ? 'text-gray-600' : 'text-red-500 italic'">{{ wh.managerName || 'Chưa có' }}</span>
                      </div>
                    </td>
                    
                    <td class="p-3 text-center space-x-1 whitespace-nowrap">
                      <button @click="openWhEmpModal(wh)" class="p-1.5 text-blue-600 hover:bg-blue-100 rounded-lg transition-colors border border-transparent hover:border-blue-200" title="Xem danh sách nhân viên">
                        <EyeIcon class="w-4 h-4 inline" />
                      </button>
                      <button @click="clearWarehouseEmployees(wh.warehouseId || wh.WarehouseId, wh.whname || wh.Whname)" class="p-1.5 text-orange-600 hover:bg-orange-100 rounded-lg transition-colors border border-transparent hover:border-orange-200" title="Gỡ TẤT CẢ nhân viên khỏi kho này">
                        <UserMinusIcon class="w-4 h-4 inline" />
                      </button>
                      <button @click="deleteWarehouse(wh.warehouseId || wh.WarehouseId, wh.whname || wh.Whname)" class="p-1.5 text-red-600 hover:bg-red-100 rounded-lg transition-colors border border-transparent hover:border-red-200" title="Xóa Kho này">
                        <TrashIcon class="w-4 h-4 inline" />
                      </button>
                    </td>
                  </tr>
                </tbody>
              </table>
            </div>
          </div>
          
          <div class="p-4 bg-gray-50 flex justify-end">
            <button @click="isWhModalOpen = false" class="px-5 py-2.5 bg-gray-600 hover:bg-gray-700 transition-colors text-white rounded-lg text-sm font-medium shadow-sm">Đóng bảng</button>
          </div>
        </div>
      </div>
    </Teleport>

    <Teleport to="body">
      <div v-if="isCreateWhModalOpen" class="fixed inset-0 z-[80] flex items-center justify-center px-4 bg-slate-900/60 backdrop-blur-sm animate-fade-in-fast">
        <div class="bg-white rounded-xl shadow-xl w-full max-w-sm overflow-hidden">
          <div class="px-4 py-3 border-b flex justify-between items-center bg-purple-600 text-white">
            <h4 class="font-bold text-sm flex items-center gap-2"><ArchiveBoxIcon class="w-4 h-4" /> Khởi tạo Kho mới</h4>
            <button @click="isCreateWhModalOpen = false" class="hover:text-red-200 transition-colors"><XMarkIcon class="w-5 h-5" /></button>
          </div>
          <div class="p-5">
            <div class="bg-purple-50 text-purple-700 p-2.5 rounded text-xs border border-purple-100 mb-4">
              Kho này sẽ được tự động liên kết với CN: <b class="font-bold">{{ selectedBranchName }}</b>
            </div>
            <form @submit.prevent="submitCreateWh" class="space-y-4">
              <div>
                <label class="block text-xs font-bold text-gray-700 mb-1">Tên kho *</label>
                <input v-model="whFormData.name" required type="text" class="w-full border border-gray-300 rounded-lg px-3 py-2.5 text-sm focus:ring-2 focus:ring-purple-500 outline-none" placeholder="VD: Kho Vật tư Tầng 1">
              </div>
              <div>
                <label class="block text-xs font-bold text-gray-700 mb-1">Địa chỉ thực tế của kho *</label>
                <textarea v-model="whFormData.address" required class="w-full border border-gray-300 rounded-lg px-3 py-2.5 text-sm h-24 focus:ring-2 focus:ring-purple-500 outline-none" placeholder="Nằm ở địa chỉ nào? (VD: Đường số 5, KCN...)"></textarea>
              </div>
              <div class="pt-2 flex justify-end gap-2">
                <button type="button" @click="isCreateWhModalOpen = false" class="px-4 py-2 border border-gray-300 rounded-lg text-sm font-medium hover:bg-gray-50 transition-colors">Hủy bỏ</button>
                <button type="submit" class="px-4 py-2 bg-purple-600 text-white rounded-lg text-sm font-bold hover:bg-purple-700 transition-colors shadow-md">Xác nhận tạo</button>
              </div>
            </form>
          </div>
        </div>
      </div>
    </Teleport>

    <Teleport to="body">
      <div v-if="isWhEmpModalOpen" class="fixed inset-0 z-[90] flex items-center justify-center bg-slate-900/60 backdrop-blur-sm p-4 animate-fade-in">
        <div class="bg-white rounded-xl shadow-2xl w-full max-w-2xl overflow-hidden">
          <div class="p-4 border-b flex justify-between items-center bg-blue-600 text-white">
            <h3 class="font-bold flex items-center gap-2 text-lg">
              <UserGroupIcon class="w-6 h-6"/> Nhân sự thuộc kho: {{ currentWhName }}
            </h3>
            <button @click="isWhEmpModalOpen = false" class="text-2xl leading-none hover:text-red-300 transition-colors">&times;</button>
          </div>
          
          <div class="p-6 max-h-[60vh] overflow-y-auto custom-scrollbar">
            <div v-if="isWhEmpLoading" class="text-center py-8 text-gray-500 font-medium">Đang tải danh sách...</div>
            
            <div v-else-if="whEmployees.length === 0" class="text-center py-10">
              <UserIcon class="w-12 h-12 text-gray-300 mx-auto mb-2" />
              <p class="text-gray-500 italic font-medium">Kho này hiện tại trống không, chưa có ai canh gác.</p>
            </div>
            
            <table v-else class="w-full text-sm text-left border rounded-lg overflow-hidden">
              <thead class="bg-gray-50 font-bold text-gray-700 border-b">
                <tr>
                  <th class="p-3">Mã NV</th>
                  <th class="p-3">Họ và tên</th>
                  <th class="p-3">SĐT</th>
                  <th class="p-3 text-center">Quyền</th>
                </tr>
              </thead>
              <tbody class="divide-y divide-gray-100">
                <tr v-for="emp in whEmployees" :key="emp.employeeId" :class="emp.roleCode === 'ql_kho' ? 'bg-amber-50 font-medium' : 'hover:bg-gray-50'">
                  <td class="p-3">{{ emp.empCode }}</td>
                  <td class="p-3">
                    {{ emp.fullName }} 
                    <span v-if="emp.roleCode === 'ql_kho'" class="text-[10px] bg-amber-200 text-amber-800 px-1.5 py-0.5 rounded ml-1 font-bold">Quản lý</span>
                  </td>
                  <td class="p-3 text-gray-600">{{ emp.phone }}</td>
                  <td class="p-3 text-center text-purple-600 font-bold uppercase text-xs">{{ emp.roleCode }}</td>
                </tr>
              </tbody>
            </table>
          </div>
          
          <div class="p-4 bg-gray-50 text-right">
            <button @click="isWhEmpModalOpen = false" class="px-5 py-2.5 bg-gray-600 hover:bg-gray-700 transition-colors text-white rounded-lg text-sm font-bold shadow-sm">Đóng</button>
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
.animate-fade-in { animation: fadeIn 0.2s ease-out; }
.animate-fade-in-fast { animation: fadeIn 0.1s ease-out; }
@keyframes fadeIn { from { opacity: 0; transform: scale(0.95); } to { opacity: 1; transform: scale(1); } }
.slide-fade-enter-active { transition: all 0.3s ease-out; }
.slide-fade-leave-active { transition: all 0.3s cubic-bezier(1, 0.5, 0.8, 1); }
.slide-fade-enter-from, .slide-fade-leave-to { transform: translateX(50px); opacity: 0; }
</style>