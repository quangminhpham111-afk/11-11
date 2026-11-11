using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using RetailSales.DTO;

namespace RetailSales.DAL
{
    public class KhachHangDAL
    {
        private const string SelectSql = "SELECT MaKH, TenKH, DienThoai, DiemTichLuy FROM KhachHang";

        private static List<KhachHangDTO> ReadList(SqlCommand cmd)
        {
            var list = new List<KhachHangDTO>();
            using (SqlDataReader rd = cmd.ExecuteReader())
            {
                while (rd.Read())
                {
                    list.Add(new KhachHangDTO
                    {
                        MaKH = rd.GetInt32(0),
                        TenKH = rd.GetString(1),
                        DienThoai = rd.IsDBNull(2) ? null : rd.GetString(2),
                        DiemTichLuy = rd.GetInt32(3)
                    });
                }
            }
            return list;
        }

        public List<KhachHangDTO> GetAll()
        {
            using (SqlConnection conn = DbHelper.CreateConnection())
            using (SqlCommand cmd = new SqlCommand(SelectSql + " ORDER BY MaKH", conn))
            {
                conn.Open();
                return ReadList(cmd);
            }
        }

        public List<KhachHangDTO> Search(string keyword)
        {
            const string sql = SelectSql +
                " WHERE TenKH LIKE @kw ESCAPE '\\' OR DienThoai LIKE @kw ESCAPE '\\' ORDER BY MaKH";
            using (SqlConnection conn = DbHelper.CreateConnection())
            using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.Add("@kw", SqlDbType.NVarChar, 200).Value = DbHelper.LikePattern(keyword);
                conn.Open();
                return ReadList(cmd);
            }
        }

        public int Insert(KhachHangDTO kh)
        {
            const string sql = "INSERT INTO KhachHang (TenKH, DienThoai, DiemTichLuy) VALUES (@ten, @dt, @diem)";
            using (SqlConnection conn = DbHelper.CreateConnection())
            using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.Add("@ten", SqlDbType.NVarChar, 100).Value = kh.TenKH;
                cmd.Parameters.Add("@dt", SqlDbType.VarChar, 15).Value = DbHelper.NullIfEmpty(kh.DienThoai);
                cmd.Parameters.Add("@diem", SqlDbType.Int).Value = kh.DiemTichLuy;
                conn.Open();
                return cmd.ExecuteNonQuery();
            }
        }

        public int Update(KhachHangDTO kh)
        {
            const string sql = "UPDATE KhachHang SET TenKH = @ten, DienThoai = @dt, DiemTichLuy = @diem WHERE MaKH = @ma";
            using (SqlConnection conn = DbHelper.CreateConnection())
            using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.Add("@ma", SqlDbType.Int).Value = kh.MaKH;
                cmd.Parameters.Add("@ten", SqlDbType.NVarChar, 100).Value = kh.TenKH;
                cmd.Parameters.Add("@dt", SqlDbType.VarChar, 15).Value = DbHelper.NullIfEmpty(kh.DienThoai);
                cmd.Parameters.Add("@diem", SqlDbType.Int).Value = kh.DiemTichLuy;
                conn.Open();
                return cmd.ExecuteNonQuery();
            }
        }

        public int Delete(int maKH)
        {
            using (SqlConnection conn = DbHelper.CreateConnection())
            using (SqlCommand cmd = new SqlCommand("DELETE FROM KhachHang WHERE MaKH = @ma", conn))
            {
                cmd.Parameters.Add("@ma", SqlDbType.Int).Value = maKH;
                conn.Open();
                return cmd.ExecuteNonQuery();
            }
        }
    }
}
