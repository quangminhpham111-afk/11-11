using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using RetailSales.DTO;

namespace RetailSales.DAL
{
    public class SanPhamDAL
    {
        private const string SelectSql = "SELECT MaSP, TenSP, GiaBan, SoLuongTon FROM SanPham";

        private static SanPhamDTO Map(SqlDataReader rd)
        {
            return new SanPhamDTO
            {
                MaSP = rd.GetString(0),
                TenSP = rd.GetString(1),
                GiaBan = rd.GetDecimal(2),
                SoLuongTon = rd.GetInt32(3)
            };
        }

        private static List<SanPhamDTO> ReadList(SqlCommand cmd)
        {
            var list = new List<SanPhamDTO>();
            using (SqlDataReader rd = cmd.ExecuteReader())
            {
                while (rd.Read()) list.Add(Map(rd));
            }
            return list;
        }

        public List<SanPhamDTO> GetAll()
        {
            using (SqlConnection conn = DbHelper.CreateConnection())
            using (SqlCommand cmd = new SqlCommand(SelectSql + " ORDER BY MaSP", conn))
            {
                conn.Open();
                return ReadList(cmd);
            }
        }

        /// <summary>Tìm theo tên hoặc mã vạch, dùng SqlParameter + ký tự đại diện %.</summary>
        public List<SanPhamDTO> Search(string keyword)
        {
            const string sql = SelectSql +
                " WHERE TenSP LIKE @kw ESCAPE '\\' OR MaSP LIKE @kw ESCAPE '\\' ORDER BY MaSP";
            using (SqlConnection conn = DbHelper.CreateConnection())
            using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.Add("@kw", SqlDbType.NVarChar, 200).Value = DbHelper.LikePattern(keyword);
                conn.Open();
                return ReadList(cmd);
            }
        }

        public SanPhamDTO GetById(string maSP)
        {
            using (SqlConnection conn = DbHelper.CreateConnection())
            using (SqlCommand cmd = new SqlCommand(SelectSql + " WHERE MaSP = @ma", conn))
            {
                cmd.Parameters.Add("@ma", SqlDbType.VarChar, 20).Value = maSP;
                conn.Open();
                List<SanPhamDTO> list = ReadList(cmd);
                return list.Count > 0 ? list[0] : null;
            }
        }

        public int Insert(SanPhamDTO sp)
        {
            const string sql = "INSERT INTO SanPham (MaSP, TenSP, GiaBan, SoLuongTon) VALUES (@ma, @ten, @gia, @ton)";
            using (SqlConnection conn = DbHelper.CreateConnection())
            using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.Add("@ma", SqlDbType.VarChar, 20).Value = sp.MaSP;
                cmd.Parameters.Add("@ten", SqlDbType.NVarChar, 150).Value = sp.TenSP;
                DbHelper.AddDecimal(cmd, "@gia", sp.GiaBan);
                cmd.Parameters.Add("@ton", SqlDbType.Int).Value = sp.SoLuongTon;
                conn.Open();
                return cmd.ExecuteNonQuery();
            }
        }

        public int Update(SanPhamDTO sp)
        {
            const string sql = "UPDATE SanPham SET TenSP = @ten, GiaBan = @gia, SoLuongTon = @ton WHERE MaSP = @ma";
            using (SqlConnection conn = DbHelper.CreateConnection())
            using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.Add("@ma", SqlDbType.VarChar, 20).Value = sp.MaSP;
                cmd.Parameters.Add("@ten", SqlDbType.NVarChar, 150).Value = sp.TenSP;
                DbHelper.AddDecimal(cmd, "@gia", sp.GiaBan);
                cmd.Parameters.Add("@ton", SqlDbType.Int).Value = sp.SoLuongTon;
                conn.Open();
                return cmd.ExecuteNonQuery();
            }
        }

        public int Delete(string maSP)
        {
            using (SqlConnection conn = DbHelper.CreateConnection())
            using (SqlCommand cmd = new SqlCommand("DELETE FROM SanPham WHERE MaSP = @ma", conn))
            {
                cmd.Parameters.Add("@ma", SqlDbType.VarChar, 20).Value = maSP;
                conn.Open();
                return cmd.ExecuteNonQuery();
            }
        }
    }
}
