<script setup>
import { ref, computed, onMounted } from 'vue'
import { 
  MagnifyingGlassIcon, FunnelIcon, ArrowPathIcon, EyeIcon, 
  CheckCircleIcon, XCircleIcon, ClockIcon, DocumentTextIcon,
  ChevronLeftIcon, ChevronRightIcon, XMarkIcon
} from '@heroicons/vue/24/outline'

// API Tương lai
const API_URL = 'https://localhost:7139/api/Approvals'

const isLoading = ref(false)
const searchQuery = ref('')
const filterType = ref('')
const activeTab = ref('PENDING') // PENDING, APPROVED, REJECTED

const currentPage = ref(1)
const itemsPerPage = 8
const isModalOpen = ref(false)
const selectedDoc = ref(null)

// --- DỮ LIỆU MẪU (MOCK DATA) ---
const mockData = ref([
  { id: 'PO-202603-001', type: 'PURCHASE_ORDER', typeName: 'Đơn đặt hàng (PO)', submitter: 'Nguyễn Văn A (Kho NV)', date: '2026-03-31T08:30:00', totalValue: '150,000,000 đ', status: 'PENDING', note: 'Nhập hàng đợt 1 tháng 4' },
  { id: 'TR-202603-042', type: 'TRANSFER', typeName: 'Điều chuyển kho', submitter: 'Trần Thị B (Kho TT)', date: '2026-03-31T09:15:00', totalValue: '500 SP', status: 'PENDING', note: 'Chuyển hàng từ Tổng về Chi nhánh 1' },
  { id: 'IN-202603-088', type: 'INBOUND', typeName: 'Phiếu Nhập Kho', submitter: 'Lê Văn C (Kho TT)', date: '2026-03-31T10:00:00', totalValue: '1,200 SP', status: 'PENDING', note: 'Nhập lô hàng linh kiện điện tử' },
  { id: 'PO-202603-002', type: 'PURCHASE_ORDER', typeName: 'Đơn đặt hàng (PO)', submitter: 'Nguyễn Văn A', date: '2026-03-30T14:20:00', totalValue: '45,000,000 đ', status: 'APPROVED', note: 'Bổ sung văn phòng phẩm' },
  { id: 'OUT-202603-105', type: 'OUTBOUND', typeName: 'Phiếu Xuất Kho', submitter: 'Trần Thị B', date: '2026-03-30T16:45:00', totalValue: '200 SP', status: 'REJECTED', note: 'Xuất hàng cho đối tác XYZ (Sai số lượng)' },
])

const stats = computed(() => {
  return {
    pending: mockData.value.filter(x => x.status === 'PENDING').length,
    approved: mockData.value.filter(x => x.status === 'APPROVED').length,
    rejected: mockData.value.filter(x => x.status === 'REJECTED').length
  }
})

const fetchApprovals = async () => {
  isLoading.value = true
  setTimeout(() => { isLoading.value = false }, 500)
}

// --- LOGIC LỌC & PHÂN TRANG ---
const filteredDocs = computed(() => {
  let result = mockData.value.filter(x => x.status === activeTab.value)
  
  if (searchQuery.value) {
    const q = searchQuery.value.toLowerCase()
    result = result.filter(doc => 
      doc.id.toLowerCase().includes(q) || 
      doc.submitter.toLowerCase().includes(q)
    )
  }
  if (filterType.value) {
    result = result.filter(doc => doc.type === filterType.value)
  }
  currentPage.value = 1 
  return result
})

const totalPages = computed(() => Math.ceil(filteredDocs.value.length / itemsPerPage))
const paginatedDocs = computed(() => filteredDocs.value.slice((currentPage.value - 1) * itemsPerPage, currentPage.value * itemsPerPage))

const changePage = (page) => {
  if (page >= 1 && page <= totalPages.value) currentPage.value = page
}

// --- THAO TÁC DUYỆT / TỪ CHỐI ---
const approveDoc = (id) => {
  if(confirm(`Bạn có chắc chắn muốn DUYỆT phiếu ${id} không?`)) {
    const index = mockData.value.findIndex(x => x.id === id)
    if(index !== -1) mockData.value[index].status = 'APPROVED'
  }
}

const rejectDoc = (id) => {
  const reason = prompt(`Nhập lý do TỪ CHỐI phiếu ${id}:`)
  if(reason !== null) {
    const index = mockData.value.findIndex(x => x.id === id)
    if(index !== -1) {
      mockData.value[index].status = 'REJECTED'
      mockData.value[index].note = reason // Lưu lý do vào note
    }
  }
}

const viewDetails = (doc) => {
  selectedDoc.value = doc
  isModalOpen.value = true
}

const formatDateTime = (dateString) => {
  const date = new Date(dateString)
  return new Intl.DateTimeFormat('vi-VN', { day: '2-digit', month: '2-digit', year: 'numeric', hour: '2-digit', minute: '2-digit' }).format(date)
}

onMounted(() => fetchApprovals())
</script>

<template>
  <div class="space-y-5 md:space-y-6 animate-fade-in pb-10 px-0 md:px-1 relative">
    
    <div class="flex flex-col sm:flex-row sm:items-center justify-between gap-4">
      <div>
        <h2 class="text-xl md:text-2xl font-bold text-gray-800 flex items-center gap-2">
          <CheckCircleIcon class="w-7 h-7 text-primary-600" /> Trung tâm Duyệt phiếu
        </h2>
        <p class="text-xs md:text-sm text-gray-500 mt-1">Nơi phê duyệt tập trung tất cả các yêu cầu từ các bộ phận</p>
      </div>
      <button @click="fetchApprovals" class="bg-white border border-gray-200 hover:bg-gray-50 text-gray-700 px-4 py-2 h-10 rounded-lg flex items-center gap-2 text-sm font-medium transition-colors shadow-sm">
        <ArrowPathIcon class="w-5 h-5" :class="{'animate-spin': isLoading}"/> Tải lại
      </button>
    </div>

    <div class="grid grid-cols-1 sm:grid-cols-3 gap-4">
      <div class="bg-white p-4 rounded-xl border border-gray-200 shadow-sm flex items-center gap-4 border-l-4 border-l-amber-400">
        <div class="p-3 bg-amber-50 text-amber-600 rounded-full"><ClockIcon class="w-6 h-6"/></div>
        <div>
          <p class="text-sm font-medium text-gray-500">Đang chờ duyệt</p>
          <p class="text-2xl font-bold text-gray-800">{{ stats.pending }} <span class="text-sm font-normal text-gray-500">phiếu</span></p>
        </div>
      </div>
      <div class="bg-white p-4 rounded-xl border border-gray-200 shadow-sm flex items-center gap-4 border-l-4 border-l-green-500">
        <div class="p-3 bg-green-50 text-green-600 rounded-full"><CheckCircleIcon class="w-6 h-6"/></div>
        <div>
          <p class="text-sm font-medium text-gray-500">Đã phê duyệt</p>
          <p class="text-2xl font-bold text-gray-800">{{ stats.approved }}</p>
        </div>
      </div>
      <div class="bg-white p-4 rounded-xl border border-gray-200 shadow-sm flex items-center gap-4 border-l-4 border-l-red-500">
        <div class="p-3 bg-red-50 text-red-600 rounded-full"><XCircleIcon class="w-6 h-6"/></div>
        <div>
          <p class="text-sm font-medium text-gray-500">Đã từ chối</p>
          <p class="text-2xl font-bold text-gray-800">{{ stats.rejected }}</p>
        </div>
      </div>
    </div>

    <div class="bg-white p-2 rounded-xl border border-gray-200 shadow-sm flex flex-col md:flex-row md:items-center justify-between gap-3">
      <div class="flex p-1 bg-gray-100 rounded-lg w-full md:w-auto">
        <button @click="activeTab = 'PENDING'" :class="['flex-1 md:flex-none px-4 py-2 text-sm font-medium rounded-md transition-all', activeTab === 'PENDING' ? 'bg-white text-amber-600 shadow' : 'text-gray-500 hover:text-gray-700']">
          Chờ duyệt ({{ stats.pending }})
        </button>
        <button @click="activeTab = 'APPROVED'" :class="['flex-1 md:flex-none px-4 py-2 text-sm font-medium rounded-md transition-all', activeTab === 'APPROVED' ? 'bg-white text-green-600 shadow' : 'text-gray-500 hover:text-gray-700']">
          Đã duyệt
        </button>
        <button @click="activeTab = 'REJECTED'" :class="['flex-1 md:flex-none px-4 py-2 text-sm font-medium rounded-md transition-all', activeTab === 'REJECTED' ? 'bg-white text-red-600 shadow' : 'text-gray-500 hover:text-gray-700']">
          Từ chối
        </button>
      </div>

      <div class="flex gap-2 w-full md:w-auto px-1 md:px-0 pb-1 md:pb-0">
        <div class="relative flex-1 md:w-64">
          <MagnifyingGlassIcon class="absolute inset-y-0 left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-gray-400" />
          <input v-model="searchQuery" type="text" class="w-full h-9 pl-9 pr-3 border border-gray-200 rounded-lg text-sm focus:ring-1 focus:ring-blue-500 outline-none" placeholder="Tìm mã phiếu, người tạo...">
        </div>
        <select v-model="filterType" class="h-9 border border-gray-200 rounded-lg text-sm px-2 focus:ring-1 focus:ring-blue-500 outline-none bg-white">
          <option value="">Loại phiếu</option>
          <option value="PURCHASE_ORDER">Đặt hàng (PO)</option>
          <option value="INBOUND">Phiếu Nhập</option>
          <option value="OUTBOUND">Phiếu Xuất</option>
          <option value="TRANSFER">Điều chuyển</option>
        </select>
      </div>
    </div>

    <div class="bg-white rounded-xl border border-gray-200 shadow-sm overflow-hidden flex flex-col">
      <div class="w-full overflow-x-auto custom-scrollbar flex-1">
        <table class="min-w-[1000px] w-full divide-y divide-gray-200">
          <thead class="bg-gray-50">
            <tr>
              <th class="px-5 py-4 text-left text-xs font-semibold text-gray-600 uppercase tracking-wider w-[12%]">Mã phiếu</th>
              <th class="px-5 py-4 text-left text-xs font-semibold text-gray-600 uppercase tracking-wider w-[15%]">Loại chứng từ</th>
              <th class="px-5 py-4 text-left text-xs font-semibold text-gray-600 uppercase tracking-wider w-[18%]">Người yêu cầu</th>
              <th class="px-5 py-4 text-left text-xs font-semibold text-gray-600 uppercase tracking-wider w-[15%]">Ngày tạo</th>
              <th class="px-5 py-4 text-left text-xs font-semibold text-gray-600 uppercase tracking-wider w-[12%]">Giá trị/SL</th>
              <th class="px-5 py-4 text-left text-xs font-semibold text-gray-600 uppercase tracking-wider w-[18%]">Ghi chú</th>
              <th class="px-5 py-4 text-center text-xs font-semibold text-gray-600 uppercase tracking-wider w-[10%]">Thao tác</th>
            </tr>
          </thead>
          <tbody class="divide-y divide-gray-100 bg-white">
            <tr v-if="paginatedDocs.length === 0">
              <td colspan="7" class="px-6 py-16 text-center">
                <DocumentTextIcon class="w-12 h-12 text-gray-300 mx-auto mb-3" />
                <h3 class="text-base font-medium text-gray-900">Không có dữ liệu</h3>
                <p class="text-sm text-gray-500 mt-1">Không tìm thấy phiếu nào trong mục này.</p>
              </td>
            </tr>
            
            <tr v-else v-for="doc in paginatedDocs" :key="doc.id" class="hover:bg-gray-50/50 transition-colors">
              <td class="px-5 py-4 text-sm font-bold text-blue-600 cursor-pointer hover:underline" @click="viewDetails(doc)">{{ doc.id }}</td>
              <td class="px-5 py-4 text-sm font-medium text-gray-800">
                <span class="bg-gray-100 text-gray-700 px-2 py-1 rounded text-xs">{{ doc.typeName }}</span>
              </td>
              <td class="px-5 py-4 text-sm text-gray-700">{{ doc.submitter }}</td>
              <td class="px-5 py-4 text-sm text-gray-500">{{ formatDateTime(doc.date) }}</td>
              <td class="px-5 py-4 text-sm font-semibold text-gray-800">{{ doc.totalValue }}</td>
              <td class="px-5 py-4 text-sm text-gray-500 truncate max-w-[150px]" :title="doc.note">{{ doc.note }}</td>
              <td class="px-5 py-4 text-center flex items-center justify-center gap-2">
                <template v-if="activeTab === 'PENDING'">
                  <button @click="approveDoc(doc.id)" title="Duyệt" class="text-green-600 hover:text-green-800 bg-green-50 hover:bg-green-100 p-1.5 rounded-md transition-colors">
                    <CheckCircleIcon class="w-5 h-5" />
                  </button>
                  <button @click="rejectDoc(doc.id)" title="Từ chối" class="text-red-600 hover:text-red-800 bg-red-50 hover:bg-red-100 p-1.5 rounded-md transition-colors">
                    <XCircleIcon class="w-5 h-5" />
                  </button>
                </template>
                <button @click="viewDetails(doc)" title="Xem chi tiết" class="text-gray-500 hover:text-blue-600 bg-gray-50 hover:bg-blue-50 p-1.5 rounded-md transition-colors">
                  <EyeIcon class="w-5 h-5" />
                </button>
              </td>
            </tr>
          </tbody>
        </table>
      </div>

      <div v-if="filteredDocs.length > 0" class="border-t border-gray-200 bg-gray-50 px-5 py-3 flex justify-between items-center">
        <span class="text-sm text-gray-600">Trang {{ currentPage }} / {{ totalPages || 1 }}</span>
        <div class="flex gap-2">
          <button @click="changePage(currentPage - 1)" :disabled="currentPage === 1" class="px-3 py-1.5 text-sm font-medium text-gray-700 bg-white border rounded hover:bg-gray-50 disabled:opacity-50 flex items-center">
            <ChevronLeftIcon class="w-4 h-4" />
          </button>
          <button @click="changePage(currentPage + 1)" :disabled="currentPage === totalPages" class="px-3 py-1.5 text-sm font-medium text-gray-700 bg-white border rounded hover:bg-gray-50 disabled:opacity-50 flex items-center">
            <ChevronRightIcon class="w-4 h-4" />
          </button>
        </div>
      </div>
    </div>

    <div v-if="isModalOpen" class="fixed inset-0 z-50 flex items-center justify-center bg-gray-900/50 backdrop-blur-sm" @click.self="isModalOpen = false">
      <div class="bg-white rounded-xl shadow-xl w-full max-w-2xl mx-4 overflow-hidden animate-slide-up">
        <div class="px-6 py-4 border-b flex justify-between items-center bg-gray-50">
          <h3 class="font-bold text-gray-800">Chi tiết chứng từ: <span class="text-blue-600">{{ selectedDoc?.id }}</span></h3>
          <button @click="isModalOpen = false" class="p-1 hover:bg-gray-200 rounded-full"><XMarkIcon class="w-5 h-5"/></button>
        </div>
        <div class="p-6">
          <div class="grid grid-cols-2 gap-4 mb-4">
            <div><p class="text-xs text-gray-500">Loại chứng từ</p><p class="font-medium">{{ selectedDoc?.typeName }}</p></div>
            <div><p class="text-xs text-gray-500">Người yêu cầu</p><p class="font-medium">{{ selectedDoc?.submitter }}</p></div>
            <div><p class="text-xs text-gray-500">Ngày tạo</p><p class="font-medium">{{ selectedDoc ? formatDateTime(selectedDoc.date) : '' }}</p></div>
            <div><p class="text-xs text-gray-500">Giá trị / Số lượng</p><p class="font-medium text-purple-600">{{ selectedDoc?.totalValue }}</p></div>
          </div>
          <div><p class="text-xs text-gray-500">Ghi chú</p><p class="bg-gray-50 p-2 rounded border mt-1 text-sm">{{ selectedDoc?.note || 'Không có ghi chú' }}</p></div>
          
          <div class="mt-6 flex justify-end gap-3 pt-4 border-t">
            <button @click="isModalOpen = false" class="px-4 py-2 border rounded-lg text-sm font-medium hover:bg-gray-50">Đóng</button>
            <template v-if="selectedDoc?.status === 'PENDING'">
              <button @click="rejectDoc(selectedDoc.id); isModalOpen=false" class="px-4 py-2 bg-red-600 text-white rounded-lg text-sm font-medium hover:bg-red-700">Từ chối</button>
              <button @click="approveDoc(selectedDoc.id); isModalOpen=false" class="px-4 py-2 bg-green-600 text-white rounded-lg text-sm font-medium hover:bg-green-700">Phê duyệt</button>
            </template>
          </div>
        </div>
      </div>
    </div>

  </div>
</template>

<style scoped>
.custom-scrollbar::-webkit-scrollbar { height: 6px; width: 6px; }
.custom-scrollbar::-webkit-scrollbar-thumb { background: #cbd5e1; border-radius: 10px; }
.animate-slide-up { animation: slideUp 0.2s ease-out; }
@keyframes slideUp { from { transform: translateY(20px); opacity: 0; } to { transform: translateY(0); opacity: 1; } }
</style>