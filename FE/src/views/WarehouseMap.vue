<script setup>
import { ref, onMounted, computed } from 'vue'
import { 
  MagnifyingGlassIcon, PlusIcon, Squares2X2Icon, MapIcon, 
  TableCellsIcon, InformationCircleIcon, AdjustmentsHorizontalIcon, 
  XMarkIcon, PencilSquareIcon, TrashIcon 
} from '@heroicons/vue/24/outline'

const viewMode = ref('map') 

// === 1. STATE CHÍNH: DỮ LIỆU KHO ===
const warehouseZones = ref([])

// === 2. STATE LOGIC TẠO / SỬA SƠ ĐỒ KHO ===
const showConfigModal = ref(false)
const configMode = ref('add') // 'add' (Xây mới) hoặc 'edit' (Sửa)
const editingRackId = ref(null) // Lưu ID kệ đang sửa

const configForm = ref({
  zoneName: 'Dãy A',
  rackName: 'Kệ A-01',
  tiers: 4,
  binsPerTier: 2
})

const zoneColors = ['bg-blue-600', 'bg-emerald-600', 'bg-amber-600', 'bg-purple-600', 'bg-rose-600']

// NÚT XÂY KỆ MỚI
const openConfigModal = () => {
  configMode.value = 'add'
  editingRackId.value = null
  configForm.value = { zoneName: 'Dãy A', rackName: `Kệ A-0${warehouseZones.value.length + 1}`, tiers: 4, binsPerTier: 2 }
  showConfigModal.value = true
}

// NÚT SỬA KỆ ĐÃ CÓ
const openEditModal = (zone, rack) => {
  configMode.value = 'edit'
  editingRackId.value = rack.id
  configForm.value = {
    zoneName: zone.name,
    rackName: rack.name,
    tiers: rack.tiers,
    binsPerTier: rack.binsPerTier
  }
  showConfigModal.value = true
}

const closeConfigModal = () => showConfigModal.value = false

// HÀM LƯU (XỬ LÝ CẢ XÂY MỚI LẪN CẬP NHẬT)
const handleSaveConfig = () => {
  if (configMode.value === 'add') {
    // --- THÊM MỚI ---
    let zone = warehouseZones.value.find(z => z.name.toUpperCase() === configForm.value.zoneName.toUpperCase())
    if (!zone) {
      zone = { id: 'ZONE-' + Date.now(), name: configForm.value.zoneName.toUpperCase(), color: zoneColors[warehouseZones.value.length % zoneColors.length], racks: [] }
      warehouseZones.value.push(zone)
    }

    const newRack = {
      id: 'RACK-' + Date.now(), name: configForm.value.rackName,
      tiers: configForm.value.tiers, binsPerTier: configForm.value.binsPerTier, bins: []
    }

    for (let t = configForm.value.tiers; t >= 1; t--) {
      for (let b = 1; b <= configForm.value.binsPerTier; b++) {
        newRack.bins.push({ code: `${configForm.value.rackName}-T${t}-O${b}`, tier: t, bin: b, status: 'empty', maxCapacity: 50, items: [] })
      }
    }
    zone.racks.push(newRack)
    alert(`Đã xây xong ${configForm.value.rackName} thành công!`)

  } else {
    // --- CHỈNH SỬA ---
    let found = false;
    for (let zone of warehouseZones.value) {
      const rackIndex = zone.racks.findIndex(r => r.id === editingRackId.value)
      if (rackIndex !== -1) {
        found = true;
        const rack = zone.racks[rackIndex]
        
        // Cập nhật thông tin
        rack.name = configForm.value.rackName
        
        // NẾU thay đổi số Tầng hoặc số Ô thì phải vẽ lại Kệ
        if (rack.tiers !== configForm.value.tiers || rack.binsPerTier !== configForm.value.binsPerTier) {
           const confirmRebuild = confirm("Sếp thay đổi số Tầng / Số Ô, hệ thống sẽ đập kệ này đi xây lại. Chắc chắn chứ?")
           if (!confirmRebuild) return;
           
           rack.tiers = configForm.value.tiers
           rack.binsPerTier = configForm.value.binsPerTier
           rack.bins = []
           for (let t = rack.tiers; t >= 1; t--) {
            for (let b = 1; b <= rack.binsPerTier; b++) {
              rack.bins.push({ code: `${rack.name}-T${t}-O${b}`, tier: t, bin: b, status: 'empty', maxCapacity: 50, items: [] })
            }
          }
        }
        break;
      }
    }
    if (found) alert('Cập nhật cấu hình Kệ thành công!')
  }
  closeConfigModal()
}

// NÚT XÓA KỆ
const deleteRack = (zone, rack) => {
  if (confirm(`Sếp có chắc chắn muốn ĐẬP BỎ [${rack.name}] thuộc [${zone.name}] không? Toàn bộ ô kệ sẽ biến mất!`)) {
    // Xóa kệ khỏi dãy
    zone.racks = zone.racks.filter(r => r.id !== rack.id)
    
    // Nếu Dãy đó không còn cái kệ nào thì xóa luôn Dãy cho sạch
    if (zone.racks.length === 0) {
      warehouseZones.value = warehouseZones.value.filter(z => z.id !== zone.id)
    }
  }
}

// === 3. LOGIC TOOLTIP ===
const hoveredBin = ref(null)
const tooltipPosition = ref({ x: 0, y: 0 })

const showTooltip = (event, bin) => {
  hoveredBin.value = bin
  const xPos = event.clientX + 20 > window.innerWidth - 250 ? event.clientX - 260 : event.clientX + 15
  tooltipPosition.value = { x: xPos, y: event.clientY + 15 }
}
const hideTooltip = () => hoveredBin.value = null

const getBinColor = (status) => {
  switch(status) {
    case 'empty': return 'bg-gray-100 border-gray-300 text-gray-500 hover:bg-gray-200'
    case 'partial': return 'bg-yellow-100 border-yellow-400 text-yellow-700 hover:bg-yellow-200'
    case 'full': return 'bg-red-500 border-red-600 text-white shadow-md hover:bg-red-600'
    default: return 'bg-gray-100 border-gray-300 text-gray-500'
  }
}

// === 4. BẢNG DANH SÁCH ===
const allBinsList = computed(() => {
  let list = []
  warehouseZones.value.forEach(zone => {
    zone.racks.forEach(rack => {
      rack.bins.forEach(bin => {
        list.push({ ...bin, zoneName: zone.name, rackName: rack.name })
      })
    })
  })
  return list
})
</script>

<template>
  <div class="space-y-4 md:space-y-6 animate-fade-in pb-10 px-0 md:px-1">
    
    <div class="flex flex-col sm:flex-row sm:items-center justify-between gap-4">
      <div>
        <h2 class="text-xl md:text-2xl font-bold text-gray-800">Sơ đồ mặt đứng Kho hàng</h2>
        <p class="text-xs md:text-sm text-gray-500 mt-1">Trực quan hóa cấu trúc Dãy > Kệ > Tầng > Ô</p>
      </div>
      <div class="flex items-center gap-2">
        <div class="bg-gray-100 p-1 rounded-lg flex items-center hidden sm:flex">
          <button @click="viewMode = 'map'" :class="['px-3 py-1.5 text-sm font-medium rounded-md transition-all', viewMode === 'map' ? 'bg-white shadow text-primary-600' : 'text-gray-500 hover:text-gray-700']">
            <MapIcon class="w-5 h-5 inline-block mr-1" /> Trực quan
          </button>
          <button @click="viewMode = 'table'" :class="['px-3 py-1.5 text-sm font-medium rounded-md transition-all', viewMode === 'table' ? 'bg-white shadow text-primary-600' : 'text-gray-500 hover:text-gray-700']">
            <TableCellsIcon class="w-5 h-5 inline-block mr-1" /> Danh sách
          </button>
        </div>
        <button @click="openConfigModal" class="bg-slate-800 hover:bg-slate-900 text-white px-3 md:px-4 py-2 md:py-2.5 rounded-lg flex items-center gap-2 text-sm font-semibold shadow-sm transition-colors w-full sm:w-auto justify-center">
          <AdjustmentsHorizontalIcon class="w-5 h-5" /> Cấu hình Kệ
        </button>
      </div>
    </div>

    <div class="bg-white p-3 md:p-4 rounded-xl border border-gray-200 flex flex-col sm:flex-row items-center gap-3 md:gap-4 shadow-sm">
      <div class="relative w-full sm:flex-1">
        <div class="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none"><MagnifyingGlassIcon class="w-5 h-5 text-gray-400" /></div>
        <input type="text" class="block w-full pl-10 pr-3 py-2 border border-gray-200 rounded-lg text-sm focus:ring-1 focus:ring-primary-500 outline-none" placeholder="Tìm kiếm mã ô, mã sản phẩm đang chứa...">
      </div>
    </div>

    <div v-if="viewMode === 'map'" class="bg-white border-2 border-slate-200 rounded-xl p-4 md:p-8 overflow-x-auto shadow-sm min-h-[500px] relative bg-grid-pattern mt-4 flex" :class="warehouseZones.length === 0 ? 'items-center justify-center' : ''">
      
      <div v-if="warehouseZones.length === 0" class="text-center p-6 bg-white/80 rounded-xl backdrop-blur-sm border border-gray-200 shadow-sm">
        <Squares2X2Icon class="w-16 h-16 text-slate-300 mx-auto mb-4" />
        <h3 class="text-lg font-bold text-slate-700">Chưa thiết lập sơ đồ mặt bằng</h3>
        <p class="text-sm text-slate-500 mt-2 mb-4 max-w-sm mx-auto">Kho hiện tại đang trống. Sếp hãy bấm nút để tự xây dựng các Kệ hàng nhé!</p>
        <button @click="openConfigModal" class="bg-primary-600 hover:bg-primary-700 text-white px-5 py-2 rounded-lg text-sm font-semibold shadow-sm flex items-center gap-2 mx-auto">
          <PlusIcon class="w-5 h-5"/> Xây Kệ mới
        </button>
      </div>

      <div v-else class="w-full">
        <div v-for="zone in warehouseZones" :key="zone.id" class="mb-12 last:mb-0">
          <div class="flex items-center gap-4 mb-4">
            <div :class="[zone.color, 'text-white px-4 py-1.5 rounded-r-full font-bold text-sm tracking-widest shadow-md -ml-4 md:-ml-8']">{{ zone.name }}</div>
            <div class="flex-1 border-t-2 border-dashed border-slate-300"></div>
          </div>

          <div class="flex flex-nowrap gap-6 pb-4">
            <div v-for="rack in zone.racks" :key="rack.id" class="flex flex-col bg-white border-4 border-slate-700 rounded-t-lg shadow-lg shrink-0">
              
              <div class="bg-slate-700 text-white py-1.5 px-2 relative group flex items-center justify-center min-h-[32px]">
                <span class="font-bold text-sm tracking-wider">{{ rack.name }}</span>
                <div class="absolute right-1 top-0.5 hidden group-hover:flex gap-1 bg-slate-700 pl-2">
                  <button @click="openEditModal(zone, rack)" class="p-1 hover:bg-slate-500 rounded text-blue-200 hover:text-white transition-colors" title="Sửa tên / Kích thước kệ"><PencilSquareIcon class="w-4 h-4"/></button>
                  <button @click="deleteRack(zone, rack)" class="p-1 hover:bg-red-600 rounded text-red-300 hover:text-white transition-colors" title="Đập bỏ kệ này"><TrashIcon class="w-4 h-4"/></button>
                </div>
              </div>

              <div class="flex p-3 bg-slate-50 relative">
                <div class="flex flex-col justify-between pr-3 border-r-2 border-slate-300 py-2 mr-3 font-bold text-slate-400 text-[10px] md:text-xs">
                  <span v-for="t in rack.tiers" :key="t">TẦNG {{ rack.tiers - t + 1 }}</span>
                </div>
                <div class="grid gap-2" :style="{ gridTemplateColumns: `repeat(${rack.binsPerTier}, minmax(0, 1fr))` }">
                  <div v-for="bin in rack.bins" :key="bin.code" @mousemove="showTooltip($event, bin)" @mouseleave="hideTooltip" :class="[getBinColor(bin.status), 'w-12 h-10 md:w-16 md:h-12 border-2 rounded flex flex-col items-center justify-center cursor-pointer transition-all hover:-translate-y-0.5 shadow-sm']">
                    <span class="text-[10px] md:text-xs font-bold opacity-80">Ô {{ bin.bin }}</span>
                  </div>
                </div>
              </div>
              
              <div class="flex justify-between px-2 h-4 bg-transparent border-t-4 border-slate-700">
                <div class="w-3 h-4 bg-slate-700"></div><div class="w-3 h-4 bg-slate-700"></div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>

    <div v-if="viewMode === 'table'" class="bg-white rounded-xl border border-gray-200 shadow-sm overflow-hidden mt-4 animate-fade-in">
      <div class="bg-gray-50 px-4 py-3 border-b border-gray-200 flex items-center justify-between">
        <span class="text-sm font-medium text-gray-600">Tổng số: <strong class="text-primary-600">{{ allBinsList.length }}</strong> vị trí</span>
      </div>
      <div class="w-full overflow-x-auto">
        <table class="min-w-[950px] w-full divide-y divide-gray-200">
          <thead class="bg-gray-50">
            <tr>
              <th class="px-5 py-3.5 text-left text-xs font-bold text-gray-500 uppercase">Mã Vị trí (Bin)</th>
              <th class="px-5 py-3.5 text-left text-xs font-bold text-gray-500 uppercase">Thuộc Dãy / Kệ</th>
              <th class="px-5 py-3.5 text-center text-xs font-bold text-gray-500 uppercase">Tầng</th>
              <th class="px-5 py-3.5 text-center text-xs font-bold text-gray-500 uppercase">Ô số</th>
              <th class="px-5 py-3.5 text-left text-xs font-bold text-gray-500 uppercase">Trạng thái</th>
            </tr>
          </thead>
          <tbody class="divide-y divide-gray-100 bg-white">
            <tr v-if="allBinsList.length === 0">
              <td colspan="5" class="px-6 py-16 text-center text-sm text-gray-500">Chưa có dữ liệu. Vui lòng quay lại tab Trực quan để xây Kệ hàng.</td>
            </tr>
            <tr v-for="bin in allBinsList" :key="bin.code" class="hover:bg-gray-50">
              <td class="px-5 py-3 text-sm font-bold text-primary-600">{{ bin.code }}</td>
              <td class="px-5 py-3 text-sm text-gray-600">{{ bin.zoneName }} - {{ bin.rackName }}</td>
              <td class="px-5 py-3 text-sm text-center text-gray-600">Tầng {{ bin.tier }}</td>
              <td class="px-5 py-3 text-sm text-center text-gray-600">Ô {{ bin.bin }}</td>
              <td class="px-5 py-3">
                <span v-if="bin.status === 'empty'" class="bg-gray-100 text-gray-600 px-2.5 py-1 rounded text-xs font-medium border">Trống</span>
                <span v-else class="bg-yellow-100 text-yellow-800 px-2.5 py-1 rounded text-xs font-medium border border-yellow-200">Có hàng</span>
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
                <input v-model="configForm.zoneName" :disabled="configMode === 'edit'" required type="text" class="w-full border rounded-lg px-3 py-2 text-sm focus:ring-primary-500 disabled:bg-gray-100" placeholder="VD: Dãy A">
              </div>
              <div>
                <label class="block text-xs font-bold mb-1">Tên Kệ <span class="text-red-500">*</span></label>
                <input v-model="configForm.rackName" required type="text" class="w-full border rounded-lg px-3 py-2 text-sm focus:ring-primary-500" placeholder="VD: Kệ A-01">
              </div>
              <div class="grid grid-cols-2 gap-4">
                <div>
                  <label class="block text-xs font-bold mb-1">Số Tầng (Chiều dọc) <span class="text-red-500">*</span></label>
                  <input v-model.number="configForm.tiers" required type="number" min="1" max="10" class="w-full border rounded-lg px-3 py-2 text-sm focus:ring-primary-500">
                </div>
                <div>
                  <label class="block text-xs font-bold mb-1">Số Ô / Tầng (Ngang) <span class="text-red-500">*</span></label>
                  <input v-model.number="configForm.binsPerTier" required type="number" min="1" max="10" class="w-full border rounded-lg px-3 py-2 text-sm focus:ring-primary-500">
                </div>
              </div>
              <div class="mt-4 flex justify-end gap-3 pt-2">
                <button type="button" @click="closeConfigModal" class="px-4 py-2 border rounded-lg text-sm hover:bg-gray-50">Hủy</button>
                <button type="submit" class="px-4 py-2 bg-slate-800 text-white rounded-lg text-sm font-semibold hover:bg-slate-900 shadow-sm flex items-center gap-2">
                  <Squares2X2Icon v-if="configMode === 'add'" class="w-4 h-4"/> 
                  <PencilSquareIcon v-else class="w-4 h-4"/> 
                  {{ configMode === 'add' ? 'Xây Kệ' : 'Lưu cập nhật' }}
                </button>
              </div>
            </form>
          </div>
        </div>
      </div>
    </Teleport>

    <Teleport to="body">
      <div v-if="hoveredBin" class="fixed z-50 bg-slate-900 text-white p-3 rounded-xl shadow-2xl pointer-events-none w-48" :style="{ top: `${tooltipPosition.y}px`, left: `${tooltipPosition.x}px` }">
        <div class="font-bold text-sm border-b border-slate-700 pb-2 mb-2 flex items-center justify-between">
          <span class="text-blue-300">{{ hoveredBin.code }}</span>
          <span class="text-[10px] bg-slate-700 px-2 py-0.5 rounded text-gray-300">TRỐNG</span>
        </div>
        <div class="text-xs text-slate-400 italic text-center py-1">Ô này chưa có hàng hóa.</div>
      </div>
    </Teleport>

  </div>
</template>

<style scoped>
.bg-grid-pattern {
  background-size: 30px 30px;
  background-image: linear-gradient(to right, #f1f5f9 1px, transparent 1px), linear-gradient(to bottom, #f1f5f9 1px, transparent 1px);
}
</style>