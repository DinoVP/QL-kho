import { ref, computed } from "vue";
import axios from "axios";

// Lấy dữ liệu từ bộ nhớ
const currentUserRole = ref(
  localStorage.getItem("role") || localStorage.getItem("userRole") || null,
);
const authToken = ref(localStorage.getItem("authToken") || null);
const currentUsername = ref(localStorage.getItem("username") || null);

// Cấu hình API
const api = axios.create({
  baseURL: "https://localhost:7139/api",
  headers: { "Content-Type": "application/json" },
});

if (authToken.value) {
  api.defaults.headers.common["Authorization"] = `Bearer ${authToken.value}`;
}

export function useAuth() {
  const login = async (username, password) => {
    try {
      const response = await api.post("/Auth/login", { username, password });
      const data = response.data;

      // 1. Lưu Token tạm thời để gọi các API khác
      localStorage.setItem("authToken", data.token);
      localStorage.setItem("username", username);
      api.defaults.headers.common["Authorization"] = `Bearer ${data.token}`;

      let role = data.role || data.Role || data.roleCode || data.RoleCode;
      let branchId = data.branchId || data.BranchId;
      let warehouseId = data.warehouseId || data.WarehouseId;

      // =========================================================================
      // 2. BẢO HIỂM 100%: NẾU API LOGIN KHÔNG TRẢ VỀ MÃ KHO -> TỰ ĐỘNG ĐI TÌM!
      // =========================================================================
      if (!warehouseId || !branchId) {
        try {
          // Gọi API Employees để lấy hồ sơ của chính user này
          const empRes = await api.get("/Employees");
          const myProfile = empRes.data.find(
            (e) => e.username === username || e.Username === username,
          );

          if (myProfile) {
            role = role || myProfile.roleCode || myProfile.RoleCode;
            branchId = branchId || myProfile.branchId || myProfile.BranchId;
            warehouseId =
              warehouseId || myProfile.warehouseId || myProfile.WarehouseId;
          }
        } catch (err) {
          console.log("Không thể lấy thông tin chi nhánh/kho phụ", err);
        }
      }
      // =========================================================================

      // 3. Xóa data cũ và lưu data mới chuẩn xác vào Trình duyệt
      localStorage.removeItem("warehouseId");
      localStorage.removeItem("branchId");

      localStorage.setItem("userRole", role);
      localStorage.setItem("role", role);
      if (branchId) localStorage.setItem("branchId", branchId);
      if (warehouseId) localStorage.setItem("warehouseId", warehouseId);

      // Cập nhật State
      authToken.value = data.token;
      currentUserRole.value = role;
      currentUsername.value = username;

      return { success: true };
    } catch (error) {
      return {
        success: false,
        message: error.response?.data?.message || "Lỗi kết nối máy chủ C#!",
      };
    }
  };

  const logout = () => {
    localStorage.clear(); // Quét sạch bộ nhớ khi Đăng xuất
    currentUserRole.value = null;
    authToken.value = null;
    currentUsername.value = null;
    delete api.defaults.headers.common["Authorization"];
  };

  const isLoggedIn = computed(() => !!authToken.value);

  // ĐÃ THÊM QUYỀN CHO THU MUA
  const canExport = computed(() =>
    ["admin", "giam_doc", "gd_chi_nhanh", "ql_kho", "nv_thu_mua"].includes(
      currentUserRole.value,
    ),
  );

  // Thu mua chỉ tạo PO, không duyệt. Giám đốc duyệt.
  const canApprovePO = computed(() =>
    ["admin", "giam_doc"].includes(currentUserRole.value),
  );

  const canApproveOperation = computed(() =>
    ["admin", "giam_doc", "gd_chi_nhanh", "ql_kho"].includes(
      currentUserRole.value,
    ),
  );

  const canModifyStock = computed(() =>
    ["admin", "giam_doc", "gd_chi_nhanh", "ql_kho"].includes(
      currentUserRole.value,
    ),
  );

  return {
    currentUserRole,
    currentUsername,
    isLoggedIn,
    login,
    logout,
    canExport,
    canApprovePO,
    canApproveOperation,
    canModifyStock,
    api,
  };
}
