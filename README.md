# 📖 TÀI LIỆU HƯỚNG DẪN SỬ DỤNG HỆ THỐNG WMS (QUẢN LÝ KHO)

Hệ thống Quản lý kho (WMS) được thiết kế chuyên nghiệp, áp dụng cơ chế bảo mật **Row-Level Security** và phân tách nhiệm vụ (Segregation of Duties) nghiêm ngặt nhằm đảm bảo an toàn dữ liệu tuyệt đối.

---

## 👥 I. PHÂN QUYỀN HỆ THỐNG CHI TIẾT (USER ROLES)

Hệ thống chia làm 5 cấp bậc với giới hạn chức năng riêng biệt:

### 1. ⚙️ Admin (Quản trị hệ thống / IT)
* **Vai trò:** Người thiết lập và bảo trì nền tảng.
* **Quyền hạn:** Thêm/sửa/xóa tài khoản Nhân sự, tạo lập Chi nhánh/Kho mới, quản lý Danh mục chung (Nhóm hàng, Đối tác) và theo dõi Nhật ký hệ thống (Audit Logs, UI Logs).
* **Giới hạn:** 🚫 **KHÔNG** có quyền can thiệp hoặc xem các dữ liệu nghiệp vụ kho (Không xem Dashboard, không Nhập/Xuất, không xem Báo cáo N-X-T, không Kiểm kê hay Duyệt đơn hàng).

### 2. 👑 Giám đốc (Sếp Tổng)
* **Vai trò:** Người điều hành tối cao của doanh nghiệp.
* **Quyền hạn:** Kiểm soát toàn bộ số liệu nghiệp vụ của **tất cả Chi nhánh và Kho**. 
* **Nhiệm vụ chính:** Xem Dashboard tổng quan, xem Báo cáo Nhập-Xuất-Tồn đa kho, tra cứu Sổ giao dịch và **Duyệt Đơn đặt hàng (PO)** / Duyệt phiếu tổng.

### 3. 🏢 Giám đốc Chi nhánh
* **Vai trò:** Quản lý cấp trung.
* **Quyền hạn:** Tương tự Giám đốc nhưng dữ liệu bị **khóa chặt** trong phạm vi Chi nhánh mình quản lý. Không nhìn thấy kho của chi nhánh khác.

### 4. 📦 Quản lý Kho (Thủ Kho)
* **Vai trò:** Trưởng trạm tại 1 kho cụ thể.
* **Quyền hạn:** Quản lý mọi biến động hàng hóa tại kho của mình.
* **Nhiệm vụ chính:** Lập Phiếu Nhập/Xuất, Điều chuyển, lập Phiếu Kiểm kê (Chốt sổ), Báo cáo hàng lỗi, và **Lập Đơn đặt hàng PO (Bản nháp để trình Giám đốc duyệt)**. Nhận các Cảnh báo tồn kho tự động.

### 5. 👷 Nhân viên Kho
* **Vai trò:** Người thực thi nghiệp vụ bốc xếp thực tế.
* **Quyền hạn:** Chỉ truy cập các tính năng phục vụ công việc chân tay.
* **Nhiệm vụ chính:** Xem Sơ đồ kho, Tra cứu vị trí lưu kho, thao tác Kho bãi (Putaway - cất hàng lên kệ/lấy hàng xuống kệ) và ghi nhận Hàng lỗi nếu phát hiện lúc bốc dỡ.

---

## 🛠️ II. HƯỚNG DẪN CÁC CHỨC NĂNG NGHIỆP VỤ CỐT LÕI

### 1. 📊 Tổng quan Hệ thống (Dashboard) - *Dành cho Khối Quản lý*
* **Chức năng:** Cung cấp cái nhìn toàn cảnh về tình hình sức khỏe của Kho bằng số liệu thực (Real-time).
* **Dữ liệu hiển thị:** Tổng giá trị tồn kho (VNĐ), số lượng phiếu Nhập/Xuất trong tháng, số lượng cảnh báo. Biểu đồ so sánh Lượng Giao Dịch trong 6 tháng gần nhất.
* **Tỉ lệ lấp đầy:** Hiển thị trực quan % không gian kệ đang được sử dụng thực tế.
* *💡 Mẹo cho Giám đốc:* Sử dụng bộ lọc góc trên cùng bên phải để xem dữ liệu cụ thể của từng Chi nhánh hoặc từng Kho riêng lẻ.

### 2. 🛒 Quy trình Đặt hàng PO (Purchase Order)
1.  **Lập đơn (Quản lý kho):** Bấm `Lập Đơn Mua Hàng`, chọn Nhà Cung Cấp, chọn mặt hàng và nhập số lượng. Phiếu tự động lưu ở trạng thái **Chờ duyệt**.
2.  **Duyệt đơn (Giám đốc):** Vào mục Đặt hàng PO, kiểm tra số lượng/nhà cung cấp và bấm `Duyệt PO`. 
3.  **Nhập Kho (Quản lý kho):** Khi xe tải giao hàng đến, Quản lý kho mở PO đã duyệt lên và bấm `Nhập Kho`. Hệ thống sẽ tự động sao chép toàn bộ danh sách hàng sang Phiếu Nhập Kho (tiết kiệm thời gian gõ lại).

### 3. 📋 Kiểm Kê (Stocktake) - *Bù trừ hao hụt*
1.  **Lập phiếu (Quản lý kho):** Tạo phiếu kiểm kê. Điền số lượng đếm được thực tế vào ô "Tồn Thực Tế".
2.  **Hệ thống phân tích:** Tự động so sánh số đếm được với "Tồn Hệ Thống" và chỉ ra độ lệch (Khớp / Thừa / Thiếu).
3.  **Chốt Sổ:** Bấm `Chốt Sổ Kiểm Kê`. Hệ thống tự động sinh ra một **Phiếu Điều Chỉnh (Adjustment)**, âm thầm cộng/trừ số lượng trên kệ để phần mềm khớp 100% với thực tế bãi.

### 4. 📈 Báo cáo Nhập - Xuất - Tồn (N-X-T)
* **Mục đích:** Bảng kê đánh giá lưu lượng hàng hóa và dòng tiền.
* **Cách xem:** Chọn khoảng thời gian (Từ ngày - Đến ngày). Thuật toán tự động lùi mốc thời gian để tính toán công thức chuẩn Kế toán: **Tồn Đầu Kỳ + Nhập Trong Kỳ - Xuất Trong Kỳ = Tồn Cuối Kỳ**.

### 5. 🚨 Trung Tâm Cảnh Báo Tồn Kho (Smart Alerts)
*Trợ lý ảo tự động phân tích và réo chuông 24/7 cho Quản lý kho.*
* 🔴 **Nghiêm trọng (Màu Đỏ):** Hàng đã cạn kiệt (Số lượng = 0) hoặc Đã Hết Hạn Sử Dụng. Yêu cầu tạo PO mua gấp hoặc xuất hủy.
* 🟠 **Cảnh báo (Màu Cam):** Số lượng tổng tụt xuống dưới Định mức tối thiểu (Min Stock), hoặc HSD còn dưới 30 ngày (Cần áp dụng xuất FEFO).
* 🔵 **Lưu ý (Màu Xanh):** Có hàng hóa đang nằm kẹt ở "Khu Chờ Nhập", cần điều động Nhân viên kho cất lên kệ (Putaway).

---

## 🔄 III. QUY TRÌNH TIÊU CHUẨN (SOP) KHUYẾN NGHỊ

### 📌 Luồng Bổ sung hàng hóa (Inbound Flow):
1. Quản lý kho nhận Cảnh báo đỏ (Sắp hết hàng).
2. Quản lý kho lập Đơn Đặt Hàng PO (Bản nháp).
3. Giám đốc xét duyệt PO.
4. Hàng về tới bãi -> Tự động hóa tạo Phiếu Nhập từ PO.
5. Nhân viên kho dùng xe nâng đưa hàng từ "Bãi chờ" lên "Kệ chính thức" (Nghiệp vụ Kho bãi).

### 📌 Luồng Xử lý Hao hụt / Hàng Lỗi:
1. Quản lý kho tạo Phiếu Kiểm Kê định kỳ.
2. Cập nhật số đếm thực tế vào phần mềm -> Phát hiện Thiếu 1 Thùng.
3. Chốt Sổ Kiểm kê (Hệ thống tự động trừ tồn kho 1 Thùng).
4. Tìm ra nguyên nhân: 1 Thùng bị thủng móp do vận chuyển.
5. Quản lý kho vào mục Hàng Lỗi lập báo cáo ghi nhận lý do thủng móp để giải trình với Kế toán và Giám đốc.