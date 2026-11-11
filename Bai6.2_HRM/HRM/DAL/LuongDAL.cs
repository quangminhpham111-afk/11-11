using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using HRM.DTO;

namespace HRM.DAL
{
    public class LuongDAL
    {
        /// <summary>Tháng đã có bảng lương nghĩa là đã chốt.</summary>
        public bool DaChotLuong(string thangNam)
        {
            using (SqlConnection conn = DbHelper.CreateConnection())
            using (SqlCommand cmd = new SqlCommand("SELECT COUNT(*) FROM BangLuong WHERE ThangNam = @tn", conn))
            {
                cmd.Parameters.Add("@tn", SqlDbType.VarChar, 7).Value = thangNam;
                conn.Open();
                return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
            }
        }

        /// <summary>Lấy chấm công + lương cơ bản/hệ số của các nhân viên có trong tháng.</summary>
        public List<DuLieuTinhLuongDTO> GetDuLieuTinhLuong(string thangNam)
        {
            const string sql =
                "SELECT nv.MaNV, nv.HoTen, nv.LuongCoBan, nv.HeSoLuong, cc.SoNgayCong, cc.SoGioOT " +
                "FROM BangChamCong cc JOIN NhanVien nv ON nv.MaNV = cc.MaNV " +
                "WHERE cc.ThangNam = @tn ORDER BY nv.MaNV";
            var list = new List<DuLieuTinhLuongDTO>();
            using (SqlConnection conn = DbHelper.CreateConnection())
            using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.Add("@tn", SqlDbType.VarChar, 7).Value = thangNam;
                conn.Open();
                using (SqlDataReader rd = cmd.ExecuteReader())
                {
                    while (rd.Read())
                    {
                        list.Add(new DuLieuTinhLuongDTO
                        {
                            MaNV = rd.GetInt32(0),
                            HoTen = rd.GetString(1),
                            LuongCoBan = rd.GetDecimal(2),
                            HeSoLuong = rd.GetDouble(3),
                            SoNgayCong = rd.GetDouble(4),
                            SoGioOT = rd.GetDouble(5)
                        });
                    }
                }
            }
            return list;
        }

        /// <summary>Đọc bảng lương đã chốt của tháng.</summary>
        public List<BangLuongDTO> GetBangLuong(string thangNam)
        {
            const string sql =
                "SELECT bl.MaNV, nv.HoTen, bl.ThangNam, cc.SoNgayCong, cc.SoGioOT, bl.ThucLinh " +
                "FROM BangLuong bl JOIN NhanVien nv ON nv.MaNV = bl.MaNV " +
                "LEFT JOIN BangChamCong cc ON cc.MaNV = bl.MaNV AND cc.ThangNam = bl.ThangNam " +
                "WHERE bl.ThangNam = @tn ORDER BY bl.MaNV";
            var list = new List<BangLuongDTO>();
            using (SqlConnection conn = DbHelper.CreateConnection())
            using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.Add("@tn", SqlDbType.VarChar, 7).Value = thangNam;
                conn.Open();
                using (SqlDataReader rd = cmd.ExecuteReader())
                {
                    while (rd.Read())
                    {
                        list.Add(new BangLuongDTO
                        {
                            MaNV = rd.GetInt32(0),
                            HoTen = rd.GetString(1),
                            ThangNam = rd.GetString(2),
                            SoNgayCong = rd.IsDBNull(3) ? 0 : rd.GetDouble(3),
                            SoGioOT = rd.IsDBNull(4) ? 0 : rd.GetDouble(4),
                            ThucLinh = rd.GetDecimal(5)
                        });
                    }
                }
            }
            return list;
        }

        /// <summary>
        /// CHỐT LƯƠNG trong một SqlTransaction:
        ///   1. Kiểm tra tháng chưa chốt
        ///   2. Ghi bảng lương cho toàn bộ nhân viên của tháng
        ///   3. Khóa dữ liệu chấm công (DaChot = 1)
        ///   4. Commit; lỗi bất kỳ -> Rollback
        /// </summary>
        public void ChotLuong(string thangNam, IList<BangLuongDTO> bangLuong)
        {
            using (SqlConnection conn = DbHelper.CreateConnection())
            {
                conn.Open();
                SqlTransaction trans = conn.BeginTransaction();
                try
                {
                    // 1. Chống chốt trùng (khóa phạm vi để 2 người không chốt cùng lúc)
                    using (SqlCommand cmd = new SqlCommand(
                        "SELECT COUNT(*) FROM BangLuong WITH (UPDLOCK, HOLDLOCK) WHERE ThangNam = @tn", conn, trans))
                    {
                        cmd.Parameters.Add("@tn", SqlDbType.VarChar, 7).Value = thangNam;
                        if (Convert.ToInt32(cmd.ExecuteScalar()) > 0)
                            throw new InvalidOperationException("Tháng " + thangNam + " đã được chốt lương trước đó.");
                    }

                    // 2. Ghi bảng lương
                    foreach (BangLuongDTO bl in bangLuong)
                    {
                        using (SqlCommand cmd = new SqlCommand(
                            "INSERT INTO BangLuong (MaNV, ThangNam, ThucLinh) VALUES (@nv, @tn, @tl)", conn, trans))
                        {
                            cmd.Parameters.Add("@nv", SqlDbType.Int).Value = bl.MaNV;
                            cmd.Parameters.Add("@tn", SqlDbType.VarChar, 7).Value = thangNam;
                            DbHelper.AddDecimal(cmd, "@tl", bl.ThucLinh);
                            cmd.ExecuteNonQuery();
                        }
                    }

                    // 3. Khóa chấm công của tháng
                    using (SqlCommand cmd = new SqlCommand(
                        "UPDATE BangChamCong SET DaChot = 1 WHERE ThangNam = @tn", conn, trans))
                    {
                        cmd.Parameters.Add("@tn", SqlDbType.VarChar, 7).Value = thangNam;
                        cmd.ExecuteNonQuery();
                    }

                    // 4. Hoàn tất
                    trans.Commit();
                }
                catch
                {
                    try { trans.Rollback(); } catch { }
                    throw;
                }
                finally
                {
                    trans.Dispose();
                }
            }
        }
    }
}
