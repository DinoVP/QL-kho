<script setup>
import { useRouter } from 'vue-router'
import { 
  ArrowRightOnRectangleIcon, UserGroupIcon, BuildingOfficeIcon, ClipboardDocumentListIcon,
  ListBulletIcon, CubeIcon, Squares2X2Icon, IdentificationIcon,
  ArrowDownTrayIcon, ArrowUpTrayIcon, TruckIcon, ExclamationTriangleIcon, ShoppingCartIcon, ClipboardDocumentCheckIcon,
  ArchiveBoxIcon, DocumentTextIcon, BellAlertIcon, MapPinIcon,
  ChartPieIcon, ChartBarIcon, ComputerDesktopIcon,
  EllipsisVerticalIcon
} from '@heroicons/vue/24/solid'

const router = useRouter()

// Đã cập nhật FULL 20 đường dẫn (path) đồng bộ với Menu trái
const dashboardGroups = [
  {
    title: 'A — Hệ thống & phân quyền',
    items: [
      { name: 'Đăng xuất', icon: ArrowRightOnRectangleIcon, color: 'text-red-500', path: '/login' },
      { name: 'Nhân sự & Phân quyền', icon: UserGroupIcon, color: 'text-blue-600', path: '/employees' },
      { name: 'Chi nhánh & Kho', icon: BuildingOfficeIcon, color: 'text-indigo-500', path: '/branches' },
      { name: 'Nhật ký hệ thống', icon: ClipboardDocumentListIcon, color: 'text-gray-600', path: '/audit-logs' },
    ]
  },
  {
    title: 'B — Danh mục & cấu hình',
    items: [
      { name: 'Danh mục chung', icon: ListBulletIcon, color: 'text-purple-500', path: '/categories' },
      { name: 'Sản phẩm (SKU)', icon: CubeIcon, color: 'text-blue-500', path: '/products' },
      { name: 'Sơ đồ kho', icon: Squares2X2Icon, color: 'text-cyan-600', path: '/warehouse-map' },
      { name: 'Đối tác (KH/NCC)', icon: IdentificationIcon, color: 'text-orange-500', path: '/partners' },
    ]
  },
  {
    title: 'C — Nghiệp vụ kho',
    items: [
      { name: 'Phiếu Nhập', icon: ArrowDownTrayIcon, color: 'text-blue-600', path: '/inbound' },
      { name: 'Phiếu Xuất', icon: ArrowUpTrayIcon, color: 'text-orange-500', path: '/outbound' },
      { name: 'Điều chuyển', icon: TruckIcon, color: 'text-indigo-500', path: '/transfer' },
      { name: 'Hàng lỗi', icon: ExclamationTriangleIcon, color: 'text-red-500', path: '/defects' },
      { name: 'Đặt hàng PO', icon: ShoppingCartIcon, color: 'text-amber-500', path: '/purchase-orders' },
      { name: 'Kiểm kê', icon: ClipboardDocumentCheckIcon, color: 'text-teal-600', path: '/inventory-check' },
    ]
  },
  {
    title: 'D — Tồn kho & kiểm soát',
    items: [
      { name: 'Tra cứu Tồn kho', icon: ArchiveBoxIcon, color: 'text-blue-600', path: '/stock' },
      { name: 'Sổ giao dịch', icon: DocumentTextIcon, color: 'text-indigo-500', path: '/transactions' },
      { name: 'Cảnh báo tồn kho', icon: BellAlertIcon, color: 'text-red-500', path: '/alerts' },
      { name: 'Vị trí lưu kho', icon: MapPinIcon, color: 'text-cyan-500', path: '/locations' },
    ]
  },
  {
    title: 'E — Phân tích & báo cáo',
    items: [
      { name: 'Dashboard Tổng quan', icon: ChartPieIcon, color: 'text-blue-500', path: '/dashboard' },
      { name: 'Báo cáo Xuất/Nhập', icon: ChartBarIcon, color: 'text-green-500', path: '/reports' },
      { name: 'Nhật ký UI', icon: ComputerDesktopIcon, color: 'text-gray-600', path: '/audit-logs' },
    ]
  }
]

// Hàm xử lý click chuyển trang
const handleCardClick = (path) => {
  if (path !== '#') {
    router.push(path)
  }
}
</script>

<template>
  <div class="space-y-6 md:space-y-8 animate-fade-in pb-12 px-0 md:px-2">
    
    <div v-for="(group, index) in dashboardGroups" :key="index">
      
      <div class="flex items-center gap-2 mb-3 md:mb-4">
        <h3 class="text-xs md:text-sm font-bold text-primary-800 tracking-wide">{{ group.title }}</h3>
        <span class="bg-gray-200 text-gray-600 text-xs px-1.5 py-0.5 rounded font-medium shadow-inner">{{ group.items.length }}</span>
      </div>

      <div class="grid grid-cols-1 sm:grid-cols-2 md:grid-cols-3 lg:grid-cols-4 xl:grid-cols-5 gap-3 md:gap-4">
        
        <div 
          v-for="item in group.items" :key="item.name"
          @click="handleCardClick(item.path)"
          class="bg-white border border-gray-200 rounded-lg p-3 md:p-3.5 flex items-center justify-between cursor-pointer shadow-sm hover:shadow-md hover:border-primary-300 transition-all active:scale-95 group"
        >
          <div class="flex items-center gap-3">
            <div class="p-2 rounded-lg bg-gray-50 group-hover:bg-primary-50 transition-colors">
              <component :is="item.icon" :class="[item.color, 'w-5 h-5 md:w-6 md:h-6']" />
            </div>
            <span class="text-sm font-semibold text-gray-700 group-hover:text-primary-700 transition-colors">{{ item.name }}</span>
          </div>
          
          <button class="text-gray-300 hover:text-gray-500 p-1 rounded hover:bg-gray-100 transition-colors">
            <EllipsisVerticalIcon class="w-5 h-5" />
          </button>
        </div>

      </div>
    </div>
  </div>
</template>