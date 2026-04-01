<script setup>
import { ref, computed, onMounted } from 'vue'
import { 
  MagnifyingGlassIcon, FunnelIcon, ClipboardDocumentListIcon, ArrowPathIcon 
} from '@heroicons/vue/24/outline'

const API_URL = 'https://localhost:7139/api/AuditLogs'

const logs = ref([])
const isLoading = ref(false)
const searchQuery = ref('')
const filterAction = ref('')

const fetchLogs = async () => {
  isLoading.value = true
  try {
    const res = await fetch(API_URL)
    if (res.ok) {
      logs.value = await res.json()
    }
  } catch (error) {
    console.error('Lỗi khi tải Audit Logs:', error)
  } finally {
    isLoading.value = false
  }
}

// --- LỌC TÌM KIẾM THÔNG MINH ---
const filteredLogs = computed(() => {
  let result = logs.value
  
  if (searchQuery.value) {
    const q = searchQuery.value.toLowerCase()
    result = result.filter(log => 
      log.userName?.toLowerCase().includes(q) || 
      log.tableName?.toLowerCase().includes(q) ||
      log.actionType?.toLowerCase().includes(q) ||
      log.details?.toLowerCase().includes(q) // Cho phép tìm kiếm luôn trong cột chi tiết
    )
  }

  if (filterAction.value) {
    result = result.filter(log => log.actionType?.toUpperCase() === filterAction.value.toUpperCase())
  }

  return result
})

// --- TÔ MÀU CHO TỪNG LOẠI HÀNH ĐỘNG ---
const getActionColor = (action) => {
  const act = action?.toUpperCase() || ''
  if (act.includes('INSERT') || act.includes('THÊM') || act.includes('ADD')) return 'bg-green-50 text-green-700 border-green-200'
  if (act.includes('UPDATE') || act.includes('CẬP NHẬT') || act.includes('EDIT')) return 'bg-blue-50 text-blue-700 border-blue-200'
  if (act.includes('DELETE') || act.includes('XÓA')) return 'bg-red-50 text-red-700 border-red-200'
  if (act.includes('LOGIN') || act.includes('ĐĂNG NHẬP')) return 'bg-purple-50 text-purple-700 border-purple-200'
  return 'bg-gray-50 text-gray-700 border-gray-200'
}

// --- DỊCH TÊN HÀNH ĐỘNG RA TIẾNG VIỆT ---
const translateAction = (action) => {
  const act = action?.toUpperCase() || ''
  if (act === 'INSERT') return 'THÊM MỚI'
  if (act === 'UPDATE') return 'CẬP NHẬT'
  if (act === 'DELETE') return 'XÓA BỎ'
  if (act === 'LOGIN') return 'ĐĂNG NHẬP'
  return act
}

// --- ĐỊNH DẠNG NGÀY GIỜ CHUẨN VN ---
const formatDateTime = (dateString) => {
  if (!dateString) return 'Không xác định'
  const date = new Date(dateString)
  return new Intl.DateTimeFormat('vi-VN', {
    day: '2-digit', month: '2-digit', year: 'numeric',
    hour: '2-digit', minute: '2-digit', second: '2-digit'
  }).format(date)
}

onMounted(() => fetchLogs())
</script>

<template>
  <div class="space-y-5 md:space-y-6 animate-fade-in pb-10 px-0 md:px-1">
    
    <div class="flex flex-col sm:flex-row sm:items-center justify-between gap-4">
      <div>
        <h2 class="text-xl md:text-2xl font-bold text-gray-800">Nhật ký Hệ thống (Audit Logs)</h2>
        <p class="text-xs md:text-sm text-gray-500 mt-1">Ghi vết toàn bộ thao tác của người dùng để đảm bảo tính minh bạch</p>
      </div>
      <button @click="fetchLogs" class="bg-white border border-gray-200 hover:bg-gray-50 text-gray-700 px-4 py-2 h-10 rounded-lg flex items-center gap-2 text-sm font-medium transition-colors shadow-sm">
        <ArrowPathIcon class="w-5 h-5" :class="{'animate-spin': isLoading}"/> Tải lại dữ liệu
      </button>
    </div>

    <div class="bg-white p-3 md:p-4 rounded-xl border border-gray-200 flex flex-col sm:flex-row items-center gap-3 md:gap-4 shadow-sm">
      <div class="relative w-full sm:flex-1">
        <div class="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none">
          <MagnifyingGlassIcon class="w-5 h-5 text-gray-400" />
        </div>
        <input v-model="searchQuery" type="text" class="block w-full h-10 pl-10 pr-3 border border-gray-200 rounded-lg text-sm focus:ring-1 focus:ring-blue-500 focus:border-blue-500 outline-none transition-all" placeholder="Tìm theo người dùng, đối tượng tác động, chi tiết...">
      </div>
      
      <div class="flex gap-2 w-full sm:w-auto">
        <select v-model="filterAction" class="w-full sm:w-auto h-10 border border-gray-200 rounded-lg text-sm px-3 focus:ring-1 focus:ring-blue-500 focus:border-blue-500 outline-none cursor-pointer bg-white">
          <option value="">Tất cả Hành động</option>
          <option value="INSERT">Thêm mới</option>
          <option value="UPDATE">Cập nhật</option>
          <option value="DELETE">Xóa bỏ</option>
          <option value="LOGIN">Đăng nhập</option>
        </select>
        <button class="bg-gray-50 border border-gray-200 text-gray-700 px-4 h-10 rounded-lg flex items-center gap-2 text-sm font-medium hover:bg-gray-100 transition-colors">
          <FunnelIcon class="w-4 h-4" /> Lọc ngày
        </button>
      </div>
    </div>

    <div class="bg-white rounded-xl border border-gray-200 shadow-sm overflow-hidden">
      <div class="w-full overflow-x-auto custom-scrollbar">
        <table class="min-w-[1000px] w-full divide-y divide-gray-200">
          <thead class="bg-gray-50/80">
            <tr>
              <th class="px-5 py-4 text-left text-xs font-semibold text-gray-600 uppercase tracking-wider w-[15%]">Thời gian</th>
              <th class="px-5 py-4 text-left text-xs font-semibold text-gray-600 uppercase tracking-wider w-[15%]">Người thực hiện</th>
              <th class="px-5 py-4 text-center text-xs font-semibold text-gray-600 uppercase tracking-wider w-[12%]">Hành động</th>
              <th class="px-5 py-4 text-left text-xs font-semibold text-gray-600 uppercase tracking-wider w-[20%]">Đối tượng (Bảng)</th>
              <th class="px-5 py-4 text-left text-xs font-semibold text-gray-600 uppercase tracking-wider w-[38%]">Chi tiết thao tác</th>
            </tr>
          </thead>
          <tbody class="divide-y divide-gray-100 bg-white">
            
            <tr v-if="isLoading">
              <td colspan="5" class="px-6 py-12 text-center text-gray-500 font-medium">Đang tải dữ liệu...</td>
            </tr>
            
            <tr v-else-if="filteredLogs.length === 0">
              <td colspan="5" class="px-6 py-16 text-center">
                <ClipboardDocumentListIcon class="w-12 h-12 text-gray-300 mx-auto mb-3" />
                <h3 class="text-base font-medium text-gray-900">Không tìm thấy nhật ký</h3>
                <p class="text-sm text-gray-500 mt-1">Chưa có dữ liệu nào khớp với điều kiện lọc của bạn.</p>
              </td>
            </tr>
            
            <tr v-else v-for="log in filteredLogs" :key="log.logId" class="hover:bg-gray-50/50 transition-colors">
              <td class="px-5 py-4 text-sm text-gray-500 whitespace-nowrap">{{ formatDateTime(log.logDate) }}</td>
              <td class="px-5 py-4 text-sm font-medium text-gray-900">{{ log.userName }}</td>
              <td class="px-5 py-4 text-center">
                <span :class="['text-xs font-medium px-2.5 py-1 rounded border tracking-wide', getActionColor(log.actionType)]">
                  {{ translateAction(log.actionType) }}
                </span>
              </td>
              <td class="px-5 py-4 text-sm font-medium text-gray-800">{{ log.tableName }}</td>
              <td class="px-5 py-4 text-sm text-gray-600 max-w-sm truncate" :title="log.details">
                {{ log.details || 'Hệ thống ghi nhận thay đổi tại phân hệ này.' }}
              </td>
            </tr>

          </tbody>
        </table>
      </div>
    </div>

  </div>
</template>

<style scoped>
.custom-scrollbar::-webkit-scrollbar { height: 6px; width: 6px; }
.custom-scrollbar::-webkit-scrollbar-track { background: transparent; }
.custom-scrollbar::-webkit-scrollbar-thumb { background: #cbd5e1; border-radius: 10px; }
.custom-scrollbar::-webkit-scrollbar-thumb:hover { background: #94a3b8; }
.animate-fade-in { animation: fadeIn 0.2s ease-out; }
@keyframes fadeIn { from { opacity: 0; transform: translateY(10px); } to { opacity: 1; transform: translateY(0); } }
</style>