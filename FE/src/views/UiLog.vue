<script setup>
import { ref, computed, onMounted } from 'vue'
import { 
  MagnifyingGlassIcon, FunnelIcon, ComputerDesktopIcon, ArrowPathIcon, 
  EyeIcon, XMarkIcon, ChevronLeftIcon, ChevronRightIcon,
  BugAntIcon, CursorArrowRaysIcon, GlobeAltIcon, ArrowRightOnRectangleIcon
} from '@heroicons/vue/24/outline'

// URL gọi tới API của Backend
const API_URL = 'https://localhost:7139/api/SysUiLogs' 

const logs = ref([])
const isLoading = ref(false)
const searchQuery = ref('')
const filterEventType = ref('')

// --- STATE PHÂN TRANG & MODAL ---
const currentPage = ref(1)
const itemsPerPage = 10
const isModalOpen = ref(false)
const selectedLog = ref(null)

// --- KẾT NỐI API BACKEND ---
const fetchLogs = async () => {
  isLoading.value = true
  try {
    const res = await fetch(API_URL, {
      headers: {
        "Authorization": "Bearer " + (localStorage.getItem("token") || "")
      }
    })
    if (res.ok) {
      const responseData = await res.json()
      
      if (Array.isArray(responseData)) {
        logs.value = responseData
      } else if (responseData && Array.isArray(responseData.data)) {
        logs.value = responseData.data
      } else if (responseData && Array.isArray(responseData.Data)) {
        logs.value = responseData.Data
      } else {
        logs.value = []
      }

    } else {
      console.error('Lỗi từ Server:', res.status)
      logs.value = []
    }
  } catch (error) {
    console.error('Lỗi kết nối API:', error)
    logs.value = []
  } finally {
    isLoading.value = false
  }
}

// --- LOGIC LỌC & PHÂN TRANG ---
const filteredLogs = computed(() => {
  let result = Array.isArray(logs.value) ? logs.value : []
  
  if (searchQuery.value) {
    const q = searchQuery.value.toLowerCase()
    result = result.filter(log => 
      log.userName?.toLowerCase().includes(q) || 
      log.path?.toLowerCase().includes(q) ||
      log.message?.toLowerCase().includes(q)
    )
  }

  if (filterEventType.value) {
    result = result.filter(log => log.eventType === filterEventType.value)
  }
  return result
})

// Tính toán Pagination dựa trên danh sách ĐÃ LỌC
const totalPages = computed(() => Math.ceil(filteredLogs.value.length / itemsPerPage) || 1)

// Reset trang về 1 nếu thay đổi bộ lọc
computed(() => {
  searchQuery.value; filterEventType.value;
  currentPage.value = 1;
});

const paginatedLogs = computed(() => {
  const start = (currentPage.value - 1) * itemsPerPage
  return filteredLogs.value.slice(start, start + itemsPerPage)
})

const changePage = (page) => {
  if (page >= 1 && page <= totalPages.value) currentPage.value = page
}

// --- XỬ LÝ MODAL ---
const openLogDetails = (log) => {
  selectedLog.value = log
  isModalOpen.value = true
}
const closeModal = () => {
  isModalOpen.value = false
  setTimeout(() => { selectedLog.value = null }, 200) 
}

// --- ĐỊNH DẠNG HIỂN THỊ ---
const getEventConfig = (type) => {
  switch (type) {
    case 'NAVIGATION': return { label: 'CHUYỂN TRANG', color: 'bg-blue-50 text-blue-700 border-blue-200', icon: ArrowRightOnRectangleIcon }
    case 'CLICK': return { label: 'TƯƠNG TÁC', color: 'bg-gray-100 text-gray-700 border-gray-200', icon: CursorArrowRaysIcon }
    case 'ERROR': return { label: 'LỖI (ERROR)', color: 'bg-red-50 text-red-700 border-red-200', icon: BugAntIcon }
    case 'API_CALL': return { label: 'GỌI API', color: 'bg-emerald-50 text-emerald-700 border-emerald-200', icon: GlobeAltIcon }
    default: return { label: 'HỆ THỐNG', color: 'bg-gray-50 text-gray-500 border-gray-200', icon: ComputerDesktopIcon }
  }
}

const formatDateTime = (dateString) => {
  if (!dateString) return 'Không có dữ liệu'
  const date = new Date(dateString)
  return new Intl.DateTimeFormat('vi-VN', {
    day: '2-digit', month: '2-digit', year: 'numeric',
    hour: '2-digit', minute: '2-digit', second: '2-digit'
  }).format(date)
}

onMounted(() => fetchLogs())
</script>

<template>
  <div class="space-y-5 md:space-y-6 animate-fade-in pb-10 px-0 md:px-1 relative">
    
    <div class="flex flex-col sm:flex-row sm:items-center justify-between gap-4">
      <div>
        <h2 class="text-xl md:text-2xl font-bold text-gray-800 flex items-center gap-2">
          <ComputerDesktopIcon class="w-7 h-7 text-primary-600" />
          Nhật ký Giao diện (UI Logs)
        </h2>
        <p class="text-xs md:text-sm text-gray-500 mt-1">Theo dõi thao tác, lỗi Frontend và hành vi người dùng trên màn hình</p>
      </div>
      <button @click="fetchLogs" class="bg-white border border-gray-200 hover:bg-gray-50 text-gray-700 px-4 py-2 h-10 rounded-lg flex items-center gap-2 text-sm font-medium transition-colors shadow-sm">
        <ArrowPathIcon class="w-5 h-5" :class="{'animate-spin': isLoading}"/> Làm mới
      </button>
    </div>

    <div class="bg-white p-3 md:p-4 rounded-xl border border-gray-200 flex flex-col sm:flex-row items-center gap-3 md:gap-4 shadow-sm">
      <div class="relative w-full sm:flex-1">
        <div class="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none">
          <MagnifyingGlassIcon class="w-5 h-5 text-gray-400" />
        </div>
        <input v-model="searchQuery" @input="currentPage = 1" type="text" class="block w-full h-10 pl-10 pr-3 border border-gray-200 rounded-lg text-sm focus:ring-1 focus:ring-blue-500 outline-none transition-all" placeholder="Tìm theo người dùng, đường dẫn, mô tả lỗi...">
      </div>
      
      <div class="flex gap-2 w-full sm:w-auto">
        <select v-model="filterEventType" @change="currentPage = 1" class="w-full sm:w-auto h-10 border border-gray-200 rounded-lg text-sm px-3 focus:ring-1 focus:ring-blue-500 outline-none cursor-pointer bg-white">
          <option value="">Tất cả Sự kiện</option>
          <option value="NAVIGATION">Chuyển trang (Nav)</option>
          <option value="CLICK">Tương tác Click</option>
          <option value="API_CALL">Gọi API (Request)</option>
          <option value="ERROR">Lỗi giao diện (Error)</option>
        </select>
        <button class="bg-gray-50 border border-gray-200 text-gray-700 px-4 h-10 rounded-lg flex items-center gap-2 text-sm font-medium hover:bg-gray-100 transition-colors">
          <FunnelIcon class="w-4 h-4" /> Lọc
        </button>
      </div>
    </div>

    <div class="bg-white rounded-xl border border-gray-200 shadow-sm overflow-hidden flex flex-col">
      <div class="w-full overflow-x-auto custom-scrollbar flex-1">
        <table class="min-w-[1000px] w-full divide-y divide-gray-200">
          <thead class="bg-slate-50">
            <tr>
              <th class="px-5 py-4 text-left text-xs font-semibold text-slate-600 uppercase tracking-wider w-[15%]">Thời gian</th>
              <th class="px-5 py-4 text-left text-xs font-semibold text-slate-600 uppercase tracking-wider w-[15%]">Người dùng (Session)</th>
              <th class="px-5 py-4 text-left text-xs font-semibold text-slate-600 uppercase tracking-wider w-[15%]">Loại sự kiện</th>
              <th class="px-5 py-4 text-left text-xs font-semibold text-slate-600 uppercase tracking-wider w-[15%]">Màn hình (Path)</th>
              <th class="px-5 py-4 text-left text-xs font-semibold text-slate-600 uppercase tracking-wider w-[30%]">Mô tả sự kiện</th>
              <th class="px-5 py-4 text-center text-xs font-semibold text-slate-600 uppercase tracking-wider w-[10%]">Thao tác</th>
            </tr>
          </thead>
          <tbody class="divide-y divide-gray-100 bg-white">
            
            <tr v-if="isLoading"><td colspan="6" class="px-6 py-12 text-center text-gray-500 font-medium">Đang tải dữ liệu Frontend Logs...</td></tr>
            <tr v-else-if="paginatedLogs.length === 0">
              <td colspan="6" class="px-6 py-16 text-center">
                <ComputerDesktopIcon class="w-12 h-12 text-gray-300 mx-auto mb-3" />
                <h3 class="text-base font-medium text-gray-900">Không có nhật ký UI</h3>
                <p class="text-sm text-gray-500 mt-1">Chưa có dữ liệu từ Backend trả về hoặc không khớp kết quả lọc.</p>
              </td>
            </tr>
            
            <tr v-else v-for="log in paginatedLogs" :key="log.id" class="hover:bg-blue-50/30 transition-colors group">
              <td class="px-5 py-4 text-sm text-gray-500 whitespace-nowrap">{{ formatDateTime(log.timestamp || log.logDate) }}</td>
              <td class="px-5 py-4 text-sm font-bold text-slate-700">{{ log.userName }}</td>
              <td class="px-5 py-4">
                <span :class="['text-[11px] font-bold px-2 py-1 rounded border tracking-wide flex items-center w-max gap-1.5', getEventConfig(log.eventType).color]">
                  <component :is="getEventConfig(log.eventType).icon" class="w-3.5 h-3.5" />
                  {{ getEventConfig(log.eventType).label }}
                </span>
              </td>
              <td class="px-5 py-4 text-sm font-mono text-purple-600">{{ log.path }}</td>
              <td class="px-5 py-4 text-sm text-gray-600 max-w-xs truncate" :class="{'text-red-600 font-medium': log.eventType === 'ERROR'}">
                {{ log.message }}
              </td>
              <td class="px-5 py-4 text-center">
                <button @click="openLogDetails(log)" class="text-blue-600 hover:text-blue-800 p-1.5 rounded-md hover:bg-blue-100 transition-colors inline-flex items-center gap-1.5 text-sm font-medium opacity-0 group-hover:opacity-100">
                  <EyeIcon class="w-4 h-4" /> Xem
                </button>
              </td>
            </tr>

          </tbody>
        </table>
      </div>

      <div v-if="filteredLogs.length > 0" class="border-t border-gray-200 bg-slate-50 px-5 py-3 flex items-center justify-between">
        <div class="hidden sm:block text-sm text-gray-600">
          Hiển thị từ <span class="font-semibold">{{ (currentPage - 1) * itemsPerPage + 1 }}</span> đến 
          <span class="font-semibold">{{ Math.min(currentPage * itemsPerPage, filteredLogs.length) }}</span> 
          / <span class="font-semibold">{{ filteredLogs.length }}</span> bản ghi
        </div>
        <div class="flex gap-2">
          <button @click="changePage(currentPage - 1)" :disabled="currentPage === 1" class="px-3 py-1.5 text-sm font-medium text-gray-700 bg-white border border-gray-300 rounded-md hover:bg-gray-50 disabled:opacity-50 disabled:cursor-not-allowed flex items-center">
            <ChevronLeftIcon class="w-4 h-4 mr-1" /> Trước
          </button>
          <span class="px-3 py-1.5 text-sm font-medium text-gray-700">Trang {{ currentPage }} / {{ totalPages }}</span>
          <button @click="changePage(currentPage + 1)" :disabled="currentPage === totalPages" class="px-3 py-1.5 text-sm font-medium text-gray-700 bg-white border border-gray-300 rounded-md hover:bg-gray-50 disabled:opacity-50 disabled:cursor-not-allowed flex items-center">
            Sau <ChevronRightIcon class="w-4 h-4 ml-1" />
          </button>
        </div>
      </div>
    </div>

    <div v-if="isModalOpen" class="fixed inset-0 z-50 flex items-center justify-center bg-gray-900/60 backdrop-blur-sm transition-opacity" @click.self="closeModal">
      <div class="bg-white rounded-xl shadow-2xl w-full max-w-3xl transform transition-all animate-slide-up mx-4 overflow-hidden flex flex-col max-h-[90vh]">
        <div class="px-6 py-4 border-b border-gray-200 flex justify-between items-center bg-slate-50">
          <h3 class="text-lg font-bold text-gray-800 flex items-center gap-2">
            <ComputerDesktopIcon class="w-5 h-5 text-gray-500"/> Chi tiết Phiên tương tác (Session)
          </h3>
          <button @click="closeModal" class="text-gray-400 hover:text-red-500 p-1.5 hover:bg-red-50 rounded-full transition-colors">
            <XMarkIcon class="w-5 h-5" />
          </button>
        </div>
        
        <div class="p-6 overflow-y-auto custom-scrollbar flex-1" v-if="selectedLog">
          <div class="grid grid-cols-2 md:grid-cols-4 gap-4 mb-6">
            <div class="col-span-1 border rounded-lg p-3 bg-white shadow-sm">
              <p class="text-[10px] text-gray-500 uppercase font-bold tracking-wider mb-1">Người dùng</p>
              <p class="text-sm font-bold text-slate-800">{{ selectedLog.userName }}</p>
            </div>
            <div class="col-span-1 border rounded-lg p-3 bg-white shadow-sm">
              <p class="text-[10px] text-gray-500 uppercase font-bold tracking-wider mb-1">Thời gian</p>
              <p class="text-sm font-medium text-slate-800">{{ formatDateTime(selectedLog.timestamp || selectedLog.logDate) }}</p>
            </div>
            <div class="col-span-2 border rounded-lg p-3 bg-white shadow-sm">
              <p class="text-[10px] text-gray-500 uppercase font-bold tracking-wider mb-1">Đường dẫn (Path)</p>
              <p class="text-sm font-mono text-purple-600">{{ selectedLog.path }}</p>
            </div>
          </div>

          <div class="mb-5">
            <p class="text-[11px] text-gray-500 uppercase font-bold tracking-wider mb-2">Mô tả sự kiện</p>
            <div class="p-4 rounded-lg border-l-4" :class="selectedLog.eventType === 'ERROR' ? 'bg-red-50 border-red-500 text-red-700' : 'bg-blue-50 border-blue-500 text-blue-800'">
              <p class="font-medium">{{ selectedLog.message }}</p>
            </div>
          </div>

          <div class="space-y-3">
            <div class="flex items-center justify-between border-b border-gray-200 pb-2">
              <h4 class="text-sm font-bold text-gray-700">Payload / Context Data</h4>
              <span :class="['text-[10px] font-bold px-2 py-0.5 rounded border tracking-wider', getEventConfig(selectedLog.eventType).color]">
                {{ getEventConfig(selectedLog.eventType).label }}
              </span>
            </div>
            <div class="bg-[#1e1e1e] rounded-lg p-4 font-mono text-sm text-green-400 overflow-x-auto shadow-inner border border-gray-800 relative">
              <div class="absolute top-2 right-2 text-xs text-gray-500">JSON</div>
              <pre>{{ selectedLog.details || '{\n  "info": "Không có dữ liệu payload"\n}' }}</pre>
            </div>
          </div>
        </div>

        <div class="px-6 py-4 border-t border-gray-200 bg-slate-50 flex justify-end">
          <button @click="closeModal" class="bg-white border border-gray-300 text-gray-700 px-5 py-2 rounded-lg text-sm font-semibold hover:bg-gray-100 transition-colors shadow-sm focus:ring-2 focus:ring-gray-200 outline-none">
            Đóng cửa sổ
          </button>
        </div>
      </div>
    </div>

  </div>
</template>

<style scoped>
.custom-scrollbar::-webkit-scrollbar { height: 6px; width: 6px; }
.custom-scrollbar::-webkit-scrollbar-track { background: transparent; }
.custom-scrollbar::-webkit-scrollbar-thumb { background: #cbd5e1; border-radius: 10px; }
.custom-scrollbar::-webkit-scrollbar-thumb:hover { background: #94a3b8; }

.animate-fade-in { animation: fadeIn 0.3s ease-out; }
.animate-slide-up { animation: slideUp 0.3s cubic-bezier(0.16, 1, 0.3, 1); }

@keyframes fadeIn { 
  from { opacity: 0; } 
  to { opacity: 1; } 
}
@keyframes slideUp { 
  from { opacity: 0; transform: translateY(20px) scale(0.98); } 
  to { opacity: 1; transform: translateY(0) scale(1); } 
}
</style>