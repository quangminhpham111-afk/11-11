# Bài 6.1: Hệ thống Quản lý Bán hàng & Siêu thị mini (Retail Sales System)

## MỤC 1: THÔNG TIN SINH VIÊN

- **Phạm Quang Minh:** 
- **24810310413:**
- **D19CNPM5:** 

---

## MỤC 2: HÌNH ẢNH KẾT QUẢ CÁC MÀN HÌNH CHỨC NĂNG

1. **Màn hình Chính / Tổng quan hệ thống:**

   ![Màn hình chính](images/01_main_screen.png)

2. **Màn hình Quản lý Danh mục (CRUD - Thêm/Sửa/Xóa):**

   ![Màn hình CRUD](images/02_crud_screen.png)

3. **Màn hình Thực thi Nghiệp vụ chính (Lập Hóa đơn / Bán hàng):**

   ![Màn hình Nghiệp vụ](images/03_transaction_screen.png)

4. **Màn hình Báo cáo - Thống kê:**

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
│   └── ScriptDB.sql          # Tạo SalesDB (5 bảng) + dữ liệu mẫu
├── RetailSales/
│   ├── DTO/                  # NhanVienDTO, KhachHangDTO, SanPhamDTO, CartItemDTO, TopSanPhamDTO
│   ├── DAL/                  # DbHelper, SanPhamDAL, KhachHangDAL, NhanVienDAL, HoaDonDAL, ThongKeDAL
│   ├── BUS/                  # SanPhamBUS, KhachHangBUS, NhanVienBUS, BanHangBUS, ThongKeBUS
│   ├── GUI/                  # FrmMain, FrmSanPham, FrmKhachHang, FrmNhanVien, FrmBanHang, FrmThongKe
│   ├── App.config            # Chuỗi kết nối SalesDB
│   └── Program.cs
├── images/                   # Ảnh chụp màn hình dùng trong README
├── README.md
└── RetailSales.sln
```

### Cách chạy

1. Mở **SSMS**, chạy file `Database/ScriptDB.sql` (tạo database `SalesDB` và dữ liệu mẫu).
2. Mở `RetailSales.sln` bằng **Visual Studio 2019/2022**.
3. Kiểm tra chuỗi kết nối trong `RetailSales/App.config` (mặc định `Data Source=.`; nếu dùng SQL Express đổi thành `.\SQLEXPRESS`).
4. Nhấn **F5** để chạy.

### Các yêu cầu đã thực hiện

| Yêu cầu | Vị trí trong mã nguồn |
|---|---|
| CRUD Sản phẩm / Khách hàng / Nhân viên (3-Tier) | `DAL/*DAL.cs`, `BUS/*BUS.cs`, `GUI/Frm*.cs` |
| Tìm sản phẩm theo tên hoặc mã vạch bằng `SqlParameter` + `%` | `SanPhamDAL.Search` (`LIKE @kw`, giá trị `%từ khóa%`) |
| Giỏ hàng bằng `DataGridView`, chọn/quét mã, chọn số lượng | `GUI/FrmBanHang.cs` (gõ/quét mã rồi Enter) |
| Chọn khách hàng hoặc Khách lẻ, tự tính tổng tiền và điểm tích lũy | `BanHangBUS.TinhTongTien`, `TinhDiemTichLuy` |
| **Thanh toán bằng `SqlTransaction`** | `HoaDonDAL.TaoHoaDon` |
| Doanh thu ngày/tháng bằng `ExecuteScalar` | `ThongKeDAL.DoanhThuNgay`, `DoanhThuThang` |
| Top 5 sản phẩm bán chạy bằng `SqlDataReader` | `ThongKeDAL.TopSanPhamBanChay` |

### Luồng giao dịch thanh toán (`HoaDonDAL.TaoHoaDon`)

```
BeginTransaction
 ├─ 1. INSERT HoaDon
 ├─ với mỗi sản phẩm trong giỏ:
 │    ├─ SELECT SoLuongTon ... WITH (UPDLOCK)   -- khóa dòng, đọc tồn kho
 │    ├─ nếu SoLuongTon < SoLuong bán  ->  throw -> trans.Rollback() + báo lỗi
 │    ├─ 2. INSERT ChiTietHoaDon
 │    └─ 3. UPDATE SanPham SET SoLuongTon = SoLuongTon - @sl
 ├─ 4. UPDATE KhachHang SET DiemTichLuy += TongTien / 100000   (bỏ qua nếu khách lẻ)
 └─ 5. trans.Commit()
```

Bất kỳ lỗi nào trong chuỗi trên đều gọi `trans.Rollback()` nên hóa đơn, chi tiết, tồn kho và điểm tích lũy luôn nhất quán (ACID).

### Phòng chống SQL Injection

- Toàn bộ câu lệnh SQL dùng `SqlParameter` (`cmd.Parameters.Add(...)`), **không nối chuỗi dữ liệu người dùng vào câu lệnh**.
- Với tìm kiếm `LIKE`, từ khóa được thêm `%` ở hai đầu **trong giá trị tham số** và các ký tự đặc biệt của LIKE (`%`, `_`, `[`, `\`) được escape (`ESCAPE '\'`).
- Thử nhập `' OR 1=1 --` vào ô tìm kiếm sản phẩm: hệ thống chỉ coi đó là chuỗi ký tự, không trả về toàn bộ bảng.

### Cách chụp ảnh minh họa (mục 5)

- **SQL Injection:** ở màn hình Quản lý Sản phẩm, nhập `' OR 1=1 --` vào ô tìm kiếm, bấm Tìm → danh sách rỗng.
- **Rollback giao dịch:** ở màn hình Bán hàng, thêm một sản phẩm vào giỏ, sửa cột *Số lượng* trên lưới thành số lớn hơn tồn kho (ví dụ 9999), bấm **THANH TOÁN** → hộp thoại báo không đủ tồn kho và giao dịch đã `ROLLBACK`; kiểm tra trong SSMS thấy không có hóa đơn mới.

### Dữ liệu mẫu

- 3 nhân viên, 4 khách hàng, 12 sản phẩm (mã vạch dạng EAN-13), 4 hóa đơn mẫu để màn hình thống kê có số liệu.
- Số điện thoại dùng chỉ mục `UNIQUE` có điều kiện `WHERE ... IS NOT NULL` để cho phép nhiều dòng không có số điện thoại.
