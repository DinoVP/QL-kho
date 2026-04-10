<script setup>
import { ref, computed, onMounted } from 'vue'
import { 
  MagnifyingGlassIcon, PlusIcon, EyeIcon, 
  XMarkIcon, ShieldExclamationIcon, TrashIcon,
  DocumentTextIcon, CheckCircleIcon, FireIcon
} from '@heroicons/vue/24/outline'
import { useAuth } from '@/composables/useAuth'

const { currentUserRole } = useAuth()
const currentRole = currentUserRole.value?.toLowerCase() || 'ql_kho'

const DEFECT_API = 'https://localhost:7139/api/Defect'
const STOCK_API = 'https://localhost:7139/api/Stock'
const PROD_API = 'https://localhost:7139/api/Products'
const LOC_API = 'https://localhost:7139/api/Locations'

const getAuthHeaders = () => ({ 
  'Content-Type': 'application/json', 
  'Authorization': 'Bearer ' + (localStorage.getItem('authToken') || '') 
})

const myWarehouseId = ref(parseInt(localStorage.getItem('warehouseId')) || 1) 

const canProcess = computed(() => ['admin', 'giam_doc', 'ql_kho'].includes(currentRole))
const canExport = computed(() => ['admin', 'giam_doc', 'gd_chi_nhanh', 'ql_kho'].includes(currentRole))

const defects = ref([])
const productsList = ref([])
const locationsList = ref([])
const stockList = ref([])
const isLoading = ref(false)

const getToday = () => new Date().toISOString().split('T')[0]

// =========================================================================
// HÀM QUY ĐỔI ĐƠN VỊ & CHUỖI (ĐỒNG BỘ TỪ KIỂM KÊ)
// =========================================================================
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
        units.push({ name: 'Thùng/Kiện', rate: prod.conversionRate || prod.ConversionRate, rel: prod.conversionRate || prod.ConversionRate });
    }
    return units.sort((a,b) => a.rate - b.rate);
}

const autoFormatStockText = (qty, units, baseUnit) => {
    if (qty <= 0) return `0 ${baseUnit}`;
    const sortedPacks = [...(units || [])].sort((a, b) => b.rate - a.rate);
    let remainingQty = qty; let components = [];
    for (const pack of sortedPacks) {
        if (pack.rate > 1 && remainingQty >= pack.rate) {
            components.push({ count: Math.floor(remainingQty / pack.rate), name: pack.name });
            remainingQty %= pack.rate;
        }
    }
    if (remainingQty > 0 || components.length === 0) components.push({ count: remainingQty, name: baseUnit });
    return components.map(c => `${c.count} ${c.name}`).join(' + ');
}

// Hàm chia các ô input về 0 ban đầu
const parseQtyToInputs = (qty, units) => {
    const counts = {};
    units.forEach(u => counts[u.name] = 0);
    if (!qty) return counts;
    let remaining = qty;
    const sortedPacks = [...units].sort((a,b) => b.rate - a.rate);
    for(const pack of sortedPacks) {
        counts[pack.name] = Math.floor(remaining / pack.rate);
        remaining %= pack.rate;
    }
    return counts;
}

const fetchData = async () => {
  isLoading.value = true
  try {
    const headers = getAuthHeaders()
    const [defRes, stockRes, prodRes, locRes] = await Promise.all([
      fetch(DEFECT_API, { headers }), 
      fetch(STOCK_API, { headers }), 
      fetch(PROD_API, { headers }), 
      fetch(LOC_API, { headers })
    ])
    
    if (prodRes.ok) productsList.value = await prodRes.json()
    if (locRes.ok) locationsList.value = await locRes.json()
    
    if (stockRes.ok) {
      const rawStocks = await stockRes.json();
      stockList.value = rawStocks.filter(s => s.warehouseId === myWarehouseId.value).map(s => {
        const prod = productsList.value.find(p => p.id === s.variantId || p.Id === s.variantId) || {};
        const loc = locationsList.value.find(l => l.id === s.locationId || l.Id === s.locationId) || {};
        return {
          stockId: s.id, variantId: s.variantId, qtyAvailable: s.qty || 0, nsx: s.nsx || '', hsd: s.hsd || '',
          sku: prod.sku || prod.Sku || 'N/A', name: prod.name || prod.Name || 'Lỗi', 
          baseUnit: prod.unit || prod.Unit || 'SL', 
          units: getUnitChain(prod),
          locationId: s.locationId, locationCode: loc.code || loc.Code || 'Khu Chờ Nhập'
        }
      })
    }

    if (defRes.ok) {
      const rawDef = await defRes.json()
      defects.value = rawDef.filter(r => r.warehouseId === myWarehouseId.value).map(r => {
        if (!r.code && r.id) r.code = `HL${r.id.toString().padStart(4, '0')}`;
        const firstItem = r.items && r.items.length > 0 ? r.items[0] : null;
        if(firstItem) {
             const prod = productsList.value.find(p => p.id === firstItem.variantId || p.Id === firstItem.variantId) || {};
             r.sku = prod.sku || prod.Sku || 'N/A';
             r.name = prod.name || prod.Name || 'N/A';
             r.qty = r.items.reduce((s, i) => s + (i.qty || 0), 0); 
             r.reason = firstItem.reason || 'Không rõ nguyên nhân';
        }
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

const filteredDefects = computed(() => {
  return defects.value.filter(d => {
    const code = d.code || d.Code || ''; 
    const name = d.name || ''; 
    const sku = d.sku || '';
    const matchSearch = code.toLowerCase().includes(searchQuery.value.toLowerCase()) || 
                        name.toLowerCase().includes(searchQuery.value.toLowerCase()) || 
                        sku.toLowerCase().includes(searchQuery.value.toLowerCase())
    return matchSearch && (filterStatus.value === '' || d.status === filterStatus.value)
  })
})

const showModal = ref(false)
const modalMode = ref('add') 
const formData = ref({ id: 0, code: '', date: getToday(), warehouseId: myWarehouseId.value, items: [], note: '', status: 'pending' })

const openModal = (mode, defect = null) => {
  modalMode.value = mode
  if (defect) {
    const safeItems = (defect.items || defect.Items || []).map(i => {
      const variantId = i.variantId || i.VariantId;
      const prod = productsList.value.find(p => p.id === variantId || p.Id === variantId) || {};
      const loc = locationsList.value.find(l => l.id === i.locationId) || {};
      
      let units = getUnitChain(prod);
      const totalQty = i.qty || i.Qty || 0; 

      return {
        variantId: variantId, sku: prod.sku || prod.Sku, name: prod.name || prod.Name, baseUnit: prod.unit || prod.Unit || 'SL', 
        units, 
        qty: totalQty, 
        maxQty: totalQty, 
        countInputs: parseQtyToInputs(totalQty, units), // Gen lại các ô input
        reason: i.reason || '',
        locationId: i.locationId, locationCode: loc.code || loc.Code || 'Khu Chờ Nhập',
        nsx: i.nsx || i.Nsx ? (i.nsx || i.Nsx).split('T')[0] : '', hsd: i.hsd || i.Hsd ? (i.hsd || i.Hsd).split('T')[0] : ''
      };
    });
    formData.value = { 
      id: defect.id || defect.Id, 
      code: defect.code || defect.Code, 
      date: defect.date || defect.Date, 
      warehouseId: defect.warehouseId || myWarehouseId.value,
      items: safeItems, 
      note: defect.note || defect.Note, 
      status: defect.status || defect.Status 
    };
  } else { 
    formData.value = { id: 0, code: '', date: getToday(), warehouseId: myWarehouseId.value, items: [], note: '', status: 'pending' } 
  }
  selectedVariantIdsToAdd.value = []; 
  stockSearchQuery.value = ''; 
  showModal.value = true
}

const closeModal = () => showModal.value = false

const getStatusBadge = (status) => {
  switch(status) {
    case 'pending': return { text: 'Chờ xử lý', class: 'bg-amber-100 text-amber-700 border-amber-200' }
    case 'completed': return { text: 'Đã xuất hủy (Trừ kho)', class: 'bg-red-100 text-red-700 border-red-200' }
    default: return { text: 'Khác', class: 'bg-gray-100 text-gray-500' }
  }
}

// ==========================================
// LOGIC TÌM HÀNG LỖI TỪ TỒN KHO THỰC TẾ
// ==========================================
const stockSearchQuery = ref('')
const selectedVariantIdsToAdd = ref([]) 

const groupedStocks = computed(() => {
    const map = {};
    stockList.value.forEach(s => {
        if (!map[s.variantId]) {
            map[s.variantId] = { variantId: s.variantId, sku: s.sku, name: s.name, baseUnit: s.baseUnit, units: s.units, totalQty: 0 };
        }
        map[s.variantId].totalQty += s.qtyAvailable;
    });
    return Object.values(map);
})

const filteredGroupedStockList = computed(() => {
  if (!stockSearchQuery.value) return groupedStocks.value;
  return groupedStocks.value.filter(s => 
    s.sku.toLowerCase().includes(stockSearchQuery.value.toLowerCase()) || 
    s.name.toLowerCase().includes(stockSearchQuery.value.toLowerCase())
  );
})

const handleAddMultipleStocks = () => {
  if (selectedVariantIdsToAdd.value.length === 0) return;
  
  selectedVariantIdsToAdd.value.forEach(vid => {
    const physicalStocks = stockList.value.filter(s => s.variantId === vid && s.qtyAvailable > 0);
    
    physicalStocks.forEach(stock => {
        const exists = formData.value.items.find(i => i.variantId === stock.variantId && i.locationId === stock.locationId && i.nsx === stock.nsx && i.hsd === stock.hsd)
        if(!exists) {
            formData.value.items.push({ 
                variantId: stock.variantId, sku: stock.sku, name: stock.name, baseUnit: stock.baseUnit, 
                units: stock.units, 
                qty: 0, 
                countInputs: parseQtyToInputs(0, stock.units), // Mặc định 0
                maxQty: stock.qtyAvailable, reason: 'Móp méo / Rách bao bì', 
                locationId: stock.locationId, locationCode: stock.locationCode, nsx: stock.nsx, hsd: stock.hsd 
            })
        }
    })
  })
  selectedVariantIdsToAdd.value = []; 
  stockSearchQuery.value = '' 
}

const calcActual = (item) => {
    let total = 0;
    item.units.forEach(u => { 
      total += (item.countInputs[u.name] || 0) * u.rate; 
    });
    
    if (total > item.maxQty) {
        alert(`Vị trí này chỉ còn tồn tối đa ${item.maxQty} ${item.baseUnit}! Không thể báo lỗi lố số lượng tồn được.`);
        item.qty = item.maxQty;
        item.countInputs = parseQtyToInputs(item.maxQty, item.units);
    } else {
        item.qty = total;
    }
}

const removeItem = (index) => formData.value.items.splice(index, 1)

const handleSubmit = async () => {
  if (formData.value.items.length === 0) return alert('Chưa chọn mặt hàng hỏng hóc nào!')
  
  const validItems = formData.value.items.filter(i => i.qty > 0);
  if (validItems.length === 0) return alert('Vui lòng điền số lượng hỏng > 0 cho các vị trí bị lỗi!');

  try {
    const method = modalMode.value === 'add' ? 'POST' : 'PUT'; 
    const payload = { 
      ...formData.value, 
      code: modalMode.value === 'add' ? "" : formData.value.code, 
      warehouseId: myWarehouseId.value,
      items: validItems.map(i => ({
        variantId: i.variantId,
        qty: i.qty,
        reason: i.reason,
        locationId: i.locationId,
        nsx: i.nsx, hsd: i.hsd
      }))
    }; 

    const res = await fetch(modalMode.value === 'add' ? DEFECT_API : `${DEFECT_API}/${formData.value.id}`, { 
      method, 
      headers: getAuthHeaders(), 
      body: JSON.stringify(payload) 
    })
    
    if (res.ok) { 
      alert('Lưu báo cáo hàng lỗi thành công!'); 
      await fetchData(); 
      closeModal(); 
    } else { 
      let errMsg = "Lỗi hệ thống!"; 
      try { const t = await res.text(); if(t) errMsg = JSON.parse(t).message || errMsg; } catch(e){} 
      alert('LỖI: ' + errMsg); 
    }
  } catch(e) { 
    console.error(e) 
  }
}

const handleCompleteDefect = async (receipt) => {
  if (!confirm(`XÁC NHẬN TIÊU HỦY / TRỪ KHO SẢN PHẨM LỖI NÀY? (Sẽ trừ vĩnh viễn khỏi Database)`)) return;
  try {
    const res = await fetch(`${DEFECT_API}/${receipt.id || receipt.Id}/complete`, { method: 'PUT', headers: getAuthHeaders() })
    if (res.ok) { 
      alert('Đã tiêu hủy hàng lỗi thành công!'); 
      await fetchData(); 
    } else { 
      let errMsg = "Lỗi hệ thống!"; 
      try { const t = await res.text(); if(t) errMsg = JSON.parse(t).message || errMsg; } catch(e){} 
      alert('LỖI TRỪ KHO: ' + errMsg); 
    }
  } catch(e) { 
    console.error(e) 
  }
}

onMounted(() => fetchData())
</script>

<template>
  <div class="space-y-5 md:space-y-6 animate-fade-in pb-10 px-0 md:px-1 relative">
    <div class="flex flex-col sm:flex-row sm:items-center justify-between gap-4">
      <div>
        <h2 class="text-xl md:text-2xl font-bold text-gray-800">Quản lý Hàng Lỗi / Phế Phẩm</h2>
        <p class="text-[10px] font-bold text-indigo-600 bg-indigo-50 px-2 py-1 rounded inline-block mt-1 border border-indigo-200 uppercase">Vai trò: {{ currentRole }}</p>
      </div>
      <div class="flex flex-wrap items-center gap-2">
        <button v-if="canExport" @click="() => alert('Tính năng đang phát triển...')" class="bg-white border border-emerald-200 text-emerald-700 px-3 py-2.5 rounded-lg text-sm font-semibold hover:bg-emerald-50 shadow-sm flex items-center gap-1.5">
          <DocumentTextIcon class="w-4 h-4"/> Xuất Excel
        </button>
        <button @click="openModal('add')" class="bg-red-600 hover:bg-red-700 text-white px-4 py-2.5 rounded-lg flex items-center gap-2 text-sm font-semibold shadow-sm transition-colors">
          <PlusIcon class="w-5 h-5" /> Báo cáo Hàng lỗi
        </button>
      </div>
    </div>

    <div class="bg-white p-3 md:p-4 rounded-xl border border-gray-200 flex flex-col sm:flex-row items-center gap-3 md:gap-4 shadow-sm">
      <div class="relative w-full sm:flex-1">
        <div class="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none">
          <MagnifyingGlassIcon class="w-5 h-5 text-gray-400" />
        </div>
        <input v-model="searchQuery" type="text" class="block w-full pl-10 pr-3 py-2 border border-gray-200 rounded-lg text-sm outline-none focus:ring-1 focus:ring-red-500" placeholder="Tìm theo mã phiếu, mã SP lỗi...">
      </div>
      <select v-model="filterStatus" class="w-full sm:w-auto border border-gray-200 rounded-lg text-sm px-4 py-2 outline-none focus:ring-1 focus:ring-red-500 cursor-pointer">
        <option value="">Tất cả Trạng thái</option>
        <option value="pending">Chờ xử lý</option>
        <option value="completed">Đã xuất hủy (Trừ kho)</option>
      </select>
    </div>

    <div class="bg-white rounded-xl border border-gray-200 shadow-sm overflow-hidden">
      <div class="w-full overflow-x-auto custom-scrollbar">
        <table class="min-w-[1000px] w-full divide-y divide-gray-200">
          <thead class="bg-gray-50">
            <tr>
              <th class="px-5 py-3.5 text-left text-xs font-bold text-gray-500 uppercase tracking-wider">Mã Phiếu</th>
              <th class="px-5 py-3.5 text-left text-xs font-bold text-gray-500 uppercase tracking-wider">Sản phẩm lỗi (Đại diện)</th>
              <th class="px-5 py-3.5 text-center text-xs font-bold text-gray-500 uppercase tracking-wider">Số lượng Hỏng</th>
              <th class="px-5 py-3.5 text-left text-xs font-bold text-gray-500 uppercase tracking-wider">Nguyên nhân</th>
              <th class="px-5 py-3.5 text-center text-xs font-bold text-gray-500 uppercase tracking-wider">Trạng thái</th>
              <th class="px-5 py-3.5 text-right text-xs font-bold text-gray-500 uppercase tracking-wider">Thao tác</th>
            </tr>
          </thead>
          <tbody class="divide-y divide-gray-100 bg-white">
            <tr v-if="filteredDefects.length === 0">
              <td colspan="6" class="px-6 py-16 text-center">
                <ShieldExclamationIcon class="w-12 h-12 text-gray-300 mx-auto mb-3" />
                <h3 class="text-base font-semibold text-gray-700">Chưa có báo cáo hàng lỗi nào</h3>
              </td>
            </tr>
            <tr v-for="defect in filteredDefects" :key="defect.id" class="hover:bg-gray-50 transition-colors">
              <td class="px-5 py-3 text-sm font-bold text-red-600">{{ defect.code }}</td>
              <td class="px-5 py-3">
                <div class="flex flex-col">
                  <span class="text-sm font-bold text-gray-900">{{ defect.name }}</span>
                  <span class="text-xs text-gray-500">Mã: {{ defect.sku }}</span>
                </div>
              </td>
              <td class="px-5 py-3 text-sm text-center font-bold text-red-600 text-lg">{{ defect.qty }}</td>
              <td class="px-5 py-3 text-sm text-gray-700 max-w-[250px] truncate" :title="defect.reason">{{ defect.reason }}</td>
              <td class="px-5 py-3 text-center">
                <span :class="['text-[10px] font-bold px-2 py-1 rounded border uppercase tracking-wider', getStatusBadge(defect.status).class]">
                  {{ getStatusBadge(defect.status).text }}
                </span>
              </td>
              <td class="px-5 py-3 text-right whitespace-nowrap">
                <div class="flex items-center justify-end gap-1.5">
                  <button @click="openModal('view', defect)" class="px-2 py-1.5 text-blue-600 hover:bg-blue-50 rounded bg-blue-50 border border-blue-100 font-medium text-xs flex items-center gap-1 transition-colors">
                    <EyeIcon class="w-4 h-4" /> Chi tiết
                  </button>
                  
                  <button v-if="defect.status === 'pending' && canProcess" @click="handleCompleteDefect(defect)" class="px-2 py-1.5 text-red-600 hover:bg-red-700 hover:text-white rounded bg-red-50 border border-red-200 font-bold text-xs flex items-center gap-1 transition-colors shadow-sm" title="Xuất hủy để trừ kho">
                    <FireIcon class="w-4 h-4" /> Xuất Hủy
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
        <div class="bg-white rounded-xl shadow-2xl w-full max-w-6xl overflow-hidden flex flex-col max-h-[90vh]">
          
          <div class="px-6 py-4 border-b border-red-100 flex items-center justify-between bg-red-50 shrink-0">
            <h3 class="text-lg font-bold text-red-800 flex items-center gap-2">
              <ShieldExclamationIcon class="w-6 h-6 text-red-600"/> 
              {{ modalMode === 'add' ? 'Báo cáo Hàng Lỗi Mới' : `Chi tiết Lỗi: ${formData.code}` }}
            </h3>
            <button @click="closeModal" class="text-gray-400 hover:text-red-500"><XMarkIcon class="w-6 h-6" /></button>
          </div>
          
          <div class="p-6 overflow-y-auto flex-1 custom-scrollbar bg-slate-50/50">
            <form id="defectForm" @submit.prevent="handleSubmit" class="space-y-6">
              
              <div class="grid grid-cols-1 sm:grid-cols-2 gap-4 bg-white p-4 rounded-lg shadow-sm border border-gray-200">
                <div>
                  <label class="block text-xs font-bold mb-1">Ngày ghi nhận *</label>
                  <input v-model="formData.date" :disabled="modalMode === 'view'" type="date" required class="w-full border rounded-lg px-3 py-2 text-sm focus:ring-red-500 disabled:bg-gray-100 outline-none">
                </div>
                <div>
                  <label class="block text-xs font-bold mb-1">Trạng thái</label>
                  <div class="px-3 py-2 text-sm font-bold rounded border text-center" :class="getStatusBadge(formData.status).class">
                    {{ getStatusBadge(formData.status).text }}
                  </div>
                </div>
              </div>

              <div v-if="modalMode === 'add'" class="bg-red-50 p-4 rounded-xl border border-red-200 shadow-sm">
                <label class="text-sm font-bold text-red-800 mb-2 block uppercase">1. Tìm Hàng Lỗi trong Tồn Kho thực tế:</label>
                <div class="relative">
                  <div class="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none">
                    <MagnifyingGlassIcon class="w-5 h-5 text-red-400" />
                  </div>
                  <input v-model="stockSearchQuery" type="text" class="w-full border border-red-300 rounded-lg pl-10 pr-3 py-2 text-sm mb-2 outline-none focus:ring-1 focus:ring-red-500 shadow-inner bg-white" placeholder="Gõ mã SKU hoặc Tên sản phẩm để tìm hàng...">
                </div>
                
                <div class="bg-white border border-red-200 rounded max-h-40 overflow-y-auto p-2">
                  <div v-if="filteredGroupedStockList.length === 0" class="p-2 text-sm text-gray-400 italic">Không tìm thấy sản phẩm nào trong kho hiện tại.</div>
                  <label v-for="stock in filteredGroupedStockList" :key="stock.variantId" class="flex items-center gap-3 p-2 hover:bg-red-50 cursor-pointer border-b border-gray-100 transition-colors">
                    <input type="checkbox" :value="stock.variantId" v-model="selectedVariantIdsToAdd" class="w-4 h-4 text-red-600 rounded">
                    <span class="text-sm flex-1 font-bold text-gray-800">[{{ stock.sku }}] {{ stock.name }}</span>
                    <span class="text-xs font-bold px-2 py-1 bg-amber-100 text-amber-800 border border-amber-200 rounded ml-2">Tổng tồn: {{ stock.totalQty }} {{ stock.baseUnit }}</span>
                  </label>
                </div>
                <button type="button" @click="handleAddMultipleStocks" class="mt-3 px-4 py-2 bg-red-600 text-white rounded-lg text-sm font-bold disabled:opacity-50 hover:bg-red-700 shadow-sm transition-colors w-full sm:w-auto" :disabled="selectedVariantIdsToAdd.length === 0">
                  2. Bung chi tiết các Vị trí chứa hàng
                </button>
              </div>

              <div>
                <h4 class="text-sm font-bold mb-2 text-gray-800">3. Khai báo Tình trạng Lỗi (Chỉ nhập số lượng hỏng > 0)</h4>
                <div class="border border-gray-200 rounded-lg overflow-x-auto shadow-sm bg-white">
                  <table class="w-full text-sm text-left">
                    <thead class="bg-gray-100 text-xs uppercase font-bold text-gray-600 border-b border-gray-200">
                      <tr>
                        <th class="px-4 py-3 min-w-[150px]">Mã SKU / Tên Hàng</th>
                        <th class="px-4 py-3 text-indigo-700 border-l border-gray-200 w-32">Vị Trí Kệ</th>
                        <th class="px-4 py-3 text-right border-l border-gray-200 w-48 text-gray-700">Tồn Lô Tham Khảo</th>
                        <th class="px-4 py-3 text-center w-[300px] text-red-600 border-l border-gray-200">Khai Báo SL Hỏng</th>
                        <th class="px-4 py-3 w-48 border-l border-gray-200">Mô tả Lỗi</th>
                        <th v-if="modalMode === 'add'" class="px-4 py-3 text-center w-10 border-l border-gray-200">#</th>
                      </tr>
                    </thead>
                    <tbody class="divide-y divide-gray-100">
                      <tr v-if="formData.items.length === 0">
                        <td :colspan="modalMode === 'add' ? 6 : 5" class="px-4 py-8 text-center text-gray-400 italic">Chưa có mặt hàng lỗi.</td>
                      </tr>
                      
                      <tr v-for="(item, idx) in formData.items" :key="idx" class="hover:bg-red-50/50 transition-colors">
                        <td class="px-4 py-2">
                          <span class="font-bold text-gray-800 block">{{ item.sku }}</span>
                          <span class="font-normal text-xs text-gray-500">{{ item.name }}</span>
                        </td>
                        <td class="px-4 py-2 border-l border-gray-100">
                          <span class="text-indigo-700 font-bold text-xs block">{{ item.locationCode }}</span>
                          <span class="font-normal text-gray-500 text-[10px]">NSX: {{item.nsx||'--'}}</span>
                        </td>
                        
                        <td class="px-4 py-2 text-right text-gray-600 border-l border-gray-100 bg-gray-50">
                            <span class="block text-xs font-bold text-gray-700">{{ autoFormatStockText(item.maxQty, item.units, item.baseUnit) }}</span>
                            <span class="block text-[10px] text-gray-500 font-medium">(= {{ item.maxQty }} {{ item.baseUnit }})</span>
                        </td>
                        
                        <td class="px-4 py-2 bg-red-50/30 border-l border-gray-100">
                          <div v-if="modalMode !== 'view'" class="flex flex-col gap-2 items-center justify-center py-1">
                              <div class="flex flex-wrap items-center justify-center gap-2">
                                  <div v-for="u in [...item.units].reverse()" :key="u.name" class="flex items-center border border-red-200 rounded overflow-hidden shadow-sm bg-white focus-within:ring-1 focus-within:ring-red-500 w-[95px]">
                                      <span class="w-12 text-center bg-red-50 text-red-800 text-[10px] font-bold py-1 border-r border-red-200 px-1 truncate" :title="u.name">{{ u.name }}</span>
                                      <input v-model.number="item.countInputs[u.name]" @input="calcActual(item)" type="number" min="0" class="flex-1 w-full text-center py-1 text-sm font-bold text-red-700 outline-none">
                                  </div>
                              </div>
                              <div class="text-[11px] text-center font-bold text-red-600 border-t border-red-200 pt-1 w-full">
                                Tổng báo hỏng: {{ item.qty }} {{ item.baseUnit }}
                              </div>
                          </div>
                          <div v-else class="text-center font-bold text-red-600">
                             <span class="block text-xs">{{ autoFormatStockText(item.qty, item.units, item.baseUnit) }}</span>
                             <span class="block text-[10px] text-gray-500 font-medium">(= {{ item.qty }} {{ item.baseUnit }})</span>
                          </div>
                        </td>

                        <td class="px-4 py-2 border-l border-gray-100 align-top">
                          <input v-if="modalMode === 'add'" v-model="item.reason" type="text" class="w-full border border-gray-300 rounded px-2 py-1.5 text-xs outline-none focus:ring-1 focus:ring-red-500 bg-white" placeholder="VD: Rách bao bì, móp méo...">
                          <span v-else class="text-xs text-gray-700">{{ item.reason }}</span>
                        </td>
                        <td v-if="modalMode === 'add'" class="px-4 py-2 text-center border-l border-gray-100 align-middle">
                          <button type="button" @click="removeItem(idx)" class="text-red-400 hover:text-red-600 transition-colors">
                            <TrashIcon class="w-5 h-5 mx-auto"/>
                          </button>
                        </td>
                      </tr>
                    </tbody>
                  </table>
                </div>
              </div>
              
              <div>
                <label class="block text-xs font-bold mb-1 text-gray-700">Ghi chú chung</label>
                <textarea v-model="formData.note" :disabled="modalMode === 'view'" rows="2" class="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm focus:ring-1 focus:ring-red-500 disabled:bg-gray-100 outline-none" placeholder="Thông tin bổ sung..."></textarea>
              </div>

            </form>
          </div>

          <div class="px-6 py-4 border-t border-gray-100 flex justify-end gap-3 shrink-0 bg-gray-50">
            <button type="button" @click="closeModal" class="px-5 py-2.5 border border-gray-300 rounded-lg text-sm font-semibold hover:bg-gray-100 bg-white transition-colors">
              {{ modalMode === 'view' ? 'Đóng lại' : 'Hủy bỏ' }}
            </button>
            <button v-if="modalMode === 'add'" type="submit" form="defectForm" class="px-5 py-2.5 bg-red-600 text-white rounded-lg text-sm font-bold hover:bg-red-700 shadow-md transition-colors flex items-center gap-2">
              <ShieldExclamationIcon class="w-5 h-5 inline"/> Gửi Báo Cáo Lỗi
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
.custom-scrollbar:hover::-webkit-scrollbar-thumb { background: #94a3b8; }
.animate-fade-in { animation: fadeIn 0.2s ease-out; }
@keyframes fadeIn { from { opacity: 0; transform: translateY(5px); } to { opacity: 1; transform: translateY(0); } }
</style>