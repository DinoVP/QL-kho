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

const getAuthHeaders = () => ({ 
  'Content-Type': 'application/json', 
  'Authorization': 'Bearer ' + (localStorage.getItem('authToken') || '') 
})

// MẶC ĐỊNH LÀ QL KHO (Để sếp test màn hình QL Kho tạo phiếu)
const currentUserRole = ref(localStorage.getItem('role') || 'ql_kho')
const myWarehouseId = ref(parseInt(localStorage.getItem('warehouseId')) || null)

// PHÂN QUYỀN
const currentRole = currentUserRole.value.toLowerCase()
const canCreate = computed(() => ['admin', 'ql_kho'].includes(currentRole))
const canViewPrice = computed(() => ['admin', 'giam_doc', 'gd_chi_nhanh'].includes(currentRole))
const canConfirmOutbound = computed(() => ['admin', 'ql_kho'].includes(currentRole))

const outboundReceipts = ref([])
const customers = ref([]) 
const productsList = ref([])
const locationsList = ref([])
const stockList = ref([])
const isLoading = ref(false)

const getToday = () => new Date().toISOString().split('T')[0]
const formatCurrency = (val) => new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(val || 0)

// =========================================================================
// HÀM NÀY BỊ THIẾU Ở BẢN TRƯỚC, GÂY LỖI TRẮNG MÀN HÌNH - NAY ĐÃ ĐƯỢC FIX CỨNG!
// =========================================================================
const getCustomerName = (id) => {
  const cus = customers.value.find(c => c.partnerId === id || c.id === id);
  return cus ? (cus.partnerName || cus.name) : `Khách hàng (ID: ${id})`;
}

const getUnitChain = (prod) => {
    let units = [{ name: prod.unit || prod.Unit || 'SL', rate: 1, rel: 1 }];
    const conversions = prod.conversions || prod.Conversions || [];
    if (conversions.length > 0) {
        const sortedConvs = [...conversions].sort((a,b) => a.rate - b.rate);
        let prevRate = 1;
        sortedConvs.forEach(c => { 
            units.push({ name: c.altUnit, rate: c.rate, rel: c.rate / prevRate }); 
            prevRate = c.rate; 
        });
    } else if ((prod.conversionRate || prod.ConversionRate) > 1) {
        const cr = prod.conversionRate || prod.ConversionRate;
        units.push({ name: 'Thùng/Kiện', rate: cr, rel: cr });
    }
    return units.sort((a,b) => a.rate - b.rate);
}

const autoFormatStockText = (qty, units, baseUnit) => {
    if (qty <= 0) return `0 ${baseUnit}`;
    const sortedPacks = [...(units || [])].sort((a, b) => b.rate - a.rate);
    let remainingQty = qty; 
    let components = []; 
    let topUnit = null;

    for (const pack of sortedPacks) {
        if (pack.rate > 1 && remainingQty >= pack.rate) {
            const count = Math.floor(remainingQty / pack.rate);
            components.push({ count, name: pack.name, rate: pack.rate });
            if (!topUnit) topUnit = pack; 
            remainingQty %= pack.rate;
        }
    }
    
    if (remainingQty > 0 || components.length === 0) {
        components.push({ count: remainingQty, name: baseUnit, rate: 1 });
    }

    if (components.length === 1 && components[0].rate > 1) {
        let chainResult = [];
        const startIndex = sortedPacks.findIndex(u => u.name === topUnit.name);
        for (let i = startIndex; i < sortedPacks.length; i++) {
            const pack = sortedPacks[i];
            if(pack.rate > 1) chainResult.push(`${qty / pack.rate} ${pack.name}`);
        }
        chainResult.push(`${qty} ${baseUnit}`);
        return [...new Set(chainResult)].join(' = ');
    }
    return components.map(c => `${c.count} ${c.name}`).join(' + ');
}

const fetchData = async () => {
  isLoading.value = true
  try {
    const headers = getAuthHeaders()
    const [outRes, stockRes, prodRes, partRes, locRes] = await Promise.all([ 
        fetch(OUTBOUND_API, { headers }), 
        fetch(STOCK_API, { headers }), 
        fetch(PROD_API, { headers }), 
        fetch(PARTNER_API, { headers }), 
        fetch(LOC_API, { headers }) 
    ])
    
    if (prodRes.ok) productsList.value = await prodRes.json()
    if (locRes.ok) locationsList.value = await locRes.json()
    
    if (partRes.ok) { 
        const pData = await partRes.json(); 
        const rawPartners = pData.data || pData.Data || pData || []; 
        customers.value = rawPartners.filter(p => p.isCustomer || p.partnerCode?.startsWith('KH')) 
    }
    
    if (stockRes.ok) {
      const rawStocks = await stockRes.json();
      stockList.value = rawStocks.filter(s => 
          (!myWarehouseId.value || s.warehouseId === myWarehouseId.value || s.WarehouseId === myWarehouseId.value) && 
          (s.locationId != null && s.locationId !== '')
      ).map(s => {
        const prod = productsList.value.find(p => p.id === s.variantId || p.Id === s.variantId) || {};
        const loc = locationsList.value.find(l => l.id === s.locationId || l.Id === s.locationId) || {};
        return { 
            stockId: s.id, 
            variantId: s.variantId, 
            qtyAvailable: s.qty || 0, 
            nsx: s.nsx || '', 
            hsd: s.hsd || '', 
            sku: prod.sku || prod.Sku || 'N/A', 
            name: prod.name || prod.Name || 'Lỗi', 
            unit: prod.unit || prod.Unit || 'Cái', 
            price: prod.price || prod.Price || 0, 
            units: getUnitChain(prod), 
            locationId: s.locationId, 
            locationCode: loc.code || loc.Code || 'Lỗi vị trí' 
        }
      })
    }

    if (outRes.ok) {
      const rawOutbound = await outRes.json()
      outboundReceipts.value = rawOutbound.filter(r => !myWarehouseId.value || r.warehouseId === myWarehouseId.value || r.WarehouseId === myWarehouseId.value).map(r => {
        const cusId = r.customerId || r.CustomerId;
        const cus = customers.value.find(c => c.partnerId === cusId || c.id === cusId);
        r.customerName = cus ? (cus.partnerName || cus.name) : `Khách hàng (ID: ${cusId})`;
        if (!r.code && r.id) r.code = `PX${r.id.toString().padStart(4, '0')}`;
        return r;
      })
    }
  } catch (error) { 
      console.error(error) 
  } finally { 
      isLoading.value = false 
  }
}

const searchQuery = ref('')
const filterStatus = ref('')

const filteredReceipts = computed(() => {
    return outboundReceipts.value.filter(r => {
        const codeMatch = (r.code || r.Code || '').toLowerCase().includes(searchQuery.value.toLowerCase());
        const nameMatch = (r.customerName || '').toLowerCase().includes(searchQuery.value.toLowerCase());
        const statusMatch = filterStatus.value === '' || (r.status || r.Status) === filterStatus.value;
        return (codeMatch || nameMatch) && statusMatch;
    })
})

const showModal = ref(false)
const modalMode = ref('add') 
const formData = ref({ id: 0, code: '', date: getToday(), customerId: '', warehouseId: myWarehouseId.value, items: [], note: '', status: 'pending' })

const openModal = (mode, receipt = null) => {
  modalMode.value = mode
  if (receipt) {
    const safeItems = (receipt.items || receipt.Items || []).map(i => {
      const prod = productsList.value.find(p => p.id === (i.variantId || i.VariantId) || p.Id === (i.variantId || i.VariantId)) || {};
      const loc = locationsList.value.find(l => l.id === (i.locationId || i.LocationId) || l.Id === (i.locationId || i.LocationId)) || {};
      const nsxStr = (i.nsx || i.Nsx)?.split('T')[0] || ''; 
      const hsdStr = (i.hsd || i.Hsd)?.split('T')[0] || '';
      const stock = stockList.value.find(s => s.variantId === (i.variantId || i.VariantId) && s.locationId === (i.locationId || i.LocationId) && s.nsx === nsxStr && s.hsd === hsdStr)
      
      let units = getUnitChain(prod);
      const totalQty = i.qty || i.Qty || 0; 
      let bestUnit = units[0]; 
      let bestQty = totalQty;
      
      for (let u = units.length - 1; u >= 0; u--) { 
          if (totalQty % units[u].rate === 0) { 
              bestUnit = units[u]; 
              bestQty = totalQty / units[u].rate; 
              break; 
          } 
      }
      
      return { 
          variantId: i.variantId || i.VariantId, 
          sku: prod.sku || prod.Sku || (i.sku || i.Sku), 
          name: prod.name || prod.Name || (i.name || i.Name), 
          baseUnit: prod.unit || 'SL', 
          units, 
          selectedUnit: bestUnit.name, 
          inputRate: bestUnit.rate, 
          inputQty: bestQty, 
          qty: totalQty, 
          maxQty: (stock ? stock.qtyAvailable : 0) + totalQty, 
          price: i.price || i.Price || prod.price || 0, 
          locationId: i.locationId || i.LocationId, 
          locationCode: loc.code || loc.Code || 'Lỗi', 
          nsx: nsxStr, 
          hsd: hsdStr 
      };
    });
    formData.value = { ...receipt, items: safeItems, warehouseId: receipt.warehouseId || myWarehouseId.value };
  } else { 
      formData.value = { id: 0, code: '', date: getToday(), customerId: '', warehouseId: myWarehouseId.value, items: [], note: '', status: 'pending' } 
  }
  selectedStockIdsToAdd.value = []; 
  stockSearchQuery.value = ''; 
  showModal.value = true
}

const closeModal = () => showModal.value = false

const getStatusBadge = (status) => {
  switch(status) {
    case 'pending': return { text: 'Chờ duyệt', class: 'bg-amber-100 text-amber-700 border-amber-200' }
    case 'approved': return { text: 'Chờ lấy hàng', class: 'bg-blue-100 text-blue-700 border-blue-200' }
    case 'completed': return { text: 'Đã xuất kho', class: 'bg-emerald-100 text-emerald-700 border-emerald-200' }
    case 'rejected': return { text: 'Từ chối', class: 'bg-red-100 text-red-700 border-red-200' }
    default: return { text: 'Khác', class: 'bg-gray-100 text-gray-500' }
  }
}

const stockSearchQuery = ref('')
const selectedStockIdsToAdd = ref([]) 

const filteredStockList = computed(() => {
    return stockList.value.filter(s => 
        s.sku.toLowerCase().includes(stockSearchQuery.value.toLowerCase()) || 
        s.name.toLowerCase().includes(stockSearchQuery.value.toLowerCase()) || 
        s.locationCode.toLowerCase().includes(stockSearchQuery.value.toLowerCase())
    )
})

const handleAddMultipleStocks = () => {
  selectedStockIdsToAdd.value.forEach(stockId => {
    const stock = stockList.value.find(s => s.stockId === stockId)
    if (stock && !formData.value.items.find(i => i.variantId === stock.variantId && i.locationId === stock.locationId && i.nsx === stock.nsx)) {
      const units = stock.units;
      const defaultUnit = units[units.length - 1]; 
      formData.value.items.push({ 
          variantId: stock.variantId, 
          sku: stock.sku, 
          name: stock.name, 
          baseUnit: stock.unit, 
          units, 
          selectedUnit: defaultUnit.name, 
          inputRate: defaultUnit.rate, 
          inputQty: 1, 
          maxQty: stock.qtyAvailable, 
          price: stock.price, 
          locationId: stock.locationId, 
          locationCode: stock.locationCode, 
          nsx: stock.nsx, 
          hsd: stock.hsd 
      })
    }
  })
  selectedStockIdsToAdd.value = [];
}

const onUnitChange = (item) => {
    const unitDef = item.units.find(u => u.name === item.selectedUnit);
    if (unitDef) {
        item.inputRate = unitDef.rate;
        validateQty(item);
    }
}

const getItemTotalQty = (item) => (item.inputQty || 0) * (item.inputRate || 1)

const validateQty = (item) => { 
    if (getItemTotalQty(item) > item.maxQty) { 
        alert(`Chỉ còn tồn ${item.maxQty}!`); 
        item.inputQty = Math.floor(item.maxQty / item.inputRate); 
    } 
}

const removeItem = (index) => formData.value.items.splice(index, 1)

const totalModalQty = computed(() => {
    return formData.value.items.reduce((sum, i) => sum + (modalMode.value === 'view' ? i.qty : getItemTotalQty(i)), 0)
})

const totalPrice = computed(() => {
    return formData.value.items.reduce((sum, i) => sum + (modalMode.value === 'view' ? (i.qty * i.price) : (getItemTotalQty(i) * i.price)), 0)
})

// === LOGIC TẠO PHIẾU ĐƯỢC CHUẨN HÓA LẠI ===
const handleSubmit = async () => {
  if (formData.value.items.length === 0) return alert('Chưa chọn mặt hàng nào!')
  if (!formData.value.customerId) return alert('Chưa chọn Khách hàng!')
  
  const payload = { 
      ...formData.value, 
      code: modalMode.value === 'add' ? "" : formData.value.code,
      warehouseId: myWarehouseId.value || 1,
      items: formData.value.items.map(i => ({ 
          ...i, 
          qty: getItemTotalQty(i),
          note: `Xuất ${i.inputQty} ${i.selectedUnit}`
      })) 
  }
  
  try {
    const res = await fetch(modalMode.value === 'add' ? OUTBOUND_API : `${OUTBOUND_API}/${formData.value.id}`, { 
        method: modalMode.value === 'add' ? 'POST' : 'PUT', 
        headers: getAuthHeaders(), 
        body: JSON.stringify(payload) 
    })
    
    if (res.ok) { 
        alert('Lưu Phiếu Xuất Thành Công!'); 
        await fetchData(); 
        showModal.value = false; 
    } else {
        alert('Lỗi hệ thống khi lưu phiếu!')
    }
  } catch (error) {
      console.error('Lỗi Submit:', error)
  }
}

const handleDelete = async (id, code) => { 
    if (confirm(`Xóa phiếu xuất [${code}]?`)) { 
        await fetch(`${OUTBOUND_API}/${id}`, { method: 'DELETE', headers: getAuthHeaders() }); 
        await fetchData(); 
    } 
}

const handleCompleteReceipt = async (receipt) => { 
    if (confirm(`Xác nhận phiếu ĐÃ XUẤT KHO? (Hàng bị trừ khỏi Kệ)`)) { 
        await fetch(`${OUTBOUND_API}/${receipt.id || receipt.Id}/complete`, { method: 'PUT', headers: getAuthHeaders() }); 
        await fetchData(); 
    } 
}

const printPDF = () => { alert(`Đang tạo file PDF...`) }

onMounted(() => fetchData())
</script>

<template>
  <div class="space-y-6 animate-fade-in pb-10 px-1 relative">
    
    <div class="flex justify-between items-center">
      <div>
        <h2 class="text-2xl font-bold text-gray-800">Phiếu Xuất kho</h2>
        <p class="text-[10px] font-bold text-indigo-600 bg-indigo-50 px-2 py-1 rounded inline-block mt-1 border border-indigo-200 uppercase">
            Vai trò: {{ currentRole }}
        </p>
      </div>
      <div class="flex gap-2">
        <button @click="printPDF" class="bg-white border border-gray-200 text-red-600 px-3 py-2 rounded-lg text-sm font-semibold hover:bg-red-50 shadow-sm flex items-center gap-1.5">
            <ArrowDownTrayIcon class="w-4 h-4"/> Xuất PDF
        </button>
        <button v-if="canCreate" @click="openModal('add')" class="bg-amber-600 text-white px-4 py-2 rounded-lg flex items-center gap-2 text-sm font-bold shadow-sm hover:bg-amber-700">
            <PlusIcon class="w-5 h-5" /> Lập Phiếu Xuất
        </button>
      </div>
    </div>

    <div class="bg-white p-4 rounded-xl border flex gap-4 shadow-sm">
      <div class="relative flex-1">
          <div class="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none">
              <MagnifyingGlassIcon class="w-5 h-5 text-gray-400" />
          </div>
          <input v-model="searchQuery" type="text" class="block w-full pl-10 pr-3 py-2 border rounded-lg text-sm outline-none focus:ring-1 focus:ring-amber-500" placeholder="Tìm theo mã phiếu, khách hàng...">
      </div>
      <select v-model="filterStatus" class="border rounded-lg text-sm px-4 py-2 outline-none focus:ring-1 focus:ring-amber-500 cursor-pointer">
          <option value="">Tất cả Trạng thái</option>
          <option value="pending">Chờ duyệt</option>
          <option value="approved">Chờ lấy hàng</option>
          <option value="completed">Đã xuất kho</option>
      </select>
    </div>

    <div class="bg-white rounded-xl border shadow-sm overflow-hidden">
      <div class="w-full overflow-x-auto custom-scrollbar">
        <table class="min-w-[1100px] w-full text-sm text-left">
          <thead class="bg-gray-50 uppercase font-bold text-gray-500 text-xs">
            <tr>
              <th class="px-5 py-3 w-24">Mã Phiếu</th>
              <th class="px-5 py-3 w-32">Ngày</th>
              <th class="px-5 py-3 w-40">Khách hàng</th>
              <th class="px-5 py-3 text-right">Tổng SL</th>
              <th v-if="canViewPrice" class="px-5 py-3 text-right w-32">Giá trị</th>
              <th class="px-5 py-3 text-center">Trạng thái</th>
              <th class="px-5 py-3 text-right w-40">Thao tác</th>
            </tr>
          </thead>
          <tbody class="divide-y divide-gray-100 bg-white">
            <tr v-if="filteredReceipts.length === 0">
                <td :colspan="canViewPrice ? 7 : 6" class="px-6 py-12 text-center text-gray-500">Kho này chưa xuất phiếu nào.</td>
            </tr>
            <tr v-for="receipt in filteredReceipts" :key="receipt.id || receipt.Id" class="hover:bg-gray-50 transition-colors">
              <td class="px-5 py-3 font-bold text-amber-700">{{ receipt.code || receipt.Code }}</td>
              <td class="px-5 py-3">{{ receipt.date || receipt.Date }}</td>
              <td class="px-5 py-3 font-bold text-blue-700">{{ getCustomerName(receipt.customerId || receipt.CustomerId) }}</td>
              <td class="px-5 py-3 text-right font-bold">{{ receipt.totalQty || receipt.TotalQty }}</td>
              
              <td v-if="canViewPrice" class="px-5 py-3 text-right font-bold text-emerald-600">
                  {{ formatCurrency(receipt.totalPrice || receipt.TotalPrice) }}
              </td>
              
              <td class="px-5 py-3 text-center whitespace-nowrap">
                  <span :class="['px-2 py-1.5 rounded text-[10px] font-bold uppercase border', getStatusBadge(receipt.status || receipt.Status).class]">
                      {{ getStatusBadge(receipt.status || receipt.Status).text }}
                  </span>
              </td>
              
              <td class="px-5 py-3 text-right space-x-1.5 whitespace-nowrap">
                <button @click="openModal('view', receipt)" class="p-1.5 text-blue-600 hover:bg-blue-50 rounded" title="Xem chi tiết">
                    <EyeIcon class="w-5 h-5" />
                </button>
                
                <button v-if="canCreate && (receipt.status || receipt.Status) === 'pending'" @click="openModal('edit', receipt)" class="p-1.5 text-amber-600 hover:bg-amber-50 rounded ml-1">
                    <PencilSquareIcon class="w-5 h-5" />
                </button>
                
                <button v-if="canCreate && (receipt.status || receipt.Status) === 'pending'" @click="handleDelete(receipt.id || receipt.Id, receipt.code || receipt.Code)" class="p-1.5 text-red-600 hover:bg-red-50 rounded ml-1">
                    <TrashIcon class="w-5 h-5" />
                </button>
                
                <button v-if="canConfirmOutbound && (receipt.status || receipt.Status) === 'approved'" @click="handleCompleteReceipt(receipt)" class="p-1.5 text-purple-600 hover:bg-purple-50 rounded ml-1" title="Xuất kho thực tế">
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
        <div class="bg-white rounded-xl shadow-2xl w-full max-w-[95vw] lg:max-w-7xl flex flex-col max-h-[90vh]">
          
          <div class="px-6 py-4 border-b flex justify-between bg-gray-50 shrink-0">
              <h3 class="font-bold text-lg">
                  <DocumentArrowUpIcon class="w-6 h-6 inline text-amber-600"/> 
                  {{ modalMode === 'add' ? 'Lập Phiếu Xuất Kho' : 'Chi tiết Phiếu Xuất: ' + formData.code }}
              </h3>
              <button @click="closeModal" class="text-gray-400 hover:text-red-500"><XMarkIcon class="w-6 h-6" /></button>
          </div>
          
          <div class="p-6 overflow-y-auto flex-1 space-y-6 custom-scrollbar bg-slate-50/50">
            
            <form id="outboundForm" @submit.prevent="handleSubmit">
                
                <div class="grid grid-cols-4 gap-4 bg-white p-4 rounded-lg border shadow-sm mb-6">
                  <div class="col-span-2">
                      <label class="block text-xs font-bold mb-1">Khách hàng *</label>
                      <select v-model="formData.customerId" :disabled="modalMode === 'view'" required class="w-full border rounded px-3 py-2 text-sm outline-none focus:ring-amber-500 cursor-pointer">
                          <option value="" disabled>-- Chọn Khách hàng --</option>
                          <option v-for="cus in customers" :key="cus.partnerId || cus.id" :value="cus.partnerId || cus.id">{{ cus.partnerName || cus.name }}</option>
                      </select>
                  </div>
                  <div>
                      <label class="block text-xs font-bold mb-1">Ngày xuất</label>
                      <input v-model="formData.date" :disabled="modalMode === 'view'" type="date" required class="w-full border rounded px-3 py-2 text-sm outline-none focus:ring-amber-500 bg-white">
                  </div>
                  <div>
                      <label class="block text-xs font-bold mb-1">Trạng thái</label>
                      <div class="px-3 py-2 text-sm font-bold rounded border text-center" :class="getStatusBadge(formData.status).class">
                          {{ getStatusBadge(formData.status).text }}
                      </div>
                  </div>
                </div>

                <div v-if="modalMode !== 'view'" class="bg-amber-50 p-4 rounded-xl border border-amber-200 mb-6">
                  <label class="text-sm font-bold text-amber-800 mb-2 block">Tra cứu & Chọn hàng ĐÃ ĐƯỢC LÊN KỆ:</label>
                  <input v-model="stockSearchQuery" type="text" class="w-full border border-amber-200 rounded px-3 py-1.5 text-sm mb-2 outline-none focus:ring-1 focus:ring-amber-500 shadow-sm bg-white" placeholder="🔍 Gõ mã SKU, tên SP... (Chỉ hiển thị hàng có Mã Kệ)">
                  
                  <div class="bg-white border border-amber-200 rounded max-h-32 overflow-y-auto p-2">
                    <div v-if="filteredStockList.length === 0" class="p-2 text-sm text-gray-400 italic">Kho đang trống hoặc toàn bộ hàng đang ở Bãi Chờ.</div>
                    <label v-for="stock in filteredStockList" :key="stock.stockId" class="flex items-center gap-3 p-2 hover:bg-amber-50 cursor-pointer border-b">
                      <input type="checkbox" :value="stock.stockId" v-model="selectedStockIdsToAdd" class="w-4 h-4 text-amber-600 rounded">
                      <span class="text-sm flex-1 font-medium">[{{ stock.sku }}] {{ stock.name }} <span class="text-indigo-600 font-bold ml-2">(Kệ: {{ stock.locationCode }})</span></span>
                      <span class="text-xs font-bold px-2 py-1 bg-blue-100 text-blue-700 rounded ml-2">Tồn: {{ stock.qtyAvailable }} {{ stock.unit }}</span>
                    </label>
                  </div>
                  <button type="button" @click="handleAddMultipleStocks" class="mt-2 px-4 py-1.5 bg-amber-600 text-white rounded text-sm font-bold disabled:opacity-50 shadow-sm hover:bg-amber-700" :disabled="selectedStockIdsToAdd.length === 0">
                      Xác nhận lấy {{selectedStockIdsToAdd.length}} Lô xuống phiếu
                  </button>
                </div>

                <div class="border rounded-lg overflow-x-auto shadow-sm bg-white mb-6">
                  <table class="w-full text-sm text-left">
                    <thead class="bg-gray-100 text-xs uppercase font-bold border-b">
                        <tr>
                            <th class="px-2 py-3">SKU</th>
                            <th class="px-2 py-3 min-w-[150px]">Tên Hàng</th>
                            <th class="px-2 py-3 w-28">Kệ / NSX-HSD</th>
                            <th class="px-2 py-3 text-right text-gray-500">Tồn Kệ</th>
                            <th v-if="modalMode !== 'view'" class="px-2 py-3 text-center bg-amber-50/50 border-l w-28 text-amber-700">ĐVT Xuất</th>
                            <th v-if="modalMode !== 'view'" class="px-2 py-3 text-center bg-amber-50/50 w-24 text-amber-700">Hệ số</th>
                            <th v-if="modalMode !== 'view'" class="px-2 py-3 text-center bg-blue-50/50 w-24 text-blue-700">SL Xuất</th>
                            <th class="px-2 py-3 text-center text-emerald-700 border-l min-w-[200px]">Tổng lẻ</th>
                            <th v-if="canViewPrice" class="px-2 py-3 text-right border-l w-28">Đơn Giá</th>
                            <th v-if="canViewPrice" class="px-2 py-3 text-right w-32">Thành tiền</th>
                            <th v-if="modalMode !== 'view'" class="px-2 py-3 text-center border-l w-10">#</th>
                        </tr>
                    </thead>
                    
                    <tbody class="divide-y divide-gray-100">
                      <tr v-if="formData.items.length === 0">
                          <td :colspan="canViewPrice ? 11 : 9" class="text-center py-8 text-gray-500 italic">Chưa có hàng hóa xuất.</td>
                      </tr>
                      
                      <tr v-for="(item, idx) in formData.items" :key="idx" class="border-b hover:bg-gray-50">
                        <td class="px-2 py-2 font-bold">{{item.sku}}</td>
                        <td class="px-2 py-2">{{item.name}}</td>
                        <td class="px-2 py-2 text-xs">
                          <span class="font-bold text-indigo-600 block">{{item.locationCode}}</span>
                          <span class="text-gray-500 block">NSX: {{item.nsx || '--'}}</span>
                          <span class="text-gray-500 block">HSD: {{item.hsd || '--'}}</span>
                        </td>
                        
                        <td class="px-2 py-2 text-right bg-gray-50">
                            <span class="block font-bold text-indigo-700 text-xs">{{ autoFormatStockText(item.maxQty, item.units, item.baseUnit) }}</span>
                        </td>
                        
                        <td v-if="modalMode !== 'view'" class="px-2 py-2 text-center bg-amber-50/20 border-l">
                            <select v-model="item.selectedUnit" @change="onUnitChange(item)" class="w-full border border-amber-200 rounded px-1.5 py-1 text-xs font-bold text-amber-800 bg-white outline-none cursor-pointer">
                                <option v-for="u in item.units" :key="u.name" :value="u.name">{{ u.name }}</option>
                            </select>
                        </td>
                        
                        <td v-if="modalMode !== 'view'" class="px-2 py-2 text-center bg-amber-50/20">
                            <div class="flex items-center gap-1 justify-center">
                                <input v-model.number="item.inputRate" @input="validateQty(item)" type="number" min="1" class="w-12 text-center border border-amber-300 rounded px-1 py-1 text-xs font-bold text-amber-700 outline-none focus:ring-1 focus:ring-amber-500 bg-white">
                                <span class="text-[9px] font-bold text-gray-400 block">{{ item.baseUnit }}</span>
                            </div>
                        </td>

                        <td v-if="modalMode !== 'view'" class="px-2 py-2 text-center bg-blue-50/20">
                            <input v-model.number="item.inputQty" @input="validateQty(item)" type="number" min="1" class="w-full text-center border border-blue-300 rounded px-1 py-1 text-sm font-bold text-blue-700 outline-none focus:ring-1 focus:ring-blue-500 bg-white shadow-inner">
                        </td>

                        <td class="px-2 py-2 text-center border-l bg-emerald-50/10">
                          <div class="text-[12px] font-bold text-emerald-800 bg-emerald-50 px-3 py-2 rounded border border-emerald-200 inline-block shadow-sm text-center w-full">
                              {{ autoFormatStockText(getItemTotalQty(item), item.units, item.baseUnit) }}
                          </div>
                        </td>

                        <td v-if="canViewPrice" class="px-2 py-2 text-right border-l bg-indigo-50/20">
                          <input v-if="modalMode === 'add'" v-model.number="item.price" type="number" min="0" class="w-full text-right border border-indigo-300 rounded px-1 py-1 text-sm font-bold text-indigo-700 bg-white outline-none">
                          <span v-else class="font-bold text-gray-700">{{formatCurrency(item.price)}}</span>
                        </td>
                        
                        <td v-if="canViewPrice" class="px-2 py-2 text-right font-bold text-emerald-600 border-l bg-indigo-50/20">
                            <span v-if="modalMode !== 'view'">{{ formatCurrency(getItemTotalQty(item) * (item.price || 0)) }}</span>
                            <span v-else>{{ formatCurrency((item.qty || 0) * (item.price || 0)) }}</span>
                        </td>
                        
                        <td v-if="modalMode !== 'view'" class="px-2 py-2 text-center border-l">
                            <button type="button" @click="removeItem(idx)" class="text-red-400 hover:text-red-600"><TrashIcon class="w-4 h-4 mx-auto"/></button>
                        </td>
                      </tr>
                    </tbody>
                    
                    <tfoot class="bg-gray-50 font-bold border-t">
                        <tr>
                          <td :colspan="modalMode !== 'view' ? 7 : 4" class="px-4 py-3 text-right uppercase text-gray-600">Tổng kết:</td>
                          <td class="px-4 py-3 text-center text-emerald-700 text-xl">{{ totalModalQty }}</td>
                          <td v-if="canViewPrice" class="px-4 py-3 border-l"></td>
                          <td v-if="canViewPrice" class="px-4 py-3 text-right text-emerald-700 border-l">{{ formatCurrency(totalPrice) }}</td>
                          <td v-if="modalMode !== 'view'" class="border-l"></td>
                        </tr>
                    </tfoot>
                  </table>
                </div>
                
                <div>
                    <label class="block text-xs font-bold mb-1">Ghi chú</label>
                    <input v-model="formData.note" :disabled="modalMode === 'view'" class="w-full border rounded-lg px-3 py-2 text-sm outline-none focus:ring-amber-500 bg-white" placeholder="Ghi chú thêm thông tin xe tải, người nhận...">
                </div>

            </form>
          </div>
          
          <div class="px-6 py-4 border-t flex justify-end gap-3 bg-gray-50 shrink-0">
            <button type="button" @click="closeModal" class="px-5 py-2.5 border rounded-lg font-bold hover:bg-gray-100 transition-colors bg-white">
                {{ modalMode === 'view' ? 'Đóng lại' : 'Hủy bỏ' }}
            </button>
            <button v-if="modalMode !== 'view'" type="submit" form="outboundForm" class="px-6 py-2.5 bg-amber-600 text-white rounded-lg font-bold hover:bg-amber-700 transition-colors shadow-md">
                Hoàn tất Phiếu Xuất
            </button>
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
</style>