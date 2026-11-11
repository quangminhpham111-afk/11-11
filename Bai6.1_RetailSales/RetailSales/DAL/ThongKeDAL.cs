using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using RetailSales.DTO;

namespace RetailSales.DAL
{
    public class ThongKeDAL
    {
        private static decimal Scalar(string sql, DateTime tu, DateTime den)
        {
            using (SqlConnection conn = DbHelper.CreateConnection())
            using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.Add("@tu", SqlDbType.DateTime).Value = tu;
                cmd.Parameters.Add("@den", SqlDbType.DateTime).Value = den;
                conn.Open();
                object o = cmd.ExecuteScalar();                 // ExecuteScalar
                return (o == null || o == DBNull.Value) ? 0m : Convert.ToDecimal(o);
            }
        }

        private const string RevenueSql =
            "SELECT ISNULL(SUM(TongTien), 0) FROM HoaDon WHERE NgayLap >= @tu AND NgayLap < @den";

        public decimal DoanhThuNgay(DateTime ngay)
        {
            DateTime tu = ngay.Date;
            return Scalar(RevenueSql, tu, tu.AddDays(1));
        }

        public decimal DoanhThuThang(int thang, int nam)
        {
            DateTime tu = new DateTime(nam, thang, 1);
            return Scalar(RevenueSql, tu, tu.AddMonths(1));
        }

        /// <summary>Top N sản phẩm bán chạy nhất, đọc bằng SqlDataReader.</summary>
        public List<TopSanPhamDTO> TopSanPhamBanChay(int top)
        {
            const string sql =
                "SELECT TOP (@top) sp.MaSP, sp.TenSP, SUM(ct.SoLuong) AS TongSL, SUM(ct.SoLuong * ct.DonGia) AS DoanhThu " +
                "FROM ChiTietHoaDon ct JOIN SanPham sp ON sp.MaSP = ct.MaSP " +
                "GROUP BY sp.MaSP, sp.TenSP " +
                "ORDER BY TongSL DESC, DoanhThu DESC";
            var list = new List<TopSanPhamDTO>();
            using (SqlConnection conn = DbHelper.CreateConnection())
            using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.Add("@top", SqlDbType.Int).Value = top;
                conn.Open();
                using (SqlDataReader rd = cmd.ExecuteReader())
                {
                    while (rd.Read())
                    {
                        list.Add(new TopSanPhamDTO
                        {
                            MaSP = rd.GetString(0),
                            TenSP = rd.GetString(1),
                            TongSoLuong = rd.GetInt32(2),
                            DoanhThu = rd.GetDecimal(3)
                        });
                    }
                }
            }
            return list;
        }
    }
}
