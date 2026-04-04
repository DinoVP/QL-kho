<script setup>
import { ref, computed, onMounted } from 'vue'
import { 
  MagnifyingGlassIcon, PlusIcon, EyeIcon, 
  XMarkIcon, ShoppingCartIcon, TrashIcon,
  PrinterIcon, CheckCircleIcon, PencilSquareIcon,
  ArrowDownTrayIcon, DocumentTextIcon, EnvelopeIcon,
  PaperAirplaneIcon, ArrowRightOnRectangleIcon
} from '@heroicons/vue/24/outline'

const API_URL = 'https://localhost:7139/api/Po'
const PARTNER_API = 'https://localhost:7139/api/CrmPartners'
const PROD_API = 'https://localhost:7139/api/Products'
const INBOUND_API = 'https://localhost:7139/api/Inbound'

const getAuthHeaders = () => ({ 'Content-Type': 'application/json', 'Authorization': 'Bearer ' + (localStorage.getItem('authToken') || '') })

const currentUserRole = ref(localStorage.getItem('role') || 'ql_kho') 
const myWarehouseId = ref(parseInt(localStorage.getItem('warehouseId')) || null)

const canApprovePO = computed(() => ['admin', 'giam_doc', 'gd_chi_nhanh'].includes(currentUserRole.value.toLowerCase()))
const canExport = computed(() => ['admin', 'giam_doc', 'gd_chi_nhanh', 'ql_kho'].includes(currentUserRole.value.toLowerCase()))

const purchaseOrders = ref([])
const suppliersList = ref([])
const productsList = ref([])
const isLoading = ref(false)

const getToday = () => new Date().toISOString().split('T')[0]

const fetchData = async () => {
  isLoading.value = true
  try {
    const headers = getAuthHeaders()
    const [poRes, partnerRes, prodRes] = await Promise.all([
      fetch(API_URL, { headers }), fetch(PARTNER_API, { headers }), fetch(PROD_API, { headers })
    ])
    
    if (prodRes.ok) productsList.value = await prodRes.json()
    if (partnerRes.ok) {
        const pData = await partnerRes.json()
        const rawPartners = pData.data || pData.Data || pData || []
        suppliersList.value = rawPartners.filter(p => p.isSupplier || p.partnerCode?.startsWith('NC'))
    }
    
    if (poRes.ok) {
        const rawPOs = await poRes.json()
        purchaseOrders.value = rawPOs.map(r => {
            const ncc = suppliersList.value.find(p => p.partnerId === r.supplierId || p.id === r.supplierId || p.Id === r.supplierId);
            return {
                ...r,
                supplierName: ncc ? (ncc.partnerName || ncc.name || ncc.Name) : 'Nhà cung cấp không xác định'
            }
        })
    }
  } catch (error) { console.error(error) } finally { isLoading.value = false }
}

const searchQuery = ref('')
const filterStatus = ref('')

const filteredPOs = computed(() => {
  return purchaseOrders.value.filter(po => {
    const matchSearch = (po.code||'').toLowerCase().includes(searchQuery.value.toLowerCase()) || 
                        (po.supplierName||'').toLowerCase().includes(searchQuery.value.toLowerCase())
    const matchStatus = filterStatus.value === '' || po.status === filterStatus.value
    return matchSearch && matchStatus
  })
})

const showModal = ref(false)
const modalMode = ref('add') 
const formData = ref({ id: 0, code: '', date: getToday(), expectedDate: getToday(), supplierId: '', items: [], note: '', status: 'draft' })

const openModal = (mode, po = null) => {
  modalMode.value = mode
  if (po) {
      const mappedItems = (po.items || []).map(i => {
          const prod = productsList.value.find(p => p.id === i.variantId || p.Id === i.variantId) || {};
          return {
              ...i,
              sku: prod.sku || prod.Sku || 'N/A',
              name: prod.name || prod.Name || 'Sản phẩm',
              unit: prod.unit || prod.Unit || 'Cái'
          }
      })
      formData.value = { ...po, items: mappedItems }
  } 
  else formData.value = { id: 0, code: '', date: getToday(), expectedDate: getToday(), supplierId: '', items: [], note: '', status: 'draft' }
  selectedVariantIdToAdd.value = ''
  showModal.value = true
}

const closeModal = () => showModal.value = false

const getStatusBadge = (status) => {
  switch(status) {
    case 'draft': return { text: 'Bản nháp', class: 'bg-gray-100 text-gray-700 border-gray-300' }
    case 'pending': return { text: 'Chờ duyệt', class: 'bg-amber-100 text-amber-700 border-amber-300' }
    case 'approved': return { text: 'Đã duyệt', class: 'bg-blue-100 text-blue-700 border-blue-300' }
    case 'sent': return { text: 'Đã gửi NCC', class: 'bg-purple-100 text-purple-700 border-purple-300' }
    case 'completed': return { text: 'Đã tạo Phiếu Nhập', class: 'bg-emerald-100 text-emerald-700 border-emerald-300' }
    default: return { text: 'Khác', class: 'bg-gray-100 text-gray-500' }
  }
}

// === LOGIC CHỌN HÀNG ===
const selectedVariantIdToAdd = ref('')

const handleAddItem = () => {
  if (!selectedVariantIdToAdd.value) return
  const existingItem = formData.value.items.find(i => i.variantId === selectedVariantIdToAdd.value)
  if (existingItem) existingItem.qty += 1
  else {
    const prod = productsList.value.find(p => p.id === selectedVariantIdToAdd.value || p.Id === selectedVariantIdToAdd.value)
    if (prod) {
        formData.value.items.push({ 
            variantId: prod.id || prod.Id, sku: prod.sku || prod.Sku, name: prod.name || prod.Name, 
            unit: prod.unit || prod.Unit, qty: 1 
        })
    }
  }
  selectedVariantIdToAdd.value = '' 
}

const removeItem = (index) => formData.value.items.splice(index, 1)

const totalQty = computed(() => formData.value.items.reduce((sum, item) => sum + (item.qty || 0), 0))

const handleSubmit = async () => {
  if (!formData.value.supplierId) return alert('Vui lòng chọn Nhà cung cấp!')
  if (formData.value.items.length === 0) return alert('Chưa chọn mặt hàng nào để đặt mua!')
  
  try {
      const payload = { ...formData.value, code: formData.value.code || "" };
      const method = modalMode.value === 'add' ? 'POST' : 'PUT';
      const url = modalMode.value === 'add' ? API_URL : `${API_URL}/${formData.value.id}`;

      const res = await fetch(url, { method, headers: getAuthHeaders(), body: JSON.stringify(payload) })
      if (res.ok) { 
          alert(modalMode.value === 'add' ? 'Tạo Đơn Đặt Hàng thành công!' : 'Cập nhật thành công!'); 
          await fetchData(); 
          closeModal(); 
      } else { 
          let errMsg = "Lỗi hệ thống!"; try { const t = await res.text(); if(t) errMsg = JSON.parse(t).message || errMsg; } catch(e){} alert('LỖI: ' + errMsg); 
      }
  } catch(e) { console.error(e) }
}

const handleApprovePO = async () => {
  if(!confirm(`Xác nhận DUYỆT đơn đặt hàng ${formData.value.code}?`)) return;
  try {
      const res = await fetch(`${API_URL}/${formData.value.id}/approve`, { method: 'PUT', headers: getAuthHeaders() })
      if(res.ok) { alert('Đã duyệt thành công!'); await fetchData(); closeModal(); }
  } catch(e) { console.error(e) }
}

const handleSendEmail = async () => {
  if(!confirm(`Gửi Email thông báo đặt hàng cho Nhà Cung Cấp?`)) return;
  try {
      const res = await fetch(`${API_URL}/${formData.value.id}/send`, { method: 'PUT', headers: getAuthHeaders() })
      if(res.ok) { alert('Đã đánh dấu Gửi Email!'); await fetchData(); closeModal(); }
  } catch(e) { console.error(e) }
}

// === TÍNH NĂNG XÓA THEO YÊU CẦU SẾP ===
const handleDeletePO = async (po) => {
    if(!confirm(`BẠN CÓ CHẮC CHẮN MUỐN XÓA ĐƠN ĐẶT HÀNG [${po.code}] KHÔNG?\nHành động này không thể hoàn tác!`)) return;
    try {
        const res = await fetch(`${API_URL}/${po.id}`, { method: 'DELETE', headers: getAuthHeaders() })
        if(res.ok) { alert('Đã xóa thành công!'); await fetchData(); }
        else { let errMsg = "Lỗi hệ thống!"; try { const t = await res.text(); if(t) errMsg = JSON.parse(t).message || errMsg; } catch(e){} alert('LỖI: ' + errMsg); }
    } catch(e) { console.error(e) }
}

const handleCreateInbound = async (po) => {
    if(!confirm(`Hàng đã về? Tạo tự động Phiếu Nhập Kho từ Đơn Đặt Hàng [${po.code}] này?`)) return;
    try {
        const inboundPayload = {
            date: getToday(),
            supplierId: po.supplierId,
            warehouseId: myWarehouseId.value || 1,
            note: `Tạo tự động từ Đơn Đặt Hàng: ${po.code}`,
            status: 'pending',
            items: po.items.map(i => ({ variantId: i.variantId, qty: i.qty, price: 0, locationId: null }))
        };

        const res = await fetch(INBOUND_API, { method: 'POST', headers: getAuthHeaders(), body: JSON.stringify(inboundPayload) })
        if (res.ok) { 
            await fetch(`${API_URL}/${po.id}/complete`, { method: 'PUT', headers: getAuthHeaders() });
            alert('Đã tạo Phiếu Nhập Kho chờ duyệt! Kiểm tra ở màn hình Phiếu Nhập.'); 
            await fetchData(); 
        } 
        else { let errMsg = "Lỗi hệ thống!"; try { const t = await res.text(); if(t) errMsg = JSON.parse(t).message || errMsg; } catch(e){} alert('LỖI: ' + errMsg); }
    } catch(e) { console.error(e) }
}

onMounted(() => fetchData())
</script>

<template>
  <div class="space-y-5 md:space-y-6 animate-fade-in pb-10 px-0 md:px-1 relative">
    
    <div class="flex flex-col sm:flex-row sm:items-center justify-between gap-4">
      <div>
        <h2 class="text-xl md:text-2xl font-bold text-gray-800">Đơn Đặt Hàng</h2>
        <p class="text-xs md:text-sm text-gray-500 mt-1">Lập danh sách vật tư/hàng hóa cần mua từ Nhà Cung Cấp</p>
      </div>
      <div class="flex flex-wrap items-center gap-2">
        <button v-if="canExport" @click="() => alert('Tính năng đang phát triển...')" class="bg-white border border-emerald-200 text-emerald-700 px-3 py-2.5 rounded-lg text-sm font-semibold hover:bg-emerald-50 transition-colors shadow-sm flex items-center gap-1.5"><DocumentTextIcon class="w-4 h-4"/> Xuất Excel</button>
        <button @click="openModal('add')" class="bg-indigo-600 hover:bg-indigo-700 text-white px-4 py-2.5 rounded-lg flex items-center gap-2 text-sm font-semibold transition-colors shadow-sm"><PlusIcon class="w-5 h-5" /> Lập Đơn Mua Hàng</button>
      </div>
    </div>

    <div class="bg-white p-3 md:p-4 rounded-xl border border-gray-200 flex flex-col sm:flex-row items-center gap-3 md:gap-4 shadow-sm">
      <div class="relative w-full sm:flex-1">
        <div class="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none"><MagnifyingGlassIcon class="w-5 h-5 text-gray-400" /></div>
        <input v-model="searchQuery" type="text" class="block w-full pl-10 pr-3 py-2 border border-gray-200 rounded-lg text-sm outline-none focus:ring-1 focus:ring-indigo-500" placeholder="Tìm theo mã PO, tên NCC...">
      </div>
      <select v-model="filterStatus" class="w-full sm:w-auto border border-gray-200 rounded-lg text-sm px-4 py-2 outline-none focus:ring-1 focus:ring-indigo-500 cursor-pointer">
        <option value="">Tất cả Trạng thái</option>
        <option value="draft">Bản nháp</option><option value="pending">Chờ duyệt</option>
        <option value="approved">Đã duyệt</option><option value="sent">Đã gửi NCC</option><option value="completed">Đã tạo Phiếu Nhập</option>
      </select>
    </div>

    <div class="bg-white rounded-xl border border-gray-200 shadow-sm overflow-hidden">
      <div class="w-full overflow-x-auto custom-scrollbar">
        <table class="min-w-[1000px] w-full divide-y divide-gray-200">
          <thead class="bg-gray-50">
            <tr>
              <th class="px-5 py-3.5 text-left text-xs font-bold text-gray-500 uppercase tracking-wider">Mã PO</th>
              <th class="px-5 py-3.5 text-left text-xs font-bold text-gray-500 uppercase tracking-wider">Ngày đặt</th>
              <th class="px-5 py-3.5 text-left text-xs font-bold text-gray-500 uppercase tracking-wider">Nhà cung cấp</th>
              <th class="px-5 py-3.5 text-right text-xs font-bold text-gray-500 uppercase tracking-wider">Tổng SL Hàng</th>
              <th class="px-5 py-3.5 text-center text-xs font-bold text-gray-500 uppercase tracking-wider">Trạng thái</th>
              <th class="px-5 py-3.5 text-right text-xs font-bold text-gray-500 uppercase tracking-wider">Thao tác</th>
            </tr>
          </thead>
          <tbody class="divide-y divide-gray-100 bg-white">
            <tr v-if="filteredPOs.length === 0">
              <td colspan="6" class="px-6 py-16 text-center">
                <ShoppingCartIcon class="w-12 h-12 text-gray-300 mx-auto mb-3" />
                <h3 class="text-base font-semibold text-gray-700">Chưa có Đơn Đặt Hàng nào</h3>
              </td>
            </tr>
            <tr v-for="po in filteredPOs" :key="po.id" class="hover:bg-gray-50 transition-colors">
              <td class="px-5 py-3 text-sm font-bold text-indigo-700">{{ po.code }}</td>
              <td class="px-5 py-3 text-sm font-medium text-gray-600">Lập: {{ po.date }}<br><span class="text-xs text-gray-400">Giao: {{ po.expectedDate }}</span></td>
              <td class="px-5 py-3 text-sm font-bold text-gray-800">{{ po.supplierName }}</td>
              <td class="px-5 py-3 text-sm text-right font-bold text-indigo-700">{{ po.totalQty }}</td>
              <td class="px-5 py-3 text-center"><span :class="['text-[10px] font-bold px-2 py-1 rounded border uppercase tracking-wider', getStatusBadge(po.status).class]">{{ getStatusBadge(po.status).text }}</span></td>
              
              <td class="px-5 py-3 text-right whitespace-nowrap">
                <div class="flex items-center justify-end gap-1.5">
                  <button v-if="po.status === 'approved' || po.status === 'sent'" @click.stop="() => alert('Đang code...')" class="p-1.5 text-blue-600 hover:bg-blue-50 rounded border border-transparent hover:border-blue-100" title="Gửi Email PDF cho NCC"><EnvelopeIcon class="w-4 h-4" /></button>
                  <button @click.stop="() => alert('Đang code...')" class="p-1.5 text-gray-600 hover:bg-gray-100 rounded border border-transparent hover:border-gray-200" title="In trực tiếp"><PrinterIcon class="w-4 h-4" /></button>
                  
                  <button v-if="(po.status === 'approved' || po.status === 'sent') && canApprovePO" @click="handleCreateInbound(po)" class="px-2 py-1.5 text-emerald-600 hover:bg-emerald-700 hover:text-white rounded bg-emerald-50 border border-emerald-200 font-bold text-xs flex items-center gap-1 transition-colors" title="Nhập Kho khi hàng về">
                      <ArrowRightOnRectangleIcon class="w-4 h-4" /> Nhập Kho
                  </button>

                  <button v-if="po.status === 'draft' || po.status === 'pending'" @click="openModal('edit', po)" class="p-1.5 text-amber-600 hover:bg-amber-50 rounded" title="Sửa Đơn"><PencilSquareIcon class="w-4 h-4" /></button>
                  
                  <button v-if="po.status === 'draft' || po.status === 'pending'" @click="handleDeletePO(po)" class="p-1.5 text-red-500 hover:bg-red-50 hover:text-red-700 rounded" title="Xóa Đơn"><TrashIcon class="w-4 h-4" /></button>

                  <button @click="openModal('view', po)" class="px-2 py-1.5 text-indigo-600 hover:bg-indigo-50 rounded bg-indigo-50 border border-indigo-100 font-medium text-xs flex items-center gap-1"><EyeIcon class="w-4 h-4" /> Chi tiết</button>
                </div>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>

    <Teleport to="body">
      <div v-if="showModal" class="fixed inset-0 z-50 flex items-center justify-center px-4 bg-slate-900/50 backdrop-blur-sm animate-fade-in">
        <div class="bg-white rounded-xl shadow-2xl w-full max-w-4xl overflow-hidden flex flex-col max-h-[90vh]">
          
          <div class="px-6 py-4 border-b flex items-center justify-between bg-indigo-50 shrink-0">
            <h3 class="text-lg font-bold text-indigo-800 flex items-center gap-2"><ShoppingCartIcon class="w-6 h-6 text-indigo-600"/> {{ modalMode === 'add' ? 'Lập Đơn Đặt Hàng Mới' : `Chi tiết PO: ${formData.code}` }}</h3>
            <button @click="closeModal" class="text-gray-400 hover:text-red-500"><XMarkIcon class="w-6 h-6" /></button>
          </div>
          
          <div class="p-6 overflow-y-auto flex-1 custom-scrollbar">
            <form id="poForm" @submit.prevent="handleSubmit" class="space-y-6">
              
              <div class="bg-slate-50 p-4 rounded-lg border border-slate-200">
                <div class="grid grid-cols-1 md:grid-cols-4 gap-4 mb-4">
                  <div><label class="block text-xs font-bold mb-1">Mã PO</label><input type="text" disabled class="w-full border rounded-lg px-3 py-2 text-sm bg-gray-100 italic" :placeholder="modalMode === 'add' ? 'Hệ thống tự sinh' : formData.code"></div>
                  <div class="md:col-span-2">
                    <label class="block text-xs font-bold mb-1">Nhà cung cấp <span class="text-red-500">*</span></label>
                    <select v-model="formData.supplierId" :disabled="modalMode === 'view'" required class="w-full border rounded-lg px-3 py-2 text-sm focus:ring-indigo-500 disabled:bg-gray-100 cursor-pointer">
                      <option value="" disabled>-- Chọn Nhà cung cấp --</option>
                      <option v-for="ncc in suppliersList" :key="ncc.partnerId||ncc.id||ncc.Id" :value="ncc.partnerId||ncc.id||ncc.Id">[{{ ncc.partnerCode||ncc.code }}] {{ ncc.partnerName||ncc.name||ncc.Name }}</option>
                    </select>
                  </div>
                  <div>
                    <label class="block text-xs font-bold mb-1">Trạng thái</label>
                    <div class="w-full border rounded-lg px-3 py-2 text-sm font-semibold text-center" :class="getStatusBadge(formData.status).class">{{ getStatusBadge(formData.status).text }}</div>
                  </div>
                </div>
                <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
                  <div><label class="block text-xs font-bold mb-1">Ngày lập phiếu <span class="text-red-500">*</span></label><input v-model="formData.date" :disabled="modalMode === 'view'" type="date" required class="w-full border rounded-lg px-3 py-2 text-sm focus:ring-indigo-500 disabled:bg-gray-100"></div>
                  <div><label class="block text-xs font-bold mb-1">Ngày dự kiến giao <span class="text-red-500">*</span></label><input v-model="formData.expectedDate" :disabled="modalMode === 'view'" type="date" required class="w-full border rounded-lg px-3 py-2 text-sm focus:ring-indigo-500 disabled:bg-gray-100"></div>
                </div>
              </div>

              <div>
                <h4 class="text-sm font-bold mb-2">Danh sách Hàng hóa Đặt mua</h4>
                <div v-if="modalMode !== 'view'" class="flex flex-col sm:flex-row gap-2 mb-3 bg-indigo-50 p-3 rounded-lg border border-indigo-100">
                  <select v-model="selectedVariantIdToAdd" class="flex-1 border rounded-lg px-3 py-2 text-sm focus:ring-indigo-500 cursor-pointer">
                    <option value="" disabled>-- Tìm & Chọn Sản phẩm để mua --</option>
                    <option v-for="prod in productsList" :key="prod.id||prod.Id" :value="prod.id||prod.Id">[{{ prod.sku||prod.Sku }}] {{ prod.name||prod.Name }}</option>
                  </select>
                  <button type="button" @click="handleAddItem" :disabled="!selectedVariantIdToAdd" class="bg-indigo-600 hover:bg-indigo-700 text-white px-4 py-2 rounded-lg text-sm font-bold disabled:opacity-50 transition-colors">Thêm vào Đơn</button>
                </div>

                <div class="border rounded-lg overflow-x-auto">
                  <table class="w-full text-sm text-left">
                    <thead class="bg-gray-50 text-xs uppercase font-bold text-gray-500">
                      <tr><th class="px-4 py-3">Mã SKU</th><th class="px-4 py-3">Tên Hàng</th><th class="px-4 py-3 text-center">ĐVT</th><th class="px-4 py-3 text-right w-32">Số Lượng Đặt</th><th v-if="modalMode !== 'view'" class="px-4 py-3 text-center w-10">#</th></tr>
                    </thead>
                    <tbody class="divide-y divide-gray-100">
                      <tr v-if="formData.items.length === 0"><td :colspan="modalMode !== 'view' ? 5 : 4" class="px-4 py-8 text-center text-gray-400 italic">Đơn đặt hàng đang trống.</td></tr>
                      <tr v-for="(item, idx) in formData.items" :key="idx" class="hover:bg-gray-50">
                        <td class="px-4 py-2 font-bold">{{ item.sku }}</td><td class="px-4 py-2">{{ item.name }}</td><td class="px-4 py-2 text-center">{{ item.unit }}</td>
                        
                        <td class="px-4 py-2 text-right">
                          <input v-if="modalMode !== 'view'" v-model.number="item.qty" type="number" min="1" class="w-full text-right border rounded px-2 py-1 font-bold text-indigo-700 focus:ring-indigo-500 outline-none">
                          <span v-else class="font-bold text-indigo-700 text-lg">{{ item.qty }}</span>
                        </td>
                        
                        <td v-if="modalMode !== 'view'" class="px-4 py-2 text-center"><button type="button" @click="removeItem(idx)" class="text-red-400 hover:text-red-600"><TrashIcon class="w-5 h-5 mx-auto"/></button></td>
                      </tr>
                    </tbody>
                    <tfoot class="bg-gray-50 font-bold border-t"><td colspan="3" class="px-4 py-3 text-right uppercase text-gray-600">Tổng SL:</td><td class="px-4 py-3 text-right text-indigo-700 text-xl">{{ totalQty }}</td><td v-if="modalMode !== 'view'"></td></tfoot>
                  </table>
                </div>
              </div>

              <div><label class="block text-xs font-bold mb-1">Ghi chú</label><textarea v-model="formData.note" :disabled="modalMode === 'view'" rows="2" class="w-full border rounded-lg px-3 py-2 text-sm disabled:bg-gray-100"></textarea></div>
            </form>
          </div>

          <div class="px-6 py-4 border-t flex flex-col sm:flex-row justify-between gap-3 bg-white shrink-0">
            <div class="flex gap-2 w-full sm:w-auto">
              <button v-if="modalMode === 'view' && formData.status === 'approved'" @click="handleSendEmail" class="px-4 py-2.5 bg-blue-50 hover:bg-blue-100 text-blue-600 rounded-lg text-sm font-bold border border-blue-200 flex items-center justify-center gap-2 transition-colors"><PaperAirplaneIcon class="w-5 h-5"/> Gửi Email cho NCC</button>
            </div>

            <div class="flex gap-2 w-full sm:w-auto">
              <template v-if="modalMode === 'view' && formData.status === 'pending' && canApprovePO">
                <button @click="handleApprovePO" class="px-4 py-2.5 bg-indigo-600 hover:bg-indigo-700 text-white rounded-lg text-sm font-bold flex items-center justify-center gap-2 shadow-sm transition-colors"><CheckCircleIcon class="w-5 h-5"/> Giám đốc Duyệt PO</button>
              </template>

              <button type="button" @click="closeModal" class="flex-1 sm:flex-none px-5 py-2.5 border rounded-lg text-sm font-semibold hover:bg-gray-50 transition-colors">{{ modalMode === 'view' ? 'Đóng bảng' : 'Hủy bỏ' }}</button>
              <button v-if="modalMode !== 'view'" type="submit" form="poForm" class="flex-1 sm:flex-none px-5 py-2.5 bg-indigo-600 text-white rounded-lg text-sm font-bold hover:bg-indigo-700 shadow-md flex items-center justify-center gap-2 transition-colors"><CheckCircleIcon class="w-5 h-5"/> Lưu Đơn Đặt Hàng</button>
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