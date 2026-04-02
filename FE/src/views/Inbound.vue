<script setup>
import { ref, computed, onMounted } from 'vue'
import { 
  MagnifyingGlassIcon, PlusIcon, EyeIcon, 
  XMarkIcon, DocumentArrowDownIcon, TrashIcon, PencilSquareIcon,
  PrinterIcon, DocumentTextIcon, ArrowDownTrayIcon, CheckCircleIcon
} from '@heroicons/vue/24/outline'

const INBOUND_API = 'https://localhost:7139/api/Inbound'
const PROD_API = 'https://localhost:7139/api/Products'
const PARTNER_API = 'https://localhost:7139/api/CrmPartners'
const LOC_API = 'https://localhost:7139/api/Locations'

const getAuthHeaders = () => ({ 'Content-Type': 'application/json', 'Authorization': 'Bearer ' + (localStorage.getItem('authToken') || '') })

const inboundReceipts = ref([]); const suppliers = ref([]); const productsList = ref([]); const locationsList = ref([]); const isLoading = ref(false)
const getToday = () => new Date().toISOString().split('T')[0]

const fetchData = async () => {
  isLoading.value = true
  try {
    const headers = getAuthHeaders()
    const [inbRes, prodRes, partRes, locRes] = await Promise.all([ fetch(INBOUND_API, { headers }), fetch(PROD_API, { headers }), fetch(PARTNER_API, { headers }), fetch(LOC_API, { headers }) ])
    if (prodRes.ok) productsList.value = await prodRes.json()
    if (locRes.ok) locationsList.value = await locRes.json()
    if (partRes.ok) {
      const pData = await partRes.json(); const rawPartners = pData.data || pData.Data || pData || []
      suppliers.value = rawPartners.filter(p => p.isSupplier || p.partnerCode?.startsWith('NCC'))
    }
    if (inbRes.ok) {
      const rawInbound = await inbRes.json()
      inboundReceipts.value = rawInbound.map(r => {
        const supId = r.supplierId || r.SupplierId;
        const sup = suppliers.value.find(s => s.partnerId === supId || s.id === supId);
        r.supplierName = sup ? (sup.partnerName || sup.name) : `Nhà cung cấp (ID: ${supId})`;
        if (!r.code && r.id) r.code = `PN${r.id.toString().padStart(4, '0')}`;
        return r;
      })
    }
  } catch (error) { console.error('Lỗi tải dữ liệu:', error) } finally { isLoading.value = false }
}

const getSupplierName = (id) => {
  const sup = suppliers.value.find(s => s.partnerId === id || s.id === id);
  return sup ? (sup.partnerName || sup.name) : `Chưa xác định (ID: ${id})`;
}

const searchQuery = ref(''); const filterStatus = ref('')
const filteredReceipts = computed(() => {
  return inboundReceipts.value.filter(r => {
    const code = r.code || r.Code || ''; const sName = getSupplierName(r.supplierId || r.SupplierId).toLowerCase(); const status = r.status || r.Status || '';
    const matchSearch = code.toLowerCase().includes(searchQuery.value.toLowerCase()) || sName.includes(searchQuery.value.toLowerCase())
    return matchSearch && (filterStatus.value === '' || status === filterStatus.value)
  })
})

const showModal = ref(false); const modalMode = ref('add') 
const formData = ref({ id: 0, code: '', date: getToday(), supplierId: '', items: [], note: '', status: 'pending' })

const openModal = (mode, receipt = null) => {
  modalMode.value = mode
  if (receipt) {
    const safeItems = (receipt.items || receipt.Items || []).map(i => {
      const variantId = i.variantId || i.VariantId;
      const prod = productsList.value.find(p => p.id === variantId || p.Id === variantId);
      return {
        variantId: variantId, sku: prod ? (prod.sku || prod.Sku) : (i.sku || i.Sku), name: prod ? (prod.name || prod.Name) : (i.name || i.Name), unit: prod ? (prod.unit || prod.Unit) : (i.unit || i.Unit), 
        qty: i.qty || i.Qty, price: i.price || i.Price, locationId: i.locationId || i.LocationId, locationCode: i.locationCode || i.LocationCode,
        nsx: i.nsx || i.Nsx ? (i.nsx || i.Nsx).split('T')[0] : '', hsd: i.hsd || i.Hsd ? (i.hsd || i.Hsd).split('T')[0] : ''
      };
    });
    formData.value = { id: receipt.id || receipt.Id, code: receipt.code || receipt.Code, date: receipt.date || receipt.Date, supplierId: receipt.supplierId || receipt.SupplierId, items: safeItems, note: receipt.note || receipt.Note, status: receipt.status || receipt.Status };
  } else { formData.value = { id: 0, code: '', date: getToday(), supplierId: '', items: [], note: '', status: 'pending' } }
  selectedSkusToAdd.value = []; productSearchQuery.value = ''; showModal.value = true
}
const closeModal = () => showModal.value = false

const getStatusBadge = (status) => {
  switch(status) {
    case 'pending': return { text: 'Chờ duyệt', class: 'bg-amber-100 text-amber-700 border-amber-200' }
    case 'approved': return { text: 'Đã duyệt (Chờ Kho)', class: 'bg-blue-100 text-blue-700 border-blue-200' }
    case 'completed': return { text: 'Đã nhập kho', class: 'bg-emerald-100 text-emerald-700 border-emerald-200' }
    case 'rejected': return { text: 'Từ chối', class: 'bg-red-100 text-red-700 border-red-200' }
    default: return { text: 'Khác', class: 'bg-gray-100 text-gray-500' }
  }
}

const getLocCurrentWeight = (loc) => {
  if (!loc.variantIds) return 0;
  return loc.variantIds.reduce((sum, vId) => {
    const p = productsList.value.find(x => x.id === vId || x.Id === vId);
    return sum + (p && p.weight ? p.weight : 0);
  }, 0);
}
const getFormattedLocOption = (loc) => {
  const currentW = getLocCurrentWeight(loc);
  const maxW = loc.maxWeight || loc.MaxWeight || 500;
  const isFull = currentW >= maxW;
  return `[${isFull ? 'QUÁ TẢI' : ((loc.variantIds||[]).length > 0 ? 'Đang chứa' : 'Trống')}] ${loc.code || loc.Code} - ${currentW}/${maxW}kg`;
}

const productSearchQuery = ref(''); const selectedSkusToAdd = ref([]) 
const filteredProductsList = computed(() => {
  if (!productSearchQuery.value) return productsList.value;
  return productsList.value.filter(p => (p.sku||p.Sku||'').toLowerCase().includes(productSearchQuery.value.toLowerCase()) || (p.name||p.Name||'').toLowerCase().includes(productSearchQuery.value.toLowerCase()));
})

const handleAddMultipleItems = () => {
  if (selectedSkusToAdd.value.length === 0) return;
  selectedSkusToAdd.value.forEach(id => {
    const prod = productsList.value.find(p => p.id === id || p.Id === id)
    if (prod) formData.value.items.push({ variantId: prod.id || prod.Id, sku: prod.sku || prod.Sku, name: prod.name || prod.Name, unit: prod.unit || prod.Unit, qty: 1, price: prod.price || prod.Price || 0, locationId: null, locationCode: '', nsx: '', hsd: '' })
  })
  selectedSkusToAdd.value = []; productSearchQuery.value = '' 
}
const updateLocationName = (item) => { const loc = locationsList.value.find(l => l.id === item.locationId || l.Id === item.locationId); item.locationCode = loc ? (loc.code || loc.Code) : ''; }
const removeItem = (index) => formData.value.items.splice(index, 1)

const totalQty = computed(() => formData.value.items.reduce((sum, item) => sum + (item.qty || 0), 0))
const totalPrice = computed(() => formData.value.items.reduce((sum, item) => sum + ((item.qty || 0) * (item.price || 0)), 0))
const formatCurrency = (val) => new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(val || 0)

const handleSubmit = async () => {
  if (formData.value.items.length === 0) return alert('Chưa chọn mặt hàng!')
  if (!formData.value.supplierId) return alert('Chưa chọn Nhà cung cấp!')
  for (const item of formData.value.items) {
    if (item.nsx && item.hsd && new Date(item.hsd) <= new Date(item.nsx)) {
      return alert(`[LỖI NGHIỆP VỤ] Sản phẩm ${item.sku} có HSD phải SAU Ngày Sản Xuất!`);
    }
  }
  try {
    const method = modalMode.value === 'add' ? 'POST' : 'PUT'
    const url = modalMode.value === 'add' ? INBOUND_API : `${INBOUND_API}/${formData.value.id}`
    const payload = { ...formData.value }; if (modalMode.value === 'add') payload.code = ""; 
    const res = await fetch(url, { method, headers: getAuthHeaders(), body: JSON.stringify(payload) })
    if (res.ok) { alert('Lưu thành công!'); await fetchData(); closeModal(); } 
    else { let errMsg = "Lỗi hệ thống!"; try { const text = await res.text(); if (text) errMsg = JSON.parse(text).message || errMsg; } catch(e) {} alert('LỖI: ' + errMsg); }
  } catch(e) { console.error(e) }
}

const handleDelete = async (id, code) => {
  if (!confirm(`Xóa phiếu [${code}]?`)) return;
  const res = await fetch(`${INBOUND_API}/${id}`, { method: 'DELETE', headers: getAuthHeaders() })
  if (res.ok) { await fetchData(); alert('Đã xóa!'); }
}

const handleCompleteReceipt = async (receipt) => {
  if (!confirm(`Xác nhận phiếu [${receipt.code || receipt.Code}] ĐÃ NHẬP KHO THÀNH CÔNG? (Hàng sẽ được ghi vào DB Tồn Kho)`)) return;
  try {
    const res = await fetch(`${INBOUND_API}/${receipt.id || receipt.Id}/complete`, { method: 'PUT', headers: getAuthHeaders() })
    if (res.ok) { 
      alert('Nhập kho thành công! Hàng đã lên kệ.'); 
      await fetchData(); 
    } else { 
      let errMsg = "Lỗi hệ thống!"; try { const t = await res.text(); if(t) errMsg = JSON.parse(t).message || errMsg; } catch(e){} alert('LỖI: ' + errMsg); 
    }
  } catch(e) { console.error(e) }
}

onMounted(() => fetchData())
</script>

<template>
  <div class="space-y-6 animate-fade-in pb-10 px-1 relative">
    <div class="flex justify-between items-center">
      <div><h2 class="text-2xl font-bold text-gray-800">Phiếu Nhập kho (Inbound)</h2></div>
      
      <div class="flex gap-2">
        <button @click="() => alert('Đang xuất báo cáo Excel...')" class="bg-white border border-gray-200 text-emerald-600 px-3 py-2 rounded-lg text-sm font-semibold hover:bg-emerald-50 transition-colors shadow-sm flex items-center gap-1.5">
          <DocumentTextIcon class="w-4 h-4"/> Xuất Excel
        </button>
        <button @click="() => alert('Đang xuất file PDF...')" class="bg-white border border-gray-200 text-red-600 px-3 py-2 rounded-lg text-sm font-semibold hover:bg-red-50 transition-colors shadow-sm flex items-center gap-1.5">
          <ArrowDownTrayIcon class="w-4 h-4"/> Xuất PDF
        </button>
        <button @click="openModal('add')" class="bg-primary-600 hover:bg-primary-700 text-white px-4 py-2 rounded-lg flex items-center gap-2 text-sm font-semibold shadow-sm"><PlusIcon class="w-5 h-5" /> Lập Phiếu Nhập</button>
      </div>
    </div>

    <div class="bg-white p-4 rounded-xl border flex gap-4 shadow-sm">
      <div class="relative flex-1"><div class="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none"><MagnifyingGlassIcon class="w-5 h-5 text-gray-400" /></div><input v-model="searchQuery" type="text" class="block w-full pl-10 pr-3 py-2 border rounded-lg text-sm outline-none focus:ring-1 focus:ring-primary-500" placeholder="Tìm theo mã phiếu, tên NCC..."></div>
      <select v-model="filterStatus" class="border rounded-lg text-sm px-4 py-2 outline-none focus:ring-1 focus:ring-primary-500 cursor-pointer"><option value="">Tất cả Trạng thái</option><option value="pending">Chờ duyệt</option><option value="approved">Đã duyệt (Chờ kho)</option><option value="completed">Đã nhập kho</option><option value="rejected">Bị từ chối</option></select>
    </div>

    <div class="bg-white rounded-xl border shadow-sm overflow-hidden">
      <div class="w-full overflow-x-auto">
        <table class="w-full text-sm text-left"><thead class="bg-gray-50 uppercase font-bold text-gray-500 text-xs"><tr><th class="px-5 py-3">Mã Phiếu</th><th class="px-5 py-3">Ngày</th><th class="px-5 py-3">Nhà cung cấp</th><th class="px-5 py-3 text-right">Tổng SL</th><th class="px-5 py-3 text-right">Giá trị</th><th class="px-5 py-3 text-center">Trạng thái</th><th class="px-5 py-3 text-right">Thao tác</th></tr></thead>
          <tbody class="divide-y divide-gray-100">
            <tr v-for="receipt in filteredReceipts" :key="receipt.id || receipt.Id" class="hover:bg-gray-50">
              <td class="px-5 py-3 font-bold text-primary-700">{{ receipt.code || receipt.Code }}</td><td class="px-5 py-3">{{ receipt.date || receipt.Date }}</td><td class="px-5 py-3 font-bold">{{ getSupplierName(receipt.supplierId || receipt.SupplierId) }}</td><td class="px-5 py-3 text-right font-bold">{{ receipt.totalQty || receipt.TotalQty }}</td><td class="px-5 py-3 text-right font-bold text-emerald-600">{{ formatCurrency(receipt.totalPrice || receipt.TotalPrice) }}</td>
              <td class="px-5 py-3 text-center"><span :class="['px-2 py-1 rounded text-[10px] font-bold uppercase border', getStatusBadge(receipt.status || receipt.Status).class]">{{ getStatusBadge(receipt.status || receipt.Status).text }}</span></td>
              
              <td class="px-5 py-3 text-right whitespace-nowrap space-x-1.5">
                <button @click="openModal('view', receipt)" class="p-1.5 text-blue-600 hover:bg-blue-50 rounded transition-colors" title="Xem chi tiết"><EyeIcon class="w-5 h-5" /></button>
                <button v-if="(receipt.status || receipt.Status) === 'pending'" @click="openModal('edit', receipt)" class="p-1.5 text-amber-600 hover:bg-amber-50 rounded transition-colors" title="Chỉnh sửa phiếu"><PencilSquareIcon class="w-5 h-5" /></button>
                <button v-if="(receipt.status || receipt.Status) === 'pending'" @click="handleDelete(receipt.id || receipt.Id, receipt.code || receipt.Code)" class="p-1.5 text-red-600 hover:bg-red-50 rounded transition-colors" title="Hủy phiếu"><TrashIcon class="w-5 h-5" /></button>
                <button v-if="(receipt.status || receipt.Status) === 'approved'" @click="handleCompleteReceipt(receipt)" class="p-1.5 text-emerald-600 hover:bg-emerald-50 rounded transition-colors" title="Xác nhận hàng đã nhập vào kho">
                  <CheckCircleIcon class="w-5 h-5" />
                </button>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>

    <Teleport to="body">
      <div v-if="showModal" class="fixed inset-0 z-50 flex items-center justify-center px-4 bg-slate-900/60 backdrop-blur-sm">
        <div class="bg-white rounded-xl shadow-2xl w-full max-w-6xl flex flex-col max-h-[90vh]">
          <div class="px-6 py-4 border-b flex justify-between bg-gray-50"><h3 class="font-bold text-lg"><DocumentArrowDownIcon class="w-6 h-6 inline text-primary-600"/> {{ modalMode === 'add' ? 'Lập Phiếu Nhập Kho' : 'Chi tiết Phiếu: ' + formData.code }}</h3><button @click="closeModal" class="text-gray-400 hover:text-red-500"><XMarkIcon class="w-6 h-6" /></button></div>
          <div class="p-6 overflow-y-auto flex-1 space-y-6">
            <div class="grid grid-cols-4 gap-4 bg-slate-50 p-4 rounded-lg border">
              <div class="col-span-2"><label class="block text-xs font-bold mb-1">Nhà cung cấp *</label><select v-model="formData.supplierId" :disabled="modalMode === 'view'" class="w-full border rounded px-3 py-2 text-sm outline-none"><option value="" disabled>-- Chọn Nhà cung cấp --</option><option v-for="ncc in suppliers" :key="ncc.partnerId || ncc.id" :value="ncc.partnerId || ncc.id">{{ ncc.partnerName || ncc.name }}</option></select></div>
              <div><label class="block text-xs font-bold mb-1">Ngày lập</label><input v-model="formData.date" :disabled="modalMode==='view'" type="date" class="w-full border rounded px-3 py-2 text-sm outline-none"></div>
              <div><label class="block text-xs font-bold mb-1">Trạng thái</label><div class="px-3 py-2 text-sm font-bold rounded border text-center" :class="getStatusBadge(formData.status).class">{{ getStatusBadge(formData.status).text }}</div></div>
            </div>

            <div v-if="modalMode !== 'view'" class="bg-blue-50 p-4 rounded-xl border border-blue-100">
              <label class="text-sm font-bold text-blue-800 mb-2 block">Tìm & Chọn Hàng hóa:</label>
              <input v-model="productSearchQuery" type="text" class="w-full border border-blue-200 rounded px-3 py-1.5 text-sm mb-2 outline-none" placeholder="🔍 Gõ tên hoặc mã SKU...">
              <div class="bg-white border border-blue-200 rounded max-h-32 overflow-y-auto p-2">
                <label v-for="prod in filteredProductsList" :key="prod.id || prod.Id" class="flex items-center gap-3 p-2 hover:bg-blue-50 cursor-pointer border-b">
                  <input type="checkbox" :value="prod.id || prod.Id" v-model="selectedSkusToAdd" class="w-4 h-4 text-blue-600 rounded">
                  <span class="text-sm flex-1">[{{ prod.sku || prod.Sku }}] {{ prod.name || prod.Name }}</span>
                </label>
              </div>
              <button type="button" @click="handleAddMultipleItems" class="mt-2 px-4 py-1.5 bg-blue-600 text-white rounded text-sm font-bold disabled:opacity-50" :disabled="selectedSkusToAdd.length === 0">Đưa {{selectedSkusToAdd.length}} SP xuống lưới</button>
            </div>

            <div class="border rounded-lg overflow-x-auto">
              <table class="w-full text-sm text-left"><thead class="bg-gray-100 text-xs uppercase font-bold border-b"><tr>
                <th class="px-2 py-3">SKU</th><th class="px-2 py-3">Tên Hàng</th><th class="px-2 py-3 w-40">Vị trí (Kệ)</th><th class="px-2 py-3 w-28">NSX</th><th class="px-2 py-3 w-28">HSD</th><th class="px-2 py-3 text-right">SL</th><th class="px-2 py-3 text-right">Giá</th><th class="px-2 py-3 text-right">Thành tiền</th><th v-if="modalMode !== 'view'" class="px-2 py-3 text-center">#</th>
              </tr></thead>
              <tbody>
                <tr v-if="formData.items.length === 0"><td :colspan="modalMode !== 'view' ? 9 : 8" class="text-center py-6 text-gray-500">Chưa có hàng hóa.</td></tr>
                <tr v-for="(item, idx) in formData.items" :key="idx" class="border-b hover:bg-gray-50">
                  <td class="px-2 py-2 font-bold">{{item.sku}}</td><td class="px-2 py-2">{{item.name}}</td>
                  <td class="px-2 py-2"><select v-if="modalMode !== 'view'" v-model="item.locationId" @change="updateLocationName(item)" class="w-full border rounded px-1 py-1 text-xs"><option :value="null">-- Khu nhận --</option><option v-for="loc in locationsList" :key="loc.id || loc.Id" :value="loc.id || loc.Id">{{loc.code||loc.Code}}</option></select><span v-else>{{item.locationCode || 'Khu nhận'}}</span></td>
                  <td class="px-2 py-2"><input v-if="modalMode !== 'view'" v-model="item.nsx" type="date" class="w-full border rounded px-1 py-1 text-xs"><span v-else class="text-xs">{{item.nsx}}</span></td>
                  <td class="px-2 py-2"><input v-if="modalMode !== 'view'" v-model="item.hsd" type="date" class="w-full border rounded px-1 py-1 text-xs"><span v-else class="text-xs">{{item.hsd}}</span></td>
                  <td class="px-2 py-2 text-right"><input v-if="modalMode !== 'view'" v-model.number="item.qty" type="number" min="1" class="w-full text-right border rounded px-1"><span v-else class="font-bold text-blue-700">{{item.qty}}</span></td>
                  <td class="px-2 py-2 text-right"><input v-if="modalMode !== 'view'" v-model.number="item.price" type="number" min="0" class="w-full text-right border rounded px-1"><span v-else>{{formatCurrency(item.price)}}</span></td>
                  <td class="px-2 py-2 text-right font-bold text-emerald-600">{{ formatCurrency((item.qty || 0) * (item.price || 0)) }}</td>
                  <td v-if="modalMode !== 'view'" class="px-2 py-2 text-center"><button type="button" @click="removeItem(idx)" class="text-red-400 hover:text-red-600"><TrashIcon class="w-4 h-4 mx-auto"/></button></td>
                </tr>
              </tbody></table>
            </div>
            <div><label class="block text-xs font-bold mb-1">Ghi chú</label><input v-model="formData.note" :disabled="modalMode === 'view'" class="w-full border rounded px-3 py-2 text-sm"></div>
          </div>
          <div class="px-6 py-4 border-t flex justify-end gap-3 bg-gray-50">
            <button @click="closeModal" class="px-5 py-2.5 border rounded-lg font-bold hover:bg-gray-100 transition-colors">Đóng</button>
            <button v-if="modalMode !== 'view'" @click="handleSubmit" class="px-6 py-2.5 bg-primary-600 text-white rounded-lg font-bold hover:bg-primary-700 transition-colors shadow-md">Hoàn tất Phiếu</button>
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