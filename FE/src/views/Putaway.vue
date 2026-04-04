<script setup>
import { ref, computed, onMounted } from 'vue'
import { 
  ArrowPathIcon, MapPinIcon, ArchiveBoxIcon, 
  ChevronRightIcon, CheckCircleIcon, ExclamationCircleIcon,
  InboxArrowDownIcon, Square3Stack3DIcon
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

const fetchData = async () => {
  isLoading.value = true
  try {
    const headers = getAuthHeaders()
    const [stockRes, locRes, prodRes] = await Promise.all([ 
        fetch(STOCK_API, { headers }), 
        fetch(LOC_API, { headers }),
        fetch(PROD_API, { headers }) 
    ])
    
    if (prodRes.ok) productsList.value = await prodRes.json()
    if (locRes.ok) locationsList.value = await locRes.json()

    if (stockRes.ok) {
        const rawStocks = await stockRes.json()
        
        stockList.value = rawStocks.map(s => {
            const variantId = s.variantId || s.VariantId;
            const prod = productsList.value.find(p => p.id === variantId || p.Id === variantId) || {};
            return {
                ...s,
                // ÉP KIỂU ĐỒNG BỘ: Chống lỗi chữ hoa chữ thường từ C#
                id: s.id || s.Id || s.StockId,
                locationId: s.locationId || s.LocationId,
                warehouseId: s.warehouseId || s.WarehouseId,
                qty: s.qty || s.Qty || s.Quantity || 0,
                locationCode: s.locationCode || s.LocationCode,
                
                sku: prod.sku || prod.Sku || 'N/A',
                name: prod.name || prod.Name || 'Sản phẩm không xác định',
                conversionRate: prod.conversionRate || prod.ConversionRate || 24,
                moveBoxQty: 0,      
                toLocationId: null  
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

// FIX: Ép kiểu danh sách Kệ để tránh lỗi ID = 0
const targetLocations = computed(() => {
    return locationsList.value
        .map(l => ({
            id: l.id || l.Id,
            code: l.code || l.Code,
            warehouseId: l.warehouseId || l.WarehouseId
        }))
        .filter(l => !myWarehouseId.value || l.warehouseId === myWarehouseId.value)
})

const handlePutaway = async (item) => {
  if (!item.toLocationId || item.moveBoxQty <= 0) return;
  
  if (item.moveBoxQty > item.qty) {
      alert(`Số thùng muốn cất (${item.moveBoxQty} thùng) vượt quá Tồn kho hiện có (${item.qty} thùng)! Sếp kiểm tra lại nhé.`);
      return;
  }

  const totalItems = item.moveBoxQty * item.conversionRate;
  if (!confirm(`Xác nhận cất ${item.moveBoxQty} thùng (tổng ${totalItems} cái) lên kệ?`)) return;

  try {
    const payload = {
        stockId: item.id,
        toLocationId: item.toLocationId,
        qty: item.moveBoxQty 
    };

    const res = await fetch(`https://localhost:7139/api/Stock/move`, {
        method: 'POST',
        headers: getAuthHeaders(),
        body: JSON.stringify(payload)
    })

    if (res.ok) {
        alert("Cất hàng lên kệ thành công!");
        await fetchData(); 
    } else {
        // NÂNG CẤP BẮT LỖI: Lấy thẳng câu thông báo từ C# để báo cho sếp
        try {
            const errorData = await res.json();
            alert("Lỗi từ hệ thống: " + (errorData.message || "Kiểm tra lại kết nối."));
        } catch(err) {
            const errorText = await res.text();
            alert("Lỗi CSDL: " + errorText);
        }
    }
  } catch(e) { 
      console.error(e);
      alert("Lỗi mạng! Không thể gọi API.");
  }
}

onMounted(() => fetchData())
</script>

<template>
  <div class="space-y-6 animate-fade-in pb-10">
    <div>
      <h2 class="text-2xl font-bold text-gray-800">Cất Hàng & Chuyển Kệ</h2>
      <p class="text-sm text-gray-500">Dọn dẹp Khu chờ nhập và luân chuyển vị trí hàng hóa trong cùng một Kho</p>
    </div>

    <div class="bg-white p-6 rounded-xl border border-gray-200 shadow-sm space-y-6">
      <div class="max-w-md">
        <label class="flex items-center gap-2 text-sm font-bold text-indigo-700 mb-2">
            <MapPinIcon class="w-5 h-5"/> BƯỚC 1: Chọn "TỪ KHU VỰC" (Vị trí đang chứa hàng)
        </label>
        <select v-model="selectedFromZone" class="w-full border-2 border-indigo-100 rounded-xl px-4 py-3 bg-indigo-50/50 font-bold text-indigo-800 outline-none focus:ring-2 focus:ring-indigo-500">
            <option value="pending">📦 KHU CHỜ NHẬP (Hàng vừa về)</option>
            <option value="racks">🏗️ TRÊN CÁC KỆ (Chuyển đổi vị trí)</option>
        </select>
        <p class="text-[11px] text-gray-400 mt-2 italic">* Mẹo: Hàng vừa Nhập kho / Điều chuyển tới sẽ nằm ở mục "📦 KHU CHỜ NHẬP".</p>
      </div>

      <div>
        <label class="flex items-center gap-2 text-sm font-bold text-emerald-700 mb-4">
            <InboxArrowDownIcon class="w-5 h-5"/> BƯỚC 2: Chọn Hàng và Cất lên "ĐẾN KỆ"
        </label>

        <div class="border rounded-xl overflow-hidden shadow-sm">
            <table class="w-full text-sm text-left">
                <thead class="bg-gray-50 text-gray-500 font-bold border-b">
                    <tr>
                        <th class="px-4 py-4 uppercase">Sản phẩm (NSX-HSD)</th>
                        <th class="px-4 py-4 text-center">ĐANG ĐỂ Ở</th>
                        <th class="px-4 py-4 text-center bg-blue-50/50 w-40">SỐ THÙNG CẤT</th>
                        <th class="px-4 py-4 text-center w-32">TỔNG SL</th>
                        <th class="px-4 py-4 text-center">ĐẾN KỆ</th>
                        <th class="px-4 py-4 text-center w-20">THAO TÁC</th>
                    </tr>
                </thead>
                <tbody class="divide-y">
                    <tr v-if="isLoading">
                        <td colspan="6" class="px-4 py-12 text-center text-gray-500 font-medium">Đang tải dữ liệu...</td>
                    </tr>
                    <tr v-else-if="filteredStocks.length === 0">
                        <td colspan="6" class="px-4 py-12 text-center text-gray-400 italic">Hiện không có hàng hóa nào tại khu vực này.</td>
                    </tr>
                    <tr v-for="item in filteredStocks" :key="item.id" class="hover:bg-gray-50/80 transition-colors">
                        <td class="px-4 py-3">
                            <div class="font-bold text-gray-900">{{ item.sku }}</div>
                            <div class="text-xs text-gray-500">{{ item.name }}</div>
                            <div class="text-[10px] text-gray-400 mt-1 uppercase">NSX: {{item.nsx || '--'}} | HSD: {{item.hsd || '--'}}</div>
                        </td>
                        <td class="px-4 py-3 text-center">
                            <span v-if="!item.locationId" class="px-2 py-1 bg-amber-100 text-amber-700 rounded text-[10px] font-bold border border-amber-200">BÃI CHỜ NHẬP</span>
                            <span v-else class="font-bold text-indigo-600">{{ item.locationCode }}</span>
                            <div class="text-[10px] text-gray-400 mt-1 font-bold">Tồn: {{ item.qty }} thùng</div>
                        </td>
                        
                        <td class="px-4 py-3 bg-blue-50/30">
                            <input v-model.number="item.moveBoxQty" type="number" min="0" :max="item.qty" class="w-full text-center border-2 border-blue-200 rounded-lg py-2 font-bold text-blue-700 outline-none focus:ring-2 focus:ring-blue-500" placeholder="0">
                            <div class="text-[10px] text-gray-400 text-center mt-1">Quy đổi: 1 Thùng = {{ item.conversionRate }} cái</div>
                        </td>
                        
                        <td class="px-4 py-3 text-center">
                            <div class="text-lg font-bold text-emerald-600">{{ item.moveBoxQty * item.conversionRate }}</div>
                            <div class="text-[10px] text-gray-400 italic">cái/chiếc</div>
                        </td>
                        
                        <td class="px-4 py-3">
                            <select v-model="item.toLocationId" class="w-full border rounded-lg px-2 py-2 text-sm font-semibold outline-none focus:ring-2 focus:ring-emerald-500">
                                <option :value="null">-- Chọn kệ đến --</option>
                                <option v-for="loc in targetLocations" :key="loc.id" :value="loc.id">{{ loc.code }}</option>
                            </select>
                        </td>
                        
                        <td class="px-4 py-3 text-center">
                            <button @click="handlePutaway(item)" :disabled="!item.toLocationId || item.moveBoxQty <= 0 || item.moveBoxQty > item.qty" class="p-2 rounded-lg bg-emerald-600 text-white hover:bg-emerald-700 disabled:bg-gray-200 disabled:text-gray-400 shadow-md transition-all">
                                <ChevronRightIcon class="w-6 h-6" />
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
@keyframes fadeIn { from { opacity: 0; transform: translateY(5px); } to { opacity: 1; transform: translateY(0); } }
</style>