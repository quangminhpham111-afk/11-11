using System.Collections.Generic;
using System.Data.SqlClient;
using RetailSales.DAL;
using RetailSales.DTO;

namespace RetailSales.BUS
{
    public class KhachHangBUS
    {
        private readonly KhachHangDAL dal = new KhachHangDAL();

        public List<KhachHangDTO> GetAll() { return dal.GetAll(); }

        public List<KhachHangDTO> Search(string keyword)
        {
            return string.IsNullOrWhiteSpace(keyword) ? dal.GetAll() : dal.Search(keyword);
        }

        /// <summary>Danh sách cho ComboBox bán hàng: phần tử đầu là "Khách lẻ" (MaKH = 0).</summary>
        public List<KhachHangDTO> GetForSale()
        {
            var list = dal.GetAll();
            list.Insert(0, new KhachHangDTO { MaKH = 0, TenKH = "— Khách lẻ —" });
            return list;
        }

        private static void Validate(KhachHangDTO kh)
        {
            if (kh == null || string.IsNullOrWhiteSpace(kh.TenKH))
                throw new BusinessException("Tên khách hàng không được để trống.");
            if (kh.DiemTichLuy < 0) throw new BusinessException("Điểm tích lũy không được âm.");
            kh.TenKH = kh.TenKH.Trim();
        }

        public void Add(KhachHangDTO kh)
        {
            Validate(kh);
            try { dal.Insert(kh); }
            catch (SqlException ex) { throw BusinessException.FromSql(ex, "Khách hàng"); }
        }

        public void Update(KhachHangDTO kh)
        {
            Validate(kh);
            try
            {
                if (dal.Update(kh) == 0) throw new BusinessException("Không tìm thấy khách hàng để sửa.");
            }
            catch (SqlException ex) { throw BusinessException.FromSql(ex, "Khách hàng"); }
        }

        public void Delete(int maKH)
        {
            try
            {
                if (dal.Delete(maKH) == 0) throw new BusinessException("Không tìm thấy khách hàng để xóa.");
            }
            catch (SqlException ex)
            {
                if (ex.Number == 547)
                    throw new BusinessException("Không thể xóa: khách hàng này đã có hóa đơn.", ex);
                throw BusinessException.FromSql(ex, "Khách hàng");
            }
        }
    }
}
