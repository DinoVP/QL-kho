<script setup>
import { ref, computed, onMounted } from 'vue'
import { 
  MagnifyingGlassIcon, PlusIcon, EyeIcon, 
  XMarkIcon, ClipboardDocumentCheckIcon, TrashIcon, PencilSquareIcon,
  DocumentArrowUpIcon, DocumentTextIcon, ArrowDownTrayIcon, PrinterIcon, CheckCircleIcon
} from '@heroicons/vue/24/outline'

const CHECK_API = 'https://localhost:7139/api/InvCheck'
const STOCK_API = 'https://localhost:7139/api/Stock'
const PROD_API = 'https://localhost:7139/api/Products'
const BRANCH_API = 'https://localhost:7139/api/Branches'

const getAuthHeaders = () => ({ 'Content-Type': 'application/json', 'Authorization': 'Bearer ' + (localStorage.getItem('authToken') || '') })

const currentUserRole = ref(localStorage.getItem('role') || 'ql_kho') 
const myWarehouseId = ref(parseInt(localStorage.getItem('warehouseId')) || 1) // Mặc định kho 1 nếu mất

const canChotSo = computed(() => ['admin', 'giam_doc', 'ql_kho'].includes(currentUserRole.value.toLowerCase()))
const canExport = computed(() => ['admin', 'giam_doc', 'gd_chi_nhanh', 'ql_kho'].includes(currentUserRole.value.toLowerCase()))

const inventoryChecks = ref([])
const stockList = ref([]) // Lưu tồn thực tế của kho
const productsList = ref([])
const myWarehouseName = ref('Kho của tôi')
const isLoading = ref(false)

const getToday = () => new Date().toISOString().split('T')[0]

const fetchData = async () => {
  isLoading.value = true
  try {
    const headers = getAuthHeaders()
    
    // Lấy tên Kho hiện tại
    try {
        const branchRes = await fetch(BRANCH_API, { headers })
        if (branchRes.ok) {
            const branches = await branchRes.json()
            for (const b of branches) {
                const bId = b.id || b.Id;
                const whRes = await fetch(`${BRANCH_API}/${bId}/warehouses-detail`, { headers })
                if(whRes.ok) {
                    const whData = await whRes.json()
                    const myWh = whData.find(w => w.warehouseId === myWarehouseId.value);
                    if(myWh) myWarehouseName.value = myWh.whname;
                }
            }
        }
    } catch(e) { console.error("Lỗi lấy tên Kho") }

    const [checkRes, stockRes, prodRes] = await Promise.all([
      fetch(CHECK_API, { headers }), fetch(STOCK_API, { headers }), fetch(PROD_API, { headers })
    ])
    
    if (prodRes.ok) productsList.value = await prodRes.json()
    
    if (stockRes.ok) {
      const rawStocks = await stockRes.json()
      // Gộp tồn kho của TẤT CẢ vị trí trong kho này thành 1 cục tổng để kiểm kê
      const grouped = {};
      rawStocks.filter(s => s.warehouseId === myWarehouseId.value).forEach(s => {
          const vId = s.variantId || s.VariantId;
          if (!grouped[vId]) grouped[vId] = { variantId: vId, qty: 0 };
          grouped[vId].qty += s.qty || s.Quantity || 0;
      });
      stockList.value = Object.values(grouped).map(s => {
          const prod = productsList.value.find(p => p.id === s.variantId || p.Id === s.variantId) || {};
          return {
              variantId: s.variantId, sku: prod.sku || prod.Sku, name: prod.name || prod.Name, 
              unit: prod.unit || prod.Unit || 'Thùng', systemQty: s.qty
          }
      });
    }

    if (checkRes.ok) {
        inventoryChecks.value = await checkRes.json();
    }
  } catch (error) { console.error('Lỗi tải dữ liệu:', error) }
  finally { isLoading.value = false }
}

const searchQuery = ref(''); const filterStatus = ref('')

const filteredChecks = computed(() => {
  return inventoryChecks.value.filter(c => {
    const matchSearch = c.code.toLowerCase().includes(searchQuery.value.toLowerCase())
    const matchStatus = filterStatus.value === '' || c.status === filterStatus.value
    // Chỉ xem phiếu của kho mình
    const matchWh = c.warehouseId === myWarehouseId.value
    return matchSearch && matchStatus && matchWh
  }).sort((a, b) => b.id - a.id)
})

const showModal = ref(false); const modalMode = ref('add') 
const formData = ref({ id: 0, code: '', date: getToday(), warehouseId: myWarehouseId.value, items: [], note: '', status: 'draft' })

const openModal = (mode, check = null) => {
  modalMode.value = mode
  if (check) {
      const mappedItems = (check.items || []).map(i => {
          const prod = productsList.value.find(p => p.id === i.variantId || p.Id === i.variantId) || {};
          return {
              variantId: i.variantId, sku: prod.sku || prod.Sku, name: prod.name || prod.Name, 
              unit: prod.unit || prod.Unit || 'Thùng', 
              systemQty: i.systemQty || 0, actualQty: i.actualQty || 0, diff: i.diffQty || 0, reason: i.reason || ''
          }
      })
      formData.value = { ...check, items: mappedItems }
  } 
  else formData.value = { id: 0, code: '', date: getToday(), warehouseId: myWarehouseId.value, items: [], note: '', status: 'draft' }
  selectedVariantIdToAdd.value = ''
  showModal.value = true
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

// === 5. LOGIC CHỌN HÀNG (CHỈ CHỌN HÀNG ĐANG CÓ TRONG KHO) ===
const selectedVariantIdToAdd = ref('')

const handleAddItem = () => {
  if (!selectedVariantIdToAdd.value) return
  const existingItem = formData.value.items.find(i => i.variantId === selectedVariantIdToAdd.value)
  if (!existingItem) {
    const stock = stockList.value.find(s => s.variantId === selectedVariantIdToAdd.value)
    if (stock) {
      formData.value.items.push({ 
        variantId: stock.variantId, sku: stock.sku, name: stock.name, unit: stock.unit, 
        systemQty: stock.systemQty, actualQty: stock.systemQty, 
        diff: 0, reason: '' 
      })
    }
  }
  selectedVariantIdToAdd.value = '' 
}

const removeItem = (index) => formData.value.items.splice(index, 1)

const updateDiff = (item) => { item.diff = item.actualQty - item.systemQty }

const handleSubmit = async () => {
  if (formData.value.items.length === 0) return alert('Chưa chọn mặt hàng nào để kiểm kê!')
  
  try {
      formData.value.warehouseId = myWarehouseId.value; // Chống hack đổi kho
      const payload = { ...formData.value, code: formData.value.code || "" };
      const method = modalMode.value === 'add' ? 'POST' : 'PUT';
      const url = modalMode.value === 'add' ? CHECK_API : `${CHECK_API}/${formData.value.id}`;

      const res = await fetch(url, { method, headers: getAuthHeaders(), body: JSON.stringify(payload) })
      if (res.ok) { 
          alert(modalMode.value === 'add' ? 'Lưu Phiếu Kiểm Kê thành công!' : 'Cập nhật thành công!'); 
          await fetchData(); 
          closeModal(); 
      } else { 
          let errMsg = "Lỗi hệ thống!"; try { const t = await res.text(); if(t) errMsg = JSON.parse(t).message || errMsg; } catch(e){} alert('LỖI: ' + errMsg); 
      }
  } catch(e) { console.error(e) }
}

const handleChotSo = async () => {
  if(!confirm(`XÁC NHẬN CHỐT SỔ TỒN KHO phiếu ${formData.value.code}?\nThao tác này sẽ cập nhật thẳng vào Database kho thực tế.`)) return;
  try {
      const res = await fetch(`${CHECK_API}/${formData.value.id}/complete`, { method: 'PUT', headers: getAuthHeaders() })
      if(res.ok) { alert('Đã chốt sổ thành công!'); await fetchData(); closeModal(); }
      else { let errMsg = "Lỗi hệ thống!"; try { const t = await res.text(); if(t) errMsg = JSON.parse(t).message || errMsg; } catch(e){} alert('LỖI: ' + errMsg); }
  } catch(e) { console.error(e) }
}

onMounted(() => fetchData())
</script>

<template>
  <div class="space-y-5 md:space-y-6 animate-fade-in pb-10 px-0 md:px-1 relative">
    
    <div class="flex flex-col sm:flex-row sm:items-center justify-between gap-4">
      <div>
        <h2 class="text-xl md:text-2xl font-bold text-gray-800">Kiểm Kê Kho</h2>
        <p class="text-xs md:text-sm text-gray-500 mt-1">Đối chiếu số lượng phần mềm với hàng tồn thực tế tại kho</p>
      </div>
      <div class="flex flex-wrap items-center gap-2">
        <button v-if="canExport" @click="() => alert('Tính năng đang phát triển...')" class="bg-white border border-blue-200 text-blue-700 px-3 py-2.5 rounded-lg text-sm font-semibold hover:bg-blue-50 transition-colors shadow-sm flex items-center gap-1.5"><DocumentArrowUpIcon class="w-4 h-4"/> Import Excel</button>
        <button v-if="canExport" @click="() => alert('Tính năng đang phát triển...')" class="bg-white border border-red-200 text-red-700 px-3 py-2.5 rounded-lg text-sm font-semibold hover:bg-red-50 transition-colors shadow-sm flex items-center gap-1.5"><ArrowDownTrayIcon class="w-4 h-4"/> Xuất PDF</button>
        <button @click="openModal('add')" class="bg-teal-600 hover:bg-teal-700 text-white px-4 py-2.5 rounded-lg flex items-center gap-2 text-sm font-semibold shadow-sm transition-colors"><PlusIcon class="w-5 h-5" /> Lập Phiếu Kiểm Kê</button>
      </div>
    </div>

    <div class="bg-white p-3 md:p-4 rounded-xl border border-gray-200 flex flex-col sm:flex-row items-center gap-3 md:gap-4 shadow-sm">
      <div class="relative w-full sm:flex-1">
        <div class="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none"><MagnifyingGlassIcon class="w-5 h-5 text-gray-400" /></div>
        <input v-model="searchQuery" type="text" class="block w-full pl-10 pr-3 py-2 border border-gray-200 rounded-lg text-sm outline-none focus:ring-1 focus:ring-teal-500" placeholder="Tìm theo mã phiếu kiểm kê...">
      </div>
      <select v-model="filterStatus" class="w-full sm:w-auto border border-gray-200 rounded-lg text-sm px-4 py-2 outline-none focus:ring-1 focus:ring-teal-500 cursor-pointer">
        <option value="">Tất cả Trạng thái</option>
        <option value="draft">Bản nháp</option><option value="checking">Đang kiểm kê</option><option value="completed">Đã chốt sổ</option>
      </select>
    </div>

    <div class="bg-white rounded-xl border border-gray-200 shadow-sm overflow-hidden">
      <div class="w-full overflow-x-auto custom-scrollbar">
        <table class="min-w-[1000px] w-full divide-y divide-gray-200">
          <thead class="bg-gray-50">
            <tr>
              <th class="px-5 py-3.5 text-left text-xs font-bold text-gray-500 uppercase tracking-wider">Mã Phiếu</th>
              <th class="px-5 py-3.5 text-left text-xs font-bold text-gray-500 uppercase tracking-wider">Ngày kiểm</th>
              <th class="px-5 py-3.5 text-left text-xs font-bold text-gray-500 uppercase tracking-wider">Kho được kiểm kê</th>
              <th class="px-5 py-3.5 text-center text-xs font-bold text-gray-500 uppercase tracking-wider">Số mặt hàng</th>
              <th class="px-5 py-3.5 text-center text-xs font-bold text-gray-500 uppercase tracking-wider">Trạng thái</th>
              <th class="px-5 py-3.5 text-right text-xs font-bold text-gray-500 uppercase tracking-wider">Thao tác</th>
            </tr>
          </thead>
          <tbody class="divide-y divide-gray-100 bg-white">
            <tr v-if="filteredChecks.length === 0">
              <td colspan="6" class="px-6 py-16 text-center">
                <ClipboardDocumentCheckIcon class="w-12 h-12 text-gray-300 mx-auto mb-3" />
                <h3 class="text-base font-semibold text-gray-700">Chưa có Phiếu Kiểm Kê nào</h3>
              </td>
            </tr>
            <tr v-for="check in filteredChecks" :key="check.id" class="hover:bg-gray-50 transition-colors">
              <td class="px-5 py-3 text-sm font-bold text-teal-700">{{ check.code }}</td>
              <td class="px-5 py-3 text-sm font-medium text-gray-600">{{ check.date }}</td>
              <td class="px-5 py-3 text-sm font-bold text-gray-800">Kho {{ check.warehouseId }}</td>
              <td class="px-5 py-3 text-sm text-center font-bold text-gray-900">{{ check.items?.length || 0 }} SKU</td>
              <td class="px-5 py-3 text-center"><span :class="['text-[10px] font-bold px-2 py-1 rounded border uppercase tracking-wider', getStatusBadge(check.status).class]">{{ getStatusBadge(check.status).text }}</span></td>
              
              <td class="px-5 py-3 text-right space-x-1.5 whitespace-nowrap">
                <button @click.stop="() => alert('Đang code...')" class="p-1.5 text-red-600 hover:bg-red-50 rounded border border-transparent hover:border-red-100" title="Tải PDF"><ArrowDownTrayIcon class="w-4 h-4" /></button>
                <button @click.stop="() => alert('Đang code...')" class="p-1.5 text-gray-600 hover:bg-gray-100 rounded border border-transparent hover:border-gray-200" title="In Phiếu Kiểm Đếm"><PrinterIcon class="w-4 h-4" /></button>
                
                <button v-if="check.status !== 'completed'" @click="openModal('edit', check)" class="p-1.5 text-amber-600 hover:bg-amber-50 rounded border border-transparent hover:border-amber-100" title="Sửa/Tiếp tục kiểm"><PencilSquareIcon class="w-4 h-4" /></button>
                <button @click="openModal('view', check)" class="p-1.5 text-teal-600 hover:bg-teal-50 rounded bg-teal-50 border border-teal-100" title="Chi tiết"><EyeIcon class="w-4 h-4" /></button>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>

    <Teleport to="body">
      <div v-if="showModal" class="fixed inset-0 z-50 flex items-center justify-center px-4 bg-slate-900/50 backdrop-blur-sm animate-fade-in">
        <div class="bg-white rounded-xl shadow-2xl w-full max-w-5xl overflow-hidden flex flex-col max-h-[90vh]">
          
          <div class="px-6 py-4 border-b border-teal-100 flex items-center justify-between bg-teal-50 shrink-0">
            <h3 class="text-lg font-bold text-teal-800 flex items-center gap-2"><ClipboardDocumentCheckIcon class="w-6 h-6 text-teal-600"/> {{ modalMode === 'add' ? 'Lập Phiếu Kiểm Kê' : `Chi tiết Phiếu: ${formData.code}` }}</h3>
            <button @click="closeModal" class="text-gray-400 hover:text-red-500"><XMarkIcon class="w-6 h-6" /></button>
          </div>
          
          <div class="p-6 overflow-y-auto flex-1 custom-scrollbar">
            <form id="checkForm" @submit.prevent="handleSubmit" class="space-y-6">
              
              <div class="bg-slate-50 p-4 rounded-lg border border-slate-200">
                <div class="grid grid-cols-1 md:grid-cols-4 gap-4">
                  <div><label class="block text-xs font-bold mb-1">Mã Phiếu</label><input type="text" disabled class="w-full border rounded-lg px-3 py-2 text-sm bg-gray-100 italic" :placeholder="modalMode === 'add' ? 'Hệ thống tự sinh' : formData.code"></div>
                  <div><label class="block text-xs font-bold mb-1">Ngày kiểm kê *</label><input v-model="formData.date" :disabled="modalMode === 'view'" type="date" required class="w-full border rounded-lg px-3 py-2 text-sm focus:ring-teal-500 disabled:bg-gray-100"></div>
                  
                  <div class="md:col-span-2">
                    <label class="block text-xs font-bold mb-1 text-teal-700">Kho đang kiểm kê</label>
                    <div class="w-full border border-teal-200 bg-teal-50 rounded-lg px-3 py-2 text-sm font-bold text-teal-800 cursor-not-allowed flex items-center">
                        <MapPinIcon class="w-4 h-4 mr-2" /> {{ myWarehouseName }} (Kho ID: {{ myWarehouseId }})
                    </div>
                  </div>
                </div>
              </div>

              <div>
                <div class="flex items-center justify-between mb-2">
                  <h4 class="text-sm font-bold text-gray-800">Danh sách Hàng hóa Kiểm đếm</h4>
                  <div class="text-sm font-bold flex items-center">Trạng thái: 
                    <select v-model="formData.status" disabled class="ml-2 border rounded px-2 py-1 text-sm font-bold bg-gray-100 cursor-not-allowed" :class="getStatusBadge(formData.status).class">
                      <option value="draft">Bản nháp</option><option value="checking">Đang kiểm kê</option><option value="completed">Đã chốt sổ</option>
                    </select>
                  </div>
                </div>

                <div v-if="modalMode !== 'view'" class="flex flex-col sm:flex-row gap-2 mb-3 bg-teal-50 p-3 rounded-lg border border-teal-100">
                  <select v-model="selectedVariantIdToAdd" class="flex-1 border rounded-lg px-3 py-2 text-sm focus:ring-teal-500 cursor-pointer">
                    <option value="" disabled>-- Chọn Sản phẩm có trong Kho --</option>
                    <option v-for="stock in stockList" :key="stock.variantId" :value="stock.variantId">[{{ stock.sku }}] {{ stock.name }} (Tồn PM: {{ stock.systemQty }} {{ stock.unit }})</option>
                  </select>
                  <button type="button" @click="handleAddItem" :disabled="!selectedVariantIdToAdd" class="bg-teal-600 hover:bg-teal-700 text-white px-4 py-2 rounded-lg text-sm font-bold disabled:opacity-50 transition-colors">Đưa vào Lưới</button>
                </div>

                <div class="border rounded-lg overflow-x-auto">
                  <table class="w-full text-sm text-left">
                    <thead class="bg-gray-50 text-xs uppercase font-bold text-gray-500">
                      <tr><th class="px-4 py-3">Mã SKU</th><th class="px-4 py-3">Tên Hàng</th><th class="px-4 py-3 text-center">Tồn Hệ Thống</th><th class="px-4 py-3 text-center w-32">Tồn Thực Tế</th><th class="px-4 py-3 text-center">Chênh lệch</th><th class="px-4 py-3 text-left w-1/4">Ghi chú / Lý do</th><th v-if="modalMode !== 'view'" class="px-4 py-3 text-center w-10">#</th></tr>
                    </thead>
                    <tbody class="divide-y divide-gray-100">
                      <tr v-if="formData.items.length === 0"><td :colspan="modalMode !== 'view' ? 7 : 6" class="px-4 py-8 text-center text-gray-400 italic">Chưa có mặt hàng nào. Vui lòng thêm sản phẩm để kiểm đếm.</td></tr>
                      <tr v-for="(item, idx) in formData.items" :key="idx" class="hover:bg-gray-50">
                        <td class="px-4 py-3 font-bold">{{ item.sku }}</td><td class="px-4 py-3">{{ item.name }}</td>
                        <td class="px-4 py-3 text-center font-bold text-gray-500 bg-gray-100">{{ item.systemQty }} {{ item.unit }}</td>
                        
                        <td class="px-4 py-3 text-center">
                          <input v-if="modalMode !== 'view'" v-model.number="item.actualQty" @input="updateDiff(item)" type="number" min="0" class="w-full text-center border rounded px-2 py-1.5 font-bold text-teal-700 focus:ring-teal-500 outline-none">
                          <span v-else class="font-bold text-teal-700">{{ item.actualQty }} {{ item.unit }}</span>
                        </td>
                        
                        <td class="px-4 py-3 text-center font-bold">
                          <span v-if="item.diff === 0" class="text-emerald-600 bg-emerald-50 px-2 py-1 rounded">Khớp (0)</span>
                          <span v-else-if="item.diff > 0" class="text-amber-600 bg-amber-50 px-2 py-1 rounded">Thừa (+{{ item.diff }})</span>
                          <span v-else class="text-red-600 bg-red-50 px-2 py-1 rounded">Thiếu ({{ item.diff }})</span>
                        </td>
                        <td class="px-4 py-3"><input v-if="modalMode !== 'view'" v-model="item.reason" type="text" class="w-full border rounded px-2 py-1.5 text-xs focus:ring-teal-500 outline-none"><span v-else class="text-xs">{{ item.reason || '—' }}</span></td>
                        <td v-if="modalMode !== 'view'" class="px-4 py-3 text-center"><button type="button" @click="removeItem(idx)" class="text-red-400 hover:text-red-600"><TrashIcon class="w-5 h-5 mx-auto"/></button></td>
                      </tr>
                    </tbody>
                  </table>
                </div>
              </div>
              
              <div><label class="block text-xs font-bold mb-1">Ghi chú tổng thể</label><textarea v-model="formData.note" :disabled="modalMode === 'view'" rows="2" class="w-full border rounded-lg px-3 py-2 text-sm focus:ring-teal-500 disabled:bg-gray-100 outline-none" placeholder="..."></textarea></div>
            </form>
          </div>

          <div class="px-6 py-4 border-t border-gray-100 flex flex-col sm:flex-row justify-between gap-3 bg-white shrink-0">
            <div class="flex gap-2 w-full sm:w-auto">
              <button v-if="modalMode === 'view' || modalMode === 'edit'" @click="() => alert('Đang code...')" class="flex-1 sm:flex-none px-4 py-2.5 bg-slate-100 hover:bg-slate-200 text-slate-700 rounded-lg text-sm font-bold border flex items-center justify-center gap-2"><PrinterIcon class="w-5 h-5"/> In Phiếu Kiểm Đếm</button>
            </div>
            
            <div class="flex gap-2 w-full sm:w-auto">
              <template v-if="modalMode === 'edit' && formData.status === 'draft'">
                <button @click="formData.status = 'checking'; handleSubmit()" class="px-4 py-2.5 bg-blue-100 text-blue-700 hover:bg-blue-200 border border-blue-200 rounded-lg text-sm font-bold flex items-center justify-center gap-2 shadow-sm transition-colors">
                  Bắt đầu kiểm kê
                </button>
              </template>
              
              <template v-if="modalMode === 'edit' && formData.status === 'checking' && canChotSo">
                <button @click="handleChotSo" class="px-4 py-2.5 bg-teal-600 hover:bg-teal-700 text-white rounded-lg text-sm font-bold flex items-center justify-center gap-2 shadow-sm transition-colors">
                  <CheckCircleIcon class="w-5 h-5"/> Chốt Sổ Kiểm Kê
                </button>
              </template>

              <button type="button" @click="closeModal" class="flex-1 sm:flex-none px-5 py-2.5 border border-gray-300 text-gray-700 rounded-lg text-sm font-semibold hover:bg-gray-50 transition-colors text-center">{{ modalMode === 'view' ? 'Đóng' : 'Hủy bỏ' }}</button>
              <button v-if="modalMode !== 'view'" type="submit" form="checkForm" class="flex-1 sm:flex-none px-5 py-2.5 bg-slate-800 text-white rounded-lg text-sm font-bold hover:bg-slate-900 shadow-md flex items-center justify-center gap-2">Lưu Bản Nháp</button>
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
</style>