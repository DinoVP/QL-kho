import { createRouter, createWebHistory } from "vue-router";
import Login from "../views/Login.vue";
import Home from "../views/Home.vue";
import Employee from "../views/Employee.vue";
import Branch from "../views/Branch.vue";
import AuditLog from "../views/AuditLog.vue";
import Category from "../views/Category.vue";
import Product from "../views/Product.vue";
import WarehouseMap from "../views/WarehouseMap.vue";
import Partner from "../views/Partner.vue";

// Import đủ 11 file mới
import Inbound from "../views/Inbound.vue";
import Outbound from "../views/Outbound.vue";
import Transfer from "../views/Transfer.vue";
import Defect from "../views/Defect.vue";
import PurchaseOrder from "../views/PurchaseOrder.vue";
import InventoryCheck from "../views/InventoryCheck.vue";
import Stock from "../views/Stock.vue";
import Transaction from "../views/Transaction.vue";
import Alerts from "../views/Alerts.vue";
import Locations from "../views/Locations.vue";
import Dashboard from "../views/Dashboard.vue";
import Reports from "../views/Reports.vue";

// IMPORT THÊM FILE CHO CÁC MENU MỚI TRONG SIDEBAR
import Approval from "../views/Approval.vue";
import UiLog from "../views/UiLog.vue";

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    { path: "/", redirect: "/login" },
    { path: "/login", name: "login", component: Login },
    { path: "/home", name: "home", component: Home },
    { path: "/employees", name: "employees", component: Employee },
    { path: "/branches", name: "branches", component: Branch },
    { path: "/audit-logs", name: "audit-logs", component: AuditLog },
    { path: "/categories", name: "categories", component: Category },
    { path: "/products", name: "products", component: Product },
    { path: "/warehouse-map", name: "warehouse-map", component: WarehouseMap },
    { path: "/partners", name: "partners", component: Partner },

    // Các Route mới
    { path: "/inbound", name: "inbound", component: Inbound },
    { path: "/outbound", name: "outbound", component: Outbound },
    { path: "/transfer", name: "transfer", component: Transfer },
    { path: "/defects", name: "defects", component: Defect },
    {
      path: "/purchase-orders",
      name: "purchase-orders",
      component: PurchaseOrder,
    },
    {
      path: "/inventory-check",
      name: "inventory-check",
      component: InventoryCheck,
    },
    { path: "/stock", name: "stock", component: Stock },
    { path: "/transactions", name: "transactions", component: Transaction },
    { path: "/alerts", name: "alerts", component: Alerts },
    { path: "/locations", name: "locations", component: Locations },
    { path: "/dashboard", name: "dashboard", component: Dashboard },
    { path: "/reports", name: "reports", component: Reports },

    // KHAI BÁO ROUTE CHO NHẬT KÝ UI VÀ DUYỆT PHIẾU
    { path: "/approvals", name: "approvals", component: Approval },
    { path: "/ui-logs", name: "ui-logs", component: UiLog },
  ],
});

export default router;
