<script setup>
import { ref, computed } from 'vue'
import { 
  BellAlertIcon, ExclamationTriangleIcon, 
  XCircleIcon, CheckCircleIcon,
  ShieldExclamationIcon, ClockIcon, ArrowUturnRightIcon
} from '@heroicons/vue/24/outline'

// === 1. STATE CHÍNH: TRỐNG CHỜ API ===
const alerts = ref([])

// === 2. BỘ LỌC ===
const filterSeverity = ref('')

const activeAlerts = computed(() => {
  return alerts.value.filter(a => {
    const isNotResolved = a.status === 'active'
    const matchSeverity = filterSeverity.value === '' || a.severity === filterSeverity.value
    return isNotResolved && matchSeverity
  })
})

const resolvedCount = computed(() => alerts.value.filter(a => a.status === 'resolved').length)

// === 3. HÀM RENDER GIAO DIỆN ===
const getAlertStyle = (severity) => {
  switch(severity) {
    case 'high': return { bg: 'bg-red-50 border-red-200', icon: XCircleIcon, iconColor: 'text-red-600', badge: 'bg-red-100 text-red-700' }
    case 'medium': return { bg: 'bg-amber-50 border-amber-200', icon: ExclamationTriangleIcon, iconColor: 'text-amber-600', badge: 'bg-amber-100 text-amber-700' }
    case 'low': return { bg: 'bg-blue-50 border-blue-200', icon: ShieldExclamationIcon, iconColor: 'text-blue-600', badge: 'bg-blue-100 text-blue-700' }
    default: return { bg: 'bg-gray-50 border-gray-200', icon: BellAlertIcon, iconColor: 'text-gray-600', badge: 'bg-gray-100 text-gray-700' }
  }
}

const getSeverityLabel = (severity) => {
  if (severity === 'high') return 'Nghiêm trọng'
  if (severity === 'medium') return 'Cảnh báo'
  if (severity === 'low') return 'Lưu ý'
  return 'Thông báo'
}

// === 4. THAO TÁC XỬ LÝ CẢNH BÁO ===
const markAsResolved = (id) => {
  const idx = alerts.value.findIndex(a => a.id === id)
  if (idx !== -1) {
    alerts.value[idx].status = 'resolved'
  }
}

const resolveAll = () => {
  if (confirm('Sếp có chắc chắn muốn đánh dấu ĐÃ ĐỌC cho tất cả cảnh báo hiện tại?')) {
    alerts.value.forEach(a => a.status = 'resolved')
  }
}
</script>

<template>
  <div class="space-y-5 md:space-y-6 animate-fade-in pb-10 px-0 md:px-1">
    <div class="flex flex-col sm:flex-row sm:items-center justify-between gap-4">
      <div>
        <h2 class="text-xl md:text-2xl font-bold text-gray-800 flex items-center gap-2">
          Trung Tâm Cảnh Báo <span class="bg-red-500 text-white text-xs px-2.5 py-0.5 rounded-full shadow-sm">{{ activeAlerts.length }}</span>
        </h2>
        <p class="text-xs md:text-sm text-gray-500 mt-1">Thông báo tự động từ hệ thống về hàng hóa và chứng từ</p>
      </div>
      <div class="flex items-center gap-2">
        <button @click="resolveAll" v-if="activeAlerts.length > 0" class="bg-white border border-gray-300 text-gray-700 px-4 py-2.5 rounded-lg text-sm font-semibold hover:bg-gray-50 transition-colors shadow-sm flex items-center gap-2">
          <CheckCircleIcon class="w-5 h-5"/> Đánh dấu tất cả đã đọc
        </button>
      </div>
    </div>

    <div class="bg-white p-3 md:p-4 rounded-xl border border-gray-200 flex items-center justify-between shadow-sm">
      <div class="flex items-center gap-3">
        <span class="text-sm font-bold text-gray-700">Lọc mức độ:</span>
        <select v-model="filterSeverity" class="border border-gray-200 rounded-lg text-sm px-4 py-2 outline-none focus:ring-1 focus:ring-primary-500 cursor-pointer">
          <option value="">Tất cả mức độ</option>
          <option value="high">Đỏ - Nghiêm trọng</option>
          <option value="medium">Cam - Cảnh báo</option>
          <option value="low">Xanh - Lưu ý</option>
        </select>
      </div>
      <div class="text-sm text-gray-500 hidden sm:block">Đã giải quyết: <span class="font-bold text-emerald-600">{{ resolvedCount }}</span> cảnh báo</div>
    </div>

    <div v-if="activeAlerts.length === 0" class="bg-white border border-gray-200 border-dashed rounded-xl p-12 text-center shadow-sm">
      <BellAlertIcon class="w-16 h-16 text-emerald-300 mx-auto mb-4" />
      <h3 class="text-lg font-bold text-gray-800">Tuyệt vời! Không có cảnh báo nào.</h3>
      <p class="text-sm text-gray-500 mt-2">Hệ thống kho đang hoạt động cực kỳ mượt mà và an toàn.</p>
    </div>

    <div v-else class="space-y-4">
      <div v-for="alert in activeAlerts" :key="alert.id" class="rounded-xl border p-4 sm:p-5 flex flex-col sm:flex-row gap-4 transition-all hover:shadow-md items-start sm:items-center relative" :class="getAlertStyle(alert.severity).bg">
        <div class="w-12 h-12 rounded-full bg-white flex items-center justify-center shrink-0 border border-gray-100 shadow-sm">
          <component :is="getAlertStyle(alert.severity).icon" class="w-7 h-7" :class="getAlertStyle(alert.severity).iconColor" />
        </div>
        <div class="flex-1">
          <div class="flex flex-wrap items-center gap-2 mb-1">
            <h4 class="text-base font-bold text-gray-900">{{ alert.title }}</h4>
            <span :class="['text-[10px] font-bold px-2 py-0.5 rounded uppercase tracking-wider', getAlertStyle(alert.severity).badge]">{{ getSeverityLabel(alert.severity) }}</span>
          </div>
          <p class="text-sm text-gray-700 mb-2 leading-relaxed max-w-4xl">{{ alert.message }}</p>
          <div class="flex items-center gap-4 text-xs font-medium text-gray-500">
            <span class="flex items-center gap-1"><ClockIcon class="w-4 h-4"/> {{ alert.time }}</span>
            <span class="text-gray-300">|</span>
            <a href="#" class="text-primary-600 hover:text-primary-800 flex items-center gap-1 font-bold underline underline-offset-2"><ArrowUturnRightIcon class="w-3.5 h-3.5" /> {{ alert.actionLink }}</a>
          </div>
        </div>
        <div class="w-full sm:w-auto flex justify-end shrink-0 mt-2 sm:mt-0">
          <button @click="markAsResolved(alert.id)" class="px-4 py-2 bg-white hover:bg-gray-50 border border-gray-300 text-gray-700 rounded-lg text-sm font-semibold transition-colors flex items-center gap-2 w-full sm:w-auto justify-center shadow-sm hover:shadow">
            <CheckCircleIcon class="w-5 h-5 text-emerald-500"/> Xong, Đóng lại!
          </button>
        </div>
      </div>
    </div>
  </div>
</template>

<style scoped>
</style>