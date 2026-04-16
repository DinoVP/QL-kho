<script setup>
import { ref, computed, onMounted } from 'vue'
import { 
  MagnifyingGlassIcon, PlusIcon, PencilSquareIcon, TrashIcon, 
  XMarkIcon, CubeIcon, PhotoIcon, ScaleIcon, ArrowsRightLeftIcon, 
  CurrencyDollarIcon, InformationCircleIcon, ArrowTrendingUpIcon, 
  ArrowTrendingDownIcon, ClockIcon 
} from '@heroicons/vue/24/outline'
import { useAuth } from '@/composables/useAuth' 

const { currentUserRole } = useAuth()
const currentRole = currentUserRole.value?.toLowerCase() || ''

// QUYỀN HẠN
const canEditProduct = computed(() => ['admin', 'giam_doc', 'nv_thu_mua'].includes(currentRole))
const canViewPrice = computed(() => ['admin', 'giam_doc', 'nv_thu_mua'].includes(currentRole))

const API_URL = 'https://localhost:7139/api/Products'
const CAT_API_URL = 'https://localhost:7139/api/Categories'

const products = ref([])
const isLoading = ref(false)

const categoryList = ref([])
const brandList = ref([])
const unitList = ref([])

const formatCurrency = (val) => new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(val || 0)

const formatDateTime = (val) => {
    if (!val) return '';
    const d = new Date(val);
    return d.toLocaleDateString('vi-VN') + ' ' + d.toLocaleTimeString('vi-VN', { hour: '2-digit', minute: '2-digit' });
}

const getAuthHeaders = () => ({
  'Content-Type': 'application/json',
  'Authorization': 'Bearer ' + (localStorage.getItem('authToken') || '')
})

const fetchDropdownData = async () => {
  try {
    const res = await fetch(CAT_API_URL, { headers: getAuthHeaders() })
    if (res.ok) {
      const data = await res.json()
      categoryList.value = data.filter(i => i.type === 'Nhóm sản phẩm').map(i => i.name)
      brandList.value = data.filter(i => i.type === 'Thương hiệu').map(i => i.name)
      unitList.value = data.filter(i => i.type === 'Đơn vị tính').map(i => i.name)
    }
  } catch (error) { 
    console.error('Lỗi load danh mục:', error) 
  }
}

const fetchProducts = async () => {
  isLoading.value = true
  try {
    const res = await fetch(API_URL, { headers: getAuthHeaders() })
    if (res.ok) {
      products.value = await res.json()
    }
  } catch (error) { 
    console.error('Lỗi load sản phẩm:', error) 
  } finally { 
    isLoading.value = false 
  }
}

const searchQuery = ref('')
const filterCategory = ref('')
const filterStatus = ref('')

const filteredProducts = computed(() => {
  return products.value.filter(p => {
    const matchSearch = (p.sku || '').toLowerCase().includes(searchQuery.value.toLowerCase()) || 
                        (p.name || '').toLowerCase().includes(searchQuery.value.toLowerCase())
    const matchCategory = filterCategory.value === '' || p.category === filterCategory.value
    const matchStatus = filterStatus.value === '' || p.status === filterStatus.value
    return matchSearch && matchCategory && matchStatus
  })
})

const showModal = ref(false)
const modalMode = ref('add') 

const formData = ref({ 
  id: 0, name: '', category: '', brand: '', unit: '', 
  packSize: '', weight: 0, status: 'active', conversions: [],
  importPrice: 0 
})

const packTiers = ref([])

// ---- STATE CHO MODAL LỊCH SỬ GIÁ ----
const showHistoryModal = ref(false)
const historyProduct = ref(null)
const priceHistoryList = ref([]) 
const isLoadingHistory = ref(false)

const addTier = () => {
  packTiers.value.unshift({ unitName: '', qtyPerPrev: 1 })
}

const removeTier = (index) => {
  packTiers.value.splice(index, 1)
}

const getAvailableUnits = (currentIndex) => {
  return unitList.value.filter(u => {
    if (u === formData.value.unit) return false; 
    for (let i = 0; i < packTiers.value.length; i++) {
      if (i !== currentIndex && packTiers.value[i].unitName === u) return false; 
    }
    return true;
  });
}

const continuousChainDisplay = computed(() => {
  if (packTiers.value.length === 0 || !formData.value.unit) return ''
  if (packTiers.value.some(t => !t.unitName)) return 'Vui lòng chọn đầy đủ tên Đơn vị...'

  let chain = []
  let currentMultiplier = 1
  
  chain.push(`1 ${packTiers.value[0].unitName}`)

  for (let i = 0; i < packTiers.value.length; i++) {
    currentMultiplier *= (packTiers.value[i].qtyPerPrev || 1)
    const nextUnitName = (i === packTiers.value.length - 1) ? formData.value.unit : packTiers.value[i+1].unitName
    chain.push(`${currentMultiplier} ${nextUnitName}`)
  }
  return chain.join(' = ')
})

const getConversionString = (product) => {
  if (!product.conversions || product.conversions.length === 0) {
    return product.packSize || 'Hàng bán lẻ (Không quy đổi)'
  }
  
  let sorted = [...product.conversions].sort((a, b) => b.rate - a.rate)
  let str = `1 ${sorted[0].altUnit}`
  
  for (let i = 0; i < sorted.length - 1; i++) {
    let childQty = sorted[i].rate / sorted[i+1].rate
    str += ` = ${childQty} ${sorted[i+1].altUnit}`
  }
  
  str += ` = ${sorted[sorted.length-1].rate} ${product.unit || 'SL'}`
  return str
}

const getTierDetails = (product) => {
  let results = [];
  results.push({ 
    name: `1 ${product.unit || 'SL'}`, 
    weight: product.weight || 0, 
    price: product.importPrice || 0,
    isMain: true 
  });

  if (product.conversions && product.conversions.length > 0) {
    product.conversions.forEach(c => {
      let w = (product.weight || 0) * c.rate;
      let p = (product.importPrice || 0) * c.rate;
      results.push({ 
        name: `1 ${c.altUnit}`, 
        weight: parseFloat(w.toFixed(3)), 
        price: p,
        isMain: false
      });
    });
  }
  return results.sort((a,b) => a.weight - b.weight);
}

// ---- CÁC HÀM XỬ LÝ FORM CHÍNH ----
const openModal = async (mode, product = null) => {
  modalMode.value = mode
  packTiers.value = []

  if (product) {
    if (product.conversions && product.conversions.length > 0) {
      let convs = [...product.conversions].sort((a,b) => a.rate - b.rate)
      let tempTiers = []
      let prevRate = 1
      for (let c of convs) {
        tempTiers.push({
          unitName: c.altUnit,
          qtyPerPrev: c.rate / prevRate 
        })
        prevRate = c.rate
      }
      packTiers.value = tempTiers.reverse()
    }
    formData.value = { ...product, importPrice: product.importPrice || 0 } 
  } else {
    formData.value = { id: 0, name: '', category: '', brand: '', unit: '', packSize: '', weight: 0, status: 'active', conversions: [], importPrice: 0 }
  }
  showModal.value = true
}

const closeModal = () => showModal.value = false

const handleSubmit = async () => {
  if (!formData.value.category || !formData.value.brand || !formData.value.unit) {
    alert("Vui lòng chọn đầy đủ Danh mục, Thương hiệu và ĐVT Cơ bản!"); return;
  }

  let payloadConversions = [];
  let absoluteRate = 1;

  for (let i = packTiers.value.length - 1; i >= 0; i--) {
    let tier = packTiers.value[i];
    if (!tier.unitName.trim()) {
      alert("Tên Đơn vị quy đổi không được để trống!"); return;
    }
    absoluteRate *= (tier.qtyPerPrev || 1);
    payloadConversions.push({ altUnit: tier.unitName.trim(), rate: absoluteRate });
  }

  try {
    const method = modalMode.value === 'add' ? 'POST' : 'PUT'
    const url = modalMode.value === 'add' ? API_URL : `${API_URL}/${formData.value.id}`
    const payload = { ...formData.value, conversions: payloadConversions, id: formData.value.id || 0 }

    const res = await fetch(url, { 
      method: method, 
      headers: getAuthHeaders(), 
      body: JSON.stringify(payload) 
    })

    if (res.ok) {
      await fetchProducts()
      closeModal()
    } else {
      const err = await res.json()
      alert('Lỗi: ' + (err.message || 'Hệ thống từ chối do trùng lặp dữ liệu'))
    }
  } catch (error) { 
    alert('Lỗi kết nối máy chủ!') 
  }
}

const handleDelete = async (id, name) => {
  if (confirm(`Sếp có chắc chắn muốn xóa Sản phẩm "${name}" không?`)) {
    try {
      const res = await fetch(`${API_URL}/${id}`, { method: 'DELETE', headers: getAuthHeaders() })
      if (res.ok) { 
        await fetchProducts() 
      } else { 
        alert('Sản phẩm đã phát sinh giao dịch hoặc đang tồn kho, không thể xóa!') 
      }
    } catch (error) { console.error('Lỗi xóa:', error) }
  }
}

// ---- CÁC HÀM XỬ LÝ MODAL LỊCH SỬ ----
const openHistoryModal = async (product) => {
    historyProduct.value = product;
    showHistoryModal.value = true;
    isLoadingHistory.value = true;
    try {
        const res = await fetch(`${API_URL}/${product.id}/price-history`, { headers: getAuthHeaders() });
        if (res.ok) {
            priceHistoryList.value = await res.json();
        } else {
            priceHistoryList.value = [];
        }
    } catch (e) { 
        console.error("Chưa có API Lịch sử giá hoặc lỗi kết nối."); 
    } finally { 
        isLoadingHistory.value = false; 
    }
}

const closeHistoryModal = () => {
    showHistoryModal.value = false;
    historyProduct.value = null;
    priceHistoryList.value = [];
}

onMounted(() => {
  fetchDropdownData()
  fetchProducts()
})
</script>

<template>
  <div class="space-y-5 md:space-y-6 animate-fade-in pb-10 px-0 md:px-1 relative">
    
    <div class="flex flex-col sm:flex-row sm:items-center justify-between gap-4">
      <div>
        <h2 class="text-xl md:text-2xl font-bold text-gray-800">Danh mục Sản phẩm (SKU)</h2>
        <p class="text-xs md:text-sm text-gray-500 mt-1">Quản lý mã hàng, quy cách đóng gói đa cấp động</p>
        <p class="text-[10px] font-bold text-indigo-600 bg-indigo-50 px-2 py-1 rounded border border-indigo-200 uppercase mt-2 w-fit">Vai trò: {{ currentRole }}</p>
      </div>
      <div class="flex gap-2">
        <button 
          v-if="canEditProduct" 
          @click="openModal('add')" 
          class="bg-primary-600 hover:bg-primary-700 text-white px-4 py-2.5 rounded-lg flex items-center gap-2 text-sm font-semibold transition-colors shadow-sm"
        >
          <PlusIcon class="w-5 h-5" /> Thêm Sản Phẩm Mới
        </button>
      </div>
    </div>

    <div class="bg-white p-3 md:p-4 rounded-xl border border-gray-200 flex flex-col lg:flex-row items-center gap-3 md:gap-4 shadow-sm">
      <div class="relative w-full lg:flex-1">
        <div class="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none">
          <MagnifyingGlassIcon class="w-5 h-5 text-gray-400" />
        </div>
        <input 
          v-model="searchQuery" 
          type="text" 
          class="block w-full pl-10 pr-3 py-2 border border-gray-200 rounded-lg text-sm outline-none focus:ring-1 focus:ring-primary-500" 
          placeholder="Tìm kiếm theo mã SKU, Tên sản phẩm..."
        >
      </div>
      
      <div class="flex flex-col sm:flex-row gap-3 w-full lg:w-auto">
        <select v-model="filterCategory" class="w-full sm:w-auto border border-gray-200 rounded-lg text-sm px-4 py-2 outline-none cursor-pointer">
          <option value="">Tất cả danh mục</option>
          <option v-for="cat in categoryList" :key="cat" :value="cat">{{ cat }}</option>
        </select>
        <select v-model="filterStatus" class="w-full sm:w-auto border border-gray-200 rounded-lg text-sm px-4 py-2 outline-none cursor-pointer">
          <option value="">Tất cả Trạng thái</option>
          <option value="active">Đang kinh doanh</option>
          <option value="inactive">Ngừng kinh doanh</option>
        </select>
      </div>
    </div>

    <div class="bg-white rounded-xl border border-gray-200 shadow-sm overflow-hidden">
      <div class="w-full overflow-x-auto custom-scrollbar">
        <table class="min-w-[1050px] w-full divide-y divide-gray-200">
          <thead class="bg-gray-50">
            <tr>
              <th class="px-5 py-3.5 text-left text-xs font-bold text-gray-500 uppercase tracking-wider w-16">Ảnh</th>
              <th class="px-5 py-3.5 text-left text-xs font-bold text-gray-500 uppercase tracking-wider">Mã & Tên Sản phẩm</th>
              <th class="px-5 py-3.5 text-left text-xs font-bold text-gray-500 uppercase tracking-wider">Phân loại</th>
              <th class="px-5 py-3.5 text-left text-xs font-bold text-gray-500 uppercase tracking-wider">Quy cách Đa cấp & Cân nặng</th>
              <th v-if="canViewPrice" class="px-5 py-3.5 text-right text-xs font-bold text-gray-500 uppercase tracking-wider">Giá Nhập (Quy đổi)</th>
              <th class="px-5 py-3.5 text-center text-xs font-bold text-gray-500 uppercase tracking-wider">Trạng thái</th>
              <th v-if="canEditProduct || canViewPrice" class="px-5 py-3.5 text-right text-xs font-bold text-gray-500 uppercase tracking-wider">Thao tác</th>
            </tr>
          </thead>
          
          <tbody class="divide-y divide-gray-100 bg-white">
            <tr v-if="isLoading">
              <td :colspan="canViewPrice ? 7 : 6" class="px-6 py-12 text-center text-gray-500">Đang tải sản phẩm...</td>
            </tr>
            <tr v-else-if="filteredProducts.length === 0">
              <td :colspan="canViewPrice ? 7 : 6" class="px-6 py-16 text-center">
                <CubeIcon class="w-12 h-12 text-gray-300 mx-auto mb-3" />
                <h3 class="text-base font-semibold text-gray-700">Chưa có sản phẩm nào</h3>
              </td>
            </tr>

            <tr v-for="product in filteredProducts" :key="product.id" class="hover:bg-gray-50 transition-colors">
              <td class="px-5 py-3"><PhotoIcon class="w-8 h-8 text-gray-300" /></td>
              <td class="px-5 py-3">
                <div class="flex flex-col">
                  <span class="text-sm font-bold text-primary-700">{{ product.sku }}</span>
                  <span class="text-sm font-bold text-gray-900 mt-0.5">{{ product.name }}</span>
                </div>
              </td>
              <td class="px-5 py-3">
                <div class="flex flex-col gap-1">
                  <span class="text-[10px] bg-gray-100 px-2 py-0.5 rounded text-gray-600 border w-fit">{{ product.category }}</span>
                  <span class="text-[10px] bg-gray-100 px-2 py-0.5 rounded text-gray-600 border w-fit">{{ product.brand }}</span>
                </div>
              </td>
              
              <td class="px-5 py-3">
                <div class="flex flex-col space-y-1">
                  <div class="flex items-center gap-1 text-xs text-gray-600">
                    <CubeIcon class="w-3.5 h-3.5"/> ĐVT Cơ bản: <strong class="text-indigo-600">{{ product.unit }}</strong>
                  </div>
                  
                  <div class="flex items-start gap-1 text-[12px] mt-0.5">
                    <ArrowsRightLeftIcon class="w-4 h-4 mt-0.5 shrink-0 text-orange-500"/> 
                    <span class="font-bold text-orange-600">{{ getConversionString(product) }}</span>
                  </div>

                  <div class="flex flex-wrap items-center gap-2 text-[11px] text-gray-600 mt-1.5 bg-gray-50 px-2 py-1.5 rounded border border-gray-100 w-fit">
                    <ScaleIcon class="w-4 h-4 text-gray-400"/>
                    <template v-for="(tInfo, idx) in getTierDetails(product)" :key="idx">
                      <span :class="{'border-l border-gray-300 pl-2': idx > 0}">
                        <strong :class="tInfo.isMain ? 'text-indigo-700' : 'text-gray-700'">{{ tInfo.name }}:</strong> {{ tInfo.weight }} kg
                      </span>
                    </template>
                  </div>
                </div>
              </td>

              <td v-if="canViewPrice" class="px-5 py-3 text-right">
                <div class="flex flex-col gap-1 items-end">
                  <span v-for="(tInfo, idx) in getTierDetails(product)" :key="'p'+idx" 
                        :class="tInfo.isMain ? 'bg-blue-50 text-blue-700 border-blue-200' : 'bg-slate-50 text-slate-600 border-slate-200'"
                        class="text-[11px] font-bold px-2 py-0.5 rounded border w-fit">
                    <span class="text-[10px] text-slate-400 mr-1">{{ tInfo.name }}</span> {{ formatCurrency(tInfo.price) }}
                  </span>
                </div>
              </td>

              <td class="px-5 py-3 text-center">
                <span :class="product.status === 'active' ? 'bg-green-100 text-green-700 border-green-200' : 'bg-red-100 text-red-700 border-red-200'" class="text-[10px] font-bold px-2 py-1 rounded border uppercase">
                  {{ product.status === 'active' ? 'Đang KD' : 'Ngừng KD' }}
                </span>
              </td>
              
              <td v-if="canEditProduct || canViewPrice" class="px-5 py-3 text-right space-x-1.5 whitespace-nowrap">
                <button v-if="canViewPrice" @click="openHistoryModal(product)" class="p-1.5 text-blue-600 hover:bg-blue-50 rounded transition-colors" title="Xem Lịch sử Biến động Giá"><ClockIcon class="w-5 h-5" /></button>
                <button v-if="canEditProduct" @click="openModal('edit', product)" class="p-1.5 text-amber-600 hover:bg-amber-50 rounded transition-colors" title="Sửa thông tin"><PencilSquareIcon class="w-5 h-5" /></button>
                <button v-if="canEditProduct" @click="handleDelete(product.id, product.name)" class="p-1.5 text-red-600 hover:bg-red-50 rounded transition-colors" title="Xóa sản phẩm"><TrashIcon class="w-5 h-5" /></button>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>

    <Teleport to="body">
      <div v-if="showModal" class="fixed inset-0 z-[60] flex items-center justify-center px-4 bg-slate-900/50 backdrop-blur-sm animate-fade-in">
        <div class="bg-white rounded-xl shadow-2xl w-full max-w-5xl overflow-hidden transform transition-all flex flex-col max-h-[95vh]">
          
          <div class="px-6 py-4 border-b flex items-center justify-between bg-gray-50 shrink-0">
            <h3 class="text-lg font-bold text-gray-800">{{ modalMode === 'add' ? 'Thêm Sản phẩm mới' : `Cập nhật: ${formData.name}` }}</h3>
            <button @click="closeModal" class="text-gray-400 hover:text-red-500"><XMarkIcon class="w-6 h-6" /></button>
          </div>

          <div class="p-6 overflow-y-auto flex-1 custom-scrollbar bg-slate-50/50">
            <form @submit.prevent="handleSubmit" id="productForm" class="space-y-6">
              <div class="grid grid-cols-1 md:grid-cols-12 gap-8">
                
                <div class="md:col-span-5 space-y-4 h-fit">
                  <div class="bg-white p-5 rounded-xl border border-gray-200 shadow-sm">
                    <h4 class="text-sm font-bold text-primary-600 border-b pb-2 flex items-center gap-2">
                      <CubeIcon class="w-4 h-4"/> 1. Thông tin Cơ bản
                    </h4>
                    
                    <div class="mt-4 space-y-4">
                      <div>
                        <label class="block text-xs font-bold mb-1">Tên Sản phẩm <span class="text-red-500">*</span></label>
                        <input v-model="formData.name" type="text" required class="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm focus:ring-1 focus:ring-primary-500" placeholder="VD: Sữa chua Vinamilk">
                      </div>

                      <div class="grid grid-cols-2 gap-4">
                        <div>
                          <label class="block text-xs font-bold mb-1">Danh mục <span class="text-red-500">*</span></label>
                          <select v-model="formData.category" required class="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm cursor-pointer">
                            <option value="" disabled selected>-- Chọn --</option>
                            <option v-for="cat in categoryList" :key="cat" :value="cat">{{ cat }}</option>
                          </select>
                        </div>
                        <div>
                          <label class="block text-xs font-bold mb-1">Thương hiệu <span class="text-red-500">*</span></label>
                          <select v-model="formData.brand" required class="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm cursor-pointer">
                            <option value="" disabled selected>-- Chọn --</option>
                            <option v-for="b in brandList" :key="b" :value="b">{{ b }}</option>
                          </select>
                        </div>
                      </div>

                      <div class="grid grid-cols-2 gap-4 p-3 bg-indigo-50/50 border border-indigo-100 rounded-lg">
                        <div>
                          <label class="block text-[11px] font-bold mb-1 text-indigo-700">ĐVT CƠ BẢN <span class="text-red-500">*</span></label>
                          <select v-model="formData.unit" required class="w-full border border-indigo-300 bg-white rounded px-2 py-1.5 text-sm font-bold text-indigo-700 cursor-pointer outline-none focus:ring-1 focus:ring-indigo-500 shadow-sm">
                            <option value="" disabled selected>-- Chọn ĐVT --</option>
                            <option v-for="u in unitList" :key="u" :value="u">{{ u }}</option>
                          </select>
                        </div>

                        <div>
                          <label class="block text-[11px] font-bold mb-1 text-indigo-700">TRỌNG LƯỢNG / 1 ĐVT</label>
                          <div class="relative">
                            <input v-model.number="formData.weight" type="number" step="0.001" min="0" class="w-full border border-indigo-300 bg-white rounded pl-2 pr-7 py-1.5 text-sm focus:ring-1 focus:ring-indigo-500 outline-none font-bold shadow-inner" placeholder="VD: 0.2">
                            <div class="absolute inset-y-0 right-0 pr-2 flex items-center pointer-events-none text-[10px] text-indigo-500 font-bold">kg</div>
                          </div>
                        </div>
                      </div>
                      
                      <div>
                        <label class="block text-xs font-bold mb-1">Trạng thái kinh doanh</label>
                        <select v-model="formData.status" class="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm cursor-pointer">
                          <option value="active">Đang kinh doanh</option>
                          <option value="inactive">Ngừng kinh doanh</option>
                        </select>
                      </div>
                    </div>
                  </div>

                  <div v-if="canViewPrice" class="bg-white p-5 rounded-xl border border-blue-200 shadow-sm bg-blue-50/10">
                    <h4 class="text-sm font-bold text-blue-700 border-b pb-2 flex items-center gap-2">
                      <CurrencyDollarIcon class="w-4 h-4"/> 2. Thiết lập Giá
                    </h4>
                    <p class="text-[10px] text-gray-500 italic mt-1 mb-4">Nhập giá mua vào của 1 {{ formData.unit || 'Đơn vị cơ bản' }}. Hệ thống sẽ tự nhân lên cho các cấp Lớn hơn.</p>
                    
                    <div>
                        <label class="block text-[11px] font-bold mb-1 text-blue-700 uppercase tracking-wide">Đơn giá Nhập (Dự tính)</label>
                        <div class="relative">
                            <input v-model.number="formData.importPrice" type="number" min="0" class="w-full border border-blue-300 rounded-lg pl-3 pr-12 py-2.5 text-base font-bold text-blue-800 focus:ring-1 focus:ring-blue-500 outline-none shadow-inner" placeholder="0">
                            <div class="absolute inset-y-0 right-0 pr-4 flex items-center pointer-events-none text-xs text-blue-500 font-bold">VNĐ</div>
                        </div>
                    </div>
                  </div>
                </div>

                <div class="md:col-span-7 bg-white p-5 rounded-xl border border-gray-200 shadow-sm flex flex-col">
                  <h4 class="text-sm font-bold text-indigo-600 border-b pb-2 flex items-center justify-between mb-4 shrink-0">
                    <div class="flex items-center gap-2"><ScaleIcon class="w-4 h-4"/> {{ canViewPrice ? '3.' : '2.' }} Quy đổi Đóng gói Đa cấp</div>
                    <button @click.prevent="addTier" class="bg-indigo-100 hover:bg-indigo-200 text-indigo-700 px-3 py-1 rounded text-xs font-bold transition-colors shadow-sm">
                      + Thêm Cấp Quy Đổi
                    </button>
                  </h4>
                  
                  <div v-if="!formData.unit" class="py-8 text-center bg-yellow-50 rounded-lg border border-dashed border-yellow-300">
                    <p class="text-sm font-medium text-yellow-700">Vui lòng chọn <b>ĐVT CƠ BẢN</b> ở Cột bên trái trước!</p>
                  </div>

                  <div v-else-if="packTiers.length > 0" class="flex-1 flex flex-col min-h-0">
                    <h5 class="text-[11px] font-bold text-gray-500 uppercase mb-2 flex items-center gap-1 shrink-0">
                      <ArrowsRightLeftIcon class="w-3.5 h-3.5"/> Cấu trúc Bắc Cầu
                    </h5>

                    <div class="space-y-2 overflow-y-auto max-h-[260px] custom-scrollbar pr-2 flex-1">
                      <div v-for="(tier, index) in packTiers" :key="index" class="flex items-center gap-2 bg-gray-50 p-2.5 rounded-lg border border-gray-200 animate-fade-in shadow-sm">
                        <span class="text-sm font-bold text-gray-400 bg-white px-2 py-1.5 border rounded shadow-sm cursor-not-allowed">1</span>
                        <select v-model="tier.unitName" required class="w-full border border-indigo-300 rounded px-2 py-1.5 text-sm font-bold text-indigo-700 bg-white cursor-pointer focus:ring-1 focus:ring-indigo-500 outline-none shadow-sm">
                          <option value="" disabled selected>-- Chọn ĐVT --</option>
                          <option v-for="u in getAvailableUnits(index)" :key="u" :value="u">{{ u }}</option>
                        </select>
                        <span class="text-sm font-bold text-gray-500">=</span>
                        <input v-model.number="tier.qtyPerPrev" type="number" min="1" placeholder="SL" class="w-16 md:w-20 border border-gray-300 rounded px-2 py-1.5 text-sm font-bold text-center focus:ring-1 focus:ring-indigo-500 outline-none shadow-inner">
                        <span class="text-sm font-bold text-gray-600 bg-gray-200 px-2 md:px-3 py-1.5 rounded min-w-[70px] text-center border border-gray-300 shadow-inner truncate cursor-not-allowed">
                          {{ index === packTiers.length - 1 ? (formData.unit || 'ĐVT Cơ Bản') : (packTiers[index+1].unitName || '...') }}
                        </span>
                        <button @click.prevent="removeTier(index)" class="text-red-400 hover:text-red-600 p-1.5 bg-white border rounded hover:bg-red-50 transition-colors shadow-sm shrink-0">
                          <TrashIcon class="w-4 h-4 md:w-5 md:h-5"/>
                        </button>
                      </div>
                    </div>

                    <div v-if="continuousChainDisplay" class="mt-4 p-3 bg-orange-50 border border-orange-200 rounded-lg flex items-start gap-2 shadow-sm shrink-0">
                      <InformationCircleIcon class="w-5 h-5 text-orange-500 mt-0.5 shrink-0" />
                      <div>
                        <span class="text-[11px] font-bold text-orange-800 uppercase block mb-1">Mô phỏng Tổng cấu trúc Tồn kho:</span>
                        <span class="text-[15px] font-black text-orange-600">{{ continuousChainDisplay }}</span>
                      </div>
                    </div>
                  </div>
                  
                  <div v-else class="py-12 text-center bg-gray-50/50 rounded-lg border border-dashed border-gray-300 flex-1 flex flex-col justify-center">
                    <CubeIcon class="w-10 h-10 text-gray-300 mx-auto mb-2" />
                    <p class="text-sm text-gray-500 font-medium">Sản phẩm bán lẻ (Không có quy đổi).</p>
                    <p class="text-xs text-gray-400 mt-1">Bấm nút "+ Thêm Cấp Quy Đổi" ở trên để thiết lập Thùng, Lốc...</p>
                  </div>

                </div>
              </div>
            </form>
          </div>

          <div class="px-6 py-4 border-t flex justify-end gap-3 shrink-0 bg-white shadow-inner">
            <button type="button" @click="closeModal" class="px-4 py-2 border border-gray-300 rounded-lg text-sm font-semibold hover:bg-gray-50 transition-colors shadow-sm">Hủy bỏ</button>
            <button type="submit" form="productForm" class="px-4 py-2 bg-primary-600 text-white rounded-lg text-sm font-semibold hover:bg-primary-700 transition-colors shadow-sm flex items-center gap-2">
              Lưu Sản phẩm
            </button>
          </div>
        </div>
      </div>
    </Teleport>

    <Teleport to="body">
      <div v-if="showHistoryModal" class="fixed inset-0 z-[70] flex items-center justify-center px-4 bg-slate-900/60 backdrop-blur-sm animate-fade-in">
        <div class="bg-white rounded-2xl shadow-2xl w-full max-w-4xl overflow-hidden transform transition-all flex flex-col max-h-[85vh]">
          
          <div class="px-6 py-5 border-b border-gray-100 flex items-center justify-between bg-white shrink-0 shadow-sm z-10">
            <div>
              <h3 class="text-xl font-black text-gray-800 flex items-center gap-2">
                <ClockIcon class="w-6 h-6 text-blue-600"/> Lịch sử Biến động Giá
              </h3>
              <p class="text-sm text-gray-500 font-medium mt-1">
                Sản phẩm: <span class="text-blue-700 font-bold">[{{ historyProduct?.sku }}] {{ historyProduct?.name }}</span>
              </p>
            </div>
            <button @click="closeHistoryModal" class="text-gray-400 hover:text-red-500 bg-gray-50 hover:bg-red-50 p-2 rounded-full transition-colors"><XMarkIcon class="w-6 h-6" /></button>
          </div>

          <div class="p-6 overflow-y-auto flex-1 custom-scrollbar bg-slate-50/50">
            
            <div class="mb-6 flex justify-end">
              <div class="bg-blue-50 border border-blue-200 px-4 py-2 rounded-lg text-right shadow-sm">
                <p class="text-[10px] text-blue-500 uppercase font-bold tracking-wider">Giá nhập đang áp dụng</p>
                <p class="text-2xl font-black text-blue-700">{{ formatCurrency(historyProduct?.importPrice) }}</p>
              </div>
            </div>

            <div v-if="isLoadingHistory" class="text-center py-16 text-gray-400 font-medium">Đang tải dữ liệu lịch sử...</div>
            <div v-else-if="priceHistoryList.length === 0" class="text-center py-16 bg-white rounded-xl border border-dashed border-gray-300">
                <ClockIcon class="w-16 h-16 text-gray-200 mx-auto mb-4" />
                <p class="text-base font-bold text-gray-500">Chưa có dữ liệu biến động giá</p>
                <p class="text-sm text-gray-400 mt-1">Hệ thống sẽ tự động ghi nhận khi bạn cập nhật giá mới.</p>
            </div>

            <div v-else class="space-y-6 relative before:absolute before:inset-0 before:ml-5 before:-translate-x-px md:before:mx-auto md:before:translate-x-0 before:h-full before:w-0.5 before:bg-gradient-to-b before:from-gray-200 before:via-gray-300 before:to-gray-100">
                <div v-for="(hist, idx) in priceHistoryList" :key="hist.historyId || idx" class="relative flex items-center justify-between md:justify-normal md:odd:flex-row-reverse group is-active">
                    
                    <div class="flex items-center justify-center w-10 h-10 rounded-full border-4 border-white shrink-0 md:order-1 md:group-odd:-translate-x-1/2 md:group-even:translate-x-1/2 shadow-md z-10 transition-transform hover:scale-110"
                         :class="hist.newPrice > hist.oldPrice ? 'bg-red-100 text-red-600' : (hist.newPrice < hist.oldPrice ? 'bg-emerald-100 text-emerald-600' : 'bg-gray-100 text-gray-500')">
                        <ArrowTrendingUpIcon v-if="hist.newPrice > hist.oldPrice" class="w-5 h-5 stroke-[2.5]"/>
                        <ArrowTrendingDownIcon v-else-if="hist.newPrice < hist.oldPrice" class="w-5 h-5 stroke-[2.5]"/>
                        <span v-else class="w-3 h-3 rounded-full bg-gray-400"></span>
                    </div>
                    
                    <div class="w-[calc(100%-4rem)] md:w-[calc(50%-3rem)] bg-white p-5 rounded-2xl border border-gray-200 shadow-sm hover:shadow-md transition-shadow">
                        <div class="flex items-center justify-between mb-2">
                            <span class="text-xs font-bold text-gray-400 bg-gray-50 px-2 py-1 rounded border">{{ formatDateTime(hist.effectiveDate) }}</span>
                            <span class="text-[10px] font-black px-2.5 py-1 rounded-full uppercase tracking-wider"
                                  :class="hist.newPrice > hist.oldPrice ? 'bg-red-50 text-red-600 border border-red-100' : (hist.newPrice < hist.oldPrice ? 'bg-emerald-50 text-emerald-600 border border-emerald-100' : 'bg-gray-100 text-gray-600 border border-gray-200')">
                                {{ hist.newPrice > hist.oldPrice ? 'TĂNG' : (hist.newPrice < hist.oldPrice ? 'GIẢM' : 'THIẾT LẬP') }}
                            </span>
                        </div>
                        
                        <div class="flex items-center gap-3 mb-3 bg-slate-50 p-3 rounded-xl border border-slate-100">
                            <div>
                              <p class="text-[10px] text-gray-500 uppercase font-bold mb-0.5">Giá cũ</p>
                              <span class="text-sm font-bold text-gray-400 line-through">{{ formatCurrency(hist.oldPrice) }}</span>
                            </div>
                            <ArrowsRightLeftIcon class="w-4 h-4 text-gray-300" />
                            <div>
                              <p class="text-[10px] text-blue-500 uppercase font-bold mb-0.5">Giá mới</p>
                              <span class="text-xl font-black" :class="hist.newPrice > hist.oldPrice ? 'text-red-600' : (hist.newPrice < hist.oldPrice ? 'text-emerald-600' : 'text-gray-800')">
                                {{ formatCurrency(hist.newPrice) }}
                              </span>
                            </div>
                        </div>

                        <div class="text-xs text-gray-600 flex flex-col gap-1.5">
                            <p class="flex items-center gap-2"><span class="w-1.5 h-1.5 rounded-full bg-gray-300"></span> Nguồn: <strong class="text-gray-800">{{ hist.source || 'Hệ thống' }}</strong></p>
                            <p class="flex items-center gap-2"><span class="w-1.5 h-1.5 rounded-full bg-gray-300"></span> Cập nhật bởi: <strong class="text-gray-800">{{ hist.updatedBy || 'Auto' }}</strong></p>
                        </div>
                    </div>

                </div>
            </div>

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
.custom-scrollbar::-webkit-scrollbar-thumb:hover { background: #94a3b8; }
.animate-fade-in { animation: fadeIn 0.15s ease-out; }
@keyframes fadeIn { from { opacity: 0; transform: translateY(2px); } to { opacity: 1; transform: translateY(0); } }
</style>