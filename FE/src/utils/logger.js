const API_URL = "https://localhost:7139/api/SysUiLogs";

export const uiLogger = {
  log(eventType, path, message, details = null) {
    try {
      let userInfo = { userName: "Khách (Chưa Login)", id: null };

      const token = localStorage.getItem("authToken");
      const savedUsername = localStorage.getItem("username");

      if (savedUsername) {
        userInfo.userName = savedUsername;
      }

      if (token) {
        try {
          const base64Url = token.split(".")[1];
          const base64 = base64Url.replace(/-/g, "+").replace(/_/g, "/");
          const decodedPayload = JSON.parse(
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

          const idClaim =
            decodedPayload[
              "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier"
            ] ||
            decodedPayload.nameid ||
            decodedPayload.sub ||
            decodedPayload.UserId ||
            decodedPayload.EmployeeId ||
            decodedPayload.Id;

          if (idClaim) userInfo.id = parseInt(idClaim);
        } catch (e) {
          console.warn("Không thể giải mã Token:", e);
        }
      }

      const payload = {
        userId: userInfo.id,
        userName: userInfo.userName,
        eventType: eventType,
        path: path,
        message: message,
        details: details ? JSON.stringify(details) : "", // <-- FIX TẠI ĐÂY LÀ HẾT LỖI 400
      };

      fetch(API_URL, {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
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
        .catch((err) =>
          console.error("❌ Lỗi kết nối mạng khi gửi UI Log:", err),
        );
    } catch (error) {
      console.error("Lỗi khởi tạo UI Log:", error);
    }
  },
};
