<script setup>
import { ref, computed, onMounted } from 'vue'
import { useAuth } from '@/composables/useAuth' 
import { 
  MagnifyingGlassIcon, PlusIcon, EyeIcon, 
  XMarkIcon, DocumentDuplicateIcon, TrashIcon, PencilSquareIcon,
  CheckCircleIcon
} from '@heroicons/vue/24/outline'

const { currentUserRole } = useAuth()
const currentRole = currentUserRole.value?.toLowerCase() || 'ql_kho'

const ORDER_API = 'https://localhost:7139/api/Po'
const PROD_API = 'https://localhost:7139/api/Products'

const getAuthHeaders = () => ({ 
    'Content-Type': 'application/json', 
    'Authorization': 'Bearer ' + (localStorage.getItem('authToken') || '') 
})

const canCreatePR = computed(() => ['admin', 'ql_kho'].includes(currentRole))
const canApprovePR = computed(() => ['admin', 'giam_doc', 'gd_chi_nhanh'].includes(currentRole))

const prList = ref([])
const productsList = ref([])
const isLoading = ref(false)

const getToday = () => new Date().toISOString().split('T')[0]

// BỘ QUY ĐỔI ĐƠN VỊ
const getUnitChain = (prod) => {
    let units = [{ name: prod.unit || prod.Unit || 'SL', rate: 1 }];
    const conversions = prod.conversions || prod.Conversions || [];
    if (conversions.length > 0) {
        [...conversions].sort((a,b) => a.rate - b.rate).forEach(c => {
            units.push({ name: c.altUnit, rate: c.rate });
        });
    } else if ((prod.conversionRate || prod.ConversionRate) > 1) {
        units.push({ name: 'Thùng/Kiện', rate: prod.conversionRate || prod.ConversionRate });
    }
    return units.sort((a,b) => a.rate - b.rate);
}

const autoFormatStockText = (qty, units, baseUnit) => {
    if (qty <= 0) return `0 ${baseUnit}`;
    const sortedPacks = [...(units || [])].sort((a, b) => b.rate - a.rate);
    let remaining = qty; 
    let components = [];
    for (const pack of sortedPacks) {
        if (pack.rate > 1 && remaining >= pack.rate) {
            components.push({ count: Math.floor(remaining / pack.rate), name: pack.name });
            remaining %= pack.rate;
        }
    }
    if (remaining > 0 || components.length === 0) {
        components.push({ count: remaining, name: baseUnit });
    }
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

// GỌI DỮ LIỆU
const fetchData = async () => {
    isLoading.value = true
    try {
        const headers = getAuthHeaders()
        const [orderRes, prodRes] = await Promise.all([
            fetch(ORDER_API, { headers }), 
            fetch(PROD_API, { headers })
        ])
        
        if (prodRes.ok) {
            productsList.value = await prodRes.json()
        }
        
        if (orderRes.ok) {
            const rawOrders = await orderRes.json()
            
            prList.value = rawOrders.map(o => {
                let type = o.type || o.Type;
                let code = o.code || o.Code || `${type}${o.id.toString().padStart(4, '0')}`;
                
                let displayStatus = o.status || o.Status;
                if (type === 'PO') displayStatus = 'completed'; 

                if (o.items) {
                    o.items = o.items.map(i => {
                        const prod = productsList.value.find(p => p.id === (i.variantId || i.VariantId)) || {};
                        return {
                            ...i,
                            name: prod.name || prod.Name || 'Sản phẩm ẩn',
                            unit: prod.unit || prod.Unit || 'SL',
                            units: getUnitChain(prod)
                        };
                    });
                }
                return { ...o, code, type, displayStatus };
            }).sort((a,b) => b.id - a.id)
        }
    } catch (error) { 
        console.error(error) 
    } finally { 
        isLoading.value = false 
    }
}

const searchQuery = ref('')
const filterStatus = ref('')

const filteredPRs = computed(() => {
    return prList.value.filter(o => 
        (o.code || '').toLowerCase().includes(searchQuery.value.toLowerCase()) && 
        (filterStatus.value === '' || o.displayStatus === filterStatus.value)
    )
})

const showModal = ref(false)
const modalMode = ref('add') 
const formData = ref({ id: 0, code: '', type: 'PR', date: getToday(), items: [], note: '', status: 'pending', displayStatus: 'pending' })

const openModal = (mode, order = null) => {
    modalMode.value = mode
    if (order) {
        const mappedItems = (order.items || []).map(i => {
            const prod = productsList.value.find(p => p.id === (i.variantId || i.VariantId)) || {};
            const units = getUnitChain(prod);
            const totalQty = i.qty || i.Qty || 1;
            
            let bestUnit = units[0]; 
            let bestInputQty = totalQty;
            for (let u = units.length - 1; u >= 0; u--) {
                if (totalQty > 0 && totalQty % units[u].rate === 0) {
                    bestUnit = units[u]; 
                    bestInputQty = totalQty / units[u].rate; 
                    break;
                }
            }
            
            return {
                variantId: i.variantId || i.VariantId, 
                sku: prod.sku || prod.Sku || 'N/A', 
                name: prod.name || prod.Name || 'Sản phẩm', 
                unit: prod.unit || prod.Unit || 'SL', 
                units: units, 
                selectedUnit: bestUnit.name, 
                inputQty: bestInputQty, 
                qty: totalQty
            }
        })
        formData.value = { ...order, type: 'PR', items: mappedItems }
    } else { 
        formData.value = { id: 0, code: '', type: 'PR', date: getToday(), items: [], note: '', status: 'pending', displayStatus: 'pending' } 
    }
    productSearchQuery.value = ''
    showModal.value = true
}

const closeModal = () => showModal.value = false

const getStatusBadge = (status) => {
    switch(status) {
        case 'pending': return { text: 'Chờ Sếp Duyệt', class: 'bg-amber-100 text-amber-700 border-amber-200' }
        case 'approved': return { text: 'Chờ Thu Mua Báo Giá', class: 'bg-indigo-100 text-indigo-700 border-indigo-200' }
        case 'completed': return { text: 'Đã Lên Đơn (PO)', class: 'bg-emerald-100 text-emerald-700 border-emerald-200' }
        default: return { text: 'Từ chối / Hủy', class: 'bg-red-100 text-red-700 border-red-200' }
    }
}

const productSearchQuery = ref('')

const filteredProducts = computed(() => {
    if (!productSearchQuery.value) return [];
    return productsList.value.filter(p => 
        (p.sku || '').toLowerCase().includes(productSearchQuery.value.toLowerCase()) || 
        (p.name || '').toLowerCase().includes(productSearchQuery.value.toLowerCase())
    );
})

const handleAddItem = (prod) => {
    const existing = formData.value.items.find(i => i.variantId === prod.id)
    if (!existing) {
        const units = getUnitChain(prod);
        const defaultUnit = units.length > 1 ? units[units.length - 1] : units[0];
        
        formData.value.items.unshift({ 
            variantId: prod.id, sku: prod.sku, name: prod.name, 
            unit: prod.unit || 'SL', units: units, 
            selectedUnit: defaultUnit.name, inputQty: 1, qty: defaultUnit.rate
        })
    } else { 
        existing.inputQty += 1; calcActual(existing);
    }
    productSearchQuery.value = '' 
}

const calcActual = (item) => {
    const unitDef = item.units.find(u => u.name === item.selectedUnit);
    item.qty = (item.inputQty || 0) * (unitDef ? unitDef.rate : 1);
}

const removeItem = (index) => {
    formData.value.items.splice(index, 1)
}

// ĐÃ FIX 400 BAD REQUEST: XỬ LÝ LỌC DỮ LIỆU SẠCH TRƯỚC KHI GỬI C#
const handleSubmit = async () => {
    if (formData.value.items.length === 0) return alert('Chưa chọn hàng hóa nào để xin mua!')
    try {
        // TẠO DỮ LIỆU SẠCH: Loại bỏ hết rác frontend như 'units', 'inputQty', 'displayStatus'
        const cleanPayload = { 
            id: modalMode.value === 'add' ? 0 : formData.value.id,
            code: modalMode.value === 'add' ? "" : formData.value.code, 
            type: 'PR',
            date: formData.value.date,
            note: formData.value.note || "",
            status: 'pending',
            totalAmount: 0, // PR chưa có giá
            partnerId: null, // PR chưa có NCC
            items: formData.value.items.map(i => ({
                variantId: i.variantId || i.VariantId,
                qty: i.qty,
                price: 0
            }))
        };
        
        const method = modalMode.value === 'add' ? 'POST' : 'PUT';
        const url = modalMode.value === 'add' ? ORDER_API : `${ORDER_API}/${formData.value.id}`;
        
        const res = await fetch(url, { 
            method, headers: getAuthHeaders(), body: JSON.stringify(cleanPayload) 
        })
        
        if (res.ok) { 
            alert('Lập phiếu xin mua (PR) thành công!'); await fetchData(); closeModal(); 
        } else { 
            // IN CHI TIẾT LỖI TỪ BACKEND ĐỂ DỄ DÀNG SỬA NẾU CÓ
            const errText = await res.text();
            alert('LỖI TỪ HỆ THỐNG C#:\n' + errText); 
            console.log("Payload đã gửi:", cleanPayload);
        }
    } catch(e) { console.error(e) }
}

const changeStatus = async (id, newStatus, confirmMsg) => {
    if(!confirm(confirmMsg)) return;
    try {
        const res = await fetch(`${ORDER_API}/${id}/status`, { 
            method: 'PUT', headers: getAuthHeaders(), body: JSON.stringify(newStatus) 
        })
        if(res.ok) { alert('Đã cập nhật!'); await fetchData(); } else { alert('Lỗi hệ thống!'); }
    } catch(e) { console.error(e) }
}

onMounted(() => fetchData())
</script>

<template>
  <div class="space-y-5 md:space-y-6 animate-fade-in pb-10 px-0 md:px-1 relative">
    
    <div class="flex flex-col sm:flex-row sm:items-center justify-between gap-4">
      <div>
        <h2 class="text-xl md:text-2xl font-bold text-gray-800">Yêu Cầu Mua Hàng (PR)</h2>
        <p class="text-[10px] font-bold text-indigo-600 bg-indigo-50 px-2 py-1 rounded inline-block mt-1 border border-indigo-200 uppercase">
            Vai trò: {{ currentRole }}
        </p>
      </div>
      <button v-if="canCreatePR" @click="openModal('add')" class="bg-indigo-600 hover:bg-indigo-700 text-white px-4 py-2.5 rounded-lg flex items-center gap-2 text-sm font-semibold shadow-sm transition-colors">
        <PlusIcon class="w-5 h-5" /> Xin mua hàng (Lập PR)
      </button>
    </div>

    <div class="bg-white p-3 md:p-4 rounded-xl border border-gray-200 flex flex-col sm:flex-row items-center gap-3 md:gap-4 shadow-sm">
      <div class="relative w-full sm:flex-1">
          <div class="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none">
              <MagnifyingGlassIcon class="w-5 h-5 text-gray-400" />
          </div>
          <input v-model="searchQuery" type="text" class="block w-full pl-10 pr-3 py-2 border border-gray-200 rounded-lg text-sm outline-none focus:ring-1 focus:ring-indigo-500" placeholder="Tìm mã phiếu...">
      </div>
      <select v-model="filterStatus" class="w-full sm:w-auto border border-gray-200 rounded-lg text-sm px-4 py-2 outline-none focus:ring-1 focus:ring-indigo-500 cursor-pointer">
          <option value="">Tất cả Trạng thái</option>
          <option value="pending">Chờ Sếp Duyệt</option>
          <option value="approved">Chờ Thu Mua Báo Giá</option>
          <option value="completed">Đã Lên Đơn (PO)</option>
      </select>
    </div>

    <div class="bg-white rounded-xl border border-gray-200 shadow-sm overflow-hidden">
      <div class="w-full overflow-x-auto custom-scrollbar">
        <table class="min-w-[1000px] w-full divide-y divide-gray-200">
          <thead class="bg-gray-50">
            <tr>
                <th class="px-5 py-3.5 text-left text-xs font-bold text-gray-500 uppercase">Mã Phiếu</th>
                <th class="px-5 py-3.5 text-left text-xs font-bold text-gray-500 uppercase">Ngày lập</th>
                <th class="px-5 py-3.5 text-left text-xs font-bold text-gray-500 uppercase min-w-[200px]">Nội dung yêu cầu</th>
                <th class="px-5 py-3.5 text-left text-xs font-bold text-gray-500 uppercase min-w-[150px]">Số lượng</th>
                <th class="px-5 py-3.5 text-left text-xs font-bold text-gray-500 uppercase w-64">Ghi chú</th>
                <th class="px-5 py-3.5 text-center text-xs font-bold text-gray-500 uppercase">Trạng thái</th>
                <th class="px-5 py-3.5 text-right text-xs font-bold text-gray-500 uppercase">Thao tác</th>
            </tr>
          </thead>
          <tbody class="divide-y divide-gray-100 bg-white">
            <tr v-if="filteredPRs.length === 0">
                <td colspan="7" class="px-6 py-16 text-center">
                    <DocumentDuplicateIcon class="w-12 h-12 text-gray-300 mx-auto mb-3" />
                    <h3 class="text-base font-semibold text-gray-700">Chưa có yêu cầu xin mua hàng nào</h3>
                </td>
            </tr>
            <tr v-for="order in filteredPRs" :key="order.id" class="hover:bg-gray-50 transition-colors">
              <td class="px-5 py-3 text-sm font-bold text-indigo-700">
                  <span v-if="order.type === 'PO'" class="text-[9px] bg-emerald-100 text-emerald-800 px-1 py-0.5 rounded border border-emerald-300 mr-1 shadow-sm">ĐÃ LÊN PO</span>
                  {{ order.code }}
              </td>
              <td class="px-5 py-3 text-sm font-medium text-gray-600">{{ order.date }}</td>
              
              <td class="px-5 py-3">
                  <span class="text-sm font-bold text-gray-800 line-clamp-2" :title="getItemNames(order.items)">{{ getItemNames(order.items) }}</span>
                  <span class="text-[10px] text-gray-500 font-medium block mt-0.5">(Gồm {{ order.items?.length || 0 }} mặt hàng)</span>
              </td>
              <td class="px-5 py-3">
                  <span class="text-sm font-bold text-indigo-600 line-clamp-2" :title="getItemQuantities(order.items)">{{ getItemQuantities(order.items) }}</span>
              </td>
              
              <td class="px-5 py-3 text-sm text-gray-600 truncate max-w-xs">{{ order.note || '---' }}</td>
              
              <td class="px-5 py-3 text-center">
                  <span :class="['text-[10px] font-bold px-2 py-1 rounded border uppercase tracking-wider', getStatusBadge(order.displayStatus).class]">
                      {{ getStatusBadge(order.displayStatus).text }}
                  </span>
              </td>
              
              <td class="px-5 py-3 text-right space-x-1.5 whitespace-nowrap">
                <button @click="openModal('view', order)" class="p-1.5 text-gray-600 hover:bg-gray-100 rounded border border-transparent" title="Xem chi tiết"><EyeIcon class="w-4 h-4" /></button>
                <button v-if="order.displayStatus === 'pending' && canCreatePR" @click="openModal('edit', order)" class="p-1.5 text-indigo-600 hover:bg-indigo-50 rounded" title="Sửa"><PencilSquareIcon class="w-4 h-4" /></button>
                <button v-if="order.displayStatus === 'pending' && canApprovePR" @click="changeStatus(order.id, 'approved', 'Sếp đồng ý duyệt cho phép Thu mua đi tìm nguồn hàng này?')" class="p-1.5 text-emerald-600 hover:bg-emerald-50 rounded border border-transparent" title="Duyệt PR"><CheckCircleIcon class="w-5 h-5" /></button>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>

    <Teleport to="body">
      <div v-if="showModal" class="fixed inset-0 z-50 flex items-center justify-center px-4 bg-slate-900/50 backdrop-blur-sm animate-fade-in">
        <div class="bg-white rounded-xl shadow-2xl w-full max-w-5xl overflow-hidden flex flex-col max-h-[90vh]">
          
          <div class="px-6 py-4 border-b border-indigo-100 flex items-center justify-between shrink-0 bg-indigo-50">
            <h3 class="text-lg font-bold flex items-center gap-2 text-indigo-800">
                <DocumentDuplicateIcon class="w-6 h-6 text-indigo-600"/> 
                {{ modalMode === 'add' ? 'Lập Phiếu Xin Mua Hàng (PR)' : `Chi tiết Yêu cầu: ${formData.code}` }}
            </h3>
            <div class="flex items-center gap-4">
                <div class="text-sm font-bold flex items-center bg-white px-3 py-1 rounded-lg border border-indigo-200">
                    Trạng thái: 
                    <span :class="['ml-2 px-2 py-0.5 rounded text-[10px] uppercase border', getStatusBadge(formData.displayStatus).class]">
                        {{ getStatusBadge(formData.displayStatus).text }}
                    </span>
                </div>
                <button @click="closeModal" class="text-gray-400 hover:text-red-500"><XMarkIcon class="w-6 h-6" /></button>
            </div>
          </div>
          
          <div class="p-6 overflow-y-auto flex-1 custom-scrollbar bg-slate-50/50">
            <form id="prForm" @submit.prevent="handleSubmit" class="space-y-6">
              
              <div class="bg-white p-4 rounded-xl border border-gray-200 shadow-sm">
                <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
                  <div>
                      <label class="block text-xs font-bold mb-1 text-gray-700">Mã Phiếu</label>
                      <input type="text" disabled class="w-full border rounded-lg px-3 py-2 text-sm bg-gray-100 italic font-bold text-gray-500" :placeholder="modalMode === 'add' ? 'Hệ thống tự động sinh' : formData.code">
                  </div>
                  <div>
                      <label class="block text-xs font-bold mb-1 text-gray-700">Ngày lập yêu cầu *</label>
                      <input v-model="formData.date" :disabled="modalMode === 'view'" type="date" required class="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm outline-none focus:ring-1 focus:ring-indigo-500">
                  </div>
                </div>
              </div>

              <div>
                <div v-if="modalMode !== 'view'" class="mb-4 bg-indigo-50 p-4 rounded-xl border border-indigo-100 relative shadow-sm">
                  <label class="text-xs font-bold text-indigo-800 mb-2 block uppercase tracking-wide">Tra cứu sản phẩm cần xin mua:</label>
                  <div class="relative">
                      <div class="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none">
                          <MagnifyingGlassIcon class="w-5 h-5 text-indigo-500" />
                      </div>
                      <input v-model="productSearchQuery" type="text" class="w-full border border-indigo-300 rounded-lg pl-10 pr-3 py-2.5 text-sm focus:ring-2 focus:ring-indigo-500 outline-none shadow-inner bg-white" placeholder="Gõ mã SKU hoặc Tên sản phẩm...">
                  </div>
                  
                  <div v-if="filteredProducts.length > 0" class="absolute z-10 mt-1 w-[calc(100%-2rem)] bg-white border border-indigo-200 rounded-lg shadow-xl max-h-60 overflow-y-auto">
                      <div v-for="prod in filteredProducts" :key="prod.id" @click="handleAddItem(prod)" class="p-3 hover:bg-indigo-50 cursor-pointer border-b border-gray-100 flex items-center justify-between transition-colors">
                          <div class="flex flex-col">
                              <span class="font-bold text-indigo-800 text-sm">{{prod.sku}}</span>
                              <span class="text-gray-700 text-xs">{{prod.name}}</span>
                          </div>
                          <div class="text-[10px] text-gray-500 font-bold bg-gray-100 px-2 py-1 rounded border">ĐVT Gốc: {{prod.unit}}</div>
                      </div>
                  </div>
                </div>

                <div class="border border-gray-200 rounded-xl overflow-x-auto shadow-sm bg-white">
                  <table class="w-full text-sm text-left">
                    <thead class="bg-gray-100 text-xs uppercase font-bold text-gray-600 border-b border-gray-200">
                      <tr>
                          <th class="px-4 py-3 w-32">Mã SKU</th>
                          <th class="px-4 py-3 min-w-[200px]">Tên Hàng Hóa</th>
                          <th class="px-4 py-3 text-center border-l border-gray-200 w-32">Đơn Vị Tính</th>
                          <th class="px-4 py-3 text-center bg-indigo-50 border-l border-gray-200 w-40 text-indigo-800">Số Lượng</th>
                          <th v-if="modalMode !== 'view'" class="px-4 py-3 text-center w-12 border-l border-gray-200">#</th>
                      </tr>
                    </thead>
                    <tbody class="divide-y divide-gray-100">
                      <tr v-if="formData.items.length === 0">
                          <td :colspan="modalMode !== 'view' ? 5 : 4" class="px-4 py-12 text-center text-gray-400 italic">Vui lòng tra cứu và thêm sản phẩm.</td>
                      </tr>
                      
                      <tr v-for="(item, idx) in formData.items" :key="idx" class="hover:bg-indigo-50/30 transition-colors">
                        <td class="px-4 py-3 font-bold text-gray-800">{{ item.sku }}</td>
                        <td class="px-4 py-3 text-gray-700">
                          <span class="block font-medium">{{ item.name }}</span>
                          <span class="text-[10px] text-gray-400 font-bold bg-gray-50 px-1 border rounded inline-block mt-0.5">ĐVT Gốc: {{item.unit}}</span>
                        </td>
                        
                        <td class="px-4 py-3 text-center border-l border-gray-100">
                            <select v-if="modalMode !== 'view'" v-model="item.selectedUnit" @change="calcActual(item)" class="w-full py-1.5 px-2 bg-gray-50 border border-gray-300 rounded text-xs font-bold outline-none cursor-pointer focus:ring-1 focus:ring-indigo-500">
                                <option v-for="u in item.units" :key="u.name" :value="u.name">{{ u.name }}</option>
                            </select>
                            <span v-else class="font-bold text-gray-700">{{ item.selectedUnit }}</span>
                        </td>

                        <td class="px-4 py-3 bg-indigo-50/20 border-l border-gray-100 text-center">
                            <div v-if="modalMode !== 'view'">
                                <input v-model.number="item.inputQty" @input="calcActual(item)" type="number" min="1" class="w-20 text-center py-1.5 font-bold text-indigo-700 outline-none border border-indigo-300 rounded bg-white focus:ring-2 focus:ring-indigo-500">
                                <div class="text-[10px] text-gray-500 font-medium mt-1">(= {{ item.qty }} {{ item.unit }})</div>
                            </div>
                            <div v-else>
                                <span class="font-bold text-indigo-700 text-lg">{{ item.inputQty }}</span>
                                <span class="block text-[10px] text-gray-500 font-medium">(= {{ item.qty }} {{ item.unit }})</span>
                            </div>
                        </td>
                        
                        <td v-if="modalMode !== 'view'" class="px-4 py-3 text-center border-l border-gray-100">
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
                  <label class="block text-xs font-bold mb-1 text-gray-700">Lý do xin mua / Ghi chú cho Sếp</label>
                  <textarea v-model="formData.note" :disabled="modalMode === 'view'" rows="2" class="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm focus:ring-1 focus:ring-indigo-500 bg-white outline-none" placeholder="Ví dụ: Kho sắp hết hàng, cần nhập gấp..."></textarea>
              </div>

            </form>
          </div>

          <div class="px-6 py-4 border-t border-gray-100 flex justify-end gap-3 bg-gray-50 shrink-0">
            <button type="button" @click="closeModal" class="px-5 py-2.5 border border-gray-300 text-gray-700 rounded-lg text-sm font-semibold hover:bg-gray-100 bg-white transition-colors">
                {{ modalMode === 'view' ? 'Đóng lại' : 'Hủy bỏ' }}
            </button>
            <button v-if="modalMode !== 'view'" type="submit" form="prForm" class="px-6 py-2.5 text-white rounded-lg text-sm font-bold shadow-md transition-colors bg-indigo-600 hover:bg-indigo-700 flex items-center gap-2">
                <DocumentDuplicateIcon class="w-5 h-5"/> Gửi Yêu Cầu Xin Mua
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