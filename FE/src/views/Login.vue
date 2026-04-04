<script setup>
import { ref } from 'vue'
import { useRouter } from 'vue-router' // Dùng để chuyển trang
import { useAuth } from '../composables/useAuth'

const router = useRouter()
const { login } = useAuth()

const username = ref('')
const password = ref('')
const isLoading = ref(false)
const errorMessage = ref('')

const handleLogin = async () => {
  if (!username.value || !password.value) {
    errorMessage.value = "Vui lòng nhập đủ thông tin!"
    return
  }

  isLoading.value = true
  errorMessage.value = ''

  // Gọi hàm login từ useAuth (Bắn API sang C#)
  const result = await login(username.value, password.value)

  if (result.success) {
    // =========================================================================
    // MẤU CHỐT Ở ĐÂY: LƯU MÃ KHO VÀ CHI NHÁNH VÀO BỘ NHỚ TRÌNH DUYỆT
    // =========================================================================
    // Dữ liệu API trả về thường nằm trong result.data hoặc result.user
    const userData = result.data || result.user; 
    
    if (userData) {
        // Lưu Role phân quyền
        if (userData.roleCode || userData.RoleCode) {
            localStorage.setItem('role', userData.roleCode || userData.RoleCode);
        }
        // Lưu ID Chi nhánh
        if (userData.branchId || userData.BranchId) {
            localStorage.setItem('branchId', userData.branchId || userData.BranchId);
        }
        // Lưu ID Kho (QUAN TRỌNG NHẤT ĐỂ TÁCH DỮ LIỆU)
        if (userData.warehouseId || userData.WarehouseId) {
            localStorage.setItem('warehouseId', userData.warehouseId || userData.WarehouseId);
        }
    }
    // =========================================================================

    alert("Đăng nhập thành công")
    // Đẩy sang trang Phiếu Nhập (Hoặc trang Trang chủ /home của sếp)
    router.push('/inbound') 
  } else {
    errorMessage.value = result.message || "Tài khoản hoặc mật khẩu không đúng!"
  }
  
  isLoading.value = false
}
</script>

<template>
  <div class="min-h-screen flex items-center justify-center bg-slate-100">
    <div class="bg-white p-8 rounded-xl shadow-lg max-w-sm w-full">
      <div class="text-center mb-8">
        <h1 class="text-2xl font-bold text-slate-800">HỆ THỐNG WMS</h1>
        <p class="text-sm text-slate-500 mt-1">Đăng nhập để vào kho</p>
      </div>

      <form @submit.prevent="handleLogin" class="space-y-5">
        <div>
          <label class="block text-sm font-bold text-slate-700 mb-1">Tên đăng nhập</label>
          <input v-model="username" type="text" required class="w-full px-4 py-2 border rounded-lg outline-none focus:ring-2 focus:ring-primary-500" placeholder="VD: qlkho, nvkho, giamdoc...">
        </div>
        
        <div>
          <label class="block text-sm font-bold text-slate-700 mb-1">Mật khẩu</label>
          <input v-model="password" type="password" required class="w-full px-4 py-2 border rounded-lg outline-none focus:ring-2 focus:ring-primary-500" placeholder="******">
        </div>

        <div v-if="errorMessage" class="text-sm text-red-600 bg-red-50 p-3 rounded-lg border border-red-100">
          {{ errorMessage }}
        </div>

        <button type="submit" :disabled="isLoading" class="w-full bg-primary-600 hover:bg-primary-700 text-white font-bold py-2.5 rounded-lg transition-colors disabled:opacity-50">
          {{ isLoading ? 'Đang kiểm tra...' : 'ĐĂNG NHẬP' }}
        </button>
      </form>
    </div>
  </div>
</template>