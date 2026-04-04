<script setup>
import { ref, computed, onMounted } from 'vue'
import { 
  MagnifyingGlassIcon, PlusIcon, EyeIcon, 
  XMarkIcon, DocumentArrowUpIcon, TrashIcon, PencilSquareIcon,
  PrinterIcon, DocumentTextIcon, ArrowDownTrayIcon, TruckIcon
} from '@heroicons/vue/24/outline'

const OUTBOUND_API = 'https://localhost:7139/api/Outbound'
const STOCK_API = 'https://localhost:7139/api/Stock' 
const PROD_API = 'https://localhost:7139/api/Products'
const PARTNER_API = 'https://localhost:7139/api/CrmPartners'
const LOC_API = 'https://localhost:7139/api/Locations'

const getAuthHeaders = () => ({ 'Content-Type': 'application/json', 'Authorization': 'Bearer ' + (localStorage.getItem('authToken') || '') })

const myWarehouseId = ref(parseInt(localStorage.getItem('warehouseId')) || null)

const outboundReceipts = ref([]); const customers = ref([]); 
const productsList = ref([]); const locationsList = ref([]); const stockList = ref([])
const isLoading = ref(false)

const getToday = () => new Date().toISOString().split('T')[0]

const fetchData = async () => {
  isLoading.value = true
  try {
    const headers = getAuthHeaders()
    const [outRes, stockRes, prodRes, partRes, locRes] = await Promise.all([
      fetch(OUTBOUND_API, { headers }), fetch(STOCK_API, { headers }), fetch(PROD_API, { headers }),
      fetch(PARTNER_API, { headers }), fetch(LOC_API, { headers })
    ])
    
    if (prodRes.ok) productsList.value = await prodRes.json()
    if (locRes.ok) locationsList.value = await locRes.json()
    if (partRes.ok) {
      const pData = await partRes.json(); const rawPartners = pData.data || pData.Data || pData || []
      customers.value = rawPartners.filter(p => p.isCustomer || p.partnerCode?.startsWith('KH'))
    }
    
    // LOAD TỒN KHO THỰC TẾ (Lọc đúng kho mình)
    if (stockRes.ok) {
      const rawStocks = await stockRes.json();
      const filteredStocks = rawStocks.filter(s => !myWarehouseId.value || s.warehouseId === myWarehouseId.value || s.WarehouseId === myWarehouseId.value)
      
      stockList.value = filteredStocks.map(s => {
        const prod = productsList.value.find(p => p.id === s.variantId || p.Id === s.variantId) || {};
        const loc = locationsList.value.find(l => l.id === s.locationId || l.Id === s.locationId) || {};
        return {
          stockId: s.id, variantId: s.variantId, qtyAvailable: s.qty || 0,
          nsx: s.nsx || '', hsd: s.hsd || '',
          sku: prod.sku || prod.Sku || 'N/A', name: prod.name || prod.Name || 'Lỗi', unit: prod.unit || prod.Unit || 'Cái',
          price: prod.price || prod.Price || 0, conversionRate: prod.conversionRate || prod.ConversionRate || 24,
          locationId: s.locationId, locationCode: loc.code || loc.Code || 'Khu chung'
        }
      })
    }

    if (outRes.ok) {
      const rawOutbound = await outRes.json()
      // LỌC CHẶT CHẼ DỮ LIỆU: Chỉ thấy phiếu xuất của kho mình
      const filteredOutbound = rawOutbound.filter(r => !myWarehouseId.value || r.warehouseId === myWarehouseId.value || r.WarehouseId === myWarehouseId.value)

      outboundReceipts.value = filteredOutbound.map(r => {
        const cusId = r.customerId || r.CustomerId;
        const cus = customers.value.find(c => c.partnerId === cusId || c.id === cusId);
        r.customerName = cus ? (cus.partnerName || cus.name) : `Khách hàng (ID: ${cusId})`;
        if (!r.code && r.id) r.code = `PX${r.id.toString().padStart(4, '0')}`;
        return r;
      })
    }
  } catch (error) { console.error('Lỗi tải dữ liệu:', error) } finally { isLoading.value = false }
}

const getCustomerName = (id) => {
  const cus = customers.value.find(c => c.partnerId === id || c.id === id);
  return cus ? (cus.partnerName || cus.name) : `Chưa xác định (ID: ${id})`;
}

const searchQuery = ref(''); const filterStatus = ref('')
const filteredReceipts = computed(() => {
  return outboundReceipts.value.filter(r => {
    const code = r.code || r.Code || ''; const cName = getCustomerName(r.customerId || r.CustomerId).toLowerCase(); const status = r.status || r.Status || '';
    const matchSearch = code.toLowerCase().includes(searchQuery.value.toLowerCase()) || cName.includes(searchQuery.value.toLowerCase())
    return matchSearch && (filterStatus.value === '' || status === filterStatus.value)
  })
})

const showModal = ref(false); const modalMode = ref('add') 
const formData = ref({ id: 0, code: '', date: getToday(), customerId: '', warehouseId: myWarehouseId.value, items: [], note: '', status: 'pending' })

const openModal = (mode, receipt = null) => {
  modalMode.value = mode
  if (receipt) {
    const safeItems = (receipt.items || receipt.Items || []).map(i => {
      const variantId = i.variantId || i.VariantId;
      const prod = productsList.value.find(p => p.id === variantId || p.Id === variantId) || {};
      const loc = locationsList.value.find(l => l.id === (i.locationId||i.LocationId) || l.Id === (i.locationId||i.LocationId)) || {};
      
      const stock = stockList.value.find(s => s.variantId === variantId && s.locationId === (i.locationId||i.LocationId) && s.nsx === (i.nsx||i.Nsx)?.split('T')[0] && s.hsd === (i.hsd||i.Hsd)?.split('T')[0])
      const available = stock ? stock.qtyAvailable : 0;
      const convRate = prod.conversionRate || prod.ConversionRate || 24;

      return {
        variantId: variantId, sku: prod.sku || prod.Sku || (i.sku || i.Sku), name: prod.name || prod.Name || (i.name || i.Name), 
        unit: prod.unit || prod.Unit || (i.unit || i.Unit), 
        qty: i.qty || i.Qty, 
        boxQty: Math.floor((i.qty || i.Qty) / convRate), 
        conversionRate: convRate,      
        maxQty: available + (i.qty || i.Qty), 
        price: i.price || i.Price, 
        locationId: i.locationId || i.LocationId, locationCode: loc.code || loc.Code || 'Khu chung',
        nsx: i.nsx || i.Nsx ? (i.nsx || i.Nsx).split('T')[0] : '', hsd: i.hsd || i.Hsd ? (i.hsd || i.Hsd).split('T')[0] : ''
      };
    });
    formData.value = { id: receipt.id || receipt.Id, code: receipt.code || receipt.Code, date: receipt.date || receipt.Date, customerId: receipt.customerId || receipt.CustomerId, warehouseId: receipt.warehouseId || receipt.WarehouseId || myWarehouseId.value, items: safeItems, note: receipt.note || receipt.Note, status: receipt.status || receipt.Status };
  } else { formData.value = { id: 0, code: '', date: getToday(), customerId: '', warehouseId: myWarehouseId.value, items: [], note: '', status: 'pending' } }
  selectedStockIdsToAdd.value = []; stockSearchQuery.value = ''; showModal.value = true
}
const closeModal = () => showModal.value = false

const getStatusBadge = (status) => {
  switch(status) {
    case 'pending': return { text: 'Chờ duyệt', class: 'bg-amber-100 text-amber-700 border-amber-200' }
    case 'approved': return { text: 'Chờ lấy hàng', class: 'bg-purple-100 text-purple-700 border-purple-200' }
    case 'completed': return { text: 'Đã xuất kho', class: 'bg-emerald-100 text-emerald-700 border-emerald-200' }
    case 'rejected': return { text: 'Từ chối', class: 'bg-red-100 text-red-700 border-red-200' }
    default: return { text: 'Khác', class: 'bg-gray-100 text-gray-500' }
  }
}

// BỘ LỌC CHỌN TỪ TỒN KHO
const stockSearchQuery = ref(''); const selectedStockIdsToAdd = ref([]) 
const filteredStockList = computed(() => {
  if (!stockSearchQuery.value) return stockList.value;
  return stockList.value.filter(s => 
    s.sku.toLowerCase().includes(stockSearchQuery.value.toLowerCase()) || 
    s.name.toLowerCase().includes(stockSearchQuery.value.toLowerCase()) ||
    s.locationCode.toLowerCase().includes(stockSearchQuery.value.toLowerCase())
  );
})

const handleAddMultipleStocks = () => {
  if (selectedStockIdsToAdd.value.length === 0) return;
  selectedStockIdsToAdd.value.forEach(stockId => {
    const stock = stockList.value.find(s => s.stockId === stockId)
    if (stock) {
      const exists = formData.value.items.find(i => i.variantId === stock.variantId && i.locationId === stock.locationId && i.nsx === stock.nsx && i.hsd === stock.hsd)
      if(!exists) {
        formData.value.items.push({ 
          variantId: stock.variantId, sku: stock.sku, name: stock.name, unit: stock.unit, 
          conversionRate: stock.conversionRate, 
          boxQty: 1,                 
          qty: 1 * stock.conversionRate,            
          maxQty: stock.qtyAvailable, 
          price: stock.price, locationId: stock.locationId, locationCode: stock.locationCode, 
          nsx: stock.nsx, hsd: stock.hsd 
        })
      }
    }
  })
  selectedStockIdsToAdd.value = []; stockSearchQuery.value = '' 
}

const calculateQty = (item) => {
    item.qty = (item.boxQty || 0) * (item.conversionRate || 1);
    if(item.qty > item.maxQty) {
        alert(`[LỖI] Sếp xuất ${item.qty} ${item.unit} nhưng Tồn kho lô này chỉ còn ${item.maxQty} ${item.unit} thôi! Hệ thống sẽ tự trả về mức tối đa.`);
        item.boxQty = Math.floor(item.maxQty / (item.conversionRate || 1));
        item.qty = item.boxQty * (item.conversionRate || 1);
    }
}

const removeItem = (index) => formData.value.items.splice(index, 1)

const totalQty = computed(() => formData.value.items.reduce((sum, item) => sum + (item.qty || 0), 0))
const totalPrice = computed(() => formData.value.items.reduce((sum, item) => sum + ((item.qty || 0) * (item.price || 0)), 0))
const formatCurrency = (val) => new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(val || 0)

const handleSubmit = async () => {
  if (formData.value.items.length === 0) return alert('Chưa chọn mặt hàng nào từ Tồn kho!')
  if (!formData.value.customerId) return alert('Chưa chọn Khách hàng!')
  
  try {
    const method = modalMode.value === 'add' ? 'POST' : 'PUT'
    const url = modalMode.value === 'add' ? OUTBOUND_API : `${OUTBOUND_API}/${formData.value.id}`
    const payload = { ...formData.value }; 
    if (modalMode.value === 'add') payload.code = ""; 
    payload.warehouseId = myWarehouseId.value || 1; // Gắn ID kho khi tạo phiếu
    
    const res = await fetch(url, { method, headers: getAuthHeaders(), body: JSON.stringify(payload) })
    if (res.ok) { alert('Lưu Phiếu thành công!'); await fetchData(); closeModal(); } 
    else { let errMsg = "Lỗi hệ thống!"; try { const text = await res.text(); if (text) errMsg = JSON.parse(text).message || errMsg; } catch(e) {} alert('LỖI: ' + errMsg); }
  } catch(e) { console.error(e) }
}

const handleDelete = async (id, code) => {
  if (!confirm(`Xóa phiếu xuất [${code}]?`)) return;
  const res = await fetch(`${OUTBOUND_API}/${id}`, { method: 'DELETE', headers: getAuthHeaders() })
  if (res.ok) { await fetchData(); alert('Đã xóa!'); }
}

const handleCompleteReceipt = async (receipt) => {
  if (!confirm(`Xác nhận phiếu [${receipt.code || receipt.Code}] ĐÃ XUẤT KHO? (Hàng sẽ bị trừ VĨNH VIỄN khỏi DB Tồn Kho)`)) return;
  try {
    const res = await fetch(`${OUTBOUND_API}/${receipt.id || receipt.Id}/complete`, { method: 'PUT', headers: getAuthHeaders() })
    if (res.ok) { 
      alert('Xuất kho thành công! Hàng đã rời kệ.'); 
      await fetchData(); 
    } else { 
      let errMsg = "Lỗi hệ thống!"; try { const t = await res.text(); if(t) errMsg = JSON.parse(t).message || errMsg; } catch(e){} alert('LỖI TRỪ KHO: ' + errMsg); 
    }
  } catch(e) { console.error(e) }
}

onMounted(() => fetchData())
</script>

<template>
  <div class="space-y-6 animate-fade-in pb-10 px-1 relative">
    <div class="flex justify-between items-center">
      <div>
        <h2 class="text-2xl font-bold text-gray-800">Phiếu Xuất kho</h2>
        <p class="text-[10px] font-bold text-indigo-600 bg-indigo-50 px-2 py-1 rounded inline-block mt-1 border border-indigo-200 uppercase">Kho ID: {{ myWarehouseId || 'Tất cả Kho (Admin)' }}</p>
      </div>
      
      <div class="flex gap-2">
        <button @click="() => alert('Đang xuất báo cáo Excel...')" class="bg-white border border-gray-200 text-emerald-600 px-3 py-2 rounded-lg text-sm font-semibold hover:bg-emerald-50 transition-colors shadow-sm flex items-center gap-1.5">
          <DocumentTextIcon class="w-4 h-4"/> Xuất Excel
        </button>
        <button @click="() => alert('Đang xuất file PDF...')" class="bg-white border border-gray-200 text-red-600 px-3 py-2 rounded-lg text-sm font-semibold hover:bg-red-50 transition-colors shadow-sm flex items-center gap-1.5">
          <ArrowDownTrayIcon class="w-4 h-4"/> Xuất PDF
        </button>
        <button @click="openModal('add')" class="bg-amber-600 hover:bg-amber-700 text-white px-4 py-2 rounded-lg flex items-center gap-2 text-sm font-semibold shadow-sm"><PlusIcon class="w-5 h-5" /> Lập Phiếu Xuất</button>
      </div>
    </div>

    <div class="bg-white p-4 rounded-xl border flex gap-4 shadow-sm">
      <div class="relative flex-1"><div class="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none"><MagnifyingGlassIcon class="w-5 h-5 text-gray-400" /></div><input v-model="searchQuery" type="text" class="block w-full pl-10 pr-3 py-2 border rounded-lg text-sm outline-none focus:ring-1 focus:ring-amber-500" placeholder="Tìm theo mã phiếu, khách hàng..."></div>
      <select v-model="filterStatus" class="border rounded-lg text-sm px-4 py-2 outline-none focus:ring-1 focus:ring-amber-500 cursor-pointer"><option value="">Tất cả Trạng thái</option><option value="pending">Chờ duyệt</option><option value="approved">Chờ lấy hàng</option><option value="completed">Đã xuất kho</option><option value="rejected">Bị từ chối</option></select>
    </div>

    <div class="bg-white rounded-xl border shadow-sm overflow-hidden">
      <div class="w-full overflow-x-auto">
        <table class="w-full text-sm text-left"><thead class="bg-gray-50 uppercase font-bold text-gray-500 text-xs"><tr><th class="px-5 py-3">Mã Phiếu</th><th class="px-5 py-3">Ngày</th><th class="px-5 py-3">Khách hàng</th><th class="px-5 py-3 text-right">Tổng SL</th><th class="px-5 py-3 text-right">Giá trị</th><th class="px-5 py-3 text-center">Trạng thái</th><th class="px-5 py-3 text-right">Thao tác</th></tr></thead>
          <tbody class="divide-y divide-gray-100">
            <tr v-if="filteredReceipts.length === 0"><td colspan="7" class="px-6 py-12 text-center text-gray-500">Kho này chưa xuất phiếu nào.</td></tr>
            <tr v-for="receipt in filteredReceipts" :key="receipt.id || receipt.Id" class="hover:bg-gray-50">
              <td class="px-5 py-3 font-bold text-amber-700">{{ receipt.code || receipt.Code }}</td><td class="px-5 py-3">{{ receipt.date || receipt.Date }}</td><td class="px-5 py-3 font-bold">{{ getCustomerName(receipt.customerId || receipt.CustomerId) }}</td><td class="px-5 py-3 text-right font-bold">{{ receipt.totalQty || receipt.TotalQty }}</td><td class="px-5 py-3 text-right font-bold text-emerald-600">{{ formatCurrency(receipt.totalPrice || receipt.TotalPrice) }}</td>
              <td class="px-5 py-3 text-center"><span :class="['px-2 py-1 rounded text-[10px] font-bold uppercase border', getStatusBadge(receipt.status || receipt.Status).class]">{{ getStatusBadge(receipt.status || receipt.Status).text }}</span></td>
              
              <td class="px-5 py-3 text-right space-x-1.5 whitespace-nowrap">
                <button @click="openModal('view', receipt)" class="p-1.5 text-blue-600 hover:bg-blue-50 rounded transition-colors" title="Xem chi tiết"><EyeIcon class="w-5 h-5" /></button>
                <button v-if="(receipt.status || receipt.Status) === 'pending'" @click="openModal('edit', receipt)" class="p-1.5 text-amber-600 hover:bg-amber-50 rounded transition-colors" title="Chỉnh sửa phiếu"><PencilSquareIcon class="w-5 h-5" /></button>
                <button v-if="(receipt.status || receipt.Status) === 'pending'" @click="handleDelete(receipt.id || receipt.Id, receipt.code || receipt.Code)" class="p-1.5 text-red-600 hover:bg-red-50 rounded transition-colors" title="Hủy phiếu"><TrashIcon class="w-5 h-5" /></button>
                
                <button v-if="(receipt.status || receipt.Status) === 'approved'" @click="handleCompleteReceipt(receipt)" class="p-1.5 text-purple-600 hover:bg-purple-50 rounded transition-colors" title="Đã xuất kho thực tế (Trừ tồn kho)">
                  <TruckIcon class="w-5 h-5" />
                </button>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>

    <Teleport to="body">
      <div v-if="showModal" class="fixed inset-0 z-50 flex items-center justify-center px-4 bg-slate-900/60 backdrop-blur-sm">
        <div class="bg-white rounded-xl shadow-2xl w-full max-w-[95vw] lg:max-w-6xl flex flex-col max-h-[90vh]">
          <div class="px-6 py-4 border-b flex justify-between bg-gray-50"><h3 class="font-bold text-lg"><DocumentArrowUpIcon class="w-6 h-6 inline text-amber-600"/> {{ modalMode === 'add' ? 'Lập Phiếu Xuất Kho' : 'Chi tiết Phiếu Xuất: ' + formData.code }}</h3><button @click="closeModal" class="text-gray-400 hover:text-red-500"><XMarkIcon class="w-6 h-6" /></button></div>
          <div class="p-6 overflow-y-auto flex-1 space-y-6">
            <div class="grid grid-cols-4 gap-4 bg-slate-50 p-4 rounded-lg border">
              <div class="col-span-2"><label class="block text-xs font-bold mb-1">Khách hàng *</label><select v-model="formData.customerId" :disabled="modalMode === 'view'" class="w-full border rounded px-3 py-2 text-sm outline-none focus:ring-amber-500"><option value="" disabled>-- Chọn Khách hàng --</option><option v-for="cus in customers" :key="cus.partnerId || cus.id" :value="cus.partnerId || cus.id">{{ cus.partnerName || cus.name }}</option></select></div>
              <div><label class="block text-xs font-bold mb-1">Ngày xuất</label><input v-model="formData.date" :disabled="modalMode==='view'" type="date" class="w-full border rounded px-3 py-2 text-sm outline-none focus:ring-amber-500"></div>
              <div><label class="block text-xs font-bold mb-1">Trạng thái</label><div class="px-3 py-2 text-sm font-bold rounded border text-center" :class="getStatusBadge(formData.status).class">{{ getStatusBadge(formData.status).text }}</div></div>
            </div>

            <div v-if="modalMode !== 'view'" class="bg-amber-50 p-4 rounded-xl border border-amber-100">
              <label class="text-sm font-bold text-amber-800 mb-2 block">Tra cứu & Chọn hàng TỪ TỒN KHO CỦA KHO NÀY:</label>
              <input v-model="stockSearchQuery" type="text" class="w-full border border-amber-200 rounded px-3 py-1.5 text-sm mb-2 outline-none focus:ring-1 focus:ring-amber-500 shadow-sm" placeholder="🔍 Gõ mã SKU, tên SP hoặc mã Kệ để tìm hàng đang có trong kho...">
              <div class="bg-white border border-amber-200 rounded max-h-32 overflow-y-auto p-2">
                <div v-if="filteredStockList.length === 0" class="p-2 text-sm text-gray-400 italic">Kho đang trống hoặc không tìm thấy.</div>
                <label v-for="stock in filteredStockList" :key="stock.stockId" class="flex items-center gap-3 p-2 hover:bg-amber-50 cursor-pointer border-b">
                  <input type="checkbox" :value="stock.stockId" v-model="selectedStockIdsToAdd" class="w-4 h-4 text-amber-600 rounded">
                  <span class="text-sm flex-1 font-medium">[{{ stock.sku }}] {{ stock.name }} <span class="text-indigo-600 font-bold ml-2">(Kệ: {{ stock.locationCode }})</span></span>
                  <span class="text-xs text-gray-500">NSX: {{stock.nsx || '--'}} | HSD: {{stock.hsd || '--'}}</span>
                  <span class="text-xs font-bold px-2 py-1 bg-blue-100 text-blue-700 rounded ml-2">Tồn: {{ stock.qtyAvailable }} {{ stock.unit }}</span>
                </label>
              </div>
              <button type="button" @click="handleAddMultipleStocks" class="mt-2 px-4 py-1.5 bg-amber-600 text-white rounded text-sm font-bold disabled:opacity-50" :disabled="selectedStockIdsToAdd.length === 0">Xác nhận lấy {{selectedStockIdsToAdd.length}} Lô xuống phiếu</button>
            </div>

            <div class="border rounded-lg overflow-x-auto shadow-sm">
              <table class="w-full text-sm text-left"><thead class="bg-gray-100 text-xs uppercase font-bold border-b"><tr>
                <th class="px-2 py-3">SKU</th>
                <th class="px-2 py-3 min-w-[150px]">Tên Hàng</th>
                <th class="px-2 py-3 w-28">Kệ / NSX-HSD</th>
                <th class="px-2 py-3 text-right">Tồn Lô</th>
                
                <th class="px-2 py-3 text-center bg-amber-50/50">Món/Thùng</th>
                <th class="px-2 py-3 text-center bg-amber-50/50">Số Thùng</th>
                <th class="px-2 py-3 text-right text-blue-700">Tổng SL (Cái)</th>
                
                <th class="px-2 py-3 text-right">Đơn Giá</th>
                <th class="px-2 py-3 text-right">Thành tiền</th>
                <th v-if="modalMode !== 'view'" class="px-2 py-3 text-center">#</th>
              </tr></thead>
              <tbody>
                <tr v-if="formData.items.length === 0"><td :colspan="modalMode !== 'view' ? 10 : 9" class="text-center py-6 text-gray-500">Chưa có hàng hóa xuất. Vui lòng chọn Tồn kho.</td></tr>
                <tr v-for="(item, idx) in formData.items" :key="idx" class="border-b hover:bg-gray-50">
                  <td class="px-2 py-2 font-bold">{{item.sku}}</td>
                  <td class="px-2 py-2">{{item.name}}</td>
                  <td class="px-2 py-2 text-xs">
                    <span class="font-bold text-indigo-600 block">{{item.locationCode}}</span>
                    <span class="text-gray-500 block">NSX: {{item.nsx || '--'}}</span>
                    <span class="text-gray-500 block">HSD: {{item.hsd || '--'}}</span>
                  </td>
                  <td class="px-2 py-2 text-right font-bold text-gray-400">{{item.maxQty}}</td>
                  
                  <td class="px-2 py-2 text-center bg-amber-50/20">
                    <input v-if="modalMode !== 'view'" v-model.number="item.conversionRate" @input="calculateQty(item)" type="number" min="1" class="w-16 text-center border border-amber-300 rounded px-1 py-1 text-sm outline-none focus:ring-1 focus:ring-amber-500" title="Bao nhiêu món 1 thùng?">
                    <span v-else class="text-sm font-medium">{{item.conversionRate || 1}}</span>
                  </td>
                  <td class="px-2 py-2 text-center bg-amber-50/20">
                    <input v-if="modalMode !== 'view'" v-model.number="item.boxQty" @input="calculateQty(item)" type="number" min="1" class="w-16 text-center border border-amber-300 rounded px-1 py-1 text-sm font-bold text-amber-700 outline-none focus:ring-1 focus:ring-amber-500" title="Xuất bao nhiêu thùng?">
                    <span v-else class="text-sm font-bold text-amber-700">{{item.boxQty || item.qty}}</span>
                  </td>
                  <td class="px-2 py-2 text-right">
                    <span class="font-bold text-blue-700 text-base">{{ item.qty }}</span>
                  </td>

                  <td class="px-2 py-2 text-right"><input v-if="modalMode !== 'view'" v-model.number="item.price" type="number" min="0" class="w-full text-right border rounded px-1 text-sm"><span v-else>{{formatCurrency(item.price)}}</span></td>
                  <td class="px-2 py-2 text-right font-bold text-emerald-600">{{ formatCurrency((item.qty || 0) * (item.price || 0)) }}</td>
                  <td v-if="modalMode !== 'view'" class="px-2 py-2 text-center"><button type="button" @click="removeItem(idx)" class="text-red-400 hover:text-red-600"><TrashIcon class="w-4 h-4 mx-auto"/></button></td>
                </tr>
              </tbody></table>
            </div>
            <div><label class="block text-xs font-bold mb-1">Ghi chú</label><input v-model="formData.note" :disabled="modalMode === 'view'" class="w-full border rounded px-3 py-2 text-sm outline-none focus:ring-amber-500"></div>
          </div>
          <div class="px-6 py-4 border-t flex justify-end gap-3 bg-gray-50">
            <button @click="closeModal" class="px-5 py-2.5 border rounded-lg font-bold hover:bg-gray-100 transition-colors">Đóng</button>
            <button v-if="modalMode !== 'view'" @click="handleSubmit" class="px-6 py-2.5 bg-amber-600 text-white rounded-lg font-bold hover:bg-amber-700 transition-colors shadow-md">Hoàn tất Phiếu Xuất</button>
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