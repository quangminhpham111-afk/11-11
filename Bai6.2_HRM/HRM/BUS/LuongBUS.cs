using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using HRM.DAL;
using HRM.DTO;

namespace HRM.BUS
{
    public class LuongBUS
    {
        public const decimal NgayCongChuan = 22m;      // số ngày công chuẩn / tháng
        public const decimal DonGiaOT = 50000m;        // đơn giá mỗi giờ OT

        private readonly LuongDAL dal = new LuongDAL();

        /// <summary>
        /// ThucLinh = (LuongCoBan * HeSoLuong / 22) * SoNgayCong + (SoGioOT * 50,000)
        /// Làm tròn đến đồng.
        /// </summary>
        public static decimal TinhThucLinh(decimal luongCoBan, double heSoLuong, double soNgayCong, double soGioOT)
        {
            decimal luongNgay = luongCoBan * (decimal)heSoLuong / NgayCongChuan;
            decimal thucLinh = luongNgay * (decimal)soNgayCong + (decimal)soGioOT * DonGiaOT;
            return Math.Round(thucLinh, 0, MidpointRounding.AwayFromZero);
        }

        public bool DaChot(string thangNam) { return dal.DaChotLuong(thangNam); }

        private List<BangLuongDTO> Tinh(string thangNam)
        {
            List<DuLieuTinhLuongDTO> du = dal.GetDuLieuTinhLuong(thangNam);
            return du.Select(x => new BangLuongDTO
            {
                MaNV = x.MaNV,
                HoTen = x.HoTen,
                ThangNam = thangNam,
                SoNgayCong = x.SoNgayCong,
                SoGioOT = x.SoGioOT,
                ThucLinh = TinhThucLinh(x.LuongCoBan, x.HeSoLuong, x.SoNgayCong, x.SoGioOT)
            }).ToList();
        }

        /// <summary>Xem trước bảng lương (tính trong bộ nhớ, chưa ghi DB).</summary>
        public List<BangLuongDTO> XemTruoc(string thangNam)
        {
            try
            {
                List<BangLuongDTO> ds = Tinh(thangNam);
                if (ds.Count == 0)
                    throw new BusinessException("Tháng " + thangNam + " chưa có dữ liệu chấm công. Hãy lưu chấm công trước.");
                return ds;
            }
            catch (SqlException ex) { throw BusinessException.FromSql(ex, "Bảng lương"); }
        }

        /// <summary>Bảng lương đã chốt (đọc từ DB).</summary>
        public List<BangLuongDTO> GetBangLuong(string thangNam)
        {
            try { return dal.GetBangLuong(thangNam); }
            catch (SqlException ex) { throw BusinessException.FromSql(ex, "Bảng lương"); }
        }

        /// <summary>Chốt lương: tính lương rồi ghi bảng lương + khóa chấm công trong một SqlTransaction.</summary>
        public List<BangLuongDTO> ChotLuong(string thangNam)
        {
            try
            {
                if (dal.DaChotLuong(thangNam))
                    throw new BusinessException("Tháng " + thangNam + " đã được chốt lương rồi.");

                List<BangLuongDTO> ds = Tinh(thangNam);
                if (ds.Count == 0)
                    throw new BusinessException("Tháng " + thangNam + " chưa có dữ liệu chấm công để chốt lương.");

                dal.ChotLuong(thangNam, ds);      // SqlTransaction nằm trong DAL
                return ds;
            }
            catch (InvalidOperationException ex)
            {
                throw new BusinessException(ex.Message + "\nGiao dịch đã được ROLLBACK.", ex);
            }
            catch (SqlException ex)
            {
                throw new BusinessException("Lỗi CSDL, giao dịch chốt lương đã được ROLLBACK: " + ex.Message, ex);
            }
        }
    }
}
