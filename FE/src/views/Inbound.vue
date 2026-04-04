<script setup>
import { ref, computed, onMounted } from 'vue'
import { 
  MagnifyingGlassIcon, PlusIcon, EyeIcon, 
  XMarkIcon, DocumentTextIcon, TrashIcon,
  CheckCircleIcon, ArrowDownTrayIcon, ArchiveBoxArrowDownIcon
} from '@heroicons/vue/24/outline'

const INBOUND_API = 'https://localhost:7139/api/Inbound'
const PROD_API = 'https://localhost:7139/api/Products'
// CHÚ Ý CHỖ NÀY: PHẢI LÀ CrmPartners THÌ MỚI HẾT LỖI 404
const PARTNER_API = 'https://localhost:7139/api/CrmPartners' 

const getAuthHeaders = () => ({ 'Content-Type': 'application/json', 'Authorization': 'Bearer ' + (localStorage.getItem('authToken') || '') })

const currentUserRole = ref(localStorage.getItem('role') || 'gd_chi_nhanh')
const myWarehouseId = ref(parseInt(localStorage.getItem('warehouseId')) || null)

const canCreate = computed(() => ['admin', 'gd_chi_nhanh', 'ql_kho'].includes(currentUserRole.value.toLowerCase()))

const receipts = ref([])
const productsList = ref([])
const partnersList = ref([])
const isLoading = ref(false)

const getToday = () => new Date().toISOString().split('T')[0]

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
        
        r.totalQty = r.items ? r.items.reduce((s, i) => s + (i.qty || 0), 0) : 0;
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
      return {
        variantId: variantId, sku: prod.sku || prod.Sku, name: prod.name || prod.Name, unit: prod.unit || prod.Unit, 
        conversionRate: prod.conversionRate || prod.ConversionRate || 24, 
        qty: i.qty || i.Qty || 1,
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

// ĐÃ SỬA CHỖ NÀY: Thêm Trạng thái Approved (Chờ nhập kho)
const getStatusBadge = (status) => {
  switch(status) {
    case 'pending': return { text: 'Chờ duyệt', class: 'bg-amber-100 text-amber-700 border-amber-200' }
    case 'approved': return { text: 'Chờ nhận hàng', class: 'bg-blue-100 text-blue-700 border-blue-200' }
    case 'completed': return { text: 'Đã vào bãi tập kết', class: 'bg-emerald-100 text-emerald-700 border-emerald-200' }
    case 'rejected': return { text: 'Bị từ chối', class: 'bg-red-100 text-red-700 border-red-200' }
    default: return { text: 'Khác', class: 'bg-gray-100 text-gray-500' }
  }
}

// ĐÃ THÊM CHỖ NÀY: Nút xác nhận hàng đã về tới kho
const handleCompleteReceipt = async (receipt) => {
  if (!confirm(`Xác nhận lô hàng của Phiếu [${receipt.code || receipt.Code}] đã về tới kho và ĐƯA VÀO BÃI TẬP KẾT?`)) return;
  try {
    const res = await fetch(`${INBOUND_API}/${receipt.id || receipt.Id}/complete`, { method: 'PUT', headers: getAuthHeaders() })
    if (res.ok) { 
      alert('Nhập kho thành công! Hàng đang nằm ở "Khu chờ nhập", hãy sang màn hình Kho Bãi (Putaway) để cất lên kệ.'); 
      await fetchData(); 
    } else { 
      let errMsg = "Lỗi hệ thống!"; try { const t = await res.text(); if(t) errMsg = JSON.parse(t).message || errMsg; } catch(e){} 
      alert('LỖI SQL: ' + errMsg); 
    }
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
    if (prod) {
      const exists = formData.value.items.find(i => i.variantId === pid)
      if(!exists) {
        formData.value.items.push({ 
          variantId: pid, sku: prod.sku || prod.Sku, name: prod.name || prod.Name, unit: prod.unit || prod.Unit, 
          conversionRate: prod.conversionRate || prod.ConversionRate || 24, 
          qty: 1, nsx: '', hsd: '' 
        })
      }
    }
  })
  selectedProductIds.value = []; productSearchQuery.value = '' 
}

const removeItem = (index) => formData.value.items.splice(index, 1)

const totalModalQty = computed(() => formData.value.items.reduce((sum, item) => sum + (item.qty || 0), 0))

const handleSubmit = async () => {
  if (!formData.value.partnerId) return alert('Vui lòng chọn Nhà cung cấp!');
  if (formData.value.items.length === 0) return alert('Chưa có sản phẩm nào trong phiếu!')
  
  try {
    const payload = { ...formData.value, code: "" };
    payload.warehouseId = myWarehouseId.value || 1;
    payload.items = payload.items.map(i => ({ ...i, locationId: null })) 

    const res = await fetch(INBOUND_API, { method: 'POST', headers: getAuthHeaders(), body: JSON.stringify(payload) })
    if (res.ok) { alert('Lập Phiếu Nhập thành công! Hãy đợi Admin duyệt.'); await fetchData(); closeModal(); } 
    else { alert('LỖI HỆ THỐNG'); }
  } catch(e) { console.error(e) }
}

onMounted(() => fetchData())
</script>

<template>
  <div class="space-y-5 md:space-y-6 animate-fade-in pb-10 px-0 md:px-1 relative">
    
    <div class="flex flex-col sm:flex-row sm:items-center justify-between gap-4">
      <div>
        <h2 class="text-xl md:text-2xl font-bold text-gray-800">Phiếu Nhập Kho</h2>
        <p class="text-xs md:text-sm text-gray-500 mt-1">Ghi nhận hàng hóa từ Nhà cung cấp gửi tới bãi tập kết</p>
        <p class="text-[10px] font-bold text-indigo-600 bg-indigo-50 px-2 py-1 rounded inline-block mt-2 border border-indigo-200 uppercase">Kho ID: {{ myWarehouseId || 'Tất cả Kho (Admin)' }}</p>
      </div>
      <div class="flex flex-wrap items-center gap-2">
        <button v-if="canCreate" @click="openModal('add')" class="bg-primary-600 hover:bg-primary-700 text-white px-4 py-2.5 rounded-lg flex items-center gap-2 text-sm font-semibold shadow-sm transition-colors"><PlusIcon class="w-5 h-5" /> Lập Phiếu Nhập</button>
      </div>
    </div>

    <div class="bg-white p-3 md:p-4 rounded-xl border border-gray-200 flex flex-col sm:flex-row items-center gap-3 md:gap-4 shadow-sm">
      <div class="relative w-full sm:flex-1"><div class="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none"><MagnifyingGlassIcon class="w-5 h-5 text-gray-400" /></div><input v-model="searchQuery" type="text" class="block w-full pl-10 pr-3 py-2 border border-gray-200 rounded-lg text-sm outline-none focus:ring-1 focus:ring-primary-500" placeholder="Tìm theo mã phiếu..."></div>
      <select v-model="filterStatus" class="w-full sm:w-auto border border-gray-200 rounded-lg text-sm px-4 py-2 outline-none focus:ring-1 focus:ring-primary-500 cursor-pointer">
        <option value="">Tất cả Trạng thái</option>
        <option value="pending">Chờ duyệt</option>
        <option value="approved">Chờ nhận hàng</option>
        <option value="completed">Đã vào bãi tập kết</option>
        <option value="rejected">Bị từ chối</option>
      </select>
    </div>

    <div class="bg-white rounded-xl border border-gray-200 shadow-sm overflow-hidden">
      <div class="w-full overflow-x-auto custom-scrollbar">
        <table class="min-w-[1050px] w-full divide-y divide-gray-200">
          <thead class="bg-gray-50">
            <tr>
              <th class="px-5 py-3.5 text-left text-xs font-bold text-gray-500 uppercase tracking-wider">Mã Phiếu</th>
              <th class="px-5 py-3.5 text-left text-xs font-bold text-gray-500 uppercase tracking-wider">Ngày lập</th>
              <th class="px-5 py-3.5 text-left text-xs font-bold text-gray-500 uppercase tracking-wider text-blue-700">Nhà Cung Cấp</th>
              <th class="px-5 py-3.5 text-center text-xs font-bold text-gray-500 uppercase tracking-wider">Tổng SL Nhập</th>
              <th class="px-5 py-3.5 text-center text-xs font-bold text-gray-500 uppercase tracking-wider">Trạng thái</th>
              <th class="px-5 py-3.5 text-right text-xs font-bold text-gray-500 uppercase tracking-wider">Thao tác</th>
            </tr>
          </thead>
          <tbody class="divide-y divide-gray-100 bg-white">
            <tr v-if="filteredReceipts.length === 0"><td colspan="6" class="px-6 py-16 text-center"><ArrowDownTrayIcon class="w-12 h-12 text-gray-300 mx-auto mb-3" /><h3 class="text-base font-semibold text-gray-700">Chưa có Phiếu Nhập nào của kho này</h3></td></tr>
            <tr v-for="receipt in filteredReceipts" :key="receipt.id" class="hover:bg-gray-50 transition-colors">
              <td class="px-5 py-3 text-sm font-bold text-primary-700">{{ receipt.code }}</td><td class="px-5 py-3 text-sm font-medium text-gray-600">{{ receipt.date }}</td>
              <td class="px-5 py-3 text-sm font-bold text-blue-700">{{ receipt.partnerName }}</td>
              <td class="px-5 py-3 text-sm text-center font-bold text-gray-800">{{ receipt.totalQty }}</td>
              <td class="px-5 py-3 text-center"><span :class="['text-[10px] font-bold px-2 py-1 rounded border uppercase tracking-wider', getStatusBadge(receipt.status).class]">{{ getStatusBadge(receipt.status).text }}</span></td>
              <td class="px-5 py-3 text-right whitespace-nowrap">
                <button @click="openModal('view', receipt)" class="p-1.5 text-blue-600 hover:bg-blue-50 rounded transition-colors" title="Xem chi tiết"><EyeIcon class="w-5 h-5" /></button>
                
                <button v-if="(receipt.status || receipt.Status) === 'approved'" @click="handleCompleteReceipt(receipt)" class="p-1.5 text-emerald-600 hover:bg-emerald-50 rounded transition-colors ml-1" title="Xác nhận hàng đã về và Đưa vào Khu Chờ Nhập">
                  <ArchiveBoxArrowDownIcon class="w-5 h-5" />
                </button>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>

    <Teleport to="body">
      <div v-if="showModal" class="fixed inset-0 z-50 flex items-center justify-center px-4 bg-slate-900/50 backdrop-blur-sm animate-fade-in">
        <div class="bg-white rounded-xl shadow-2xl w-full max-w-[95vw] lg:max-w-6xl overflow-hidden flex flex-col max-h-[90vh]">
          
          <div class="px-6 py-4 border-b border-gray-100 flex items-center justify-between bg-gray-50 shrink-0"><h3 class="text-lg font-bold flex items-center gap-2"><ArrowDownTrayIcon class="w-6 h-6 text-primary-600"/> {{ modalMode === 'add' ? 'Lập Phiếu Nhập Kho' : `Chi tiết: ${formData.code}` }}</h3><button @click="closeModal" class="text-gray-400 hover:text-red-500"><XMarkIcon class="w-6 h-6" /></button></div>
          
          <div class="p-6 overflow-y-auto flex-1 custom-scrollbar">
            <form id="inboundForm" @submit.prevent="handleSubmit" class="space-y-6">
              
              <div class="grid grid-cols-1 sm:grid-cols-3 gap-4 bg-slate-50 p-4 rounded-lg border border-slate-200">
                <div>
                  <label class="block text-xs font-bold mb-1">Nhà cung cấp *</label>
                  <select v-model="formData.partnerId" :disabled="modalMode === 'view'" required class="w-full border rounded-lg px-3 py-2 text-sm bg-white focus:ring-primary-500 disabled:bg-gray-100">
                    <option value="" disabled>-- Chọn Nhà cung cấp --</option>
                    <option v-for="p in partnersList" :key="p.partnerId||p.id||p.Id" :value="p.partnerId||p.id||p.Id">{{ p.partnerName||p.name||p.Name }}</option>
                  </select>
                </div>
                <div><label class="block text-xs font-bold mb-1">Ngày lập *</label><input v-model="formData.date" :disabled="modalMode === 'view'" type="date" required class="w-full border bg-white rounded-lg px-3 py-2 text-sm focus:ring-primary-500 disabled:bg-gray-100"></div>
                <div><label class="block text-xs font-bold mb-1">Trạng thái</label><div class="px-3 py-2 text-sm font-bold rounded border text-center" :class="getStatusBadge(formData.status).class">{{ getStatusBadge(formData.status).text }}</div></div>
              </div>

              <div v-if="modalMode === 'add'" class="bg-blue-50 p-4 rounded-xl border border-blue-100">
                <label class="text-sm font-bold text-blue-800 mb-2 block">Tìm & Chọn Hàng hóa:</label>
                <input v-model="productSearchQuery" type="text" class="w-full border border-blue-200 rounded px-3 py-1.5 text-sm mb-2 outline-none focus:ring-1 focus:ring-blue-500 shadow-sm bg-white" placeholder="🔍 Gõ tên hoặc mã SKU...">
                <div class="bg-white border border-blue-200 rounded max-h-32 overflow-y-auto p-2">
                  <label v-for="p in filteredProducts" :key="p.id||p.Id" class="flex items-center gap-3 p-2 hover:bg-blue-50 cursor-pointer border-b">
                    <input type="checkbox" :value="p.id||p.Id" v-model="selectedProductIds" class="w-4 h-4 text-blue-600 rounded">
                    <span class="text-sm flex-1 font-medium">[{{ p.sku||p.Sku }}] {{ p.name||p.Name }}</span>
                  </label>
                </div>
                <button type="button" @click="handleAddProducts" class="mt-2 px-4 py-1.5 bg-blue-600 text-white rounded text-sm font-bold disabled:opacity-50" :disabled="selectedProductIds.length === 0">Đưa {{selectedProductIds.length}} SP xuống lưới</button>
              </div>

              <div>
                <div class="border rounded-lg overflow-x-auto shadow-sm">
                  <table class="w-full text-sm text-left">
                    <thead class="bg-gray-50 text-xs uppercase font-bold text-gray-500 border-b"><tr>
                      <th class="px-3 py-3 min-w-[150px]">SKU</th>
                      <th class="px-3 py-3">Tên Hàng</th>
                      <th class="px-3 py-3 text-center">NSX</th>
                      <th class="px-3 py-3 text-center">HSD</th>
                      <th class="px-3 py-3 text-center bg-orange-50 text-orange-700">Quy Đổi</th>
                      <th class="px-3 py-3 text-center w-32">Tổng SL (Cái)</th>
                      <th v-if="modalMode === 'add'" class="px-3 py-3 text-center w-10">#</th>
                    </tr></thead>
                    <tbody class="divide-y divide-gray-100">
                      <tr v-if="formData.items.length === 0"><td :colspan="modalMode === 'add' ? 7 : 6" class="px-4 py-8 text-center text-gray-400 italic">Chưa có sản phẩm nào.</td></tr>
                      <tr v-for="(item, idx) in formData.items" :key="idx" class="hover:bg-gray-50">
                        <td class="px-3 py-2 font-bold">{{ item.sku }}</td>
                        <td class="px-3 py-2 text-gray-700">{{ item.name }}</td>
                        <td class="px-3 py-2 text-center"><input v-if="modalMode === 'add'" v-model="item.nsx" type="date" class="border rounded px-1 py-1 text-xs"><span v-else>{{item.nsx}}</span></td>
                        <td class="px-3 py-2 text-center"><input v-if="modalMode === 'add'" v-model="item.hsd" type="date" class="border rounded px-1 py-1 text-xs"><span v-else>{{item.hsd}}</span></td>
                        
                        <td class="px-3 py-2 text-center bg-orange-50/30">
                          <span class="font-bold text-orange-600">{{ item.conversionRate || 24 }}</span><br>
                          <span class="text-[9px] text-gray-400">Món/Thùng</span>
                        </td>
                        
                        <td class="px-3 py-2">
                          <input v-if="modalMode === 'add'" v-model.number="item.qty" type="number" min="1" class="w-full text-center border border-blue-300 rounded px-1 py-1.5 text-base font-bold text-blue-700 outline-none focus:ring-1 focus:ring-blue-500">
                          <span v-else class="font-bold block text-center text-blue-700 text-base">{{item.qty}}</span>
                        </td>
                        
                        <td v-if="modalMode === 'add'" class="px-3 py-2 text-center"><button type="button" @click="removeItem(idx)" class="text-red-400 hover:text-red-600"><TrashIcon class="w-5 h-5 mx-auto"/></button></td>
                      </tr>
                    </tbody>
                    <tfoot class="bg-gray-50 font-bold border-t">
                      <tr>
                        <td colspan="5" class="px-4 py-3 text-right uppercase text-gray-600">Tổng Số Lượng:</td>
                        <td class="px-4 py-3 text-center text-blue-700 text-xl">{{ totalModalQty }}</td>
                        <td v-if="modalMode === 'add'"></td>
                      </tr>
                    </tfoot>
                  </table>
                </div>
              </div>
              <div><label class="block text-xs font-bold mb-1">Ghi chú</label><textarea v-model="formData.note" :disabled="modalMode === 'view'" rows="2" class="w-full border rounded-lg px-3 py-2 text-sm outline-none focus:ring-1 focus:ring-primary-500 disabled:bg-gray-100"></textarea></div>
            </form>
          </div>

          <div class="px-6 py-4 border-t flex justify-end gap-3 shrink-0 bg-white">
            <button type="button" @click="closeModal" class="px-5 py-2 border rounded-lg text-sm font-semibold hover:bg-gray-50">{{ modalMode === 'view' ? 'Đóng lại' : 'Hủy bỏ' }}</button>
            <button v-if="modalMode === 'add'" type="submit" form="inboundForm" class="px-5 py-2 bg-primary-600 text-white rounded-lg text-sm font-bold hover:bg-primary-700 shadow-md">Hoàn tất Phiếu</button>
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