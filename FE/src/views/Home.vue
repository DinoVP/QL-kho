<script setup>
import { computed } from 'vue'
import { useRouter } from 'vue-router'
import { useAuth } from '../composables/useAuth'
import { 
  ArrowRightOnRectangleIcon, UserGroupIcon, BuildingOfficeIcon, ClipboardDocumentListIcon,
  ListBulletIcon, CubeIcon, Squares2X2Icon, IdentificationIcon,
  ArrowDownTrayIcon, ArrowUpTrayIcon, TruckIcon, ExclamationTriangleIcon, ShoppingCartIcon, ClipboardDocumentCheckIcon,
  ArchiveBoxIcon, DocumentTextIcon, BellAlertIcon, MapPinIcon,
  ChartPieIcon, ChartBarIcon, ComputerDesktopIcon,
  EllipsisVerticalIcon, CheckBadgeIcon
} from '@heroicons/vue/24/solid' // Thêm CheckBadgeIcon solid

const router = useRouter()
const { currentUserRole, logout } = useAuth()

const dashboardGroups = [
  {
    title: 'A — Hệ thống & phân quyền',
    items: [
      // ĐÃ THÊM: nv_thu_mua
      { name: 'Đăng xuất', icon: ArrowRightOnRectangleIcon, color: 'text-red-500', path: '/login', roles: ['admin', 'giam_doc', 'gd_chi_nhanh', 'ql_kho', 'nv_kho', 'nv_thu_mua'] },
      { name: 'Nhân sự & Phân quyền', icon: UserGroupIcon, color: 'text-blue-600', path: '/employees', roles: ['admin', 'giam_doc', 'gd_chi_nhanh', 'ql_kho'] },
      { name: 'Chi nhánh & Kho', icon: BuildingOfficeIcon, color: 'text-indigo-500', path: '/branches', roles: ['admin', 'giam_doc', 'gd_chi_nhanh'] },
      { name: 'Nhật ký hệ thống', icon: ClipboardDocumentListIcon, color: 'text-gray-600', path: '/audit-logs', roles: ['admin'] },
    ]
  },
  {
    title: 'B — Danh mục & cấu hình',
    items: [
      // ĐÃ THÊM: nv_thu_mua
      { name: 'Danh mục chung', icon: ListBulletIcon, color: 'text-purple-500', path: '/categories', roles: ['admin', 'giam_doc', 'gd_chi_nhanh', 'ql_kho', 'nv_thu_mua'] },
      { name: 'Sản phẩm (SKU)', icon: CubeIcon, color: 'text-blue-500', path: '/products', roles: ['admin', 'giam_doc', 'gd_chi_nhanh', 'ql_kho', 'nv_thu_mua'] },
      { name: 'Sơ đồ kho', icon: Squares2X2Icon, color: 'text-cyan-600', path: '/warehouse-map', roles: ['admin', 'ql_kho'] },
      { name: 'Đối tác (KH/NCC)', icon: IdentificationIcon, color: 'text-orange-500', path: '/partners', roles: ['admin', 'giam_doc', 'gd_chi_nhanh', 'ql_kho', 'nv_thu_mua'] },
    ]
  },
  {
    title: 'C — Nghiệp vụ kho',
    items: [
      { name: 'Duyệt phiếu tổng', icon: CheckBadgeIcon, color: 'text-green-600', path: '/approvals', roles: ['admin', 'giam_doc', 'gd_chi_nhanh'] },
      { name: 'Phiếu Nhập', icon: ArrowDownTrayIcon, color: 'text-blue-600', path: '/inbound', roles: ['admin', 'ql_kho'] },
      { name: 'Phiếu Xuất', icon: ArrowUpTrayIcon, color: 'text-orange-500', path: '/outbound', roles: ['admin', 'ql_kho'] },
      { name: 'Điều chuyển', icon: TruckIcon, color: 'text-indigo-500', path: '/transfer', roles: ['admin', 'giam_doc', 'gd_chi_nhanh', 'ql_kho'] },
      { name: 'Hàng lỗi', icon: ExclamationTriangleIcon, color: 'text-red-500', path: '/defects', roles: ['admin', 'giam_doc', 'gd_chi_nhanh', 'ql_kho', 'nv_kho'] },
      // ĐÃ THÊM: nv_thu_mua
      { name: 'Đặt hàng PO', icon: ShoppingCartIcon, color: 'text-amber-500', path: '/purchase-orders', roles: ['admin', 'giam_doc', 'ql_kho', 'nv_thu_mua'] },
      { name: 'Kiểm kê', icon: ClipboardDocumentCheckIcon, color: 'text-teal-600', path: '/inventory-check', roles: ['admin', 'giam_doc', 'gd_chi_nhanh', 'ql_kho'] },
    ]
  },
  {
    title: 'D — Tồn kho & kiểm soát',
    items: [
      // ĐÃ THÊM: nv_thu_mua
      { name: 'Tra cứu Tồn kho', icon: ArchiveBoxIcon, color: 'text-blue-600', path: '/stock', roles: ['admin', 'ql_kho', 'nv_kho', 'nv_thu_mua'] },
      { name: 'Sổ giao dịch', icon: DocumentTextIcon, color: 'text-indigo-500', path: '/transactions', roles: ['admin', 'giam_doc', 'gd_chi_nhanh', 'ql_kho'] },
      { name: 'Cảnh báo tồn kho', icon: BellAlertIcon, color: 'text-red-500', path: '/alerts', roles: ['admin', 'ql_kho', 'nv_thu_mua'] },
      { name: 'Vị trí lưu kho', icon: MapPinIcon, color: 'text-cyan-500', path: '/locations', roles: ['admin', 'ql_kho', 'nv_kho'] },
    ]
  },
  {
    title: 'E — Phân tích & báo cáo',
    items: [
      { name: 'Dashboard Tổng quan', icon: ChartPieIcon, color: 'text-blue-500', path: '/dashboard', roles: ['admin', 'giam_doc', 'gd_chi_nhanh'] },
      { name: 'Báo cáo Xuất/Nhập', icon: ChartBarIcon, color: 'text-green-500', path: '/reports', roles: ['admin', 'giam_doc', 'gd_chi_nhanh', 'ql_kho'] },
      { name: 'Nhật ký UI', icon: ComputerDesktopIcon, color: 'text-gray-600', path: '/ui-logs', roles: ['admin'] },
    ]
  }
]

const filteredDashboardGroups = computed(() => {
  if (!currentUserRole.value) return []
  return dashboardGroups.map(group => {
    const allowedItems = group.items.filter(item => item.roles.includes(currentUserRole.value))
    return { ...group, items: allowedItems }
  }).filter(group => group.items.length > 0)
})

const handleCardClick = (path) => {
  if (path === '/login') {
    if(confirm('Sếp muốn thoát khỏi hệ thống?')) {
      logout()
      router.push('/login')
    }
  } else if (path !== '#') {
    router.push(path)
  }
}
</script>

<template>
  <div class="space-y-6 md:space-y-8 animate-fade-in pb-12 px-0 md:px-2">
    <div v-for="(group, index) in filteredDashboardGroups" :key="index">
      
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