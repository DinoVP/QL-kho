import { ref, computed } from "vue";
import axios from "axios";

const currentUserRole = ref(localStorage.getItem("userRole") || null);
const authToken = ref(localStorage.getItem("authToken") || null);
const currentUsername = ref(localStorage.getItem("username") || null);

// ĐÃ SỬA LẠI ĐÚNG CỔNG HTTPS 7139 CỦA SẾP
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

      localStorage.setItem("authToken", data.token);
      localStorage.setItem("userRole", data.role);
      localStorage.setItem("username", data.username);

      authToken.value = data.token;
      currentUserRole.value = data.role;
      currentUsername.value = data.username;

      api.defaults.headers.common["Authorization"] = `Bearer ${data.token}`;

      return { success: true };
    } catch (error) {
      return {
        success: false,
        message: error.response?.data?.message || "Lỗi kết nối máy chủ C#!",
      };
    }
  };

  const logout = () => {
    localStorage.clear();
    currentUserRole.value = null;
    authToken.value = null;
    currentUsername.value = null;
    delete api.defaults.headers.common["Authorization"];
  };

  const isLoggedIn = computed(() => !!authToken.value);

  const canExport = computed(() =>
    ["admin", "giam_doc", "gd_chi_nhanh", "ql_kho"].includes(
      currentUserRole.value,
    ),
  );
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
