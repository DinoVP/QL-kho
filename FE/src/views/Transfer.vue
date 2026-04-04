<script setup>
import { ref, computed, onMounted } from 'vue'
import { 
  MagnifyingGlassIcon, PlusIcon, EyeIcon, 
  XMarkIcon, ArrowsRightLeftIcon, TrashIcon,
  PrinterIcon, DocumentTextIcon, CheckCircleIcon, 
  TruckIcon, LockClosedIcon
} from '@heroicons/vue/24/outline'

const TRANSFER_API = 'https://localhost:7139/api/Transfer'
const STOCK_API = 'https://localhost:7139/api/Stock' 
const PROD_API = 'https://localhost:7139/api/Products'
const LOC_API = 'https://localhost:7139/api/Locations'
const BRANCH_API = 'https://localhost:7139/api/Branches' 

const getAuthHeaders = () => ({ 'Content-Type': 'application/json', 'Authorization': 'Bearer ' + (localStorage.getItem('authToken') || '') })

// === XỬ LÝ PHÂN QUYỀN CHỐNG LỖI ===
const getSafeRole = () => {
    const r = localStorage.getItem('role') || localStorage.getItem('Role') || 'gd_chi_nhanh';
    return r.toLowerCase();
}
const getSafeBranchId = () => {
    const bId = localStorage.getItem('branchId') || localStorage.getItem('BranchId');
    return bId ? parseInt(bId) : null; 
}

const currentUserRole = ref(getSafeRole()) 
const currentUserBranchId = ref(getSafeBranchId()) 

const canCreateTransfer = computed(() => ['admin', 'gd_chi_nhanh'].includes(currentUserRole.value))
const canExport = computed(() => ['admin', 'gd_chi_nhanh'].includes(currentUserRole.value))
const canReceiveTransfer = computed(() => ['admin', 'gd_chi_nhanh', 'ql_kho'].includes(currentUserRole.value))

const transferReceipts = ref([])
const warehousesList = ref([]) 
const productsList = ref([]); const locationsList = ref([]); const stockList = ref([])
const isLoading = ref(false)

const getToday = () => new Date().toISOString().split('T')[0]

// === LOGIC LỌC KHO THÔNG MINH (CHỐNG TRẮNG FORM) ===
const availableFromWarehouses = computed(() => {
    if (['admin', 'giam_doc'].includes(currentUserRole.value)) return warehousesList.value;
    
    // Nếu là GĐ Chi nhánh, lọc theo BranchId
    if (currentUserBranchId.value) {
        const filtered = warehousesList.value.filter(wh => wh.branchId === currentUserBranchId.value);
        // Nếu lọc ra rỗng (có thể do sếp gán nhầm chi nhánh), thì cho hiện hết để sếp vẫn test được
        return filtered.length > 0 ? filtered : warehousesList.value;
    }
    
    // Nếu LocalStorage mất BranchId, hiện hết để không bị kẹt
    return warehousesList.value;
})

const fetchData = async () => {
  isLoading.value = true
  try {
    const headers = getAuthHeaders()
    
    // XỬ LÝ LẤY KHO TỪ API CHI NHÁNH (BẮT ĐÚNG CHỮ HOA/THƯỜNG)
    try {
        const branchRes = await fetch(BRANCH_API, { headers })
        if (branchRes.ok) {
            const branches = await branchRes.json()
            let allWh = []
            for (const b of branches) {
                const branchId = b.id || b.Id;
                if (!branchId) continue;

                const whRes = await fetch(`${BRANCH_API}/${branchId}/warehouses-detail`, { headers })
                if(whRes.ok) {
                    const whData = await whRes.json()
                    const mappedWh = whData.map(w => ({
                        id: w.warehouseId || w.WarehouseId || w.id || w.Id,
                        name: w.whname || w.Whname || w.name || w.Name || 'Kho chưa có tên',
                        branchId: branchId
                    }))
                    allWh = [...allWh, ...mappedWh]
                }
            }
            warehousesList.value = allWh
        }
    } catch(e) { console.error("Lỗi lấy danh sách Kho", e); }

    const [transRes, stockRes, prodRes, locRes] = await Promise.all([
      fetch(TRANSFER_API, { headers }), fetch(STOCK_API, { headers }), 
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
          sku: prod.sku || prod.Sku || 'N/A', name: prod.name || prod.Name || 'Lỗi', unit: prod.unit || prod.Unit || 'Cái',
          conversionRate: prod.conversionRate || prod.ConversionRate || 24, 
          locationId: s.locationId, locationCode: loc.code || loc.Code || 'Khu chung',
          warehouseId: loc.warehouseId || loc.WarehouseId || 1 
        }
      })
    }

    if (transRes.ok) {
      const rawTrans = await transRes.json()
      transferReceipts.value = rawTrans.map(r => {
        if (!r.code && r.id) r.code = `DC${r.id.toString().padStart(4, '0')}`;
        const fromWh = warehousesList.value.find(w => w.id === r.fromWarehouseId || w.Id === r.fromWarehouseId);
        const toWh = warehousesList.value.find(w => w.id === r.toWarehouseId || w.Id === r.toWarehouseId);
        r.fromWarehouseName = fromWh ? fromWh.name : `Kho (ID: ${r.fromWarehouseId || '?'})`;
        r.toWarehouseName = toWh ? toWh.name : `Kho (ID: ${r.toWarehouseId || '?'})`;
        r.totalQty = r.items ? r.items.reduce((s, i) => s + (i.qty || 0), 0) : 0;
        return r;
      })
    }
  } catch (error) { console.error(error) } finally { isLoading.value = false }
}

const searchQuery = ref(''); const filterStatus = ref('')
const filteredReceipts = computed(() => {
  return transferReceipts.value.filter(r => {
    const code = r.code || r.Code || ''; 
    const matchSearch = code.toLowerCase().includes(searchQuery.value.toLowerCase())
    return matchSearch && (filterStatus.value === '' || r.status === filterStatus.value)
  })
})

const showModal = ref(false); const modalMode = ref('add') 
const formData = ref({ id: 0, code: '', date: getToday(), fromWarehouseId: '', toWarehouseId: '', items: [], note: '', status: 'pending' })

const openModal = (mode, receipt = null) => {
  modalMode.value = mode
  if (receipt) {
    const safeItems = (receipt.items || receipt.Items || []).map(i => {
      const variantId = i.variantId || i.VariantId;
      const prod = productsList.value.find(p => p.id === variantId || p.Id === variantId) || {};
      const fromLoc = locationsList.value.find(l => l.id === i.fromLocationId) || {};
      const stock = stockList.value.find(s => s.variantId === variantId && s.locationId === i.fromLocationId && s.nsx === (i.nsx||i.Nsx)?.split('T')[0] && s.hsd === (i.hsd||i.Hsd)?.split('T')[0])
      const available = stock ? stock.qtyAvailable : 0;
      const convRate = prod.conversionRate || prod.ConversionRate || 24;

      return {
        variantId: variantId, sku: prod.sku || prod.Sku, name: prod.name || prod.Name, unit: prod.unit || prod.Unit, 
        conversionRate: convRate, boxQty: Math.floor((i.qty || i.Qty) / convRate), qty: i.qty || i.Qty, 
        maxQty: available + (i.qty || i.Qty), 
        fromLocationId: i.fromLocationId, fromLocationCode: fromLoc.code || fromLoc.Code || 'Khu chung',
        toLocationId: null, 
        nsx: i.nsx || i.Nsx ? (i.nsx || i.Nsx).split('T')[0] : '', hsd: i.hsd || i.Hsd ? (i.hsd || i.Hsd).split('T')[0] : ''
      };
    });
    formData.value = { 
        id: receipt.id || receipt.Id, code: receipt.code || receipt.Code, date: receipt.date || receipt.Date, 
        fromWarehouseId: receipt.fromWarehouseId || receipt.FromWarehouseId || '', 
        toWarehouseId: receipt.toWarehouseId || receipt.ToWarehouseId || '',
        items: safeItems, note: receipt.note || receipt.Note, status: receipt.status || receipt.Status 
    };
  } else { formData.value = { id: 0, code: '', date: getToday(), fromWarehouseId: '', toWarehouseId: '', items: [], note: '', status: 'pending' } }
  selectedStockIdsToAdd.value = []; stockSearchQuery.value = ''; showModal.value = true
}
const closeModal = () => showModal.value = false

const getStatusBadge = (status) => {
  switch(status) {
    case 'pending': return { text: 'Chờ điều chuyển', class: 'bg-amber-100 text-amber-700 border-amber-200' }
    case 'completed': return { text: 'Đã nhận đủ', class: 'bg-emerald-100 text-emerald-700 border-emerald-200' }
    default: return { text: 'Khác', class: 'bg-gray-100 text-gray-500' }
  }
}

const stockSearchQuery = ref(''); const selectedStockIdsToAdd = ref([]) 
const filteredStockList = computed(() => {
  let list = stockList.value.filter(s => s.warehouseId === formData.value.fromWarehouseId);
  if (stockSearchQuery.value) {
      list = list.filter(s => s.sku.toLowerCase().includes(stockSearchQuery.value.toLowerCase()) || s.name.toLowerCase().includes(stockSearchQuery.value.toLowerCase()));
  }
  return list;
})

const handleFromWarehouseChange = () => {
    formData.value.items = [];
    selectedStockIdsToAdd.value = [];
    if(formData.value.fromWarehouseId === formData.value.toWarehouseId) formData.value.toWarehouseId = '';
}

const handleAddMultipleStocks = () => {
  if (selectedStockIdsToAdd.value.length === 0) return;
  selectedStockIdsToAdd.value.forEach(stockId => {
    const stock = stockList.value.find(s => s.stockId === stockId)
    if (stock) {
      const exists = formData.value.items.find(i => i.variantId === stock.variantId && i.fromLocationId === stock.locationId && i.nsx === stock.nsx && i.hsd === stock.hsd)
      if(!exists) {
        formData.value.items.push({ 
          variantId: stock.variantId, sku: stock.sku, name: stock.name, unit: stock.unit, 
          conversionRate: stock.conversionRate, boxQty: 1, qty: 1 * stock.conversionRate, 
          maxQty: stock.qtyAvailable, fromLocationId: stock.locationId, fromLocationCode: stock.locationCode, 
          toLocationId: null, nsx: stock.nsx, hsd: stock.hsd 
        })
      }
    }
  })
  selectedStockIdsToAdd.value = []; stockSearchQuery.value = '' 
}

const calculateQty = (item) => {
    item.qty = (item.boxQty || 0) * (item.conversionRate || 1); 
    if(item.qty > item.maxQty) { 
        alert(`[LỖI] Kệ này chỉ còn ${item.maxQty} ${item.unit}! Hệ thống tự động trả về mức xuất tối đa.`); 
        item.boxQty = Math.floor(item.maxQty / (item.conversionRate || 1));
        item.qty = item.boxQty * (item.conversionRate || 1);
    }
}

const removeItem = (index) => formData.value.items.splice(index, 1)
const totalQty = computed(() => formData.value.items.reduce((sum, item) => sum + (item.qty || 0), 0))

const handleSubmit = async () => {
  if (!formData.value.fromWarehouseId || !formData.value.toWarehouseId) return alert('Sếp chưa chọn Kho Xuất hoặc Kho Nhập!');
  if (formData.value.fromWarehouseId === formData.value.toWarehouseId) return alert('Lỗi: Kho nhập và Kho xuất không được trùng nhau!');
  if (formData.value.items.length === 0) return alert('Chưa chọn mặt hàng nào cần chuyển!')
  
  try {
    const res = await fetch(TRANSFER_API, { method: 'POST', headers: getAuthHeaders(), body: JSON.stringify({ ...formData.value, code: "" }) })
    if (res.ok) { alert('Lập lệnh Điều chuyển thành công! Hàng đang trên đường.'); await fetchData(); closeModal(); } 
    else { let errMsg = "Lỗi hệ thống!"; try { const t = await res.text(); if(t) errMsg = JSON.parse(t).message || errMsg; } catch(e){} alert('LỖI: ' + errMsg); }
  } catch(e) { console.error(e) }
}

const handleCompleteTransfer = async (receipt) => {
  if (!confirm(`XÁC NHẬN KHO ĐÍCH ĐÃ NHẬN ĐỦ HÀNG?\nHàng sẽ được gán vào "Khu Chờ Nhập" của Kho mới. QL Kho cần dùng tính năng "Chuyển Kệ" để gán lên kệ cụ thể sau.`)) return;
  try {
    const res = await fetch(`${TRANSFER_API}/${receipt.id || receipt.Id}/complete`, { method: 'PUT', headers: getAuthHeaders() })
    if (res.ok) { alert('Hoàn tất! Hàng đã nhập vào kho.'); await fetchData(); } 
    else { let errMsg = "Lỗi hệ thống!"; try { const t = await res.text(); if(t) errMsg = JSON.parse(t).message || errMsg; } catch(e){} alert('LỖI: ' + errMsg); }
  } catch(e) { console.error(e) }
}

onMounted(() => fetchData())
</script>

<template>
  <div class="space-y-5 md:space-y-6 animate-fade-in pb-10 px-0 md:px-1 relative">
    
    <div class="flex flex-col sm:flex-row sm:items-center justify-between gap-4">
      <div>
        <h2 class="text-xl md:text-2xl font-bold text-gray-800">Điều chuyển Nội bộ</h2>
        <p class="text-xs md:text-sm text-gray-500 mt-1">Lập lệnh và ghi nhận hàng hóa luân chuyển giữa các Kho</p>
      </div>
      <div class="flex flex-wrap items-center gap-2">
        <button v-if="canExport" @click="() => alert('Xuất Excel...')" class="bg-white border border-emerald-200 text-emerald-700 px-3 py-2.5 rounded-lg text-sm font-semibold hover:bg-emerald-50 shadow-sm flex items-center gap-1.5"><DocumentTextIcon class="w-4 h-4"/> Xuất Excel</button>
        <button v-if="canCreateTransfer" @click="openModal('add')" class="bg-primary-600 hover:bg-primary-700 text-white px-4 py-2.5 rounded-lg flex items-center gap-2 text-sm font-semibold shadow-sm transition-colors"><PlusIcon class="w-5 h-5" /> Lập Lệnh Điều Chuyển</button>
      </div>
    </div>

    <div class="bg-white p-3 md:p-4 rounded-xl border border-gray-200 flex flex-col sm:flex-row items-center gap-3 md:gap-4 shadow-sm">
      <div class="relative w-full sm:flex-1"><div class="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none"><MagnifyingGlassIcon class="w-5 h-5 text-gray-400" /></div><input v-model="searchQuery" type="text" class="block w-full pl-10 pr-3 py-2 border border-gray-200 rounded-lg text-sm outline-none focus:ring-1 focus:ring-primary-500" placeholder="Tìm theo mã phiếu..."></div>
      <select v-model="filterStatus" class="w-full sm:w-auto border border-gray-200 rounded-lg text-sm px-4 py-2 outline-none focus:ring-1 focus:ring-primary-500 cursor-pointer"><option value="">Tất cả Trạng thái</option><option value="pending">Chờ điều chuyển</option><option value="completed">Đã nhận đủ</option></select>
    </div>

    <div class="bg-white rounded-xl border border-gray-200 shadow-sm overflow-hidden">
      <div class="w-full overflow-x-auto custom-scrollbar">
        <table class="min-w-[1050px] w-full divide-y divide-gray-200">
          <thead class="bg-gray-50">
            <tr>
              <th class="px-5 py-3.5 text-left text-xs font-bold text-gray-500 uppercase tracking-wider">Mã Phiếu</th>
              <th class="px-5 py-3.5 text-left text-xs font-bold text-gray-500 uppercase tracking-wider">Ngày lập</th>
              <th class="px-5 py-3.5 text-left text-xs font-bold text-gray-500 uppercase tracking-wider text-orange-700">TỪ KHO (Xuất)</th>
              <th class="px-5 py-3.5 text-left text-xs font-bold text-gray-500 uppercase tracking-wider text-emerald-700">ĐẾN KHO (Nhập)</th>
              <th class="px-5 py-3.5 text-center text-xs font-bold text-gray-500 uppercase tracking-wider">Tổng SL</th>
              <th class="px-5 py-3.5 text-center text-xs font-bold text-gray-500 uppercase tracking-wider">Trạng thái</th>
              <th class="px-5 py-3.5 text-right text-xs font-bold text-gray-500 uppercase tracking-wider">Thao tác</th>
            </tr>
          </thead>
          <tbody class="divide-y divide-gray-100 bg-white">
            <tr v-if="filteredReceipts.length === 0"><td colspan="7" class="px-6 py-16 text-center"><ArrowsRightLeftIcon class="w-12 h-12 text-gray-300 mx-auto mb-3" /><h3 class="text-base font-semibold text-gray-700">Chưa có Phiếu Điều chuyển nào</h3></td></tr>
            <tr v-for="receipt in filteredReceipts" :key="receipt.id" class="hover:bg-gray-50 transition-colors">
              <td class="px-5 py-3 text-sm font-bold text-primary-700">{{ receipt.code }}</td><td class="px-5 py-3 text-sm font-medium text-gray-600">{{ receipt.date }}</td>
              <td class="px-5 py-3 text-sm font-bold text-orange-700">{{ receipt.fromWarehouseName }}</td><td class="px-5 py-3 text-sm font-bold text-emerald-700">{{ receipt.toWarehouseName }}</td>
              <td class="px-5 py-3 text-sm text-center font-bold text-blue-700 text-lg">{{ receipt.totalQty }}</td>
              <td class="px-5 py-3 text-center"><span :class="['text-[10px] font-bold px-2 py-1 rounded border uppercase tracking-wider', getStatusBadge(receipt.status).class]">{{ getStatusBadge(receipt.status).text }}</span></td>
              <td class="px-5 py-3 text-right whitespace-nowrap">
                <div class="flex items-center justify-end gap-1.5">
                  <button @click="openModal('view', receipt)" class="p-1.5 text-blue-600 hover:bg-blue-50 rounded transition-colors" title="Xem chi tiết"><EyeIcon class="w-5 h-5" /></button>
                  <button v-if="receipt.status === 'pending' && canReceiveTransfer" @click="handleCompleteTransfer(receipt)" class="px-2 py-1 text-emerald-700 bg-emerald-50 hover:bg-emerald-100 border border-emerald-200 rounded font-bold text-xs transition-colors flex items-center gap-1" title="Xác nhận Kho đích đã nhận đủ">
                    <TruckIcon class="w-4 h-4" /> Nhận Hàng
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
        <div class="bg-white rounded-xl shadow-2xl w-full max-w-[95vw] lg:max-w-6xl overflow-hidden flex flex-col max-h-[90vh]">
          
          <div class="px-6 py-4 border-b border-gray-100 flex items-center justify-between bg-gray-50 shrink-0"><h3 class="text-lg font-bold flex items-center gap-2"><ArrowsRightLeftIcon class="w-6 h-6 text-primary-600"/> {{ modalMode === 'add' ? 'Lập Lệnh Điều Chuyển' : `Chi tiết: ${formData.code}` }}</h3><button @click="closeModal" class="text-gray-400 hover:text-red-500"><XMarkIcon class="w-6 h-6" /></button></div>
          
          <div class="p-6 overflow-y-auto flex-1 custom-scrollbar">
            <form id="transferForm" @submit.prevent="handleSubmit" class="space-y-6">
              
              <div class="bg-slate-50 p-4 rounded-lg border border-slate-200">
                <div class="grid grid-cols-1 sm:grid-cols-2 gap-4 mb-4 pb-4 border-b border-gray-200">
                  <div><label class="block text-xs font-bold mb-1">Ngày luân chuyển *</label><input v-model="formData.date" :disabled="modalMode === 'view'" type="date" required class="w-full border rounded-lg px-3 py-2 text-sm focus:ring-primary-500 disabled:bg-gray-100"></div>
                  <div><label class="block text-xs font-bold mb-1">Trạng thái</label><div class="px-3 py-2 text-sm font-bold rounded border text-center" :class="getStatusBadge(formData.status).class">{{ getStatusBadge(formData.status).text }}</div></div>
                </div>

                <div class="grid grid-cols-1 sm:grid-cols-2 gap-6 mt-4">
                  <div class="bg-orange-50 p-3 rounded border border-orange-100">
                    <label class="block text-xs font-bold text-orange-800 mb-1">BƯỚC 1: TỪ KHO (Kho xuất) *</label>
                    <select v-model="formData.fromWarehouseId" @change="handleFromWarehouseChange" :disabled="modalMode === 'view'" required class="w-full border border-orange-300 rounded-lg px-3 py-2 text-sm font-bold text-orange-700 focus:ring-orange-500 disabled:bg-gray-100 cursor-pointer bg-white shadow-sm">
                      <option value="" disabled>-- Chọn Kho Xuất để tìm hàng --</option>
                      <option v-for="wh in availableFromWarehouses" :key="wh.id" :value="wh.id">{{ wh.name }}</option>
                    </select>
                  </div>
                  <div class="bg-emerald-50 p-3 rounded border border-emerald-100 relative">
                    <div v-if="!formData.fromWarehouseId && modalMode === 'add'" class="absolute inset-0 bg-white/60 backdrop-blur-[1px] flex items-center justify-center z-10 rounded"></div>
                    <label class="block text-xs font-bold text-emerald-800 mb-1">BƯỚC 3: ĐẾN KHO (Kho nhập) *</label>
                    <select v-model="formData.toWarehouseId" :disabled="modalMode === 'view'" required class="w-full border border-emerald-300 rounded-lg px-3 py-2 text-sm font-bold text-emerald-700 focus:ring-emerald-500 disabled:bg-gray-100 cursor-pointer bg-white shadow-sm">
                      <option value="" disabled>-- Chọn Kho Nhập --</option>
                      <option v-for="wh in warehousesList" :key="wh.id" :value="wh.id" :disabled="wh.id === formData.fromWarehouseId" :class="wh.id === formData.fromWarehouseId ? 'text-gray-400' : ''">{{ wh.name }} {{ wh.id === formData.fromWarehouseId ? '(Đang là kho xuất)' : '' }}</option>
                    </select>
                  </div>
                </div>
              </div>

              <div v-if="modalMode === 'add'" class="bg-blue-50 p-4 rounded-xl border border-blue-100 relative">
                 <div v-if="!formData.fromWarehouseId" class="absolute inset-0 bg-white/80 backdrop-blur-[2px] flex flex-col items-center justify-center z-10 rounded-xl border border-gray-200">
                    <LockClosedIcon class="w-8 h-8 text-gray-400 mb-2" />
                    <p class="text-sm font-bold text-gray-600">Vui lòng chọn TỪ KHO (Bước 1) để lấy danh sách hàng tồn.</p>
                 </div>

                <label class="text-sm font-bold text-blue-800 mb-2 block">BƯỚC 2: Chọn hàng TỪ TỒN KHO để chuyển đi:</label>
                <input v-model="stockSearchQuery" type="text" class="w-full border border-blue-200 rounded px-3 py-1.5 text-sm mb-2 outline-none focus:ring-1 focus:ring-blue-500 shadow-sm bg-white" placeholder="🔍 Gõ mã SKU, tên SP để tìm hàng đang tồn...">
                <div class="bg-white border border-blue-200 rounded max-h-32 overflow-y-auto p-2">
                  <div v-if="filteredStockList.length === 0 && formData.fromWarehouseId" class="p-2 text-sm text-gray-400 italic">Kho này đang trống hoặc không tìm thấy hàng phù hợp.</div>
                  <label v-for="stock in filteredStockList" :key="stock.stockId" class="flex items-center gap-3 p-2 hover:bg-blue-50 cursor-pointer border-b">
                    <input type="checkbox" :value="stock.stockId" v-model="selectedStockIdsToAdd" class="w-4 h-4 text-blue-600 rounded">
                    <span class="text-sm flex-1 font-medium">[{{ stock.sku }}] {{ stock.name }}</span>
                    <span class="text-xs font-bold text-orange-600 bg-orange-50 px-2 py-0.5 rounded ml-2">Đang ở Kệ: {{ stock.locationCode }}</span>
                    <span class="text-xs font-bold px-2 py-1 bg-emerald-100 text-emerald-700 rounded ml-2">Tồn: {{ stock.qtyAvailable }} {{ stock.unit }}</span>
                  </label>
                </div>
                <button type="button" @click="handleAddMultipleStocks" class="mt-2 px-4 py-1.5 bg-blue-600 text-white rounded text-sm font-bold disabled:opacity-50" :disabled="selectedStockIdsToAdd.length === 0">Xác nhận đưa {{selectedStockIdsToAdd.length}} Lô vào phiếu</button>
              </div>

              <div>
                <h4 class="text-sm font-bold mb-2">Danh sách Hàng hóa Điều chuyển</h4>
                <div class="border rounded-lg overflow-x-auto shadow-sm">
                  <table class="w-full text-sm text-left"><thead class="bg-gray-50 text-xs uppercase font-bold text-gray-500 border-b"><tr>
                    <th class="px-3 py-3 min-w-[150px]">SKU / Tên Hàng</th>
                    <th class="px-3 py-3 text-center">ĐVT</th>
                    <th class="px-3 py-3 text-orange-700">TỪ KỆ (Xuất đi)</th>
                    <th class="px-3 py-3 text-center w-32">NSX - HSD</th>
                    <th class="px-3 py-3 text-right">Tồn Lô</th>
                    <th class="px-3 py-3 text-center bg-blue-50/50">Số Thùng (Nhập)</th>
                    <th class="px-3 py-3 text-center bg-blue-50/50">Quy đổi</th>
                    <th class="px-3 py-3 text-right text-blue-700">Tổng SL Chuyển</th>
                    <th v-if="modalMode === 'add'" class="px-3 py-3 text-center w-10">#</th>
                  </tr></thead>
                  <tbody class="divide-y divide-gray-100">
                    <tr v-if="formData.items.length === 0"><td :colspan="modalMode === 'add' ? 9 : 8" class="px-4 py-8 text-center text-gray-400 italic">Chưa có mặt hàng nào. Vui lòng hoàn thành Bước 1 và 2.</td></tr>
                    <tr v-for="(item, idx) in formData.items" :key="idx" class="hover:bg-gray-50 transition-colors">
                      <td class="px-3 py-2 font-bold">{{ item.sku }} <br><span class="font-normal text-xs text-gray-500">{{ item.name }}</span></td>
                      <td class="px-3 py-2 text-center text-xs font-medium">{{ item.unit }}</td>
                      <td class="px-3 py-2 text-orange-700 font-bold text-xs">{{ item.fromLocationCode }}</td>
                      
                      <td class="px-3 py-2 text-center text-xs text-gray-500 whitespace-nowrap">
                        NSX: {{ item.nsx || '--' }}<br>HSD: <span class="text-amber-600 font-medium">{{ item.hsd || '--' }}</span>
                      </td>
                      <td class="px-3 py-2 text-right text-gray-400 font-bold">{{ item.maxQty }}</td>
                      
                      <td class="px-3 py-2 text-center bg-blue-50/20">
                        <input v-if="modalMode === 'add'" v-model.number="item.boxQty" @input="calculateQty(item)" type="number" min="1" class="w-16 text-center border border-blue-300 rounded px-1 py-1 text-sm font-bold text-blue-700 outline-none focus:ring-1 focus:ring-blue-500">
                        <span v-else class="text-sm font-bold text-blue-700">{{item.boxQty || item.qty}}</span>
                      </td>
                      <td class="px-3 py-2 text-center bg-blue-50/20 text-gray-600 font-medium">x {{ item.conversionRate }}</td>
                      <td class="px-3 py-2 text-right"><span class="font-bold text-blue-700 text-lg">{{ item.qty }}</span></td>
                      <td v-if="modalMode === 'add'" class="px-3 py-2 text-center"><button type="button" @click="removeItem(idx)" class="text-red-400 hover:text-red-600"><TrashIcon class="w-5 h-5 mx-auto"/></button></td>
                    </tr>
                  </tbody>
                  <tfoot class="bg-gray-50 font-bold border-t"><td colspan="7" class="px-4 py-3 text-right uppercase text-gray-600">Tổng SL Chuyển:</td><td class="px-4 py-3 text-right text-blue-700 text-xl">{{ totalQty }}</td><td v-if="modalMode === 'add'"></td></tfoot>
                  </table>
                </div>
              </div>
              <div><label class="block text-xs font-bold mb-1">Ghi chú / Lý do điều chuyển</label><textarea v-model="formData.note" :disabled="modalMode === 'view'" rows="2" class="w-full border rounded-lg px-3 py-2 text-sm focus:ring-primary-500 disabled:bg-gray-100" placeholder="..."></textarea></div>
            </form>
          </div>

          <div class="px-6 py-4 border-t flex flex-col sm:flex-row justify-between gap-3 shrink-0 bg-white">
            <div class="flex gap-2 w-full sm:w-auto">
              <button v-if="modalMode === 'view'" @click="() => alert('In Phiếu...')" class="flex-1 sm:flex-none px-4 py-2 bg-slate-100 hover:bg-slate-200 text-slate-700 rounded-lg text-sm font-bold border flex items-center gap-2"><PrinterIcon class="w-5 h-5"/> In Phiếu Điều Chuyển</button>
            </div>
            <div class="flex gap-2 w-full sm:w-auto justify-end">
              <button type="button" @click="closeModal" class="px-5 py-2 border rounded-lg text-sm font-semibold hover:bg-gray-50">{{ modalMode === 'view' ? 'Đóng lại' : 'Hủy bỏ' }}</button>
              <button v-if="modalMode === 'add'" type="submit" form="transferForm" class="px-5 py-2 bg-primary-600 text-white rounded-lg text-sm font-bold hover:bg-primary-700 shadow-md"><ArrowsRightLeftIcon class="w-5 h-5 inline mr-1"/> Xác nhận Lập Lệnh</button>
            </div>
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
.animate-fade-in { animation: fadeIn 0.2s ease-out; }
@keyframes fadeIn { from { opacity: 0; transform: translateY(5px); } to { opacity: 1; transform: translateY(0); } }
</style>