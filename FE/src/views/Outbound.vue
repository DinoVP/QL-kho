<script setup>
import { ref, computed, onMounted } from 'vue'
import { 
  MagnifyingGlassIcon, PlusIcon, EyeIcon, 
  XMarkIcon, DocumentArrowUpIcon, TrashIcon, PencilSquareIcon,
  PrinterIcon, DocumentTextIcon, ArrowDownTrayIcon,
  CheckCircleIcon, XCircleIcon
} from '@heroicons/vue/24/outline'

const OUTBOUND_API = 'https://localhost:7139/api/Outbound'
const PROD_API = 'https://localhost:7139/api/Products'
const PARTNER_API = 'https://localhost:7139/api/CrmPartners'
const LOC_API = 'https://localhost:7139/api/Locations'

const getAuthHeaders = () => ({
  'Content-Type': 'application/json',
  'Authorization': 'Bearer ' + (localStorage.getItem('authToken') || '')
})

const outboundReceipts = ref([])
const customers = ref([])
const productsList = ref([])
const locationsList = ref([])
const isLoading = ref(false)

const getToday = () => new Date().toISOString().split('T')[0]

// LẤY DỮ LIỆU
const fetchData = async () => {
  isLoading.value = true
  try {
    const headers = getAuthHeaders()
    const [outRes, prodRes, partRes, locRes] = await Promise.all([
      fetch(OUTBOUND_API, { headers }), fetch(PROD_API, { headers }),
      fetch(PARTNER_API, { headers }), fetch(LOC_API, { headers })
    ])
    
    if (outRes.ok) outboundReceipts.value = await outRes.json()
    if (prodRes.ok) productsList.value = await prodRes.json()
    if (locRes.ok) locationsList.value = await locRes.json()
    
    if (partRes.ok) {
      const pData = await partRes.json()
      const rawPartners = pData.data || pData.Data || pData || []
      // Lọc lấy Khách hàng
      customers.value = rawPartners.filter(p => p.isCustomer || p.partnerCode?.startsWith('KH'))
    }
  } catch (error) { console.error('Lỗi tải dữ liệu:', error) }
  finally { isLoading.value = false }
}

const searchQuery = ref('')
const filterStatus = ref('')

const filteredReceipts = computed(() => {
  return outboundReceipts.value.filter(r => {
    const matchSearch = (r.code || '').toLowerCase().includes(searchQuery.value.toLowerCase()) || 
                        (r.customerName || '').toLowerCase().includes(searchQuery.value.toLowerCase())
    const matchStatus = filterStatus.value === '' || r.status === filterStatus.value
    return matchSearch && matchStatus
  })
})

// LOGIC MODAL
const showModal = ref(false)
const modalMode = ref('add') 
const formData = ref({ id: 0, code: '', date: getToday(), customerId: '', customerName: '', items: [], note: '', status: 'pending' })

const openModal = (mode, receipt = null) => {
  modalMode.value = mode
  if (receipt) formData.value = JSON.parse(JSON.stringify(receipt)) 
  else formData.value = { id: 0, code: '', date: getToday(), customerId: '', customerName: '', items: [], note: '', status: 'pending' }
  selectedSkusToAdd.value = [] 
  showModal.value = true
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

// LOGIC CHỌN HÀNG MULTI (GIỐNG INBOUND)
const selectedSkusToAdd = ref([]) 
const handleAddMultipleItems = () => {
  if (selectedSkusToAdd.value.length === 0) return;
  selectedSkusToAdd.value.forEach(id => {
    const prod = productsList.value.find(p => p.id === id)
    if (prod) {
      formData.value.items.push({ 
        variantId: prod.id, sku: prod.sku, name: prod.name, unit: prod.unit, 
        qty: 1, price: prod.price || 0, locationId: null, locationCode: ''
      })
    }
  })
  selectedSkusToAdd.value = [] 
}

const removeItem = (index) => formData.value.items.splice(index, 1)

// LOGIC CHỌN VỊ TRÍ LẤY HÀNG (PICKING)
// Hàm này chỉ lọc ra các Vị trí (Kệ) ĐANG CHỨA cái sản phẩm (variantId) này
const getAvailableLocationsForProduct = (variantId) => {
  return locationsList.value.filter(loc => loc.variantIds && loc.variantIds.includes(variantId));
}

const updateLocationName = (item) => {
  const loc = locationsList.value.find(l => l.id === item.locationId);
  item.locationCode = loc ? loc.code : '';
}

const totalQty = computed(() => formData.value.items.reduce((sum, item) => sum + (item.qty || 0), 0))
const totalPrice = computed(() => formData.value.items.reduce((sum, item) => sum + ((item.qty || 0) * (item.price || 0)), 0))
const formatCurrency = (val) => new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(val || 0)

// LƯU PHIẾU (ADD / EDIT)
const handleSubmit = async () => {
  if (formData.value.items.length === 0) return alert('Sếp chưa chọn mặt hàng nào để xuất kho!')
  if (!formData.value.customerId) return alert('Vui lòng chọn Khách hàng nhận!')

  const cus = customers.value.find(c => c.partnerId === formData.value.customerId || c.id === formData.value.customerId)
  formData.value.customerName = cus ? (cus.partnerName || cus.name) : 'Khách hàng ẩn';
  formData.value.totalQty = totalQty.value;
  formData.value.totalPrice = totalPrice.value;

  try {
    const method = modalMode.value === 'add' ? 'POST' : 'PUT'
    const url = modalMode.value === 'add' ? OUTBOUND_API : `${OUTBOUND_API}/${formData.value.id}`
    
    const res = await fetch(url, { method, headers: getAuthHeaders(), body: JSON.stringify(formData.value) })
    if (res.ok) {
      alert(`${modalMode.value === 'add' ? 'Tạo' : 'Cập nhật'} Phiếu xuất thành công!`);
      await fetchData();
      closeModal();
    } else { 
      const err = await res.json(); alert('LỖI: ' + err.message); 
    }
  } catch(e) { console.error(e) }
}

// XÓA PHIẾU
const handleDelete = async (id, code) => {
  if (!confirm(`Hủy bỏ phiếu xuất [${code}] vĩnh viễn?`)) return;
  try {
    const res = await fetch(`${OUTBOUND_API}/${id}`, { method: 'DELETE', headers: getAuthHeaders() })
    if (res.ok) {
      await fetchData();
      alert('Đã xóa phiếu thành công!');
    } else {
      const err = await res.json(); alert('LỖI: ' + err.message);
    }
  } catch(e) { console.error(e) }
}

onMounted(() => { fetchData() })
</script>

<template>
  <div class="space-y-5 md:space-y-6 animate-fade-in pb-10 px-0 md:px-1 relative">
    
    <div class="flex flex-col sm:flex-row sm:items-center justify-between gap-4">
      <div>
        <h2 class="text-xl md:text-2xl font-bold text-gray-800">Phiếu Xuất kho (Outbound)</h2>
        <p class="text-xs md:text-sm text-gray-500 mt-1">Lập chứng từ xuất hàng hóa giao cho Khách hàng / Chi nhánh</p>
      </div>
      <div class="flex flex-wrap items-center gap-2">
        <button @click="openModal('add')" class="bg-primary-600 hover:bg-primary-700 text-white px-4 py-2.5 rounded-lg flex items-center gap-2 text-sm font-semibold shadow-sm">
          <PlusIcon class="w-5 h-5" /> Lập Phiếu Xuất
        </button>
      </div>
    </div>

    <div class="bg-white p-3 md:p-4 rounded-xl border border-gray-200 flex flex-col sm:flex-row items-center gap-3 md:gap-4 shadow-sm">
      <div class="relative w-full sm:flex-1">
        <div class="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none"><MagnifyingGlassIcon class="w-5 h-5 text-gray-400" /></div>
        <input v-model="searchQuery" type="text" class="block w-full pl-10 pr-3 py-2 border border-gray-200 rounded-lg text-sm outline-none focus:ring-1 focus:ring-primary-500" placeholder="Tìm theo mã phiếu, tên Khách hàng...">
      </div>
      <select v-model="filterStatus" class="w-full sm:w-auto border border-gray-200 rounded-lg text-sm px-4 py-2 outline-none focus:ring-1 focus:ring-primary-500 cursor-pointer">
        <option value="">Tất cả Trạng thái</option>
        <option value="pending">Chờ duyệt</option>
        <option value="approved">Đã Duyệt (Chờ lấy hàng)</option>
        <option value="completed">Đã xuất kho</option>
        <option value="rejected">Bị Từ chối</option>
      </select>
    </div>

    <div class="bg-white rounded-xl border border-gray-200 shadow-sm overflow-hidden">
      <div class="w-full overflow-x-auto custom-scrollbar">
        <table class="min-w-[1050px] w-full divide-y divide-gray-200">
          <thead class="bg-gray-50">
            <tr>
              <th class="px-5 py-3.5 text-left text-xs font-bold text-gray-500 uppercase tracking-wider">Mã Phiếu</th>
              <th class="px-5 py-3.5 text-left text-xs font-bold text-gray-500 uppercase tracking-wider">Ngày xuất</th>
              <th class="px-5 py-3.5 text-left text-xs font-bold text-gray-500 uppercase tracking-wider">Khách hàng nhận</th>
              <th class="px-5 py-3.5 text-right text-xs font-bold text-gray-500 uppercase tracking-wider">Tổng SL</th>
              <th class="px-5 py-3.5 text-right text-xs font-bold text-gray-500 uppercase tracking-wider">Tổng giá trị</th>
              <th class="px-5 py-3.5 text-center text-xs font-bold text-gray-500 uppercase tracking-wider">Trạng thái</th>
              <th class="px-5 py-3.5 text-right text-xs font-bold text-gray-500 uppercase tracking-wider">Thao tác</th>
            </tr>
          </thead>
          <tbody class="divide-y divide-gray-100 bg-white">
            <tr v-if="isLoading"><td colspan="7" class="px-6 py-12 text-center text-gray-500 font-medium">Đang tải dữ liệu...</td></tr>
            <tr v-else-if="filteredReceipts.length === 0">
              <td colspan="7" class="px-6 py-16 text-center">
                <DocumentArrowUpIcon class="w-12 h-12 text-gray-300 mx-auto mb-3" />
                <h3 class="text-base font-semibold text-gray-700">Chưa có Phiếu Xuất nào</h3>
              </td>
            </tr>
            <tr v-for="receipt in filteredReceipts" :key="receipt.id" class="hover:bg-gray-50 transition-colors">
              <td class="px-5 py-3 text-sm font-bold text-primary-700">{{ receipt.code }}</td>
              <td class="px-5 py-3 text-sm font-medium text-gray-600">{{ receipt.date }}</td>
              <td class="px-5 py-3 text-sm font-bold text-gray-800">{{ receipt.customerName }}</td>
              <td class="px-5 py-3 text-sm text-right font-medium text-gray-900">{{ receipt.totalQty }}</td>
              <td class="px-5 py-3 text-sm text-right font-bold text-amber-600">{{ formatCurrency(receipt.totalPrice) }}</td>
              <td class="px-5 py-3 text-center"><span :class="['text-[10px] font-bold px-2 py-1 rounded border uppercase tracking-wider', getStatusBadge(receipt.status).class]">{{ getStatusBadge(receipt.status).text }}</span></td>
              
              <td class="px-5 py-3 text-right whitespace-nowrap space-x-2">
                <button @click="openModal('view', receipt)" class="p-1.5 text-blue-600 hover:bg-blue-50 rounded" title="Xem chi tiết"><EyeIcon class="w-5 h-5" /></button>
                <button v-if="receipt.status === 'pending'" @click="openModal('edit', receipt)" class="p-1.5 text-amber-600 hover:bg-amber-50 rounded" title="Chỉnh sửa phiếu"><PencilSquareIcon class="w-5 h-5" /></button>
                <button v-if="receipt.status === 'pending'" @click="handleDelete(receipt.id, receipt.code)" class="p-1.5 text-red-600 hover:bg-red-50 rounded" title="Hủy phiếu"><TrashIcon class="w-5 h-5" /></button>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>

    <Teleport to="body">
      <div v-if="showModal" class="fixed inset-0 z-50 flex items-center justify-center px-4 bg-slate-900/50 backdrop-blur-sm animate-fade-in">
        <div class="bg-white rounded-xl shadow-2xl w-full max-w-5xl overflow-hidden flex flex-col max-h-[90vh]">
          
          <div class="px-6 py-4 border-b border-gray-100 flex items-center justify-between bg-gray-50 shrink-0">
            <h3 class="text-lg font-bold text-gray-800 flex items-center gap-2">
              <DocumentArrowUpIcon class="w-6 h-6 text-amber-600"/> 
              <span v-if="modalMode === 'add'">Lập Phiếu Xuất Kho</span>
              <span v-else-if="modalMode === 'edit'">Sửa Phiếu: {{ formData.code }}</span>
              <span v-else>Chi tiết Phiếu Xuất: {{ formData.code }}</span>
            </h3>
            <button @click="closeModal" class="text-gray-400 hover:text-red-500"><XMarkIcon class="w-6 h-6" /></button>
          </div>

          <div class="p-6 overflow-y-auto flex-1 custom-scrollbar">
            <form id="outboundForm" @submit.prevent="handleSubmit" class="space-y-6">
              
              <div class="bg-slate-50 p-4 rounded-lg border border-slate-200">
                <div class="grid grid-cols-1 sm:grid-cols-4 gap-4">
                  <div class="sm:col-span-2">
                    <label class="block text-xs font-bold text-gray-700 mb-1">Khách hàng / Đơn vị nhận <span class="text-red-500">*</span></label>
                    <select v-model="formData.customerId" :disabled="modalMode === 'view'" required class="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm focus:ring-amber-500 disabled:bg-gray-100 cursor-pointer">
                      <option value="" disabled>-- Chọn Khách hàng --</option>
                      <option v-for="cus in customers" :key="cus.partnerId || cus.id" :value="cus.partnerId || cus.id">
                        {{ cus.partnerName || cus.name }}
                      </option>
                    </select>
                  </div>
                  <div>
                    <label class="block text-xs font-bold text-gray-700 mb-1">Ngày xuất <span class="text-red-500">*</span></label>
                    <input v-model="formData.date" :disabled="modalMode === 'view'" type="date" required class="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm focus:ring-amber-500 disabled:bg-gray-100">
                  </div>
                  <div>
                    <label class="block text-xs font-bold text-gray-700 mb-1">Trạng thái</label>
                    <div class="px-3 py-2 text-sm font-bold rounded-lg border text-center" :class="getStatusBadge(formData.status).class">
                      {{ getStatusBadge(formData.status).text }}
                    </div>
                  </div>
                </div>
              </div>

              <div>
                <h4 class="text-sm font-bold text-slate-800 mb-2">Chi tiết Hàng hóa & Vị trí lấy hàng (Picking)</h4>

                <div v-if="modalMode !== 'view'" class="flex flex-col gap-2 mb-4 bg-amber-50 p-4 rounded-xl border border-amber-100">
                  <label class="text-sm font-bold text-amber-800">1. Tích chọn các mặt hàng cần xuất:</label>
                  <div class="bg-white border border-amber-200 rounded-lg max-h-40 overflow-y-auto custom-scrollbar p-2 shadow-inner">
                    <div v-if="productsList.length === 0" class="p-2 text-sm text-gray-400 italic">Chưa có sản phẩm nào.</div>
                    <label v-for="prod in productsList" :key="prod.id" class="flex items-center gap-3 p-2 hover:bg-amber-50 cursor-pointer border-b border-gray-100 last:border-0 rounded transition-colors">
                      <input type="checkbox" :value="prod.id" v-model="selectedSkusToAdd" class="w-4 h-4 text-amber-600 border-gray-300 rounded focus:ring-amber-500 cursor-pointer">
                      <span class="text-sm font-medium text-gray-800 flex-1">[{{ prod.sku }}] {{ prod.name }}</span>
                    </label>
                  </div>
                  <div class="flex justify-between items-center mt-2">
                    <span class="text-xs font-bold text-amber-700">Đã tích chọn: {{ selectedSkusToAdd.length }} sản phẩm</span>
                    <button type="button" @click="handleAddMultipleItems" :disabled="selectedSkusToAdd.length === 0" class="bg-amber-600 hover:bg-amber-700 text-white px-6 py-2 rounded-lg text-sm font-bold disabled:opacity-50 transition-colors shadow-sm">
                      2. Đưa xuống lưới
                    </button>
                  </div>
                </div>

                <div class="border border-gray-200 rounded-lg overflow-x-auto shadow-sm">
                  <table class="w-full text-sm text-left">
                    <thead class="bg-gray-100 text-xs uppercase font-bold text-gray-600 border-b">
                      <tr>
                        <th class="px-4 py-3">Mã SKU</th>
                        <th class="px-4 py-3">Tên Hàng</th>
                        <th class="px-4 py-3 text-center">ĐVT</th>
                        <th class="px-4 py-3 w-64 text-indigo-700">Lấy từ Vị trí (Picking)</th>
                        <th class="px-4 py-3 text-right w-24 text-amber-700">Số lượng</th>
                        <th class="px-4 py-3 text-right w-32">Đơn giá Bán</th>
                        <th class="px-4 py-3 text-right">Thành tiền</th>
                        <th v-if="modalMode !== 'view'" class="px-4 py-3 text-center w-10">#</th>
                      </tr>
                    </thead>
                    <tbody class="divide-y divide-gray-200">
                      <tr v-if="formData.items.length === 0">
                        <td :colspan="modalMode !== 'view' ? 8 : 7" class="px-4 py-8 text-center text-gray-400 italic font-medium">Chưa có mặt hàng nào.</td>
                      </tr>
                      <tr v-for="(item, idx) in formData.items" :key="idx" class="hover:bg-amber-50/50 transition-colors">
                        <td class="px-4 py-2 font-bold text-gray-700">{{ item.sku }}</td>
                        <td class="px-4 py-2 font-medium">{{ item.name }}</td>
                        <td class="px-4 py-2 text-center text-gray-500">{{ item.unit }}</td>
                        
                        <td class="px-4 py-2">
                          <select v-if="modalMode !== 'view'" v-model="item.locationId" @change="updateLocationName(item)" class="w-full border border-indigo-300 rounded px-2 py-1.5 text-xs focus:ring-indigo-500 font-bold bg-indigo-50 outline-none cursor-pointer">
                            <option :value="null">-- Kho chung (Không rõ vị trí) --</option>
                            <option v-for="loc in getAvailableLocationsForProduct(item.variantId)" :key="loc.id" :value="loc.id">
                              {{ loc.code }}
                            </option>
                          </select>
                          <span v-else class="font-bold text-indigo-600 text-xs">{{ item.locationCode || 'Kho chung' }}</span>
                        </td>

                        <td class="px-4 py-2 text-right">
                          <input v-if="modalMode !== 'view'" v-model.number="item.qty" type="number" min="1" class="w-full text-right border border-amber-300 bg-amber-50 rounded px-2 py-1 font-bold text-amber-700 focus:outline-none focus:ring-1 focus:ring-amber-500">
                          <span v-else class="font-bold text-amber-700">{{ item.qty }}</span>
                        </td>
                        <td class="px-4 py-2 text-right">
                          <input v-if="modalMode !== 'view'" v-model.number="item.price" type="number" min="0" class="w-full text-right border border-gray-300 rounded px-2 py-1 focus:outline-none focus:ring-1 focus:ring-amber-500 text-xs">
                          <span v-else class="text-xs font-medium">{{ formatCurrency(item.price) }}</span>
                        </td>
                        <td class="px-4 py-2 text-right font-bold text-amber-600">
                          {{ formatCurrency((item.qty || 0) * (item.price || 0)) }}
                        </td>
                        <td v-if="modalMode !== 'view'" class="px-4 py-2 text-center">
                          <button type="button" @click="removeItem(idx)" class="text-red-400 hover:text-red-600 transition-colors"><TrashIcon class="w-5 h-5 mx-auto"/></button>
                        </td>
                      </tr>
                    </tbody>
                    <tfoot class="bg-gray-50 font-bold border-t border-gray-200">
                      <tr>
                        <td colspan="4" class="px-4 py-3 text-right uppercase text-gray-600">Tổng xuất:</td>
                        <td class="px-4 py-3 text-right text-amber-700 text-base">{{ totalQty }}</td>
                        <td class="px-4 py-3"></td>
                        <td class="px-4 py-3 text-right text-amber-600 text-base">{{ formatCurrency(totalPrice) }}</td>
                        <td v-if="modalMode !== 'view'"></td>
                      </tr>
                    </tfoot>
                  </table>
                </div>
              </div>

              <div>
                <label class="block text-xs font-bold text-gray-700 mb-1">Ghi chú phiếu</label>
                <input v-model="formData.note" :disabled="modalMode === 'view'" class="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm focus:ring-amber-500 disabled:bg-gray-100" placeholder="VD: Giao nhanh...">
              </div>

            </form>
          </div>

          <div class="px-6 py-4 border-t border-gray-100 flex justify-end items-center bg-white shrink-0">
            <div class="flex gap-2">
              <button type="button" @click="closeModal" class="px-6 py-2 border border-gray-300 text-gray-700 rounded-lg text-sm font-bold hover:bg-gray-50 transition-colors">
                {{ modalMode === 'view' ? 'Đóng' : 'Hủy' }}
              </button>
              
              <button v-if="modalMode !== 'view'" type="submit" form="outboundForm" class="px-6 py-2 bg-amber-600 text-white rounded-lg text-sm font-bold hover:bg-amber-700 shadow-md transition-colors flex items-center gap-2">
                <DocumentArrowUpIcon class="w-5 h-5"/> {{ modalMode === 'add' ? 'Hoàn tất Phiếu Xuất' : 'Lưu Thay Đổi' }}
              </button>
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
.custom-scrollbar:hover::-webkit-scrollbar-thumb { background: #94a3b8; }
.animate-fade-in { animation: fadeIn 0.2s ease-out; }
@keyframes fadeIn { from { opacity: 0; transform: translateY(5px); } to { opacity: 1; transform: translateY(0); } }
</style>