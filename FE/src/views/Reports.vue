<script setup>
import { ref, computed, onMounted } from 'vue'
import { 
  DocumentChartBarIcon, FunnelIcon, 
  ArrowDownTrayIcon, PrinterIcon,
  CalendarDaysIcon, BuildingStorefrontIcon, ArrowPathIcon
} from '@heroicons/vue/24/outline'

const STOCK_API = 'https://localhost:7139/api/Stock'
const INBOUND_API = 'https://localhost:7139/api/Inbound'
const CHECK_API = 'https://localhost:7139/api/InvCheck'
const PROD_API = 'https://localhost:7139/api/Products'
const BRANCH_API = 'https://localhost:7139/api/Branches'

const getAuthHeaders = () => ({ 'Content-Type': 'application/json', 'Authorization': 'Bearer ' + (localStorage.getItem('authToken') || '') })

// === 1. TÙY CHỌN LỌC BÁO CÁO ===
// Tự động set ngày đầu tháng đến ngày hiện tại
const today = new Date();
const firstDayOfMonth = new Date(today.getFullYear(), today.getMonth(), 2).toISOString().split('T')[0];
const todayStr = today.toISOString().split('T')[0];

const filterForm = ref({
  fromDate: firstDayOfMonth,
  toDate: todayStr,
  warehouseId: '',
  category: ''
})

const isLoading = ref(false)
const rawData = ref({
    stocks: [],
    inbounds: [],
    checks: [],
    products: [],
    warehouses: []
})

// === 2. GỌI API LẤY TOÀN BỘ DATA ===
const fetchData = async () => {
  isLoading.value = true
  try {
    const headers = getAuthHeaders()
    
    // Lấy Danh sách Kho
    try {
        const branchRes = await fetch(BRANCH_API, { headers })
        if (branchRes.ok) {
            const branches = await branchRes.json()
            let allWh = []
            for (const b of branches) {
                const bId = b.id || b.Id;
                const whRes = await fetch(`${BRANCH_API}/${bId}/warehouses-detail`, { headers })
                if(whRes.ok) {
                    const whData = await whRes.json()
                    allWh = [...allWh, ...whData.map(w => ({ id: w.warehouseId, name: w.whname }))]
                }
            }
            rawData.value.warehouses = allWh
        }
    } catch(e) { console.error("Lỗi lấy kho:", e) }

    const [stockRes, inbRes, chkRes, prodRes] = await Promise.all([
      fetch(STOCK_API, { headers }), fetch(INBOUND_API, { headers }), 
      fetch(CHECK_API, { headers }), fetch(PROD_API, { headers })
    ])
    
    if (prodRes.ok) rawData.value.products = await prodRes.json()
    if (stockRes.ok) rawData.value.stocks = await stockRes.json()
    if (inbRes.ok) rawData.value.inbounds = await inbRes.json()
    if (chkRes.ok) rawData.value.checks = await chkRes.json()

  } catch (error) { console.error('Lỗi tải dữ liệu Báo cáo:', error) }
  finally { isLoading.value = false }
}

// === 3. NHÀO NẶN DỮ LIỆU THÀNH BÁO CÁO NHẬP XUẤT TỒN ===
const reportData = computed(() => {
    if (rawData.value.products.length === 0) return [];
    
    const fFrom = new Date(filterForm.value.fromDate).getTime();
    const fTo = new Date(filterForm.value.toDate).setHours(23, 59, 59, 999); // Lấy hết ngày To
    const fWhId = filterForm.value.warehouseId ? parseInt(filterForm.value.warehouseId) : null;
    
    // Gom tất cả giao dịch (Nhập, Xuất, Kiểm kê bù trừ) lại thành 1 mảng
    let allTransactions = [];

    // Nhập kho
    rawData.value.inbounds.filter(i => i.status === 'completed').forEach(inb => {
        const txTime = new Date(inb.date).getTime();
        (inb.items || []).forEach(item => {
            allTransactions.push({
                variantId: item.variantId || item.VariantId,
                warehouseId: inb.warehouseId,
                time: txTime,
                qty: item.qty || 0, // Nhập là số dương
                type: 'IN'
            });
        });
    });

    // Kiểm kê (Chỉ lấy phần chênh lệch bù trừ)
    rawData.value.checks.filter(c => c.status === 'completed').forEach(chk => {
        const txTime = new Date(chk.date || chk.Date).getTime(); // Backend chưa trả CheckDate, tạm giả lập
        (chk.items || []).forEach(item => {
            if(item.diffQty !== 0) {
                allTransactions.push({
                    variantId: item.variantId || item.VariantId,
                    warehouseId: chk.warehouseId,
                    time: txTime,
                    qty: item.diffQty, // Thừa là +, Thiếu là -
                    type: item.diffQty > 0 ? 'IN' : 'OUT'
                });
            }
        });
    });

    // BƯỚC QUAN TRỌNG: Quét qua từng sản phẩm tại từng Kho
    let finalReport = [];

    rawData.value.products.forEach(prod => {
        const pId = prod.id || prod.Id;
        if(filterForm.value.category && prod.categoryName !== filterForm.value.category) return; // Lọc danh mục

        // Lấy danh sách các kho đang có giao dịch hoặc tồn kho của sản phẩm này
        const involvedWhIds = new Set();
        rawData.value.stocks.filter(s => (s.variantId || s.VariantId) === pId).forEach(s => involvedWhIds.add(s.warehouseId));
        allTransactions.filter(tx => tx.variantId === pId).forEach(tx => involvedWhIds.add(tx.warehouseId));

        involvedWhIds.forEach(whId => {
            if (fWhId && whId !== fWhId) return; // Lọc theo kho

            const whName = rawData.value.warehouses.find(w => w.id === whId)?.name || `Kho ${whId}`;
            
            // Tính số lượng Nhập/Xuất TRONG KỲ (Từ FromDate đến ToDate)
            const txInPeriod = allTransactions.filter(tx => tx.variantId === pId && tx.warehouseId === whId && tx.time >= fFrom && tx.time <= fTo);
            let inQty = 0; let outQty = 0;
            txInPeriod.forEach(tx => {
                if(tx.qty > 0) inQty += tx.qty;
                if(tx.qty < 0) outQty += Math.abs(tx.qty);
            });

            // Tính TỒN HIỆN TẠI (Lấy thẳng từ bảng Stock)
            const currentStockLines = rawData.value.stocks.filter(s => (s.variantId || s.VariantId) === pId && s.warehouseId === whId);
            const currentEndQty = currentStockLines.reduce((sum, s) => sum + (s.qty || s.Quantity || 0), 0);

            // TỒN CUỐI KỲ: Bằng Tồn hiện tại, trừ đi các giao dịch xảy ra SAU ToDate (Nếu sếp xem quá khứ)
            const txAfterPeriod = allTransactions.filter(tx => tx.variantId === pId && tx.warehouseId === whId && tx.time > fTo);
            const netChangeAfter = txAfterPeriod.reduce((sum, tx) => sum + tx.qty, 0);
            const endQty = currentEndQty - netChangeAfter;

            // TỒN ĐẦU KỲ: Bằng Tồn cuối kỳ - Nhập trong kỳ + Xuất trong kỳ
            const startQty = endQty - inQty + outQty;

            // Chỉ đưa vào báo cáo nếu có Số dư đầu, Cuối, hoặc có phát sinh giao dịch
            if (startQty > 0 || endQty > 0 || inQty > 0 || outQty > 0) {
                finalReport.push({
                    id: `${pId}_${whId}`,
                    sku: prod.sku || prod.Sku,
                    name: prod.name || prod.Name,
                    category: prod.categoryName || prod.CategoryName || 'Chung',
                    warehouseId: whId,
                    warehouse: whName,
                    unit: prod.unit || prod.Unit || 'Thùng',
                    startQty, inQty, outQty, endQty
                });
            }
        });
    });

    return finalReport.sort((a,b) => a.sku.localeCompare(b.sku));
})

// Tính toán tổng cộng bám theo báo cáo
const totalStart = computed(() => reportData.value.reduce((sum, item) => sum + item.startQty, 0))
const totalIn = computed(() => reportData.value.reduce((sum, item) => sum + item.inQty, 0))
const totalOut = computed(() => reportData.value.reduce((sum, item) => sum + item.outQty, 0))
const totalEnd = computed(() => reportData.value.reduce((sum, item) => sum + item.endQty, 0))

const handleExportExcel = () => {
  const whName = rawData.value.warehouses.find(w => w.id == filterForm.value.warehouseId)?.name || 'TẤT CẢ CÁC KHO'
  alert(`Đang xuất Báo cáo NXT - [${whName}] từ ${filterForm.value.fromDate} đến ${filterForm.value.toDate} ra Excel...`)
}

onMounted(() => fetchData())
</script>

<template>
  <div class="space-y-5 md:space-y-6 animate-fade-in pb-10 px-0 md:px-1 relative">
    
    <div class="flex flex-col sm:flex-row sm:items-center justify-between gap-4">
      <div>
        <h2 class="text-xl md:text-2xl font-bold text-gray-800">Báo cáo Nhập - Xuất - Tồn (N-X-T)</h2>
        <p class="text-xs md:text-sm text-gray-500 mt-1">Bảng kê biến động số lượng hàng hóa theo thời gian tại các chi nhánh</p>
      </div>
      <div class="flex gap-2">
        <button @click="fetchData" class="bg-white border border-gray-300 text-gray-700 px-4 py-2.5 rounded-lg text-sm font-semibold hover:bg-gray-50 transition-colors shadow-sm flex items-center gap-2">
          <ArrowPathIcon class="w-5 h-5" :class="{'animate-spin': isLoading}"/> Tải lại dữ liệu
        </button>
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
          <select v-model="filterForm.warehouseId" class="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm focus:ring-1 focus:ring-primary-500 outline-none cursor-pointer font-bold text-primary-700 bg-primary-50">
            <option value="">TỔNG HỢP TẤT CẢ CÁC KHO</option>
            <option v-for="wh in rawData.warehouses" :key="wh.id" :value="wh.id">{{ wh.name }}</option>
          </select>
        </div>
        <div>
          <label class="block text-xs font-bold text-gray-700 mb-1">Nhóm hàng hóa</label>
          <select v-model="filterForm.category" class="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm focus:ring-1 focus:ring-primary-500 outline-none cursor-pointer">
            <option value="">Tất cả danh mục</option>
            <option value="Đồ Lạnh">Đồ Lạnh</option>
            <option value="Đồ Khô">Đồ Khô</option>
          </select>
        </div>
      </div>
    </div>

    <div class="bg-white rounded-xl border border-gray-200 shadow-sm overflow-hidden mt-2">
      <div class="bg-gray-50 px-5 py-3 border-b border-gray-200 flex flex-col sm:flex-row sm:items-center justify-between">
        <span class="text-sm font-bold text-gray-800 flex items-center gap-2">
          SỐ LIỆU TỪ: <span class="text-primary-600 bg-white px-2 py-0.5 rounded border">{{ filterForm.fromDate }}</span> ĐẾN <span class="text-primary-600 bg-white px-2 py-0.5 rounded border">{{ filterForm.toDate }}</span>
          <span class="ml-2 px-2 py-1 bg-gray-200 rounded text-xs text-gray-700 uppercase tracking-wider">{{ filterForm.warehouseId ? rawData.warehouses.find(w => w.id == filterForm.warehouseId)?.name : 'TỔNG HỢP CÁC KHO' }}</span>
        </span>
        <span class="text-xs font-medium text-gray-500 mt-2 sm:mt-0 italic">Lưu ý: Chỉ thống kê các phiếu nhập/xuất đã hoàn thành</span>
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
            <tr v-if="isLoading"><td colspan="8" class="px-6 py-12 text-center border border-gray-200 text-gray-500 font-bold">Đang tổng hợp số liệu...</td></tr>
            <tr v-else-if="reportData.length === 0">
              <td colspan="8" class="px-6 py-16 text-center border border-gray-200">
                <DocumentChartBarIcon class="w-12 h-12 text-gray-300 mx-auto mb-3" />
                <h3 class="text-base font-semibold text-gray-700">Chưa có dữ liệu Báo cáo</h3>
                <p class="text-sm text-gray-500 mt-1">Không phát sinh giao dịch hoặc tồn kho trong thời gian này.</p>
              </td>
            </tr>

            <tr v-for="item in reportData" :key="item.id" class="hover:bg-slate-50 transition-colors group">
              <td class="px-4 py-3 border border-gray-200 text-sm font-bold text-slate-700">{{ item.sku }}</td>
              <td class="px-4 py-3 border border-gray-200"><div class="flex flex-col"><span class="text-sm font-bold text-gray-900 truncate max-w-[200px]">{{ item.name }}</span><span class="text-[10px] text-gray-500">{{ item.category }}</span></div></td>
              <td class="px-4 py-3 border border-gray-200 text-xs font-bold text-gray-600 flex items-center gap-1.5 h-full mt-1.5"><BuildingStorefrontIcon class="w-3.5 h-3.5 text-gray-400"/>{{ item.warehouse }}</td>
              <td class="px-4 py-3 border border-gray-200 text-center text-xs font-bold text-gray-500 uppercase">{{ item.unit }}</td>
              <td class="px-4 py-3 border border-gray-200 text-right text-sm font-semibold text-gray-700">{{ item.startQty }}</td>
              <td class="px-4 py-3 border border-gray-200 text-right text-sm font-bold text-emerald-600 bg-emerald-50/10 group-hover:bg-emerald-50/30">{{ item.inQty > 0 ? `+${item.inQty}` : '-' }}</td>
              <td class="px-4 py-3 border border-gray-200 text-right text-sm font-bold text-amber-600 bg-amber-50/10 group-hover:bg-amber-50/30">{{ item.outQty > 0 ? `-${item.outQty}` : '-' }}</td>
              <td class="px-4 py-3 border border-gray-200 text-right text-base font-bold text-blue-700 bg-blue-50/10 group-hover:bg-blue-50/30">{{ item.endQty }}</td>
            </tr>
          </tbody>

          <tfoot class="bg-slate-100 border-t-2 border-slate-400 font-bold">
            <tr>
              <td colspan="4" class="px-4 py-4 border border-gray-200 text-right text-sm text-slate-800 uppercase tracking-widest">Tổng cộng toàn bộ (Thùng):</td>
              <td class="px-4 py-4 border border-gray-200 text-right text-base text-slate-800">{{ totalStart }}</td>
              <td class="px-4 py-4 border border-gray-200 text-right text-base text-emerald-600 bg-emerald-50/50">+{{ totalIn }}</td>
              <td class="px-4 py-4 border border-gray-200 text-right text-base text-amber-600 bg-amber-50/50">-{{ totalOut }}</td>
              <td class="px-4 py-4 border border-gray-200 text-right text-xl text-blue-700 bg-blue-50/50">{{ totalEnd }}</td>
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
.animate-fade-in { animation: fadeIn 0.2s ease-out; }
@keyframes fadeIn { from { opacity: 0; transform: translateY(5px); } to { opacity: 1; transform: translateY(0); } }
</style>