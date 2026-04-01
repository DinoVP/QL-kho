const API_URL = "https://localhost:7139/api/SysUiLogs";

export const uiLogger = {
  log(eventType, path, message, details = null) {
    try {
      let userInfo = { userName: "Khách (Chưa Login)", id: null };

      // BƯỚC 1: Lấy đúng tên biến mà sếp đã lưu trong useAuth.js
      const token = localStorage.getItem("authToken");
      const savedUsername = localStorage.getItem("username");

      if (savedUsername) {
        userInfo.userName = savedUsername;
      }

      // BƯỚC 2: Giải mã JWT authToken để moi cái UserId ra (Vì useAuth không lưu ID)
      if (token) {
        try {
          const base64Url = token.split(".")[1];
          const base64 = base64Url.replace(/-/g, "+").replace(/_/g, "/");
          const payload = JSON.parse(
            decodeURIComponent(
              window
                .atob(base64)
                .split("")
                .map(function (c) {
                  return "%" + ("00" + c.charCodeAt(0).toString(16)).slice(-2);
                })
                .join(""),
            ),
          );

          // Cố gắng bắt ID theo chuẩn của C# JWT
          const idClaim =
            payload[
              "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier"
            ] ||
            payload.nameid ||
            payload.sub ||
            payload.UserId ||
            payload.EmployeeId ||
            payload.Id;

          if (idClaim) userInfo.id = parseInt(idClaim);
        } catch (e) {
          console.warn("Không thể giải mã Token:", e);
        }
      }

      // BƯỚC 3: Đóng gói dữ liệu và bắn xuống Backend
      const payload = {
        userId: userInfo.id,
        userName: userInfo.userName,
        eventType: eventType, // 'NAVIGATION', 'CLICK', 'API_CALL', 'ERROR'
        path: path,
        message: message,
        details: details ? JSON.stringify(details) : null,
      };

      console.log("🚀 Đang chuẩn bị bắn Log UI:", payload);

      fetch(API_URL, {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
          // Đính kèm token chuẩn để Backend cho phép lưu
          Authorization: "Bearer " + (token || ""),
        },
        body: JSON.stringify(payload),
      })
        .then(async (res) => {
          if (!res.ok) {
            const errorText = await res.text();
            console.error(
              `❌ Backend UI Log từ chối lưu (Lỗi ${res.status}):`,
              errorText,
            );
          } else {
            console.log("✅ Đã lưu Log UI thành công xuống Database!");
          }
        })
        .catch((err) => {
          console.error("❌ Lỗi kết nối mạng khi gửi UI Log:", err);
        });
    } catch (error) {
      console.error("Lỗi khởi tạo UI Log:", error);
    }
  },
};
