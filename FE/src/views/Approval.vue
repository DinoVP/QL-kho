<script setup>
import { ref, computed, onMounted } from 'vue'
import { 
  MagnifyingGlassIcon, CheckCircleIcon, XCircleIcon, 
  EyeIcon, XMarkIcon, ArrowDownTrayIcon, ArrowUpTrayIcon,
  MapPinIcon, DocumentDuplicateIcon, ShoppingCartIcon, UserIcon,
  BuildingStorefrontIcon, UsersIcon
} from '@heroicons/vue/24/outline'

const INBOUND_API = 'https://localhost:7139/api/Inbound'
const OUTBOUND_API = 'https://localhost:7139/api/Outbound'
const ORDER_API = 'https://localhost:7139/api/Po'
const PROD_API = 'https://localhost:7139/api/Products'
const PARTNER_API = 'https://localhost:7139/api/CrmPartners' // API Đối tác (KH/NCC)

const getAuthHeaders = () => ({ 
    'Content-Type': 'application/json', 
    'Authorization': 'Bearer ' + (localStorage.getItem('authToken') || '') 
})

const processReceipts = ref([])
const isLoading = ref(false)
const activeTab = ref('inbound') 

const formatCurrency = (val) => new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(val || 0)

const getUnitChain = (prod) => {
    let units = [{ name: prod.unit || prod.Unit || 'SL', rate: 1 }];
    const conversions = prod.conversions || prod.Conversions || [];
    if (conversions.length > 0) {
        [...conversions].sort((a,b) => a.rate - b.rate).forEach(c => units.push({ name: c.altUnit, rate: c.rate }));
    } else if ((prod.conversionRate || prod.ConversionRate) > 1) {
        units.push({ name: 'Thùng/Kiện', rate: prod.conversionRate || prod.ConversionRate });
    }
    return units.sort((a,b) => a.rate - b.rate);
}

const autoFormatStockText = (qty, units, baseUnit) => {
    if (qty <= 0) return `0 ${baseUnit}`;
    const sortedPacks = [...(units || [])].sort((a, b) => b.rate - a.rate);
    let remaining = qty; let components = [];
    for (const pack of sortedPacks) {
        if (pack.rate > 1 && remaining >= pack.rate) {
            components.push({ count: Math.floor(remaining / pack.rate), name: pack.name });
            remaining %= pack.rate;
        }
    }
    if (remaining > 0 || components.length === 0) components.push({ count: remaining, name: baseUnit });
    return components.map(c => `${c.count} ${c.name}`).join(' + ');
}

const getItemNames = (items) => {
    if (!items || items.length === 0) return '---';
    const names = items.slice(0, 2).map(i => i.name).join(', ');
    return items.length > 2 ? `${names} ... (+${items.length - 2})` : names;
}

const getItemQuantities = (items) => {
    if (!items || items.length === 0) return '---';
    const qtys = items.slice(0, 2).map(i => autoFormatStockText(i.qty || i.Qty, i.units, i.unit));
    return items.length > 2 ? `${qtys.join(', ')} ...` : qtys.join(', ');
}

const fetchData = async () => {
  isLoading.value = true
  try {
    const headers = getAuthHeaders()
    const [inbRes, outRes, orderRes, prodRes, partnerRes] = await Promise.all([ 
        fetch(INBOUND_API, { headers }), fetch(OUTBOUND_API, { headers }),
        fetch(ORDER_API, { headers }), fetch(PROD_API, { headers }), fetch(PARTNER_API, { headers })
    ])
    
    let inbData = [], outData = [], orderData = [], products = [], partners = []
    
    if (prodRes.ok) products = await prodRes.json()
    if (partnerRes.ok) {
        const pData = await partnerRes.json()
        partners = pData.data || pData.Data || pData || []
    }
    if (inbRes.ok) { const text = await inbRes.text(); inbData = text ? JSON.parse(text) : []; }
    if (outRes.ok) { const text = await outRes.text(); outData = text ? JSON.parse(text) : []; }
    if (orderRes.ok) { const text = await orderRes.text(); orderData = text ? JSON.parse(text) : []; }
    
    // XỬ LÝ DỮ LIỆU NHẬP KHO
    const mappedInbound = inbData.map(r => {
        // Tìm tên Đối tác dựa trên supplierId
        const partnerId = r.supplierId || r.SupplierId || r.partnerId || r.PartnerId;
        const partner = partners.find(p => p.partnerId === partnerId || p.id === partnerId);
        const partnerName = partner ? (partner.partnerName || partner.name) : (r.supplierName || r.SupplierName || 'Chưa xác định NCC');

        return { 
          ...r, id: r.id || r.Id, code: r.code || r.Code, status: r.status || r.Status,
          date: r.date || r.Date, type: 'inbound', typeName: 'Nhập Kho', 
          partnerName: partnerName, 
          warehouseName: r.warehouseName || r.WarehouseName || 'Kho Tổng'
        }
    })

    // XỬ LÝ DỮ LIỆU XUẤT KHO
    const mappedOutbound = outData.map(r => {
        // Tìm tên Đối tác dựa trên customerId
        const partnerId = r.customerId || r.CustomerId || r.partnerId || r.PartnerId;
        const partner = partners.find(p => p.partnerId === partnerId || p.id === partnerId);
        const partnerName = partner ? (partner.partnerName || partner.name) : (r.customerName || r.CustomerName || 'Khách vãng lai');

        return { 
          ...r, id: r.id || r.Id, code: r.code || r.Code, status: r.status || r.Status,
          date: r.date || r.Date, type: 'outbound', typeName: 'Xuất Kho', 
          partnerName: partnerName, 
          warehouseName: r.warehouseName || r.WarehouseName || 'Kho Tổng'
        }
    })

    // XỬ LÝ YÊU CẦU MUA HÀNG (PR)
    const mappedPR = orderData.filter(o => o.type === 'PR' || o.Type === 'PR').map(r => ({
      ...r, id: r.id || r.Id, code: r.code || r.Code || `PR${(r.id||0).toString().padStart(4,'0')}`, status: r.status || r.Status,
      date: r.date || r.Date || r.orderDate || '', type: 'pr', typeName: 'Xin Mua Hàng (PR)',
      warehouseName: 'Kho Tổng', // Hoặc lấy theo user tạo
      note: r.note || r.Note || '',
      totalPrice: 0,
      items: (r.items || r.Items || []).map(i => {
          const p = products.find(x => x.id === (i.variantId || i.VariantId)) || {}
          return { sku: p.sku || p.Sku, name: p.name || p.Name, qty: i.qty || i.Qty, price: 0, unit: p.unit || p.Unit || 'SL', units: getUnitChain(p) }
      })
    }))

    // XỬ LÝ ĐƠN ĐẶT HÀNG (PO)
    const mappedPO = orderData.filter(o => o.type === 'PO' || o.Type === 'PO').map(r => {
      const pId = r.supplierId || r.SupplierId || r.partnerId || r.PartnerId;
      const sup = partners.find(s => s.partnerId === pId || s.id === pId)
      return {
          ...r, id: r.id || r.Id, code: r.code || r.Code || `PO${(r.id||0).toString().padStart(4,'0')}`, status: r.status || r.Status,
          date: r.date || r.Date || r.orderDate || '', type: 'po', typeName: 'Đơn Đặt Hàng (PO)',
          partnerName: sup ? (sup.partnerName || sup.name) : 'NCC Ẩn/Chưa chọn',
          note: r.note || r.Note || '',
          totalPrice: r.totalAmount || r.TotalAmount || 0,
          items: (r.items || r.Items || []).map(i => {
              const p = products.find(x => x.id === (i.variantId || i.VariantId)) || {}
              return { sku: p.sku || p.Sku, name: p.name || p.Name, qty: i.qty || i.Qty, price: i.price || i.Price || 0, unit: p.unit || p.Unit || 'SL', units: getUnitChain(p) }
          })
      }
    })
    
    processReceipts.value = [...mappedInbound, ...mappedOutbound, ...mappedPR, ...mappedPO]

  } catch (error) { console.error('Lỗi tải dữ liệu:', error) } finally { isLoading.value = false }
}

const searchQuery = ref('')
const filteredReceipts = computed(() => {
  return processReceipts.value.filter(r => {
    const matchSearch = (r.code || '').toLowerCase().includes(searchQuery.value.toLowerCase()) || 
                        (r.partnerName || '').toLowerCase().includes(searchQuery.value.toLowerCase())
    return matchSearch && r.type === activeTab.value
  }).sort((a,b) => b.id - a.id) 
})

const showModal = ref(false)
const selectedReceipt = ref(null)

const openModal = (receipt) => {
  const safeItems = (receipt.items || receipt.Items || []).map(i => ({
    sku: i.sku || i.Sku || 'N/A', name: i.name || i.Name || 'Hàng hóa', 
    qty: i.qty || i.Qty || 0, price: i.price || i.Price || 0, 
    locationCode: i.locationCode || i.LocationCode || '', nsx: i.nsx || i.Nsx || '', hsd: i.hsd || i.Hsd || '',
    unit: i.unit || 'SL', units: i.units || [{name: 'SL', rate: 1}]
  }));
  selectedReceipt.value = { ...receipt, items: safeItems }
  showModal.value = true
}

const closeModal = () => { showModal.value = false; selectedReceipt.value = null }

const handleAction = async (action, receipt) => {
  let msg = action === 'approve' ? `Sếp đồng ý DUYỆT chứng từ [${receipt.code}]?` : `Sếp muốn TỪ CHỐI (Bỏ phiếu) [${receipt.code}]?`;
  if (!confirm(msg)) return;
  
  try {
    let res;
    if (receipt.type === 'inbound' || receipt.type === 'outbound') {
        const API_URL = receipt.type === 'inbound' ? INBOUND_API : OUTBOUND_API;
        res = await fetch(`${API_URL}/${receipt.id}/${action}`, { method: 'PUT', headers: getAuthHeaders() })
    } else {
        const newStatus = action === 'approve' ? 'approved' : 'rejected';
        res = await fetch(`${ORDER_API}/${receipt.id}/status`, { 
            method: 'PUT', 
            headers: getAuthHeaders(), 
            body: JSON.stringify(newStatus) 
        })
    }

    if (res.ok) { 
        alert('Xử lý thành công thưa Sếp!'); 
        if (showModal.value) closeModal(); 
        await fetchData(); 
    } else { 
        alert('LỖI HỆ THỐNG');
    }
  } catch(e) { console.error(e) }
}

const getStatusBadge = (status) => {
  if (!status) return { text: 'Khác', class: 'bg-gray-100 text-gray-500' };
  
  const s = status.toLowerCase().replace(/["']/g, "").trim(); 
  
  switch(s) {
    case 'pending': return { text: 'Cần Duyệt', class: 'bg-amber-100 text-amber-700 border-amber-200 animate-pulse shadow-sm' }
    case 'approved': return { text: 'Đã Duyệt', class: 'bg-blue-100 text-blue-700 border-blue-200' }
    case 'completed': return { text: 'Hoàn Thành', class: 'bg-emerald-100 text-emerald-700 border-emerald-200' }
    case 'rejected': return { text: 'Đã Hủy', class: 'bg-red-100 text-red-700 border-red-200' }
    case 'sent': return { text: 'Đã gửi NCC', class: 'bg-purple-100 text-purple-700 border-purple-200' }
    default: return { text: status, class: 'bg-gray-100 text-gray-500' }
  }
}

onMounted(() => fetchData())
</script>

<template>
  <div class="space-y-6 animate-fade-in pb-10 px-1 relative">
    
    <div class="flex justify-between items-center">
      <div>
        <h2 class="text-2xl font-bold text-gray-800">Trung Tâm Duyệt Phiếu Tổng</h2>
        <p class="text-sm text-gray-500 mt-1">Nơi Quản lý/Giám đốc kiểm tra và Phê duyệt mọi hoạt động của Kho & Mua hàng.</p>
      </div>
      <button @click="fetchData" class="bg-white border border-gray-200 text-gray-700 px-4 py-2 rounded-lg text-sm font-semibold hover:bg-gray-50 transition-colors shadow-sm flex items-center gap-2">
          <svg v-if="isLoading" class="animate-spin h-4 w-4 text-gray-500" fill="none" viewBox="0 0 24 24"><circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4"></circle><path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path></svg>
          Làm mới dữ liệu
      </button>
    </div>

    <div class="bg-white rounded-xl border flex flex-col md:flex-row gap-4 p-2 shadow-sm items-center justify-between">
      <div class="flex flex-wrap bg-gray-100 rounded-lg w-full md:w-auto p-1 gap-1">
        <button @click="activeTab = 'inbound'" :class="['px-4 py-2 text-sm font-bold rounded-md flex items-center gap-1.5 transition-all', activeTab === 'inbound' ? 'bg-white text-blue-700 shadow-sm' : 'text-gray-500 hover:text-gray-700']"><ArrowDownTrayIcon class="w-4 h-4"/> Nhập Kho</button>
        <button @click="activeTab = 'outbound'" :class="['px-4 py-2 text-sm font-bold rounded-md flex items-center gap-1.5 transition-all', activeTab === 'outbound' ? 'bg-white text-orange-600 shadow-sm' : 'text-gray-500 hover:text-gray-700']"><ArrowUpTrayIcon class="w-4 h-4"/> Xuất Kho</button>
        <button @click="activeTab = 'pr'" :class="['px-4 py-2 text-sm font-bold rounded-md flex items-center gap-1.5 transition-all', activeTab === 'pr' ? 'bg-white text-indigo-600 shadow-sm' : 'text-gray-500 hover:text-gray-700']"><DocumentDuplicateIcon class="w-4 h-4"/> Xin Mua (PR)</button>
        <button @click="activeTab = 'po'" :class="['px-4 py-2 text-sm font-bold rounded-md flex items-center gap-1.5 transition-all', activeTab === 'po' ? 'bg-white text-purple-600 shadow-sm' : 'text-gray-500 hover:text-gray-700']"><ShoppingCartIcon class="w-4 h-4"/> Đặt Hàng (PO)</button>
      </div>
      <div class="relative w-full md:w-96">
        <div class="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none"><MagnifyingGlassIcon class="w-5 h-5 text-gray-400" /></div>
        <input v-model="searchQuery" type="text" class="block w-full pl-10 pr-3 py-2 border rounded-lg text-sm outline-none focus:ring-1 focus:ring-blue-500" placeholder="Tìm kiếm mã phiếu, đối tác...">
      </div>
    </div>

    <div class="bg-white rounded-xl border shadow-sm overflow-hidden">
      <div class="overflow-x-auto custom-scrollbar">
          <table class="w-full text-sm text-left min-w-[1000px]">
            <thead class="bg-gray-50 uppercase font-bold text-[11px] text-gray-500 border-b tracking-wider">
              <tr>
                <th class="px-5 py-4 w-28">Mã Phiếu</th>
                <th class="px-5 py-4 w-32">Ngày lập</th>
                
                <th v-if="activeTab === 'inbound' || activeTab === 'outbound'" class="px-5 py-4 text-indigo-700 w-40">Kho Luân Chuyển</th> 
                
                <th v-if="activeTab === 'inbound'" class="px-5 py-4 text-purple-700 w-48">Nhà Cung Cấp (NCC)</th>
                <th v-if="activeTab === 'outbound'" class="px-5 py-4 text-orange-700 w-48">Khách Hàng</th>
                
                <th v-if="activeTab === 'pr'" class="px-5 py-4 w-40 text-indigo-700">Kho Yêu Cầu</th>
                <th v-if="activeTab === 'po'" class="px-5 py-4 w-48 text-purple-700">Nhà Cung Cấp (NCC)</th>
                
                <th v-if="activeTab === 'pr' || activeTab === 'po'" class="px-5 py-4 min-w-[200px]">Tên Mặt Hàng</th>
                <th v-if="activeTab === 'pr' || activeTab === 'po'" class="px-5 py-4 min-w-[150px]">Số Lượng</th>

                <th v-if="activeTab === 'po'" class="px-5 py-4 text-right w-36">Tổng Tiền</th>
                <th v-if="activeTab === 'pr' || activeTab === 'po'" class="px-5 py-4 w-48">Ghi chú</th>

                <th class="px-5 py-4 text-center w-36">Trạng Thái</th>
                <th class="px-5 py-4 text-right w-36">Thao tác</th>
              </tr>
            </thead>
            <tbody class="divide-y divide-gray-100">
              <tr v-if="filteredReceipts.length === 0">
                  <td colspan="10" class="px-6 py-16 text-center text-gray-400"><MagnifyingGlassIcon class="w-10 h-10 mx-auto mb-2 opacity-50"/> Không có chứng từ nào trong mục này.</td>
              </tr>
              <tr v-for="receipt in filteredReceipts" :key="receipt.id" class="hover:bg-slate-50 transition-colors">
                <td class="px-5 py-4 font-bold text-slate-800">{{ receipt.code }}</td>
                <td class="px-5 py-4 text-gray-600 font-medium">{{ receipt.date }}</td>
                
                <td v-if="activeTab === 'inbound' || activeTab === 'outbound'" class="px-5 py-4 font-bold text-indigo-700 truncate max-w-[200px]"><MapPinIcon class="w-4 h-4 inline mr-1 text-indigo-400 -mt-0.5"/>{{ receipt.warehouseName }}</td>
                
                <td v-if="activeTab === 'inbound'" class="px-5 py-4 font-bold text-purple-700 truncate max-w-[250px]"><BuildingStorefrontIcon class="w-4 h-4 inline mr-1 text-purple-400 -mt-0.5"/>{{ receipt.partnerName }}</td>
                <td v-if="activeTab === 'outbound'" class="px-5 py-4 font-bold text-orange-600 truncate max-w-[250px]"><UsersIcon class="w-4 h-4 inline mr-1 text-orange-400 -mt-0.5"/>{{ receipt.partnerName }}</td>
                
                <td v-if="activeTab === 'pr'" class="px-5 py-4">
                    <span class="font-bold text-indigo-700 block"><MapPinIcon class="w-4 h-4 inline mr-1 text-indigo-400 -mt-0.5"/>{{ receipt.warehouseName }}</span>
                    <span class="text-[10px] text-gray-500 font-medium flex items-center gap-1 mt-0.5"><UserIcon class="w-3 h-3"/> Người lập: Thủ Kho</span>
                </td>
                
                <td v-if="activeTab === 'po'" class="px-5 py-4 font-bold text-purple-700 truncate max-w-[250px]"><BuildingStorefrontIcon class="w-4 h-4 inline mr-1 text-purple-400 -mt-0.5"/>{{ receipt.partnerName }}</td>
                
                <td v-if="activeTab === 'pr' || activeTab === 'po'" class="px-5 py-4">
                    <span class="text-sm font-bold text-gray-800 line-clamp-2" :title="getItemNames(receipt.items)">{{ getItemNames(receipt.items) }}</span>
                </td>
                <td v-if="activeTab === 'pr' || activeTab === 'po'" class="px-5 py-4">
                    <span class="text-sm font-bold text-indigo-600 line-clamp-2" :title="getItemQuantities(receipt.items)">{{ getItemQuantities(receipt.items) }}</span>
                </td>
                
                <td v-if="activeTab === 'po'" class="px-5 py-4 text-right font-bold text-emerald-600">{{ formatCurrency(receipt.totalPrice) }}</td>
                
                <td v-if="activeTab === 'pr' || activeTab === 'po'" class="px-5 py-4 text-gray-600 text-xs truncate max-w-[200px]" :title="receipt.note">{{ receipt.note || '---' }}</td>

                <td class="px-5 py-4 text-center">
                    <span :class="['px-2.5 py-1 rounded-md text-[10px] font-bold uppercase border tracking-wider', getStatusBadge(receipt.status).class]">{{ getStatusBadge(receipt.status).text }}</span>
                </td>
                <td class="px-5 py-4 text-right space-x-1.5 whitespace-nowrap">
                  <button @click="openModal(receipt)" class="p-1.5 text-blue-600 hover:bg-blue-50 rounded transition-colors" title="Xem chi tiết"><EyeIcon class="w-5 h-5" /></button>
                  <template v-if="receipt.status === 'pending'">
                    <button @click="handleAction('approve', receipt)" class="p-1.5 text-emerald-600 hover:bg-emerald-50 rounded transition-colors" title="Phê duyệt"><CheckCircleIcon class="w-5 h-5" /></button>
                    <button @click="handleAction('reject', receipt)" class="p-1.5 text-red-600 hover:bg-red-50 rounded transition-colors" title="Từ chối"><XCircleIcon class="w-5 h-5" /></button>
                  </template>
                </td>
              </tr>
            </tbody>
          </table>
      </div>
    </div>

    <Teleport to="body">
      <div v-if="showModal" class="fixed inset-0 z-50 flex items-center justify-center bg-slate-900/60 backdrop-blur-sm p-4">
        <div class="bg-white rounded-xl shadow-2xl w-full max-w-5xl flex flex-col max-h-[90vh] overflow-hidden animate-fade-in">
          
          <div class="px-6 py-4 border-b flex justify-between items-center shrink-0" :class="{'bg-blue-50': selectedReceipt.type==='inbound', 'bg-orange-50': selectedReceipt.type==='outbound', 'bg-indigo-50': selectedReceipt.type==='pr', 'bg-purple-50': selectedReceipt.type==='po'}">
            <h3 class="font-bold text-lg flex items-center gap-2" :class="{'text-blue-800': selectedReceipt.type==='inbound', 'text-orange-800': selectedReceipt.type==='outbound', 'text-indigo-800': selectedReceipt.type==='pr', 'text-purple-800': selectedReceipt.type==='po'}">
                <ArrowDownTrayIcon v-if="selectedReceipt.type==='inbound'" class="w-6 h-6"/>
                <ArrowUpTrayIcon v-if="selectedReceipt.type==='outbound'" class="w-6 h-6"/>
                <DocumentDuplicateIcon v-if="selectedReceipt.type==='pr'" class="w-6 h-6"/>
                <ShoppingCartIcon v-if="selectedReceipt.type==='po'" class="w-6 h-6"/>
                Chi tiết: {{ selectedReceipt.code }}
            </h3>
            <button @click="closeModal" class="text-gray-400 hover:text-red-500 transition-colors"><XMarkIcon class="w-6 h-6" /></button>
          </div>
          
          <div class="p-6 overflow-y-auto flex-1 space-y-6 custom-scrollbar bg-white">
            
            <div class="grid grid-cols-2 md:grid-cols-4 gap-4 bg-slate-50 p-4 rounded-xl border border-gray-200 shadow-sm">
              <div v-if="selectedReceipt.type !== 'pr'">
                  <p class="text-[10px] font-bold text-gray-500 uppercase tracking-wider">
                    <span v-if="selectedReceipt.type === 'po' || selectedReceipt.type === 'inbound'">Nhà Cung Cấp</span>
                    <span v-else>Khách hàng</span>
                  </p>
                  <p class="text-sm font-bold mt-1 truncate flex items-center gap-1" :class="{'text-purple-700': selectedReceipt.type === 'inbound' || selectedReceipt.type === 'po', 'text-orange-600': selectedReceipt.type === 'outbound'}" :title="selectedReceipt.partnerName">
                    <BuildingStorefrontIcon v-if="selectedReceipt.type === 'inbound' || selectedReceipt.type === 'po'" class="w-4 h-4"/>
                    <UsersIcon v-if="selectedReceipt.type === 'outbound'" class="w-4 h-4"/>
                    {{ selectedReceipt.partnerName }}
                  </p>
              </div>
              <div v-if="selectedReceipt.type === 'inbound' || selectedReceipt.type === 'outbound'">
                  <p class="text-[10px] font-bold text-gray-500 uppercase tracking-wider">Kho Luân chuyển</p>
                  <p class="text-sm font-bold text-indigo-700 mt-1 truncate" :title="selectedReceipt.warehouseName">{{ selectedReceipt.warehouseName }}</p>
              </div>
              <div v-if="selectedReceipt.type === 'pr' || selectedReceipt.type === 'po'">
                  <p class="text-[10px] font-bold text-gray-500 uppercase tracking-wider">Ngày lập phiếu</p>
                  <p class="text-sm font-bold text-gray-800 mt-1">{{ selectedReceipt.date }}</p>
              </div>
              <div>
                  <p class="text-[10px] font-bold text-gray-500 uppercase tracking-wider">Loại chứng từ</p>
                  <p class="text-sm font-bold text-blue-600 mt-1">{{ selectedReceipt.typeName }}</p>
              </div>
              <div>
                  <p class="text-[10px] font-bold text-gray-500 uppercase tracking-wider">Trạng thái</p>
                  <div class="mt-1"><span :class="['px-2 py-1 rounded text-[10px] font-bold uppercase border tracking-wider', getStatusBadge(selectedReceipt.status).class]">{{ getStatusBadge(selectedReceipt.status).text }}</span></div>
              </div>
            </div>

            <div class="border border-gray-200 rounded-xl overflow-x-auto shadow-sm">
                <table class="w-full text-sm text-left">
                  <thead class="bg-gray-100 uppercase font-bold text-xs text-gray-600 border-b border-gray-200">
                    <tr>
                        <th class="px-4 py-3 w-32">Mã SKU</th>
                        <th class="px-4 py-3 min-w-[200px]">Tên Mặt Hàng</th>
                        <template v-if="selectedReceipt.type === 'inbound' || selectedReceipt.type === 'outbound'">
                            <th class="px-4 py-3 border-l border-gray-200 text-center w-32">Vị trí Kệ</th>
                            <th v-if="selectedReceipt.type === 'inbound'" class="px-4 py-3 border-l border-gray-200 text-center w-28">NSX - HSD</th>
                        </template>
                        <th class="px-4 py-3 text-center border-l border-gray-200 bg-indigo-50 text-indigo-800 w-40">SL Đề Nghị</th>
                        <template v-if="selectedReceipt.type === 'po'">
                            <th class="px-4 py-3 text-right border-l border-gray-200 bg-purple-50 text-purple-800 w-36">Đơn giá Nhập</th>
                            <th class="px-4 py-3 text-right border-l border-gray-200 bg-purple-50 text-purple-800 w-36">Thành tiền</th>
                        </template>
                    </tr>
                  </thead>
                  <tbody class="divide-y divide-gray-100">
                    <tr v-for="(item, idx) in selectedReceipt.items" :key="idx" class="hover:bg-slate-50 transition-colors">
                      <td class="px-4 py-3 font-bold text-gray-800">{{item.sku}}</td>
                      <td class="px-4 py-3 text-gray-700 font-medium">{{item.name}}</td>
                      <template v-if="selectedReceipt.type === 'inbound' || selectedReceipt.type === 'outbound'">
                          <td class="px-4 py-3 font-bold text-indigo-600 text-center border-l border-gray-100">{{item.locationCode || 'Kho chung'}}</td>
                          <td v-if="selectedReceipt.type === 'inbound'" class="px-4 py-3 text-[11px] text-gray-500 text-center border-l border-gray-100">{{item.nsx}} <br> <span class="text-red-500">{{item.hsd}}</span></td>
                      </template>
                      <td class="px-4 py-3 text-center border-l border-gray-100">
                          <span class="font-bold text-indigo-700 text-lg">{{ autoFormatStockText(item.qty, item.units, item.unit) }}</span>
                          <div class="text-[10px] text-gray-400 font-medium mt-0.5" v-if="item.unit">(= {{item.qty}} {{item.unit}})</div>
                      </td>
                      <template v-if="selectedReceipt.type === 'po'">
                          <td class="px-4 py-3 text-right font-bold text-gray-700 border-l border-gray-100">{{ formatCurrency(item.price) }}</td>
                          <td class="px-4 py-3 text-right font-bold text-emerald-600 border-l border-gray-100">{{ formatCurrency(item.qty * item.price) }}</td>
                      </template>
                    </tr>
                  </tbody>
                  <tfoot v-if="selectedReceipt.type === 'po'" class="bg-gray-50 font-bold border-t border-gray-200">
                      <tr>
                          <td colspan="4" class="px-4 py-3 text-right uppercase text-gray-600">Tổng Giá Trị Chốt Đơn:</td>
                          <td class="px-4 py-3 text-right text-emerald-700 text-lg">{{ formatCurrency(selectedReceipt.totalPrice) }}</td>
                      </tr>
                  </tfoot>
                </table>
                <div v-if="selectedReceipt.type === 'pr' || selectedReceipt.type === 'po'" class="bg-gray-50 p-4 border-t border-gray-200">
                    <p class="text-xs font-bold text-gray-600 uppercase mb-1">Ghi chú phiếu:</p>
                    <p class="text-sm text-gray-800 italic">{{ selectedReceipt.note || 'Không có ghi chú' }}</p>
                </div>
            </div>
          </div>
          
          <div class="px-6 py-4 border-t flex justify-end gap-3 bg-gray-50 shrink-0">
            <button @click="closeModal" class="px-6 py-2.5 border border-gray-300 rounded-lg font-bold text-gray-700 hover:bg-white transition-colors bg-gray-100 shadow-sm">Đóng lại</button>
            <template v-if="selectedReceipt.status === 'pending'">
              <button @click="handleAction('reject', selectedReceipt)" class="px-6 py-2.5 bg-red-100 hover:bg-red-200 text-red-700 rounded-lg font-bold shadow-sm transition-colors border border-red-200 flex items-center gap-2"><XCircleIcon class="w-5 h-5"/> Từ chối phiếu</button>
              <button @click="handleAction('approve', selectedReceipt)" class="px-6 py-2.5 bg-emerald-600 hover:bg-emerald-700 text-white rounded-lg font-bold shadow-sm transition-colors flex items-center gap-2"><CheckCircleIcon class="w-5 h-5"/> Phê Duyệt Ngay</button>
            </template>
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