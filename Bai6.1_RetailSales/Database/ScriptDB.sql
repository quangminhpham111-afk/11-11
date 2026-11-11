/* =====================================================================
   BÀI 6.1 - HỆ THỐNG QUẢN LÝ BÁN HÀNG & SIÊU THỊ MINI (SalesDB)
   Script khởi tạo CSDL + dữ liệu mẫu (SQL Server)
   Chạy toàn bộ script trong SSMS (có thể chạy lại nhiều lần).
   ===================================================================== */
USE master;
GO
IF DB_ID(N'SalesDB') IS NULL
    CREATE DATABASE SalesDB;
GO
USE SalesDB;
GO

/* ---------- Xóa bảng cũ (theo thứ tự khóa ngoại) ---------- */
IF OBJECT_ID(N'dbo.ChiTietHoaDon', N'U') IS NOT NULL DROP TABLE dbo.ChiTietHoaDon;
IF OBJECT_ID(N'dbo.HoaDon',        N'U') IS NOT NULL DROP TABLE dbo.HoaDon;
IF OBJECT_ID(N'dbo.SanPham',       N'U') IS NOT NULL DROP TABLE dbo.SanPham;
IF OBJECT_ID(N'dbo.KhachHang',     N'U') IS NOT NULL DROP TABLE dbo.KhachHang;
IF OBJECT_ID(N'dbo.NhanVien',      N'U') IS NOT NULL DROP TABLE dbo.NhanVien;
GO

/* ---------- 1. NhanVien ---------- */
CREATE TABLE dbo.NhanVien
(
    MaNV         INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_NhanVien PRIMARY KEY,
    TenNV        NVARCHAR(100)     NOT NULL,
    SoDienThoai  VARCHAR(15)       NULL,
    ChucVu       NVARCHAR(50)      NULL
);
-- UNIQUE cho SoDienThoai nhưng vẫn cho phép nhiều dòng NULL
CREATE UNIQUE INDEX UQ_NhanVien_SDT ON dbo.NhanVien(SoDienThoai) WHERE SoDienThoai IS NOT NULL;
GO

/* ---------- 2. KhachHang ---------- */
CREATE TABLE dbo.KhachHang
(
    MaKH         INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_KhachHang PRIMARY KEY,
    TenKH        NVARCHAR(100)     NOT NULL,
    DienThoai    VARCHAR(15)       NULL,
    DiemTichLuy  INT               NOT NULL CONSTRAINT DF_KhachHang_Diem DEFAULT 0
);
CREATE UNIQUE INDEX UQ_KhachHang_DT ON dbo.KhachHang(DienThoai) WHERE DienThoai IS NOT NULL;
GO

/* ---------- 3. SanPham ---------- */
CREATE TABLE dbo.SanPham
(
    MaSP         VARCHAR(20)   NOT NULL CONSTRAINT PK_SanPham PRIMARY KEY,   -- mã vạch
    TenSP        NVARCHAR(150) NOT NULL,
    GiaBan       DECIMAL(18,2) NOT NULL CONSTRAINT CK_SanPham_Gia CHECK (GiaBan > 0),
    SoLuongTon   INT           NOT NULL CONSTRAINT CK_SanPham_Ton CHECK (SoLuongTon >= 0)
);
GO

/* ---------- 4. HoaDon ---------- */
CREATE TABLE dbo.HoaDon
(
    MaHD      INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_HoaDon PRIMARY KEY,
    NgayLap   DATETIME          NOT NULL CONSTRAINT DF_HoaDon_Ngay DEFAULT GETDATE(),
    MaNV      INT               NOT NULL CONSTRAINT FK_HoaDon_NhanVien  FOREIGN KEY REFERENCES dbo.NhanVien(MaNV),
    MaKH      INT               NULL     CONSTRAINT FK_HoaDon_KhachHang FOREIGN KEY REFERENCES dbo.KhachHang(MaKH),
    TongTien  DECIMAL(18,2)     NOT NULL CONSTRAINT DF_HoaDon_Tong DEFAULT 0
);
GO

/* ---------- 5. ChiTietHoaDon ---------- */
CREATE TABLE dbo.ChiTietHoaDon
(
    MaHD     INT           NOT NULL CONSTRAINT FK_CT_HoaDon FOREIGN KEY REFERENCES dbo.HoaDon(MaHD),
    MaSP     VARCHAR(20)   NOT NULL CONSTRAINT FK_CT_SanPham FOREIGN KEY REFERENCES dbo.SanPham(MaSP),
    SoLuong  INT           NOT NULL CONSTRAINT CK_CT_SoLuong CHECK (SoLuong > 0),
    DonGia   DECIMAL(18,2) NOT NULL,
    CONSTRAINT PK_ChiTietHoaDon PRIMARY KEY (MaHD, MaSP)
);
GO

/* =====================================================================
   DỮ LIỆU MẪU
   ===================================================================== */
INSERT INTO dbo.NhanVien (TenNV, SoDienThoai, ChucVu) VALUES
(N'Nguyễn Văn An',  '0901000001', N'Thu ngân'),
(N'Trần Thị Bình',  '0901000002', N'Thu ngân'),
(N'Lê Quang Cường', '0901000003', N'Quản lý');

INSERT INTO dbo.KhachHang (TenKH, DienThoai, DiemTichLuy) VALUES
(N'Phạm Minh Đức',  '0912000001', 1),
(N'Hoàng Thu Hà',   '0912000002', 2),
(N'Vũ Thanh Hương', '0912000003', 2),
(N'Đỗ Quốc Khánh',  '0912000004', 0);

INSERT INTO dbo.SanPham (MaSP, TenSP, GiaBan, SoLuongTon) VALUES
('8934563138164', N'Mì Hảo Hảo tôm chua cay',        4500,   100),
('8935049500155', N'Nước ngọt Coca-Cola lon 330ml',  10000,   80),
('8934673101010', N'Sữa tươi Vinamilk 1L',           32000,   50),
('7622210100313', N'Bánh Oreo 133g',                 18000,   60),
('8936036020013', N'Dầu ăn Simply 1L',               58000,   30),
('8938505970012', N'Gạo ST25 túi 5kg',              135000,   25),
('8935001710010', N'Nước suối Aquafina 500ml',        6000,  120),
('8935024140015', N'Cà phê G7 3in1 hộp 16 gói',      52000,   40),
('8936012340019', N'Trứng gà vỉ 10 quả',             35000,   45),
('8934868150018', N'Bột giặt Omo 3kg',              160000,   20),
('8934868160017', N'Kem đánh răng P/S 180g',         21000,   70),
('8886467100012', N'Snack Lay''s vị tự nhiên 54g',   12000,   90);

-- Hóa đơn mẫu (để màn hình Thống kê có dữ liệu)
INSERT INTO dbo.HoaDon (NgayLap, MaNV, MaKH, TongTien) VALUES
(DATEADD(DAY, 0, GETDATE()), 1, 1,    159000),
(DATEADD(DAY, 0, GETDATE()), 2, NULL, 171000),
(DATEADD(DAY,-1, GETDATE()), 1, 2,    298500),
(DATEADD(DAY,-2, GETDATE()), 2, 3,    214000);

INSERT INTO dbo.ChiTietHoaDon (MaHD, MaSP, SoLuong, DonGia) VALUES
(1, '8934563138164', 10,   4500),
(1, '8935049500155',  5,  10000),
(1, '8934673101010',  2,  32000),
(2, '8938505970012',  1, 135000),
(2, '8935001710010',  6,   6000),
(3, '8934868150018',  1, 160000),
(3, '8936036020013',  2,  58000),
(3, '8934563138164',  5,   4500),
(4, '8935049500155', 10,  10000),
(4, '8886467100012',  5,  12000),
(4, '7622210100313',  3,  18000);
GO

SELECT N'Tạo SalesDB thành công' AS KetQua;
GO
