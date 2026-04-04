<script setup>
import { ref, onMounted, computed } from 'vue'
import { 
  MagnifyingGlassIcon, PlusIcon, Squares2X2Icon, MapIcon, 
  TableCellsIcon, XMarkIcon, PencilSquareIcon, TrashIcon, 
  MapPinIcon, ArchiveBoxArrowDownIcon, CubeIcon, AdjustmentsHorizontalIcon
} from '@heroicons/vue/24/outline'
import { uiLogger } from '@/utils/logger'
import { useAuth } from '@/composables/useAuth' 

const { currentUserRole } = useAuth()
const canEdit = computed(() => ['admin', 'giam_doc', 'ql_kho'].includes(currentUserRole.value))

// CHỐT CHẶN: LẤY ĐÚNG ID KHO TỪ TRÌNH DUYỆT
const myWarehouseId = ref(parseInt(localStorage.getItem('warehouseId')) || null)

const currentWorkplace = ref({ branchName: 'Chi nhánh hiện tại', warehouseName: 'Đang tải...', warehouseId: myWarehouseId.value })

const viewMode = ref('map') 
const API_URL = 'https://localhost:7139/api/WarehouseLayout'
const LOC_API_URL = 'https://localhost:7139/api/Locations'
const PROD_API_URL = 'https://localhost:7139/api/Products'
const BRANCH_API = 'https://localhost:7139/api/Branches'

const getAuthHeaders = () => ({ 'Content-Type': 'application/json', 'Authorization': 'Bearer ' + (localStorage.getItem('authToken') || '') })

const warehouseZones = ref([]); const productsList = ref([]); const isLoading = ref(false)
const zoneColors = ['bg-blue-600', 'bg-emerald-600', 'bg-amber-600', 'bg-purple-600', 'bg-rose-600']

const fetchData = async () => {
  isLoading.value = true
  try {
    const headers = getAuthHeaders()
    
    // 1. TÌM ĐÚNG TÊN KHO (SỬA LỖI LẤY NHẦM KHO ĐẦU TIÊN)
    if (myWarehouseId.value) {
        try {
            const branchRes = await fetch(BRANCH_API, { headers })
            if (branchRes.ok) {
                const branches = await branchRes.json()
                let foundWh = null;
                for (const b of branches) {
                    const bId = b.id || b.Id;
                    const whRes = await fetch(`${BRANCH_API}/${bId}/warehouses-detail`, { headers })
                    if(whRes.ok) {
                        const whData = await whRes.json()
                        // Tìm đúng kho có ID khớp với myWarehouseId
                        foundWh = whData.find(w => w.warehouseId === myWarehouseId.value || w.id === myWarehouseId.value)
                        if (foundWh) break;
                    }
                }
                if (foundWh) {
                    currentWorkplace.value.warehouseName = foundWh.whname || foundWh.name || `Kho ${myWarehouseId.value}`;
                } else {
                    currentWorkplace.value.warehouseName = `Kho ID: ${myWarehouseId.value}`;
                }
            }
        } catch(e) { console.log(e) }
    } else {
        currentWorkplace.value.warehouseName = "Tất cả Kho (Admin)";
    }

    // 2. LẤY DỮ LIỆU SƠ ĐỒ KHO
    const [zoneRes, rackRes, prodRes, locRes] = await Promise.all([
      fetch(`${API_URL}/zones`, { headers }), fetch(`${API_URL}/racks`, { headers }),
      fetch(PROD_API_URL, { headers }), fetch(LOC_API_URL, { headers })
    ])
    
    if (prodRes.ok) productsList.value = await prodRes.json()
    let zonesData = [], racksData = [], locsData = []
    if (zoneRes.ok) zonesData = await zoneRes.json()
    if (rackRes.ok) racksData = await rackRes.json()
    if (locRes.ok) locsData = await locRes.json()

    // LỌC CHẶT CHẼ TRỰC TIẾP TỪ myWarehouseId.value CHỨ KHÔNG DÙNG BIẾN TRUNG GIAN NỮA
    const filteredZones = zonesData.filter(z => !myWarehouseId.value || z.warehouseId === myWarehouseId.value || z.WarehouseId === myWarehouseId.value)

    warehouseZones.value = filteredZones.map((zone, index) => {
      const zoneRacks = racksData.filter(r => r.zoneId === zone.id).map(rack => {
        const tiers = rack.tiers || 4; const binsPerTier = rack.binsPerTier || 2; const bins = [];
        for (let t = tiers; t >= 1; t--) {
          for (let b = 1; b <= binsPerTier; b++) {
            const binCode = `${rack.code}-T${t}-O${b}`
            const locDb = locsData.find(l => l.code === binCode)
            bins.push({ code: binCode, tier: t, bin: b, status: locDb ? locDb.status : 'empty', variantIds: locDb ? locDb.variantIds : [] })
          }
        }
        return { id: rack.id, name: rack.code, tiers: tiers, binsPerTier: binsPerTier, bins: bins }
      })
      return { id: zone.id, name: zone.code, color: zoneColors[index % zoneColors.length], racks: zoneRacks }
    })
  } catch (error) { console.error("Lỗi:", error) } finally { isLoading.value = false }
}

const showConfigModal = ref(false); const configMode = ref('add'); const configForm = ref({ zoneName: '', rackName: '', tiers: null, binsPerTier: null }); const editingRackId = ref(null)

// ĐÃ XÓA TỰ SINH MÃ KỆ - TRẢ LẠI Ô NHẬP TAY
const openConfigModal = () => { 
  configMode.value = 'add'; editingRackId.value = null;
  configForm.value = { zoneName: '', rackName: '', tiers: null, binsPerTier: null }; 
  showConfigModal.value = true 
}

const openEditModal = (zone, rack) => { configMode.value = 'edit'; editingRackId.value = rack.id; configForm.value = { zoneName: zone.name, rackName: rack.name, tiers: rack.tiers, binsPerTier: rack.binsPerTier }; showConfigModal.value = true }
const closeConfigModal = () => showConfigModal.value = false

const handleSaveConfig = async () => {
  if (!myWarehouseId.value) return alert("Chỉ quản lý kho cụ thể mới được xây kệ!");
  try {
    const payload = { code: configForm.value.rackName.toUpperCase(), tiers: configForm.value.tiers, binsPerTier: configForm.value.binsPerTier };
    if (configMode.value === 'add') {
      let currentZoneId = null
      let existingZone = warehouseZones.value.find(z => z.name.toUpperCase() === configForm.value.zoneName.toUpperCase())
      if (existingZone) currentZoneId = existingZone.id
      else {
        const resZone = await fetch(`${API_URL}/zones`, { method: 'POST', headers: getAuthHeaders(), body: JSON.stringify({ code: configForm.value.zoneName.toUpperCase(), warehouseId: myWarehouseId.value }) })
        if (resZone.ok) currentZoneId = (await resZone.json()).id
      }
      payload.zoneId = currentZoneId;
      await fetch(`${API_URL}/racks`, { method: 'POST', headers: getAuthHeaders(), body: JSON.stringify(payload) })
      await fetchData(); closeConfigModal(); alert('Xây kệ thành công!');
    } else {
      await fetch(`${API_URL}/racks/${editingRackId.value}`, { method: 'PUT', headers: getAuthHeaders(), body: JSON.stringify(payload) })
      await fetchData(); closeConfigModal(); alert('Cập nhật kệ thành công!');
    }
  } catch (error) { console.error(error) }
}

const deleteRack = async (zone, rack) => {
  if (confirm(`ĐẬP BỎ kệ [${rack.name}]?`)) {
    await fetch(`${API_URL}/racks/${rack.id}`, { method: 'DELETE', headers: getAuthHeaders() }); await fetchData()
  }
}

// TÍNH NĂNG XEM VÀ LẤY HÀNG RA
const showAssignModal = ref(false)
const assignForm = ref({ locationCode: '', currentItems: [] })

const openAssignModal = (bin) => {
  assignForm.value.locationCode = bin.code
  assignForm.value.currentItems = bin.variantIds.map(vId => {
    const p = productsList.value.find(x => x.id === vId);
    return { variantId: vId, name: p ? p.name : 'Sản phẩm ẩn' }
  })
  showAssignModal.value = true
}
const closeAssignModal = () => showAssignModal.value = false

const handleRemoveItem = async (variantId) => {
  try {
    const res = await fetch(`${LOC_API_URL}/remove-stock-item/${assignForm.value.locationCode}/${variantId}`, {
      method: 'DELETE', headers: getAuthHeaders()
    })
    if (res.ok) {
      uiLogger.log('API_CALL', '/warehouse-layout', `Lấy hàng ra khỏi Ô: ${assignForm.value.locationCode}`)
      await fetchData(); closeAssignModal();
    }
  } catch(e) { console.error(e) }
}

const getBinColor = (status) => {
  switch(status) {
    case 'empty': return 'bg-gray-100 border-gray-300 text-gray-500 hover:bg-gray-200 cursor-pointer'
    case 'partial': return 'bg-amber-100 border-amber-400 text-amber-700 hover:bg-amber-200 cursor-pointer shadow-md'
    case 'full': return 'bg-red-500 border-red-600 text-white shadow-lg hover:bg-red-600 cursor-pointer'
    default: return 'bg-gray-100 border-gray-300 text-gray-500'
  }
}

const allBinsList = computed(() => {
  let list = []
  warehouseZones.value.forEach(zone => {
    zone.racks.forEach(rack => { rack.bins.forEach(bin => { list.push({ ...bin, zoneName: zone.name, rackName: rack.name }) }) })
  })
  return list
})

onMounted(() => { fetchData() })
</script>

<template>
  <div class="space-y-4 md:space-y-6 animate-fade-in pb-10 px-0 md:px-1">
    
    <div class="flex flex-col md:flex-row md:items-start justify-between gap-4">
      <div>
        <h2 class="text-xl md:text-2xl font-bold text-gray-800">Sơ đồ Kho hàng</h2>
        <p class="text-xs md:text-sm text-gray-500 mt-1">Trực quan hóa cấu trúc. Click vào Ô kệ để xem và Lấy hàng ra bãi tập kết.</p>
      </div>
      
      <div class="flex flex-col items-end gap-3">
        <div class="bg-blue-50 border border-blue-200 text-blue-800 px-3 py-1.5 rounded-lg flex items-center gap-2 text-sm shadow-sm w-full md:w-auto">
          <MapPinIcon class="w-5 h-5 text-blue-600" />
          <div class="flex flex-col">
            <span class="text-[10px] font-bold uppercase tracking-wider text-blue-500">KHO HIỆN TẠI</span>
            <span class="font-semibold">{{ currentWorkplace.warehouseName }}</span>
          </div>
        </div>

        <div class="flex items-center gap-2 w-full md:w-auto">
          <div class="bg-gray-100 p-1 rounded-lg flex items-center hidden sm:flex">
            <button @click="viewMode = 'map'" :class="['px-3 py-1.5 text-sm font-medium rounded-md flex items-center gap-1', viewMode === 'map' ? 'bg-white shadow text-primary-600' : 'text-gray-500 hover:text-gray-700']"><MapIcon class="w-4 h-4"/> Sơ đồ</button>
            <button @click="viewMode = 'table'" :class="['px-3 py-1.5 text-sm font-medium rounded-md flex items-center gap-1', viewMode === 'table' ? 'bg-white shadow text-primary-600' : 'text-gray-500 hover:text-gray-700']"><TableCellsIcon class="w-4 h-4"/> Danh sách</button>
          </div>
          <button v-if="canEdit" @click="openConfigModal" class="bg-primary-600 hover:bg-primary-700 text-white px-3 md:px-4 py-2 md:py-2.5 rounded-lg flex items-center gap-2 text-sm font-bold shadow-sm transition-colors w-full sm:w-auto justify-center">
            <PlusIcon class="w-5 h-5" /> Xây Kệ Mới
          </button>
        </div>
      </div>
    </div>

    <div v-if="viewMode === 'map'" class="bg-white border-2 border-slate-200 rounded-xl p-4 md:p-8 overflow-x-auto shadow-sm min-h-[500px] relative bg-grid-pattern mt-4 flex" :class="warehouseZones.length === 0 ? 'items-center justify-center' : ''">
      <div v-if="isLoading" class="text-center w-full text-gray-500 font-medium">Đang vẽ sơ đồ...</div>
      <div v-else-if="warehouseZones.length === 0" class="text-center p-6 bg-white/80 rounded-xl backdrop-blur-sm border border-gray-200 shadow-sm m-auto">
        <Squares2X2Icon class="w-16 h-16 text-slate-300 mx-auto mb-4" />
        <h3 class="text-lg font-bold text-slate-700">Kho này đang trống</h3>
        <p class="text-sm text-gray-500 mt-1">Sếp bấm nút "Xây Kệ Mới" ở góc phải để bắt đầu thiết lập nhé.</p>
      </div>

      <div v-else class="w-full">
        <div v-for="zone in warehouseZones" :key="zone.id" class="mb-12 last:mb-0">
          <div class="flex items-center gap-4 mb-4">
            <div :class="[zone.color, 'text-white px-4 py-1.5 rounded-r-full font-bold text-sm tracking-widest shadow-md uppercase']">{{ zone.name }}</div>
            <div class="flex-1 border-t-2 border-dashed border-slate-300"></div>
          </div>
          <div class="flex flex-nowrap gap-6 pb-4">
            <div v-for="rack in zone.racks" :key="rack.id" class="flex flex-col bg-white border-4 border-slate-700 rounded-t-lg shadow-lg shrink-0">
              <div class="bg-slate-700 text-white py-1.5 px-2 relative group flex items-center justify-center min-h-[32px]">
                <span class="font-bold text-sm tracking-wider uppercase">{{ rack.name }}</span>
                <div v-if="canEdit" class="absolute right-1 top-0.5 hidden group-hover:flex gap-1 bg-slate-700 pl-2">
                  <button @click="openEditModal(zone, rack)" class="p-1 hover:bg-blue-600 rounded text-blue-300 hover:text-white" title="Sửa Kệ"><PencilSquareIcon class="w-4 h-4"/></button>
                  <button @click="deleteRack(zone, rack)" class="p-1 hover:bg-red-600 rounded text-red-300 hover:text-white" title="Đập Kệ"><TrashIcon class="w-4 h-4"/></button>
                </div>
              </div>
              <div class="flex p-3 bg-slate-50 relative">
                <div class="flex flex-col justify-between pr-3 border-r-2 border-slate-300 py-2 mr-3 font-bold text-slate-400 text-[10px] md:text-xs">
                  <span v-for="t in rack.tiers" :key="t">TẦNG {{ rack.tiers - t + 1 }}</span>
                </div>
                <div class="grid gap-2" :style="{ gridTemplateColumns: `repeat(${rack.binsPerTier}, minmax(0, 1fr))` }">
                  <div v-for="bin in rack.bins" :key="bin.code" 
                       @click="openAssignModal(bin)" 
                       :class="[getBinColor(bin.status), 'w-12 h-10 md:w-16 md:h-12 border-2 rounded flex flex-col items-center justify-center transition-all hover:-translate-y-0.5']">
                    <span class="text-[10px] md:text-xs font-bold opacity-80">Ô {{ bin.bin }}</span>
                    <span v-if="bin.variantIds.length > 0" class="text-[8px] font-bold mt-0.5 bg-black/20 px-1 rounded">{{ bin.variantIds.length }} món</span>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>

    <div v-if="viewMode === 'table'" class="bg-white rounded-xl border border-gray-200 shadow-sm overflow-hidden mt-4 animate-fade-in">
      <div class="bg-gray-50 px-4 py-3 border-b border-gray-200 flex items-center justify-between">
        <span class="text-sm font-medium text-gray-600">Tổng số ô kệ: <strong class="text-primary-600">{{ allBinsList.length }}</strong> vị trí</span>
      </div>
      <div class="w-full overflow-x-auto">
        <table class="min-w-[950px] w-full divide-y divide-gray-200">
          <thead class="bg-gray-50">
            <tr>
              <th class="px-5 py-3.5 text-left text-xs font-bold text-gray-500 uppercase">Mã Ô Kệ (Location)</th>
              <th class="px-5 py-3.5 text-left text-xs font-bold text-gray-500 uppercase">Thuộc Dãy / Kệ</th>
              <th class="px-5 py-3.5 text-center text-xs font-bold text-gray-500 uppercase">Tọa độ chi tiết</th>
              <th class="px-5 py-3.5 text-center text-xs font-bold text-gray-500 uppercase">Trạng thái chứa hàng</th>
            </tr>
          </thead>
          <tbody class="divide-y divide-gray-100 bg-white">
            <tr v-for="bin in allBinsList" :key="bin.code" class="hover:bg-gray-50">
              <td class="px-5 py-3 text-sm font-bold text-primary-600 flex items-center gap-2"><MapPinIcon class="w-4 h-4 text-gray-400"/> {{ bin.code }}</td>
              <td class="px-5 py-3 text-sm font-bold text-gray-700 uppercase">{{ bin.zoneName }} - Kệ {{ bin.rackName }}</td>
              <td class="px-5 py-3 text-center">
                <span class="bg-slate-100 text-slate-700 px-2 py-1 rounded text-xs font-semibold mr-1">Tầng {{ bin.tier }}</span>
                <span class="bg-slate-100 text-slate-700 px-2 py-1 rounded text-xs font-semibold">Ô {{ bin.bin }}</span>
              </td>
              <td class="px-5 py-3 text-center">
                <span v-if="bin.status === 'empty'" class="bg-gray-100 text-gray-600 px-3 py-1 rounded-full text-[10px] font-bold border uppercase">Đang Trống</span>
                <span v-else class="bg-amber-100 text-amber-700 border-amber-200 px-3 py-1 rounded-full text-[10px] font-bold border uppercase">Đang chứa {{ bin.variantIds.length }} món</span>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>

    <Teleport to="body">
      <div v-if="showConfigModal" class="fixed inset-0 z-50 flex items-center justify-center px-4 bg-slate-900/50 backdrop-blur-sm animate-fade-in">
        <div class="bg-white rounded-xl shadow-2xl w-full max-w-md overflow-hidden">
          <div class="px-5 py-4 border-b flex justify-between bg-gray-50">
            <h3 class="text-lg font-bold flex gap-2"><PlusIcon class="w-5 h-5"/> {{ configMode === 'add' ? 'Xây Kệ Hàng Mới' : 'Cập nhật Kệ Hàng' }}</h3>
            <button @click="closeConfigModal" class="text-gray-400 hover:text-red-500"><XMarkIcon class="w-6 h-6"/></button>
          </div>
          <div class="p-5">
            <form @submit.prevent="handleSaveConfig" class="space-y-4">
              <div>
                <label class="block text-xs font-bold mb-1">Thuộc Dãy (Khu vực) *</label>
                <input v-model="configForm.zoneName" :disabled="configMode === 'edit'" required type="text" class="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm uppercase disabled:bg-gray-100" placeholder="VD: DAY A">
              </div>
              <div>
                <label class="block text-xs font-bold mb-1">Mã Kệ *</label>
                <input v-model="configForm.rackName" required type="text" class="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm uppercase" placeholder="VD: KE-A01">
              </div>
              
              <div class="grid grid-cols-2 gap-4">
                <div>
                  <label class="block text-xs font-bold mb-1">Số Tầng (Dọc) *</label>
                  <input v-model.number="configForm.tiers" required type="number" min="1" max="10" class="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm" placeholder="VD: 4">
                </div>
                <div>
                  <label class="block text-xs font-bold mb-1">Số Ô / Tầng (Ngang) *</label>
                  <input v-model.number="configForm.binsPerTier" required type="number" min="1" max="10" class="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm" placeholder="VD: 2">
                </div>
              </div>

              <div class="mt-6 pt-4 border-t flex justify-end gap-3">
                <button type="button" @click="closeConfigModal" class="px-4 py-2 border border-gray-300 rounded-lg text-sm font-semibold hover:bg-gray-50">Hủy</button>
                <button type="submit" class="px-4 py-2 bg-primary-600 text-white rounded-lg text-sm font-bold shadow-sm hover:bg-primary-700">
                  {{ configMode === 'add' ? 'Xác nhận Xây' : 'Cập nhật' }}
                </button>
              </div>
            </form>
          </div>
        </div>
      </div>
    </Teleport>

    <Teleport to="body">
      <div v-if="showAssignModal" class="fixed inset-0 z-50 flex items-center justify-center px-4 bg-slate-900/50 backdrop-blur-sm animate-fade-in">
        <div class="bg-white rounded-xl shadow-2xl w-full max-w-lg overflow-hidden">
          <div class="px-5 py-4 border-b flex justify-between bg-blue-50">
            <h3 class="text-lg font-bold text-blue-800 flex items-center gap-2">
              <ArchiveBoxArrowDownIcon class="w-5 h-5"/> Hàng trong Ô: {{ assignForm.locationCode }}
            </h3>
            <button @click="closeAssignModal" class="text-gray-400 hover:text-red-500"><XMarkIcon class="w-6 h-6"/></button>
          </div>
          <div class="p-5 space-y-6">
            
            <div>
              <div v-if="assignForm.currentItems.length === 0" class="text-sm text-gray-400 italic bg-gray-50 p-3 rounded border border-dashed text-center">
                Ô này hiện đang trống.
              </div>
              <ul v-else class="space-y-2 max-h-60 overflow-y-auto custom-scrollbar pr-2">
                <li v-for="(item, idx) in assignForm.currentItems" :key="idx" class="flex justify-between items-center bg-amber-50 border border-amber-200 p-2 rounded">
                  <span class="text-sm font-semibold text-gray-800 flex items-center gap-2"><CubeIcon class="w-4 h-4 text-amber-600"/> {{ item.name }}</span>
                  <button @click="handleRemoveItem(item.variantId)" class="text-xs bg-white text-red-500 border border-red-200 px-3 py-1.5 rounded hover:bg-red-50 font-bold transition-colors">Lấy xuống bãi</button>
                </li>
              </ul>
              <p v-if="assignForm.currentItems.length > 0" class="text-[10px] text-gray-500 mt-3 text-center italic">* Lấy hàng xuống sẽ đẩy sản phẩm về lại "Khu Chờ Nhập" (Bãi tập kết).</p>
            </div>

          </div>
        </div>
      </div>
    </Teleport>
  </div>
</template>

<style scoped>
.bg-grid-pattern { background-size: 30px 30px; background-image: linear-gradient(to right, #f1f5f9 1px, transparent 1px), linear-gradient(to bottom, #f1f5f9 1px, transparent 1px); }
.custom-scrollbar::-webkit-scrollbar { height: 6px; width: 6px; }
.custom-scrollbar::-webkit-scrollbar-track { background: transparent; }
.custom-scrollbar::-webkit-scrollbar-thumb { background: #cbd5e1; border-radius: 10px; }
.animate-fade-in { animation: fadeIn 0.2s ease-out; }
@keyframes fadeIn { from { opacity: 0; transform: translateY(5px); } to { opacity: 1; transform: translateY(0); } }
</style>