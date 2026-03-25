<script setup>
import { ref, computed } from 'vue'
import { 
  ChartBarIcon, ArrowTrendingUpIcon, ArrowTrendingDownIcon, 
  CubeIcon, CurrencyDollarIcon, PresentationChartLineIcon,
  ExclamationCircleIcon, ChartPieIcon, BuildingStorefrontIcon,
  DocumentArrowDownIcon, DocumentArrowUpIcon, PresentationChartBarIcon
} from '@heroicons/vue/24/outline'

// === 1. THỐNG KÊ TỔNG QUAN (VỀ 0) ===
const stats = ref({
  totalValue: '0 ₫', inboundThisMonth: 0, outboundThisMonth: 0, pendingOrders: 0, activeAlerts: 0
})

// === 2. DỮ LIỆU BIỂU ĐỒ: TRỐNG CHỜ API ===
const inboundChartData = ref([])
const outboundChartData = ref([])

// Xử lý chia cho 0 khi chưa có data
const maxInbound = computed(() => inboundChartData.value.length ? Math.max(...inboundChartData.value.map(d => d.value)) : 1)
const maxOutbound = computed(() => outboundChartData.value.length ? Math.max(...outboundChartData.value.map(d => d.value)) : 1)

// === 3. DỮ LIỆU BIỂU ĐỒ TRÒN ===
const pieChartData = ref([])
const pieGradient = computed(() => {
  if (pieChartData.value.length === 0) return 'conic-gradient(#f1f5f9 0% 100%)' // Màu xám trống
  // Logic vẽ (khi có data từ API)
  return 'conic-gradient(#f1f5f9 0% 100%)' 
})

// === 4. TỈ LỆ LẤP ĐẦY KHO ===
const warehouseCapacity = ref([])

// === 5. TOP HÀNG HÓA ===
const topProducts = ref([])
</script>

<template>
  <div class="space-y-6 animate-fade-in pb-10 px-0 md:px-1">
    
    <div class="flex flex-col sm:flex-row sm:items-center justify-between gap-4">
      <div>
        <h2 class="text-xl md:text-2xl font-bold text-gray-800">Tổng quan Hệ thống (Dashboard)</h2>
        <p class="text-xs md:text-sm text-gray-500 mt-1">Trung tâm điều khiển và báo cáo đồ thị trực quan</p>
      </div>
      <button class="bg-primary-600 hover:bg-primary-700 text-white px-4 py-2.5 rounded-lg text-sm font-semibold transition-colors shadow-sm flex items-center gap-2">
        <PresentationChartLineIcon class="w-5 h-5"/> Tải File PDF Báo Cáo
      </button>
    </div>

    <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4 md:gap-6">
      <div class="bg-white p-5 rounded-2xl border border-gray-100 shadow-sm flex flex-col relative overflow-hidden group hover:shadow-md transition-shadow">
        <div class="absolute -right-4 -top-4 w-16 h-16 bg-blue-50 rounded-full group-hover:scale-150 transition-transform duration-500"></div>
        <div class="flex items-center justify-between mb-4 relative z-10"><h3 class="text-sm font-semibold text-gray-500 uppercase tracking-wider">Tổng Giá Trị Tồn</h3><CurrencyDollarIcon class="w-6 h-6 text-blue-500" /></div>
        <p class="text-2xl font-extrabold text-gray-800 relative z-10">{{ stats.totalValue }}</p>
        <p class="text-xs font-medium text-gray-400 mt-2 relative z-10 flex items-center gap-1">Chờ dữ liệu...</p>
      </div>
      <div class="bg-white p-5 rounded-2xl border border-gray-100 shadow-sm flex flex-col relative overflow-hidden group hover:shadow-md transition-shadow">
        <div class="absolute -right-4 -top-4 w-16 h-16 bg-emerald-50 rounded-full group-hover:scale-150 transition-transform duration-500"></div>
        <div class="flex items-center justify-between mb-4 relative z-10"><h3 class="text-sm font-semibold text-gray-500 uppercase tracking-wider">Phiếu Nhập (Tháng)</h3><DocumentArrowDownIcon class="w-6 h-6 text-emerald-500" /></div>
        <p class="text-3xl font-extrabold text-gray-800 relative z-10">{{ stats.inboundThisMonth }}</p>
        <p class="text-xs font-medium text-gray-400 mt-2 relative z-10">Chờ dữ liệu...</p>
      </div>
      <div class="bg-white p-5 rounded-2xl border border-gray-100 shadow-sm flex flex-col relative overflow-hidden group hover:shadow-md transition-shadow">
        <div class="absolute -right-4 -top-4 w-16 h-16 bg-amber-50 rounded-full group-hover:scale-150 transition-transform duration-500"></div>
        <div class="flex items-center justify-between mb-4 relative z-10"><h3 class="text-sm font-semibold text-gray-500 uppercase tracking-wider">Phiếu Xuất (Tháng)</h3><DocumentArrowUpIcon class="w-6 h-6 text-amber-500" /></div>
        <p class="text-3xl font-extrabold text-gray-800 relative z-10">{{ stats.outboundThisMonth }}</p>
        <p class="text-xs font-medium text-amber-600 mt-2 relative z-10 flex items-center gap-1">{{ stats.pendingOrders }} đơn đang chờ xử lý</p>
      </div>
      <div class="bg-white p-5 rounded-2xl border border-gray-100 shadow-sm flex flex-col relative overflow-hidden group hover:shadow-md transition-shadow">
        <div class="absolute -right-4 -top-4 w-16 h-16 bg-red-50 rounded-full group-hover:scale-150 transition-transform duration-500"></div>
        <div class="flex items-center justify-between mb-4 relative z-10"><h3 class="text-sm font-semibold text-gray-500 uppercase tracking-wider">Cảnh báo tồn kho</h3><ExclamationCircleIcon class="w-6 h-6 text-red-500" /></div>
        <p class="text-3xl font-extrabold text-red-600 relative z-10">{{ stats.activeAlerts }}</p>
        <p class="text-xs font-medium text-red-500 mt-2 relative z-10">Hết hàng hoặc dưới định mức</p>
      </div>
    </div>

    <div class="bg-white rounded-2xl border border-gray-100 shadow-sm p-5 md:p-6">
      <div class="flex items-center justify-between mb-6">
        <h3 class="text-base font-bold text-gray-800 flex items-center gap-2"><PresentationChartBarIcon class="w-5 h-5 text-indigo-500"/> Biến động Giá trị Tồn kho (Tỷ VNĐ)</h3>
      </div>
      <div class="w-full h-48 sm:h-64 flex items-center justify-center border border-gray-100 bg-gray-50 rounded-lg border-dashed">
        <span class="text-gray-400 text-sm font-medium">Hệ thống đang chờ đồng bộ số liệu tài chính...</span>
      </div>
    </div>

    <div class="grid grid-cols-1 lg:grid-cols-2 gap-6">
      <div class="bg-white rounded-2xl border border-gray-100 shadow-sm p-5 md:p-6 flex flex-col">
        <div class="flex items-center justify-between mb-6">
          <h3 class="text-base font-bold text-gray-800 flex items-center gap-2"><DocumentArrowDownIcon class="w-5 h-5 text-emerald-500"/> Thống kê Nhập Kho (Phiếu)</h3>
        </div>
        <div class="flex-1 min-h-[200px] flex items-center justify-center bg-gray-50 rounded-lg border border-dashed border-gray-200">
           <span class="text-gray-400 text-sm font-medium">Chưa có dữ liệu Nhập kho</span>
        </div>
      </div>

      <div class="bg-white rounded-2xl border border-gray-100 shadow-sm p-5 md:p-6 flex flex-col">
        <div class="flex items-center justify-between mb-6">
          <h3 class="text-base font-bold text-gray-800 flex items-center gap-2"><DocumentArrowUpIcon class="w-5 h-5 text-blue-500"/> Thống kê Xuất Kho (Phiếu)</h3>
        </div>
        <div class="flex-1 min-h-[200px] flex items-center justify-center bg-gray-50 rounded-lg border border-dashed border-gray-200">
           <span class="text-gray-400 text-sm font-medium">Chưa có dữ liệu Xuất kho</span>
        </div>
      </div>
    </div>

    <div class="grid grid-cols-1 lg:grid-cols-3 gap-6">
      
      <div class="bg-white rounded-2xl border border-gray-100 shadow-sm p-5 md:p-6 flex flex-col items-center">
        <h3 class="text-base font-bold text-gray-800 mb-6 flex items-center gap-2 w-full justify-start"><ChartPieIcon class="w-5 h-5 text-gray-500"/> Cơ cấu Nhóm hàng Tồn</h3>
        <div class="relative w-48 h-48 rounded-full flex items-center justify-center shadow-inner bg-gray-100 border border-gray-200">
          <div class="w-32 h-32 bg-white rounded-full absolute flex flex-col items-center justify-center shadow-sm">
            <span class="text-xs font-semibold text-gray-400 uppercase tracking-widest">Tổng SKU</span>
            <span class="text-2xl font-black text-gray-300">0</span>
          </div>
        </div>
      </div>

      <div class="bg-white rounded-2xl border border-gray-100 shadow-sm p-5 md:p-6">
        <h3 class="text-base font-bold text-gray-800 mb-6 flex items-center gap-2"><BuildingStorefrontIcon class="w-5 h-5 text-gray-500"/> Tỉ lệ Lấp đầy Kho</h3>
        <div class="flex h-48 items-center justify-center">
          <span class="text-gray-400 text-sm font-medium italic">Chưa thiết lập vị trí kho</span>
        </div>
      </div>

      <div class="bg-white rounded-2xl border border-gray-100 shadow-sm p-5 md:p-6 flex flex-col">
        <h3 class="text-base font-bold text-gray-800 mb-6 flex items-center gap-2"><ChartBarIcon class="w-5 h-5 text-gray-500 transform rotate-90"/> Top Xuất Kho</h3>
        <div class="flex h-48 items-center justify-center">
          <span class="text-gray-400 text-sm font-medium italic">Chưa có giao dịch xuất hàng</span>
        </div>
      </div>

    </div>
  </div>
</template>