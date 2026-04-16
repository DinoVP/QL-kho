<script setup>
import { ref, computed, onMounted } from 'vue'
import { useAuth } from '@/composables/useAuth' 
import { 
  MagnifyingGlassIcon, PlusIcon, EyeIcon, 
  XMarkIcon, PencilSquareIcon, CheckCircleIcon, ArrowRightCircleIcon, 
  BuildingStorefrontIcon, ShoppingCartIcon
} from '@heroicons/vue/24/outline'

const { currentUserRole } = useAuth()
const currentRole = currentUserRole.value?.toLowerCase() || 'nv_thu_mua'

// API CHUẨN ĐÃ ĐƯỢC FIX
const ORDER_API = 'https://localhost:7139/api/Po'
const PROD_API = 'https://localhost:7139/api/Products'
const PARTNER_API = 'https://localhost:7139/api/CrmPartners'

const getAuthHeaders = () => ({ 
    'Content-Type': 'application/json', 
    'Authorization': 'Bearer ' + (localStorage.getItem('authToken') || '') 
})

const canCreatePO = computed(() => ['admin', 'nv_thu_mua'].includes(currentRole))
const canApprovePO = computed(() => ['admin', 'giam_doc', 'gd_chi_nhanh'].includes(currentRole))

const orders = ref([])
const productsList = ref([])
const suppliersList = ref([])
const isLoading = ref(false)

const getToday = () => new Date().toISOString().split('T')[0]
const formatCurrency = (val) => new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(val || 0)

// =========================================================================
// BỘ QUY ĐỔI ĐƠN VỊ ĐA CẤP 
// =========================================================================
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

// HÀM XỬ LÝ 2 CỘT DANH SÁCH BÊN NGOÀI
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
        const [orderRes, prodRes, partnerRes] = await Promise.all([
            fetch(ORDER_API, { headers }), fetch(PROD_API, { headers }), fetch(PARTNER_API, { headers })
        ])
        
        if (prodRes.ok) productsList.value = await prodRes.json()
        if (partnerRes.ok) {
            const pData = await partnerRes.json();
            suppliersList.value = (pData.data || pData.Data || pData || []).filter(p => p.isSupplier || p.partnerCode?.startsWith('NC'))
        }
        
        if (orderRes.ok) {
            const rawOrders = await orderRes.json()
            orders.value = rawOrders.filter(o => (o.type === 'PR' && o.status === 'approved') || o.type === 'PO' || o.Type === 'PO').map(o => {
                if (!o.code && o.id) o.code = `${o.type === 'PO' ? 'PO' : 'PR'}${o.id.toString().padStart(4, '0')}`;
                
                if (o.type === 'PO' && o.partnerId) {
                    const sup = suppliersList.value.find(s => s.partnerId === o.partnerId || s.id === o.partnerId);
                    o.supplierName = sup ? (sup.partnerName || sup.name) : 'NCC Ẩn';
                }
                
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
                return o;
            }).sort((a,b) => b.id - a.id)
        }
    } catch (error) { console.error(error) } finally { isLoading.value = false }
}

const searchQuery = ref('')
const filterType = ref('')

const filteredOrders = computed(() => {
    return orders.value.filter(o => 
        ((o.code || '').toLowerCase().includes(searchQuery.value.toLowerCase()) || 
        (o.supplierName || '').toLowerCase().includes(searchQuery.value.toLowerCase())) && 
        (filterType.value === '' || o.type === filterType.value)
    )
})

const showModal = ref(false)
const modalMode = ref('convert_po') 
const formData = ref({ id: 0, code: '', type: 'PO', date: getToday(), partnerId: '', items: [], note: '', status: 'pending' })

const openModal = (mode, order = null) => {
    modalMode.value = mode
    if (order) {
        const mappedItems = (order.items || []).map(i => {
            const prod = productsList.value.find(p => p.id === (i.variantId || i.VariantId)) || {};
            const units = getUnitChain(prod);
            const totalQty = i.qty || i.Qty || 1;
            
            let bestUnit = units[0]; let bestInputQty = totalQty;
            for (let u = units.length - 1; u >= 0; u--) {
                if (totalQty > 0 && totalQty % units[u].rate === 0) {
                    bestUnit = units[u]; bestInputQty = totalQty / units[u].rate; break;
                }
            }

            let suggestedPrice = i.price || i.Price || 0;
            if (suggestedPrice === 0 && mode === 'convert_po') {
                // ĐÃ FIX VÀ BỔ SUNG: Thêm fallback dự phòng lấy "price" cũ nếu data chưa kịp nâng cấp lên "importPrice"
                suggestedPrice = prod.importPrice || prod.ImportPrice || prod.price || prod.Price || 0;
            }

            return {
                variantId: i.variantId || i.VariantId, sku: prod.sku || prod.Sku, name: prod.name || prod.Name, 
                unit: prod.unit || prod.Unit || 'SL', units: units, selectedUnit: bestUnit.name, 
                inputQty: bestInputQty, qty: totalQty, price: suggestedPrice
            }
        })
        
        formData.value = { 
            ...order, type: order.type || order.Type, items: mappedItems, partnerId: order.partnerId || order.PartnerId || '' 
        }
        
        if (mode === 'convert_po') {
            formData.value.type = 'PO'; formData.value.status = 'pending'; formData.value.code = formData.value.code.replace('PR', 'PO');
        }
    }
    showModal.value = true
}

const closeModal = () => showModal.value = false

const getStatusBadge = (status, type) => {
    if (type === 'PR') { 
        if (status === 'approved') return { text: 'Chờ Thu Mua Chốt Giá', class: 'bg-amber-100 text-amber-700 border-amber-200' } 
    } else {
        if (status === 'pending') return { text: 'Chờ Sếp duyệt giá', class: 'bg-purple-100 text-purple-700 border-purple-200' }
        if (status === 'approved') return { text: 'Đã đặt hàng NCC', class: 'bg-emerald-100 text-emerald-700 border-emerald-200' }
    }
    return { text: 'Từ chối / Hủy', class: 'bg-red-100 text-red-700 border-red-200' }
}

const totalAmount = computed(() => formData.value.items.reduce((sum, item) => sum + (item.qty * (item.price || 0)), 0))

// ĐÃ FIX: LỖI 400 BAD REQUEST KHI LÊN ĐƠN PO
const handleSubmit = async () => {
    if (formData.value.items.length === 0) return alert('Chưa có hàng hóa nào!')
    if (!formData.value.partnerId) return alert('Bắt buộc phải chọn Nhà cung cấp báo giá!')
    try {
        const payload = { 
            ...formData.value, 
            code: modalMode.value.includes('add') ? "" : formData.value.code,
            totalAmount: totalAmount.value // ĐÃ FIX: Truyền tổng tiền về Database
        };
        
        // ĐÃ FIX: Chuyển PR lên PO thực chất là Cập nhật (PUT) đè lên Phiếu cũ
        const method = 'PUT';
        const url = `${ORDER_API}/${formData.value.id}`;
        
        const res = await fetch(url, { method, headers: getAuthHeaders(), body: JSON.stringify(payload) })
        if (res.ok) { 
            alert('Chốt đơn PO thành công!'); 
            await fetchData(); 
            closeModal(); 
        } else { 
            const errText = await res.text();
            alert('LỖI TỪ MÁY CHỦ:\n' + errText); 
        }
    } catch(e) { console.error(e) }
}

const changeStatus = async (id, newStatus, confirmMsg) => {
    if(!confirm(confirmMsg)) return;
    try {
        const res = await fetch(`${ORDER_API}/${id}/status`, { method: 'PUT', headers: getAuthHeaders(), body: JSON.stringify(newStatus) })
        if(res.ok) { alert('Đã cập nhật trạng thái!'); await fetchData(); } else { alert('LỖI HỆ THỐNG'); }
    } catch(e) { console.error(e) }
}

onMounted(() => fetchData())
</script>

<template>
  <div class="space-y-5 md:space-y-6 animate-fade-in pb-10 px-0 md:px-1 relative">
    
    <div class="flex flex-col sm:flex-row sm:items-center justify-between gap-4">
      <div>
        <h2 class="text-xl md:text-2xl font-bold text-gray-800">Đơn Đặt Hàng (PO)</h2>
        <p class="text-[10px] font-bold text-purple-600 bg-purple-50 px-2 py-1 rounded inline-block mt-2 border border-purple-200 uppercase">
            Vai trò: {{ currentRole }}
        </p>
      </div>
    </div>

    <div class="bg-white p-3 md:p-4 rounded-xl border border-gray-200 flex flex-col sm:flex-row items-center gap-3 md:gap-4 shadow-sm">
      <div class="relative w-full sm:flex-1">
          <div class="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none">
              <MagnifyingGlassIcon class="w-5 h-5 text-gray-400" />
          </div>
          <input v-model="searchQuery" type="text" class="block w-full pl-10 pr-3 py-2 border border-gray-200 rounded-lg text-sm outline-none focus:ring-1 focus:ring-purple-500" placeholder="Tìm theo mã phiếu, NCC...">
      </div>
      <select v-model="filterType" class="w-full sm:w-auto border border-gray-200 rounded-lg text-sm px-4 py-2 outline-none focus:ring-1 focus:ring-purple-500 cursor-pointer">
          <option value="">Tất cả (PR chờ giá & PO)</option>
          <option value="PR">Phiếu PR chờ Thu mua báo giá</option>
          <option value="PO">Đơn PO đã chốt</option>
      </select>
    </div>

    <div class="bg-white rounded-xl border border-gray-200 shadow-sm overflow-hidden">
      <div class="w-full overflow-x-auto custom-scrollbar">
        <table class="min-w-[1000px] w-full divide-y divide-gray-200">
          <thead class="bg-gray-50">
            <tr>
                <th class="px-5 py-3.5 text-left text-xs font-bold text-gray-500 uppercase w-28">Mã Phiếu</th>
                <th class="px-5 py-3.5 text-left text-xs font-bold text-gray-500 uppercase w-32">Ngày lập</th>
                <th class="px-5 py-3.5 text-left text-xs font-bold text-gray-500 uppercase w-48">Nhà Cung Cấp</th>
                <th class="px-5 py-3.5 text-left text-xs font-bold text-gray-500 uppercase min-w-[200px]">Tên Mặt Hàng</th>
                <th class="px-5 py-3.5 text-left text-xs font-bold text-gray-500 uppercase min-w-[150px]">Số lượng</th>
                <th class="px-5 py-3.5 text-right text-xs font-bold text-gray-500 uppercase">Tổng Tiền (Dự kiến)</th>
                <th class="px-5 py-3.5 text-center text-xs font-bold text-gray-500 uppercase">Trạng thái</th>
                <th class="px-5 py-3.5 text-right text-xs font-bold text-gray-500 uppercase">Thao tác</th>
            </tr>
          </thead>
          <tbody class="divide-y divide-gray-100 bg-white">
            <tr v-if="filteredOrders.length === 0">
                <td colspan="8" class="px-6 py-16 text-center">
                    <ShoppingCartIcon class="w-12 h-12 text-gray-300 mx-auto mb-3" />
                    <h3 class="text-base font-semibold text-gray-700">Chưa có công việc nào</h3>
                </td>
            </tr>
            <tr v-for="order in filteredOrders" :key="order.id" class="hover:bg-gray-50 transition-colors">
              <td class="px-5 py-3 text-sm font-bold" :class="order.type === 'PO' ? 'text-purple-700' : 'text-indigo-700'">
                  <span class="bg-gray-100 text-[9px] px-1 py-0.5 rounded border mr-1">{{ order.type }}</span>{{ order.code }}
              </td>
              <td class="px-5 py-3 text-sm font-medium text-gray-600">{{ order.date }}</td>
              <td class="px-5 py-3 text-sm font-bold text-gray-800">
                  <span v-if="order.type === 'PR'" class="text-amber-600 italic text-xs font-normal">Chờ Thu mua báo giá</span>
                  <span v-else class="text-purple-700 flex items-center gap-1"><BuildingStorefrontIcon class="w-4 h-4"/> {{ order.supplierName }}</span>
              </td>
              <td class="px-5 py-3"><span class="text-sm font-bold text-gray-800 line-clamp-2" :title="getItemNames(order.items)">{{ getItemNames(order.items) }}</span></td>
              <td class="px-5 py-3"><span class="text-sm font-bold text-purple-600 line-clamp-2" :title="getItemQuantities(order.items)">{{ getItemQuantities(order.items) }}</span></td>
              <td class="px-5 py-3 text-right font-bold text-emerald-600">
                  <span v-if="order.type === 'PR'" class="text-amber-600 italic text-[11px] font-normal">Chờ báo giá</span>
                  <span v-else>{{ formatCurrency(order.totalAmount || 0) }}</span>
              </td>
              <td class="px-5 py-3 text-center">
                  <span :class="['text-[10px] font-bold px-2 py-1 rounded border uppercase tracking-wider', getStatusBadge(order.status, order.type).class]">{{ getStatusBadge(order.status, order.type).text }}</span>
              </td>
              <td class="px-5 py-3 text-right space-x-1.5 whitespace-nowrap">
                <button @click="openModal('view', order)" class="p-1.5 text-gray-600 hover:bg-gray-100 rounded border border-transparent" title="Xem chi tiết"><EyeIcon class="w-4 h-4" /></button>
                <button v-if="order.type === 'PR' && order.status === 'approved' && canCreatePO" @click="openModal('convert_po', order)" class="p-1.5 text-purple-600 hover:bg-purple-100 rounded bg-purple-50 border border-purple-200 font-bold text-xs px-2 shadow-sm"><ArrowRightCircleIcon class="w-4 h-4 inline mr-1" /> Lên PO</button>
                <template v-if="order.type === 'PO'">
                    <button v-if="order.status === 'pending' && canCreatePO" @click="openModal('edit_po', order)" class="p-1.5 text-purple-600 hover:bg-purple-50 rounded" title="Sửa giá"><PencilSquareIcon class="w-4 h-4" /></button>
                    <button v-if="order.status === 'pending' && canApprovePO" @click="changeStatus(order.id, 'approved', 'Sếp đồng ý chốt đơn này?')" class="p-1.5 text-emerald-600 hover:bg-emerald-50 rounded border border-transparent" title="Sếp Duyệt Giá"><CheckCircleIcon class="w-5 h-5" /></button>
                </template>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>

    <Teleport to="body">
      <div v-if="showModal" class="fixed inset-0 z-50 flex items-center justify-center px-4 bg-slate-900/50 backdrop-blur-sm animate-fade-in">
        <div class="bg-white rounded-xl shadow-2xl w-full max-w-[95vw] lg:max-w-6xl overflow-hidden flex flex-col max-h-[90vh]">
          
          <div class="px-6 py-4 border-b border-purple-100 flex items-center justify-between shrink-0 bg-purple-50">
            <h3 class="text-lg font-bold flex items-center gap-2 text-purple-800">
                <ShoppingCartIcon class="w-6 h-6 text-purple-600"/> 
                {{ modalMode === 'convert_po' ? 'Chốt Giá & Lên Đơn Đặt Hàng (PO)' : `Chi tiết PO: ${formData.code}` }}
            </h3>
            <div class="flex items-center gap-4">
                <div class="text-sm font-bold flex items-center bg-white px-3 py-1 rounded-lg border border-purple-200">
                    Trạng thái: <span :class="['ml-2 px-2 py-0.5 rounded text-[10px] uppercase border', getStatusBadge(formData.status, formData.type).class]">{{ getStatusBadge(formData.status, formData.type).text }}</span>
                </div>
                <button @click="closeModal" class="text-gray-400 hover:text-red-500"><XMarkIcon class="w-6 h-6" /></button>
            </div>
          </div>
          
          <div class="p-6 overflow-y-auto flex-1 custom-scrollbar bg-slate-50/50">
            <form id="poForm" @submit.prevent="handleSubmit" class="space-y-6">
              
              <div class="bg-white p-4 rounded-xl border border-gray-200 shadow-sm">
                <div class="grid grid-cols-1 md:grid-cols-4 gap-4">
                  <div>
                      <label class="block text-xs font-bold mb-1">Mã Phiếu</label>
                      <input type="text" disabled class="w-full border rounded-lg px-3 py-2 text-sm bg-gray-100 italic font-bold text-gray-500" :placeholder="formData.code">
                  </div>
                  <div>
                      <label class="block text-xs font-bold mb-1">Ngày đặt hàng *</label>
                      <input v-model="formData.date" :disabled="modalMode === 'view'" type="date" required class="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm outline-none focus:ring-1 focus:ring-purple-500 bg-white">
                  </div>
                  <div class="md:col-span-2">
                    <label class="block text-xs font-bold mb-1 text-purple-700">Nhà cung cấp báo giá *</label>
                    <select v-model="formData.partnerId" :disabled="modalMode === 'view'" required class="w-full border border-purple-300 bg-purple-50 font-bold rounded-lg px-3 py-2 text-sm outline-none focus:ring-2 focus:ring-purple-500">
                        <option value="" disabled>-- Chọn Nhà Cung Cấp báo giá tốt nhất --</option>
                        <option v-for="sup in suppliersList" :key="sup.partnerId" :value="sup.partnerId">[{{sup.partnerCode}}] {{ sup.partnerName }}</option>
                    </select>
                  </div>
                </div>
              </div>

              <div>
                <h4 class="text-sm font-bold mb-2 text-gray-800 uppercase">Cập nhật Đơn Giá Mua</h4>
                <div class="border border-gray-200 rounded-xl overflow-x-auto shadow-sm bg-white">
                  <table class="w-full text-sm text-left">
                    <thead class="bg-gray-100 text-xs uppercase font-bold text-gray-600 border-b border-gray-200">
                      <tr>
                          <th class="px-4 py-3 w-32">Mã SKU</th>
                          <th class="px-4 py-3 min-w-[200px]">Tên Hàng Hóa</th>
                          <th class="px-4 py-3 text-center border-l border-gray-200 w-32">Đơn Vị Tính</th>
                          <th class="px-4 py-3 text-center bg-indigo-50 border-l border-gray-200 w-32">Số Lượng Y/C</th>
                          <th class="px-4 py-3 text-right bg-purple-50 border-l border-gray-200 w-44 text-purple-800">Đơn Giá Mua (ĐVT Gốc)</th>
                          <th class="px-4 py-3 text-right bg-purple-50 border-l border-gray-200 w-40 text-purple-800">Thành tiền</th>
                      </tr>
                    </thead>
                    <tbody class="divide-y divide-gray-100">
                      <tr v-if="formData.items.length === 0"><td colspan="6" class="px-4 py-12 text-center text-gray-400 italic">Phiếu trống.</td></tr>
                      
                      <tr v-for="(item, idx) in formData.items" :key="idx" class="hover:bg-purple-50/30 transition-colors">
                        <td class="px-4 py-3 font-bold text-gray-800">{{ item.sku }}</td>
                        <td class="px-4 py-3 text-gray-700">
                          <span class="block font-medium">{{ item.name }}</span>
                          <span class="text-[10px] text-gray-400 font-bold bg-gray-50 px-1 border rounded mt-0.5 inline-block">ĐVT Gốc: {{item.unit}}</span>
                        </td>
                        
                        <td class="px-4 py-3 text-center border-l border-gray-100">
                            <span class="font-bold text-gray-700">{{ item.selectedUnit }}</span>
                        </td>

                        <td class="px-4 py-3 bg-indigo-50/20 border-l border-gray-100 text-center">
                            <span class="font-bold text-indigo-700 text-lg">{{ item.inputQty }}</span>
                            <span class="block text-[10px] text-gray-500 font-medium mt-0.5">(= {{ item.qty }} {{ item.unit }})</span>
                        </td>
                        
                        <td class="px-4 py-3 text-right bg-purple-50/20 border-l border-gray-100">
                          <div v-if="modalMode === 'convert_po' || modalMode === 'edit_po'" class="flex items-center">
                              <input v-model.number="item.price" type="number" min="0" class="w-full text-right border border-purple-300 rounded-lg px-2 py-1.5 font-bold text-purple-700 focus:ring-2 focus:ring-purple-500 outline-none shadow-inner bg-white" placeholder="Báo giá">
                          </div>
                          <span v-else class="font-bold text-gray-700">{{ formatCurrency(item.price) }}</span>
                        </td>
                        
                        <td class="px-4 py-3 text-right font-bold text-emerald-600 bg-purple-50/20 border-l border-gray-100">
                            {{ formatCurrency(item.qty * (item.price || 0)) }}
                        </td>
                      </tr>
                    </tbody>
                    <tfoot class="bg-gray-50 font-bold border-t border-gray-200">
                      <tr>
                          <td colspan="5" class="px-4 py-3 text-right uppercase text-gray-600">Tổng Tiền Đơn Hàng (Dự Kiến Xuất Quỹ):</td>
                          <td class="px-4 py-3 text-right text-emerald-700 text-lg">{{ formatCurrency(totalAmount) }}</td>
                      </tr>
                    </tfoot>
                  </table>
                </div>
              </div>
              
              <div><label class="block text-xs font-bold mb-1 text-gray-700">Điều khoản thanh toán / Ghi chú</label><textarea v-model="formData.note" :disabled="modalMode === 'view'" rows="2" class="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm focus:ring-1 focus:ring-purple-500 bg-white outline-none" placeholder="Ví dụ: Công nợ 30 ngày..."></textarea></div>

            </form>
          </div>

          <div class="px-6 py-4 border-t border-gray-100 flex justify-end gap-3 bg-gray-50 shrink-0">
            <button type="button" @click="closeModal" class="px-5 py-2.5 border border-gray-300 text-gray-700 rounded-lg text-sm font-semibold hover:bg-gray-100 bg-white transition-colors">{{ modalMode === 'view' ? 'Đóng lại' : 'Hủy bỏ' }}</button>
            <button v-if="modalMode !== 'view'" type="submit" form="poForm" class="px-6 py-2.5 text-white rounded-lg text-sm font-bold shadow-md transition-colors bg-purple-600 hover:bg-purple-700 flex items-center gap-2"><ShoppingCartIcon class="w-5 h-5"/> Lên Đơn Đặt Hàng (PO)</button>
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