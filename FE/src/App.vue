<script setup>
import { computed } from "vue";
import { RouterView, useRoute, useRouter } from "vue-router";
import Sidebar from "./components/Sidebar.vue";
import {
  UserCircleIcon,
  BellIcon,
  ArrowRightOnRectangleIcon,
} from "@heroicons/vue/24/outline";
import { useAuth } from "./composables/useAuth"; // Gọi đồ nghề lấy thông tin Đăng nhập

const route = useRoute();
const router = useRouter();

// Rút dữ liệu từ hệ thống Auth ra
const { currentUsername, currentUserRole, logout } = useAuth();

const isLoginPage = computed(() => route.path === "/login");

// Tự động dịch mã Role (lưu trong localStorage) sang Tiếng Việt hiển thị cho xịn
const roleDisplayName = computed(() => {
  switch (currentUserRole.value) {
    case 'admin': return 'Quản trị viên';
    case 'giam_doc': return 'Giám đốc công ty';
    case 'gd_chi_nhanh': return 'GĐ Chi nhánh';
    case 'ql_kho': return 'Quản lý kho';
    case 'nv_kho': return 'Nhân viên kho';
    default: return 'Khách viếng thăm';
  }
});

// Xử lý nút Đăng xuất cực mượt
const handleLogout = () => {
  if (confirm("Sếp muốn đăng xuất khỏi hệ thống chứ?")) {
    logout(); // Xóa sạch Token trong máy
    router.push("/login"); // Đá văng về trang Login
  }
};
</script>

<template>
  <template v-if="isLoginPage">
    <RouterView />
  </template>

  <div v-else class="flex h-screen w-full bg-gray-50 font-sans text-gray-900 overflow-hidden">
    <Sidebar />

    <div class="flex-1 flex flex-col h-screen overflow-hidden">
      <header class="h-16 bg-white border-b border-gray-200 flex items-center justify-between px-4 md:px-6 shrink-0 z-10">
        
        <div class="flex items-center text-gray-800 font-bold tracking-wider"></div>

        <div class="hidden md:block flex-1 max-w-xl mx-8">
          <div class="relative">
            <div class="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none">
              <svg class="w-4 h-4 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z"></path>
              </svg>
            </div>
            <input
              type="text"
              class="block w-full pl-10 pr-3 py-2 border border-gray-200 rounded-md text-sm text-gray-900 placeholder-gray-400 bg-gray-50 focus:bg-white focus:border-primary-500 focus:ring-1 focus:ring-primary-500 outline-none transition-all"
              placeholder="Tìm kiếm Menu..."
            />
          </div>
        </div>

        <div class="flex items-center gap-2 md:gap-6 ml-auto">
          <button class="text-gray-400 hover:text-primary-600 transition-colors p-2">
            <BellIcon class="w-6 h-6" />
          </button>

          <div class="flex items-center gap-2 md:gap-4 md:pl-6 border-l border-gray-200 pl-2">
            <div class="hidden md:block text-right text-sm">
              <div class="font-bold text-gray-800 capitalize">{{ currentUsername || 'Khách' }}</div>
              <div class="text-gray-500 text-xs font-medium">{{ roleDisplayName }}</div>
            </div>
            
            <UserCircleIcon class="w-8 h-8 md:w-10 md:h-10 text-gray-400" />

            <button
              @click="handleLogout"
              title="Đăng xuất"
              class="text-gray-400 hover:text-red-600 p-1.5 rounded-md hover:bg-red-50 transition-colors"
            >
              <ArrowRightOnRectangleIcon class="w-5 h-5 md:w-6 md:h-6" />
            </button>
          </div>
        </div>
      </header>

      <main class="flex-1 p-4 md:p-8 overflow-y-auto">
        <RouterView />
      </main>
      
    </div>
  </div>
</template>