<script setup>
import { ref, computed, onMounted } from 'vue'
import { 
  MapPinIcon, InboxArrowDownIcon,
  ChevronRightIcon, RectangleGroupIcon, ExclamationTriangleIcon,
  ArrowPathIcon
} from '@heroicons/vue/24/outline'

const STOCK_API = 'https://localhost:7139/api/Stock'
const LOC_API = 'https://localhost:7139/api/Locations'
const PROD_API = 'https://localhost:7139/api/Products' 

const getAuthHeaders = () => ({ 'Content-Type': 'application/json', 'Authorization': 'Bearer ' + (localStorage.getItem('authToken') || '') })
const myWarehouseId = ref(parseInt(localStorage.getItem('warehouseId')) || null)

const stockList = ref([])
const locationsList = ref([])
const productsList = ref([])
const isLoading = ref(false)
const selectedFromZone = ref('pending') 

const getAvailableUnits = (prod) => {
    let units = [{ name: prod.unit || prod.Unit || 'SL', rate: 1 }];
    const conversions = prod.conversions || prod.Conversions || [];
    
    if (conversions.length > 0) {
        conversions.forEach(c => units.push({ name: c.altUnit, rate: c.rate }));
    } else if ((prod.conversionRate || prod.ConversionRate) > 1) {
        units.push({ name: 'Thùng/Kiện', rate: prod.conversionRate || prod.ConversionRate });
    }
    return units.sort((a,b) => a.rate - b.rate);
}

// =======================================================================
// ĐÃ ĐỒNG BỘ: THUẬT TOÁN TỰ ĐỘNG DỊCH CHUỖI THÔNG MINH KẾT HỢP "=" VÀ "+"
// =======================================================================
const autoFormatStockText = (qty, units, baseUnit) => {
    if (qty <= 0) return `0 ${baseUnit}`;

    const sortedPacks = [...(units || [])].sort((a, b) => b.rate - a.rate);
    let remainingQty = qty;
    let components = [];
    let topUnit = null;

    // Phân rã số lượng ra các cấp
    for (const pack of sortedPacks) {
        if (pack.rate > 1 && remainingQty >= pack.rate) {
            const count = Math.floor(remainingQty / pack.rate);
            components.push({ count, name: pack.name, rate: pack.rate });
            if (!topUnit) topUnit = pack; // Ghi nhớ đơn vị lớn nhất có số lượng
            remainingQty %= pack.rate;
        }
    }
    if (remainingQty > 0 || components.length === 0) {
        components.push({ count: remainingQty, name: baseUnit, rate: 1 });
    }

    // NẾU LÀ SỐ LƯỢNG CHẴN CỦA 1 ĐƠN VỊ LỚN (VD: Chính xác 1 Pallet, 2 Thùng To)
    if (components.length === 1 && components[0].rate > 1) {
        let chainResult = [];
        const startIndex = sortedPacks.findIndex(u => u.name === topUnit.name);
        for (let i = startIndex; i < sortedPacks.length; i++) {
            const pack = sortedPacks[i];
            if(pack.rate > 1) {
                chainResult.push(`${qty / pack.rate} ${pack.name}`);
            }
        }
        chainResult.push(`${qty} ${baseUnit}`);
        return [...new Set(chainResult)].join(' = '); // Dùng dấu =
    }

    // NẾU LÀ SỐ LƯỢNG LẺ TẺ (VD: 1 Pallet + 2 Thùng To + 5 SL)
    return components.map(c => `${c.count} ${c.name}`).join(' + '); // Dùng dấu +
}

const fetchData = async () => {
  isLoading.value = true
  try {
    const headers = getAuthHeaders()
    const [stockRes, locRes, prodRes] = await Promise.all([ 
        fetch(STOCK_API, { headers }), fetch(LOC_API, { headers }), fetch(PROD_API, { headers }) 
    ])
    
    if (prodRes.ok) productsList.value = await prodRes.json()
    if (locRes.ok) locationsList.value = await locRes.json()

    if (stockRes.ok) {
        const rawStocks = await stockRes.json()
        stockList.value = rawStocks.map(s => {
            const variantId = s.variantId || s.VariantId;
            const prod = productsList.value.find(p => p.id === variantId || p.Id === variantId) || {};
            const units = getAvailableUnits(prod);
            const defaultUnit = units[units.length - 1]; 

            return {
                ...s, id: s.id || s.Id || s.StockId, locationId: s.locationId || s.LocationId, warehouseId: s.warehouseId || s.WarehouseId, qty: s.qty || s.Qty || s.Quantity || 0, locationCode: s.locationCode || s.LocationCode,
                sku: prod.sku || prod.Sku || 'N/A', name: prod.name || prod.Name || 'Sản phẩm không xác định', baseUnit: prod.unit || prod.Unit || 'SL',
                units: units, selectedUnit: defaultUnit.name, selectedRate: defaultUnit.rate, moveInputQty: 0, toLocationId: null  
            }
        })
    }
  } catch (e) { console.error(e) } finally { isLoading.value = false }
}

const filteredStocks = computed(() => {
  return stockList.value.filter(s => {
    const matchWh = !myWarehouseId.value || s.warehouseId === myWarehouseId.value;
    const matchZone = selectedFromZone.value === 'pending' ? !s.locationId : s.locationId;
    return matchWh && matchZone;
  })
})

const targetLocations = computed(() => {
    return locationsList.value.map(l => ({ id: l.id || l.Id, code: l.code || l.Code, warehouseId: l.warehouseId || l.WarehouseId })).filter(l => !myWarehouseId.value || l.warehouseId === myWarehouseId.value)
})

const onUnitChange = (item) => {
    const unitDef = item.units.find(u => u.name === item.selectedUnit);
    if (unitDef) item.selectedRate = unitDef.rate;
}

const getMoveTotalBaseQty = (item) => {
    return (item.moveInputQty || 0) * (item.selectedRate || 1);
}

const handlePutaway = async (item) => {
  if (!item.toLocationId || item.moveInputQty <= 0) return;
  const moveTotalQty = getMoveTotalBaseQty(item);

  if (moveTotalQty > item.qty) {
      alert(`[LỖI] Khu vực này chỉ còn đúng ${autoFormatStockText(item.qty, item.units, item.baseUnit)}! Sếp kiểm tra lại số lượng hoặc chọn lại Đơn vị nhé.`);
      return;
  }
  if (!confirm(`Xác nhận bốc ${item.moveInputQty} ${item.selectedUnit} đi cất lên Kệ đích?`)) return;

  try {
    const payload = { stockId: item.id, toLocationId: item.toLocationId, qty: moveTotalQty };
    const res = await fetch(`https://localhost:7139/api/Stock/move`, { method: 'POST', headers: getAuthHeaders(), body: JSON.stringify(payload) })
    if (res.ok) { alert("Chuyển hàng thành công!"); await fetchData(); } 
    else { try { const errorData = await res.json(); alert("Lỗi từ hệ thống: " + (errorData.message || "Kiểm tra lại.")); } catch(err) { const errorText = await res.text(); alert("Lỗi CSDL: " + errorText); } }
  } catch(e) { console.error(e); alert("Lỗi mạng! Không thể gọi API."); }
}

onMounted(() => fetchData())
</script>

<template>
  <div class="space-y-6 animate-fade-in pb-10">
    <div class="flex items-center justify-between">
      <div>
        <h2 class="text-2xl font-bold text-gray-800">Cất Hàng & Chuyển Kệ (Putaway)</h2>
        <p class="text-sm text-gray-500 mt-1">Dọn dẹp Khu chờ nhập và luân chuyển vị trí hàng hóa trong cùng một Kho</p>
      </div>
      <button @click="fetchData" class="p-2 bg-white border border-gray-200 rounded-lg hover:bg-gray-50 text-gray-600 shadow-sm" title="Làm mới">
        <ArrowPathIcon class="w-5 h-5" :class="{'animate-spin': isLoading}" />
      </button>
    </div>

    <div class="bg-white p-6 rounded-xl border border-gray-200 shadow-sm space-y-6">
      
      <div class="max-w-md">
        <label class="flex items-center gap-2 text-sm font-bold text-indigo-700 mb-2 uppercase tracking-wide"><RectangleGroupIcon class="w-5 h-5"/> Bước 1: Chọn "Từ Khu Vực"</label>
        <div class="relative">
          <div class="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none"><MapPinIcon class="w-5 h-5 text-indigo-500" /></div>
          <select v-model="selectedFromZone" class="w-full border-2 border-indigo-100 rounded-xl pl-10 pr-4 py-3 bg-indigo-50/50 font-bold text-indigo-800 outline-none focus:ring-2 focus:ring-indigo-500 cursor-pointer appearance-none">
              <option value="pending">Khu Chờ Nhập (Hàng vừa cập bến)</option><option value="racks">Trên Các Kệ (Chuyển đổi vị trí kho)</option>
          </select>
          <div class="absolute inset-y-0 right-0 flex items-center pr-3 pointer-events-none"><svg class="h-5 w-5 text-indigo-400" viewBox="0 0 20 20" fill="currentColor"><path fill-rule="evenodd" d="M5.293 7.293a1 1 0 011.414 0L10 10.586l3.293-3.293a1 1 0 111.414 1.414l-4 4a1 1 0 01-1.414 0l-4-4a1 1 0 010-1.414z" clip-rule="evenodd" /></svg></div>
        </div>
      </div>

      <hr class="border-gray-100">

      <div>
        <label class="flex items-center gap-2 text-sm font-bold text-emerald-700 mb-4 uppercase tracking-wide"><InboxArrowDownIcon class="w-5 h-5"/> Bước 2: Chọn Đơn Vị & Cất Lên Kệ</label>

        <div class="border rounded-xl overflow-x-auto shadow-sm">
            <table class="min-w-[1150px] w-full text-sm text-left">
                <thead class="bg-gray-50 text-gray-500 font-bold border-b">
                    <tr>
                        <th class="px-4 py-3.5 uppercase tracking-wider">Sản phẩm</th>
                        <th class="px-4 py-3.5 text-center uppercase tracking-wider min-w-[200px]">Đang Để Ở (Tồn)</th>
                        <th class="px-4 py-3.5 text-center bg-blue-50/50 w-32 uppercase tracking-wider border-l">Bốc bằng Đơn vị</th>
                        <th class="px-4 py-3.5 text-center bg-blue-50/50 w-28 uppercase tracking-wider">Số lượng Bốc</th>
                        <th class="px-4 py-3.5 text-center w-32 uppercase tracking-wider">Thực tế Bốc</th>
                        <th class="px-4 py-3.5 text-center uppercase tracking-wider border-l">Đến Kệ Đích</th>
                        <th class="px-4 py-3.5 text-center w-20 uppercase tracking-wider">Thao tác</th>
                    </tr>
                </thead>
                <tbody class="divide-y divide-gray-100">
                    <tr v-if="isLoading"><td colspan="7" class="px-4 py-12 text-center text-gray-500 font-medium">Đang tải dữ liệu...</td></tr>
                    <tr v-else-if="filteredStocks.length === 0"><td colspan="7" class="px-4 py-16 text-center"><InboxArrowDownIcon class="w-12 h-12 text-gray-300 mx-auto mb-3" /><h3 class="text-base font-semibold text-gray-500">Hiện không có hàng hóa nào tại khu vực này.</h3></td></tr>
                    <tr v-for="item in filteredStocks" :key="item.id" class="hover:bg-gray-50/80 transition-colors">
                        
                        <td class="px-4 py-3">
                            <div class="font-bold text-gray-900 text-base">{{ item.sku }}</div>
                            <div class="text-xs text-gray-500 mt-0.5">{{ item.name }}</div>
                            <div class="text-[10px] text-gray-400 mt-1.5 uppercase font-medium bg-gray-50 px-2 py-0.5 rounded w-fit border border-gray-100">NSX: <span class="text-gray-600">{{item.nsx || '--'}}</span> • HSD: <span class="text-gray-600">{{item.hsd || '--'}}</span></div>
                        </td>
                        
                        <td class="px-4 py-3 text-center">
                            <span v-if="!item.locationId" class="px-2 py-1 bg-amber-100 text-amber-700 rounded text-[10px] font-bold border border-amber-200">Khu Chờ Nhập</span>
                            <span v-else class="font-bold text-indigo-600 bg-indigo-50 px-2 py-1 rounded border border-indigo-100">{{ item.locationCode }}</span>
                            <div class="text-[12px] text-emerald-700 mt-2 font-bold">{{ autoFormatStockText(item.qty, item.units, item.baseUnit) }}</div>
                            <div class="text-[9px] text-gray-400 font-medium mt-0.5">(Tổng: {{ item.qty }} {{item.baseUnit}})</div>
                        </td>
                        
                        <td class="px-4 py-3 bg-blue-50/30 border-l text-center">
                            <select v-model="item.selectedUnit" @change="onUnitChange(item)" class="w-full border border-blue-200 rounded px-2 py-1.5 text-xs font-bold text-blue-800 bg-white outline-none cursor-pointer">
                                <option v-for="u in item.units" :key="u.name" :value="u.name">{{ u.name }}</option>
                            </select>
                            <div class="text-[10px] text-gray-500 mt-1.5 font-medium">1 {{ item.selectedUnit }} = {{ item.selectedRate }} {{ item.baseUnit }}</div>
                        </td>

                        <td class="px-4 py-3 bg-blue-50/30 text-center">
                            <input v-model.number="item.moveInputQty" type="number" min="0" class="w-full text-center border border-blue-300 rounded-lg py-1.5 text-sm font-bold text-blue-700 outline-none focus:ring-1 focus:ring-blue-500 shadow-inner bg-white" placeholder="0">
                        </td>
                        
                        <td class="px-4 py-3 text-center bg-blue-50/10">
                            <div class="text-xs font-bold" :class="getMoveTotalBaseQty(item) > item.qty ? 'text-red-600' : 'text-blue-700'">
                              {{ autoFormatStockText(getMoveTotalBaseQty(item), item.units, item.baseUnit) }}
                            </div>
                        </td>
                        
                        <td class="px-4 py-3 border-l">
                            <select v-model="item.toLocationId" class="w-full border border-gray-300 rounded-lg px-3 py-2.5 text-sm font-semibold outline-none focus:ring-1 focus:ring-emerald-500 cursor-pointer bg-white">
                                <option :value="null" disabled>-- Chọn Kệ Đích --</option>
                                <option v-for="loc in targetLocations" :key="loc.id" :value="loc.id">{{ loc.code }}</option>
                            </select>
                        </td>
                        
                        <td class="px-4 py-3 text-center">
                            <button @click="handlePutaway(item)" :disabled="!item.toLocationId || item.moveInputQty <= 0" class="p-2.5 rounded-lg bg-emerald-600 text-white hover:bg-emerald-700 disabled:bg-gray-200 disabled:text-gray-400 shadow-md transition-all flex items-center justify-center mx-auto">
                                <ChevronRightIcon class="w-5 h-5 font-bold" />
                            </button>
                        </td>
                    </tr>
                </tbody>
            </table>
        </div>
      </div>
    </div>
  </div>
</template>

<style scoped>
.custom-scrollbar::-webkit-scrollbar { height: 6px; width: 6px; }
.custom-scrollbar::-webkit-scrollbar-track { background: transparent; }
.custom-scrollbar::-webkit-scrollbar-thumb { background: #cbd5e1; border-radius: 10px; }
.animate-fade-in { animation: fadeIn 0.2s ease-out; }
</style>