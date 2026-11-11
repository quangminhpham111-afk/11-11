using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using RetailSales.DTO;

namespace RetailSales.DAL
{
    public class NhanVienDAL
    {
        private const string SelectSql = "SELECT MaNV, TenNV, SoDienThoai, ChucVu FROM NhanVien";

        private static List<NhanVienDTO> ReadList(SqlCommand cmd)
        {
            var list = new List<NhanVienDTO>();
            using (SqlDataReader rd = cmd.ExecuteReader())
            {
                while (rd.Read())
                {
                    list.Add(new NhanVienDTO
                    {
                        MaNV = rd.GetInt32(0),
                        TenNV = rd.GetString(1),
                        SoDienThoai = rd.IsDBNull(2) ? null : rd.GetString(2),
                        ChucVu = rd.IsDBNull(3) ? null : rd.GetString(3)
                    });
                }
            }
            return list;
        }

        public List<NhanVienDTO> GetAll()
        {
            using (SqlConnection conn = DbHelper.CreateConnection())
            using (SqlCommand cmd = new SqlCommand(SelectSql + " ORDER BY MaNV", conn))
            {
                conn.Open();
                return ReadList(cmd);
            }
        }

        public List<NhanVienDTO> Search(string keyword)
        {
            const string sql = SelectSql +
                " WHERE TenNV LIKE @kw ESCAPE '\\' OR SoDienThoai LIKE @kw ESCAPE '\\' ORDER BY MaNV";
            using (SqlConnection conn = DbHelper.CreateConnection())
            using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.Add("@kw", SqlDbType.NVarChar, 200).Value = DbHelper.LikePattern(keyword);
                conn.Open();
                return ReadList(cmd);
            }
        }

        public int Insert(NhanVienDTO nv)
        {
            const string sql = "INSERT INTO NhanVien (TenNV, SoDienThoai, ChucVu) VALUES (@ten, @sdt, @cv)";
            using (SqlConnection conn = DbHelper.CreateConnection())
            using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.Add("@ten", SqlDbType.NVarChar, 100).Value = nv.TenNV;
                cmd.Parameters.Add("@sdt", SqlDbType.VarChar, 15).Value = DbHelper.NullIfEmpty(nv.SoDienThoai);
                cmd.Parameters.Add("@cv", SqlDbType.NVarChar, 50).Value = DbHelper.NullIfEmpty(nv.ChucVu);
                conn.Open();
                return cmd.ExecuteNonQuery();
            }
        }

        public int Update(NhanVienDTO nv)
        {
            const string sql = "UPDATE NhanVien SET TenNV = @ten, SoDienThoai = @sdt, ChucVu = @cv WHERE MaNV = @ma";
            using (SqlConnection conn = DbHelper.CreateConnection())
            using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.Add("@ma", SqlDbType.Int).Value = nv.MaNV;
                cmd.Parameters.Add("@ten", SqlDbType.NVarChar, 100).Value = nv.TenNV;
                cmd.Parameters.Add("@sdt", SqlDbType.VarChar, 15).Value = DbHelper.NullIfEmpty(nv.SoDienThoai);
                cmd.Parameters.Add("@cv", SqlDbType.NVarChar, 50).Value = DbHelper.NullIfEmpty(nv.ChucVu);
                conn.Open();
                return cmd.ExecuteNonQuery();
            }
        }

        public int Delete(int maNV)
        {
            using (SqlConnection conn = DbHelper.CreateConnection())
            using (SqlCommand cmd = new SqlCommand("DELETE FROM NhanVien WHERE MaNV = @ma", conn))
            {
                cmd.Parameters.Add("@ma", SqlDbType.Int).Value = maNV;
                conn.Open();
                return cmd.ExecuteNonQuery();
            }
        }
    }
}
