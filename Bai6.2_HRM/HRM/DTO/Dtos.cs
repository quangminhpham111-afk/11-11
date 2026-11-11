using System;

namespace HRM.DTO
{
    public class PhongBanDTO
    {
        public int MaPB { get; set; }
        public string TenPB { get; set; }
        public string Mota { get; set; }
    }

    public class NhanVienDTO
    {
        public int MaNV { get; set; }
        public string HoTen { get; set; }
        public DateTime? NgaySinh { get; set; }
        public string CCCD { get; set; }
        public int MaPB { get; set; }
        public string TenPB { get; set; }          // chỉ để hiển thị (JOIN)
        public decimal LuongCoBan { get; set; }
        public double HeSoLuong { get; set; }
    }

    /// <summary>Dữ liệu để tính lương: chấm công + thông tin lương của nhân viên.</summary>
    public class DuLieuTinhLuongDTO
    {
        public int MaNV { get; set; }
        public string HoTen { get; set; }
        public decimal LuongCoBan { get; set; }
        public double HeSoLuong { get; set; }
        public double SoNgayCong { get; set; }
        public double SoGioOT { get; set; }
    }

    public class BangLuongDTO
    {
        public int MaNV { get; set; }
        public string HoTen { get; set; }
        public string ThangNam { get; set; }
        public double SoNgayCong { get; set; }
        public double SoGioOT { get; set; }
        public decimal ThucLinh { get; set; }
    }
}
