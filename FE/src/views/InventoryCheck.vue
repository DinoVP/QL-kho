<script setup>
import { ref, computed, onMounted } from 'vue'
import { useAuth } from '@/composables/useAuth' 
import { 
  MagnifyingGlassIcon, PlusIcon, EyeIcon, 
  XMarkIcon, ClipboardDocumentCheckIcon, TrashIcon, PencilSquareIcon,
  DocumentArrowUpIcon, ArrowDownTrayIcon, PrinterIcon, CheckCircleIcon, MapPinIcon
} from '@heroicons/vue/24/outline'

const { currentUserRole } = useAuth()
const currentRole = currentUserRole.value?.toLowerCase() || 'ql_kho'

const CHECK_API = 'https://localhost:7139/api/InvCheck'
const STOCK_API = 'https://localhost:7139/api/Stock'
const PROD_API = 'https://localhost:7139/api/Products'
const BRANCH_API = 'https://localhost:7139/api/Branches'

const getAuthHeaders = () => ({ 'Content-Type': 'application/json', 'Authorization': 'Bearer ' + (localStorage.getItem('authToken') || '') })
const myWarehouseId = ref(parseInt(localStorage.getItem('warehouseId')) || 1) 

const canChotSo = computed(() => ['admin', 'giam_doc', 'ql_kho'].includes(currentRole))
const canExport = computed(() => ['admin', 'giam_doc', 'gd_chi_nhanh', 'ql_kho'].includes(currentRole))

const inventoryChecks = ref([]); const stockList = ref([]); const productsList = ref([])
const myWarehouseName = ref('Kho của tôi'); const isLoading = ref(false)

const getToday = () => new Date().toISOString().split('T')[0]

const getUnitChain = (prod) => {
    let units = [{ name: prod.unit || prod.Unit || 'SL', rate: 1, rel: 1 }];
    const conversions = prod.conversions || prod.Conversions || [];
    if (conversions.length > 0) {
        const sortedConvs = [...conversions].sort((a,b) => a.rate - b.rate);
        let prevRate = 1;
        sortedConvs.forEach(c => { units.push({ name: c.altUnit, rate: c.rate, rel: c.rate / prevRate }); prevRate = c.rate; });
    } else if ((prod.conversionRate || prod.ConversionRate) > 1) {
        units.push({ name: 'Thùng/Kiện', rate: prod.conversionRate || prod.ConversionRate, rel: prod.conversionRate || prod.ConversionRate });
    }
    return units.sort((a,b) => a.rate - b.rate);
}

const autoFormatStockText = (qty, units, baseUnit) => {
    if (qty <= 0) return `0 ${baseUnit}`;
    const sortedPacks = [...(units || [])].sort((a, b) => b.rate - a.rate);
    let remainingQty = qty; let components = [];
    for (const pack of sortedPacks) {
        if (pack.rate > 1 && remainingQty >= pack.rate) {
            components.push({ count: Math.floor(remainingQty / pack.rate), name: pack.name });
            remainingQty %= pack.rate;
        }
    }
    if (remainingQty > 0 || components.length === 0) components.push({ count: remainingQty, name: baseUnit });
    return components.map(c => `${c.count} ${c.name}`).join(' + ');
}

// Hàm chia các ô input về 0 ban đầu
const parseQtyToInputs = (qty, units) => {
    const counts = {};
    units.forEach(u => counts[u.name] = 0);
    if (!qty) return counts;
    let remaining = qty;
    const sortedPacks = [...units].sort((a,b) => b.rate - a.rate);
    for(const pack of sortedPacks) {
        counts[pack.name] = Math.floor(remaining / pack.rate);
        remaining %= pack.rate;
    }
    return counts;
}

const printBlankSheet = () => {
    let printWindow = window.open('', '_blank');
    let html = `<html><head><title>Phiếu Kiểm Đếm Kho</title><style>body { font-family: Arial, sans-serif; padding: 20px; } table { width: 100%; border-collapse: collapse; margin-top: 10px; } th, td { border: 1px solid #000; padding: 10px; text-align: left; font-size: 13px; } th { background: #f0f0f0; } .center { text-align: center; } .blank-box { height: 25px; }</style></head><body><h2>PHIẾU KIỂM ĐẾM KHO THỰC TẾ</h2><p class="center"><strong>Kho:</strong> ${myWarehouseName.value} | <strong>Ngày in:</strong> ${new Date().toLocaleDateString('vi-VN')} | <strong>Người kiểm:</strong> ..............................</p><table><thead><tr><th style="width: 50px;" class="center">STT</th><th style="width: 120px;">Mã SKU</th><th>Tên Hàng Hóa</th><th class="center" style="width: 180px;">Tồn Hệ Thống</th><th class="center" style="width: 250px;">KIỂM ĐẾM THỰC TẾ (Ghi tay)</th><th style="width: 150px;">Ghi chú</th></tr></thead><tbody>`;
    stockList.value.forEach((s, index) => { html += `<tr><td class="center">${index + 1}</td><td><strong>${s.sku}</strong></td><td>${s.name}</td><td class="center">${autoFormatStockText(s.systemQty, s.units, s.unit)}</td><td class="blank-box"></td><td></td></tr>`; });
    html += `</tbody></table><div style="margin-top: 30px; display: flex; justify-content: space-around;"><div class="center"><strong>Người lập phiếu</strong><br><br><br>........................</div><div class="center"><strong>Thủ kho xác nhận</strong><br><br><br>........................</div></div></body></html>`;
    printWindow.document.write(html); printWindow.document.close();
    setTimeout(() => { printWindow.print(); }, 500);
}

const fetchData = async () => {
  isLoading.value = true
  try {
    const headers = getAuthHeaders()
    try {
        const branchRes = await fetch(BRANCH_API, { headers })
        if (branchRes.ok) {
            const branches = await branchRes.json()
            for (const b of branches) {
                const whRes = await fetch(`${BRANCH_API}/${b.id || b.Id}/warehouses-detail`, { headers })
                if(whRes.ok) {
                    const whData = await whRes.json()
                    const myWh = whData.find(w => w.warehouseId === myWarehouseId.value);
                    if(myWh) myWarehouseName.value = myWh.whname;
                }
            }
        }
    } catch(e) {}

    const [checkRes, stockRes, prodRes] = await Promise.all([ fetch(CHECK_API, { headers }), fetch(STOCK_API, { headers }), fetch(PROD_API, { headers }) ])
    if (prodRes.ok) productsList.value = await prodRes.json()
    if (stockRes.ok) {
      const rawStocks = await stockRes.json()
      const grouped = {};
      rawStocks.filter(s => s.warehouseId === myWarehouseId.value).forEach(s => {
          const vId = s.variantId || s.VariantId;
          if (!grouped[vId]) grouped[vId] = { variantId: vId, qty: 0 };
          grouped[vId].qty += s.qty || s.Quantity || 0;
      });
      stockList.value = Object.values(grouped).map(s => {
          const prod = productsList.value.find(p => p.id === s.variantId || p.Id === s.variantId) || {};
          return { variantId: s.variantId, sku: prod.sku || prod.Sku, name: prod.name || prod.Name, unit: prod.unit || prod.Unit || 'SL', systemQty: s.qty, units: getUnitChain(prod) }
      });
    }
    if (checkRes.ok) inventoryChecks.value = await checkRes.json();
  } catch (error) { console.error(error) } finally { isLoading.value = false }
}

const searchQuery = ref(''); const filterStatus = ref('')
const filteredChecks = computed(() => inventoryChecks.value.filter(c => c.code.toLowerCase().includes(searchQuery.value.toLowerCase()) && (filterStatus.value === '' || c.status === filterStatus.value) && c.warehouseId === myWarehouseId.value).sort((a, b) => b.id - a.id))

const showModal = ref(false); const modalMode = ref('add') 
const formData = ref({ id: 0, code: '', date: getToday(), warehouseId: myWarehouseId.value, items: [], note: '', status: 'draft' })

const openModal = (mode, check = null) => {
  modalMode.value = mode
  if (check) {
      const mappedItems = (check.items || []).map(i => {
          const prod = productsList.value.find(p => p.id === i.variantId || p.Id === i.variantId) || {};
          const units = getUnitChain(prod);
          return {
              variantId: i.variantId, sku: prod.sku || prod.Sku, name: prod.name || prod.Name, unit: prod.unit || prod.Unit || 'SL', units: units,
              systemQty: i.systemQty || 0, actualQty: i.actualQty || 0, diff: i.diffQty || 0, reason: i.reason || '', countInputs: parseQtyToInputs(i.actualQty || 0, units)
          }
      })
      formData.value = { ...check, items: mappedItems }
  } else { formData.value = { id: 0, code: '', date: getToday(), warehouseId: myWarehouseId.value, items: [], note: '', status: 'draft' } }
  stockSearchQuery.value = ''; showModal.value = true
}

const closeModal = () => showModal.value = false

const getStatusBadge = (status) => {
  switch(status) {
    case 'draft': return { text: 'Bản nháp', class: 'bg-gray-100 text-gray-700' }
    case 'checking': return { text: 'Đang kiểm kê', class: 'bg-blue-100 text-blue-700 border-blue-200' }
    case 'completed': return { text: 'Đã chốt sổ', class: 'bg-teal-100 text-teal-700 border-teal-200' }
    default: return { text: 'Khác', class: 'bg-gray-100 text-gray-500' }
  }
}

// THANH TÌM KIẾM HÀNG TRONG LÚC NHẬP SỐ LIỆU
const stockSearchQuery = ref('')
const filteredStockList = computed(() => {
  if (!stockSearchQuery.value) return [];
  return stockList.value.filter(s => s.sku.toLowerCase().includes(stockSearchQuery.value.toLowerCase()) || s.name.toLowerCase().includes(stockSearchQuery.value.toLowerCase()));
})

const handleAddItem = (stock) => {
  const existingItem = formData.value.items.find(i => i.variantId === stock.variantId)
  if (!existingItem) {
      formData.value.items.unshift({ 
        variantId: stock.variantId, sku: stock.sku, name: stock.name, unit: stock.unit, units: stock.units,
        systemQty: stock.systemQty, 
        actualQty: 0, // FIX LỖI SẾP YÊU CẦU: Khởi tạo về 0 để ép đếm lại từ đầu
        countInputs: parseQtyToInputs(0, stock.units), // Tất cả các ô Pallet, Thùng đều = 0
        diff: 0 - stock.systemQty, // Mặc định lệch toàn bộ cho đến khi nhập số
        reason: '' 
      })
  }
  stockSearchQuery.value = '' 
}

const removeItem = (index) => formData.value.items.splice(index, 1)

const calcActual = (item) => {
    let total = 0;
    item.units.forEach(u => { total += (item.countInputs[u.name] || 0) * u.rate; });
    item.actualQty = total;
    item.diff = item.actualQty - item.systemQty;
}

const handleSubmit = async () => {
  if (formData.value.items.length === 0) return alert('Chưa có dữ liệu kiểm kê!')
  try {
      formData.value.warehouseId = myWarehouseId.value; 
      const method = modalMode.value === 'add' ? 'POST' : 'PUT';
      const url = modalMode.value === 'add' ? CHECK_API : `${CHECK_API}/${formData.value.id}`;
      const res = await fetch(url, { method, headers: getAuthHeaders(), body: JSON.stringify({ ...formData.value, code: formData.value.code || "" }) })
      if (res.ok) { alert(modalMode.value === 'add' ? 'Lưu Phiếu thành công!' : 'Cập nhật thành công!'); await fetchData(); closeModal(); } else { alert('LỖI HỆ THỐNG'); }
  } catch(e) { console.error(e) }
}

const handleChotSo = async () => {
  if(!confirm(`XÁC NHẬN CHỐT SỔ phiếu ${formData.value.code}?\nThao tác này sẽ cập nhật thẳng vào Database kho thực tế.`)) return;
  try {
      const res = await fetch(`${CHECK_API}/${formData.value.id}/complete`, { method: 'PUT', headers: getAuthHeaders() })
      if(res.ok) { alert('Đã chốt sổ thành công!'); await fetchData(); closeModal(); } else { alert('LỖI HỆ THỐNG'); }
  } catch(e) { console.error(e) }
}

const downloadPDF = (check) => { alert(`Đang xuất file PDF cho phiếu kiểm kê: ${check.code}`) }

onMounted(() => fetchData())
</script>

<template>
  <div class="space-y-5 md:space-y-6 animate-fade-in pb-10 px-0 md:px-1 relative">
    
    <div class="flex flex-col sm:flex-row sm:items-center justify-between gap-4">
      <div>
        <h2 class="text-xl md:text-2xl font-bold text-gray-800">Kiểm Kê Kho</h2>
        <p class="text-[10px] font-bold text-indigo-600 bg-indigo-50 px-2 py-1 rounded inline-block mt-1 border border-indigo-200 uppercase">Vai trò: {{ currentRole }}</p>
      </div>
      <div class="flex flex-wrap items-center gap-2">
        <button v-if="canExport" @click="printBlankSheet" class="bg-white border border-gray-300 text-gray-800 px-3 py-2.5 rounded-lg text-sm font-semibold hover:bg-gray-100 shadow-sm flex items-center gap-1.5"><PrinterIcon class="w-4 h-4"/> In Phiếu Đếm Kho (Bản Trống)</button>
        <button @click="openModal('add')" class="bg-teal-600 hover:bg-teal-700 text-white px-4 py-2.5 rounded-lg flex items-center gap-2 text-sm font-semibold shadow-sm transition-colors"><PlusIcon class="w-5 h-5" /> Nhập Số Liệu (Từ giấy)</button>
      </div>
    </div>

    <div class="bg-white p-3 md:p-4 rounded-xl border border-gray-200 flex flex-col sm:flex-row items-center gap-3 md:gap-4 shadow-sm">
      <div class="relative w-full sm:flex-1"><div class="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none"><MagnifyingGlassIcon class="w-5 h-5 text-gray-400" /></div><input v-model="searchQuery" type="text" class="block w-full pl-10 pr-3 py-2 border border-gray-200 rounded-lg text-sm outline-none focus:ring-1 focus:ring-teal-500" placeholder="Tìm theo mã phiếu kiểm kê..."></div>
      <select v-model="filterStatus" class="w-full sm:w-auto border border-gray-200 rounded-lg text-sm px-4 py-2 outline-none focus:ring-1 focus:ring-teal-500 cursor-pointer"><option value="">Tất cả Trạng thái</option><option value="draft">Bản nháp</option><option value="checking">Đang kiểm kê</option><option value="completed">Đã chốt sổ</option></select>
    </div>

    <div class="bg-white rounded-xl border border-gray-200 shadow-sm overflow-hidden">
      <div class="w-full overflow-x-auto custom-scrollbar">
        <table class="min-w-[1000px] w-full divide-y divide-gray-200">
          <thead class="bg-gray-50">
            <tr><th class="px-5 py-3.5 text-left text-xs font-bold text-gray-500 uppercase">Mã Phiếu</th><th class="px-5 py-3.5 text-left text-xs font-bold text-gray-500 uppercase">Ngày kiểm</th><th class="px-5 py-3.5 text-left text-xs font-bold text-gray-500 uppercase">Kho kiểm kê</th><th class="px-5 py-3.5 text-center text-xs font-bold text-gray-500 uppercase">Số mặt hàng</th><th class="px-5 py-3.5 text-center text-xs font-bold text-gray-500 uppercase">Trạng thái</th><th class="px-5 py-3.5 text-right text-xs font-bold text-gray-500 uppercase">Thao tác</th></tr>
          </thead>
          <tbody class="divide-y divide-gray-100 bg-white">
            <tr v-if="filteredChecks.length === 0"><td colspan="6" class="px-6 py-16 text-center"><ClipboardDocumentCheckIcon class="w-12 h-12 text-gray-300 mx-auto mb-3" /><h3 class="text-base font-semibold text-gray-700">Chưa có Phiếu Kiểm Kê nào</h3></td></tr>
            <tr v-for="check in filteredChecks" :key="check.id" class="hover:bg-gray-50 transition-colors">
              <td class="px-5 py-3 text-sm font-bold text-teal-700">{{ check.code }}</td><td class="px-5 py-3 text-sm font-medium text-gray-600">{{ check.date }}</td><td class="px-5 py-3 text-sm font-bold text-gray-800">Kho {{ check.warehouseId }}</td><td class="px-5 py-3 text-sm text-center font-bold text-gray-900">{{ check.items?.length || 0 }} SKU</td>
              <td class="px-5 py-3 text-center"><span :class="['text-[10px] font-bold px-2 py-1 rounded border uppercase tracking-wider', getStatusBadge(check.status).class]">{{ getStatusBadge(check.status).text }}</span></td>
              <td class="px-5 py-3 text-right space-x-1.5 whitespace-nowrap">
                <button v-if="check.status === 'completed'" @click.stop="downloadPDF(check)" class="p-1.5 text-red-600 hover:bg-red-50 rounded border border-transparent" title="Tải file PDF lưu trữ"><ArrowDownTrayIcon class="w-4 h-4" /></button>
                <button v-if="check.status !== 'completed'" @click="openModal('edit', check)" class="p-1.5 text-amber-600 hover:bg-amber-50 rounded border border-transparent" title="Tiếp tục gõ số liệu"><PencilSquareIcon class="w-4 h-4" /></button>
                <button @click="openModal('view', check)" class="p-1.5 text-teal-600 hover:bg-teal-50 rounded bg-teal-50 border border-teal-100" title="Xem chi tiết"><EyeIcon class="w-4 h-4" /></button>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>

    <Teleport to="body">
      <div v-if="showModal" class="fixed inset-0 z-50 flex items-center justify-center px-4 bg-slate-900/50 backdrop-blur-sm animate-fade-in">
        <div class="bg-white rounded-xl shadow-2xl w-full max-w-6xl overflow-hidden flex flex-col max-h-[95vh]">
          
          <div class="px-6 py-4 border-b border-teal-100 flex items-center justify-between bg-teal-50 shrink-0">
            <h3 class="text-lg font-bold text-teal-800 flex items-center gap-2"><ClipboardDocumentCheckIcon class="w-6 h-6 text-teal-600"/> {{ modalMode === 'add' ? 'Nhập số liệu Kiểm Kê (Từ giấy)' : `Chi tiết Phiếu: ${formData.code}` }}</h3>
            <div class="flex items-center gap-4"><div class="text-sm font-bold flex items-center bg-white px-3 py-1 rounded-lg border border-teal-200">Trạng thái: <span :class="['ml-2 px-2 py-0.5 rounded text-[10px] uppercase border', getStatusBadge(formData.status).class]">{{ getStatusBadge(formData.status).text }}</span></div><button @click="closeModal" class="text-gray-400 hover:text-red-500"><XMarkIcon class="w-6 h-6" /></button></div>
          </div>
          
          <div class="p-6 overflow-y-auto flex-1 custom-scrollbar bg-slate-50/50">
            <form id="checkForm" @submit.prevent="handleSubmit" class="space-y-6">
              
              <div class="bg-white p-4 rounded-xl border border-gray-200 shadow-sm">
                <div class="grid grid-cols-1 md:grid-cols-4 gap-4">
                  <div><label class="block text-xs font-bold mb-1">Mã Phiếu</label><input type="text" disabled class="w-full border rounded-lg px-3 py-2 text-sm bg-gray-100 italic" :placeholder="modalMode === 'add' ? 'Hệ thống tự sinh' : formData.code"></div>
                  <div><label class="block text-xs font-bold mb-1">Ngày nhập số liệu *</label><input v-model="formData.date" :disabled="modalMode === 'view'" type="date" required class="w-full border rounded-lg px-3 py-2 text-sm focus:ring-teal-500 outline-none"></div>
                  <div class="md:col-span-2"><label class="block text-xs font-bold mb-1 text-teal-700">Kho đang kiểm kê</label><div class="w-full border border-teal-200 bg-teal-50 rounded-lg px-3 py-2 text-sm font-bold text-teal-800 flex items-center"><MapPinIcon class="w-4 h-4 mr-2" /> {{ myWarehouseName }} (Kho ID: {{ myWarehouseId }})</div></div>
                </div>
              </div>

              <div>
                <div v-if="modalMode !== 'view'" class="mb-4 bg-teal-50 p-4 rounded-xl border border-teal-100 relative shadow-sm">
                  <label class="text-xs font-bold text-teal-800 mb-2 block uppercase tracking-wide">Tra cứu SP trên phiếu giấy để thêm vào lưới nhập liệu:</label>
                  <div class="relative">
                    <div class="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none"><MagnifyingGlassIcon class="w-5 h-5 text-teal-500" /></div>
                    <input v-model="stockSearchQuery" type="text" class="w-full border border-teal-300 rounded-lg pl-10 pr-3 py-2.5 text-sm focus:ring-2 focus:ring-teal-500 outline-none shadow-inner bg-white" placeholder="Gõ mã SKU hoặc Tên sản phẩm...">
                  </div>
                  <div v-if="filteredStockList.length > 0" class="absolute z-10 mt-1 w-[calc(100%-2rem)] bg-white border border-teal-200 rounded-lg shadow-xl max-h-60 overflow-y-auto">
                      <div v-for="stock in filteredStockList" :key="stock.variantId" @click="handleAddItem(stock)" class="p-3 hover:bg-teal-50 cursor-pointer border-b border-gray-100 flex items-center justify-between transition-colors">
                          <div class="flex flex-col"><span class="font-bold text-teal-800 text-sm">{{stock.sku}}</span><span class="text-gray-700 text-xs">{{stock.name}}</span></div>
                          <div class="text-[11px] text-gray-500 font-bold bg-gray-100 px-2 py-1 rounded border">Tồn PM: {{stock.systemQty}} {{stock.unit}}</div>
                      </div>
                  </div>
                </div>

                <div class="border border-gray-200 rounded-xl overflow-x-auto shadow-sm bg-white">
                  <table class="w-full text-sm text-left">
                    <thead class="bg-gray-100 text-xs uppercase font-bold text-gray-600 border-b border-gray-200">
                      <tr><th class="px-4 py-3 w-32">SKU</th><th class="px-4 py-3 min-w-[150px]">Tên Hàng</th><th class="px-4 py-3 text-right bg-blue-50 border-l border-gray-200 w-48 text-blue-800">Tồn Hệ Thống</th><th class="px-4 py-3 text-center bg-teal-50 border-l border-gray-200 min-w-[280px] text-teal-800">Số Lượng Thực Tế (Nhập từ giấy)</th><th class="px-4 py-3 text-center border-l border-gray-200 w-28">Chênh lệch</th><th class="px-4 py-3 text-left w-48 border-l border-gray-200">Ghi chú</th><th v-if="modalMode !== 'view'" class="px-4 py-3 text-center w-10 border-l border-gray-200">#</th></tr>
                    </thead>
                    <tbody class="divide-y divide-gray-100">
                      <tr v-if="formData.items.length === 0"><td :colspan="modalMode !== 'view' ? 7 : 6" class="px-4 py-12 text-center text-gray-400 italic">Vui lòng gõ tìm kiếm để thêm sản phẩm vừa đếm từ giấy vào.</td></tr>
                      <tr v-for="(item, idx) in formData.items" :key="idx" class="hover:bg-gray-50 transition-colors">
                        <td class="px-4 py-3 font-bold text-gray-800">{{ item.sku }}</td><td class="px-4 py-3 text-gray-700">{{ item.name }}</td>
                        
                        <td class="px-4 py-3 text-right bg-blue-50/20 border-l border-gray-100">
                            <span class="block text-xs font-bold text-indigo-700">{{ autoFormatStockText(item.systemQty, item.units, item.unit) }}</span>
                            <span class="block text-[10px] text-gray-500 font-medium">(= {{ item.systemQty }} {{ item.unit }})</span>
                        </td>
                        
                        <td class="px-4 py-3 bg-teal-50/30 border-l border-gray-100">
                          <div v-if="modalMode !== 'view'" class="flex flex-col gap-1.5 justify-center">
                              <div v-for="u in [...item.units].reverse()" :key="u.name" class="flex items-center border border-teal-200 rounded overflow-hidden shadow-sm bg-white focus-within:ring-1 focus-within:ring-teal-500 focus-within:border-teal-500">
                                  <span class="w-20 text-center bg-teal-50 text-teal-800 text-[10px] font-bold py-1.5 border-r border-teal-200">{{ u.name }}</span>
                                  <input v-model.number="item.countInputs[u.name]" @input="calcActual(item)" type="number" min="0" class="flex-1 w-full text-center py-1 text-sm font-bold text-teal-900 outline-none">
                              </div>
                              <div class="text-[11px] text-center font-bold text-teal-700 mt-1 border-t border-teal-200 pt-1">Tổng đếm (Máy tự nhân): {{ item.actualQty }} {{ item.unit }}</div>
                          </div>
                          <div v-else class="text-center font-bold text-teal-700">
                             <span class="block text-xs">{{ autoFormatStockText(item.actualQty, item.units, item.unit) }}</span>
                             <span class="block text-[10px] text-gray-500 font-medium">(= {{ item.actualQty }} {{ item.unit }})</span>
                          </div>
                        </td>
                        
                        <td class="px-4 py-3 text-center font-bold border-l border-gray-100 align-middle">
                          <span v-if="item.diff === 0" class="text-emerald-600 bg-emerald-50 px-2 py-1 rounded w-full block">Khớp (0)</span>
                          <span v-else-if="item.diff > 0" class="text-amber-600 bg-amber-50 px-2 py-1 rounded w-full block">Thừa (+{{ item.diff }})</span>
                          <span v-else class="text-red-600 bg-red-50 px-2 py-1 rounded w-full block">Thiếu ({{ item.diff }})</span>
                        </td>
                        
                        <td class="px-4 py-3 border-l border-gray-100 align-top"><input v-if="modalMode !== 'view'" v-model="item.reason" type="text" class="w-full border border-gray-300 rounded px-2 py-1.5 text-xs outline-none focus:ring-1 focus:ring-teal-500 bg-white" placeholder="Giải trình lệch (nếu có)"><span v-else class="text-xs text-gray-600">{{ item.reason || '—' }}</span></td>
                        <td v-if="modalMode !== 'view'" class="px-4 py-3 text-center border-l border-gray-100 align-middle"><button type="button" @click="removeItem(idx)" class="text-red-400 hover:text-red-600 transition-colors"><TrashIcon class="w-5 h-5 mx-auto"/></button></td>
                      </tr>
                    </tbody>
                  </table>
                </div>
              </div>
              
              <div><label class="block text-xs font-bold mb-1">Ghi chú tổng thể</label><textarea v-model="formData.note" :disabled="modalMode === 'view'" rows="2" class="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm focus:ring-1 focus:ring-teal-500 bg-white outline-none" placeholder="Ghi chú thêm về đợt kiểm kê..."></textarea></div>
            </form>
          </div>

          <div class="px-6 py-4 border-t border-gray-100 flex flex-col sm:flex-row justify-end gap-3 bg-gray-50 shrink-0">
            <div class="flex gap-2 w-full sm:w-auto">
              <template v-if="modalMode === 'edit' && formData.status === 'draft'"><button @click="formData.status = 'checking'; handleSubmit()" class="px-4 py-2.5 bg-blue-100 text-blue-700 hover:bg-blue-200 border border-blue-200 rounded-lg text-sm font-bold shadow-sm transition-colors">Bắt đầu tính lệch</button></template>
              <template v-if="modalMode === 'edit' && formData.status === 'checking' && canChotSo"><button @click="handleChotSo" class="px-4 py-2.5 bg-teal-600 hover:bg-teal-700 text-white rounded-lg text-sm font-bold flex items-center gap-2 shadow-sm transition-colors"><CheckCircleIcon class="w-5 h-5"/> Chốt Sổ (Cập nhật tồn)</button></template>
              <button type="button" @click="closeModal" class="flex-1 sm:flex-none px-5 py-2.5 border border-gray-300 text-gray-700 rounded-lg text-sm font-semibold hover:bg-gray-100 bg-white transition-colors">{{ modalMode === 'view' ? 'Đóng lại' : 'Hủy bỏ' }}</button>
              <button v-if="modalMode !== 'view'" type="submit" form="checkForm" class="flex-1 sm:flex-none px-5 py-2.5 bg-slate-800 text-white rounded-lg text-sm font-bold hover:bg-slate-900 shadow-md transition-colors">Lưu Bản Nháp</button>
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
</style>