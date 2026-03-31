<script setup>
import { ref, computed, onMounted } from 'vue'
import { useAuth } from '../composables/useAuth' 
import { 
  PlusIcon, LockClosedIcon, LockOpenIcon, EyeIcon,
  UserCircleIcon, IdentificationIcon, UserIcon,
  CheckCircleIcon, XCircleIcon, MagnifyingGlassIcon,
  MapPinIcon, HomeModernIcon, XMarkIcon
} from '@heroicons/vue/24/outline'

const { currentUserRole } = useAuth() 

const API_URL = 'https://localhost:7139/api/Employees' 

const employees = ref([])
const workplaces = ref({ branches: [], warehouses: [] })
const isLoading = ref(false)
const searchQuery = ref('') 

const filteredEmployees = computed(() => {
  if (!searchQuery.value) return employees.value
  const query = searchQuery.value.toLowerCase()
  return employees.value.filter(emp => 
    (emp.fullName && emp.fullName.toLowerCase().includes(query)) ||
    (emp.empCode && emp.empCode.toLowerCase().includes(query)) ||
    (emp.username && emp.username.toLowerCase().includes(query)) ||
    (emp.phone && emp.phone.includes(query))
  )
})

const toast = ref({ show: false, message: '', type: 'success' })
const showToast = (message, type = 'success') => {
  toast.value = { show: true, message, type }
  setTimeout(() => { toast.value.show = false }, 3000) 
}

// --- MODAL THÊM MỚI ---
const isAddModalOpen = ref(false)
const formData = ref({
  fullName: '', phone: '', email: '', departmentId: 1, titleId: 1, 
  username: '', password: '', roleCode: 'nv_kho', 
  branchId: null, warehouseId: null 
})

const availableWarehouses = computed(() => {
  if (!formData.value.branchId) return []
  return workplaces.value.warehouses.filter(w => w.branchId === formData.value.branchId)
})

// --- MODAL XEM CHI TIẾT ---
const isDetailModalOpen = ref(false)
const selectedEmployee = ref(null)

const roleOptions = [
  { value: 'admin', label: 'Quản trị viên' },
  { value: 'giam_doc', label: 'Giám đốc' },
  { value: 'gd_chi_nhanh', label: 'GĐ Chi nhánh' },
  { value: 'ql_kho', label: 'Quản lý kho' },
  { value: 'nv_kho', label: 'Nhân viên kho' }
]

// =========================================================================
// --- TÍNH NĂNG MỚI: MODAL GÁN LẠI VỊ TRÍ ---
// =========================================================================
const isAssignModalOpen = ref(false)
const assignFormData = ref({ employeeId: null, fullName: '', roleCode: '', branchId: null, warehouseId: null })

const availableWarehousesForAssign = computed(() => {
  if (!assignFormData.value.branchId) return []
  return workplaces.value.warehouses.filter(w => w.branchId === assignFormData.value.branchId)
})

const openAssignModal = (emp) => {
  assignFormData.value = {
    employeeId: emp.employeeId,
    fullName: emp.fullName,
    roleCode: emp.roleCode,
    branchId: emp.branchId || null,
    warehouseId: emp.warehouseId || null
  }
  isAssignModalOpen.value = true
}

const submitAssignWorkplace = async () => {
  // Bắt lỗi rỗng
  if (['gd_chi_nhanh', 'ql_kho', 'nv_kho'].includes(assignFormData.value.roleCode) && !assignFormData.value.branchId) {
    showToast('Sếp vui lòng chọn Chi nhánh!', 'error')
    return
  }
  if (['ql_kho', 'nv_kho'].includes(assignFormData.value.roleCode) && !assignFormData.value.warehouseId) {
    showToast('Sếp vui lòng chọn Kho!', 'error')
    return
  }

  try {
    const res = await fetch(`${API_URL}/${assignFormData.value.employeeId}/assign-workplace`, {
      method: 'PUT',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({
        branchId: assignFormData.value.branchId,
        warehouseId: assignFormData.value.warehouseId
      })
    })
    if (res.ok) {
      showToast('Đã gán vị trí thành công!', 'success')
      isAssignModalOpen.value = false
      fetchData()
    } else {
      showToast('Lỗi khi gán vị trí!', 'error')
    }
  } catch (error) { showToast('Lỗi máy chủ!', 'error') }
}
// =========================================================================

const fetchData = async () => {
  isLoading.value = true
  try {
    const [empRes, workRes] = await Promise.all([ fetch(API_URL), fetch(`${API_URL}/workplaces`) ])
    if (empRes.ok) employees.value = await empRes.json()
    if (workRes.ok) workplaces.value = await workRes.json()
  } catch (error) {
    showToast('Không thể kết nối tải dữ liệu.', 'error')
  } finally { isLoading.value = false }
}

const submitForm = async () => {
  if (!formData.value.fullName || !formData.value.username || !formData.value.password) {
    showToast('Vui lòng nhập đủ Họ Tên, Tài khoản và Mật khẩu!', 'error');
    return;
  }
  
  if (['gd_chi_nhanh', 'ql_kho', 'nv_kho'].includes(formData.value.roleCode) && !formData.value.branchId) {
    showToast('Sếp vui lòng chọn Chi nhánh làm việc!', 'error'); return;
  }
  if (['ql_kho', 'nv_kho'].includes(formData.value.roleCode) && !formData.value.warehouseId) {
    showToast('Sếp vui lòng chọn Kho làm việc!', 'error'); return;
  }

  try {
    const response = await fetch(API_URL, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(formData.value)
    })
    
    if (response.ok) {
      const result = await response.json()
      showToast(result.message, 'success') 
      isAddModalOpen.value = false
      formData.value = { fullName: '', phone: '', email: '', departmentId: 1, titleId: 1, username: '', password: '', roleCode: 'nv_kho', branchId: null, warehouseId: null }
      fetchData()
    } else {
      const errorData = await response.json()
      showToast('Lỗi: ' + errorData.message, 'error') 
    }
  } catch (error) { showToast('Lỗi máy chủ Backend.', 'error') }
}

const toggleStatus = async (employeeId) => {
  if(!confirm('Sếp có chắc muốn thay đổi trạng thái tài khoản này?')) return
  try {
    const response = await fetch(`${API_URL}/toggle-status/${employeeId}`, { method: 'PUT' })
    if (response.ok) {
      const result = await response.json()
      showToast(result.message, 'success')
      fetchData()
    }
  } catch (error) { showToast('Lỗi kết nối!', 'error') }
}

const removeWorkplace = async (employeeId, empName) => {
  if (!confirm(`Sếp có chắc muốn gỡ "${empName}" ra khỏi vị trí làm việc hiện tại?`)) return
  try {
    const res = await fetch(`${API_URL}/remove-workplace/${employeeId}`, { method: 'PUT' })
    if (res.ok) {
      showToast('Đã gỡ nhân viên khỏi vị trí làm việc!', 'success')
      fetchData()
    } else { showToast('Lỗi khi gỡ vị trí!', 'error') }
  } catch (error) { showToast('Lỗi máy chủ!', 'error') }
}

const openDetail = (emp) => {
  selectedEmployee.value = emp
  isDetailModalOpen.value = true
}

onMounted(() => { fetchData() })
</script>

<template>
  <div class="p-4 md:p-6 bg-gray-50 min-h-screen relative overflow-hidden">
    
    <Transition name="slide-fade">
      <div v-if="toast.show" class="fixed top-6 right-6 z-[100] flex items-center p-4 mb-4 text-gray-900 bg-white rounded-lg shadow-xl border-l-4" :class="toast.type === 'success' ? 'border-green-500' : 'border-red-500'">
        <div class="inline-flex items-center justify-center flex-shrink-0 w-8 h-8 rounded-lg" :class="toast.type === 'success' ? 'text-green-500 bg-green-100' : 'text-red-500 bg-red-100'">
          <CheckCircleIcon v-if="toast.type === 'success'" class="w-6 h-6" />
          <XCircleIcon v-else class="w-6 h-6" />
        </div>
        <div class="ml-3 text-sm font-medium mr-4">{{ toast.message }}</div>
      </div>
    </Transition>

    <div class="flex flex-col md:flex-row justify-between items-start md:items-center mb-6 gap-4">
      <h2 class="text-2xl font-bold text-gray-800 flex items-center gap-2">
        <UserCircleIcon class="w-8 h-8 text-primary-600" /> Quản lý Nhân sự & Phân quyền
      </h2>
      
      <div class="flex items-center gap-3 w-full md:w-auto">
        <div class="relative w-full md:w-64">
          <div class="absolute inset-y-0 left-0 flex items-center pl-3 pointer-events-none">
            <MagnifyingGlassIcon class="w-5 h-5 text-gray-400" />
          </div>
          <input v-model="searchQuery" type="text" class="bg-white border border-gray-300 text-gray-900 text-sm rounded-lg focus:ring-primary-500 focus:border-primary-500 block w-full pl-10 p-2.5 shadow-sm" placeholder="Tìm mã, tên, SĐT...">
        </div>

        <button v-if="currentUserRole === 'admin'" @click="isAddModalOpen = true" class="bg-primary-600 hover:bg-primary-700 text-white px-4 py-2.5 rounded-lg font-medium shadow flex items-center gap-2 transition-colors whitespace-nowrap">
          <PlusIcon class="w-5 h-5" /> Thêm NV
        </button>
      </div>
    </div>

    <div class="bg-white rounded-xl shadow-sm border border-gray-200 overflow-hidden">
      <div class="overflow-x-auto">
        <table class="w-full text-left border-collapse">
          <thead>
            <tr class="bg-gray-100 text-gray-600 text-sm uppercase tracking-wider">
              <th class="p-4 font-semibold">Mã NV</th>
              <th class="p-4 font-semibold">Họ và Tên</th>
              <th class="p-4 font-semibold">Vị trí làm việc</th>
              <th class="p-4 font-semibold">Tài khoản</th>
              <th class="p-4 font-semibold">Mã Quyền</th>
              <th class="p-4 font-semibold text-center">Trạng thái</th>
              <th class="p-4 font-semibold text-center">Thao tác</th>
            </tr>
          </thead>
          <tbody class="divide-y divide-gray-200">
            <tr v-if="isLoading"><td colspan="7" class="p-8 text-center text-gray-500">Đang tải dữ liệu...</td></tr>
            <tr v-else-if="filteredEmployees.length === 0"><td colspan="7" class="p-8 text-center text-gray-500">Chưa có dữ liệu nhân sự.</td></tr>
            <tr v-else v-for="emp in filteredEmployees" :key="emp.employeeId" class="hover:bg-gray-50 transition-colors">
              <td class="p-4 text-gray-800 font-medium">{{ emp.empCode }}</td>
              <td class="p-4 text-gray-800">{{ emp.fullName }}</td>
              
              <td class="p-4">
                <div class="flex flex-col gap-1">
                  <div v-if="emp.branchName" class="flex items-center gap-1.5 text-blue-700 text-sm font-medium">
                    <HomeModernIcon class="w-4 h-4" /> {{ emp.branchName }}
                  </div>
                  <div v-if="emp.warehouseName" class="flex items-center gap-1.5 text-gray-500 text-xs">
                    <MapPinIcon class="w-3.5 h-3.5" /> {{ emp.warehouseName }}
                  </div>
                  <div v-if="!emp.branchName && !emp.warehouseName" class="text-gray-400 text-sm italic">Chưa gán vị trí</div>
                </div>
              </td>

              <td class="p-4 text-gray-600">
                <span v-if="emp.username !== 'Chưa cấp TK'" class="bg-blue-100 text-blue-700 px-2 py-1 rounded-md text-xs font-bold">{{ emp.username }}</span>
                <span v-else class="text-gray-400 italic">{{ emp.username }}</span>
              </td>
              <td class="p-4 text-gray-600"><span class="bg-purple-100 text-purple-700 px-2 py-1 rounded-md text-xs font-bold">{{ emp.roleCode }}</span></td>
              <td class="p-4 text-center">
                <span :class="emp.isActive ? 'bg-green-100 text-green-700' : 'bg-red-100 text-red-700'" class="px-3 py-1 rounded-full text-xs font-bold">
                  {{ emp.isActive ? 'Đang hoạt động' : 'Đã khóa' }}
                </span>
              </td>
              
              <td class="p-4 flex justify-center gap-1">
                <button 
                  v-if="['gd_chi_nhanh', 'ql_kho', 'nv_kho'].includes(emp.roleCode) && currentUserRole === 'admin'" 
                  @click="openAssignModal(emp)" 
                  title="Gán vị trí làm việc" 
                  class="text-orange-500 hover:bg-orange-100 p-2 rounded-lg transition-colors border border-transparent hover:border-orange-200"
                >
                  <MapPinIcon class="w-5 h-5" />
                </button>

                <button 
                  v-if="(emp.branchName || emp.warehouseName) && currentUserRole === 'admin'" 
                  @click="removeWorkplace(emp.employeeId, emp.fullName)" 
                  title="Gỡ khỏi vị trí làm việc" 
                  class="text-red-500 hover:bg-red-100 p-2 rounded-lg transition-colors border border-transparent hover:border-red-200"
                >
                  <XMarkIcon class="w-5 h-5" />
                </button>

                <button @click="openDetail(emp)" title="Xem chi tiết" class="text-blue-500 hover:bg-blue-50 p-2 rounded-lg transition-colors border border-transparent hover:border-current">
                  <EyeIcon class="w-5 h-5" />
                </button>
                
                <button v-if="currentUserRole === 'admin'" @click="toggleStatus(emp.employeeId)" :title="emp.isActive ? 'Khóa tài khoản' : 'Mở khóa tài khoản'" :class="emp.isActive ? 'text-amber-500 hover:bg-amber-50' : 'text-green-500 hover:bg-green-50'" class="p-2 rounded-lg transition-colors border border-transparent hover:border-current">
                  <LockClosedIcon v-if="emp.isActive" class="w-5 h-5" />
                  <LockOpenIcon v-else class="w-5 h-5" />
                </button>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>

    <Teleport to="body">
      <div v-if="isAssignModalOpen" class="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-[80] p-4 animate-fade-in">
        <div class="bg-white rounded-xl shadow-2xl w-full max-w-sm overflow-hidden">
          <div class="bg-orange-500 p-4 flex justify-between items-center text-white">
            <h3 class="font-bold text-lg flex items-center gap-2"><MapPinIcon class="w-6 h-6" /> Điều động Nhân sự</h3>
            <button @click="isAssignModalOpen = false" class="text-white hover:text-gray-200 text-2xl leading-none">&times;</button>
          </div>
          <div class="p-5">
            <div class="mb-4">
              <p class="text-sm text-gray-500">Nhân viên:</p>
              <p class="font-bold text-gray-800 text-lg">{{ assignFormData.fullName }}</p>
            </div>

            <div class="space-y-4">
              <div>
                <label class="block text-sm font-bold text-blue-700 mb-1">Gán vào Chi nhánh *</label>
                <select v-model="assignFormData.branchId" class="w-full border border-blue-300 rounded-lg p-2.5 focus:ring-2 focus:ring-blue-500 bg-white">
                  <option :value="null">-- Chọn Chi nhánh --</option>
                  <option v-for="b in workplaces.branches" :key="b.branchId" :value="b.branchId">{{ b.branchName }}</option>
                </select>
              </div>

              <div v-if="['ql_kho', 'nv_kho'].includes(assignFormData.roleCode) && assignFormData.branchId" class="animate-fade-in">
                <label class="block text-sm font-bold text-orange-600 mb-1">Gán vào Kho *</label>
                <select v-model="assignFormData.warehouseId" class="w-full border border-orange-300 rounded-lg p-2.5 focus:ring-2 focus:ring-orange-500 bg-white">
                  <option :value="null">-- Chọn Kho --</option>
                  <option v-for="w in availableWarehousesForAssign" :key="w.warehouseId" :value="w.warehouseId">{{ w.warehouseName }}</option>
                </select>
                <div v-if="availableWarehousesForAssign.length === 0" class="text-xs text-red-500 mt-1 italic">Chi nhánh này chưa có kho nào!</div>
              </div>
            </div>

            <div class="mt-6 pt-4 border-t flex justify-end gap-2">
              <button @click="isAssignModalOpen = false" class="px-4 py-2 border rounded-lg text-gray-600 hover:bg-gray-50 text-sm font-medium">Hủy</button>
              <button @click="submitAssignWorkplace" class="px-4 py-2 bg-orange-500 text-white rounded-lg text-sm font-bold hover:bg-orange-600 transition-colors">Lưu vị trí</button>
            </div>
          </div>
        </div>
      </div>
    </Teleport>

    <div v-if="isAddModalOpen" class="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50 p-4 animate-fade-in">
      <div class="bg-white rounded-xl shadow-2xl w-full max-w-4xl overflow-hidden">
        <div class="bg-primary-600 p-4 flex justify-between items-center text-white">
          <h3 class="font-bold text-lg flex items-center gap-2"><IdentificationIcon class="w-6 h-6" /> Thêm Nhân viên mới</h3>
          <button @click="isAddModalOpen = false" class="text-white hover:text-gray-200 text-2xl leading-none">&times;</button>
        </div>
        
        <div class="p-6">
          <div class="bg-blue-50 text-blue-700 p-3 rounded-lg text-sm mb-5 border border-blue-200 flex items-center gap-2">
            <span class="font-bold">Lưu ý:</span> Mã nhân viên sẽ được tự động sinh ra dựa trên Phân quyền.
          </div>

          <div class="grid grid-cols-1 md:grid-cols-3 gap-6">
            <div class="space-y-4">
              <h4 class="font-semibold text-gray-700 border-b pb-2 flex items-center gap-2"><UserIcon class="w-4 h-4"/> Thông tin cá nhân</h4>
              <div><label class="block text-sm text-gray-600 mb-1">Họ và Tên *</label><input v-model="formData.fullName" type="text" class="w-full border border-gray-300 rounded-lg p-2.5"></div>
              <div><label class="block text-sm text-gray-600 mb-1">Số điện thoại</label><input v-model="formData.phone" type="tel" class="w-full border border-gray-300 rounded-lg p-2.5"></div>
              <div><label class="block text-sm text-gray-600 mb-1">Email</label><input v-model="formData.email" type="email" class="w-full border border-gray-300 rounded-lg p-2.5"></div>
            </div>

            <div class="space-y-4">
              <h4 class="font-semibold text-gray-700 border-b pb-2 flex items-center gap-2"><LockClosedIcon class="w-4 h-4"/> Thông tin Tài khoản</h4>
              <div><label class="block text-sm text-gray-600 mb-1">Tên đăng nhập *</label><input v-model="formData.username" type="text" class="w-full border border-gray-300 rounded-lg p-2.5"></div>
              <div><label class="block text-sm text-gray-600 mb-1">Mật khẩu *</label><input v-model="formData.password" type="password" class="w-full border border-gray-300 rounded-lg p-2.5"></div>
              <div>
                <label class="block text-sm font-bold text-purple-700 mb-1">Phân quyền *</label>
                <select v-model="formData.roleCode" class="w-full border-2 border-purple-300 rounded-lg p-2.5 bg-white text-purple-900 font-medium">
                  <option v-for="role in roleOptions" :key="role.value" :value="role.value">{{ role.label }} ({{ role.value }})</option>
                </select>
              </div>
            </div>

            <div class="space-y-4 bg-gray-50 p-4 rounded-xl border border-gray-200">
              <h4 class="font-semibold text-gray-700 border-b border-gray-300 pb-2 flex items-center gap-2"><MapPinIcon class="w-4 h-4"/> Vị trí làm việc</h4>
              <div v-if="['admin', 'giam_doc'].includes(formData.roleCode)" class="text-sm text-gray-500 italic mt-4 text-center">Quyền quản trị không gán cố định.</div>
              <div v-if="['gd_chi_nhanh', 'ql_kho', 'nv_kho'].includes(formData.roleCode)" class="mt-2 space-y-4 animate-fade-in">
                <div>
                  <label class="block text-sm font-bold text-blue-700 mb-1">Chọn Chi nhánh *</label>
                  <select v-model="formData.branchId" class="w-full border border-blue-300 rounded-lg p-2.5 bg-white">
                    <option :value="null">-- Chọn Chi nhánh --</option>
                    <option v-for="b in workplaces.branches" :key="b.branchId" :value="b.branchId">{{ b.branchName }}</option>
                  </select>
                </div>
                <div v-if="['ql_kho', 'nv_kho'].includes(formData.roleCode) && formData.branchId" class="animate-fade-in">
                  <label class="block text-sm font-bold text-orange-600 mb-1">Chọn Kho *</label>
                  <select v-model="formData.warehouseId" class="w-full border border-orange-300 rounded-lg p-2.5 bg-white">
                    <option :value="null">-- Chọn Kho --</option>
                    <option v-for="w in availableWarehouses" :key="w.warehouseId" :value="w.warehouseId">{{ w.warehouseName }}</option>
                  </select>
                  <div v-if="availableWarehouses.length === 0" class="text-xs text-red-500 mt-1 italic">Chi nhánh chưa có kho!</div>
                </div>
              </div>
            </div>
          </div>

          <div class="pt-5 flex justify-end gap-3 border-t mt-6">
            <button @click="isAddModalOpen = false" class="px-6 py-2.5 text-gray-600 bg-gray-100 hover:bg-gray-200 rounded-lg font-medium transition-colors">Hủy bỏ</button>
            <button @click="submitForm" class="px-6 py-2.5 text-white bg-primary-600 hover:bg-primary-700 rounded-lg font-medium">Xác nhận Lưu</button>
          </div>
        </div>
      </div>
    </div>

    <div v-if="isDetailModalOpen" class="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50 p-4 animate-fade-in">
      <div class="bg-white rounded-xl shadow-2xl w-full max-w-md overflow-hidden">
        <div class="bg-gray-100 p-4 flex justify-between items-center border-b">
          <h3 class="font-bold text-gray-800 text-lg flex items-center gap-2"><UserIcon class="w-6 h-6 text-primary-600" /> Hồ sơ Nhân sự</h3>
          <button @click="isDetailModalOpen = false" class="text-gray-500 hover:text-gray-800 text-2xl leading-none">&times;</button>
        </div>
        
        <div class="p-6 space-y-4" v-if="selectedEmployee">
          <div class="flex justify-between items-center border-b pb-2"><span class="text-gray-500 text-sm">Họ và tên:</span><span class="font-bold text-gray-800">{{ selectedEmployee.fullName }}</span></div>
          <div class="flex justify-between items-center border-b pb-2"><span class="text-gray-500 text-sm">Mã Nhân viên:</span><span class="font-bold text-primary-600">{{ selectedEmployee.empCode }}</span></div>
          <div class="flex justify-between items-center border-b pb-2">
            <span class="text-gray-500 text-sm">Nơi làm việc:</span>
            <div class="text-right">
              <div v-if="selectedEmployee.branchName" class="font-bold text-blue-700">{{ selectedEmployee.branchName }}</div>
              <div v-if="selectedEmployee.warehouseName" class="text-xs font-medium text-gray-500 mt-0.5">{{ selectedEmployee.warehouseName }}</div>
              <div v-if="!selectedEmployee.branchName" class="font-medium text-gray-400 italic">Chưa gán vị trí</div>
            </div>
          </div>
          <div class="flex justify-between items-center border-b pb-2"><span class="text-gray-500 text-sm">Số điện thoại:</span><span class="font-medium text-gray-800">{{ selectedEmployee.phone || 'Chưa cập nhật' }}</span></div>
          <div class="flex justify-between items-center border-b pb-2"><span class="text-gray-500 text-sm">Email liên hệ:</span><span class="font-medium text-gray-800">{{ selectedEmployee.email || 'Chưa cập nhật' }}</span></div>
          <div class="flex justify-between items-center border-b pb-2"><span class="text-gray-500 text-sm">Tài khoản:</span><span class="font-medium text-gray-800">{{ selectedEmployee.username }}</span></div>
          <div class="flex justify-between items-center pb-2"><span class="text-gray-500 text-sm">Phân quyền:</span><span class="font-medium text-purple-600">{{ selectedEmployee.roleCode }}</span></div>
        </div>
        <div class="p-4 bg-gray-50 text-right"><button @click="isDetailModalOpen = false" class="px-5 py-2 text-white bg-gray-600 rounded-lg">Đóng lại</button></div>
      </div>
    </div>

  </div>
</template>

<style scoped>
.animate-fade-in { animation: fadeIn 0.2s ease-out; }
@keyframes fadeIn { from { opacity: 0; transform: scale(0.95); } to { opacity: 1; transform: scale(1); } }
.slide-fade-enter-active { transition: all 0.3s ease-out; }
.slide-fade-leave-active { transition: all 0.3s cubic-bezier(1, 0.5, 0.8, 1); }
.slide-fade-enter-from, .slide-fade-leave-to { transform: translateX(50px); opacity: 0; }
</style>