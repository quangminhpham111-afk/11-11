namespace RetailSales.DTO
{
    public class NhanVienDTO
    {
        public int MaNV { get; set; }
        public string TenNV { get; set; }
        public string SoDienThoai { get; set; }
        public string ChucVu { get; set; }
    }

    public class KhachHangDTO
    {
        public int MaKH { get; set; }          // 0 = Khách lẻ (không lưu DB)
        public string TenKH { get; set; }
        public string DienThoai { get; set; }
        public int DiemTichLuy { get; set; }

        public string HienThi
        {
            get { return string.IsNullOrEmpty(DienThoai) ? TenKH : TenKH + " - " + DienThoai; }
        }
    }

    public class SanPhamDTO
    {
        public string MaSP { get; set; }
        public string TenSP { get; set; }
        public decimal GiaBan { get; set; }
        public int SoLuongTon { get; set; }

        public string HienThi
        {
            get { return MaSP + " - " + TenSP + " (tồn " + SoLuongTon + ")"; }
        }
    }

    /// <summary>Một dòng trong giỏ hàng (DataGridView).</summary>
    public class CartItemDTO
    {
        public string MaSP { get; set; }
        public string TenSP { get; set; }
        public int SoLuong { get; set; }
        public decimal DonGia { get; set; }
        public decimal ThanhTien { get { return SoLuong * DonGia; } }
    }

    public class TopSanPhamDTO
    {
        public string MaSP { get; set; }
        public string TenSP { get; set; }
        public int TongSoLuong { get; set; }
        public decimal DoanhThu { get; set; }
    }
}
