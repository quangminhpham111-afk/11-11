using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using RetailSales.DAL;
using RetailSales.DTO;

namespace RetailSales.BUS
{
    public class BanHangBUS
    {
        private readonly HoaDonDAL dal = new HoaDonDAL();

        /// <summary>Thêm sản phẩm vào giỏ (gộp dòng nếu trùng mã), kiểm tra tồn kho.</summary>
        public static void ThemVaoGio(IList<CartItemDTO> gio, SanPhamDTO sp, int soLuong)
        {
            if (sp == null) throw new BusinessException("Vui lòng chọn sản phẩm.");
            if (soLuong <= 0) throw new BusinessException("Số lượng phải lớn hơn 0.");

            CartItemDTO dong = gio.FirstOrDefault(x => x.MaSP == sp.MaSP);
            int daCo = dong == null ? 0 : dong.SoLuong;
            if (daCo + soLuong > sp.SoLuongTon)
                throw new BusinessException("Không đủ hàng: \"" + sp.TenSP + "\" chỉ còn " + sp.SoLuongTon +
                                            " (trong giỏ đã có " + daCo + ").");

            if (dong == null)
                gio.Add(new CartItemDTO { MaSP = sp.MaSP, TenSP = sp.TenSP, SoLuong = soLuong, DonGia = sp.GiaBan });
            else
                dong.SoLuong += soLuong;
        }

        public static decimal TinhTongTien(IEnumerable<CartItemDTO> gio)
        {
            return gio.Sum(x => x.ThanhTien);
        }

        /// <summary>Điểm tích lũy = TongTien / 100000 (làm tròn xuống).</summary>
        public static int TinhDiemTichLuy(decimal tongTien)
        {
            return (int)(tongTien / 100000m);
        }

        /// <summary>Thanh toán: gọi DAL thực hiện SqlTransaction. maKH = 0 nghĩa là khách lẻ.</summary>
        public int ThanhToan(int maNV, int maKH, IList<CartItemDTO> gio)
        {
            if (maNV <= 0) throw new BusinessException("Vui lòng chọn nhân viên thu ngân.");
            if (gio == null || gio.Count == 0) throw new BusinessException("Giỏ hàng đang trống.");
            if (gio.Any(x => x.SoLuong <= 0)) throw new BusinessException("Số lượng mỗi dòng phải lớn hơn 0.");

            decimal tong = TinhTongTien(gio);
            try
            {
                return dal.TaoHoaDon(maNV, maKH > 0 ? (int?)maKH : null, gio, tong);
            }
            catch (InvalidOperationException ex)
            {
                // Không đủ tồn kho: DAL đã Rollback toàn bộ giao dịch
                throw new BusinessException(ex.Message + "\nGiao dịch đã được ROLLBACK, không có dữ liệu nào bị thay đổi.", ex);
            }
            catch (SqlException ex)
            {
                throw new BusinessException("Lỗi CSDL, giao dịch đã được ROLLBACK: " + ex.Message, ex);
            }
        }
    }
}
