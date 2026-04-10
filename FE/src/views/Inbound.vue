<script setup>
import { ref, computed, onMounted } from 'vue'
import { 
  MagnifyingGlassIcon, PlusIcon, EyeIcon, 
  XMarkIcon, DocumentTextIcon, TrashIcon,
  CheckCircleIcon, ArrowDownTrayIcon, ArchiveBoxArrowDownIcon, PrinterIcon
} from '@heroicons/vue/24/outline'

const INBOUND_API = 'https://localhost:7139/api/Inbound'
const PROD_API = 'https://localhost:7139/api/Products'
const PARTNER_API = 'https://localhost:7139/api/CrmPartners' 

const getAuthHeaders = () => ({ 'Content-Type': 'application/json', 'Authorization': 'Bearer ' + (localStorage.getItem('authToken') || '') })

const currentUserRole = ref(localStorage.getItem('role') || 'ql_kho')
const myWarehouseId = ref(parseInt(localStorage.getItem('warehouseId')) || null)

// =========================================================================
// ĐÃ FIX PHÂN QUYỀN CHUẨN: 
// 1. canApprove: Chỉ Giám đốc / Admin mới được duyệt phiếu
// 2. canConfirmPutaway: QL Kho được bấm nhận hàng vào bãi
// =========================================================================
const currentRole = currentUserRole.value.toLowerCase()
const canCreate = computed(() => ['admin', 'nv_thu_mua'].includes(currentRole))
const canViewPrice = computed(() => ['admin', 'giam_doc', 'gd_chi_nhanh', 'nv_thu_mua'].includes(currentRole))
const canApprove = computed(() => ['admin', 'giam_doc', 'gd_chi_nhanh'].includes(currentRole)) 
const canConfirmPutaway = computed(() => ['admin', 'ql_kho'].includes(currentRole))

const receipts = ref([])
const productsList = ref([])
const partnersList = ref([])
const isLoading = ref(false)

const getToday = () => new Date().toISOString().split('T')[0]
const formatCurrency = (val) => new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(val || 0)

const getUnitChain = (prod) => {
    let units = [{ name: prod.unit || prod.Unit || 'SL', rate: 1, rel: 1 }];
    const conversions = prod.conversions || prod.Conversions || [];
    if (conversions.length > 0) {
        const sortedConvs = [...conversions].sort((a,b) => a.rate - b.rate);
        let prevRate = 1;
        sortedConvs.forEach(c => { units.push({ name: c.altUnit, rate: c.rate, rel: c.rate / prevRate }); prevRate = c.rate; });
    } else if ((prod.conversionRate || prod.ConversionRate) > 1) {
        const cr = prod.conversionRate || prod.ConversionRate;
        units.push({ name: 'Thùng/Kiện', rate: cr, rel: cr });
    }
    return units.sort((a,b) => a.rate - b.rate);
}

const buildChainEditor = (units) => {
    let chain = [];
    for(let i = 1; i < units.length; i++) { chain.push({ name: units[i].name, prevName: units[i-1].name, rel: units[i].rate / units[i-1].rate }); }
    return chain;
}

const autoFormatStockText = (qty, units, baseUnit) => {
    if (qty <= 0) return `0 ${baseUnit}`;
    const sortedPacks = [...(units || [])].sort((a, b) => b.rate - a.rate);
    let remainingQty = qty; let components = []; let topUnit = null;

    for (const pack of sortedPacks) {
        if (pack.rate > 1 && remainingQty >= pack.rate) {
            const count = Math.floor(remainingQty / pack.rate);
            components.push({ count, name: pack.name, rate: pack.rate });
            if (!topUnit) topUnit = pack; 
            remainingQty %= pack.rate;
        }
    }
    if (remainingQty > 0 || components.length === 0) components.push({ count: remainingQty, name: baseUnit, rate: 1 });

    if (components.length === 1 && components[0].rate > 1) {
        let chainResult = [];
        const startIndex = sortedPacks.findIndex(u => u.name === topUnit.name);
        for (let i = startIndex; i < sortedPacks.length; i++) {
            const pack = sortedPacks[i];
            if(pack.rate > 1) chainResult.push(`${qty / pack.rate} ${pack.name}`);
        }
        chainResult.push(`${qty} ${baseUnit}`);
        return [...new Set(chainResult)].join(' = ');
    }
    return components.map(c => `${c.count} ${c.name}`).join(' + ');
}

const fetchData = async () => {
  isLoading.value = true
  try {
    const headers = getAuthHeaders()
    const [inboundRes, prodRes, partnerRes] = await Promise.all([
      fetch(INBOUND_API, { headers }), fetch(PROD_API, { headers }), fetch(PARTNER_API, { headers })
    ])
    
    if (prodRes.ok) productsList.value = await prodRes.json()
    if (partnerRes.ok) {
      const pData = await partnerRes.json()
      const rawPartners = pData.data || pData.Data || pData || []
      partnersList.value = rawPartners.filter(p => p.isSupplier || p.partnerCode?.startsWith('NC'))
    }
    
    if (inboundRes.ok) {
      const rawData = await inboundRes.json()
      const filteredData = rawData.filter(r => !myWarehouseId.value || r.warehouseId === myWarehouseId.value || r.WarehouseId === myWarehouseId.value)

      receipts.value = filteredData.map(r => {
        if (!r.code && r.id) r.code = `PN${r.id.toString().padStart(4, '0')}`;
        const partner = partnersList.value.find(p => p.partnerId === r.partnerId || p.id === r.partnerId || p.Id === r.partnerId);
        r.partnerName = partner ? (partner.partnerName || partner.name || partner.Name) : 'NCC Khác';
        
        r.formattedItems = (r.items || r.Items || []).map(i => {
            const variantId = i.variantId || i.VariantId;
            const prod = productsList.value.find(p => p.id === variantId || p.Id === variantId) || {};
            return { name: prod.name || prod.Name || i.name || i.Name || 'Sản phẩm', text: autoFormatStockText(i.qty || i.Qty || 0, getUnitChain(prod), prod.unit || prod.Unit || 'SL') }
        });
        return r;
      })
    }
  } catch (error) { console.error(error) } finally { isLoading.value = false }
}

const searchQuery = ref(''); const filterStatus = ref('')
const filteredReceipts = computed(() => {
  return receipts.value.filter(r => {
    const code = r.code || r.Code || ''; 
    const matchSearch = code.toLowerCase().includes(searchQuery.value.toLowerCase())
    return matchSearch && (filterStatus.value === '' || r.status === filterStatus.value)
  })
})

const showModal = ref(false); const modalMode = ref('add') 
const formData = ref({ id: 0, code: '', date: getToday(), partnerId: '', warehouseId: myWarehouseId.value, items: [], note: '', status: 'pending' })

const openModal = (mode, receipt = null) => {
  modalMode.value = mode
  if (receipt) {
    const safeItems = (receipt.items || receipt.Items || []).map(i => {
      const variantId = i.variantId || i.VariantId;
      const prod = productsList.value.find(p => p.id === variantId || p.Id === variantId) || {};
      const units = getUnitChain(prod);
      const chain = buildChainEditor(units); 

      return {
        variantId: variantId, sku: prod.sku || prod.Sku || (i.sku || i.Sku), name: prod.name || prod.Name || (i.name || i.Name), 
        baseUnit: prod.unit || prod.Unit || 'SL', units: units, chain: chain, selectedUnit: units[units.length - 1].name,
        qty: i.qty || i.Qty || 0, inputQty: 1, 
        price: i.price || i.Price || prod.importPrice || prod.ImportPrice || 0, 
        nsx: i.nsx || i.Nsx ? (i.nsx || i.Nsx).split('T')[0] : '', hsd: i.hsd || i.Hsd ? (i.hsd || i.Hsd).split('T')[0] : ''
      };
    });
    formData.value = { 
        id: receipt.id || receipt.Id, code: receipt.code || receipt.Code, date: receipt.date || receipt.Date, 
        partnerId: receipt.partnerId || receipt.PartnerId || '', warehouseId: receipt.warehouseId || receipt.WarehouseId || myWarehouseId.value,
        items: safeItems, note: receipt.note || receipt.Note, status: receipt.status || receipt.Status 
    };
  } else { formData.value = { id: 0, code: '', date: getToday(), partnerId: '', warehouseId: myWarehouseId.value, items: [], note: '', status: 'pending' } }
  selectedProductIds.value = []; productSearchQuery.value = ''; showModal.value = true
}
const closeModal = () => showModal.value = false

const getStatusBadge = (status) => {
  switch(status) {
    case 'pending': return { text: 'Chờ duyệt', class: 'bg-amber-100 text-amber-700 border-amber-200' }
    case 'approved': return { text: 'Chờ nhận hàng', class: 'bg-blue-100 text-blue-700 border-blue-200' }
    case 'completed': return { text: 'Đã vào bãi', class: 'bg-emerald-100 text-emerald-700 border-emerald-200' }
    case 'rejected': return { text: 'Bị từ chối', class: 'bg-red-100 text-red-700 border-red-200' }
    default: return { text: 'Khác', class: 'bg-gray-100 text-gray-500' }
  }
}

const handleCompleteReceipt = async (receipt) => {
  if (!confirm(`Xác nhận lô hàng của Phiếu [${receipt.code || receipt.Code}] đã về tới kho và ĐƯA VÀO BÃI TẬP KẾT?`)) return;
  try {
    const res = await fetch(`${INBOUND_API}/${receipt.id || receipt.Id}/complete`, { method: 'PUT', headers: getAuthHeaders() })
    if (res.ok) { alert('Nhận hàng thành công! Hãy sang mục Kho Bãi (Putaway) để cất lên kệ.'); await fetchData(); } 
    else { alert('LỖI HỆ THỐNG'); }
  } catch(e) { console.error(e) }
}

const productSearchQuery = ref(''); const selectedProductIds = ref([]) 
const filteredProducts = computed(() => {
  if (!productSearchQuery.value) return productsList.value;
  return productsList.value.filter(p => (p.sku||p.Sku||'').toLowerCase().includes(productSearchQuery.value.toLowerCase()) || (p.name||p.Name||'').toLowerCase().includes(productSearchQuery.value.toLowerCase()));
})

const handleAddProducts = () => {
  if (selectedProductIds.value.length === 0) return;
  selectedProductIds.value.forEach(pid => {
    const prod = productsList.value.find(p => p.id === pid || p.Id === pid)
    if (prod && !formData.value.items.find(i => i.variantId === pid)) {
        const units = getUnitChain(prod);
        const chain = buildChainEditor(units); 
        const defaultUnit = units[units.length - 1]; 
        formData.value.items.push({ 
          variantId: pid, sku: prod.sku || prod.Sku, name: prod.name || prod.Name, baseUnit: prod.unit || prod.Unit || 'SL', 
          units: units, chain: chain, selectedUnit: defaultUnit.name, inputQty: 1, 
          price: prod.importPrice || prod.ImportPrice || 0, nsx: '', hsd: '' 
        })
    }
  })
  selectedProductIds.value = []; productSearchQuery.value = '' 
}

const recalcAllUnits = (item) => {
    let currentRate = 1; item.units[0].rate = 1; 
    item.chain.forEach(c => { currentRate = currentRate * (c.rel || 1); let targetUnit = item.units.find(x => x.name === c.name); if (targetUnit) targetUnit.rate = currentRate; });
}

const removeItem = (index) => formData.value.items.splice(index, 1)

const getItemTotalQty = (item) => {
    if (modalMode.value === 'view') return item.qty || 0;
    const selected = item.units.find(u => u.name === item.selectedUnit);
    return (item.inputQty || 0) * (selected ? selected.rate : 1);
}

const handleSubmit = async () => {
  if (!formData.value.partnerId) return alert('Vui lòng chọn Nhà cung cấp!');
  if (formData.value.items.length === 0) return alert('Chưa có sản phẩm nào trong phiếu!')
  try {
    const payload = { ...formData.value, code: "", warehouseId: myWarehouseId.value || 1 };
    payload.items = payload.items.map(i => {
        const summary = i.units.filter(u => u.rate > 1).map(u => `1 ${u.name}=${u.rate}`).join(', ');
        return { 
            variantId: i.variantId, qty: getItemTotalQty(i), price: i.price || 0, 
            nsx: i.nsx || null, hsd: i.hsd || null, locationId: null, note: `Nhập ${i.inputQty} ${i.selectedUnit} (${summary})`
        }
    });
    const res = await fetch(INBOUND_API, { method: 'POST', headers: getAuthHeaders(), body: JSON.stringify(payload) })
    if (res.ok) { alert('Lập Phiếu thành công! QL Kho sẽ tiến hành duyệt.'); await fetchData(); closeModal(); } else alert('LỖI HỆ THỐNG');
  } catch(e) { console.error(e) }
}

const handleApprove = async (id) => { if(confirm("Duyệt phiếu này?")) { await fetch(`${INBOUND_API}/${id}/approve`, { method: 'PUT', headers: getAuthHeaders() }); fetchData(); } }
const handleReject = async (id) => { if(confirm("Từ chối phiếu này?")) { await fetch(`${INBOUND_API}/${id}/reject`, { method: 'PUT', headers: getAuthHeaders() }); fetchData(); } }
const printPDF = (receipt) => { alert(`Đang tạo file PDF cho phiếu ${receipt.code || receipt.Code}...`) }

onMounted(() => fetchData())
</script>

<template>
  <div class="space-y-5 md:space-y-6 animate-fade-in pb-10 px-0 md:px-1 relative">
    <div class="flex flex-col sm:flex-row sm:items-center justify-between gap-4">
      <div>
        <h2 class="text-xl md:text-2xl font-bold text-gray-800">Phiếu Nhập Kho</h2>
        <p class="text-xs md:text-sm text-gray-500 mt-1">Ghi nhận hàng hóa từ Nhà cung cấp gửi tới bãi tập kết</p>
        <p class="text-[10px] font-bold text-indigo-600 bg-indigo-50 px-2 py-1 rounded border border-indigo-200 uppercase mt-2 w-fit">Vai trò: {{ currentUserRole }}</p>
      </div>
      <div class="flex flex-wrap items-center gap-2">
        <button v-if="canCreate" @click="openModal('add')" class="bg-primary-600 hover:bg-primary-700 text-white px-4 py-2.5 rounded-lg flex items-center gap-2 text-sm font-semibold shadow-sm"><PlusIcon class="w-5 h-5" /> Lập Phiếu Nhập</button>
      </div>
    </div>

    <div class="bg-white p-3 md:p-4 rounded-xl border border-gray-200 flex flex-col sm:flex-row items-center gap-3 md:gap-4 shadow-sm">
      <div class="relative w-full sm:flex-1"><div class="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none"><MagnifyingGlassIcon class="w-5 h-5 text-gray-400" /></div><input v-model="searchQuery" type="text" class="block w-full pl-10 pr-3 py-2 border border-gray-200 rounded-lg text-sm outline-none focus:ring-1 focus:ring-primary-500" placeholder="Tìm theo mã phiếu..."></div>
      <select v-model="filterStatus" class="w-full sm:w-auto border border-gray-200 rounded-lg text-sm px-4 py-2 outline-none cursor-pointer"><option value="">Tất cả Trạng thái</option><option value="pending">Chờ duyệt</option><option value="approved">Chờ nhận hàng</option><option value="completed">Đã vào bãi tập kết</option></select>
    </div>

    <div class="bg-white rounded-xl border border-gray-200 shadow-sm overflow-hidden">
      <div class="w-full overflow-x-auto custom-scrollbar">
        <table class="min-w-[1100px] w-full divide-y divide-gray-200">
          <thead class="bg-gray-50">
            <tr>
              <th class="px-5 py-3.5 text-left text-xs font-bold text-gray-500 uppercase tracking-wider w-24">Mã Phiếu</th>
              <th class="px-5 py-3.5 text-left text-xs font-bold text-gray-500 uppercase tracking-wider w-32">Ngày lập</th>
              <th class="px-5 py-3.5 text-left text-xs font-bold text-blue-700 uppercase tracking-wider w-40">Nhà Cung Cấp</th>
              <th class="px-5 py-3.5 text-left text-xs font-bold text-emerald-700 uppercase tracking-wider">Chi Tiết Hàng</th>
              <th v-if="canViewPrice" class="px-5 py-3.5 text-right text-xs font-bold text-gray-500 uppercase tracking-wider w-32">Tổng Tiền</th>
              <th class="px-5 py-3.5 text-center text-xs font-bold text-gray-500 uppercase tracking-wider">Trạng thái</th>
              <th class="px-5 py-3.5 text-right text-xs font-bold text-gray-500 uppercase tracking-wider w-40">Thao tác</th>
            </tr>
          </thead>
          <tbody class="divide-y divide-gray-100 bg-white">
            <tr v-if="filteredReceipts.length === 0"><td :colspan="canViewPrice ? 7 : 6" class="px-6 py-16 text-center"><ArrowDownTrayIcon class="w-12 h-12 text-gray-300 mx-auto mb-3" /><h3 class="text-base font-semibold text-gray-700">Chưa có Phiếu Nhập nào</h3></td></tr>
            <tr v-for="receipt in filteredReceipts" :key="receipt.id" class="hover:bg-gray-50 transition-colors">
              <td class="px-5 py-3 text-sm font-bold text-primary-700">{{ receipt.code }}</td><td class="px-5 py-3 text-sm font-medium text-gray-600">{{ receipt.date }}</td>
              <td class="px-5 py-3 text-sm font-bold text-blue-700">{{ receipt.partnerName }}</td>
              
              <td class="px-5 py-3 text-sm">
                <div v-for="(fi, idx) in receipt.formattedItems" :key="idx" class="mb-1 text-gray-800 flex items-center flex-wrap gap-1.5">
                    <span class="font-bold text-gray-700 text-xs">{{ fi.name }}:</span> 
                    <span class="bg-emerald-50 border border-emerald-200 text-emerald-800 px-2 py-0.5 rounded shadow-sm text-[12px] font-bold">{{ fi.text }}</span>
                </div>
              </td>

              <td v-if="canViewPrice" class="px-5 py-3 text-right text-sm font-bold text-emerald-600">{{ formatCurrency(receipt.totalPrice || receipt.TotalPrice) }}</td>

              <td class="px-5 py-3 text-center whitespace-nowrap"><span :class="['text-[10px] font-bold px-2 py-1.5 rounded border uppercase tracking-wider', getStatusBadge(receipt.status).class]">{{ getStatusBadge(receipt.status).text }}</span></td>
              
              <td class="px-5 py-3 text-right whitespace-nowrap">
                <button @click="openModal('view', receipt)" class="p-1.5 text-blue-600 hover:bg-blue-50 rounded transition-colors" title="Xem chi tiết"><EyeIcon class="w-5 h-5" /></button>
                
                <button v-if="['approved', 'completed'].includes(receipt.status || receipt.Status)" @click="printPDF(receipt)" class="p-1.5 text-gray-600 hover:bg-gray-200 rounded transition-colors ml-1" title="In phiếu Nhập (PDF)"><PrinterIcon class="w-5 h-5" /></button>

                <button v-if="(receipt.status || receipt.Status) === 'pending' && canApprove" @click="handleApprove(receipt.id || receipt.Id)" class="p-1.5 text-emerald-600 hover:bg-emerald-50 rounded transition-colors ml-1" title="Duyệt phiếu"><CheckCircleIcon class="w-5 h-5" /></button>
                <button v-if="(receipt.status || receipt.Status) === 'pending' && canApprove" @click="handleReject(receipt.id || receipt.Id)" class="p-1.5 text-red-600 hover:bg-red-50 rounded transition-colors ml-1" title="Từ chối (Hủy)"><XMarkIcon class="w-5 h-5" /></button>
                <button v-if="(receipt.status || receipt.Status) === 'approved' && canConfirmPutaway" @click="handleCompleteReceipt(receipt)" class="p-1.5 text-emerald-600 hover:bg-emerald-50 rounded transition-colors ml-1" title="Nhận hàng vào bãi tập kết"><ArchiveBoxArrowDownIcon class="w-5 h-5" /></button>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>

    <Teleport to="body">
      <div v-if="showModal" class="fixed inset-0 z-50 flex items-center justify-center px-4 bg-slate-900/50 backdrop-blur-sm animate-fade-in">
        <div class="bg-white rounded-xl shadow-2xl w-full max-w-[95vw] lg:max-w-7xl overflow-hidden flex flex-col max-h-[90vh]">
          
          <div class="px-6 py-4 border-b border-gray-100 flex items-center justify-between bg-gray-50 shrink-0"><h3 class="text-lg font-bold flex items-center gap-2"><ArrowDownTrayIcon class="w-6 h-6 text-primary-600"/> {{ modalMode === 'add' ? 'Lập Phiếu Nhập Kho (NV Thu Mua)' : `Chi tiết: ${formData.code}` }}</h3><button @click="closeModal" class="text-gray-400 hover:text-red-500"><XMarkIcon class="w-6 h-6" /></button></div>
          
          <div class="p-6 overflow-y-auto flex-1 custom-scrollbar">
            <form id="inboundForm" @submit.prevent="handleSubmit" class="space-y-6">
              
              <div class="grid grid-cols-1 sm:grid-cols-3 gap-4 bg-slate-50 p-4 rounded-lg border border-slate-200">
                <div>
                  <label class="block text-xs font-bold mb-1">Nhà cung cấp *</label>
                  <select v-model="formData.partnerId" :disabled="modalMode === 'view'" required class="w-full border rounded-lg px-3 py-2 text-sm bg-white cursor-pointer"><option value="" disabled>-- Chọn --</option><option v-for="p in partnersList" :key="p.partnerId||p.id" :value="p.partnerId||p.id">{{ p.partnerName||p.name }}</option></select>
                </div>
                <div><label class="block text-xs font-bold mb-1">Ngày lập *</label><input v-model="formData.date" :disabled="modalMode === 'view'" type="date" required class="w-full border bg-white rounded-lg px-3 py-2 text-sm"></div>
                <div><label class="block text-xs font-bold mb-1">Trạng thái</label><div class="px-3 py-2 text-sm font-bold rounded border text-center" :class="getStatusBadge(formData.status).class">{{ getStatusBadge(formData.status).text }}</div></div>
              </div>

              <div v-if="modalMode === 'add'" class="bg-blue-50 p-4 rounded-xl border border-blue-100">
                <label class="text-sm font-bold text-blue-800 mb-2 block">Tìm & Chọn Hàng hóa:</label>
                <input v-model="productSearchQuery" type="text" class="w-full border border-blue-200 rounded px-3 py-1.5 text-sm mb-2 outline-none focus:ring-1 focus:ring-blue-500 bg-white" placeholder="🔍 Gõ tên hoặc mã SKU...">
                <div class="bg-white border border-blue-200 rounded max-h-32 overflow-y-auto p-2">
                  <label v-for="p in filteredProducts" :key="p.id||p.Id" class="flex items-center gap-3 p-2 hover:bg-blue-50 cursor-pointer border-b"><input type="checkbox" :value="p.id||p.Id" v-model="selectedProductIds" class="w-4 h-4 text-blue-600 rounded"><span class="text-sm flex-1 font-medium">[{{ p.sku||p.Sku }}] {{ p.name||p.Name }}</span></label>
                </div>
                <button type="button" @click="handleAddProducts" class="mt-2 px-4 py-1.5 bg-blue-600 text-white rounded text-sm font-bold disabled:opacity-50 shadow-sm" :disabled="selectedProductIds.length === 0">Đưa {{selectedProductIds.length}} SP xuống lưới</button>
              </div>

              <div class="border rounded-lg overflow-x-auto shadow-sm">
                <table class="w-full text-sm text-left">
                  <thead class="bg-gray-50 text-xs uppercase font-bold text-gray-500 border-b">
                    <tr>
                      <th class="px-3 py-3 min-w-[150px]">Sản Phẩm</th>
                      <th class="px-3 py-3 text-center w-28">NSX - HSD</th>
                      <th v-if="modalMode === 'add'" class="px-3 py-3 text-right bg-amber-50 text-amber-700 min-w-[200px] border-l">Chuỗi Quy cách</th>
                      <th v-if="modalMode === 'add'" class="px-3 py-3 text-center bg-blue-50 text-blue-700 w-32 border-l">ĐVT</th>
                      <th v-if="modalMode === 'add'" class="px-3 py-3 text-center bg-blue-50 text-blue-700 w-28">SL Nhập</th>
                      <th class="px-3 py-3 text-center bg-emerald-50 text-emerald-800 border-l min-w-[250px]">Tổng Lẻ</th>
                      <th v-if="canViewPrice" class="px-3 py-3 text-right bg-indigo-50 text-indigo-700 border-l w-28">Giá Nhập</th>
                      <th v-if="canViewPrice" class="px-3 py-3 text-right bg-indigo-50 text-indigo-700 border-l w-32">Thành tiền</th>
                      <th v-if="modalMode === 'add'" class="px-3 py-3 text-center w-10 border-l">#</th>
                    </tr>
                  </thead>
                  <tbody class="divide-y divide-gray-100">
                    <tr v-if="formData.items.length === 0"><td :colspan="canViewPrice ? 9 : 7" class="px-4 py-8 text-center text-gray-400 italic">Chưa có sản phẩm nào.</td></tr>
                    <tr v-for="(item, idx) in formData.items" :key="idx" class="hover:bg-gray-50">
                        <td class="px-3 py-2"><div class="font-bold text-gray-800">{{ item.sku }}</div><div class="text-xs text-gray-600">{{ item.name }}</div></td>
                        
                        <td class="px-3 py-2 text-center flex flex-col gap-1">
                            <input v-if="modalMode === 'add'" v-model="item.nsx" type="date" class="border border-gray-300 rounded px-1 py-1 text-[10px] bg-white">
                            <span v-else class="text-[10px] text-gray-500 font-bold bg-gray-100 px-1 py-0.5 rounded">NSX: {{item.nsx || '--'}}</span>
                            <input v-if="modalMode === 'add'" v-model="item.hsd" type="date" class="border border-gray-300 rounded px-1 py-1 text-[10px] bg-white">
                            <span v-else class="text-[10px] text-red-500 font-bold bg-red-50 px-1 py-0.5 rounded border border-red-100">HSD: {{item.hsd || '--'}}</span>
                        </td>
                        
                        <td v-if="modalMode === 'add'" class="px-3 py-2 bg-amber-50/10 border-l">
                            <div v-if="item.chain && item.chain.length > 0" class="flex flex-col gap-1.5 justify-center pr-2">
                                <div v-for="(c, cIdx) in item.chain" :key="cIdx" class="flex items-center justify-end gap-1.5">
                                    <span class="text-[10px] font-bold text-gray-600 whitespace-nowrap">1 {{c.name}} =</span>
                                    <input v-model.number="c.rel" @input="recalcAllUnits(item)" type="number" min="1" class="w-14 text-center border border-amber-300 rounded px-1 py-0.5 text-xs font-bold text-amber-700 outline-none focus:ring-1 focus:ring-amber-500 bg-white">
                                    <span class="text-[9px] font-bold text-gray-500 whitespace-nowrap w-12 text-left">{{c.prevName}}</span>
                                </div>
                            </div>
                            <div v-else class="text-[10px] text-gray-400 italic text-center">Hàng lẻ</div>
                        </td>

                        <td v-if="modalMode === 'add'" class="px-3 py-2 text-center bg-blue-50/20 border-l">
                            <select v-model="item.selectedUnit" class="w-full border border-blue-200 rounded px-1 py-1.5 text-xs font-bold text-blue-800 bg-white outline-none cursor-pointer"><option v-for="u in item.units" :key="u.name" :value="u.name">{{ u.name }}</option></select>
                        </td>

                        <td v-if="modalMode === 'add'" class="px-3 py-2 text-center bg-blue-50/20">
                            <input v-model.number="item.inputQty" type="number" min="1" class="w-full text-center border border-blue-300 rounded-lg px-1 py-1.5 text-base font-bold text-blue-700 outline-none shadow-inner bg-white">
                        </td>
                      
                        <td class="px-3 py-2 text-center border-l bg-emerald-50/20">
                            <div class="text-[12px] font-bold text-emerald-800 bg-emerald-50 px-3 py-2 rounded border border-emerald-200 inline-block shadow-sm text-center">
                                {{ autoFormatStockText(getItemTotalQty(item), item.units, item.baseUnit) }}
                            </div>
                        </td>

                        <td v-if="canViewPrice" class="px-3 py-2 text-right bg-indigo-50/20 border-l">
                            <input v-if="modalMode === 'add'" v-model.number="item.price" type="number" min="0" class="w-full text-right border border-indigo-300 rounded px-1 py-1.5 text-sm font-bold text-indigo-700 outline-none">
                            <span v-else class="font-bold text-gray-700">{{ formatCurrency(item.price) }}</span>
                        </td>
                        <td v-if="canViewPrice" class="px-3 py-2 text-right bg-indigo-50/20 font-bold text-indigo-700 border-l">{{ formatCurrency(getItemTotalQty(item) * (item.price || 0)) }}</td>
                        <td v-if="modalMode === 'add'" class="px-3 py-2 text-center border-l"><button type="button" @click="removeItem(idx)" class="text-red-400 hover:text-red-600"><TrashIcon class="w-5 h-5 mx-auto"/></button></td>
                    </tr>
                  </tbody>
                </table>
              </div>
              <div><label class="block text-xs font-bold mb-1">Ghi chú</label><textarea v-model="formData.note" :disabled="modalMode === 'view'" rows="2" class="w-full border rounded-lg px-3 py-2 text-sm bg-white"></textarea></div>
            </form>
          </div>

          <div class="px-6 py-4 border-t flex justify-end gap-3 shrink-0 bg-white shadow-inner">
            <button type="button" @click="closeModal" class="px-5 py-2 border rounded-lg text-sm font-semibold hover:bg-gray-50">{{ modalMode === 'view' ? 'Đóng lại' : 'Hủy bỏ' }}</button>
            <button v-if="modalMode === 'add'" type="submit" form="inboundForm" class="px-6 py-2.5 bg-primary-600 text-white rounded-lg text-sm font-bold hover:bg-primary-700 shadow-md">Lưu Phiếu Nhập</button>
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
</style>