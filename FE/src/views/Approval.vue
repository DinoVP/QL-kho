<script setup>
import { ref, computed, onMounted } from 'vue'
import { 
  MagnifyingGlassIcon, CheckCircleIcon, XCircleIcon, 
  EyeIcon, XMarkIcon, ArrowDownTrayIcon, ArrowUpTrayIcon
} from '@heroicons/vue/24/outline'

const INBOUND_API = 'https://localhost:7139/api/Inbound'
const OUTBOUND_API = 'https://localhost:7139/api/Outbound'

const getAuthHeaders = () => ({ 'Content-Type': 'application/json', 'Authorization': 'Bearer ' + (localStorage.getItem('authToken') || '') })

const processReceipts = ref([]); const isLoading = ref(false); const activeTab = ref('inbound') 

const fetchData = async () => {
  isLoading.value = true
  try {
    const headers = getAuthHeaders()
    const [inbRes, outRes] = await Promise.all([ fetch(INBOUND_API, { headers }), fetch(OUTBOUND_API, { headers }) ])
    let inbData = [], outData = []
    
    if (inbRes.ok) { const text = await inbRes.text(); inbData = text ? JSON.parse(text) : []; }
    if (outRes.ok) { const text = await outRes.text(); outData = text ? JSON.parse(text) : []; }
    
    const mappedInbound = inbData.map(r => ({ 
      ...r, id: r.id || r.Id, code: r.code || r.Code, status: r.status || r.Status,
      date: r.date || r.Date, totalQty: r.totalQty || r.TotalQty, totalPrice: r.totalPrice || r.TotalPrice,
      type: 'inbound', typeName: 'Nhập Kho', partnerName: r.supplierName || r.SupplierName || 'Khách vãng lai'
    }))

    const mappedOutbound = outData.map(r => ({ 
      ...r, id: r.id || r.Id, code: r.code || r.Code, status: r.status || r.Status,
      date: r.date || r.Date, totalQty: r.totalQty || r.TotalQty, totalPrice: r.totalPrice || r.TotalPrice,
      type: 'outbound', typeName: 'Xuất Kho', partnerName: r.customerName || r.CustomerName || 'Khách vãng lai'
    }))
    
    processReceipts.value = [...mappedInbound, ...mappedOutbound]

  } catch (error) { console.error('Lỗi tải dữ liệu:', error) }
  finally { isLoading.value = false }
}

const searchQuery = ref('')
const filteredReceipts = computed(() => {
  return processReceipts.value.filter(r => {
    const matchSearch = (r.code || '').toLowerCase().includes(searchQuery.value.toLowerCase()) || 
                        (r.partnerName || '').toLowerCase().includes(searchQuery.value.toLowerCase())
    return matchSearch && r.type === activeTab.value
  })
})

const showModal = ref(false); const selectedReceipt = ref(null)

const openModal = (receipt) => {
  const safeItems = (receipt.items || receipt.Items || []).map(i => ({
    sku: i.sku || i.Sku, name: i.name || i.Name, qty: i.qty || i.Qty, price: i.price || i.Price, 
    locationCode: i.locationCode || i.LocationCode, nsx: i.nsx || i.Nsx || '', hsd: i.hsd || i.Hsd || ''
  }));
  selectedReceipt.value = { ...receipt, items: safeItems }; showModal.value = true
}

const closeModal = () => { showModal.value = false; selectedReceipt.value = null }
const formatCurrency = (val) => new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(val || 0)

const handleAction = async (action, receipt) => {
  let msg = action === 'approve' ? `DUYỆT chứng từ [${receipt.code}]?` : `TỪ CHỐI (Bỏ phiếu) [${receipt.code}]?`;
  if (!confirm(msg)) return;
  try {
    const API_URL = receipt.type === 'inbound' ? INBOUND_API : OUTBOUND_API;
    const res = await fetch(`${API_URL}/${receipt.id}/${action}`, { method: 'PUT', headers: getAuthHeaders() })
    if (res.ok) { 
        alert('Xử lý thành công!'); 
        if (showModal.value) closeModal(); 
        await fetchData(); 
    } 
    else { 
        let errMsg = "Lỗi hệ thống không xác định!";
        try { const text = await res.text(); if (text) { const err = JSON.parse(text); errMsg = err.message || errMsg; } } catch(e) {}
        alert('LỖI HỆ THỐNG: ' + errMsg);
    }
  } catch(e) { console.error(e) }
}

const getStatusBadge = (status) => {
  switch(status) {
    case 'pending': return { text: 'Cần Duyệt', class: 'bg-amber-100 text-amber-700 border-amber-200 animate-pulse' }
    case 'approved': return { text: 'Đã Duyệt', class: 'bg-blue-100 text-blue-700 border-blue-200' }
    case 'completed': return { text: 'Hoàn Thành', class: 'bg-emerald-100 text-emerald-700 border-emerald-200' }
    case 'rejected': return { text: 'Đã Hủy', class: 'bg-red-100 text-red-700 border-red-200' }
    default: return { text: 'Khác', class: 'bg-gray-100 text-gray-500' }
  }
}

onMounted(() => fetchData())
</script>

<template>
  <div class="space-y-6 animate-fade-in pb-10 px-1 relative">
    <div class="flex justify-between items-center">
      <div>
        <h2 class="text-2xl font-bold text-gray-800">Duyệt Phiếu Tổng</h2>
        <p class="text-sm text-gray-500 mt-1">Nơi Quản lý/Giám đốc kiểm tra và Phê duyệt các chứng từ.</p>
      </div>
      <button @click="fetchData" class="bg-white border border-gray-200 text-gray-700 px-4 py-2 rounded-lg text-sm font-semibold hover:bg-gray-50 transition-colors shadow-sm">Làm mới dữ liệu</button>
    </div>

    <div class="bg-white rounded-xl border flex gap-4 p-2 shadow-sm items-center">
      <div class="flex bg-gray-100 rounded-lg">
        <button @click="activeTab = 'inbound'" :class="['px-6 py-2 text-sm font-bold rounded-md flex items-center gap-1.5', activeTab === 'inbound' ? 'bg-white text-blue-700 shadow-sm' : 'text-gray-500']"><ArrowDownTrayIcon class="w-4 h-4"/> Nhập Kho</button>
        <button @click="activeTab = 'outbound'" :class="['px-6 py-2 text-sm font-bold rounded-md flex items-center gap-1.5', activeTab === 'outbound' ? 'bg-white text-amber-700 shadow-sm' : 'text-gray-500']"><ArrowUpTrayIcon class="w-4 h-4"/> Xuất Kho</button>
      </div>
      <div class="relative flex-1">
        <div class="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none"><MagnifyingGlassIcon class="w-5 h-5 text-gray-400" /></div>
        <input v-model="searchQuery" type="text" class="block w-full pl-10 pr-3 py-2 border rounded-lg text-sm outline-none focus:ring-1 focus:ring-primary-500" placeholder="Tìm kiếm mã phiếu, đối tác...">
      </div>
    </div>

    <div class="bg-white rounded-xl border shadow-sm overflow-hidden">
      <table class="w-full text-sm text-left">
        <thead class="bg-gray-50 uppercase font-bold text-gray-500">
          <tr><th class="px-5 py-3">Mã Phiếu</th><th class="px-5 py-3">Ngày</th><th class="px-5 py-3">Đối tác</th><th class="px-5 py-3 text-center">Trạng Thái</th><th class="px-5 py-3 text-right">Thao tác</th></tr>
        </thead>
        <tbody class="divide-y divide-gray-100">
          <tr v-if="filteredReceipts.length === 0"><td colspan="5" class="px-6 py-12 text-center text-gray-500">Không có dữ liệu.</td></tr>
          <tr v-for="receipt in filteredReceipts" :key="receipt.id" class="hover:bg-slate-50">
            <td class="px-5 py-3 font-bold text-slate-800">{{ receipt.code }}</td><td class="px-5 py-3">{{ receipt.date }}</td><td class="px-5 py-3 font-bold">{{ receipt.partnerName }}</td>
            <td class="px-5 py-3 text-center"><span :class="['px-2 py-1 rounded text-[10px] font-bold uppercase border', getStatusBadge(receipt.status).class]">{{ getStatusBadge(receipt.status).text }}</span></td>
            <td class="px-5 py-3 text-right space-x-1.5">
              <button @click="openModal(receipt)" class="p-1.5 text-blue-600 hover:bg-blue-50 rounded transition-colors" title="Xem chi tiết"><EyeIcon class="w-5 h-5" /></button>
              
              <template v-if="receipt.status === 'pending'">
                <button @click="handleAction('approve', receipt)" class="p-1.5 text-emerald-600 hover:bg-emerald-50 rounded transition-colors" title="Phê duyệt phiếu"><CheckCircleIcon class="w-5 h-5" /></button>
                <button @click="handleAction('reject', receipt)" class="p-1.5 text-red-600 hover:bg-red-50 rounded transition-colors" title="Từ chối (Hủy phiếu)"><XCircleIcon class="w-5 h-5" /></button>
              </template>
            </td>
          </tr>
        </tbody>
      </table>
    </div>

    <Teleport to="body">
      <div v-if="showModal" class="fixed inset-0 z-50 flex items-center justify-center bg-slate-900/60 backdrop-blur-sm">
        <div class="bg-white rounded-xl shadow-2xl w-full max-w-5xl flex flex-col max-h-[90vh]">
          <div class="px-6 py-4 border-b flex justify-between bg-slate-50"><h3 class="font-bold text-lg"><EyeIcon class="w-6 h-6 inline text-blue-500 mr-1"/> Chi tiết Phiếu: {{ selectedReceipt.code }}</h3><button @click="closeModal" class="text-gray-400 hover:text-red-500"><XMarkIcon class="w-6 h-6" /></button></div>
          <div class="p-6 overflow-y-auto flex-1 space-y-6">
            <div class="grid grid-cols-4 gap-4 bg-slate-50 p-4 rounded-lg border">
              <div><p class="text-xs font-bold text-gray-500 uppercase">Đối tác</p><p class="text-sm font-bold text-gray-900 mt-1">{{ selectedReceipt.partnerName }}</p></div>
              <div><p class="text-xs font-bold text-gray-500 uppercase">Ngày lập</p><p class="text-sm font-bold text-gray-900 mt-1">{{ selectedReceipt.date }}</p></div>
              <div><p class="text-xs font-bold text-gray-500 uppercase">Loại chứng từ</p><p class="text-sm font-bold text-blue-600 mt-1">{{ selectedReceipt.typeName }}</p></div>
              <div><p class="text-xs font-bold text-gray-500 uppercase">Trạng thái</p><p class="text-sm font-bold text-amber-600 mt-1">{{ getStatusBadge(selectedReceipt.status).text }}</p></div>
            </div>

            <table class="w-full text-sm text-left border">
              <thead class="bg-gray-100 uppercase font-bold text-xs border-b">
                <tr><th class="px-4 py-3">SKU</th><th class="px-4 py-3">Tên</th><th class="px-4 py-3">Vị trí Kệ</th><th v-if="selectedReceipt.type==='inbound'" class="px-4 py-3">NSX-HSD</th><th class="px-4 py-3 text-right">SL</th></tr>
              </thead>
              <tbody>
                <tr v-for="(item, idx) in selectedReceipt.items" :key="idx" class="border-b">
                  <td class="px-4 py-2 font-bold">{{item.sku}}</td><td class="px-4 py-2">{{item.name}}</td><td class="px-4 py-2 font-bold text-indigo-600">{{item.locationCode || 'Kho chung'}}</td>
                  <td v-if="selectedReceipt.type==='inbound'" class="px-4 py-2 text-xs text-gray-500">{{item.nsx}} <br> {{item.hsd}}</td>
                  <td class="px-4 py-2 text-right font-bold text-blue-700">{{item.qty}}</td>
                </tr>
              </tbody>
            </table>
          </div>
          <div class="px-6 py-4 border-t flex justify-end gap-3 bg-gray-50">
            <button @click="closeModal" class="px-6 py-2.5 border rounded-lg font-bold hover:bg-gray-100 transition-colors">Đóng lại</button>
            <template v-if="selectedReceipt.status === 'pending'">
              <button @click="handleAction('reject', selectedReceipt)" class="px-6 py-2.5 bg-red-50 hover:bg-red-100 text-red-600 rounded-lg font-bold shadow-sm transition-colors">Từ chối</button>
              <button @click="handleAction('approve', selectedReceipt)" class="px-6 py-2.5 bg-blue-600 hover:bg-blue-700 text-white rounded-lg font-bold shadow-sm transition-colors">Phê Duyệt</button>
            </template>
          </div>
        </div>
      </div>
    </Teleport>
  </div>
</template>