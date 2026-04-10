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
const canEdit = computed(() => ['admin', 'giam_doc', 'ql_kho', 'gd_chi_nhanh'].includes(currentUserRole.value?.toLowerCase()))

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

const allLocsInDB = ref([]) 

const fetchData = async () => {
  isLoading.value = true
  try {
    const headers = getAuthHeaders()
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

    const [zoneRes, rackRes, prodRes, locRes] = await Promise.all([
      fetch(`${API_URL}/zones`, { headers }), fetch(`${API_URL}/racks`, { headers }),
      fetch(PROD_API_URL, { headers }), fetch(LOC_API_URL, { headers })
    ])
    
    if (prodRes.ok) productsList.value = await prodRes.json()
    let zonesData = [], racksData = []
    if (zoneRes.ok) zonesData = await zoneRes.json()
    if (rackRes.ok) racksData = await rackRes.json()
    if (locRes.ok) {
        allLocsInDB.value = await locRes.json() 
    }

    const filteredZones = zonesData.filter(z => !myWarehouseId.value || z.warehouseId === myWarehouseId.value || z.WarehouseId === myWarehouseId.value)

    warehouseZones.value = filteredZones.map((zone, index) => {
      const zoneRacks = racksData.filter(r => r.zoneId === zone.id || r.ZoneId === zone.id).map(rack => {
        const rackCode = rack.code || rack.Code;

        // Tự động đo đạc số Ô/Tầng từ Database
        const rackLocs = allLocsInDB.value.filter(l => (l.code || l.Code).startsWith(`${rackCode}-T`));
        
        let actualTiers = 0;
        let actualBins = 0;

        rackLocs.forEach(l => {
            const code = l.code || l.Code;
            const match = code.match(/-T(\d+)-O(\d+)$/);
            if (match) {
                const t = parseInt(match[1]);
                const b = parseInt(match[2]);
                if (t > actualTiers) actualTiers = t;
                if (b > actualBins) actualBins = b;
            }
        });

        const tiers = actualTiers > 0 ? actualTiers : (rack.tiers || rack.Tiers || 4);
        const binsPerTier = actualBins > 0 ? actualBins : (rack.binsPerTier || rack.BinsPerTier || 2);

        const bins = [];
        for (let t = tiers; t >= 1; t--) {
          for (let b = 1; b <= binsPerTier; b++) {
            const binCode = `${rackCode}-T${t}-O${b}`
            const locDb = rackLocs.find(l => (l.code || l.Code) === binCode)
            bins.push({ 
                code: binCode, tier: t, bin: b, 
                status: locDb ? (locDb.status || locDb.Status || 'empty') : 'empty', 
                variantIds: locDb ? (locDb.variantIds || locDb.VariantIds || []) : [] 
            })
          }
        }
        return { id: rack.id || rack.Id, name: rackCode, tiers: tiers, binsPerTier: binsPerTier, bins: bins }
      })
      return { id: zone.id || zone.Id, name: zone.code || zone.Code, color: zoneColors[index % zoneColors.length], racks: zoneRacks }
    })
  } catch (error) { console.error("Lỗi vẽ sơ đồ:", error) } finally { isLoading.value = false }
}

const showConfigModal = ref(false); const configMode = ref('add'); const configForm = ref({ zoneName: '', rackName: '', tiers: null, binsPerTier: null }); const editingRackId = ref(null)

const openConfigModal = () => { 
  configMode.value = 'add'; editingRackId.value = null;
  configForm.value = { zoneName: '', rackName: '', tiers: null, binsPerTier: null }; 
  showConfigModal.value = true 
}

const openEditModal = (zone, rack) => { configMode.value = 'edit'; editingRackId.value = rack.id; configForm.value = { zoneName: zone.name, rackName: rack.name, tiers: rack.tiers, binsPerTier: rack.binsPerTier }; showConfigModal.value = true }
const closeConfigModal = () => showConfigModal.value = false

const handleSaveConfig = async () => {
  if (!myWarehouseId.value) return alert("Vui lòng chọn kho trước khi xây kệ!");
  try {
    let currentZoneId = null;
    let existingZone = warehouseZones.value.find(z => z.name.toUpperCase() === configForm.value.zoneName.toUpperCase());
    
    if (existingZone) {
        currentZoneId = existingZone.id;
    } else {
        const resZone = await fetch(`${API_URL}/zones`, { method: 'POST', headers: getAuthHeaders(), body: JSON.stringify({ code: configForm.value.zoneName.toUpperCase(), warehouseId: myWarehouseId.value }) });
        if (resZone.ok) {
            currentZoneId = (await resZone.json()).id;
        } else {
            return alert("Lỗi: Không thể tạo được Dãy mới!");
        }
    }

    const payload = { 
        id: editingRackId.value || 0,
        zoneId: currentZoneId,
        code: configForm.value.rackName.toUpperCase(), 
        tiers: configForm.value.tiers, 
        binsPerTier: configForm.value.binsPerTier 
    };

    if (configMode.value === 'add') {
      const res = await fetch(`${API_URL}/racks`, { method: 'POST', headers: getAuthHeaders(), body: JSON.stringify(payload) });
      if (res.ok) {
          await fetchData(); closeConfigModal(); alert('Xây kệ thành công!');
      } else {
          alert('Lỗi xây kệ: ' + await res.text());
      }
    } else {
      const res = await fetch(`${API_URL}/racks/${editingRackId.value}`, { method: 'PUT', headers: getAuthHeaders(), body: JSON.stringify(payload) });
      if (res.ok) {
          // BỎ GỌI HÀM CLEANUP Ở ĐÂY VÌ BACKEND ĐÃ LO RỒI
          await fetchData(); closeConfigModal(); alert('Cập nhật kệ thành công!');
      } else {
          alert(`LỖI CẬP NHẬT (400): ${await res.text()}`);
      }
    }
  } catch (error) { console.error(error) }
}

const deleteRack = async (zone, rack) => {
  if (confirm(`Sếp có chắc muốn đập bỏ kệ [${rack.name}]? Toàn bộ vị trí trên kệ này sẽ biến mất.`)) {
    // BỎ GỌI HÀM CLEANUP Ở ĐÂY LUÔN
    await fetch(`${API_URL}/racks/${rack.id}`, { method: 'DELETE', headers: getAuthHeaders() }); 
    await fetchData();
  }
}

const showAssignModal = ref(false)
const assignForm = ref({ locationCode: '', currentItems: [] })

const openAssignModal = (bin) => {
  assignForm.value.locationCode = bin.code
  assignForm.value.currentItems = bin.variantIds.map(vId => {
    const p = productsList.value.find(x => x.id === vId || x.Id === vId);
    return { variantId: vId, name: p ? (p.name || p.Name) : 'Sản phẩm ẩn' }
  })
  showAssignModal.value = true
}
const closeAssignModal = () => showAssignModal.value = false

const handleRemoveItem = async (variantId) => {
  if(!confirm("Lấy sản phẩm này ra khỏi kệ và đưa về Bãi tập kết?")) return;
  try {
    const res = await fetch(`${LOC_API_URL}/remove-stock-item/${assignForm.value.locationCode}/${variantId}`, {
      method: 'DELETE', headers: getAuthHeaders()
    })
    if (res.ok) {
      await fetchData(); closeAssignModal();
    }
  } catch(e) { console.error(e) }
}

const getBinColor = (status) => {
  switch(status) {
    case 'empty': return 'bg-gray-100 border-gray-300 text-gray-400 hover:bg-gray-200 cursor-pointer'
    case 'partial': return 'bg-amber-100 border-amber-400 text-amber-700 hover:bg-amber-200 cursor-pointer shadow-sm'
    case 'full': return 'bg-red-500 border-red-600 text-white shadow-md hover:bg-red-600 cursor-pointer'
    default: return 'bg-gray-100 border-gray-300'
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
      <div v-if="isLoading" class="text-center w-full text-gray-500 font-medium">Đang vẽ sơ đồ kho...</div>
      <div v-else-if="warehouseZones.length === 0" class="text-center p-6 bg-white/80 rounded-xl backdrop-blur-sm border border-gray-200 shadow-sm m-auto">
        <Squares2X2Icon class="w-16 h-16 text-slate-300 mx-auto mb-4" />
        <h3 class="text-lg font-bold text-slate-700">Kho này đang trống kệ</h3>
        <p class="text-sm text-gray-500 mt-1">Sếp hãy bấm "Xây Kệ Mới" để thiết lập sơ đồ dãy kệ nhé.</p>
      </div>

      <div v-else class="w-full">
        <div v-for="zone in warehouseZones" :key="zone.id" class="mb-12 last:mb-0">
          <div class="flex items-center gap-4 mb-6">
            <div :class="[zone.color, 'text-white px-5 py-2 rounded-r-full font-bold text-sm tracking-widest shadow-md uppercase']">{{ zone.name }}</div>
            <div class="flex-1 border-t-2 border-dashed border-slate-300"></div>
          </div>
          
          <div class="flex flex-nowrap gap-8 pb-4 overflow-x-visible">
            <div v-for="rack in zone.racks" :key="rack.id" class="flex flex-col bg-white border-4 border-slate-700 rounded-t-xl shadow-xl shrink-0">
              <div class="bg-slate-700 text-white py-2 px-3 relative group flex items-center justify-center min-h-[36px]">
                <span class="font-black text-xs tracking-widest uppercase">{{ rack.name }}</span>
                <div v-if="canEdit" class="absolute right-1 top-1 hidden group-hover:flex gap-1 bg-slate-700 pl-2">
                  <button @click="openEditModal(zone, rack)" class="p-1 hover:bg-blue-600 rounded text-blue-300 hover:text-white"><PencilSquareIcon class="w-4 h-4"/></button>
                  <button @click="deleteRack(zone, rack)" class="p-1 hover:bg-red-600 rounded text-red-300 hover:text-white"><TrashIcon class="w-4 h-4"/></button>
                </div>
              </div>

              <div class="flex p-4 bg-slate-50 relative">
                <div class="flex flex-col justify-between pr-4 border-r-2 border-slate-300 py-2 mr-4 font-black text-slate-400 text-[10px] uppercase">
                  <span v-for="t in rack.tiers" :key="t">Tầng {{ rack.tiers - t + 1 }}</span>
                </div>
                <div class="grid gap-3" :style="{ gridTemplateColumns: `repeat(${rack.binsPerTier}, minmax(0, 1fr))` }">
                  <div v-for="bin in rack.bins" :key="bin.code" 
                       @click="openAssignModal(bin)" 
                       :class="[getBinColor(bin.status), 'w-14 h-12 md:w-20 md:h-16 border-2 rounded-lg flex flex-col items-center justify-center transition-all hover:scale-105 active:scale-95']">
                    <span class="text-[10px] md:text-xs font-bold uppercase tracking-tighter">Ô {{ bin.bin }}</span>
                    <span v-if="bin.variantIds.length > 0" class="text-[9px] font-black mt-1 bg-black/10 px-1.5 py-0.5 rounded-full">
                        {{ bin.variantIds.length }} món
                    </span>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>

    <div v-if="viewMode === 'table'" class="bg-white rounded-xl border border-gray-200 shadow-sm overflow-hidden mt-4 animate-fade-in">
      <div class="bg-gray-50 px-5 py-4 border-b border-gray-200 flex items-center justify-between">
        <span class="text-sm font-bold text-gray-600 uppercase tracking-wide">Danh sách chi tiết <strong class="text-primary-600">{{ allBinsList.length }}</strong> vị trí lưu kho</span>
      </div>
      <div class="w-full overflow-x-auto">
        <table class="min-w-[950px] w-full divide-y divide-gray-200 text-sm">
          <thead class="bg-gray-50">
            <tr>
              <th class="px-6 py-4 text-left font-bold text-gray-500 uppercase tracking-wider">Mã Vị Trí (Bin Code)</th>
              <th class="px-6 py-4 text-left font-bold text-gray-500 uppercase tracking-wider">Dãy - Kệ</th>
              <th class="px-6 py-4 text-center font-bold text-gray-500 uppercase tracking-wider">Tọa độ</th>
              <th class="px-6 py-4 text-center font-bold text-gray-500 uppercase tracking-wider">Tình trạng</th>
            </tr>
          </thead>
          <tbody class="divide-y divide-gray-100 bg-white">
            <tr v-for="bin in allBinsList" :key="bin.code" class="hover:bg-gray-50">
              <td class="px-6 py-4 font-black text-primary-700 flex items-center gap-3"><MapPinIcon class="w-5 h-5 text-gray-400"/> {{ bin.code }}</td>
              <td class="px-6 py-4 font-bold text-gray-600 uppercase">{{ bin.zoneName }} / {{ bin.rackName }}</td>
              <td class="px-6 py-4 text-center">
                <span class="bg-slate-100 text-slate-600 px-3 py-1 rounded font-bold text-xs uppercase">Tầng {{ bin.tier }} • Ô {{ bin.bin }}</span>
              </td>
              <td class="px-6 py-4 text-center">
                <span v-if="bin.status === 'empty'" class="bg-gray-100 text-gray-500 px-3 py-1 rounded-full text-[10px] font-black border uppercase">TRỐNG</span>
                <span v-else class="bg-amber-100 text-amber-700 border-amber-200 px-3 py-1 rounded-full text-[10px] font-black border uppercase">ĐANG CHỨA {{ bin.variantIds.length }} MẶT HÀNG</span>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>

    <Teleport to="body">
      <div v-if="showConfigModal" class="fixed inset-0 z-50 flex items-center justify-center px-4 bg-slate-900/60 backdrop-blur-sm animate-fade-in">
        <div class="bg-white rounded-2xl shadow-2xl w-full max-w-md overflow-hidden">
          <div class="px-6 py-5 border-b flex justify-between bg-gray-50">
            <h3 class="text-xl font-black text-gray-800 flex gap-2"><AdjustmentsHorizontalIcon class="w-6 h-6 text-primary-600"/> {{ configMode === 'add' ? 'Xây Kệ Mới' : 'Cập Nhật Kệ' }}</h3>
            <button @click="closeConfigModal" class="text-gray-400 hover:text-red-500 transition-colors"><XMarkIcon class="w-7 h-7"/></button>
          </div>
          <div class="p-6">
            <form @submit.prevent="handleSaveConfig" class="space-y-5">
              <div><label class="block text-xs font-black uppercase text-gray-500 mb-2">Thuộc Dãy (VD: DAY A) *</label><input v-model="configForm.zoneName" :disabled="configMode === 'edit'" required type="text" class="w-full border-2 border-gray-200 rounded-xl px-4 py-3 text-sm font-bold uppercase focus:border-primary-500 outline-none disabled:bg-gray-100" placeholder="NHẬP TÊN DÃY..."></div>
              <div><label class="block text-xs font-black uppercase text-gray-500 mb-2">Mã Kệ (VD: KE-01) *</label><input v-model="configForm.rackName" required type="text" class="w-full border-2 border-gray-200 rounded-xl px-4 py-3 text-sm font-bold uppercase focus:border-primary-500 outline-none" placeholder="NHẬP MÃ KỆ..."></div>
              <div class="grid grid-cols-2 gap-4">
                <div><label class="block text-xs font-black uppercase text-gray-500 mb-2">Số Tầng *</label><input v-model.number="configForm.tiers" required type="number" min="1" max="10" class="w-full border-2 border-gray-200 rounded-xl px-4 py-3 text-sm font-bold focus:border-primary-500 outline-none"></div>
                <div><label class="block text-xs font-black uppercase text-gray-500 mb-2">Số Ô / Tầng *</label><input v-model.number="configForm.binsPerTier" required type="number" min="1" max="10" class="w-full border-2 border-gray-200 rounded-xl px-4 py-3 text-sm font-bold focus:border-primary-500 outline-none"></div>
              </div>
              <div class="mt-8 pt-4 flex gap-3">
                <button type="button" @click="closeConfigModal" class="flex-1 px-4 py-3 border-2 border-gray-200 rounded-xl text-sm font-black uppercase hover:bg-gray-50 transition-colors">Hủy</button>
                <button type="submit" class="flex-[2] px-4 py-3 bg-primary-600 text-white rounded-xl text-sm font-black uppercase shadow-lg hover:bg-primary-700 transition-all">Lưu Thiết Lập</button>
              </div>
            </form>
          </div>
        </div>
      </div>
    </Teleport>

    <Teleport to="body">
      <div v-if="showAssignModal" class="fixed inset-0 z-50 flex items-center justify-center px-4 bg-slate-900/60 backdrop-blur-sm animate-fade-in">
        <div class="bg-white rounded-2xl shadow-2xl w-full max-w-lg overflow-hidden">
          <div class="px-6 py-5 border-b flex justify-between bg-blue-600 text-white">
            <h3 class="text-lg font-black flex items-center gap-3 uppercase tracking-tighter"><ArchiveBoxArrowDownIcon class="w-6 h-6"/> Ô: {{ assignForm.locationCode }}</h3>
            <button @click="closeAssignModal" class="text-white/80 hover:text-white"><XMarkIcon class="w-7 h-7"/></button>
          </div>
          <div class="p-6">
            <div v-if="assignForm.currentItems.length === 0" class="text-center py-10">
              <Squares2X2Icon class="w-16 h-16 text-gray-200 mx-auto mb-4" />
              <p class="text-gray-400 font-bold italic uppercase text-sm">Vị trí này đang để trống</p>
            </div>
            <div v-else>
              <label class="block text-[10px] font-black text-gray-400 uppercase mb-4 tracking-widest">Sản phẩm đang lưu trữ:</label>
              <div class="space-y-3 max-h-80 overflow-y-auto custom-scrollbar pr-2">
                <div v-for="(item, idx) in assignForm.currentItems" :key="idx" class="flex justify-between items-center bg-slate-50 border-2 border-slate-100 p-3 rounded-xl hover:border-blue-200 transition-all">
                  <span class="text-sm font-black text-gray-800 flex items-center gap-3"><CubeIcon class="w-5 h-5 text-blue-500"/> {{ item.name }}</span>
                  <button @click="handleRemoveItem(item.variantId)" class="px-4 py-2 bg-white text-red-600 border-2 border-red-100 rounded-lg text-[10px] font-black uppercase hover:bg-red-50 transition-all shadow-sm">Lấy Ra Bãi</button>
                </div>
              </div>
              <p class="text-[10px] text-gray-500 mt-6 text-center font-bold italic">* Lưu ý: Thao tác "Lấy Ra Bãi" sẽ chuyển hàng về khu vực chờ nhập.</p>
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
@keyframes fadeIn { from { opacity: 0; transform: translateY(10px); } to { opacity: 1; transform: translateY(0); } }
</style>