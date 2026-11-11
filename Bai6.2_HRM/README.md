# Bài 6.2: Hệ thống Quản lý Nhân sự & Tính lương (HRM & Payroll System)

## MỤC 1: THÔNG TIN SINH VIÊN

- **Họ và tên sinh viên:** [Điền họ tên]
- **Mã sinh viên:** [Điền MSV]
- **Lớp / Khóa:** [Điền tên lớp]

---

## MỤC 2: HÌNH ẢNH KẾT QUẢ CÁC MÀN HÌNH CHỨC NĂNG

1. **Màn hình Chính / Tổng quan hệ thống:**

   ![Màn hình chính](images/01_main_screen.png)

2. **Màn hình Quản lý Danh mục (CRUD - Thêm/Sửa/Xóa):**

   ![Màn hình CRUD](images/02_crud_screen.png)

3. **Màn hình Thực thi Nghiệp vụ chính (Chấm công / Chốt lương):**

   ![Màn hình Nghiệp vụ](images/03_transaction_screen.png)

4. **Màn hình Báo cáo - Thống kê (Bảng lương):**

   ![Màn hình Thống kê](images/04_report_screen.png)

5. **Minh họa An toàn Dữ liệu (Phòng chống SQL Injection / Báo lỗi Transaction Rollback):**

   ![Minh họa Xử lý lỗi](images/05_error_handling.png)

---

## MỤC 3: THÔNG TIN KỸ THUẬT

### Công nghệ

- C# WinForms, .NET Framework 4.8, ADO.NET (`System.Data.SqlClient`)
- SQL Server (2017 trở lên, bản Express/LocalDB đều được)
- Kiến trúc 3-Tier: **DTO – DAL – BUS – GUI**

### Cấu trúc thư mục

```
├── Database/
│   └── ScriptDB.sql          # Tạo HRMDB (4 bảng) + dữ liệu mẫu
├── HRM/
│   ├── DTO/                  # PhongBanDTO, NhanVienDTO, DuLieuTinhLuongDTO, BangLuongDTO
│   ├── DAL/                  # DbHelper, PhongBanDAL, NhanVienDAL, ChamCongDAL, LuongDAL
│   ├── BUS/                  # PhongBanBUS, NhanVienBUS, ChamCongBUS, LuongBUS
│   ├── GUI/                  # FrmMain, FrmPhongBan, FrmNhanVien, FrmChamCong, FrmLuong
│   ├── App.config            # Chuỗi kết nối HRMDB
│   └── Program.cs
├── images/                   # Ảnh chụp màn hình dùng trong README
├── README.md
└── HRM.sln
```

### Cách chạy

1. Mở **SSMS**, chạy file `Database/ScriptDB.sql` (tạo database `HRMDB` và dữ liệu mẫu).
2. Mở `HRM.sln` bằng **Visual Studio 2019/2022**.
3. Kiểm tra chuỗi kết nối trong `HRM/App.config` (mặc định `Data Source=.`; nếu dùng SQL Express đổi thành `.\SQLEXPRESS`).
4. Nhấn **F5** để chạy.

### Các yêu cầu đã thực hiện

| Yêu cầu | Vị trí trong mã nguồn |
|---|---|
| Thêm/sửa/xóa nhân viên, **chuyển phòng ban** | `NhanVienDAL`, `NhanVienBUS.ChuyenPhongBan`, `GUI/FrmNhanVien.cs` |
| Tìm nhân viên theo tên, phòng ban hoặc CCCD (parameterized query) | `NhanVienDAL.Search` |
| Nạp chấm công bằng `SqlDataAdapter` vào `DataTable` hiển thị trên `DataGridView` | `ChamCongDAL.LoadChamCong` |
| Nhập ngày công / giờ OT trực tiếp trên lưới | `GUI/FrmChamCong.cs` |
| **Lưu chấm công bằng `SqlCommandBuilder` + `adapter.Update()`** | `ChamCongDAL.SaveChamCong` |
| Công thức lương | `LuongBUS.TinhThucLinh` |
| **Chốt lương bằng `SqlTransaction`** (ghi bảng lương + khóa chấm công) | `LuongDAL.ChotLuong` |

### Chấm công theo mô hình ngắt kết nối

1. Chọn Tháng/Năm → `SqlDataAdapter.Fill()` nạp chấm công của tháng vào `DataTable`; nhân viên chưa có dòng chấm công được thêm vào `DataTable` ở trạng thái `Added`, nên **toàn bộ nhân viên** đều hiện trên lưới.
2. Người dùng sửa **Số ngày công** và **Số giờ OT** trực tiếp trên `DataGridView` (chỉ đổi dữ liệu trong bộ nhớ, không có kết nối mở).
3. **Lưu chấm công** → `SqlCommandBuilder` tự sinh lệnh INSERT/UPDATE, `adapter.Update(dataTable)` đồng bộ mọi thay đổi về SQL Server trong một thao tác (bọc trong transaction để lưu trọn vẹn hoặc không lưu gì).
4. Sau khi lưu, bảng được nạp lại để lấy khóa `MaCong` mới sinh.

### Công thức tính lương

```
ThucLinh = (LuongCoBan * HeSoLuong / 22) * SoNgayCong + (SoGioOT * 50,000)
```

Kết quả được làm tròn đến đồng. Ví dụ: lương cơ bản 12.000.000, hệ số 1,2, đủ 22 ngày công, 4 giờ OT → `12.000.000 × 1,2 / 22 × 22 + 4 × 50.000 = 14.600.000`.

### Chốt lương (`LuongDAL.ChotLuong`)

```
BeginTransaction
 ├─ 1. Kiểm tra tháng chưa có bảng lương (SELECT ... WITH (UPDLOCK, HOLDLOCK))
 ├─ 2. INSERT BangLuong cho toàn bộ nhân viên có chấm công trong tháng
 ├─ 3. UPDATE BangChamCong SET DaChot = 1 WHERE ThangNam = @tn   -- khóa chấm công
 └─ 4. Commit  (lỗi bất kỳ -> Rollback)
```

Sau khi chốt, màn hình Chấm công chuyển sang chế độ chỉ đọc cho tháng đó và nút **Lưu chấm công** bị vô hiệu hóa; ràng buộc `UNIQUE (MaNV, ThangNam)` ngăn việc chốt trùng.

### Ghi chú về thiết kế CSDL

- **`BangChamCong.DaChot (bit)`** là cột bổ sung so với đề để thực hiện yêu cầu "khóa dữ liệu chấm công".
- **`BangChamCong`** và **`BangLuong`** có ràng buộc `UNIQUE (MaNV, ThangNam)`; `BangLuong` dùng `MaLuong` (identity) làm khóa chính vì đề nêu đồng thời cả hai khóa.
- `CCCD` dùng chỉ mục `UNIQUE` có điều kiện `WHERE CCCD IS NOT NULL` để cho phép nhân viên chưa có CCCD.
- `ThangNam` có dạng `MM/yyyy` (ví dụ `11/2026`).
- Script có sẵn chấm công và bảng lương **tháng 10/2026 (đã chốt)** để minh họa trạng thái khóa; tháng 11/2026 để trống cho bạn chấm công và chốt lương.

### Phòng chống SQL Injection

Mọi truy vấn đều dùng `SqlParameter`; các ký tự đặc biệt trong từ khóa `LIKE` được escape. Cách chụp ảnh minh họa (mục 5):

- **SQL Injection:** ở màn hình Hồ sơ Nhân sự, nhập `' OR 1=1 --` vào ô tìm kiếm → danh sách rỗng (chuỗi được coi là dữ liệu, không phải lệnh SQL).
- **Rollback / báo lỗi:** ở màn hình Tính lương, chốt lương tháng 10/2026 (đã chốt) → báo lỗi "đã được chốt lương rồi"; hoặc thêm một nhân viên trùng CCCD → báo lỗi trùng, không có dữ liệu nào bị thay đổi.

### Kịch bản dùng thử nhanh

1. **Chấm công:** chọn tháng 11/2026 → nhập ngày công/OT → **Lưu chấm công**.
2. **Tính lương (xem trước):** chọn tháng 11/2026 → *Tính lương (xem trước)*.
3. **Chốt lương tháng** → quay lại màn hình Chấm công tháng 11/2026 sẽ thấy dữ liệu bị khóa.
