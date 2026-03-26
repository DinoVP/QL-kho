<script setup>
import { ref, computed } from 'vue'
import { 
  MagnifyingGlassIcon, PlusIcon, EyeIcon, 
  XMarkIcon, DocumentArrowUpIcon, TrashIcon,
  PrinterIcon, DocumentTextIcon, ArrowDownTrayIcon,
  CheckCircleIcon, XCircleIcon
} from '@heroicons/vue/24/outline'

// === 1. GIẢ LẬP PHÂN QUYỀN (Thay đổi để test: 'admin', 'giam_doc', 'gd_chi_nhanh', 'ql_kho', 'nv_kho') ===
const currentUserRole = ref('ql_kho') 

// Logic kiểm tra quyền: Ai được Duyệt, Ai được Xuất file?
const canApprove = computed(() => ['admin', 'giam_doc', 'gd_chi_nhanh', 'ql_kho'].includes(currentUserRole.value))
const canExport = computed(() => ['admin', 'giam_doc', 'gd_chi_nhanh', 'ql_kho'].includes(currentUserRole.value))

// === 2. STATE CHÍNH: TRỐNG CHỜ API ===
const outboundReceipts = ref([])

// === 3. DANH MỤC BỔ TRỢ (CHỜ API ĐỔ VÀO ĐỂ LẬP PHIẾU) ===
const mockCustomers = ref([])
const mockProducts = ref([])

// === 4. BỘ LỌC TÌM KIẾM ===
const searchQuery = ref('')
const filterStatus = ref('')

const filteredReceipts = computed(() => {
  return outboundReceipts.value.filter(r => {
    const matchSearch = r.code.toLowerCase().includes(searchQuery.value.toLowerCase()) || 
                        (r.customerName && r.customerName.toLowerCase().includes(searchQuery.value.toLowerCase()))
    const matchStatus = filterStatus.value === '' || r.status === filterStatus.value
    return matchSearch && matchStatus
  }).sort((a, b) => b.id - a.id)
})

// === 5. LOGIC MODAL TẠO / XEM PHIẾU ===
const showModal = ref(false)
const modalMode = ref('add') 

const formData = ref({ 
  id: null, code: '', date: '', customerId: '', 
  items: [], note: '', status: 'pending' 
})

const generateReceiptCode = () => `PX${(outboundReceipts.value.length + 1).toString().padStart(4, '0')}`
const getToday = () => new Date().toISOString().split('T')[0]

const openModal = (mode, receipt = null) => {
  modalMode.value = mode
  if (receipt) {
    formData.value = JSON.parse(JSON.stringify(receipt)) 
  } else {
    formData.value = { id: null, code: '', date: getToday(), customerId: '', items: [], note: '', status: 'pending' }
  }
  showModal.value = true
}

const closeModal = () => showModal.value = false

const getStatusBadge = (status) => {
  switch(status) {
    case 'pending': return { text: 'Chờ duyệt', class: 'bg-amber-100 text-amber-700 border-amber-200' }
    case 'shipping': return { text: 'Đang giao hàng', class: 'bg-blue-100 text-blue-700 border-blue-200' }
    case 'completed': return { text: 'Đã giao thành công', class: 'bg-emerald-100 text-emerald-700 border-emerald-200' }
    case 'rejected': return { text: 'Từ chối', class: 'bg-red-100 text-red-700 border-red-200' }
    default: return { text: 'Khác', class: 'bg-gray-100 text-gray-500' }
  }
}

// === 6. LOGIC CHỌN HÀNG XUẤT ===
const selectedSkuToAdd = ref('')

const handleAddItem = () => {
  if (!selectedSkuToAdd.value) return
  const existingItem = formData.value.items.find(i => i.sku === selectedSkuToAdd.value)
  if (existingItem) existingItem.qty += 1
  else {
    const prod = mockProducts.value.find(p => p.sku === selectedSkuToAdd.value)
    if (prod) formData.value.items.push({ sku: prod.sku, name: prod.name, unit: prod.unit, qty: 1, price: prod.price })
  }
  selectedSkuToAdd.value = '' 
}

const removeItem = (index) => formData.value.items.splice(index, 1)

// === 7. LOGIC TÍNH TOÁN, LƯU & XỬ LÝ PHIẾU ===
const totalQty = computed(() => formData.value.items.reduce((sum, item) => sum + item.qty, 0))
const totalPrice = computed(() => formData.value.items.reduce((sum, item) => sum + (item.qty * item.price), 0))
const formatCurrency = (val) => new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(val)

const handleSubmit = () => {
  if (formData.value.items.length === 0) return alert('Sếp chưa chọn mặt hàng nào để xuất kho!')
  if (!formData.value.customerId) return alert('Vui lòng chọn Khách hàng nhận!')
  
  if (modalMode.value === 'add') {
    outboundReceipts.value.push({
      ...formData.value, id: Date.now(), code: generateReceiptCode(),
      totalQty: totalQty.value, totalPrice: totalPrice.value,
      customerName: mockCustomers.value.find(c => c.id === formData.value.customerId)?.name || 'Chưa rõ'
    })
    alert('Tạo Phiếu Xuất Kho thành công!')
  }
  closeModal()
}

// Logic Duyệt / Từ chối (Dành cho Cấp quản lý)
const handleApprove = (status) => {
  alert(`Đã ${status === 'shipping' ? 'DUYỆT (Chuyển sang Giao hàng)' : 'TỪ CHỐI'} phiếu xuất ${formData.value.code}`)
  formData.value.status = status // Ở đây có API thì sẽ gọi API update status
  closeModal()
}

const handlePrintReceipt = () => alert(`Đang xuất lệnh máy in: IN PHIẾU XUẤT KHO mã [${formData.value.code}]...`)
const handlePrintInvoice = () => alert(`Đang tải file PDF: HÓA ĐƠN XUẤT HÀNG cho mã [${formData.value.code}]...`)
</script>

<template>
  <div class="space-y-5 md:space-y-6 animate-fade-in pb-10 px-0 md:px-1 relative">
    
    <div class="flex flex-col sm:flex-row sm:items-center justify-between gap-4">
      <div>
        <h2 class="text-xl md:text-2xl font-bold text-gray-800">Phiếu Xuất kho (Outbound)</h2>
        <p class="text-xs md:text-sm text-gray-500 mt-1">Lập chứng từ xuất hàng hóa giao cho Khách hàng / Chi nhánh</p>
      </div>
      
      <div class="flex flex-wrap items-center gap-2">
        <button v-if="canExport" @click="() => alert('Xuất toàn bộ danh sách ra Excel...')" class="bg-white border border-emerald-200 text-emerald-700 px-3 py-2.5 rounded-lg text-sm font-semibold hover:bg-emerald-50 transition-colors shadow-sm flex items-center gap-1.5">
          <DocumentTextIcon class="w-4 h-4"/> Xuất Excel
        </button>
        <button v-if="canExport" @click="() => alert('Xuất báo cáo danh sách ra PDF...')" class="bg-white border border-red-200 text-red-700 px-3 py-2.5 rounded-lg text-sm font-semibold hover:bg-red-50 transition-colors shadow-sm flex items-center gap-1.5">
          <ArrowDownTrayIcon class="w-4 h-4"/> Xuất PDF
        </button>
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
        <option value="shipping">Đang giao hàng</option>
        <option value="completed">Đã giao thành công</option>
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
              <th class="px-5 py-3.5 text-left text-xs font-bold text-gray-500 uppercase tracking-wider">Đối tác nhận</th>
              <th class="px-5 py-3.5 text-right text-xs font-bold text-gray-500 uppercase tracking-wider">Tổng SL</th>
              <th class="px-5 py-3.5 text-right text-xs font-bold text-gray-500 uppercase tracking-wider">Tổng giá trị</th>
              <th class="px-5 py-3.5 text-center text-xs font-bold text-gray-500 uppercase tracking-wider">Trạng thái</th>
              <th class="px-5 py-3.5 text-right text-xs font-bold text-gray-500 uppercase tracking-wider">Thao tác</th>
            </tr>
          </thead>
          <tbody class="divide-y divide-gray-100 bg-white">
            <tr v-if="filteredReceipts.length === 0">
              <td colspan="7" class="px-6 py-16 text-center">
                <DocumentArrowUpIcon class="w-12 h-12 text-gray-300 mx-auto mb-3" />
                <h3 class="text-base font-semibold text-gray-700">Chưa có Phiếu Xuất nào</h3>
                <p class="text-sm text-gray-500 mt-1">Bấm "Lập Phiếu Xuất" để bắt đầu xuất kho.</p>
              </td>
            </tr>

            <tr v-for="receipt in filteredReceipts" :key="receipt.id" class="hover:bg-gray-50 transition-colors">
              <td class="px-5 py-3 text-sm font-bold text-primary-700">{{ receipt.code }}</td>
              <td class="px-5 py-3 text-sm font-medium text-gray-600">{{ receipt.date }}</td>
              <td class="px-5 py-3 text-sm font-bold text-gray-800">{{ receipt.customerName }}</td>
              <td class="px-5 py-3 text-sm text-right font-medium text-gray-900">{{ receipt.totalQty }}</td>
              <td class="px-5 py-3 text-sm text-right font-bold text-emerald-600">{{ formatCurrency(receipt.totalPrice) }}</td>
              <td class="px-5 py-3 text-center">
                <span :class="['text-[10px] font-bold px-2 py-1 rounded border uppercase tracking-wider', getStatusBadge(receipt.status).class]">
                  {{ getStatusBadge(receipt.status).text }}
                </span>
              </td>
              
              <td class="px-5 py-3 text-right whitespace-nowrap">
                <div class="flex items-center justify-end gap-1.5">
                  <button @click.stop="() => alert('Đang tải file Hóa Đơn PDF...')" class="p-1.5 text-red-600 hover:bg-red-50 rounded border border-transparent hover:border-red-100 transition-colors" title="Tải PDF Hóa Đơn">
                    <ArrowDownTrayIcon class="w-4 h-4" />
                  </button>
                  <button @click.stop="() => alert('Đang kết nối máy in...')" class="p-1.5 text-gray-600 hover:bg-gray-100 rounded border border-transparent hover:border-gray-200 transition-colors" title="In trực tiếp">
                    <PrinterIcon class="w-4 h-4" />
                  </button>
                  <button @click="openModal('view', receipt)" class="px-2 py-1.5 text-blue-600 hover:bg-blue-50 rounded bg-blue-50 border border-blue-100 font-medium text-xs flex items-center gap-1 transition-colors">
                    <EyeIcon class="w-4 h-4" /> Chi tiết
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
        <div class="bg-white rounded-xl shadow-2xl w-full max-w-4xl overflow-hidden flex flex-col max-h-[90vh]">
          
          <div class="px-6 py-4 border-b border-gray-100 flex items-center justify-between bg-gray-50 shrink-0">
            <h3 class="text-lg font-bold text-gray-800 flex items-center gap-2">
              <DocumentArrowUpIcon class="w-6 h-6 text-primary-600"/> 
              {{ modalMode === 'add' ? 'Lập Phiếu Xuất Kho' : `Chi tiết Phiếu: ${formData.code}` }}
            </h3>
            <button @click="closeModal" class="text-gray-400 hover:text-red-500"><XMarkIcon class="w-6 h-6" /></button>
          </div>

          <div class="p-6 overflow-y-auto flex-1 custom-scrollbar">
            <form id="outboundForm" @submit.prevent="handleSubmit" class="space-y-6">
              
              <div class="bg-slate-50 p-4 rounded-lg border border-slate-200">
                <h4 class="text-sm font-bold text-slate-800 mb-4 border-b border-slate-200 pb-2">Thông tin chứng từ</h4>
                <div class="grid grid-cols-1 sm:grid-cols-3 gap-4">
                  <div>
                    <label class="block text-xs font-bold text-gray-700 mb-1">Mã Phiếu</label>
                    <input v-model="formData.code" type="text" disabled class="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm bg-gray-100 italic" placeholder="Tự động sinh">
                  </div>
                  <div>
                    <label class="block text-xs font-bold text-gray-700 mb-1">Ngày xuất <span class="text-red-500">*</span></label>
                    <input v-model="formData.date" :disabled="modalMode === 'view'" type="date" required class="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm focus:ring-primary-500 disabled:bg-gray-100">
                  </div>
                  <div>
                    <label class="block text-xs font-bold text-gray-700 mb-1">Trạng thái</label>
                    <select v-model="formData.status" disabled class="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm font-semibold bg-gray-100 disabled:text-gray-600" :class="getStatusBadge(formData.status).class">
                      <option value="pending">Chờ duyệt</option>
                      <option value="shipping">Đang giao hàng</option>
                      <option value="completed">Đã giao thành công</option>
                      <option value="rejected">Bị Từ chối</option>
                    </select>
                  </div>
                </div>

                <div class="mt-4">
                  <label class="block text-xs font-bold text-gray-700 mb-1">Khách hàng nhận <span class="text-red-500">*</span></label>
                  <select v-model="formData.customerId" :disabled="modalMode === 'view'" required class="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm focus:ring-primary-500 disabled:bg-gray-100 cursor-pointer">
                    <option value="" disabled>-- Chọn Khách hàng --</option>
                    <option v-for="cus in mockCustomers" :key="cus.id" :value="cus.id">
                      [{{ cus.id }}] {{ cus.name }}
                    </option>
                  </select>
                </div>
              </div>

              <div>
                <h4 class="text-sm font-bold text-slate-800 mb-2">Danh sách Hàng hóa Xuất kho</h4>
                <div v-if="modalMode === 'add'" class="flex flex-col sm:flex-row gap-2 mb-3 bg-blue-50 p-3 rounded-lg border border-blue-100">
                  <select v-model="selectedSkuToAdd" class="flex-1 border border-gray-300 rounded-lg px-3 py-2 text-sm focus:ring-primary-500 cursor-pointer">
                    <option value="" disabled>-- Chọn Sản phẩm để xuất kho --</option>
                    <option v-for="prod in mockProducts" :key="prod.sku" :value="prod.sku">
                      {{ prod.sku }} - {{ prod.name }}
                    </option>
                  </select>
                  <button type="button" @click="handleAddItem" :disabled="!selectedSkuToAdd" class="bg-blue-600 hover:bg-blue-700 text-white px-4 py-2 rounded-lg text-sm font-bold disabled:opacity-50 transition-colors whitespace-nowrap">
                    Thêm vào Lưới
                  </button>
                </div>

                <div class="border border-gray-200 rounded-lg overflow-x-auto">
                  <table class="w-full text-sm text-left">
                    <thead class="bg-gray-50 text-xs uppercase font-bold text-gray-500">
                      <tr>
                        <th class="px-4 py-3">SKU</th>
                        <th class="px-4 py-3">Tên Hàng</th>
                        <th class="px-4 py-3 text-center">ĐVT</th>
                        <th class="px-4 py-3 text-right">SL</th>
                        <th class="px-4 py-3 text-right">Đơn giá</th>
                        <th class="px-4 py-3 text-right">Thành tiền</th>
                        <th v-if="modalMode === 'add'" class="px-4 py-3 text-center">#</th>
                      </tr>
                    </thead>
                    <tbody class="divide-y divide-gray-100">
                      <tr v-if="formData.items.length === 0">
                        <td :colspan="modalMode === 'add' ? 7 : 6" class="px-4 py-8 text-center text-gray-400 italic">
                          Chưa có mặt hàng nào trong phiếu.
                        </td>
                      </tr>
                      <tr v-for="(item, idx) in formData.items" :key="idx" class="hover:bg-gray-50">
                        <td class="px-4 py-2 font-bold text-gray-700">{{ item.sku }}</td>
                        <td class="px-4 py-2 font-medium">{{ item.name }}</td>
                        <td class="px-4 py-2 text-center text-gray-500">{{ item.unit }}</td>
                        <td class="px-4 py-2 text-right">
                          <input v-if="modalMode === 'add'" v-model.number="item.qty" type="number" min="1" class="w-full text-right border border-gray-300 rounded px-2 py-1 font-bold text-blue-700 focus:ring-1 focus:ring-blue-500 outline-none">
                          <span v-else class="font-bold text-blue-700">{{ item.qty }}</span>
                        </td>
                        <td class="px-4 py-2 text-right">
                          <input v-if="modalMode === 'add'" v-model.number="item.price" type="number" min="0" class="w-full text-right border border-gray-300 rounded px-2 py-1 outline-none focus:ring-1 focus:ring-blue-500">
                          <span v-else>{{ formatCurrency(item.price) }}</span>
                        </td>
                        <td class="px-4 py-2 text-right font-bold text-emerald-600">
                          {{ formatCurrency(item.qty * item.price) }}
                        </td>
                        <td v-if="modalMode === 'add'" class="px-4 py-2 text-center">
                          <button type="button" @click="removeItem(idx)" class="text-red-400 hover:text-red-600"><TrashIcon class="w-5 h-5 mx-auto"/></button>
                        </td>
                      </tr>
                    </tbody>
                    <tfoot class="bg-gray-50 font-bold border-t border-gray-200">
                      <tr>
                        <td colspan="3" class="px-4 py-3 text-right uppercase text-gray-600">Tổng cộng:</td>
                        <td class="px-4 py-3 text-right text-blue-700 text-base">{{ totalQty }}</td>
                        <td class="px-4 py-3"></td>
                        <td class="px-4 py-3 text-right text-emerald-600 text-base">{{ formatCurrency(totalPrice) }}</td>
                        <td v-if="modalMode === 'add'"></td>
                      </tr>
                    </tfoot>
                  </table>
                </div>
              </div>

              <div>
                <label class="block text-xs font-bold text-gray-700 mb-1">Ghi chú phiếu xuất</label>
                <textarea v-model="formData.note" :disabled="modalMode === 'view'" rows="2" class="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm focus:ring-primary-500 disabled:bg-gray-100" placeholder="VD: Giao nhanh..."></textarea>
              </div>

            </form>
          </div>

          <div class="px-6 py-4 border-t border-gray-100 flex flex-col sm:flex-row justify-between items-center gap-3 bg-white shrink-0">
            
            <div class="flex gap-2 w-full sm:w-auto">
              <button v-if="modalMode === 'view'" @click="handlePrintReceipt" class="flex-1 sm:flex-none px-4 py-2.5 bg-slate-100 hover:bg-slate-200 text-slate-700 rounded-lg text-sm font-bold transition-colors flex items-center justify-center gap-2 border border-slate-200 shadow-sm">
                <PrinterIcon class="w-5 h-5"/> In Phiếu
              </button>
              <button v-if="modalMode === 'view'" @click="handlePrintInvoice" class="flex-1 sm:flex-none px-4 py-2.5 bg-slate-100 hover:bg-slate-200 text-slate-700 rounded-lg text-sm font-bold transition-colors flex items-center justify-center gap-2 border border-slate-200 shadow-sm">
                <DocumentTextIcon class="w-5 h-5"/> In Hóa Đơn
              </button>
            </div>

            <div class="flex gap-2 w-full sm:w-auto">
              <template v-if="modalMode === 'view' && formData.status === 'pending' && canApprove">
                <button @click="handleApprove('rejected')" class="px-4 py-2.5 bg-red-50 hover:bg-red-100 text-red-600 border border-red-200 rounded-lg text-sm font-bold flex items-center gap-1.5 transition-colors">
                  <XCircleIcon class="w-5 h-5"/> Từ chối
                </button>
                <button @click="handleApprove('shipping')" class="px-4 py-2.5 bg-emerald-600 hover:bg-emerald-700 text-white rounded-lg text-sm font-bold flex items-center gap-1.5 shadow-sm transition-colors">
                  <CheckCircleIcon class="w-5 h-5"/> Duyệt Xuất Kho
                </button>
              </template>

              <button type="button" @click="closeModal" class="flex-1 sm:flex-none px-5 py-2.5 border border-gray-300 text-gray-700 rounded-lg text-sm font-semibold hover:bg-gray-50 transition-colors text-center">
                {{ modalMode === 'view' ? 'Đóng lại' : 'Hủy bỏ' }}
              </button>
              <button v-if="modalMode === 'add'" type="submit" form="outboundForm" class="flex-1 sm:flex-none px-5 py-2.5 bg-primary-600 text-white rounded-lg text-sm font-bold hover:bg-primary-700 shadow-md transition-colors flex items-center justify-center gap-2">
                <DocumentArrowUpIcon class="w-5 h-5"/> Hoàn tất Phiếu
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
</style>