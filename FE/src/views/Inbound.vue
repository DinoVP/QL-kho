<script setup>
import { ref, computed } from 'vue'
import { 
  MagnifyingGlassIcon, PlusIcon, EyeIcon, 
  XMarkIcon, DocumentArrowDownIcon, TrashIcon,
  PrinterIcon, DocumentTextIcon
} from '@heroicons/vue/24/outline'

// === 1. STATE CHÍNH: TRỐNG CHỜ API ===
const inboundReceipts = ref([])

// === 2. DANH MỤC BỔ TRỢ (CHỜ API ĐỔ VÀO ĐỂ LẬP PHIẾU) ===
const mockSuppliers = ref([])
const mockProducts = ref([])

// === 3. BỘ LỌC TÌM KIẾM ===
const searchQuery = ref('')
const filterStatus = ref('')

const filteredReceipts = computed(() => {
  return inboundReceipts.value.filter(r => {
    const matchSearch = r.code.toLowerCase().includes(searchQuery.value.toLowerCase()) || 
                        (r.supplierName && r.supplierName.toLowerCase().includes(searchQuery.value.toLowerCase()))
    const matchStatus = filterStatus.value === '' || r.status === filterStatus.value
    return matchSearch && matchStatus
  }).sort((a, b) => b.id - a.id) 
})

// === 4. LOGIC MODAL TẠO / XEM PHIẾU ===
const showModal = ref(false)
const modalMode = ref('add') 

const formData = ref({ 
  id: null, code: '', date: '', supplierId: '', 
  items: [], note: '', status: 'pending' 
})

const generateReceiptCode = () => `PN${(inboundReceipts.value.length + 1).toString().padStart(4, '0')}`
const getToday = () => new Date().toISOString().split('T')[0]

const openModal = (mode, receipt = null) => {
  modalMode.value = mode
  if (receipt) {
    formData.value = JSON.parse(JSON.stringify(receipt)) 
  } else {
    formData.value = { id: null, code: '', date: getToday(), supplierId: '', items: [], note: '', status: 'pending' }
  }
  showModal.value = true
}

const closeModal = () => showModal.value = false

const getStatusBadge = (status) => {
  switch(status) {
    case 'pending': return { text: 'Chờ duyệt', class: 'bg-amber-100 text-amber-700 border-amber-200' }
    case 'approved': return { text: 'Đã duyệt', class: 'bg-blue-100 text-blue-700 border-blue-200' }
    case 'delivered': return { text: 'Đã giao tới', class: 'bg-emerald-100 text-emerald-700 border-emerald-200' }
    default: return { text: 'Khác', class: 'bg-gray-100 text-gray-500' }
  }
}

// === 5. LOGIC CHỌN HÀNG ===
const selectedSkuToAdd = ref('')

const availableProducts = computed(() => {
  if (!formData.value.supplierId) return []
  const ncc = mockSuppliers.value.find(s => s.id === formData.value.supplierId)
  if (!ncc || !ncc.assignedSkus) return []
  return mockProducts.value.filter(p => ncc.assignedSkus.includes(p.sku))
})

const handleSupplierChange = () => {
  formData.value.items = []
  selectedSkuToAdd.value = ''
}

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

// === 6. LOGIC TÍNH TOÁN & LƯU & IN ẤN ===
const totalQty = computed(() => formData.value.items.reduce((sum, item) => sum + item.qty, 0))
const totalPrice = computed(() => formData.value.items.reduce((sum, item) => sum + (item.qty * item.price), 0))
const formatCurrency = (val) => new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(val)

const handleSubmit = () => {
  if (formData.value.items.length === 0) return alert('Sếp chưa chọn mặt hàng nào cho phiếu nhập!')
  if (modalMode.value === 'add') {
    inboundReceipts.value.push({
      ...formData.value, id: Date.now(), code: generateReceiptCode(),
      totalQty: totalQty.value, totalPrice: totalPrice.value,
      supplierName: mockSuppliers.value.find(s => s.id === formData.value.supplierId)?.name || 'Chưa rõ'
    })
    alert('Tạo Phiếu Nhập Kho thành công!')
  }
  closeModal()
}

const handlePrintReceipt = () => alert(`Đang xuất lệnh máy in: IN PHIẾU NHẬP KHO mã [${formData.value.code}]...`)
const handlePrintInvoice = () => alert(`Đang tải file PDF: HÓA ĐƠN GTGT cho mã [${formData.value.code}]...`)
</script>

<template>
  <div class="space-y-5 md:space-y-6 animate-fade-in pb-10 px-0 md:px-1 relative">
    <div class="flex flex-col sm:flex-row sm:items-center justify-between gap-4">
      <div>
        <h2 class="text-xl md:text-2xl font-bold text-gray-800">Phiếu Nhập kho (Inbound)</h2>
        <p class="text-xs md:text-sm text-gray-500 mt-1">Lập chứng từ nhập hàng hóa từ Nhà cung cấp vào kho</p>
      </div>
      <button @click="openModal('add')" class="bg-primary-600 hover:bg-primary-700 text-white px-4 py-2.5 rounded-lg flex items-center gap-2 text-sm font-semibold shadow-sm">
        <PlusIcon class="w-5 h-5" /> Lập Phiếu Nhập
      </button>
    </div>

    <div class="bg-white p-3 md:p-4 rounded-xl border border-gray-200 flex flex-col sm:flex-row items-center gap-3 md:gap-4 shadow-sm">
      <div class="relative w-full sm:flex-1">
        <div class="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none"><MagnifyingGlassIcon class="w-5 h-5 text-gray-400" /></div>
        <input v-model="searchQuery" type="text" class="block w-full pl-10 pr-3 py-2 border border-gray-200 rounded-lg text-sm outline-none focus:ring-1 focus:ring-primary-500" placeholder="Tìm theo mã phiếu, tên NCC...">
      </div>
      <select v-model="filterStatus" class="w-full sm:w-auto border border-gray-200 rounded-lg text-sm px-4 py-2 outline-none focus:ring-1 focus:ring-primary-500 cursor-pointer">
        <option value="">Tất cả Trạng thái</option>
        <option value="pending">Chờ duyệt</option>
        <option value="approved">Đã duyệt</option>
        <option value="delivered">Đã giao tới</option>
      </select>
    </div>

    <div class="bg-white rounded-xl border border-gray-200 shadow-sm overflow-hidden">
      <div class="w-full overflow-x-auto custom-scrollbar">
        <table class="min-w-[1000px] w-full divide-y divide-gray-200">
          <thead class="bg-gray-50">
            <tr>
              <th class="px-5 py-3.5 text-left text-xs font-bold text-gray-500 uppercase tracking-wider">Mã Phiếu</th>
              <th class="px-5 py-3.5 text-left text-xs font-bold text-gray-500 uppercase tracking-wider">Ngày nhập</th>
              <th class="px-5 py-3.5 text-left text-xs font-bold text-gray-500 uppercase tracking-wider">Nhà cung cấp</th>
              <th class="px-5 py-3.5 text-right text-xs font-bold text-gray-500 uppercase tracking-wider">Tổng SL</th>
              <th class="px-5 py-3.5 text-right text-xs font-bold text-gray-500 uppercase tracking-wider">Tổng giá trị</th>
              <th class="px-5 py-3.5 text-center text-xs font-bold text-gray-500 uppercase tracking-wider">Trạng thái</th>
              <th class="px-5 py-3.5 text-right text-xs font-bold text-gray-500 uppercase tracking-wider">Thao tác</th>
            </tr>
          </thead>
          <tbody class="divide-y divide-gray-100 bg-white">
            <tr v-if="filteredReceipts.length === 0">
              <td colspan="7" class="px-6 py-16 text-center">
                <DocumentArrowDownIcon class="w-12 h-12 text-gray-300 mx-auto mb-3" />
                <h3 class="text-base font-semibold text-gray-700">Chưa có Phiếu Nhập nào</h3>
                <p class="text-sm text-gray-500 mt-1">Bấm "Lập Phiếu Nhập" để bắt đầu đưa hàng vào kho.</p>
              </td>
            </tr>
            <tr v-for="receipt in filteredReceipts" :key="receipt.id" class="hover:bg-gray-50">
              <td class="px-5 py-3 text-sm font-bold text-primary-700">{{ receipt.code }}</td>
              <td class="px-5 py-3 text-sm font-medium text-gray-600">{{ receipt.date }}</td>
              <td class="px-5 py-3 text-sm font-bold text-gray-800">{{ receipt.supplierName }}</td>
              <td class="px-5 py-3 text-sm text-right font-medium text-gray-900">{{ receipt.totalQty }}</td>
              <td class="px-5 py-3 text-sm text-right font-bold text-emerald-600">{{ formatCurrency(receipt.totalPrice) }}</td>
              <td class="px-5 py-3 text-center"><span :class="['text-[10px] font-bold px-2 py-1 rounded border uppercase', getStatusBadge(receipt.status).class]">{{ getStatusBadge(receipt.status).text }}</span></td>
              <td class="px-5 py-3 text-right">
                <button @click="openModal('view', receipt)" class="px-3 py-1.5 text-blue-600 hover:bg-blue-50 rounded bg-blue-50 border border-blue-100 font-medium text-xs flex items-center gap-1 ml-auto">
                  <EyeIcon class="w-4 h-4" /> Chi tiết
                </button>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>

    <Teleport to="body">
      <div v-if="showModal" class="fixed inset-0 z-50 flex items-center justify-center px-4 bg-slate-900/50 backdrop-blur-sm animate-fade-in">
        <div class="bg-white rounded-xl shadow-2xl w-full max-w-4xl overflow-hidden flex flex-col max-h-[90vh]">
          <div class="px-6 py-4 border-b flex items-center justify-between bg-gray-50 shrink-0">
            <h3 class="text-lg font-bold text-gray-800 flex items-center gap-2"><DocumentArrowDownIcon class="w-6 h-6 text-primary-600"/> {{ modalMode === 'add' ? 'Lập Phiếu Nhập Kho' : `Chi tiết: ${formData.code}` }}</h3>
            <button @click="closeModal" class="text-gray-400 hover:text-red-500"><XMarkIcon class="w-6 h-6" /></button>
          </div>
          <div class="p-6 overflow-y-auto flex-1 custom-scrollbar">
            <form id="inboundForm" @submit.prevent="handleSubmit" class="space-y-6">
              <div class="bg-slate-50 p-4 rounded-lg border border-slate-200">
                <h4 class="text-sm font-bold text-slate-800 mb-4 border-b pb-2">Thông tin chứng từ</h4>
                <div class="grid grid-cols-1 sm:grid-cols-3 gap-4">
                  <div><label class="block text-xs font-bold mb-1">Mã Phiếu</label><input v-model="formData.code" type="text" disabled class="w-full border rounded-lg px-3 py-2 text-sm bg-gray-100 italic" placeholder="Tự động sinh"></div>
                  <div><label class="block text-xs font-bold mb-1">Ngày lập *</label><input v-model="formData.date" :disabled="modalMode === 'view'" type="date" required class="w-full border rounded-lg px-3 py-2 text-sm focus:ring-primary-500 disabled:bg-gray-100"></div>
                  <div><label class="block text-xs font-bold mb-1">Trạng thái</label>
                    <select v-model="formData.status" :disabled="modalMode === 'view'" class="w-full border rounded-lg px-3 py-2 text-sm font-semibold disabled:bg-gray-100" :class="getStatusBadge(formData.status).class">
                      <option value="pending">Chờ duyệt</option><option value="approved">Đã duyệt</option><option value="delivered">Đã giao tới</option>
                    </select>
                  </div>
                </div>
                <div class="mt-4">
                  <label class="block text-xs font-bold mb-1">Nhà cung cấp (Đối tác) *</label>
                  <select v-model="formData.supplierId" @change="handleSupplierChange" :disabled="modalMode === 'view'" required class="w-full border rounded-lg px-3 py-2 text-sm focus:ring-primary-500 disabled:bg-gray-100 cursor-pointer">
                    <option value="" disabled>-- Chọn Nhà cung cấp --</option>
                    <option v-for="ncc in mockSuppliers" :key="ncc.id" :value="ncc.id">[{{ ncc.id }}] {{ ncc.name }}</option>
                  </select>
                </div>
              </div>

              <div>
                <h4 class="text-sm font-bold mb-2">Danh sách Hàng hóa Nhập</h4>
                <div v-if="modalMode === 'add'" class="flex flex-col sm:flex-row gap-2 mb-3 bg-blue-50 p-3 rounded-lg border border-blue-100">
                  <div v-if="!formData.supplierId" class="text-sm text-blue-700 italic font-medium w-full text-center">Vui lòng chọn NCC để hiện hàng hóa.</div>
                  <template v-else>
                    <select v-model="selectedSkuToAdd" class="flex-1 border rounded-lg px-3 py-2 text-sm focus:ring-primary-500 cursor-pointer">
                      <option value="" disabled>-- Chọn Sản phẩm --</option>
                      <option v-for="prod in availableProducts" :key="prod.sku" :value="prod.sku">{{ prod.sku }} - {{ prod.name }}</option>
                    </select>
                    <button type="button" @click="handleAddItem" :disabled="!selectedSkuToAdd" class="bg-blue-600 hover:bg-blue-700 text-white px-4 py-2 rounded-lg text-sm font-bold disabled:opacity-50">Thêm vào Lưới</button>
                  </template>
                </div>

                <div class="border rounded-lg overflow-x-auto">
                  <table class="w-full text-sm text-left">
                    <thead class="bg-gray-50 text-xs uppercase font-bold text-gray-500">
                      <tr><th class="px-4 py-3">SKU</th><th class="px-4 py-3">Tên Hàng</th><th class="px-4 py-3 text-center">ĐVT</th><th class="px-4 py-3 text-right">SL</th><th class="px-4 py-3 text-right">Đơn giá</th><th class="px-4 py-3 text-right">Thành tiền</th><th v-if="modalMode === 'add'" class="px-4 py-3 text-center">#</th></tr>
                    </thead>
                    <tbody class="divide-y divide-gray-100">
                      <tr v-if="formData.items.length === 0"><td :colspan="modalMode === 'add' ? 7 : 6" class="px-4 py-8 text-center text-gray-400 italic">Chưa có mặt hàng nào.</td></tr>
                      <tr v-for="(item, idx) in formData.items" :key="idx" class="hover:bg-gray-50">
                        <td class="px-4 py-2 font-bold">{{ item.sku }}</td><td class="px-4 py-2">{{ item.name }}</td><td class="px-4 py-2 text-center">{{ item.unit }}</td>
                        <td class="px-4 py-2 text-right"><input v-if="modalMode === 'add'" v-model.number="item.qty" type="number" min="1" class="w-full text-right border rounded px-2 py-1 font-bold text-blue-700"><span v-else class="font-bold text-blue-700">{{ item.qty }}</span></td>
                        <td class="px-4 py-2 text-right"><input v-if="modalMode === 'add'" v-model.number="item.price" type="number" min="0" class="w-full text-right border rounded px-2 py-1"><span v-else>{{ formatCurrency(item.price) }}</span></td>
                        <td class="px-4 py-2 text-right font-bold text-emerald-600">{{ formatCurrency(item.qty * item.price) }}</td>
                        <td v-if="modalMode === 'add'" class="px-4 py-2 text-center"><button type="button" @click="removeItem(idx)" class="text-red-400 hover:text-red-600"><TrashIcon class="w-5 h-5 mx-auto"/></button></td>
                      </tr>
                    </tbody>
                    <tfoot class="bg-gray-50 font-bold border-t"><td colspan="3" class="px-4 py-3 text-right uppercase">Tổng cộng:</td><td class="px-4 py-3 text-right text-blue-700">{{ totalQty }}</td><td></td><td class="px-4 py-3 text-right text-emerald-600">{{ formatCurrency(totalPrice) }}</td><td v-if="modalMode === 'add'"></td></tfoot>
                  </table>
                </div>
              </div>

              <div>
                <label class="block text-xs font-bold mb-1">Ghi chú</label>
                <textarea v-model="formData.note" :disabled="modalMode === 'view'" rows="2" class="w-full border rounded-lg px-3 py-2 text-sm focus:ring-primary-500 disabled:bg-gray-100" placeholder="..."></textarea>
              </div>
            </form>
          </div>

          <div class="px-6 py-4 border-t flex flex-col sm:flex-row justify-between gap-3 shrink-0">
            <div class="flex gap-2 w-full sm:w-auto">
              <button v-if="modalMode === 'view'" @click="handlePrintReceipt" class="flex-1 sm:flex-none px-4 py-2.5 bg-slate-100 hover:bg-slate-200 text-slate-700 rounded-lg text-sm font-bold border"><PrinterIcon class="w-5 h-5 inline mr-1"/> In Phiếu</button>
              <button v-if="modalMode === 'view'" @click="handlePrintInvoice" class="flex-1 sm:flex-none px-4 py-2.5 bg-slate-100 hover:bg-slate-200 text-slate-700 rounded-lg text-sm font-bold border"><DocumentTextIcon class="w-5 h-5 inline mr-1"/> In Hóa Đơn</button>
            </div>
            <div class="flex gap-2 w-full sm:w-auto">
              <button type="button" @click="closeModal" class="flex-1 sm:flex-none px-5 py-2.5 border rounded-lg text-sm font-semibold hover:bg-gray-50">{{ modalMode === 'view' ? 'Đóng lại' : 'Hủy bỏ' }}</button>
              <button v-if="modalMode === 'add'" type="submit" form="inboundForm" class="flex-1 sm:flex-none px-5 py-2.5 bg-primary-600 text-white rounded-lg text-sm font-bold hover:bg-primary-700 shadow-md"><DocumentArrowDownIcon class="w-5 h-5 inline mr-1"/> Hoàn tất Phiếu</button>
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