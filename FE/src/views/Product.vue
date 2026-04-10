<script setup>
import { ref, computed, onMounted } from 'vue'
import { 
  MagnifyingGlassIcon, PlusIcon, PencilSquareIcon, TrashIcon, EyeIcon, 
  XMarkIcon, CubeIcon, PhotoIcon, ScaleIcon, ArrowsRightLeftIcon, 
  ArrowRightIcon, CurrencyDollarIcon 
} from '@heroicons/vue/24/outline'
import { useAuth } from '@/composables/useAuth' 

const { currentUserRole } = useAuth()
const currentRole = currentUserRole.value?.toLowerCase() || ''

// 1. CHỈ THU MUA VÀ ADMIN MỚI ĐƯỢC QUẢN LÝ SẢN PHẨM & GIÁ NHẬP
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

// ĐÃ XÓA HOÀN TOÀN 'price' (Giá Bán) - CHỈ CÒN 'importPrice'
const formData = ref({ 
  id: 0, name: '', category: '', brand: '', unit: '', 
  packSize: '', weight: 0, status: 'active', conversions: [],
  importPrice: 0 
})

// === QUY CÁCH ĐÓNG GÓI ===
const packState = ref({
  hasSmall: false, smallRate: 1,
  hasBig: false, bigRate: 1,
  hasPallet: false, palletRate: 1
})

const smallTotal = computed(() => packState.value.hasSmall ? packState.value.smallRate : 1)
const bigTotal = computed(() => packState.value.hasBig ? (packState.value.bigRate * smallTotal.value) : smallTotal.value)
const palletTotal = computed(() => packState.value.hasPallet ? (packState.value.palletRate * bigTotal.value) : bigTotal.value)

const prevForBig = computed(() => packState.value.hasSmall ? 'Thùng nhỏ' : (formData.value.unit || 'Sản phẩm lẻ'))
const prevForPallet = computed(() => packState.value.hasBig ? 'Thùng to' : (packState.value.hasSmall ? 'Thùng nhỏ' : (formData.value.unit || 'Sản phẩm lẻ')))

const smallBoxFormula = computed(() => { 
  return `1 Thùng nhỏ = ${packState.value.smallRate} ${formData.value.unit || 'SL'}` 
})
const bigBoxFormula = computed(() => {
  let str = `1 Thùng to = `
  if (packState.value.hasSmall) { str += `${packState.value.bigRate} Thùng nhỏ = ` }
  str += `${bigTotal.value} ${formData.value.unit || 'SL'}`
  return str
})
const palletFormula = computed(() => {
  let str = `1 Pallet = `
  if (packState.value.hasBig) {
    str += `${packState.value.palletRate} Thùng to = `
    if (packState.value.hasSmall) { str += `${packState.value.palletRate * packState.value.bigRate} Thùng nhỏ = ` }
  } else if (packState.value.hasSmall) {
    str += `${packState.value.palletRate} Thùng nhỏ = `
  }
  str += `${palletTotal.value} ${formData.value.unit || 'SL'}`
  return str
})

const getConversionString = (product) => {
  if (!product.conversions || product.conversions.length === 0) {
    if (product.packSize && !isNaN(product.packSize)) return `1 Thùng = ${product.packSize} ${product.unit || 'SL'}`;
    return product.packSize || 'Chưa thiết lập quy cách Đa cấp'
  }
  
  const small = product.conversions.find(c => c.altUnit === 'Thùng nhỏ')
  const big = product.conversions.find(c => c.altUnit === 'Thùng to')
  const pallet = product.conversions.find(c => c.altUnit === 'Pallet')
  const unit = product.unit || 'SL'

  let parts = []
  if (pallet) {
    parts.push(`1 Pallet`)
    if (big && big.rate > 0) parts.push(`${pallet.rate / big.rate} Thùng to`)
    if (small && small.rate > 0) parts.push(`${pallet.rate / small.rate} Thùng nhỏ`)
    parts.push(`${pallet.rate} ${unit}`)
    return parts.join(' = ')
  }
  if (big) {
    parts.push(`1 Thùng to`)
    if (small && small.rate > 0) parts.push(`${big.rate / small.rate} Thùng nhỏ`)
    parts.push(`${big.rate} ${unit}`)
    return parts.join(' = ')
  }
  if (small) return `1 Thùng nhỏ = ${small.rate} ${unit}`
  return product.packSize || 'Chưa thiết lập quy cách'
}

const getWeightDisplay = (product) => {
  if (!product.weight || product.weight <= 0) return [{ name: `Trọng lượng`, weight: 0 }];
  
  let results = []; let basePackRate = 1; let basePackName = 'Thùng/Kiện'; let hasConversions = false;

  if (product.conversions && product.conversions.length > 0) {
    hasConversions = true;
    const small = product.conversions.find(c => c.altUnit === 'Thùng nhỏ');
    if (small) { basePackRate = small.rate; basePackName = 'Thùng nhỏ'; } 
    else { basePackRate = product.conversions[0].rate; basePackName = product.conversions[0].altUnit; }
  } else if (product.conversionRate > 1) {
    hasConversions = true; basePackRate = product.conversionRate; basePackName = 'Thùng';
  } 

  if (!hasConversions) return [{ name: `1 ${product.unit || 'SL'}`, weight: product.weight, isMain: true }];

  const unitWeight = product.weight / basePackRate;
  results.push({ name: `1 ${basePackName}`, weight: product.weight, isMain: true });

  if (product.conversions && product.conversions.length > 0) {
    product.conversions.forEach(c => {
      if (c.altUnit !== basePackName) {
        const w = unitWeight * c.rate;
        results.push({ name: `1 ${c.altUnit}`, weight: parseFloat(w.toFixed(3)) });
      }
    });
  }
  return results.sort((a,b) => a.weight - b.weight);
}

const openModal = (mode, product = null) => {
  modalMode.value = mode
  
  packState.value = { hasSmall: false, smallRate: 1, hasBig: false, bigRate: 1, hasPallet: false, palletRate: 1 }

  if (product) {
    if (product.conversions && product.conversions.length > 0) {
      let convs = [...product.conversions].sort((a,b) => a.rate - b.rate)
      
      let small = convs.find(c => c.altUnit === 'Thùng nhỏ')
      let big = convs.find(c => c.altUnit === 'Thùng to')
      let pallet = convs.find(c => c.altUnit === 'Pallet')

      if (!small && !big && !pallet) {
        if(convs.length >= 1) small = convs[0]
        if(convs.length >= 2) big = convs[1]
        if(convs.length >= 3) pallet = convs[2]
      }

      if (small) { packState.value.hasSmall = true; packState.value.smallRate = small.rate; }
      if (big) { packState.value.hasBig = true; packState.value.bigRate = big.rate / (small ? small.rate : 1); }
      if (pallet) { packState.value.hasPallet = true; packState.value.palletRate = pallet.rate / (big ? big.rate : (small ? small.rate : 1)); }
    }
    // Set importPrice, ignore Price
    formData.value = { 
      ...product, 
      importPrice: product.importPrice || 0 
    } 
  } else {
    formData.value = { id: 0, name: '', category: '', brand: '', unit: '', packSize: '', weight: 0, status: 'active', conversions: [], importPrice: 0 }
  }
  showModal.value = true
}

const closeModal = () => showModal.value = false

const handleSubmit = async () => {
  if (!formData.value.category || !formData.value.brand || !formData.value.unit) {
    alert("Vui lòng chọn đầy đủ Danh mục, Thương hiệu, Đơn vị tính cơ bản!"); return;
  }

  let payloadConversions = [];

  if (packState.value.hasSmall && packState.value.smallRate > 0) payloadConversions.push({ altUnit: 'Thùng nhỏ', rate: smallTotal.value })
  if (packState.value.hasBig && packState.value.bigRate > 0) payloadConversions.push({ altUnit: 'Thùng to', rate: bigTotal.value })
  if (packState.value.hasPallet && packState.value.palletRate > 0) payloadConversions.push({ altUnit: 'Pallet', rate: palletTotal.value })

  formData.value.packSize = payloadConversions.map(c => `1 ${c.altUnit}=${c.rate} ${formData.value.unit}`).join(' | ')

  try {
    const method = modalMode.value === 'add' ? 'POST' : 'PUT'
    const url = modalMode.value === 'add' ? API_URL : `${API_URL}/${formData.value.id}`
    const payload = { 
      ...formData.value, 
      conversions: payloadConversions, 
      id: formData.value.id || 0 
    }

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
        alert('Lỗi xóa sản phẩm') 
      }
    } catch (error) { 
      console.error('Lỗi xóa:', error) 
    }
  }
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
        <p class="text-xs md:text-sm text-gray-500 mt-1">Quản lý mã hàng, quy cách đóng gói và khối lượng</p>
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
              <th v-if="canViewPrice" class="px-5 py-3.5 text-right text-xs font-bold text-gray-500 uppercase tracking-wider">Giá Nhập</th>
              <th class="px-5 py-3.5 text-left text-xs font-bold text-gray-500 uppercase tracking-wider">Quy cách Đa cấp & Cân nặng</th>
              <th class="px-5 py-3.5 text-center text-xs font-bold text-gray-500 uppercase tracking-wider">Trạng thái</th>
              <th v-if="canEditProduct" class="px-5 py-3.5 text-right text-xs font-bold text-gray-500 uppercase tracking-wider">Thao tác</th>
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
              
              <td v-if="canViewPrice" class="px-5 py-3 text-right">
                  <span class="text-sm font-bold text-blue-700 bg-blue-50 px-2 py-1 rounded border border-blue-100">
                    {{ formatCurrency(product.importPrice) }}
                  </span>
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
                    <template v-for="(wInfo, idx) in getWeightDisplay(product)" :key="idx">
                      <span :class="{'border-l border-gray-300 pl-2': idx > 0}">
                        <strong :class="wInfo.isMain ? 'text-indigo-700' : 'text-gray-700'">{{ wInfo.name }}:</strong> {{ wInfo.weight }} kg
                      </span>
                    </template>
                  </div>

                </div>
              </td>
              <td class="px-5 py-3 text-center">
                <span :class="product.status === 'active' ? 'bg-green-100 text-green-700 border-green-200' : 'bg-red-100 text-red-700 border-red-200'" class="text-[10px] font-bold px-2 py-1 rounded border uppercase">
                  {{ product.status === 'active' ? 'Đang KD' : 'Ngừng KD' }}
                </span>
              </td>
              
              <td v-if="canEditProduct" class="px-5 py-3 text-right space-x-2">
                <button @click="openModal('edit', product)" class="p-1.5 text-amber-600 hover:bg-amber-50 rounded"><PencilSquareIcon class="w-5 h-5" /></button>
                <button @click="handleDelete(product.id, product.name)" class="p-1.5 text-red-600 hover:bg-red-50 rounded"><TrashIcon class="w-5 h-5" /></button>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>

    <Teleport to="body">
      <div v-if="showModal" class="fixed inset-0 z-50 flex items-center justify-center px-4 bg-slate-900/50 backdrop-blur-sm animate-fade-in">
        <div class="bg-white rounded-xl shadow-2xl w-full max-w-5xl overflow-hidden transform transition-all flex flex-col max-h-[95vh]">
          
          <div class="px-6 py-4 border-b flex items-center justify-between bg-gray-50 shrink-0">
            <h3 class="text-lg font-bold text-gray-800">{{ modalMode === 'add' ? 'Thêm Sản phẩm mới' : 'Cập nhật Sản phẩm' }}</h3>
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
                        <input v-model="formData.name" type="text" required class="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm focus:ring-1 focus:ring-primary-500" placeholder="VD: Nước tăng lực Redbull">
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
                    <p class="text-[10px] text-gray-500 italic mt-1 mb-4">Giá này sẽ tự động được điền khi NV Thu mua lập Phiếu Nhập.</p>
                    
                    <div>
                        <label class="block text-[11px] font-bold mb-1 text-blue-700 uppercase tracking-wide">Đơn giá Nhập (Dự tính)</label>
                        <div class="relative">
                            <input v-model.number="formData.importPrice" type="number" min="0" class="w-full border border-blue-300 rounded-lg pl-3 pr-12 py-2.5 text-base font-bold text-blue-800 focus:ring-1 focus:ring-blue-500 outline-none shadow-inner" placeholder="0">
                            <div class="absolute inset-y-0 right-0 pr-4 flex items-center pointer-events-none text-xs text-blue-500 font-bold">VNĐ</div>
                        </div>
                    </div>
                  </div>

                </div>

                <div class="md:col-span-7 space-y-5 bg-white p-5 rounded-xl border border-gray-200 shadow-sm">
                  <h4 class="text-sm font-bold text-indigo-600 border-b pb-2 flex items-center gap-2">
                    <ScaleIcon class="w-4 h-4"/> {{ canViewPrice ? '3.' : '2.' }} Thiết lập Đóng gói & Quy đổi
                  </h4>
                  
                  <div class="grid grid-cols-2 gap-4">
                    <div>
                      <label class="block text-xs font-bold mb-1 text-gray-700">Đơn vị CƠ BẢN (Nhỏ nhất) <span class="text-red-500">*</span></label>
                      <select v-model="formData.unit" required class="w-full border border-indigo-200 bg-indigo-50/50 rounded-lg px-3 py-2 text-sm cursor-pointer font-bold text-indigo-700 outline-none focus:ring-1 focus:ring-indigo-500">
                        <option value="" disabled selected>VD: Cái, Lon, Gói...</option>
                        <option v-for="u in unitList" :key="u" :value="u">{{ u }}</option>
                      </select>
                      <p class="text-[10px] text-red-500 mt-1 italic font-medium">* Hệ thống dùng ĐVT Cơ bản để chống lệch kho.</p>
                    </div>

                    <div>
                      <label class="block text-xs font-bold mb-1 text-indigo-700">Trọng lượng (kg) / <span class="uppercase border-b border-indigo-700">1 Thùng nhỏ</span></label>
                      <div class="relative">
                        <input v-model.number="formData.weight" type="number" step="0.001" min="0" class="w-full border border-indigo-300 bg-indigo-50/20 rounded-lg pl-3 pr-8 py-2 text-sm focus:ring-1 focus:ring-indigo-500 outline-none font-bold" placeholder="VD: 5.5">
                        <div class="absolute inset-y-0 right-0 pr-3 flex items-center pointer-events-none text-xs text-indigo-500 font-bold">kg</div>
                      </div>
                      <p class="text-[10px] text-gray-500 mt-1 italic font-medium">Hệ thống sẽ tự động nhân lên cho Thùng to / Pallet.</p>
                    </div>
                  </div>
                  
                  <div class="border-t border-gray-200 pt-3">
                    <h5 class="text-[11px] font-bold text-gray-500 uppercase mb-3 flex items-center gap-1">
                      <ArrowsRightLeftIcon class="w-3.5 h-3.5"/> Bảng Quy đổi Đa cấp
                    </h5>

                    <div class="space-y-3">
                      
                      <div class="border rounded-lg p-3 transition-all" :class="packState.hasSmall ? 'border-green-400 bg-green-50/50 shadow-sm' : 'border-gray-200 bg-gray-50/50'">
                        <label class="flex items-center gap-2 cursor-pointer select-none">
                          <input type="checkbox" v-model="packState.hasSmall" class="w-4 h-4 text-green-600 rounded focus:ring-green-500 cursor-pointer">
                          <span class="text-sm font-bold text-gray-800">Thêm Thùng nhỏ (Lốc/Hộp/Vỉ)</span>
                        </label>
                        <div v-if="packState.hasSmall" class="mt-3 flex flex-col gap-2 pl-6 animate-fade-in">
                          <div class="flex items-center gap-2 flex-wrap">
                            <span class="text-sm font-bold text-gray-500">1 Thùng nhỏ chứa</span>
                            <input v-model.number="packState.smallRate" type="number" min="1" placeholder="SL" class="w-20 border border-white rounded px-3 py-2 text-sm text-center font-bold shadow-sm outline-none focus:ring-1 focus:ring-green-500">
                            <span class="text-sm font-semibold text-gray-700">{{ formData.unit || 'SL' }}</span>
                          </div>
                          
                          <div class="text-[12px] font-bold text-orange-600 bg-orange-50 px-3 py-1.5 rounded border border-orange-200 w-fit flex items-center gap-1.5">
                            Quy đổi: <ArrowRightIcon class="w-3.5 h-3.5 text-orange-500" />
                            {{ smallBoxFormula }}
                          </div>
                        </div>
                      </div>

                      <div class="border rounded-lg p-3 transition-all" :class="packState.hasBig ? 'border-blue-400 bg-blue-50/50 shadow-sm' : 'border-gray-200 bg-gray-50/50'">
                        <label class="flex items-center gap-2 cursor-pointer select-none">
                          <input type="checkbox" v-model="packState.hasBig" class="w-4 h-4 text-blue-600 rounded focus:ring-blue-500 cursor-pointer">
                          <span class="text-sm font-bold text-gray-800">Thêm Thùng to (Kiện)</span>
                        </label>
                        <div v-if="packState.hasBig" class="mt-3 flex flex-col gap-2 pl-6 animate-fade-in">
                          <div class="flex items-center gap-2 flex-wrap">
                            <span class="text-sm font-bold text-gray-500">1 Thùng to chứa</span>
                            <input v-model.number="packState.bigRate" type="number" min="1" placeholder="SL" class="w-20 border border-white rounded px-3 py-2 text-sm text-center font-bold shadow-sm outline-none focus:ring-1 focus:ring-blue-500">
                            <span class="text-sm font-semibold text-blue-700">{{ prevForBig }}</span>
                          </div>
                          
                          <div class="text-[12px] font-bold text-orange-600 bg-orange-50 px-3 py-1.5 rounded border border-orange-200 w-fit flex items-center gap-1.5">
                            Quy đổi: <ArrowRightIcon class="w-3.5 h-3.5 text-orange-500" /> 
                            {{ bigBoxFormula }}
                          </div>
                        </div>
                      </div>

                      <div class="border rounded-lg p-3 transition-all" :class="packState.hasPallet ? 'border-indigo-400 bg-indigo-50/50 shadow-sm' : 'border-gray-200 bg-gray-50/50'">
                        <label class="flex items-center gap-2 cursor-pointer select-none">
                          <input type="checkbox" v-model="packState.hasPallet" class="w-4 h-4 text-indigo-600 rounded focus:ring-indigo-500 cursor-pointer">
                          <span class="text-sm font-bold text-gray-800">Cấu hình tải Pallet</span>
                        </label>
                        <div v-if="packState.hasPallet" class="mt-3 flex flex-col gap-2 pl-6 animate-fade-in">
                          <div class="flex items-center gap-2 flex-wrap">
                            <span class="text-sm font-bold text-gray-500">1 Pallet chứa</span>
                            <input v-model.number="packState.palletRate" type="number" min="1" placeholder="SL" class="w-20 border border-white rounded px-3 py-2 text-sm text-center font-bold shadow-sm outline-none focus:ring-1 focus:ring-indigo-500">
                            <span class="text-sm font-semibold text-indigo-700">{{ prevForPallet }}</span>
                          </div>
                          
                          <div class="text-[12px] font-bold text-orange-600 bg-orange-50 px-3 py-1.5 rounded border border-orange-200 w-fit flex items-center gap-1.5">
                            Quy đổi: <ArrowRightIcon class="w-3.5 h-3.5 text-orange-500" />
                            {{ palletFormula }}
                          </div>
                        </div>
                      </div>

                    </div>
                  </div>

                </div>
              </div>

            </form>
          </div>

          <div class="px-6 py-4 border-t flex justify-end gap-3 shrink-0 bg-white shadow-inner">
            <button type="button" @click="closeModal" class="px-4 py-2 border border-gray-300 rounded-lg text-sm font-semibold hover:bg-gray-50">Hủy bỏ</button>
            <button type="submit" form="productForm" class="px-4 py-2 bg-primary-600 text-white rounded-lg text-sm font-semibold hover:bg-primary-700 shadow-sm flex items-center gap-2">
              Lưu Sản phẩm
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
.custom-scrollbar::-webkit-scrollbar-thumb:hover { background: #94a3b8; }
.animate-fade-in { animation: fadeIn 0.15s ease-out; }
@keyframes fadeIn { from { opacity: 0; transform: translateY(2px); } to { opacity: 1; transform: translateY(0); } }
</style>