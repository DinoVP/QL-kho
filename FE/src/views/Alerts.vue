<script setup>
import { ref, computed, onMounted } from 'vue'
import { 
  BellAlertIcon, ExclamationTriangleIcon, 
  XCircleIcon, CheckCircleIcon,
  ShieldExclamationIcon, ClockIcon, ArrowUturnRightIcon,
  ArrowPathIcon
} from '@heroicons/vue/24/outline'

const STOCK_API = 'https://localhost:7139/api/Stock'
const PROD_API = 'https://localhost:7139/api/Products'
const LOC_API = 'https://localhost:7139/api/Locations'

const getAuthHeaders = () => ({ 'Content-Type': 'application/json', 'Authorization': 'Bearer ' + (localStorage.getItem('authToken') || '') })
const myWarehouseId = ref(parseInt(localStorage.getItem('warehouseId')) || null)

// === 1. STATE CHÍNH ===
const alerts = ref([])
const isLoading = ref(false)

const fetchData = async () => {
  isLoading.value = true
  try {
    const headers = getAuthHeaders()
    const [stockRes, prodRes, locRes] = await Promise.all([
      fetch(STOCK_API, { headers }), fetch(PROD_API, { headers }), fetch(LOC_API, { headers })
    ])
    
    if (stockRes.ok && prodRes.ok) {
        const rawStocks = await stockRes.json()
        const products = await prodRes.json()
        const locations = locRes.ok ? await locRes.json() : []

        // Chỉ xét tồn kho của Kho đang đăng nhập
        const myStocks = rawStocks.filter(s => !myWarehouseId.value || s.warehouseId === myWarehouseId.value)
        
        let generatedAlerts = []
        let alertIdCounter = 1;

        myStocks.forEach(s => {
            const prod = products.find(p => p.id === s.variantId || p.Id === s.variantId) || {};
            const loc = locations.find(l => l.id === s.locationId || l.Id === s.locationId) || {};
            
            const sku = prod.sku || prod.Sku || 'N/A'
            const name = prod.name || prod.Name || 'Sản phẩm'
            const convRate = prod.conversionRate || prod.ConversionRate || 24
            const minStock = prod.minStock || prod.MinStock || 10
            
            const totalItems = (s.qty || 0) * convRate; // Tổng số cái/chiếc
            const locName = loc.code || loc.Code || 'Khu Chờ Nhập'
            
            // 1. KIỂM TRA SỐ LƯỢNG
            if (s.qty === 0) {
                generatedAlerts.push({
                    id: alertIdCounter++, type: 'qty_out', severity: 'high', status: 'active',
                    title: `[HẾT HÀNG] ${sku} - ${name}`,
                    message: `Sản phẩm này đã cạn kiệt (0 thùng) tại vị trí [${locName}]. Vui lòng tạo Đơn Đặt Hàng (PO) bổ sung ngay lập tức!`,
                    time: 'Vừa cập nhật', actionLink: 'Tạo Đơn Đặt Hàng'
                })
            } else if (totalItems <= minStock) {
                generatedAlerts.push({
                    id: alertIdCounter++, type: 'qty_low', severity: 'medium', status: 'active',
                    title: `[SẮP HẾT HÀNG] ${sku} - ${name}`,
                    message: `Tồn kho hiện tại là ${s.qty} thùng (${totalItems} món), đã chạm hoặc thấp hơn định mức tối thiểu (${minStock} món) tại vị trí [${locName}].`,
                    time: 'Vừa cập nhật', actionLink: 'Xem Tồn Kho'
                })
            }

            // 2. KIỂM TRA HẠN SỬ DỤNG
            if (s.hsd) {
                const today = new Date(); today.setHours(0,0,0,0);
                const expiryDate = new Date(s.hsd);
                const diffTime = expiryDate - today;
                const diffDays = Math.ceil(diffTime / (1000 * 60 * 60 * 24));

                if (diffDays < 0) {
                    generatedAlerts.push({
                        id: alertIdCounter++, type: 'exp_out', severity: 'high', status: 'active',
                        title: `[ĐÃ HẾT HẠN] ${sku} - ${name} (${s.qty} Thùng)`,
                        message: `Lô hàng tại vị trí [${locName}] đã HẾT HẠN SỬ DỤNG vào ngày ${s.hsd}. Tuyệt đối không được xuất bán lô hàng này. Đề nghị chuyển sang khu vực Hàng Lỗi/Hủy.`,
                        time: 'Vừa cập nhật', actionLink: 'Lập báo cáo Hàng lỗi'
                    })
                } else if (diffDays >= 0 && diffDays <= 30) {
                    generatedAlerts.push({
                        id: alertIdCounter++, type: 'exp_low', severity: 'medium', status: 'active',
                        title: `[SẮP HẾT HẠN] ${sku} - ${name} (${s.qty} Thùng)`,
                        message: `Lô hàng tại vị trí [${locName}] chỉ còn ${diffDays} ngày nữa là hết hạn (HSD: ${s.hsd}). Cần ưu tiên xuất bán (FEFO) lô này trước.`,
                        time: 'Vừa cập nhật', actionLink: 'Tạo Điều chuyển'
                    })
                }
            }

            // 3. KIỂM TRA HÀNG BỊ KẸT Ở BÃI CHỜ (Chưa cất lên kệ)
            if (!s.locationId && s.qty > 0) {
                generatedAlerts.push({
                    id: alertIdCounter++, type: 'loc_pending', severity: 'low', status: 'active',
                    title: `[HÀNG CHƯA LÊN KỆ] ${sku} - ${name}`,
                    message: `Có ${s.qty} thùng đang nằm ở Khu Chờ Nhập chưa được cất lên Kệ chính thức. Vui lòng điều chuyển lên kệ để giải phóng không gian bãi.`,
                    time: 'Vừa cập nhật', actionLink: 'Cất hàng lên kệ'
                })
            }
        })
        
        alerts.value = generatedAlerts;
    }
  } catch (error) { console.error('Lỗi tải Cảnh báo:', error) }
  finally { isLoading.value = false }
}

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
  if (confirm('Sếp có chắc chắn muốn ẩn/xóa tất cả cảnh báo hiện tại trên màn hình này?')) {
    alerts.value.forEach(a => a.status = 'resolved')
  }
}

onMounted(() => fetchData())
</script>

<template>
  <div class="space-y-5 md:space-y-6 animate-fade-in pb-10 px-0 md:px-1 relative">
    
    <div class="flex flex-col sm:flex-row sm:items-center justify-between gap-4">
      <div>
        <h2 class="text-xl md:text-2xl font-bold text-gray-800 flex items-center gap-2">
          Trung Tâm Cảnh Báo 
          <span v-if="activeAlerts.length > 0" class="bg-red-500 text-white text-xs px-2.5 py-0.5 rounded-full shadow-sm animate-pulse">{{ activeAlerts.length }}</span>
        </h2>
        <p class="text-xs md:text-sm text-gray-500 mt-1">Thông báo tự động từ hệ thống về hàng hóa, hạn sử dụng và định mức</p>
      </div>
      <div class="flex items-center gap-2">
        <button @click="fetchData" class="bg-white border border-gray-300 text-gray-700 px-4 py-2.5 rounded-lg text-sm font-semibold hover:bg-gray-50 transition-colors shadow-sm flex items-center gap-2">
          <ArrowPathIcon class="w-5 h-5" :class="{'animate-spin': isLoading}"/> Quét lại Kho
        </button>
        <button @click="resolveAll" v-if="activeAlerts.length > 0" class="bg-slate-800 text-white px-4 py-2.5 rounded-lg text-sm font-semibold hover:bg-slate-900 transition-colors shadow-sm flex items-center gap-2">
          <CheckCircleIcon class="w-5 h-5"/> Xóa tất cả
        </button>
      </div>
    </div>

    <div class="bg-white p-3 md:p-4 rounded-xl border border-gray-200 flex items-center justify-between shadow-sm">
      <div class="flex items-center gap-3">
        <span class="text-sm font-bold text-gray-700">Lọc mức độ:</span>
        <select v-model="filterSeverity" class="border border-gray-200 rounded-lg text-sm px-4 py-2 outline-none focus:ring-1 focus:ring-primary-500 cursor-pointer">
          <option value="">Tất cả mức độ</option>
          <option value="high">Đỏ - Nghiêm trọng (Hết hàng / Hết hạn)</option>
          <option value="medium">Cam - Cảnh báo (Sắp hết / Sắp hết hạn)</option>
          <option value="low">Xanh - Lưu ý (Kẹt bãi chờ)</option>
        </select>
      </div>
      <div class="text-sm text-gray-500 hidden sm:block">Đã giải quyết / Ẩn đi: <span class="font-bold text-emerald-600">{{ resolvedCount }}</span> cảnh báo</div>
    </div>

    <div v-if="isLoading" class="bg-white border border-gray-200 border-dashed rounded-xl p-12 text-center shadow-sm">
      <ArrowPathIcon class="w-12 h-12 text-gray-300 mx-auto mb-4 animate-spin" />
      <h3 class="text-base font-semibold text-gray-700">Đang quét toàn bộ kho...</h3>
    </div>

    <div v-else-if="activeAlerts.length === 0" class="bg-white border border-gray-200 border-dashed rounded-xl p-12 text-center shadow-sm">
      <BellAlertIcon class="w-16 h-16 text-emerald-300 mx-auto mb-4" />
      <h3 class="text-lg font-bold text-gray-800">Tuyệt vời! Không có cảnh báo nào.</h3>
      <p class="text-sm text-gray-500 mt-2">Hệ thống kho đang hoạt động cực kỳ mượt mà, hàng hóa dồi dào và an toàn.</p>
    </div>

    <div v-else class="space-y-4">
      <div v-for="alert in activeAlerts" :key="alert.id" class="rounded-xl border p-4 sm:p-5 flex flex-col sm:flex-row gap-4 transition-all hover:shadow-md items-start sm:items-center relative" :class="getAlertStyle(alert.severity).bg">
        <div class="w-12 h-12 rounded-full bg-white flex items-center justify-center shrink-0 border border-gray-100 shadow-sm">
          <component :is="getAlertStyle(alert.severity).icon" class="w-7 h-7" :class="getAlertStyle(alert.severity).iconColor" />
        </div>
        <div class="flex-1">
          <div class="flex flex-wrap items-center gap-2 mb-1">
            <h4 class="text-base font-bold text-gray-900">{{ alert.title }}</h4>
            <span :class="['text-[10px] font-bold px-2 py-0.5 rounded uppercase tracking-wider border border-transparent', getAlertStyle(alert.severity).badge]">{{ getSeverityLabel(alert.severity) }}</span>
          </div>
          <p class="text-sm text-gray-700 mb-2 leading-relaxed max-w-4xl">{{ alert.message }}</p>
          <div class="flex items-center gap-4 text-xs font-medium text-gray-500">
            <span class="flex items-center gap-1"><ClockIcon class="w-4 h-4"/> {{ alert.time }}</span>
            <span class="text-gray-300">|</span>
            <button @click="() => alert('Tính năng chuyển hướng đang phát triển...')" class="text-primary-600 hover:text-primary-800 flex items-center gap-1 font-bold underline underline-offset-2"><ArrowUturnRightIcon class="w-3.5 h-3.5" /> {{ alert.actionLink }}</button>
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
.animate-fade-in { animation: fadeIn 0.2s ease-out; }
@keyframes fadeIn { from { opacity: 0; transform: translateY(5px); } to { opacity: 1; transform: translateY(0); } }
</style>