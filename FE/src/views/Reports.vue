<script setup>
import { ref, computed } from 'vue'
import { 
  DocumentChartBarIcon, FunnelIcon, 
  ArrowDownTrayIcon, PrinterIcon,
  CalendarDaysIcon
} from '@heroicons/vue/24/outline'

// === 1. TÙY CHỌN LỌC BÁO CÁO ===
const filterForm = ref({
  fromDate: '2026-03-01',
  toDate: '2026-03-31',
  warehouse: '',
  category: ''
})

// === 2. DỮ LIỆU MẪU: BÁO CÁO NHẬP XUẤT TỒN (NXT) ===
const reportData = ref([
  {
    id: 1, sku: 'SKU-001', name: 'Laptop Dell XPS 15', unit: 'Cái', category: 'Điện tử',
    startQty: 100, // Tồn đầu kỳ
    inQty: 50,     // Nhập trong kỳ
    outQty: 30,    // Xuất trong kỳ
    endQty: 120    // Tồn cuối kỳ (= Đầu + Nhập - Xuất)
  },
  {
    id: 2, sku: 'SKU-002', name: 'Bột giặt OMO 3.6kg', unit: 'Túi', category: 'Tiêu dùng',
    startQty: 2000, 
    inQty: 500,     
    outQty: 1500,    
    endQty: 1000    
  },
  {
    id: 3, sku: 'SKU-003', name: 'Màn hình LG 27"', unit: 'Cái', category: 'Điện tử',
    startQty: 0, 
    inQty: 100,     
    outQty: 100,    
    endQty: 0    
  },
  {
    id: 4, sku: 'SKU-004', name: 'Bàn phím cơ Keychron K8', unit: 'Cái', category: 'Phụ kiện',
    startQty: 50, 
    inQty: 0,     
    outQty: 45,    
    endQty: 5    
  }
])

// Tính toán tổng cộng cho footer
const totalStart = computed(() => reportData.value.reduce((sum, item) => sum + item.startQty, 0))
const totalIn = computed(() => reportData.value.reduce((sum, item) => sum + item.inQty, 0))
const totalOut = computed(() => reportData.value.reduce((sum, item) => sum + item.outQty, 0))
const totalEnd = computed(() => reportData.value.reduce((sum, item) => sum + item.endQty, 0))

// Hàm xử lý xuất file
const handleExportExcel = () => {
  alert(`Đang xuất Báo cáo NXT từ ngày ${filterForm.value.fromDate} đến ${filterForm.value.toDate} ra file Excel...`)
}
</script>

<template>
  <div class="space-y-5 md:space-y-6 animate-fade-in pb-10 px-0 md:px-1 relative">
    
    <div class="flex flex-col sm:flex-row sm:items-center justify-between gap-4">
      <div>
        <h2 class="text-xl md:text-2xl font-bold text-gray-800">Báo cáo Nhập - Xuất - Tồn (NXT)</h2>
        <p class="text-xs md:text-sm text-gray-500 mt-1">Bảng kê chi tiết biến động hàng hóa trong một khoảng thời gian</p>
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
          <select v-model="filterForm.warehouse" class="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm focus:ring-1 focus:ring-primary-500 outline-none cursor-pointer">
            <option value="">Tất cả kho</option>
            <option value="HN">Tổng kho Miền Bắc</option>
            <option value="HCM">Chi nhánh Miền Nam</option>
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
        <span class="text-sm font-bold text-gray-800">SỐ LIỆU TỪ: <span class="text-primary-600">{{ filterForm.fromDate }}</span> ĐẾN <span class="text-primary-600">{{ filterForm.toDate }}</span></span>
        <span class="text-xs font-medium text-gray-500 mt-1 sm:mt-0">Đơn vị tính: Tùy theo mặt hàng</span>
      </div>

      <div class="w-full overflow-x-auto custom-scrollbar">
        <table class="min-w-[1000px] w-full border-collapse">
          <thead class="bg-slate-100">
            <tr>
              <th colspan="3" class="px-4 py-2 border border-gray-200 text-center text-xs font-bold text-slate-700 uppercase tracking-wider bg-slate-200/50">Thông tin hàng hóa</th>
              <th class="px-4 py-2 border border-gray-200 text-center text-xs font-bold text-slate-700 uppercase tracking-wider">Đầu kỳ</th>
              <th class="px-4 py-2 border border-gray-200 text-center text-xs font-bold text-slate-700 uppercase tracking-wider bg-emerald-50 text-emerald-800">Nhập trong kỳ</th>
              <th class="px-4 py-2 border border-gray-200 text-center text-xs font-bold text-slate-700 uppercase tracking-wider bg-amber-50 text-amber-800">Xuất trong kỳ</th>
              <th class="px-4 py-2 border border-gray-200 text-center text-xs font-bold text-slate-700 uppercase tracking-wider bg-blue-50 text-blue-800">Cuối kỳ</th>
            </tr>
            <tr>
              <th class="px-4 py-3 border border-gray-200 text-left text-xs font-bold text-slate-500 uppercase w-32">Mã SKU</th>
              <th class="px-4 py-3 border border-gray-200 text-left text-xs font-bold text-slate-500 uppercase">Tên Hàng</th>
              <th class="px-4 py-3 border border-gray-200 text-center text-xs font-bold text-slate-500 uppercase w-20">ĐVT</th>
              
              <th class="px-4 py-3 border border-gray-200 text-right text-xs font-bold text-slate-500 uppercase w-28">Số lượng (1)</th>
              <th class="px-4 py-3 border border-gray-200 text-right text-xs font-bold text-emerald-600 uppercase w-28 bg-emerald-50/30">Số lượng (2)</th>
              <th class="px-4 py-3 border border-gray-200 text-right text-xs font-bold text-amber-600 uppercase w-28 bg-amber-50/30">Số lượng (3)</th>
              <th class="px-4 py-3 border border-gray-200 text-right text-xs font-bold text-blue-600 uppercase w-28 bg-blue-50/30">SL Tồn (1+2-3)</th>
            </tr>
          </thead>
          
          <tbody class="bg-white">
            <tr v-if="reportData.length === 0">
              <td colspan="7" class="px-6 py-12 text-center text-sm text-gray-500 italic border border-gray-200">
                Không phát sinh dữ liệu trong kỳ báo cáo này.
              </td>
            </tr>

            <tr v-for="item in reportData" :key="item.id" class="hover:bg-slate-50 transition-colors group">
              <td class="px-4 py-3 border border-gray-200 text-sm font-bold text-slate-700">{{ item.sku }}</td>
              <td class="px-4 py-3 border border-gray-200">
                <div class="flex flex-col">
                  <span class="text-sm font-bold text-gray-900 truncate max-w-[200px]" :title="item.name">{{ item.name }}</span>
                  <span class="text-[10px] text-gray-500">{{ item.category }}</span>
                </div>
              </td>
              <td class="px-4 py-3 border border-gray-200 text-center text-xs font-medium text-gray-500">{{ item.unit }}</td>
              
              <td class="px-4 py-3 border border-gray-200 text-right text-sm font-semibold text-gray-700">{{ item.startQty }}</td>
              
              <td class="px-4 py-3 border border-gray-200 text-right text-sm font-bold text-emerald-600 bg-emerald-50/10 group-hover:bg-emerald-50/30">
                {{ item.inQty > 0 ? `+${item.inQty}` : '-' }}
              </td>
              
              <td class="px-4 py-3 border border-gray-200 text-right text-sm font-bold text-amber-600 bg-amber-50/10 group-hover:bg-amber-50/30">
                {{ item.outQty > 0 ? `-${item.outQty}` : '-' }}
              </td>
              
              <td class="px-4 py-3 border border-gray-200 text-right text-base font-bold text-blue-700 bg-blue-50/10 group-hover:bg-blue-50/30">
                {{ item.endQty }}
              </td>
            </tr>
          </tbody>

          <tfoot class="bg-slate-100 border-t-2 border-slate-300 font-bold">
            <tr>
              <td colspan="3" class="px-4 py-4 border border-gray-200 text-right text-sm text-slate-800 uppercase">Tổng cộng toàn kho:</td>
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
.custom-scrollbar:hover::-webkit-scrollbar-thumb { background: #94a3b8; }
</style>