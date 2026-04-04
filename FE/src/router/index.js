import { createRouter, createWebHistory } from "vue-router";
import { uiLogger } from "../utils/logger"; // <-- Kết nối file logger

import Login from "../views/Login.vue";
import Home from "../views/Home.vue";
import Employee from "../views/Employee.vue";
import Branch from "../views/Branch.vue";
import AuditLog from "../views/AuditLog.vue";
import Category from "../views/Category.vue";
import Product from "../views/Product.vue";
import WarehouseMap from "../views/WarehouseMap.vue";
import Partner from "../views/Partner.vue";

import Inbound from "../views/Inbound.vue";
import Outbound from "../views/Outbound.vue";
import Transfer from "../views/Transfer.vue";
import Putaway from "../views/Putaway.vue"; // <-- THÊM COMPONENT MỚI
import Defect from "../views/Defect.vue";
import PurchaseOrder from "../views/PurchaseOrder.vue";
import InventoryCheck from "../views/InventoryCheck.vue";
import Stock from "../views/Stock.vue";
import Transaction from "../views/Transaction.vue";
import Alerts from "../views/Alerts.vue";
import Locations from "../views/Locations.vue";
import Dashboard from "../views/Dashboard.vue";
import Reports from "../views/Reports.vue";

import Approval from "../views/Approval.vue";
import UiLog from "../views/UiLog.vue";

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    { path: "/", redirect: "/login" },
    {
      path: "/login",
      name: "login",
      component: Login,
      meta: { title: "Đăng nhập" },
    },
    {
      path: "/home",
      name: "home",
      component: Home,
      meta: { title: "Trang chủ" },
    },
    {
      path: "/employees",
      name: "employees",
      component: Employee,
      meta: { title: "Nhân sự & Phân quyền" },
    },
    {
      path: "/branches",
      name: "branches",
      component: Branch,
      meta: { title: "Chi nhánh & Kho" },
    },
    {
      path: "/audit-logs",
      name: "audit-logs",
      component: AuditLog,
      meta: { title: "Nhật ký hệ thống" },
    },
    {
      path: "/categories",
      name: "categories",
      component: Category,
      meta: { title: "Danh mục chung" },
    },
    {
      path: "/products",
      name: "products",
      component: Product,
      meta: { title: "Sản phẩm (SKU)" },
    },
    {
      path: "/warehouse-map",
      name: "warehouse-map",
      component: WarehouseMap,
      meta: { title: "Sơ đồ kho" },
    },
    {
      path: "/partners",
      name: "partners",
      component: Partner,
      meta: { title: "Đối tác (KH/NCC)" },
    },

    {
      path: "/inbound",
      name: "inbound",
      component: Inbound,
      meta: { title: "Phiếu nhập" },
    },
    {
      path: "/outbound",
      name: "outbound",
      component: Outbound,
      meta: { title: "Phiếu xuất" },
    },
    {
      path: "/transfer",
      name: "transfer",
      component: Transfer,
      meta: { title: "Điều chuyển" },
    },
    {
      path: "/putaway",
      name: "putaway",
      component: Putaway,
      meta: { title: "Kho bãi" }, 
    },
    {
      path: "/defects",
      name: "defects",
      component: Defect,
      meta: { title: "Hàng lỗi" },
    },
    {
      path: "/purchase-orders",
      name: "purchase-orders",
      component: PurchaseOrder,
      meta: { title: "Đặt hàng PO" },
    },
    {
      path: "/inventory-check",
      name: "inventory-check",
      component: InventoryCheck,
      meta: { title: "Kiểm kê" },
    },
    {
      path: "/stock",
      name: "stock",
      component: Stock,
      meta: { title: "Tra cứu tồn kho" },
    },
    {
      path: "/transactions",
      name: "transactions",
      component: Transaction,
      meta: { title: "Sổ giao dịch" },
    },
    {
      path: "/alerts",
      name: "alerts",
      component: Alerts,
      meta: { title: "Cảnh báo tồn kho" },
    },
    {
      path: "/locations",
      name: "locations",
      component: Locations,
      meta: { title: "Vị trí lưu kho" },
    },
    {
      path: "/dashboard",
      name: "dashboard",
      component: Dashboard,
      meta: { title: "Dashboard (Biểu đồ)" },
    },
    {
      path: "/reports",
      name: "reports",
      component: Reports,
      meta: { title: "Báo cáo xuất/nhập" },
    },

    {
      path: "/approvals",
      name: "approvals",
      component: Approval,
      meta: { title: "Duyệt phiếu tổng" },
    },
    {
      path: "/ui-logs",
      name: "ui-logs",
      component: UiLog,
      meta: { title: "Nhật ký UI" },
    },
  ],
});

// === AUTO BẮT SỰ KIỆN KHI CHUYỂN TRANG ===
router.afterEach((to, from) => {
  // Bỏ qua nếu F5 tại chỗ (đường dẫn không đổi)
  if (to.path === from.path) return;

  // Lấy Tên Tiếng Việt từ thuộc tính 'meta', nếu không có thì để là 'bên ngoài'
  const fromName = from.meta?.title || "Bên ngoài";
  const toName = to.meta?.title || "Không xác định";

  // Nối chuỗi đúng chuẩn tiếng Việt theo ý bạn (dùng toLowerCase để chữ thường dễ đọc)
  const message = `Người dùng điều hướng từ trang ${fromName.toLowerCase()} sang trang ${toName.toLowerCase()}`;

  // Gọi hàm log bắn xuống Backend
  uiLogger.log("NAVIGATION", to.path, message, {
    fromPath: from.path,
    toPath: to.path,
    fromMenu: from.meta?.title,
    toMenu: to.meta?.title,
  });
});
// =========================================

export default router;
