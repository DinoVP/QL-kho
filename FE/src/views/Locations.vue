<script setup>
import { ref, computed, onMounted } from 'vue'
import { MagnifyingGlassIcon, PlusIcon, MapPinIcon, XMarkIcon, QrCodeIcon, CubeIcon, ScaleIcon, PencilSquareIcon, TrashIcon, AdjustmentsHorizontalIcon } from '@heroicons/vue/24/outline'

const API_URL = 'https://localhost:7139/api/Locations'
const PROD_API_URL = 'https://localhost:7139/api/Products'

const getAuthHeaders = () => ({
  'Content-Type': 'application/json',
  'Authorization': 'Bearer ' + (localStorage.getItem('authToken') || '')
})

const locations = ref([])
const productsList = ref([])
const isLoading = ref(false)

const fetchData = async () => {
  isLoading.value = true
  try {
    const headers = getAuthHeaders()
    const [locRes, prodRes] = await Promise.all([
      fetch(API_URL, { headers }),
      fetch(PROD_API_URL, { headers })
    ])
    if (prodRes.ok) productsList.value = await prodRes.json()
    if (locRes.ok) locations.value = await locRes.json()
  } catch (error) { console.error('Lỗi tải dữ liệu:', error) }
  finally { isLoading.value = false }
}

const searchQuery = ref('')
const filterStatus = ref('')

const filteredLocations = computed(() => {
  return locations.value.filter(loc => {
    const matchSearch = (loc.code || '').toLowerCase().includes(searchQuery.value.toLowerCase())
    const matchStatus = filterStatus.value === '' || loc.status === filterStatus.value
    return matchSearch && matchStatus
  })
})

// === LOGIC TÍNH TOÁN CÂN NẶNG & THANH TIẾN ĐỘ ===
const calculateTotalWeight = (variantIds) => {
  if (!variantIds || variantIds.length === 0) return 0;
  let total = 0;
  variantIds.forEach(id => {
    const p = productsList.value.find(x => x.id === id);
    if (p && p.weight) total += p.weight;
  })
  return total; 
}

const getWeightPercent = (current, max) => {
  if (!max || max === 0) return 0;
  const pct = (current / max) * 100;
  return pct > 100 ? 100 : pct;
}

const getWeightColor = (current, max) => {
  const pct = (current / max) * 100;
  if (pct < 60) return 'bg-emerald-500';
  if (pct < 90) return 'bg-amber-500';
  return 'bg-red-600 shadow-[0_0_8px_rgba(220,38,38,0.7)]'; // Màu đỏ phát sáng khi quá tải
}

const getStatusBadge = (status) => {
  switch(status) {
    case 'empty': return { text: 'Trống', class: 'bg-gray-100 text-gray-700 border-gray-300' }
    case 'full': return { text: 'Có hàng', class: 'bg-indigo-100 text-indigo-700 border-indigo-300' }
    default: return { text: 'Khác', class: 'bg-gray-100 text-gray-500' }
  }
}

// === LOGIC MODAL SỬA VỊ TRÍ ===
const showModal = ref(false)
const formData = ref({ id: null, code: '', rack: '', maxWeight: 500 })

const openModal = (loc) => {
  // Gắn đúng dữ liệu của dòng đang chọn vào Form
  formData.value = { 
    id: loc.id, 
    code: loc.code, 
    rack: loc.rack, 
    maxWeight: loc.maxWeight || 500 
  }
  showModal.value = true
}
const closeModal = () => showModal.value = false

const handleSubmit = async () => {
  if (!formData.value.code) return;
  try {
    const res = await fetch(`${API_URL}/${formData.value.id}`, {
      method: 'PUT',
      headers: getAuthHeaders(),
      body: JSON.stringify({ 
        code: formData.value.code, 
        rack: formData.value.rack,
        maxWeight: formData.value.maxWeight 
      })
    })
    if (res.ok) {
      await fetchData();
      closeModal();
    } else {
      const err = await res.json()
      alert('Lỗi: ' + err.message)
    }
  } catch(e) { console.error(e) }
}

const handleDelete = async (id, code) => {
  if (confirm(`Bạn có chắc chắn muốn XÓA vị trí [${code}] không?`)) {
    try {
      const res = await fetch(`${API_URL}/${id}`, { method: 'DELETE', headers: getAuthHeaders() })
      if (res.ok) {
        await fetchData();
      } else {
        const err = await res.json()
        alert('KHÔNG THỂ XÓA: ' + err.message)
      }
    } catch(e) { console.error(e) }
  }
}

onMounted(() => { fetchData() })
</script>

<template>
  <div class="space-y-5 md:space-y-6 animate-fade-in pb-10 px-0 md:px-1 relative">
    <div class="flex flex-col sm:flex-row sm:items-center justify-between gap-4">
      <div>
        <h2 class="text-xl md:text-2xl font-bold text-gray-800">Chi tiết Vị trí (Bin Locations)</h2>
        <p class="text-xs md:text-sm text-gray-500 mt-1">Giám sát tải trọng thực tế của từng Vị trí Kệ hàng bằng <strong class="text-indigo-600">Thanh tiến độ (Progress)</strong></p>
      </div>
      <div class="flex gap-2">
        <button class="bg-white border border-gray-300 text-gray-700 px-4 py-2.5 rounded-lg text-sm font-semibold hover:bg-gray-50 transition-colors shadow-sm flex items-center gap-2">
          <QrCodeIcon class="w-5 h-5 text-indigo-600"/> In QR Code Kệ
        </button>
      </div>
    </div>

    <div class="bg-white p-3 md:p-4 rounded-xl border border-gray-200 flex flex-col md:flex-row items-center gap-3 md:gap-4 shadow-sm">
      <div class="relative w-full md:flex-1">
        <div class="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none"><MagnifyingGlassIcon class="w-5 h-5 text-gray-400" /></div>
        <input v-model="searchQuery" type="text" class="block w-full pl-10 pr-3 py-2 border border-gray-200 rounded-lg text-sm outline-none focus:ring-1 focus:ring-slate-500" placeholder="Tìm theo mã vị trí...">
      </div>
      <select v-model="filterStatus" class="w-full sm:w-auto border border-gray-200 rounded-lg text-sm px-4 py-2 outline-none cursor-pointer">
        <option value="">Tất cả Trạng thái</option><option value="empty">Trống</option><option value="full">Có hàng</option>
      </select>
    </div>

    <div class="bg-white rounded-xl border border-gray-200 shadow-sm overflow-hidden">
      <div class="w-full overflow-x-auto custom-scrollbar">
        <table class="min-w-[1050px] w-full divide-y divide-gray-200">
          <thead class="bg-gray-50">
            <tr>
              <th class="px-5 py-3.5 text-left text-xs font-bold text-gray-500 uppercase tracking-wider">Mã Vị Trí (Bin Code)</th>
              <th class="px-5 py-3.5 text-center text-xs font-bold text-gray-500 uppercase tracking-wider">Tọa độ chi tiết</th>
              <th class="px-5 py-3.5 text-center text-xs font-bold text-gray-500 uppercase tracking-wider">Tình trạng chứa hàng</th>
              <th class="px-5 py-3.5 text-left text-xs font-bold text-gray-500 uppercase tracking-wider w-64">TỔNG TẢI TRỌNG (Tiến độ)</th>
              <th class="px-5 py-3.5 text-center text-xs font-bold text-gray-500 uppercase tracking-wider">Trạng thái</th>
              <th class="px-5 py-3.5 text-right text-xs font-bold text-gray-500 uppercase tracking-wider">Thao tác</th>
            </tr>
          </thead>
          <tbody class="divide-y divide-gray-100 bg-white">
            <tr v-if="isLoading"><td colspan="6" class="px-6 py-12 text-center text-gray-500 font-medium">Đang tính toán tải trọng...</td></tr>
            <tr v-else-if="filteredLocations.length === 0">
              <td colspan="6" class="px-6 py-16 text-center">
                <MapPinIcon class="w-12 h-12 text-gray-300 mx-auto mb-3" />
                <h3 class="text-base font-semibold text-gray-700">Chưa có vị trí lưu kho nào</h3>
              </td>
            </tr>
            <tr v-for="loc in filteredLocations" :key="loc.id" class="hover:bg-gray-50">
              <td class="px-5 py-3">
                <div class="text-sm font-bold text-indigo-700 flex items-center gap-2"><MapPinIcon class="w-4 h-4 text-indigo-500" />{{ loc.code }}</div>
                <div class="text-[10px] text-gray-500 font-bold uppercase mt-1">{{ loc.zone }} - {{ loc.rack }}</div>
              </td>
              <td class="px-5 py-3 text-center">
                <div class="inline-flex gap-1">
                  <span class="bg-gray-100 text-gray-700 px-2 py-0.5 rounded text-xs font-bold border border-gray-200">Tầng {{ loc.tier }}</span>
                  <span class="bg-gray-100 text-gray-700 px-2 py-0.5 rounded text-xs font-bold border border-gray-200">Ô {{ loc.bin }}</span>
                </div>
              </td>
              <td class="px-5 py-3 text-center">
                <div class="flex items-center justify-center gap-1 font-bold">
                  <CubeIcon class="w-4 h-4 text-gray-400"/> 
                  <span :class="loc.variantIds.length > 0 ? 'text-amber-600' : 'text-gray-400'">{{ loc.variantIds.length }} Sản phẩm</span>
                </div>
              </td>
              
              <td class="px-5 py-3">
                <div class="flex flex-col w-full">
                  <div class="flex justify-between text-[10px] font-bold text-gray-500 mb-1">
                    <span :class="calculateTotalWeight(loc.variantIds) > loc.maxWeight ? 'text-red-600' : 'text-gray-700'">
                      Đang chứa: {{ calculateTotalWeight(loc.variantIds).toFixed(1) }} kg
                    </span>
                    <span>Max: {{ loc.maxWeight }} kg</span>
                  </div>
                  <div class="w-full bg-gray-200 rounded-full h-2.5 overflow-hidden border border-gray-300">
                    <div class="h-2.5 rounded-full transition-all duration-500"
                         :class="getWeightColor(calculateTotalWeight(loc.variantIds), loc.maxWeight)"
                         :style="{ width: getWeightPercent(calculateTotalWeight(loc.variantIds), loc.maxWeight) + '%' }">
                    </div>
                  </div>
                </div>
              </td>

              <td class="px-5 py-3 text-center"><span :class="['text-[10px] font-bold px-3 py-1 rounded-full border uppercase tracking-wider whitespace-nowrap', getStatusBadge(loc.status).class]">{{ getStatusBadge(loc.status).text }}</span></td>
              
              <td class="px-5 py-3 text-right space-x-2 whitespace-nowrap">
                <button @click="openModal(loc)" class="p-1.5 text-amber-600 hover:bg-amber-50 rounded border border-transparent hover:border-amber-200" title="Chỉnh sửa tải trọng / Tên mã"><PencilSquareIcon class="w-5 h-5" /></button>
                <button @click="handleDelete(loc.id, loc.code)" class="p-1.5 text-red-600 hover:bg-red-50 rounded border border-transparent hover:border-red-200" title="Đập bỏ ô này"><TrashIcon class="w-5 h-5" /></button>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>

    <Teleport to="body">
      <div v-if="showModal" class="fixed inset-0 z-50 flex items-center justify-center px-4 bg-slate-900/50 backdrop-blur-sm animate-fade-in">
        <div class="bg-white rounded-xl shadow-2xl w-full max-w-sm overflow-hidden">
          <div class="px-5 py-4 border-b flex justify-between bg-gray-50">
            <h3 class="text-lg font-bold text-gray-800 flex items-center gap-2">
              <AdjustmentsHorizontalIcon class="w-5 h-5"/> Chỉnh sửa Vị trí
            </h3>
            <button @click="closeModal" class="text-gray-400 hover:text-red-500"><XMarkIcon class="w-6 h-6"/></button>
          </div>
          <div class="p-5">
            <form @submit.prevent="handleSubmit" class="space-y-4">
              <div>
                <label class="block text-xs font-bold mb-1">Mã Vị Trí (Location Code)</label>
                <input v-model="formData.code" required type="text" class="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm uppercase focus:ring-primary-500">
              </div>
              
              <div class="mt-4">
                <label class="block text-xs font-bold mb-1 text-indigo-700">Tải trọng tối đa của Kệ này (kg)</label>
                <div class="relative">
                  <input v-model.number="formData.maxWeight" required type="number" min="1" class="w-full border border-indigo-200 bg-indigo-50 rounded-lg pl-3 pr-8 py-2 text-sm focus:ring-indigo-500">
                  <div class="absolute inset-y-0 right-0 pr-3 flex items-center pointer-events-none text-xs text-gray-500 font-bold">kg</div>
                </div>
                <p class="text-[10px] text-gray-500 mt-1 italic">Dùng để tính thanh tiến độ (Progress Bar).</p>
              </div>

              <div class="mt-6 pt-4 border-t flex justify-end gap-3">
                <button type="button" @click="closeModal" class="px-4 py-2 border border-gray-300 rounded-lg text-sm hover:bg-gray-50 font-semibold">Hủy</button>
                <button type="submit" class="px-4 py-2 bg-slate-800 text-white rounded-lg text-sm font-bold hover:bg-slate-900 shadow-sm flex items-center gap-2">
                  <PencilSquareIcon class="w-4 h-4"/> Lưu Thay Đổi
                </button>
              </div>
            </form>
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