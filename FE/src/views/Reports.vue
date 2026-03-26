<script setup>
import { ref, computed } from 'vue'
import { 
  DocumentChartBarIcon, FunnelIcon, 
  ArrowDownTrayIcon, PrinterIcon,
  CalendarDaysIcon, BuildingStorefrontIcon
} from '@heroicons/vue/24/outline'

// === 1. TÙY CHỌN LỌC BÁO CÁO ===
const filterForm = ref({
  fromDate: new Date().toISOString().split('T')[0], // Mặc định ngày hôm nay
  toDate: new Date().toISOString().split('T')[0],
  warehouse: '',
  category: ''
})

// === 2. DỮ LIỆU ĐÃ DỌN SẠCH CHỜ API ===
const reportData = ref([])

// === 3. LOGIC LỌC DỮ LIỆU ĐA KHO ===
const filteredReport = computed(() => {
  return reportData.value.filter(item => {
    const matchWarehouse = filterForm.value.warehouse === '' || item.warehouse === filterForm.value.warehouse
    const matchCategory = filterForm.value.category === '' || item.category === filterForm.value.category
    return matchWarehouse && matchCategory
  })
})

// Tính toán tổng cộng bám theo dữ liệu đã lọc
const totalStart = computed(() => filteredReport.value.reduce((sum, item) => sum + item.startQty, 0))
const totalIn = computed(() => filteredReport.value.reduce((sum, item) => sum + item.inQty, 0))
const totalOut = computed(() => filteredReport.value.reduce((sum, item) => sum + item.outQty, 0))
const totalEnd = computed(() => filteredReport.value.reduce((sum, item) => sum + item.endQty, 0))

const handleExportExcel = () => {
  const whName = filterForm.value.warehouse || 'TẤT CẢ CÁC KHO'
  alert(`Đang chờ API để xuất Báo cáo NXT - [${whName}] từ ${filterForm.value.fromDate} đến ${filterForm.value.toDate} ra Excel...`)
}
</script>

<template>
  <div class="space-y-5 md:space-y-6 animate-fade-in pb-10 px-0 md:px-1 relative">
    
    <div class="flex flex-col sm:flex-row sm:items-center justify-between gap-4">
      <div>
        <h2 class="text-xl md:text-2xl font-bold text-gray-800">Báo cáo Nhập - Xuất - Tồn (Đa Kho)</h2>
        <p class="text-xs md:text-sm text-gray-500 mt-1">Bảng kê biến động hàng hóa theo từng chi nhánh lưu trữ</p>
      </div>
      <div class="flex gap-2">
        <button class="bg-white border border-gray-300 text-gray-700 px-4 py-2.5 rounded-lg text-sm font-semibold hover:bg-gray-50 transition-colors shadow-sm flex items-center gap-2">
          <PrinterIcon class="w-5 h-5"/> In Báo Cáo
        </button>
        <button @click="handleExportExcel" class="bg-emerald-600 hover:bg-emerald-700 text-white px-4 py-2.5 rounded-lg flex items-center gap-2 text-sm font-semibold transition-colors shadow-sm">
          <ArrowDownTrayIcon class="w-5 h-5" /> Xuất Excel
        </button>
      </div>
    </div>

    <div class="bg-white p-4 md:p-5 rounded-2xl border border-gray-200 shadow-sm">
      <h3 class="text-sm font-bold text-gray-800 mb-4 flex items-center gap-2">
        <FunnelIcon class="w-5 h-5 text-primary-600"/> Điều kiện lọc báo cáo
      </h3>
      <div class="grid grid-cols-1 sm:grid-cols-2 md:grid-cols-4 gap-4">
        <div>
          <label class="block text-xs font-bold text-gray-700 mb-1">Từ ngày</label>
          <div class="relative">
            <div class="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none"><CalendarDaysIcon class="w-4 h-4 text-gray-400" /></div>
            <input v-model="filterForm.fromDate" type="date" class="block w-full pl-9 pr-3 py-2 border border-gray-300 rounded-lg text-sm focus:ring-1 focus:ring-primary-500 outline-none">
          </div>
        </div>
        <div>
          <label class="block text-xs font-bold text-gray-700 mb-1">Đến ngày</label>
          <div class="relative">
            <div class="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none"><CalendarDaysIcon class="w-4 h-4 text-gray-400" /></div>
            <input v-model="filterForm.toDate" type="date" class="block w-full pl-9 pr-3 py-2 border border-gray-300 rounded-lg text-sm focus:ring-1 focus:ring-primary-500 outline-none">
          </div>
        </div>
        <div>
          <label class="block text-xs font-bold text-gray-700 mb-1">Chi nhánh / Kho</label>
          <select v-model="filterForm.warehouse" class="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm focus:ring-1 focus:ring-primary-500 outline-none cursor-pointer font-bold text-primary-700 bg-primary-50">
            <option value="">TỔNG HỢP TẤT CẢ CÁC KHO</option>
            <option value="Tổng kho Miền Bắc">Tổng kho Miền Bắc</option>
            <option value="Chi nhánh Miền Nam">Chi nhánh Miền Nam</option>
            <option value="Trung tâm Đà Nẵng">Trung tâm Đà Nẵng</option>
          </select>
        </div>
        <div>
          <label class="block text-xs font-bold text-gray-700 mb-1">Nhóm hàng hóa</label>
          <select v-model="filterForm.category" class="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm focus:ring-1 focus:ring-primary-500 outline-none cursor-pointer">
            <option value="">Tất cả danh mục</option>
            <option value="Điện tử">Điện tử</option>
            <option value="Tiêu dùng">Tiêu dùng</option>
          </select>
        </div>
      </div>
      <div class="mt-4 flex justify-end">
        <button class="bg-slate-800 hover:bg-slate-900 text-white px-5 py-2 rounded-lg text-sm font-semibold transition-colors flex items-center gap-2 shadow-sm">
          <DocumentChartBarIcon class="w-5 h-5"/> Xem Báo Cáo
        </button>
      </div>
    </div>

    <div class="bg-white rounded-xl border border-gray-200 shadow-sm overflow-hidden mt-2">
      <div class="bg-gray-50 px-5 py-3 border-b border-gray-200 flex flex-col sm:flex-row sm:items-center justify-between">
        <span class="text-sm font-bold text-gray-800">
          SỐ LIỆU TỪ: <span class="text-primary-600">{{ filterForm.fromDate }}</span> ĐẾN <span class="text-primary-600">{{ filterForm.toDate }}</span>
          <span class="ml-2 px-2 py-0.5 bg-gray-200 rounded text-xs text-gray-600">{{ filterForm.warehouse || 'TỔNG HỢP CÁC KHO' }}</span>
        </span>
        <span class="text-xs font-medium text-gray-500 mt-1 sm:mt-0">Đơn vị tính: Tùy theo mặt hàng</span>
      </div>

      <div class="w-full overflow-x-auto custom-scrollbar">
        <table class="min-w-[1100px] w-full border-collapse">
          <thead class="bg-slate-100">
            <tr>
              <th colspan="4" class="px-4 py-2 border border-gray-200 text-center text-xs font-bold text-slate-700 uppercase tracking-wider bg-slate-200/50">Thông tin hàng hóa & Vị trí</th>
              <th class="px-4 py-2 border border-gray-200 text-center text-xs font-bold text-slate-700 uppercase tracking-wider">Đầu kỳ</th>
              <th class="px-4 py-2 border border-gray-200 text-center text-xs font-bold text-emerald-800 uppercase tracking-wider bg-emerald-50">Nhập trong kỳ</th>
              <th class="px-4 py-2 border border-gray-200 text-center text-xs font-bold text-amber-800 uppercase tracking-wider bg-amber-50">Xuất trong kỳ</th>
              <th class="px-4 py-2 border border-gray-200 text-center text-xs font-bold text-blue-800 uppercase tracking-wider bg-blue-50">Cuối kỳ</th>
            </tr>
            <tr>
              <th class="px-4 py-3 border border-gray-200 text-left text-xs font-bold text-slate-500 uppercase w-32">Mã SKU</th>
              <th class="px-4 py-3 border border-gray-200 text-left text-xs font-bold text-slate-500 uppercase">Tên Hàng</th>
              <th class="px-4 py-3 border border-gray-200 text-left text-xs font-bold text-slate-500 uppercase w-48">Thuộc Kho</th>
              <th class="px-4 py-3 border border-gray-200 text-center text-xs font-bold text-slate-500 uppercase w-20">ĐVT</th>
              
              <th class="px-4 py-3 border border-gray-200 text-right text-xs font-bold text-slate-500 uppercase w-28">Số lượng (1)</th>
              <th class="px-4 py-3 border border-gray-200 text-right text-xs font-bold text-emerald-600 uppercase w-28 bg-emerald-50/30">Số lượng (2)</th>
              <th class="px-4 py-3 border border-gray-200 text-right text-xs font-bold text-amber-600 uppercase w-28 bg-amber-50/30">Số lượng (3)</th>
              <th class="px-4 py-3 border border-gray-200 text-right text-xs font-bold text-blue-600 uppercase w-28 bg-blue-50/30">SL Tồn (1+2-3)</th>
            </tr>
          </thead>
          
          <tbody class="bg-white">
            <tr v-if="filteredReport.length === 0">
              <td colspan="8" class="px-6 py-16 text-center border border-gray-200">
                <DocumentChartBarIcon class="w-12 h-12 text-gray-300 mx-auto mb-3" />
                <h3 class="text-base font-semibold text-gray-700">Chưa có dữ liệu Báo cáo</h3>
                <p class="text-sm text-gray-500 mt-1">Vui lòng đợi kết nối API để lấy số liệu Nhập Xuất Tồn.</p>
              </td>
            </tr>

            <tr v-for="item in filteredReport" :key="item.id" class="hover:bg-slate-50 transition-colors group">
              <td class="px-4 py-3 border border-gray-200 text-sm font-bold text-slate-700">{{ item.sku }}</td>
              <td class="px-4 py-3 border border-gray-200"><div class="flex flex-col"><span class="text-sm font-bold text-gray-900 truncate max-w-[200px]">{{ item.name }}</span><span class="text-[10px] text-gray-500">{{ item.category }}</span></div></td>
              <td class="px-4 py-3 border border-gray-200 text-xs font-bold text-gray-600 flex items-center gap-1.5 h-full mt-1.5"><BuildingStorefrontIcon class="w-3.5 h-3.5 text-gray-400"/>{{ item.warehouse }}</td>
              <td class="px-4 py-3 border border-gray-200 text-center text-xs font-medium text-gray-500">{{ item.unit }}</td>
              <td class="px-4 py-3 border border-gray-200 text-right text-sm font-semibold text-gray-700">{{ item.startQty }}</td>
              <td class="px-4 py-3 border border-gray-200 text-right text-sm font-bold text-emerald-600 bg-emerald-50/10 group-hover:bg-emerald-50/30">{{ item.inQty > 0 ? `+${item.inQty}` : '-' }}</td>
              <td class="px-4 py-3 border border-gray-200 text-right text-sm font-bold text-amber-600 bg-amber-50/10 group-hover:bg-amber-50/30">{{ item.outQty > 0 ? `-${item.outQty}` : '-' }}</td>
              <td class="px-4 py-3 border border-gray-200 text-right text-base font-bold text-blue-700 bg-blue-50/10 group-hover:bg-blue-50/30">{{ item.endQty }}</td>
            </tr>
          </tbody>

          <tfoot class="bg-slate-100 border-t-2 border-slate-400 font-bold">
            <tr>
              <td colspan="4" class="px-4 py-4 border border-gray-200 text-right text-sm text-slate-800 uppercase">Tổng cộng:</td>
              <td class="px-4 py-4 border border-gray-200 text-right text-sm text-slate-800">{{ totalStart }}</td>
              <td class="px-4 py-4 border border-gray-200 text-right text-sm text-emerald-600 bg-emerald-50/50">+{{ totalIn }}</td>
              <td class="px-4 py-4 border border-gray-200 text-right text-sm text-amber-600 bg-amber-50/50">-{{ totalOut }}</td>
              <td class="px-4 py-4 border border-gray-200 text-right text-base text-blue-700 bg-blue-50/50">{{ totalEnd }}</td>
            </tr>
          </tfoot>
        </table>
      </div>
    </div>
  </div>
</template>

<style scoped>
.custom-scrollbar::-webkit-scrollbar { height: 6px; width: 6px; }
.custom-scrollbar::-webkit-scrollbar-track { background: transparent; }
.custom-scrollbar::-webkit-scrollbar-thumb { background: #cbd5e1; border-radius: 10px; }
</style>