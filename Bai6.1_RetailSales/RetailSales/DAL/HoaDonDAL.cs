using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using RetailSales.DTO;

namespace RetailSales.DAL
{
    public class HoaDonDAL
    {
        /// <summary>
        /// Lập hóa đơn trong MỘT SqlTransaction (ACID):
        ///   1. Chèn HoaDon
        ///   2. Chèn ChiTietHoaDon
        ///   3. Trừ SoLuongTon (nếu tồn &lt; số lượng bán -> Rollback + báo lỗi)
        ///   4. Cộng DiemTichLuy cho khách (TongTien / 100000)
        ///   5. Commit
        /// Trả về MaHD vừa tạo. Ném InvalidOperationException khi không đủ tồn kho.
        /// </summary>
        public int TaoHoaDon(int maNV, int? maKH, IList<CartItemDTO> items, decimal tongTien)
        {
            using (SqlConnection conn = DbHelper.CreateConnection())
            {
                conn.Open();
                SqlTransaction trans = conn.BeginTransaction();
                try
                {
                    // 1. Chèn hóa đơn
                    int maHD;
                    const string sqlHD =
                        "INSERT INTO HoaDon (NgayLap, MaNV, MaKH, TongTien) " +
                        "OUTPUT INSERTED.MaHD VALUES (GETDATE(), @nv, @kh, @tong)";
                    using (SqlCommand cmd = new SqlCommand(sqlHD, conn, trans))
                    {
                        cmd.Parameters.Add("@nv", SqlDbType.Int).Value = maNV;
                        cmd.Parameters.Add("@kh", SqlDbType.Int).Value =
                            maKH.HasValue ? (object)maKH.Value : DBNull.Value;
                        DbHelper.AddDecimal(cmd, "@tong", tongTien);
                        maHD = Convert.ToInt32(cmd.ExecuteScalar());
                    }

                    foreach (CartItemDTO item in items)
                    {
                        // 3a. Khóa dòng sản phẩm và đọc tồn kho hiện tại
                        int ton;
                        const string sqlTon =
                            "SELECT SoLuongTon FROM SanPham WITH (UPDLOCK, ROWLOCK) WHERE MaSP = @ma";
                        using (SqlCommand cmd = new SqlCommand(sqlTon, conn, trans))
                        {
                            cmd.Parameters.Add("@ma", SqlDbType.VarChar, 20).Value = item.MaSP;
                            object o = cmd.ExecuteScalar();
                            if (o == null)
                                throw new InvalidOperationException("Sản phẩm '" + item.MaSP + "' không tồn tại.");
                            ton = Convert.ToInt32(o);
                        }

                        // 3b. Không đủ hàng -> lỗi (catch bên dưới sẽ Rollback)
                        if (ton < item.SoLuong)
                        {
                            throw new InvalidOperationException(
                                "Sản phẩm \"" + item.TenSP + "\" không đủ tồn kho (còn " + ton +
                                ", cần bán " + item.SoLuong + ").");
                        }

                        // 2. Chèn chi tiết hóa đơn
                        const string sqlCT =
                            "INSERT INTO ChiTietHoaDon (MaHD, MaSP, SoLuong, DonGia) VALUES (@hd, @ma, @sl, @gia)";
                        using (SqlCommand cmd = new SqlCommand(sqlCT, conn, trans))
                        {
                            cmd.Parameters.Add("@hd", SqlDbType.Int).Value = maHD;
                            cmd.Parameters.Add("@ma", SqlDbType.VarChar, 20).Value = item.MaSP;
                            cmd.Parameters.Add("@sl", SqlDbType.Int).Value = item.SoLuong;
                            DbHelper.AddDecimal(cmd, "@gia", item.DonGia);
                            cmd.ExecuteNonQuery();
                        }

                        // 3c. Trừ tồn kho
                        const string sqlTru =
                            "UPDATE SanPham SET SoLuongTon = SoLuongTon - @sl WHERE MaSP = @ma";
                        using (SqlCommand cmd = new SqlCommand(sqlTru, conn, trans))
                        {
                            cmd.Parameters.Add("@sl", SqlDbType.Int).Value = item.SoLuong;
                            cmd.Parameters.Add("@ma", SqlDbType.VarChar, 20).Value = item.MaSP;
                            cmd.ExecuteNonQuery();
                        }
                    }

                    // 4. Cộng điểm tích lũy: DiemTichLuy += TongTien / 100000
                    if (maKH.HasValue)
                    {
                        const string sqlDiem =
                            "UPDATE KhachHang SET DiemTichLuy = DiemTichLuy + @diem WHERE MaKH = @kh";
                        using (SqlCommand cmd = new SqlCommand(sqlDiem, conn, trans))
                        {
                            cmd.Parameters.Add("@diem", SqlDbType.Int).Value = (int)(tongTien / 100000m);
                            cmd.Parameters.Add("@kh", SqlDbType.Int).Value = maKH.Value;
                            cmd.ExecuteNonQuery();
                        }
                    }

                    // 5. Hoàn tất
                    trans.Commit();
                    return maHD;
                }
                catch
                {
                    try { trans.Rollback(); } catch { /* giao dịch đã bị hủy */ }
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
