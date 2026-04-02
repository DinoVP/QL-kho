<script setup>
import { ref, onMounted, computed } from 'vue'
import { 
  MagnifyingGlassIcon, PlusIcon, Squares2X2Icon, MapIcon, 
  TableCellsIcon, XMarkIcon, PencilSquareIcon, TrashIcon, 
  MapPinIcon, ArchiveBoxArrowDownIcon, ArrowUpOnSquareStackIcon
} from '@heroicons/vue/24/outline'
import { uiLogger } from '@/utils/logger'
import { useAuth } from '@/composables/useAuth' 

const { currentUserRole } = useAuth()
const canEdit = computed(() => ['admin', 'giam_doc', 'ql_kho'].includes(currentUserRole.value))

const currentWorkplace = ref({ branchName: 'Chi nhánh hiện tại', warehouseName: 'Đang tải Kho...', warehouseId: null })

const viewMode = ref('map') 
const API_URL = 'https://localhost:7139/api/WarehouseLayout'
const LOC_API_URL = 'https://localhost:7139/api/Locations'
const PROD_API_URL = 'https://localhost:7139/api/Products'

const getAuthHeaders = () => ({
  'Content-Type': 'application/json',
  'Authorization': 'Bearer ' + (localStorage.getItem('authToken') || '')
})

const warehouseZones = ref([])
const productsList = ref([])
const isLoading = ref(false)
const zoneColors = ['bg-blue-600', 'bg-emerald-600', 'bg-amber-600', 'bg-purple-600', 'bg-rose-600']

const fetchData = async () => {
  isLoading.value = true
  try {
    const headers = getAuthHeaders()
    const whRes = await fetch(`${API_URL}/warehouses-dropdown`, { headers })
    if (whRes.ok) {
      const whData = await whRes.json()
      if (whData.length > 0) {
        currentWorkplace.value.warehouseId = whData[0].id
        currentWorkplace.value.warehouseName = whData[0].name
      }
    }

    const [zoneRes, rackRes, prodRes, locRes] = await Promise.all([
      fetch(`${API_URL}/zones`, { headers }), fetch(`${API_URL}/racks`, { headers }),
      fetch(PROD_API_URL, { headers }), fetch(LOC_API_URL, { headers })
    ])
    
    if (prodRes.ok) productsList.value = await prodRes.json()
    let zonesData = [], racksData = [], locsData = []
    if (zoneRes.ok) zonesData = await zoneRes.json()
    if (rackRes.ok) racksData = await rackRes.json()
    if (locRes.ok) locsData = await locRes.json()

    const filteredZones = zonesData.filter(z => z.warehouseId === currentWorkplace.value.warehouseId)

    warehouseZones.value = filteredZones.map((zone, index) => {
      const zoneRacks = racksData.filter(r => r.zoneId === zone.id).map(rack => {
        const tiers = rack.tiers || 4; 
        const binsPerTier = rack.binsPerTier || 2;
        const bins = [];
        for (let t = tiers; t >= 1; t--) {
          for (let b = 1; b <= binsPerTier; b++) {
            const binCode = `${rack.code}-T${t}-O${b}`
            const locDb = locsData.find(l => l.code === binCode)
            // Gắn mảng VariantIds vào để biết ô đang chứa những món gì
            bins.push({ code: binCode, tier: t, bin: b, status: locDb ? locDb.status : 'empty', variantIds: locDb ? locDb.variantIds : [] })
          }
        }
        return { id: rack.id, name: rack.code, tiers: tiers, binsPerTier: binsPerTier, bins: bins }
      })
      return { id: zone.id, name: zone.code, color: zoneColors[index % zoneColors.length], racks: zoneRacks }
    })
  } catch (error) { console.error("Lỗi:", error) } 
  finally { isLoading.value = false }
}

// LOGIC XÂY KỆ (ĐÃ BỎ MAX WEIGHT)
const showConfigModal = ref(false)
const configMode = ref('add') 
const editingRackId = ref(null) 
const configForm = ref({ zoneName: '', rackName: '', tiers: null, binsPerTier: null })

const openConfigModal = () => {
  configMode.value = 'add'; editingRackId.value = null
  configForm.value = { zoneName: '', rackName: '', tiers: null, binsPerTier: null }
  showConfigModal.value = true
}

const openEditModal = (zone, rack) => {
  configMode.value = 'edit'; editingRackId.value = rack.id
  configForm.value = { zoneName: zone.name, rackName: rack.name, tiers: rack.tiers, binsPerTier: rack.binsPerTier }
  showConfigModal.value = true
}
const closeConfigModal = () => showConfigModal.value = false

const handleSaveConfig = async () => {
  if (!currentWorkplace.value.warehouseId) return;
  const rackCodeClean = configForm.value.rackName.toUpperCase();
  const zoneCodeClean = configForm.value.zoneName.toUpperCase();
  const payload = { code: rackCodeClean, tiers: configForm.value.tiers, binsPerTier: configForm.value.binsPerTier };

  try {
    if (configMode.value === 'add') {
      let currentZoneId = null
      let existingZone = warehouseZones.value.find(z => z.name.toUpperCase() === zoneCodeClean)
      if (existingZone) currentZoneId = existingZone.id
      else {
        const resZone = await fetch(`${API_URL}/zones`, { method: 'POST', headers: getAuthHeaders(), body: JSON.stringify({ code: zoneCodeClean, warehouseId: currentWorkplace.value.warehouseId }) })
        if (resZone.ok) currentZoneId = (await resZone.json()).id
      }
      payload.zoneId = currentZoneId;
      const resRack = await fetch(`${API_URL}/racks`, { method: 'POST', headers: getAuthHeaders(), body: JSON.stringify(payload) })
      if (resRack.ok) { await fetchData(); closeConfigModal(); alert('Xây kệ thành công!'); }
    } else {
      const resRack = await fetch(`${API_URL}/racks/${editingRackId.value}`, { method: 'PUT', headers: getAuthHeaders(), body: JSON.stringify(payload) })
      if (resRack.ok) { await fetchData(); closeConfigModal(); alert('Cập nhật kệ thành công!'); }
    }
  } catch (error) { console.error("Lỗi:", error) }
}

const deleteRack = async (zone, rack) => {
  if (confirm(`ĐẬP BỎ kệ [${rack.name}]?`)) {
    await fetch(`${API_URL}/racks/${rack.id}`, { method: 'DELETE', headers: getAuthHeaders() })
    await fetchData()
  }
}

// ==========================================
// TÍNH NĂNG GÁN HÀNG VÀ LẤY HÀNG RA
// ==========================================
const showAssignModal = ref(false)
const assignForm = ref({ locationCode: '', variantId: '', currentItems: [] })

const openAssignModal = (bin) => {
  if (!canEdit.value) return; 
  assignForm.value.locationCode = bin.code
  assignForm.value.variantId = productsList.value.length > 0 ? productsList.value[0].id : ''
  // Lấy tên các sản phẩm đang nằm trong ô này
  assignForm.value.currentItems = bin.variantIds.map(vId => {
    const p = productsList.value.find(x => x.id === vId);
    return { variantId: vId, name: p ? p.name : 'Sản phẩm ẩn' }
  })
  showAssignModal.value = true
}

const closeAssignModal = () => showAssignModal.value = false

const handleAssignProduct = async () => {
  if (!assignForm.value.variantId) return;
  try {
    const res = await fetch(`${LOC_API_URL}/assign-stock`, {
      method: 'POST', headers: getAuthHeaders(),
      body: JSON.stringify({ locationCode: assignForm.value.locationCode, variantId: assignForm.value.variantId })
    })
    if (res.ok) {
      uiLogger.log('API_CALL', '/warehouse-layout', `Cất hàng vào Ô: ${assignForm.value.locationCode}`)
      await fetchData(); closeAssignModal();
    }
  } catch(e) { console.error(e) }
}

// NÚT RÚT 1 MÓN HÀNG RA KHỎI KỆ
const handleRemoveItem = async (variantId) => {
  try {
    const res = await fetch(`${LOC_API_URL}/remove-stock-item/${assignForm.value.locationCode}/${variantId}`, {
      method: 'DELETE', headers: getAuthHeaders()
    })
    if (res.ok) {
      uiLogger.log('API_CALL', '/warehouse-layout', `Lấy hàng ra khỏi Ô: ${assignForm.value.locationCode}`)
      await fetchData(); 
      closeAssignModal();
    }
  } catch(e) { console.error(e) }
}

// Bảng màu cho Ô: Rỗng (Trống), Partial (Đang chứa 1-2 món), Full (Nhiều món)
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
        <h2 class="text-xl md:text-2xl font-bold text-gray-800">Sơ đồ Kho hàng (Visual Map)</h2>
        <p class="text-xs md:text-sm text-gray-500 mt-1">Trực quan hóa cấu trúc. <strong class="text-primary-600">Click vào Ô kệ để Cất hàng hoặc Lấy hàng.</strong></p>
      </div>
      
      <div class="flex flex-col items-end gap-3">
        <div class="bg-blue-50 border border-blue-200 text-blue-800 px-3 py-1.5 rounded-lg flex items-center gap-2 text-sm shadow-sm w-full md:w-auto">
          <MapPinIcon class="w-5 h-5 text-blue-600" />
          <div class="flex flex-col">
            <span class="text-[10px] font-bold uppercase tracking-wider text-blue-500">{{ currentWorkplace.branchName }}</span>
            <span class="font-semibold">{{ currentWorkplace.warehouseName }}</span>
          </div>
        </div>

        <div class="flex items-center gap-2 w-full md:w-auto">
          <div class="bg-gray-100 p-1 rounded-lg flex items-center hidden sm:flex">
            <button @click="viewMode = 'map'" :class="['px-3 py-1.5 text-sm font-medium rounded-md transition-all flex items-center gap-1', viewMode === 'map' ? 'bg-white shadow text-primary-600' : 'text-gray-500 hover:text-gray-700']"><MapIcon class="w-4 h-4"/> Sơ đồ</button>
            <button @click="viewMode = 'table'" :class="['px-3 py-1.5 text-sm font-medium rounded-md transition-all flex items-center gap-1', viewMode === 'table' ? 'bg-white shadow text-primary-600' : 'text-gray-500 hover:text-gray-700']"><TableCellsIcon class="w-4 h-4"/> Danh sách</button>
          </div>
          <button v-if="canEdit" @click="openConfigModal" class="bg-primary-600 hover:bg-primary-700 text-white px-3 md:px-4 py-2 md:py-2.5 rounded-lg flex items-center gap-2 text-sm font-bold shadow-sm transition-colors w-full sm:w-auto justify-center">
            <PlusIcon class="w-5 h-5" /> Xây Kệ Mới
          </button>
        </div>
      </div>
    </div>

    <div v-if="viewMode === 'map'" class="bg-white border-2 border-slate-200 rounded-xl p-4 md:p-8 overflow-x-auto shadow-sm min-h-[500px] relative bg-grid-pattern mt-4 flex" :class="warehouseZones.length === 0 ? 'items-center justify-center' : ''">
      <div v-if="isLoading" class="text-center w-full text-gray-500 font-medium">Đang vẽ sơ đồ...</div>
      <div v-else-if="warehouseZones.length === 0" class="text-center p-6 bg-white/80 rounded-xl backdrop-blur-sm border border-gray-200 shadow-sm">
        <Squares2X2Icon class="w-16 h-16 text-slate-300 mx-auto mb-4" />
        <h3 class="text-lg font-bold text-slate-700">Kho đang trống</h3>
        <p class="text-sm text-gray-500 mt-1">Sếp bấm nút "Xây Kệ Mới" ở góc phải để bắt đầu thiết lập nhé.</p>
      </div>

      <div v-else class="w-full">
        <div v-for="zone in warehouseZones" :key="zone.id" class="mb-12 last:mb-0">
          <div class="flex items-center gap-4 mb-4">
            <div :class="[zone.color, 'text-white px-4 py-1.5 rounded-r-full font-bold text-sm tracking-widest shadow-md -ml-4 md:-ml-8 uppercase']">{{ zone.name }}</div>
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
            <h3 class="text-lg font-bold text-gray-800 flex items-center gap-2">
              <AdjustmentsHorizontalIcon class="w-5 h-5"/> {{ configMode === 'add' ? 'Xây dựng Kệ hàng' : 'Cập nhật Kệ hàng' }}
            </h3>
            <button @click="closeConfigModal" class="text-gray-400 hover:text-red-500"><XMarkIcon class="w-6 h-6"/></button>
          </div>
          <div class="p-5">
            <form @submit.prevent="handleSaveConfig" class="space-y-4">
              <div>
                <label class="block text-xs font-bold mb-1">Thuộc Dãy (Khu vực) <span class="text-red-500">*</span></label>
                <input v-model="configForm.zoneName" :disabled="configMode === 'edit'" required type="text" class="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm uppercase disabled:bg-gray-100" placeholder="VD: DAY A">
              </div>
              <div>
                <label class="block text-xs font-bold mb-1">Mã Kệ (Không dấu) <span class="text-red-500">*</span></label>
                <input v-model="configForm.rackName" required type="text" class="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm uppercase" placeholder="VD: KE A-01">
              </div>
              
              <div class="grid grid-cols-2 gap-4">
                <div>
                  <label class="block text-xs font-bold mb-1">Số Tầng (Dọc) <span class="text-red-500">*</span></label>
                  <input v-model.number="configForm.tiers" required type="number" min="1" max="10" class="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm" placeholder="VD: 4">
                </div>
                <div>
                  <label class="block text-xs font-bold mb-1">Số Ô / Tầng (Ngang) <span class="text-red-500">*</span></label>
                  <input v-model.number="configForm.binsPerTier" required type="number" min="1" max="10" class="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm" placeholder="VD: 2">
                </div>
              </div>

              <div class="mt-6 pt-4 border-t flex justify-end gap-3">
                <button type="button" @click="closeConfigModal" class="px-4 py-2 border border-gray-300 rounded-lg text-sm hover:bg-gray-50 font-semibold">Hủy</button>
                <button type="submit" class="px-4 py-2 bg-primary-600 text-white rounded-lg text-sm font-bold hover:bg-primary-700 shadow-sm flex items-center gap-2">
                  <Squares2X2Icon v-if="configMode === 'add'" class="w-4 h-4"/> 
                  <PencilSquareIcon v-else class="w-4 h-4"/> 
                  {{ configMode === 'add' ? 'Xây Kệ' : 'Lưu Thay Đổi' }}
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
              <ArchiveBoxArrowDownIcon class="w-5 h-5"/> Quản lý Hàng trong Ô: {{ assignForm.locationCode }}
            </h3>
            <button @click="closeAssignModal" class="text-gray-400 hover:text-red-500"><XMarkIcon class="w-6 h-6"/></button>
          </div>
          <div class="p-5 space-y-6">
            
            <div>
              <h4 class="text-sm font-bold text-gray-700 border-b pb-2 mb-3">1. Hàng hóa đang chứa trên kệ</h4>
              <div v-if="assignForm.currentItems.length === 0" class="text-sm text-gray-400 italic bg-gray-50 p-3 rounded border border-dashed text-center">
                Ô này hiện đang trống.
              </div>
              <ul v-else class="space-y-2 max-h-40 overflow-y-auto custom-scrollbar pr-2">
                <li v-for="(item, idx) in assignForm.currentItems" :key="idx" class="flex justify-between items-center bg-amber-50 border border-amber-200 p-2 rounded">
                  <span class="text-sm font-semibold text-gray-800 flex items-center gap-2"><CubeIcon class="w-4 h-4 text-amber-600"/> {{ item.name }}</span>
                  <button @click="handleRemoveItem(item.variantId)" class="text-xs bg-white text-red-500 border border-red-200 px-2 py-1 rounded hover:bg-red-50 font-bold transition-colors">Lấy ra</button>
                </li>
              </ul>
            </div>

            <div class="bg-slate-50 p-4 rounded-lg border border-slate-200">
              <h4 class="text-sm font-bold text-primary-700 mb-3">2. Cất thêm hàng vào ô này</h4>
              <form @submit.prevent="handleAssignProduct" class="flex gap-2">
                <select v-model="assignForm.variantId" required class="flex-1 border border-gray-300 rounded-lg px-3 py-2 text-sm focus:ring-primary-500 bg-white">
                  <option value="" disabled>-- Chọn sản phẩm để cất vào --</option>
                  <option v-for="p in productsList" :key="p.id" :value="p.id">[{{ p.sku }}] - {{ p.name }}</option>
                </select>
                <button type="submit" :disabled="productsList.length === 0" class="px-4 py-2 bg-blue-600 text-white rounded-lg text-sm font-bold hover:bg-blue-700 shadow-sm shrink-0">
                  Cất vào
                </button>
              </form>
            </div>

          </div>
        </div>
      </div>
    </Teleport>
  </div>
</template>

<style scoped>
.bg-grid-pattern {
  background-size: 30px 30px;
  background-image: linear-gradient(to right, #f1f5f9 1px, transparent 1px), linear-gradient(to bottom, #f1f5f9 1px, transparent 1px);
}
.custom-scrollbar::-webkit-scrollbar { height: 6px; width: 6px; }
.custom-scrollbar::-webkit-scrollbar-track { background: transparent; }
.custom-scrollbar::-webkit-scrollbar-thumb { background: #cbd5e1; border-radius: 10px; }
.custom-scrollbar::-webkit-scrollbar-thumb:hover { background: #94a3b8; }
</style>