<script setup>
import { ref, computed, onMounted } from 'vue'
import { 
  MagnifyingGlassIcon, PlusIcon, EyeIcon, 
  XMarkIcon, ShieldExclamationIcon, TrashIcon,
  PrinterIcon, DocumentTextIcon, ArrowDownTrayIcon,
  CheckCircleIcon, XCircleIcon, FireIcon
} from '@heroicons/vue/24/outline'

const DEFECT_API = 'https://localhost:7139/api/Defect'
const STOCK_API = 'https://localhost:7139/api/Stock'
const PROD_API = 'https://localhost:7139/api/Products'
const LOC_API = 'https://localhost:7139/api/Locations'

const getAuthHeaders = () => ({ 'Content-Type': 'application/json', 'Authorization': 'Bearer ' + (localStorage.getItem('authToken') || '') })

const currentUserRole = ref(localStorage.getItem('role') || 'ql_kho') 
const canProcess = computed(() => ['admin', 'giam_doc', 'ql_kho'].includes(currentUserRole.value))
const canExport = computed(() => ['admin', 'giam_doc', 'gd_chi_nhanh', 'ql_kho'].includes(currentUserRole.value))

const defects = ref([]); const productsList = ref([]); const locationsList = ref([]); const stockList = ref([])
const isLoading = ref(false)

const getToday = () => new Date().toISOString().split('T')[0]

const fetchData = async () => {
  isLoading.value = true
  try {
    const headers = getAuthHeaders()
    const [defRes, stockRes, prodRes, locRes] = await Promise.all([
      fetch(DEFECT_API, { headers }), fetch(STOCK_API, { headers }), 
      fetch(PROD_API, { headers }), fetch(LOC_API, { headers })
    ])
    
    if (prodRes.ok) productsList.value = await prodRes.json()
    if (locRes.ok) locationsList.value = await locRes.json()
    
    if (stockRes.ok) {
      const rawStocks = await stockRes.json();
      stockList.value = rawStocks.map(s => {
        const prod = productsList.value.find(p => p.id === s.variantId || p.Id === s.variantId) || {};
        const loc = locationsList.value.find(l => l.id === s.locationId || l.Id === s.locationId) || {};
        return {
          stockId: s.id, variantId: s.variantId, qtyAvailable: s.qty || 0, nsx: s.nsx || '', hsd: s.hsd || '',
          sku: prod.sku || prod.Sku || 'N/A', name: prod.name || prod.Name || 'Lỗi', unit: prod.unit || prod.Unit || 'Thùng',
          locationId: s.locationId, locationCode: loc.code || loc.Code || 'Khu Chờ Nhập'
        }
      })
    }

    if (defRes.ok) {
      const rawDef = await defRes.json()
      defects.value = rawDef.map(r => {
        if (!r.code && r.id) r.code = `HL${r.id.toString().padStart(4, '0')}`;
        const firstItem = r.items && r.items.length > 0 ? r.items[0] : null;
        if(firstItem) {
             const prod = productsList.value.find(p => p.id === firstItem.variantId || p.Id === firstItem.variantId) || {};
             r.sku = prod.sku || prod.Sku || 'N/A';
             r.name = prod.name || prod.Name || 'N/A';
             r.qty = r.items.reduce((s, i) => s + (i.qty || 0), 0); 
             r.reason = firstItem.reason || 'Không rõ nguyên nhân';
        }
        return r;
      })
    }
  } catch (error) { console.error(error) } finally { isLoading.value = false }
}

const searchQuery = ref(''); const filterStatus = ref('')
const filteredDefects = computed(() => {
  return defects.value.filter(d => {
    const code = d.code || d.Code || ''; const name = d.name || ''; const sku = d.sku || '';
    const matchSearch = code.toLowerCase().includes(searchQuery.value.toLowerCase()) || name.toLowerCase().includes(searchQuery.value.toLowerCase()) || sku.toLowerCase().includes(searchQuery.value.toLowerCase())
    return matchSearch && (filterStatus.value === '' || d.status === filterStatus.value)
  })
})

const showModal = ref(false); const modalMode = ref('add') 
const formData = ref({ id: 0, code: '', date: getToday(), items: [], note: '', status: 'pending' })

const openModal = (mode, defect = null) => {
  modalMode.value = mode
  if (defect) {
    const safeItems = (defect.items || defect.Items || []).map(i => {
      const variantId = i.variantId || i.VariantId;
      const prod = productsList.value.find(p => p.id === variantId || p.Id === variantId) || {};
      const loc = locationsList.value.find(l => l.id === i.locationId) || {};
      return {
        variantId: variantId, sku: prod.sku || prod.Sku, name: prod.name || prod.Name, unit: prod.unit || prod.Unit, 
        qty: i.qty || i.Qty, maxQty: i.qty || i.Qty, reason: i.reason || '',
        locationId: i.locationId, locationCode: loc.code || loc.Code || 'Khu Chờ Nhập',
        nsx: i.nsx || i.Nsx ? (i.nsx || i.Nsx).split('T')[0] : '', hsd: i.hsd || i.Hsd ? (i.hsd || i.Hsd).split('T')[0] : ''
      };
    });
    formData.value = { id: defect.id || defect.Id, code: defect.code || defect.Code, date: defect.date || defect.Date, items: safeItems, note: defect.note || defect.Note, status: defect.status || defect.Status };
  } else { formData.value = { id: 0, code: '', date: getToday(), items: [], note: '', status: 'pending' } }
  selectedVariantIdsToAdd.value = []; stockSearchQuery.value = ''; showModal.value = true
}

const closeModal = () => showModal.value = false

const getStatusBadge = (status) => {
  switch(status) {
    case 'pending': return { text: 'Chờ xử lý', class: 'bg-amber-100 text-amber-700 border-amber-200' }
    case 'completed': return { text: 'Đã hủy khỏi kho', class: 'bg-red-100 text-red-700 border-red-200' }
    default: return { text: 'Khác', class: 'bg-gray-100 text-gray-500' }
  }
}

// ==========================================
// LOGIC GỘP SẢN PHẨM Ở BƯỚC 1 THEO Ý SẾP
// ==========================================
const stockSearchQuery = ref(''); const selectedVariantIdsToAdd = ref([]) 

// Gom nhóm các dòng tồn kho lại theo VariantId (SKU)
const groupedStocks = computed(() => {
    const map = {};
    stockList.value.forEach(s => {
        if (!map[s.variantId]) {
            map[s.variantId] = { variantId: s.variantId, sku: s.sku, name: s.name, unit: s.unit, totalQty: 0 };
        }
        map[s.variantId].totalQty += s.qtyAvailable;
    });
    return Object.values(map);
})

const filteredGroupedStockList = computed(() => {
  if (!stockSearchQuery.value) return groupedStocks.value;
  return groupedStocks.value.filter(s => s.sku.toLowerCase().includes(stockSearchQuery.value.toLowerCase()) || s.name.toLowerCase().includes(stockSearchQuery.value.toLowerCase()));
})

const handleAddMultipleStocks = () => {
  if (selectedVariantIdsToAdd.value.length === 0) return;
  
  selectedVariantIdsToAdd.value.forEach(vid => {
    // 1. Tìm tất cả các VỊ TRÍ đang chứa mã sản phẩm này
    const physicalStocks = stockList.value.filter(s => s.variantId === vid && s.qtyAvailable > 0);
    
    // 2. Bung nó ra thành từng dòng chi tiết ở bảng dưới
    physicalStocks.forEach(stock => {
        const exists = formData.value.items.find(i => i.variantId === stock.variantId && i.locationId === stock.locationId && i.nsx === stock.nsx && i.hsd === stock.hsd)
        if(!exists) {
            formData.value.items.push({ 
                variantId: stock.variantId, sku: stock.sku, name: stock.name, unit: stock.unit, 
                qty: 0, // Mặc định là 0 để sếp tự điền, tránh nhập lố
                maxQty: stock.qtyAvailable, reason: 'Móp méo / Rách bao bì', 
                locationId: stock.locationId, locationCode: stock.locationCode, nsx: stock.nsx, hsd: stock.hsd 
            })
        }
    })
  })
  selectedVariantIdsToAdd.value = []; stockSearchQuery.value = '' 
}

const checkMaxQty = (item) => {
    if(item.qty > item.maxQty) { alert(`Vị trí này chỉ còn ${item.maxQty} ${item.unit}!`); item.qty = item.maxQty; }
}

const removeItem = (index) => formData.value.items.splice(index, 1)

const handleSubmit = async () => {
  if (formData.value.items.length === 0) return alert('Chưa chọn mặt hàng hỏng hóc nào!')
  
  // Loại bỏ những dòng sếp để số lượng là 0 (Không hỏng)
  const validItems = formData.value.items.filter(i => i.qty > 0);
  if (validItems.length === 0) return alert('Vui lòng điền số lượng hỏng > 0 cho các vị trí bị lỗi!');

  try {
    const method = 'POST'; 
    const payload = { ...formData.value, code: "", items: validItems }; 
    const res = await fetch(DEFECT_API, { method, headers: getAuthHeaders(), body: JSON.stringify(payload) })
    if (res.ok) { alert('Ghi nhận hàng lỗi thành công!'); await fetchData(); closeModal(); } 
    else { let errMsg = "Lỗi hệ thống!"; try { const t = await res.text(); if(t) errMsg = JSON.parse(t).message || errMsg; } catch(e){} alert('LỖI: ' + errMsg); }
  } catch(e) { console.error(e) }
}

const handleCompleteDefect = async (receipt) => {
  if (!confirm(`XÁC NHẬN TIÊU HỦY / TRỪ KHO SẢN PHẨM LỖI NÀY? (Sẽ trừ vĩnh viễn khỏi DB)`)) return;
  try {
    const res = await fetch(`${DEFECT_API}/${receipt.id || receipt.Id}/complete`, { method: 'PUT', headers: getAuthHeaders() })
    if (res.ok) { alert('Đã tiêu hủy hàng lỗi thành công!'); await fetchData(); } 
    else { let errMsg = "Lỗi hệ thống!"; try { const t = await res.text(); if(t) errMsg = JSON.parse(t).message || errMsg; } catch(e){} alert('LỖI: ' + errMsg); }
  } catch(e) { console.error(e) }
}

onMounted(() => fetchData())
</script>

<template>
  <div class="space-y-5 md:space-y-6 animate-fade-in pb-10 px-0 md:px-1 relative">
    <div class="flex flex-col sm:flex-row sm:items-center justify-between gap-4">
      <div>
        <h2 class="text-xl md:text-2xl font-bold text-gray-800">Quản lý Hàng Lỗi / Phế Phẩm</h2>
        <p class="text-xs md:text-sm text-gray-500 mt-1">Ghi nhận hàng hóa bị hỏng hóc để loại trừ (Xuất Hủy) khỏi tồn kho khả dụng</p>
      </div>
      <div class="flex flex-wrap items-center gap-2">
        <button v-if="canExport" @click="() => alert('Xuất Excel...')" class="bg-white border border-emerald-200 text-emerald-700 px-3 py-2.5 rounded-lg text-sm font-semibold hover:bg-emerald-50 shadow-sm flex items-center gap-1.5"><DocumentTextIcon class="w-4 h-4"/> Xuất Excel</button>
        <button @click="openModal('add')" class="bg-red-600 hover:bg-red-700 text-white px-4 py-2.5 rounded-lg flex items-center gap-2 text-sm font-semibold shadow-sm transition-colors"><PlusIcon class="w-5 h-5" /> Báo cáo Hàng lỗi</button>
      </div>
    </div>

    <div class="bg-white p-3 md:p-4 rounded-xl border border-gray-200 flex flex-col sm:flex-row items-center gap-3 md:gap-4 shadow-sm">
      <div class="relative w-full sm:flex-1"><div class="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none"><MagnifyingGlassIcon class="w-5 h-5 text-gray-400" /></div><input v-model="searchQuery" type="text" class="block w-full pl-10 pr-3 py-2 border border-gray-200 rounded-lg text-sm outline-none focus:ring-1 focus:ring-red-500" placeholder="Tìm theo mã phiếu, mã SP lỗi..."></div>
      <select v-model="filterStatus" class="w-full sm:w-auto border border-gray-200 rounded-lg text-sm px-4 py-2 outline-none focus:ring-1 focus:ring-red-500 cursor-pointer"><option value="">Tất cả Trạng thái</option><option value="pending">Chờ xử lý</option><option value="completed">Đã hủy khỏi kho</option></select>
    </div>

    <div class="bg-white rounded-xl border border-gray-200 shadow-sm overflow-hidden">
      <div class="w-full overflow-x-auto custom-scrollbar">
        <table class="min-w-[1000px] w-full divide-y divide-gray-200">
          <thead class="bg-gray-50">
            <tr>
              <th class="px-5 py-3.5 text-left text-xs font-bold text-gray-500 uppercase tracking-wider">Mã Phiếu</th>
              <th class="px-5 py-3.5 text-left text-xs font-bold text-gray-500 uppercase tracking-wider">Sản phẩm lỗi (Đại diện)</th>
              <th class="px-5 py-3.5 text-center text-xs font-bold text-gray-500 uppercase tracking-wider">Số lượng Hỏng</th>
              <th class="px-5 py-3.5 text-left text-xs font-bold text-gray-500 uppercase tracking-wider">Nguyên nhân</th>
              <th class="px-5 py-3.5 text-center text-xs font-bold text-gray-500 uppercase tracking-wider">Trạng thái</th>
              <th class="px-5 py-3.5 text-right text-xs font-bold text-gray-500 uppercase tracking-wider">Thao tác</th>
            </tr>
          </thead>
          <tbody class="divide-y divide-gray-100 bg-white">
            <tr v-if="filteredDefects.length === 0"><td colspan="6" class="px-6 py-16 text-center"><ShieldExclamationIcon class="w-12 h-12 text-gray-300 mx-auto mb-3" /><h3 class="text-base font-semibold text-gray-700">Chưa có báo cáo hàng lỗi nào</h3></td></tr>
            <tr v-for="defect in filteredDefects" :key="defect.id" class="hover:bg-gray-50 transition-colors">
              <td class="px-5 py-3 text-sm font-bold text-red-600">{{ defect.code }}</td>
              <td class="px-5 py-3"><div class="flex flex-col"><span class="text-sm font-bold text-gray-900">{{ defect.name }}</span><span class="text-xs text-gray-500">Mã: {{ defect.sku }}</span></div></td>
              <td class="px-5 py-3 text-sm text-center font-bold text-red-600 text-lg">{{ defect.qty }}</td>
              <td class="px-5 py-3 text-sm text-gray-700 max-w-[250px] truncate" :title="defect.reason">{{ defect.reason }}</td>
              <td class="px-5 py-3 text-center"><span :class="['text-[10px] font-bold px-2 py-1 rounded border uppercase tracking-wider', getStatusBadge(defect.status).class]">{{ getStatusBadge(defect.status).text }}</span></td>
              <td class="px-5 py-3 text-right whitespace-nowrap">
                <div class="flex items-center justify-end gap-1.5">
                  <button @click="openModal('view', defect)" class="px-2 py-1.5 text-blue-600 hover:bg-blue-50 rounded bg-blue-50 border border-blue-100 font-medium text-xs flex items-center gap-1 transition-colors"><EyeIcon class="w-4 h-4" /> Chi tiết</button>
                  <button v-if="defect.status === 'pending' && canProcess" @click="handleCompleteDefect(defect)" class="px-2 py-1.5 text-red-600 hover:bg-red-700 hover:text-white rounded bg-red-50 border border-red-200 font-bold text-xs flex items-center gap-1 transition-colors" title="Xuất hủy để trừ kho">
                    <FireIcon class="w-4 h-4" /> Xuất Hủy
                  </button>
                </div>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>

    <Teleport to="body">
      <div v-if="showModal" class="fixed inset-0 z-50 flex items-center justify-center px-4 bg-slate-900/50 backdrop-blur-sm animate-fade-in">
        <div class="bg-white rounded-xl shadow-2xl w-full max-w-5xl overflow-hidden flex flex-col max-h-[90vh]">
          <div class="px-6 py-4 border-b border-red-100 flex items-center justify-between bg-red-50 shrink-0"><h3 class="text-lg font-bold text-red-800 flex items-center gap-2"><ShieldExclamationIcon class="w-6 h-6 text-red-600"/> {{ modalMode === 'add' ? 'Báo cáo Hàng Lỗi Mới' : `Chi tiết Lỗi: ${formData.code}` }}</h3><button @click="closeModal" class="text-gray-400 hover:text-red-500"><XMarkIcon class="w-6 h-6" /></button></div>
          <div class="p-6 overflow-y-auto flex-1 custom-scrollbar">
            <form id="defectForm" @submit.prevent="handleSubmit" class="space-y-6">
              
              <div class="grid grid-cols-1 sm:grid-cols-2 gap-4">
                <div><label class="block text-xs font-bold mb-1">Ngày ghi nhận *</label><input v-model="formData.date" :disabled="modalMode === 'view'" type="date" required class="w-full border rounded-lg px-3 py-2 text-sm focus:ring-red-500 disabled:bg-gray-100"></div>
                <div><label class="block text-xs font-bold mb-1">Trạng thái</label><div class="px-3 py-2 text-sm font-bold rounded border text-center" :class="getStatusBadge(formData.status).class">{{ getStatusBadge(formData.status).text }}</div></div>
              </div>

              <div v-if="modalMode === 'add'" class="bg-red-50 p-4 rounded-xl border border-red-100">
                <label class="text-sm font-bold text-red-800 mb-2 block">1. Tìm Hàng Lỗi trong Tồn Kho thực tế:</label>
                <input v-model="stockSearchQuery" type="text" class="w-full border border-red-200 rounded px-3 py-1.5 text-sm mb-2 outline-none focus:ring-1 focus:ring-red-500 shadow-sm" placeholder="🔍 Gõ mã SKU, tên SP để tìm hàng đang tồn...">
                <div class="bg-white border border-red-200 rounded max-h-32 overflow-y-auto p-2">
                  <label v-for="stock in filteredGroupedStockList" :key="stock.variantId" class="flex items-center gap-3 p-2 hover:bg-red-50 cursor-pointer border-b">
                    <input type="checkbox" :value="stock.variantId" v-model="selectedVariantIdsToAdd" class="w-4 h-4 text-red-600 rounded">
                    <span class="text-sm flex-1 font-medium">[{{ stock.sku }}] {{ stock.name }}</span>
                    <span class="text-xs font-bold px-2 py-1 bg-amber-100 text-amber-800 border border-amber-200 rounded ml-2">Tổng tồn kho: {{ stock.totalQty }} {{ stock.unit }}</span>
                  </label>
                </div>
                <button type="button" @click="handleAddMultipleStocks" class="mt-2 px-4 py-1.5 bg-red-600 text-white rounded text-sm font-bold disabled:opacity-50 hover:bg-red-700 shadow-sm" :disabled="selectedVariantIdsToAdd.length === 0">2. Bung chi tiết các Vị trí chứa hàng</button>
              </div>

              <div>
                <h4 class="text-sm font-bold mb-2">3. Khai báo Tình trạng Lỗi (Nhập số lượng hỏng > 0)</h4>
                <div class="border rounded-lg overflow-x-auto">
                  <table class="w-full text-sm text-left"><thead class="bg-gray-50 text-xs uppercase font-bold text-gray-500"><tr>
                    <th class="px-4 py-3">Mã SKU / Tên Hàng</th><th class="px-4 py-3 text-indigo-700">Vị Trí Kệ</th><th class="px-4 py-3 text-center">Tồn Lô</th><th class="px-4 py-3 text-center w-28 text-red-600">SL HỎNG</th><th class="px-4 py-3 w-64">Mô tả Lỗi</th><th v-if="modalMode === 'add'" class="px-4 py-3 text-center w-10">#</th>
                  </tr></thead>
                  <tbody class="divide-y divide-gray-100">
                    <tr v-if="formData.items.length === 0"><td :colspan="modalMode === 'add' ? 6 : 5" class="px-4 py-8 text-center text-gray-400 italic">Chưa có mặt hàng lỗi.</td></tr>
                    <tr v-for="(item, idx) in formData.items" :key="idx" class="hover:bg-red-50/50 transition-colors">
                      <td class="px-4 py-2 font-bold">{{ item.sku }} <br><span class="font-normal text-xs text-gray-500">{{ item.name }}</span></td>
                      <td class="px-4 py-2 text-indigo-700 font-bold text-xs">{{ item.locationCode }}<br><span class="font-normal text-gray-500">NSX: {{item.nsx||'--'}}</span></td>
                      <td class="px-4 py-2 text-center text-gray-400 font-bold">{{ item.maxQty }}</td>
                      <td class="px-4 py-2 text-center"><input v-if="modalMode === 'add'" v-model.number="item.qty" @input="checkMaxQty(item)" type="number" min="0" :max="item.maxQty" class="w-full text-center border border-red-300 bg-red-50 rounded px-2 py-1 font-bold text-red-700 focus:outline-none focus:ring-1 focus:ring-red-500"><span v-else class="font-bold text-red-700 text-lg">{{ item.qty }}</span></td>
                      <td class="px-4 py-2"><input v-if="modalMode === 'add'" v-model="item.reason" type="text" class="w-full border rounded px-2 py-1 text-xs" placeholder="VD: Rách bao bì, móp méo..."><span v-else class="text-xs">{{ item.reason }}</span></td>
                      <td v-if="modalMode === 'add'" class="px-4 py-2 text-center"><button type="button" @click="removeItem(idx)" class="text-red-400 hover:text-red-600"><TrashIcon class="w-5 h-5 mx-auto"/></button></td>
                    </tr>
                  </tbody>
                  </table>
                </div>
              </div>
              <div><label class="block text-xs font-bold mb-1">Ghi chú chung</label><textarea v-model="formData.note" :disabled="modalMode === 'view'" rows="2" class="w-full border rounded-lg px-3 py-2 text-sm focus:ring-red-500 disabled:bg-gray-100" placeholder="..."></textarea></div>
            </form>
          </div>

          <div class="px-6 py-4 border-t flex justify-end gap-3 shrink-0 bg-white">
            <button type="button" @click="closeModal" class="px-5 py-2 border rounded-lg text-sm font-semibold hover:bg-gray-50">{{ modalMode === 'view' ? 'Đóng lại' : 'Hủy bỏ' }}</button>
            <button v-if="modalMode === 'add'" type="submit" form="defectForm" class="px-5 py-2 bg-red-600 text-white rounded-lg text-sm font-bold hover:bg-red-700 shadow-md"><ShieldExclamationIcon class="w-5 h-5 inline mr-1"/> Gửi Báo Cáo Lỗi</button>
          </div>

        </div>
      </div>
    </Teleport>
  </div>
</template>

<style scoped>
.custom-scrollbar::-webkit-scrollbar { height: 6px; width: 6px; }
.custom-scrollbar::-webkit-scrollbar-track { background: transparent; }
.custom-scrollbar::-webkit-scrollbar-thumb { background: #cbd5e1; border-radius: 10px; }
.custom-scrollbar:hover::-webkit-scrollbar-thumb { background: #94a3b8; }
.animate-fade-in { animation: fadeIn 0.2s ease-out; }
@keyframes fadeIn { from { opacity: 0; transform: translateY(5px); } to { opacity: 1; transform: translateY(0); } }
</style>