<script setup>
import { ref, computed, onMounted } from 'vue'
import { useAuth } from '@/composables/useAuth' 
import { 
  MagnifyingGlassIcon, PlusIcon, EyeIcon, XMarkIcon, 
  TrashIcon, CheckCircleIcon, ArrowPathIcon, MapPinIcon,
  PrinterIcon, DocumentTextIcon, TruckIcon, LockClosedIcon,
  ArrowRightCircleIcon
} from '@heroicons/vue/24/outline'

const TRANSFER_API = 'https://localhost:7139/api/Transfer'
const STOCK_API = 'https://localhost:7139/api/Stock' 
const PROD_API = 'https://localhost:7139/api/Products'
const LOC_API = 'https://localhost:7139/api/Locations'
const BRANCH_API = 'https://localhost:7139/api/Branches' 

const getAuthHeaders = () => ({ 'Content-Type': 'application/json', 'Authorization': 'Bearer ' + (localStorage.getItem('authToken') || '') })

// PHÂN QUYỀN CHUẨN NGHIỆP VỤ KHO
const getSafeRole = () => (localStorage.getItem('role') || localStorage.getItem('Role') || '').toLowerCase();
const getSafeBranchId = () => {
    const bId = localStorage.getItem('branchId') || localStorage.getItem('BranchId');
    return bId ? parseInt(bId) : null; 
}
const getSafeWarehouseId = () => {
    const wId = localStorage.getItem('warehouseId') || localStorage.getItem('WarehouseId');
    return wId ? parseInt(wId) : null; 
}

const userRole = ref(getSafeRole())
const userBranchId = ref(getSafeBranchId())
const userWarehouseId = ref(getSafeWarehouseId()) 

const canCreateTransfer = computed(() => userRole.value === 'gd_chi_nhanh')
const canExport = computed(() => ['admin', 'gd_chi_nhanh'].includes(userRole.value))
const canDispatch = (fromWhId) => userRole.value === 'ql_kho' && userWarehouseId.value === fromWhId;
const canReceive = (toWhId) => userRole.value === 'ql_kho' && userWarehouseId.value === toWhId;

const transferReceipts = ref([])
const warehousesList = ref([]) 
const productsList = ref([]); const locationsList = ref([]); const stockList = ref([])
const isLoading = ref(false)

const getToday = () => new Date().toISOString().split('T')[0]

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

const availableFromWarehouses = computed(() => {
    if (userBranchId.value) {
        const filtered = warehousesList.value.filter(wh => wh.branchId === userBranchId.value);
        return filtered.length > 0 ? filtered : warehousesList.value;
    }
    return warehousesList.value;
})

const fetchData = async () => {
  isLoading.value = true
  try {
    const headers = getAuthHeaders()
    const branchRes = await fetch(BRANCH_API, { headers })
    if (branchRes.ok) {
        const branches = await branchRes.json()
        let allWh = []
        for (const b of branches) {
            const whRes = await fetch(`${BRANCH_API}/${b.id || b.Id}/warehouses-detail`, { headers })
            if(whRes.ok) {
                const whData = await whRes.json()
                allWh = [...allWh, ...whData.map(w => ({ id: w.warehouseId || w.WarehouseId || w.id, name: w.whname || w.Whname || w.name, branchId: b.id || b.Id }))]
            }
        }
        warehousesList.value = allWh
    }

    const [transRes, stockRes, prodRes, locRes] = await Promise.all([ fetch(TRANSFER_API, { headers }), fetch(STOCK_API, { headers }), fetch(PROD_API, { headers }), fetch(LOC_API, { headers }) ])
    if (prodRes.ok) productsList.value = await prodRes.json()
    if (locRes.ok) locationsList.value = await locRes.json()
    
    if (stockRes.ok) {
      const rawStocks = await stockRes.json();
      stockList.value = rawStocks.map(s => {
        const prod = productsList.value.find(p => p.id === s.variantId || p.Id === s.variantId) || {};
        const loc = locationsList.value.find(l => l.id === s.locationId || l.Id === s.locationId) || {};
        return {
          stockId: s.id || s.Id, variantId: s.variantId || s.VariantId, 
          qtyAvailable: s.qty || s.Qty || 0, nsx: s.nsx || s.Nsx || '', hsd: s.hsd || s.Hsd || '',
          sku: prod.sku || prod.Sku || 'N/A', name: prod.name || prod.Name || 'Lỗi', 
          baseUnit: prod.unit || prod.Unit || 'SL',
          units: getUnitChain(prod), 
          locationId: s.locationId || s.LocationId, locationCode: loc.code || loc.Code || 'Khu chung',
          warehouseId: loc.warehouseId || loc.WarehouseId || 1 
        }
      }).filter(s => s.qtyAvailable > 0)
    }

    if (transRes.ok) {
      const rawTrans = await transRes.json()
      transferReceipts.value = rawTrans.map(r => {
        const fromWh = warehousesList.value.find(w => w.id === r.fromWarehouseId || w.Id === r.fromWarehouseId);
        const toWh = warehousesList.value.find(w => w.id === r.toWarehouseId || w.Id === r.toWarehouseId);
        return {
          ...r,
          code: r.code || r.Code || `DC${(r.id||r.Id).toString().padStart(4,'0')}`,
          fromWarehouseName: fromWh ? fromWh.name : `Kho ID ${r.fromWarehouseId||r.FromWarehouseId}`,
          toWarehouseName: toWh ? toWh.name : `Kho ID ${r.toWarehouseId||r.ToWarehouseId}`,
          totalQty: (r.items || r.Items || []).reduce((s, i) => s + (i.qty || i.Qty || 0), 0)
        }
      }).sort((a,b) => (b.id||b.Id) - (a.id||a.Id))
    }
  } catch (error) { console.error(error) } finally { isLoading.value = false }
}

const searchQuery = ref(''); const filterStatus = ref('')
const filteredReceipts = computed(() => {
  return transferReceipts.value.filter(r => {
    const code = r.code || r.Code || ''; 
    const matchSearch = code.toLowerCase().includes(searchQuery.value.toLowerCase())
    return matchSearch && (filterStatus.value === '' || r.status === filterStatus.value)
  })
})

const showModal = ref(false); const modalMode = ref('add') 
const formData = ref({ id: 0, code: '', date: getToday(), fromWarehouseId: '', toWarehouseId: '', items: [], status: 'pending', note: '' })

const openModal = (mode, receipt = null) => {
  modalMode.value = mode
  if (receipt) {
    const safeItems = (receipt.items || receipt.Items || []).map(i => {
      const variantId = i.variantId || i.VariantId;
      const prod = productsList.value.find(p => p.id === variantId || p.Id === variantId) || {};
      const units = getUnitChain(prod);
      const totalQty = i.qty || i.Qty || 1;
      
      let bestUnit = units[0]; let bestInputQty = totalQty;
      for (let u = units.length - 1; u >= 0; u--) {
          if (totalQty > 0 && totalQty % units[u].rate === 0) {
              bestUnit = units[u]; bestInputQty = totalQty / units[u].rate; break;
          }
      }

      const fromLoc = locationsList.value.find(l => l.id === i.fromLocationId) || {};
      const stock = stockList.value.find(s => s.variantId === variantId && s.locationId === i.fromLocationId && s.nsx === (i.nsx||i.Nsx)?.split('T')[0] && s.hsd === (i.hsd||i.Hsd)?.split('T')[0])
      const available = stock ? stock.qtyAvailable : 0;

      return {
        variantId: variantId, sku: prod.sku || prod.Sku, name: prod.name || prod.Name, 
        baseUnit: prod.unit || prod.Unit, units: units,
        selectedUnit: bestUnit.name, inputQty: bestInputQty, transferQty: totalQty,
        maxQty: available + totalQty, 
        fromLocationId: i.fromLocationId, fromLocationCode: fromLoc.code || fromLoc.Code || 'Khu chung',
        toLocationId: null, 
        nsx: i.nsx || i.Nsx ? (i.nsx || i.Nsx).split('T')[0] : '', hsd: i.hsd || i.Hsd ? (i.hsd || i.Hsd).split('T')[0] : '',
        error: false
      };
    });
    formData.value = { 
        id: receipt.id || receipt.Id, code: receipt.code || receipt.Code, date: receipt.date || receipt.Date, 
        fromWarehouseId: receipt.fromWarehouseId || receipt.FromWarehouseId || '', 
        toWarehouseId: receipt.toWarehouseId || receipt.ToWarehouseId || '',
        items: safeItems, note: receipt.note || receipt.Note, status: receipt.status || receipt.Status 
    };
  } else { formData.value = { id: 0, code: '', date: getToday(), fromWarehouseId: '', toWarehouseId: '', items: [], note: '', status: 'pending' } }
  selectedStockIdsToAdd.value = []; stockSearchQuery.value = ''; showModal.value = true
}

const closeModal = () => showModal.value = false

const getStatusBadge = (status) => {
  switch(status) {
    case 'pending': return { text: 'Chờ vận chuyển', class: 'bg-amber-100 text-amber-700 border-amber-200' }
    case 'shipping': return { text: 'Đang vận chuyển', class: 'bg-blue-100 text-blue-700 border-blue-200 animate-pulse' }
    case 'completed': return { text: 'Đã nhận đủ', class: 'bg-emerald-100 text-emerald-700 border-emerald-200' }
    default: return { text: 'Khác', class: 'bg-gray-100 text-gray-500' }
  }
}

const stockSearchQuery = ref(''); const selectedStockIdsToAdd = ref([]) 
const filteredStockList = computed(() => {
  let list = stockList.value.filter(s => s.warehouseId === formData.value.fromWarehouseId);
  if (stockSearchQuery.value) {
      list = list.filter(s => s.sku.toLowerCase().includes(stockSearchQuery.value.toLowerCase()) || s.name.toLowerCase().includes(stockSearchQuery.value.toLowerCase()));
  }
  return list;
})

const handleFromWarehouseChange = () => {
    formData.value.items = [];
    selectedStockIdsToAdd.value = [];
    if(formData.value.fromWarehouseId === formData.value.toWarehouseId) formData.value.toWarehouseId = '';
}

const handleAddMultipleStocks = () => {
  if (selectedStockIdsToAdd.value.length === 0) return;
  selectedStockIdsToAdd.value.forEach(stockId => {
    const stock = stockList.value.find(s => s.stockId === stockId)
    if (stock) {
      const exists = formData.value.items.find(i => i.variantId === stock.variantId && i.fromLocationId === stock.locationId && i.nsx === stock.nsx && i.hsd === stock.hsd)
      if(!exists) {
        const defaultUnit = stock.units.length > 1 ? stock.units[stock.units.length - 1] : stock.units[0];
        formData.value.items.push({ 
          variantId: stock.variantId, sku: stock.sku, name: stock.name, 
          baseUnit: stock.baseUnit, units: stock.units,
          selectedUnit: defaultUnit.name, inputQty: 1, transferQty: defaultUnit.rate, 
          maxQty: stock.qtyAvailable, fromLocationId: stock.locationId, fromLocationCode: stock.locationCode, 
          toLocationId: null, nsx: stock.nsx, hsd: stock.hsd, error: false 
        })
      }
    }
  })
  selectedStockIdsToAdd.value = []; stockSearchQuery.value = '' 
}

const calcActual = (item) => {
    const unitDef = item.units.find(u => u.name === item.selectedUnit);
    const rate = unitDef ? unitDef.rate : 1;
    item.transferQty = (item.inputQty || 0) * rate;
    
    if(item.transferQty > item.maxQty) { 
        item.error = true;
    } else {
        item.error = false;
    }
}

const removeItem = (index) => formData.value.items.splice(index, 1)
const totalQty = computed(() => formData.value.items.reduce((sum, item) => sum + (item.transferQty || 0), 0))

const handleSubmit = async () => {
  if (!formData.value.fromWarehouseId || !formData.value.toWarehouseId) return alert('Sếp chưa chọn Kho Xuất hoặc Kho Nhập!');
  if (formData.value.fromWarehouseId === formData.value.toWarehouseId) return alert('Lỗi: Kho nhập và Kho xuất không được trùng nhau!');
  if (formData.value.items.length === 0) return alert('Chưa chọn mặt hàng nào cần chuyển!')
  if (formData.value.items.some(i => i.error)) return alert('Lỗi: Đang chuyển quá số lượng Tồn Kho cho phép!');
  
  try {
    const cleanPayload = {
        id: modalMode.value === 'add' ? 0 : formData.value.id,
        code: modalMode.value === 'add' ? "" : formData.value.code,
        date: formData.value.date,
        fromWarehouseId: formData.value.fromWarehouseId,
        toWarehouseId: formData.value.toWarehouseId,
        status: 'pending',
        note: formData.value.note || '',
        items: formData.value.items.map(i => ({
            variantId: i.variantId,
            fromLocationId: i.fromLocationId,
            toLocationId: i.toLocationId,
            qty: i.transferQty, 
            nsx: i.nsx,
            hsd: i.hsd
        }))
    }

    const method = modalMode.value === 'add' ? 'POST' : 'PUT';
    const url = modalMode.value === 'add' ? TRANSFER_API : `${TRANSFER_API}/${formData.value.id}`;
        
    const res = await fetch(url, { method, headers: getAuthHeaders(), body: JSON.stringify(cleanPayload) })
    if (res.ok) { alert('Lập lệnh Điều chuyển thành công!'); await fetchData(); closeModal(); } 
    else { let errMsg = "Lỗi hệ thống!"; try { const t = await res.text(); if(t) errMsg = JSON.parse(t).message || errMsg; } catch(e){} alert('LỖI: ' + errMsg); }
  } catch(e) { console.error(e) }
}

// HÀM 1: KHO XUẤT BẤM VẬN CHUYỂN
const handleShipping = async (id) => {
    if (!confirm('Kho xuất xác nhận: Bắt đầu xuất hàng vận chuyển đi?')) return;
    try {
        const res = await fetch(`${TRANSFER_API}/${id}/shipping`, { 
            method: 'PUT', 
            headers: getAuthHeaders() 
        });
        if (res.ok) {
            alert('Đã xuất hàng! Hàng đang được vận chuyển.');
            await fetchData();
        } else {
            alert('LỖI 404: Sếp cần thêm hàm /shipping vào C# thì mới chạy được nhé!');
        }
    } catch (e) { console.error(e) }
}

// HÀM 2: KHO NHẬP BẤM NHẬN HÀNG (TRẢ LẠI API GỐC CỦA SẾP)
const handleCompleteTransfer = async (receipt) => {
  if (!confirm(`XÁC NHẬN KHO ĐÍCH ĐÃ NHẬN ĐỦ HÀNG?\nHàng sẽ được đưa vào "Khu Chờ Nhập" của Kho mới.`)) return;
  try {
    const res = await fetch(`${TRANSFER_API}/${receipt.id || receipt.Id}/complete`, { 
        method: 'PUT', 
        headers: getAuthHeaders() 
    })
    if (res.ok) { alert('Hoàn tất! Hàng đã cập bến an toàn.'); await fetchData(); } 
    else { let errMsg = "Lỗi hệ thống!"; try { const t = await res.text(); if(t) errMsg = JSON.parse(t).message || errMsg; } catch(e){} alert('LỖI: ' + errMsg); }
  } catch(e) { console.error(e) }
}

onMounted(() => fetchData())
</script>

<template>
  <div class="space-y-6 pb-10 px-1 relative">
    <div class="flex flex-col sm:flex-row sm:items-center justify-between gap-4">
      <div>
        <h2 class="text-2xl font-bold text-gray-800">Điều chuyển Nội bộ</h2>
        <p class="text-sm text-gray-500 mt-1">Lập lệnh và luân chuyển hàng hóa</p>
      </div>
      <div class="flex gap-2">
          <button v-if="canExport" @click="() => alert('Chức năng đang phát triển...')" class="bg-white border border-emerald-200 text-emerald-700 px-3 py-2.5 rounded-lg text-sm font-semibold hover:bg-emerald-50 shadow-sm flex items-center gap-1.5"><DocumentTextIcon class="w-4 h-4"/> Xuất Excel</button>
          
          <button v-if="canCreateTransfer" @click="openModal('add')" class="bg-blue-600 hover:bg-blue-700 text-white px-4 py-2.5 rounded-lg font-bold flex items-center gap-2 shadow-md transition-colors">
            <PlusIcon class="w-5 h-5"/> Lập lệnh điều chuyển
          </button>
      </div>
    </div>

    <div class="bg-white p-3 md:p-4 rounded-xl border border-gray-200 flex flex-col sm:flex-row items-center gap-3 md:gap-4 shadow-sm">
      <div class="relative w-full sm:flex-1"><div class="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none"><MagnifyingGlassIcon class="w-5 h-5 text-gray-400" /></div><input v-model="searchQuery" type="text" class="block w-full pl-10 pr-3 py-2 border border-gray-200 rounded-lg text-sm outline-none focus:ring-1 focus:ring-blue-500" placeholder="Tìm theo mã phiếu..."></div>
      <select v-model="filterStatus" class="w-full sm:w-auto border border-gray-200 rounded-lg text-sm px-4 py-2 outline-none focus:ring-1 focus:ring-blue-500 cursor-pointer">
          <option value="">Tất cả Trạng thái</option>
          <option value="pending">Chờ vận chuyển</option>
          <option value="shipping">Đang vận chuyển</option>
          <option value="completed">Đã nhận đủ</option>
      </select>
    </div>

    <div class="bg-white rounded-xl border border-gray-200 shadow-sm overflow-hidden">
      <div class="overflow-x-auto custom-scrollbar">
          <table class="w-full text-sm text-left min-w-[1000px]">
            <thead class="bg-gray-50 text-gray-500 font-bold uppercase text-xs border-b">
              <tr>
                <th class="px-5 py-4 w-28">Mã Lệnh</th>
                <th class="px-5 py-4 text-orange-700">Từ Kho (Xuất đi)</th>
                <th class="px-5 py-4 text-emerald-700">Đến Kho (Nhận)</th>
                <th class="px-5 py-4 text-center">Tổng SL Gốc</th>
                <th class="px-5 py-4 text-center">Trạng Thái</th>
                <th class="px-5 py-4 text-right">Thao tác xử lý</th>
              </tr>
            </thead>
            <tbody class="divide-y divide-gray-100">
              <tr v-if="transferReceipts.length === 0"><td colspan="6" class="px-6 py-12 text-center text-gray-400"><ArrowsRightLeftIcon class="w-12 h-12 text-gray-300 mx-auto mb-3" />Chưa có Phiếu Điều chuyển nào.</td></tr>
              <tr v-for="r in transferReceipts" :key="r.id" class="hover:bg-gray-50 transition-colors">
                <td class="px-5 py-4 font-bold text-blue-700">{{ r.code }}</td>
                <td class="px-5 py-4 font-bold text-orange-700"><MapPinIcon class="w-4 h-4 inline mr-1 -mt-0.5 text-orange-400"/>{{ r.fromWarehouseName }}</td>
                <td class="px-5 py-4 font-bold text-emerald-700"><MapPinIcon class="w-4 h-4 inline mr-1 -mt-0.5 text-emerald-400"/>{{ r.toWarehouseName }}</td>
                <td class="px-5 py-4 text-center font-bold text-blue-800 text-lg">{{ r.totalQty }}</td>
                <td class="px-5 py-4 text-center">
                  <span :class="['px-2.5 py-1.5 rounded-md text-[10px] font-bold uppercase border tracking-wide', getStatusBadge(r.status).class]">
                    {{ getStatusBadge(r.status).text }}
                  </span>
                </td>
                <td class="px-5 py-4 text-right space-x-2 whitespace-nowrap">
                  
                  <button v-if="r.status === 'pending' && canDispatch(r.fromWarehouseId || r.FromWarehouseId)" 
                    @click="handleShipping(r.id || r.Id)"
                    class="bg-orange-50 text-orange-700 border border-orange-200 px-3 py-1.5 rounded-md font-bold text-xs hover:bg-orange-100 shadow-sm transition-colors">
                    <ArrowRightCircleIcon class="w-4 h-4 inline mr-1 -mt-0.5"/> Vận chuyển
                  </button>

                  <button v-if="r.status === 'shipping' && canReceive(r.toWarehouseId || r.ToWarehouseId)" 
                    @click="handleCompleteTransfer(r)"
                    class="bg-emerald-50 text-emerald-700 border border-emerald-200 px-3 py-1.5 rounded-md font-bold text-xs hover:bg-emerald-100 shadow-sm transition-colors">
                    <CheckCircleIcon class="w-4 h-4 inline mr-1 -mt-0.5"/> Nhận đầy đủ hàng
                  </button>

                  <button @click="modalMode='view'; showModal=true; formData = {...r}" class="p-1.5 text-gray-400 hover:text-blue-600 transition-colors" title="Xem chi tiết"><EyeIcon class="w-5 h-5"/></button>
                </td>
              </tr>
            </tbody>
          </table>
      </div>
    </div>

    <Teleport to="body">
      <div v-if="showModal" class="fixed inset-0 z-50 flex items-center justify-center px-4 bg-slate-900/50 backdrop-blur-sm animate-fade-in">
        <div class="bg-white rounded-xl shadow-2xl w-full max-w-[95vw] lg:max-w-6xl overflow-hidden flex flex-col max-h-[95vh]">
          
          <div class="px-6 py-4 border-b border-blue-100 flex items-center justify-between bg-blue-50 shrink-0"><h3 class="text-lg font-bold flex items-center gap-2 text-blue-800"><ArrowPathIcon class="w-6 h-6 text-blue-600"/> {{ modalMode === 'add' ? 'Lập Lệnh Điều Chuyển' : `Chi tiết: ${formData.code}` }}</h3><button @click="closeModal" class="text-gray-400 hover:text-red-500"><XMarkIcon class="w-6 h-6" /></button></div>
          
          <div class="p-6 overflow-y-auto flex-1 custom-scrollbar bg-slate-50/50">
            <form id="transferForm" @submit.prevent="handleSubmit" class="space-y-6">
              
              <div class="bg-white p-4 rounded-xl border border-gray-200 shadow-sm flex flex-wrap gap-4">
                  <div class="flex-1 min-w-[200px]"><label class="block text-xs font-bold mb-1">Ngày luân chuyển *</label><input v-model="formData.date" :disabled="modalMode === 'view'" type="date" required class="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm outline-none focus:ring-1 focus:ring-blue-500 bg-white disabled:bg-gray-100"></div>
                  <div class="flex-1 min-w-[200px] text-center rounded-lg border py-2" :class="getStatusBadge(formData.status).class"><label class="block text-xs font-bold mb-1 uppercase tracking-wider">Trạng thái</label><span class="font-bold">{{ getStatusBadge(formData.status).text }}</span></div>
              </div>

              <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
                  <div class="bg-orange-50 p-4 rounded-xl border border-orange-200 shadow-sm">
                      <label class="block text-xs font-bold mb-1 text-orange-800">BƯỚC 1: TỪ KHO (Kho xuất) *</label>
                      <select v-model="formData.fromWarehouseId" @change="handleFromWarehouseChange" :disabled="modalMode === 'view'" required class="w-full border border-orange-300 font-bold text-orange-700 rounded-lg px-3 py-2 text-sm outline-none focus:ring-2 focus:ring-orange-500 bg-white disabled:bg-gray-100 cursor-pointer">
                          <option value="" disabled>-- Chọn kho lấy hàng --</option>
                          <option v-for="w in availableFromWarehouses" :key="w.id" :value="w.id">{{ w.name }}</option>
                      </select>
                  </div>
                  <div class="bg-emerald-50 p-4 rounded-xl border border-emerald-200 shadow-sm relative">
                      <div v-if="!formData.fromWarehouseId && modalMode === 'add'" class="absolute inset-0 bg-white/60 backdrop-blur-[1px] flex items-center justify-center z-10 rounded-xl"></div>
                      <label class="block text-xs font-bold mb-1 text-emerald-800">BƯỚC 3: ĐẾN KHO (Kho nhập) *</label>
                      <select v-model="formData.toWarehouseId" :disabled="modalMode === 'view'" required class="w-full border border-emerald-300 font-bold text-emerald-700 rounded-lg px-3 py-2 text-sm outline-none focus:ring-2 focus:ring-emerald-500 bg-white disabled:bg-gray-100 cursor-pointer">
                          <option value="" disabled>-- Chọn kho đích đến --</option>
                          <option v-for="w in warehousesList" :key="w.id" :value="w.id" :disabled="w.id === formData.fromWarehouseId" :class="w.id === formData.fromWarehouseId ? 'text-gray-300 italic' : ''">{{ w.name }} {{ w.id === formData.fromWarehouseId ? '(Đang là kho xuất)' : '' }}</option>
                      </select>
                  </div>
              </div>

              <div v-if="modalMode === 'add'" class="bg-blue-50 p-4 rounded-xl border border-blue-200 relative shadow-sm">
                  <div v-if="!formData.fromWarehouseId" class="absolute inset-0 bg-white/80 backdrop-blur-[2px] flex flex-col items-center justify-center z-10 rounded-xl border border-gray-200">
                     <LockClosedIcon class="w-8 h-8 text-gray-400 mb-2" />
                     <p class="text-sm font-bold text-gray-600">Vui lòng chọn TỪ KHO (Bước 1) để lấy danh sách hàng tồn.</p>
                  </div>
                  <label class="text-xs font-bold text-blue-800 mb-2 block">BƯỚC 2: Chọn hàng TỪ TỒN KHO để chuyển đi:</label>
                  <div class="relative mb-2"><div class="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none"><MagnifyingGlassIcon class="w-4 h-4 text-blue-500" /></div><input v-model="stockSearchQuery" type="text" class="w-full border border-blue-300 rounded pl-9 pr-3 py-2 text-sm focus:ring-2 focus:ring-blue-500 outline-none bg-white" placeholder="Gõ mã SKU, tên SP để tìm hàng đang tồn..."></div>
                  
                  <div class="bg-white border border-blue-200 rounded max-h-48 overflow-y-auto">
                      <div v-if="filteredStockList.length===0 && formData.fromWarehouseId" class="p-4 text-center text-sm text-gray-500">Kho này hiện trống hoặc không tìm thấy hàng.</div>
                      <label v-for="stock in filteredStockList" :key="stock.stockId" class="flex items-center gap-3 p-3 border-b border-gray-100 hover:bg-blue-50/50 cursor-pointer">
                          <input type="checkbox" :value="stock.stockId" v-model="selectedStockIdsToAdd" class="w-4 h-4 text-blue-600 rounded cursor-pointer focus:ring-blue-500">
                          <div class="flex-1"><p class="text-sm font-bold text-gray-800">[{{stock.sku}}] {{stock.name}}</p><p class="text-[10px] text-gray-500">NSX: {{stock.nsx || '--'}} | HSD: <span class="text-red-500 font-medium">{{stock.hsd || '--'}}</span> <span v-if="stock.batchNo">| Lô: {{stock.batchNo}}</span></p></div>
                          <div class="text-right">
                              <span class="text-xs font-bold text-orange-600 bg-orange-100 px-2 py-0.5 rounded border border-orange-200">Đang ở Kệ: {{stock.locationCode}}</span>
                              <span class="ml-2 text-xs font-bold text-emerald-700 bg-emerald-100 px-2 py-0.5 rounded border border-emerald-200">Tồn Lô: {{stock.qtyAvailable}} {{stock.baseUnit}}</span>
                          </div>
                      </label>
                  </div>
                  <button type="button" @click="handleAddMultipleStocks" :disabled="selectedStockIdsToAdd.length === 0" class="mt-3 bg-blue-600 hover:bg-blue-700 disabled:bg-gray-400 text-white px-4 py-2 rounded text-sm font-bold shadow-sm transition-colors">Xác nhận đưa {{selectedStockIdsToAdd.length}} Lô vào phiếu</button>
              </div>

              <div v-if="formData.items.length > 0">
                <h4 class="text-sm font-bold mb-2 text-gray-800 uppercase">Danh sách Hàng hóa Điều chuyển</h4>
                <div class="border border-gray-200 rounded-xl overflow-x-auto shadow-sm bg-white">
                  <table class="w-full text-sm text-left">
                    <thead class="bg-gray-100 text-xs uppercase font-bold text-gray-600 border-b border-gray-200">
                      <tr>
                          <th class="px-4 py-3 min-w-[200px]">SKU / Tên Hàng</th>
                          <th class="px-4 py-3 w-32 border-l border-gray-200">Từ Kệ (Xuất đi)</th>
                          <th class="px-4 py-3 w-32 border-l border-gray-200 text-center text-emerald-800 bg-emerald-50">Tồn Lô (Gốc)</th>
                          
                          <th class="px-4 py-3 w-32 border-l border-gray-200 bg-blue-50 text-center text-blue-800">Đơn Vị Chuyển</th>
                          <th class="px-4 py-3 w-36 border-l border-gray-200 bg-blue-50 text-center text-blue-800">Số Lượng Chuyển</th>
                          
                          <th v-if="modalMode === 'add'" class="px-4 py-3 text-center w-12 border-l border-gray-200">#</th>
                      </tr>
                    </thead>
                    <tbody class="divide-y divide-gray-100">
                      <tr v-for="(item, idx) in formData.items" :key="idx" class="hover:bg-blue-50/30 transition-colors" :class="{'bg-red-50': item.error}">
                        <td class="px-4 py-3">
                          <span class="block font-bold text-gray-800">{{ item.sku }}</span>
                          <span class="block text-xs text-gray-600 mt-0.5">{{ item.name }}</span>
                          <span class="text-[9px] text-gray-500 border border-gray-200 rounded px-1 bg-gray-50 mt-1 inline-block font-bold">ĐVT Gốc: {{item.baseUnit}}</span>
                        </td>
                        <td class="px-4 py-3 border-l border-gray-100">
                            <span class="font-bold text-orange-600 text-xs block">{{item.fromLocationCode}}</span>
                            <span class="text-[10px] text-gray-500 block mt-1">HSD: <span class="text-red-500">{{item.hsd || '--'}}</span></span>
                        </td>
                        <td class="px-4 py-3 font-bold text-emerald-700 text-center text-lg bg-emerald-50/30 border-l border-gray-100">{{item.maxQty}}</td>
                        
                        <td class="px-4 py-3 text-center border-l border-gray-100">
                            <select v-if="modalMode === 'add'" v-model="item.selectedUnit" @change="calcActual(item)" class="w-full py-1.5 px-2 bg-white border border-blue-300 rounded text-xs font-bold outline-none cursor-pointer focus:ring-2 focus:ring-blue-500 text-blue-800">
                                <option v-for="u in item.units" :key="u.name" :value="u.name">{{ u.name }}</option>
                            </select>
                            <span v-else class="font-bold text-gray-700">{{item.selectedUnit}}</span>
                        </td>

                        <td class="px-4 py-3 border-l border-gray-100 text-center bg-blue-50/20">
                            <div v-if="modalMode === 'add'">
                                <input v-model.number="item.inputQty" @input="calcActual(item)" type="number" min="1" class="w-20 text-center py-1.5 font-bold outline-none border rounded bg-white focus:ring-2 shadow-sm" :class="item.error ? 'border-red-500 text-red-600 focus:ring-red-500' : 'border-blue-300 text-blue-700 focus:ring-blue-500'">
                                <div class="text-[10px] font-medium mt-1" :class="item.error ? 'text-red-600 font-bold bg-red-100 rounded px-1' : 'text-gray-500'">
                                    <span v-if="item.error">❌ VƯỢT TỒN KHO!</span>
                                    <span v-else>(= {{ item.transferQty }} {{ item.baseUnit }})</span>
                                </div>
                            </div>
                            <div v-else>
                                <span class="font-bold text-blue-700 text-lg block">{{ item.inputQty }}</span>
                                <span class="text-[10px] text-gray-500 font-medium block">(= {{ item.transferQty }} {{ item.baseUnit }})</span>
                            </div>
                        </td>
                        
                        <td v-if="modalMode === 'add'" class="px-4 py-3 text-center border-l border-gray-100"><button type="button" @click="removeItem(idx)" class="text-red-400 hover:text-red-600 transition-colors"><TrashIcon class="w-5 h-5 mx-auto"/></button></td>
                      </tr>
                    </tbody>
                    <tfoot class="bg-gray-50 font-bold border-t border-gray-200">
                      <tr>
                          <td colspan="4" class="px-4 py-3 text-right uppercase text-gray-600">Tổng SL Chuyển (Quy đổi ĐVT Gốc):</td>
                          <td class="px-4 py-3 text-center text-blue-700 text-lg">{{ totalQty }}</td>
                          <td v-if="modalMode === 'add'"></td>
                      </tr>
                    </tfoot>
                  </table>
                </div>
              </div>

              <div><label class="block text-xs font-bold mb-1 text-gray-700">Ghi chú / Lý do điều chuyển</label><textarea v-model="formData.note" :disabled="modalMode === 'view'" rows="2" class="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm focus:ring-1 focus:ring-blue-500 disabled:bg-gray-100 bg-white outline-none" placeholder="Ví dụ: Chuyển hàng cận date về kho tổng..."></textarea></div>

            </form>
          </div>

          <div class="px-6 py-4 border-t border-gray-100 flex flex-col sm:flex-row justify-between gap-3 shrink-0 bg-gray-50">
            <div class="flex gap-2 w-full sm:w-auto">
              <button v-if="modalMode === 'view'" @click="() => alert('Chức năng In Phiếu đang cập nhật...')" class="flex-1 sm:flex-none px-4 py-2 bg-white hover:bg-gray-100 text-gray-700 rounded-lg text-sm font-bold border border-gray-300 shadow-sm flex items-center gap-2 transition-colors"><PrinterIcon class="w-5 h-5"/> In Phiếu Điều Chuyển</button>
            </div>
            <div class="flex gap-2 w-full sm:w-auto justify-end">
              <button type="button" @click="closeModal" class="px-5 py-2.5 border border-gray-300 bg-white rounded-lg text-sm font-semibold text-gray-700 hover:bg-gray-100 transition-colors shadow-sm">{{ modalMode === 'view' ? 'Đóng lại' : 'Hủy bỏ' }}</button>
              <button v-if="modalMode === 'add'" type="submit" form="transferForm" class="px-6 py-2.5 bg-blue-600 text-white rounded-lg text-sm font-bold hover:bg-blue-700 shadow-md transition-colors flex items-center gap-2"><ArrowPathIcon class="w-5 h-5"/> Xác nhận Lập Lệnh</button>
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