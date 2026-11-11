/* =====================================================================
   BÀI 6.2 - HỆ THỐNG QUẢN LÝ NHÂN SỰ & TÍNH LƯƠNG (HRMDB)
   Script khởi tạo CSDL + dữ liệu mẫu (SQL Server)
   Chạy toàn bộ script trong SSMS (có thể chạy lại nhiều lần).
   ===================================================================== */
USE master;
GO
IF DB_ID(N'HRMDB') IS NULL
    CREATE DATABASE HRMDB;
GO
USE HRMDB;
GO

IF OBJECT_ID(N'dbo.BangLuong',    N'U') IS NOT NULL DROP TABLE dbo.BangLuong;
IF OBJECT_ID(N'dbo.BangChamCong', N'U') IS NOT NULL DROP TABLE dbo.BangChamCong;
IF OBJECT_ID(N'dbo.NhanVien',     N'U') IS NOT NULL DROP TABLE dbo.NhanVien;
IF OBJECT_ID(N'dbo.PhongBan',     N'U') IS NOT NULL DROP TABLE dbo.PhongBan;
GO

/* ---------- 1. PhongBan ---------- */
CREATE TABLE dbo.PhongBan
(
    MaPB   INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_PhongBan PRIMARY KEY,
    TenPB  NVARCHAR(100)     NOT NULL,
    Mota   NVARCHAR(255)     NULL
);
GO

/* ---------- 2. NhanVien ---------- */
CREATE TABLE dbo.NhanVien
(
    MaNV         INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_NhanVien PRIMARY KEY,
    HoTen        NVARCHAR(100)     NOT NULL,
    NgaySinh     DATETIME          NULL,
    CCCD         VARCHAR(20)       NULL,
    MaPB         INT               NOT NULL CONSTRAINT FK_NhanVien_PhongBan FOREIGN KEY REFERENCES dbo.PhongBan(MaPB),
    LuongCoBan   DECIMAL(18,2)     NOT NULL CONSTRAINT CK_NhanVien_Luong CHECK (LuongCoBan > 0),
    HeSoLuong    FLOAT             NOT NULL CONSTRAINT DF_NhanVien_HeSo DEFAULT 1.0
);
-- UNIQUE cho CCCD nhưng cho phép nhiều dòng NULL
CREATE UNIQUE INDEX UQ_NhanVien_CCCD ON dbo.NhanVien(CCCD) WHERE CCCD IS NOT NULL;
GO

/* ---------- 3. BangChamCong ----------
   Bổ sung cột DaChot (bit) để "khóa dữ liệu chấm công" sau khi chốt lương.
   UNIQUE (MaNV, ThangNam): mỗi nhân viên chỉ có 1 dòng chấm công / tháng. */
CREATE TABLE dbo.BangChamCong
(
    MaCong       INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_BangChamCong PRIMARY KEY,
    MaNV         INT               NOT NULL CONSTRAINT FK_ChamCong_NhanVien FOREIGN KEY REFERENCES dbo.NhanVien(MaNV),
    ThangNam     VARCHAR(7)        NOT NULL,                       -- ví dụ '11/2026'
    SoNgayCong   FLOAT             NOT NULL CONSTRAINT DF_ChamCong_Cong DEFAULT 0
                                   CONSTRAINT CK_ChamCong_Cong CHECK (SoNgayCong >= 0 AND SoNgayCong <= 31),
    SoGioOT      FLOAT             NOT NULL CONSTRAINT DF_ChamCong_OT DEFAULT 0
                                   CONSTRAINT CK_ChamCong_OT CHECK (SoGioOT >= 0),
    DaChot       BIT               NOT NULL CONSTRAINT DF_ChamCong_DaChot DEFAULT 0,
    CONSTRAINT UQ_ChamCong_NV_Thang UNIQUE (MaNV, ThangNam)
);
GO

/* ---------- 4. BangLuong ----------
   MaLuong là khóa chính (identity); cặp (MaNV, ThangNam) là UNIQUE
   để không thể chốt lương 2 lần cho cùng nhân viên trong cùng tháng. */
CREATE TABLE dbo.BangLuong
(
    MaLuong    INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_BangLuong PRIMARY KEY,
    MaNV       INT               NOT NULL CONSTRAINT FK_BangLuong_NhanVien FOREIGN KEY REFERENCES dbo.NhanVien(MaNV),
    ThangNam   VARCHAR(7)        NOT NULL,
    ThucLinh   DECIMAL(18,2)     NOT NULL,
    CONSTRAINT UQ_BangLuong_NV_Thang UNIQUE (MaNV, ThangNam)
);
GO

/* =====================================================================
   DỮ LIỆU MẪU
   ===================================================================== */
INSERT INTO dbo.PhongBan (TenPB, Mota) VALUES
(N'Phòng Nhân sự',     N'Quản lý hồ sơ, tuyển dụng, chấm công'),
(N'Phòng Kế toán',     N'Tài chính, kế toán, tính lương'),
(N'Phòng Kỹ thuật',    N'Phát triển và vận hành hệ thống'),
(N'Phòng Kinh doanh',  N'Bán hàng và chăm sóc khách hàng');

INSERT INTO dbo.NhanVien (HoTen, NgaySinh, CCCD, MaPB, LuongCoBan, HeSoLuong) VALUES
(N'Nguyễn Thị Lan',    '1990-03-15', '001090000001', 1, 12000000, 1.2),
(N'Trần Văn Minh',     '1988-07-22', '001088000002', 2, 14000000, 1.5),
(N'Lê Hoàng Nam',      '1995-11-05', '001095000003', 3, 15000000, 1.3),
(N'Phạm Thu Hằng',     '1997-01-30', '001097000004', 3, 13000000, 1.0),
(N'Đặng Quốc Việt',    '1992-09-12', '001092000005', 4, 11000000, 1.1),
(N'Vũ Ngọc Anh',       '1999-05-20', '001099000006', 4, 10000000, 1.0),
(N'Bùi Thanh Tùng',    '1985-12-08', '001085000007', 2, 18000000, 2.0),
(N'Hoàng Mai Phương',  '1998-04-17', '001098000008', 1, 10500000, 1.0);

-- Chấm công mẫu tháng 10/2026 (đã chốt) để minh họa dữ liệu bị khóa
INSERT INTO dbo.BangChamCong (MaNV, ThangNam, SoNgayCong, SoGioOT, DaChot) VALUES
(1, '10/2026', 22, 4, 1),
(2, '10/2026', 21, 0, 1),
(3, '10/2026', 22, 10, 1);

INSERT INTO dbo.BangLuong (MaNV, ThangNam, ThucLinh) VALUES
(1, '10/2026', ROUND((12000000 * 1.2 / 22.0) * 22 + 4  * 50000, 0)),
(2, '10/2026', ROUND((14000000 * 1.5 / 22.0) * 21 + 0  * 50000, 0)),
(3, '10/2026', ROUND((15000000 * 1.3 / 22.0) * 22 + 10 * 50000, 0));
GO

SELECT N'Tạo HRMDB thành công' AS KetQua;
GO
