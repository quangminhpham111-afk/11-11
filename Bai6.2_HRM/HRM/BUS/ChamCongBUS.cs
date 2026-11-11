using System;
using System.Data;
using System.Data.SqlClient;
using HRM.DAL;

namespace HRM.BUS
{
    /// <summary>Nghiệp vụ chấm công (mô hình ngắt kết nối). Một đối tượng dùng cho một form.</summary>
    public class ChamCongBUS : IDisposable
    {
        private readonly ChamCongDAL dal = new ChamCongDAL();
        private readonly LuongDAL luongDal = new LuongDAL();

        /// <summary>Khóa tháng dạng "MM/yyyy", ví dụ "11/2026".</summary>
        public static string ThangNamKey(int thang, int nam)
        {
            if (thang < 1 || thang > 12) throw new BusinessException("Tháng phải từ 1 đến 12.");
            if (nam < 2000 || nam > 2100) throw new BusinessException("Năm không hợp lệ.");
            return thang.ToString("00") + "/" + nam;
        }

        public bool DaChotLuong(string thangNam) { return luongDal.DaChotLuong(thangNam); }

        public DataTable Load(string thangNam)
        {
            try { return dal.LoadChamCong(thangNam); }
            catch (SqlException ex) { throw BusinessException.FromSql(ex, "Chấm công"); }
        }

        /// <summary>Kiểm tra dữ liệu trên lưới rồi đồng bộ về SQL Server bằng adapter.Update().</summary>
        public int Save(DataTable dt, string thangNam)
        {
            if (luongDal.DaChotLuong(thangNam))
                throw new BusinessException("Tháng " + thangNam + " đã chốt lương, dữ liệu chấm công đã bị khóa.");

            foreach (DataRow r in dt.Rows)
            {
                if (r.RowState == DataRowState.Deleted || r.RowState == DataRowState.Unchanged) continue;

                string ten = Convert.ToString(r["HoTen"]);
                double ngay = r["SoNgayCong"] == DBNull.Value ? 0 : Convert.ToDouble(r["SoNgayCong"]);
                double ot = r["SoGioOT"] == DBNull.Value ? 0 : Convert.ToDouble(r["SoGioOT"]);
                if (ngay < 0 || ngay > 31)
                    throw new BusinessException("Số ngày công của \"" + ten + "\" phải từ 0 đến 31.");
                if (ot < 0)
                    throw new BusinessException("Số giờ OT của \"" + ten + "\" không được âm.");
            }

            try { return dal.SaveChamCong(dt); }
            catch (SqlException ex) { throw BusinessException.FromSql(ex, "Chấm công"); }
            catch (DBConcurrencyException ex)
            {
                throw new BusinessException("Dữ liệu đã bị thay đổi bởi người khác. Hãy tải lại tháng này.", ex);
            }
        }

        public void Dispose() { dal.Dispose(); }
    }
}
