using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using HRM.DTO;

namespace HRM.DAL
{
    public class NhanVienDAL
    {
        private const string SelectSql =
            "SELECT nv.MaNV, nv.HoTen, nv.NgaySinh, nv.CCCD, nv.MaPB, pb.TenPB, nv.LuongCoBan, nv.HeSoLuong " +
            "FROM NhanVien nv JOIN PhongBan pb ON pb.MaPB = nv.MaPB";

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
                        HoTen = rd.GetString(1),
                        NgaySinh = rd.IsDBNull(2) ? (DateTime?)null : rd.GetDateTime(2),
                        CCCD = rd.IsDBNull(3) ? null : rd.GetString(3),
                        MaPB = rd.GetInt32(4),
                        TenPB = rd.GetString(5),
                        LuongCoBan = rd.GetDecimal(6),
                        HeSoLuong = rd.GetDouble(7)
                    });
                }
            }
            return list;
        }

        public List<NhanVienDTO> GetAll()
        {
            using (SqlConnection conn = DbHelper.CreateConnection())
            using (SqlCommand cmd = new SqlCommand(SelectSql + " ORDER BY nv.MaNV", conn))
            {
                conn.Open();
                return ReadList(cmd);
            }
        }

        /// <summary>
        /// Tìm theo tên / CCCD / tên phòng ban (parameterized + LIKE), có thể lọc thêm theo mã phòng ban (0 = tất cả).
        /// </summary>
        public List<NhanVienDTO> Search(string keyword, int maPB)
        {
            const string sql = SelectSql +
                " WHERE (nv.HoTen LIKE @kw ESCAPE '\\' OR nv.CCCD LIKE @kw ESCAPE '\\' OR pb.TenPB LIKE @kw ESCAPE '\\')" +
                " AND (@pb = 0 OR nv.MaPB = @pb) ORDER BY nv.MaNV";
            using (SqlConnection conn = DbHelper.CreateConnection())
            using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.Add("@kw", SqlDbType.NVarChar, 200).Value = DbHelper.LikePattern(keyword);
                cmd.Parameters.Add("@pb", SqlDbType.Int).Value = maPB;
                conn.Open();
                return ReadList(cmd);
            }
        }

        private static void AddCommonParams(SqlCommand cmd, NhanVienDTO nv)
        {
            cmd.Parameters.Add("@ten", SqlDbType.NVarChar, 100).Value = nv.HoTen;
            cmd.Parameters.Add("@ns", SqlDbType.DateTime).Value =
                nv.NgaySinh.HasValue ? (object)nv.NgaySinh.Value : DBNull.Value;
            cmd.Parameters.Add("@cccd", SqlDbType.VarChar, 20).Value = DbHelper.NullIfEmpty(nv.CCCD);
            cmd.Parameters.Add("@pb", SqlDbType.Int).Value = nv.MaPB;
            DbHelper.AddDecimal(cmd, "@luong", nv.LuongCoBan);
            cmd.Parameters.Add("@hs", SqlDbType.Float).Value = nv.HeSoLuong;
        }

        public int Insert(NhanVienDTO nv)
        {
            const string sql =
                "INSERT INTO NhanVien (HoTen, NgaySinh, CCCD, MaPB, LuongCoBan, HeSoLuong) " +
                "VALUES (@ten, @ns, @cccd, @pb, @luong, @hs)";
            using (SqlConnection conn = DbHelper.CreateConnection())
            using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                AddCommonParams(cmd, nv);
                conn.Open();
                return cmd.ExecuteNonQuery();
            }
        }

        public int Update(NhanVienDTO nv)
        {
            const string sql =
                "UPDATE NhanVien SET HoTen = @ten, NgaySinh = @ns, CCCD = @cccd, MaPB = @pb, " +
                "LuongCoBan = @luong, HeSoLuong = @hs WHERE MaNV = @ma";
            using (SqlConnection conn = DbHelper.CreateConnection())
            using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                AddCommonParams(cmd, nv);
                cmd.Parameters.Add("@ma", SqlDbType.Int).Value = nv.MaNV;
                conn.Open();
                return cmd.ExecuteNonQuery();
            }
        }

        public int ChuyenPhongBan(int maNV, int maPBMoi)
        {
            using (SqlConnection conn = DbHelper.CreateConnection())
            using (SqlCommand cmd = new SqlCommand("UPDATE NhanVien SET MaPB = @pb WHERE MaNV = @ma", conn))
            {
                cmd.Parameters.Add("@pb", SqlDbType.Int).Value = maPBMoi;
                cmd.Parameters.Add("@ma", SqlDbType.Int).Value = maNV;
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
