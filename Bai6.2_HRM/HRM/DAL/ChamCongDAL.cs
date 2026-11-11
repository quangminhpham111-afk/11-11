using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace HRM.DAL
{
    /// <summary>
    /// Chấm công theo MÔ HÌNH NGẮT KẾT NỐI (Disconnected Model):
    ///   - SqlDataAdapter.Fill  : nạp dữ liệu vào DataTable
    ///   - Người dùng sửa trực tiếp trên DataGridView (chỉ thay đổi trong bộ nhớ)
    ///   - SqlCommandBuilder + adapter.Update : đồng bộ toàn bộ thay đổi về SQL Server
    /// Một đối tượng ChamCongDAL tương ứng với một lần nạp (một tháng).
    /// </summary>
    public class ChamCongDAL : IDisposable
    {
        private readonly SqlConnection conn = DbHelper.CreateConnection();
        private SqlDataAdapter adapter;
        private SqlCommandBuilder builder;

        /// <summary>
        /// Nạp chấm công của tháng. Nhân viên chưa có dòng chấm công được thêm vào DataTable
        /// (RowState = Added, chưa ghi DB) để người dùng nhập luôn trên lưới.
        /// </summary>
        public DataTable LoadChamCong(string thangNam)
        {
            adapter = new SqlDataAdapter(
                "SELECT MaCong, MaNV, ThangNam, SoNgayCong, SoGioOT, DaChot " +
                "FROM BangChamCong WHERE ThangNam = @tn ORDER BY MaNV", conn);
            adapter.SelectCommand.Parameters.Add("@tn", SqlDbType.VarChar, 7).Value = thangNam;
            adapter.MissingSchemaAction = MissingSchemaAction.AddWithKey;   // lấy khóa chính + identity

            builder = new SqlCommandBuilder(adapter);                        // tự sinh INSERT/UPDATE/DELETE
            builder.ConflictOption = ConflictOption.OverwriteChanges;        // WHERE chỉ theo khóa chính

            var dt = new DataTable("ChamCong");
            adapter.Fill(dt);

            // Cột MaCong là identity: dùng giá trị tạm âm để không trùng khóa với dòng thật
            DataColumn colKey = dt.Columns["MaCong"];
            colKey.AutoIncrement = true;
            colKey.AutoIncrementSeed = -1;
            colKey.AutoIncrementStep = -1;

            // Thông tin hiển thị (không thuộc bảng BangChamCong nên adapter bỏ qua khi Update)
            var nhanVien = new DataTable();
            using (var ad2 = new SqlDataAdapter(
                "SELECT nv.MaNV, nv.HoTen, pb.TenPB FROM NhanVien nv JOIN PhongBan pb ON pb.MaPB = nv.MaPB ORDER BY nv.MaNV",
                conn))
            {
                ad2.Fill(nhanVien);
            }

            dt.Columns.Add("HoTen", typeof(string));
            dt.Columns.Add("TenPB", typeof(string));

            var thongTin = new Dictionary<int, DataRow>();
            foreach (DataRow nv in nhanVien.Rows) thongTin[(int)nv["MaNV"]] = nv;

            foreach (DataRow r in dt.Rows)
            {
                DataRow nv;
                if (thongTin.TryGetValue((int)r["MaNV"], out nv))
                {
                    r["HoTen"] = nv["HoTen"];
                    r["TenPB"] = nv["TenPB"];
                }
            }
            dt.AcceptChanges();     // việc gán HoTen/TenPB không được tính là thay đổi dữ liệu

            // Thêm dòng (Added) cho nhân viên chưa có chấm công tháng này
            var daCo = new HashSet<int>();
            foreach (DataRow r in dt.Rows) daCo.Add((int)r["MaNV"]);

            foreach (DataRow nv in nhanVien.Rows)
            {
                int maNV = (int)nv["MaNV"];
                if (daCo.Contains(maNV)) continue;

                DataRow row = dt.NewRow();
                row["MaNV"] = maNV;
                row["ThangNam"] = thangNam;
                row["SoNgayCong"] = 0.0;
                row["SoGioOT"] = 0.0;
                row["DaChot"] = false;
                row["HoTen"] = nv["HoTen"];
                row["TenPB"] = nv["TenPB"];
                dt.Rows.Add(row);
            }
            return dt;
        }

        /// <summary>
        /// Đồng bộ mọi thay đổi (thêm/sửa/xóa) về SQL Server bằng MỘT lần adapter.Update,
        /// bọc trong transaction để hoặc lưu hết, hoặc không lưu gì. Trả về số dòng đã ghi.
        /// </summary>
        public int SaveChamCong(DataTable dt)
        {
            if (adapter == null) throw new InvalidOperationException("Chưa nạp dữ liệu chấm công.");

            builder.RefreshSchema();    // xóa lệnh đã sinh trước đó để gắn transaction mới

            conn.Open();
            SqlTransaction trans = conn.BeginTransaction();
            try
            {
                adapter.InsertCommand = builder.GetInsertCommand();
                adapter.UpdateCommand = builder.GetUpdateCommand();
                adapter.DeleteCommand = builder.GetDeleteCommand();
                adapter.InsertCommand.Transaction = trans;
                adapter.UpdateCommand.Transaction = trans;
                adapter.DeleteCommand.Transaction = trans;

                int n = adapter.Update(dt);
                trans.Commit();
                return n;
            }
            catch
            {
                try { trans.Rollback(); } catch { }
                throw;
            }
            finally
            {
                trans.Dispose();
                conn.Close();
            }
        }

        public void Dispose()
        {
            if (builder != null) builder.Dispose();
            if (adapter != null) adapter.Dispose();
            conn.Dispose();
        }
    }
}
