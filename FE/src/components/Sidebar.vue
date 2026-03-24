<script setup>
import { ref, onMounted, onUnmounted } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import { 
  HomeIcon, UserGroupIcon, BuildingOfficeIcon, ClipboardDocumentListIcon,
  ListBulletIcon, CubeIcon, Squares2X2Icon, IdentificationIcon,
  ArrowDownTrayIcon, ArrowUpTrayIcon, TruckIcon, ExclamationTriangleIcon, ShoppingCartIcon,
  ClipboardDocumentCheckIcon, ArchiveBoxIcon, DocumentTextIcon, BellAlertIcon, MapPinIcon,
  ChartPieIcon, ChartBarIcon, Bars3Icon 
} from '@heroicons/vue/24/outline'

const router = useRouter()
const route = useRoute()

// Tự động thu gọn trên màn hình nhỏ (iPad/Mobile)
const isCollapsed = ref(window.innerWidth < 1024)

const handleResize = () => {
  isCollapsed.value = window.innerWidth < 1024
}

onMounted(() => window.addEventListener('resize', handleResize))
onUnmounted(() => window.removeEventListener('resize', handleResize))

// Đã đổi TOÀN BỘ path sang tiếng Anh khớp với file Router
const menuItems = [
  { name: 'Trang chủ', path: '/home', icon: HomeIcon },
  { name: 'Nhân sự & Phân quyền', path: '/employees', icon: UserGroupIcon },
  { name: 'Chi nhánh & Kho', path: '/branches', icon: BuildingOfficeIcon },
  { name: 'Nhật ký hệ thống', path: '/audit-logs', icon: ClipboardDocumentListIcon },
  
  { name: 'Danh mục chung', path: '/categories', icon: ListBulletIcon },
  { name: 'Sản phẩm (SKU)', path: '/products', icon: CubeIcon },
  { name: 'Sơ đồ kho', path: '/warehouse-map', icon: Squares2X2Icon },
  { name: 'Đối tác (KH/NCC)', path: '/partners', icon: IdentificationIcon },
  
  { name: 'Phiếu Nhập', path: '/inbound', icon: ArrowDownTrayIcon },
  { name: 'Phiếu Xuất', path: '/outbound', icon: ArrowUpTrayIcon },
  { name: 'Điều chuyển', path: '/transfer', icon: TruckIcon },
  { name: 'Hàng lỗi', path: '/defects', icon: ExclamationTriangleIcon },
  { name: 'Đặt hàng PO', path: '/purchase-orders', icon: ShoppingCartIcon },
  { name: 'Kiểm kê', path: '/inventory-check', icon: ClipboardDocumentCheckIcon },
  
  { name: 'Tra cứu Tồn kho', path: '/stock', icon: ArchiveBoxIcon },
  { name: 'Sổ giao dịch', path: '/transactions', icon: DocumentTextIcon },
  { name: 'Cảnh báo tồn kho', path: '/alerts', icon: BellAlertIcon },
  { name: 'Vị trí lưu kho', path: '/locations', icon: MapPinIcon },
  
  { name: 'Dashboard (Biểu đồ)', path: '/dashboard', icon: ChartPieIcon },
  { name: 'Báo cáo Xuất/Nhập', path: '/reports', icon: ChartBarIcon },
]

// Hàm chuyển trang
const goTo = (path) => {
  router.push(path)
}
</script>

<template>
  <aside 
    class="bg-sidebar-bg text-sidebar-text flex flex-col h-screen border-r border-primary-800 shrink-0 transition-all duration-300 ease-in-out relative z-20 shadow-xl" 
    :class="isCollapsed ? 'w-20' : 'w-72'"
  >
    
    <div class="h-16 flex items-center border-b border-primary-800 px-4" :class="isCollapsed ? 'justify-center' : 'justify-between'">
      <div v-if="!isCollapsed" class="flex items-center gap-2 overflow-hidden whitespace-nowrap cursor-pointer hover:opacity-80" @click="goTo('/home')">
        <CubeIcon class="w-8 h-8 text-primary-200 shrink-0" />
        <span class="text-2xl font-bold text-white tracking-wider">QL KHO</span>
      </div>
      <button @click="isCollapsed = !isCollapsed" class="p-2 rounded-lg hover:bg-primary-800 text-primary-200 transition-colors" title="Thu gọn / Mở rộng">
        <Bars3Icon class="w-6 h-6" />
      </button>
    </div>

    <nav class="flex-1 overflow-y-auto py-4 space-y-1 overflow-x-hidden custom-scrollbar">
      <button 
        v-for="item in menuItems" :key="item.name"
        @click="goTo(item.path)"
        :title="isCollapsed ? item.name : ''"
        :class="[
          'w-full flex items-center transition-colors duration-200 cursor-pointer',
          isCollapsed ? 'justify-center py-3 px-2' : 'py-3 px-6',
          route.path === item.path 
            ? 'bg-primary-600 text-white border-l-4 border-white'
            : 'text-primary-100 hover:bg-sidebar-hover hover:text-white border-l-4 border-transparent'
        ]"
      >
        <component :is="item.icon" class="w-6 h-6 shrink-0" :class="isCollapsed ? '' : 'mr-4'" />
        <span v-if="!isCollapsed" class="text-sm font-medium whitespace-nowrap">{{ item.name }}</span>
      </button>
    </nav>
  </aside>
</template>

<style scoped>
.custom-scrollbar::-webkit-scrollbar { width: 4px; }
.custom-scrollbar::-webkit-scrollbar-track { background: transparent; }
.custom-scrollbar::-webkit-scrollbar-thumb { background: #075985; border-radius: 10px; }
.custom-scrollbar:hover::-webkit-scrollbar-thumb { background: #0284c7; }
</style>