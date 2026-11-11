using System.Collections.Generic;
using System.Data.SqlClient;
using RetailSales.DAL;
using RetailSales.DTO;

namespace RetailSales.BUS
{
    public class SanPhamBUS
    {
        private readonly SanPhamDAL dal = new SanPhamDAL();

        public List<SanPhamDTO> GetAll() { return dal.GetAll(); }

        public List<SanPhamDTO> Search(string keyword)
        {
            return string.IsNullOrWhiteSpace(keyword) ? dal.GetAll() : dal.Search(keyword);
        }

        public SanPhamDTO GetById(string maSP)
        {
            return string.IsNullOrWhiteSpace(maSP) ? null : dal.GetById(maSP.Trim());
        }

        private static void Validate(SanPhamDTO sp)
        {
            if (sp == null) throw new BusinessException("Dữ liệu sản phẩm không hợp lệ.");
            if (string.IsNullOrWhiteSpace(sp.MaSP)) throw new BusinessException("Mã sản phẩm không được để trống.");
            if (sp.MaSP.Trim().Length > 20) throw new BusinessException("Mã sản phẩm tối đa 20 ký tự.");
            if (string.IsNullOrWhiteSpace(sp.TenSP)) throw new BusinessException("Tên sản phẩm không được để trống.");
            if (sp.GiaBan <= 0) throw new BusinessException("Giá bán phải lớn hơn 0.");
            if (sp.SoLuongTon < 0) throw new BusinessException("Số lượng tồn không được âm.");
            sp.MaSP = sp.MaSP.Trim();
            sp.TenSP = sp.TenSP.Trim();
        }

        public void Add(SanPhamDTO sp)
        {
            Validate(sp);
            try { dal.Insert(sp); }
            catch (SqlException ex) { throw BusinessException.FromSql(ex, "Sản phẩm"); }
        }

        public void Update(SanPhamDTO sp)
        {
            Validate(sp);
            try
            {
                if (dal.Update(sp) == 0) throw new BusinessException("Không tìm thấy sản phẩm để sửa.");
            }
            catch (SqlException ex) { throw BusinessException.FromSql(ex, "Sản phẩm"); }
        }

        public void Delete(string maSP)
        {
            try
            {
                if (dal.Delete(maSP) == 0) throw new BusinessException("Không tìm thấy sản phẩm để xóa.");
            }
            catch (SqlException ex)
            {
                if (ex.Number == 547)
                    throw new BusinessException("Không thể xóa: sản phẩm này đã phát sinh trong hóa đơn.", ex);
                throw BusinessException.FromSql(ex, "Sản phẩm");
            }
        }
    }
}
