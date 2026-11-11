using System.Collections.Generic;
using System.Data.SqlClient;
using RetailSales.DAL;
using RetailSales.DTO;

namespace RetailSales.BUS
{
    public class NhanVienBUS
    {
        private readonly NhanVienDAL dal = new NhanVienDAL();

        public List<NhanVienDTO> GetAll() { return dal.GetAll(); }

        public List<NhanVienDTO> Search(string keyword)
        {
            return string.IsNullOrWhiteSpace(keyword) ? dal.GetAll() : dal.Search(keyword);
        }

        private static void Validate(NhanVienDTO nv)
        {
            if (nv == null || string.IsNullOrWhiteSpace(nv.TenNV))
                throw new BusinessException("Tên nhân viên không được để trống.");
            nv.TenNV = nv.TenNV.Trim();
        }

        public void Add(NhanVienDTO nv)
        {
            Validate(nv);
            try { dal.Insert(nv); }
            catch (SqlException ex) { throw BusinessException.FromSql(ex, "Nhân viên"); }
        }

        public void Update(NhanVienDTO nv)
        {
            Validate(nv);
            try
            {
                if (dal.Update(nv) == 0) throw new BusinessException("Không tìm thấy nhân viên để sửa.");
            }
            catch (SqlException ex) { throw BusinessException.FromSql(ex, "Nhân viên"); }
        }

        public void Delete(int maNV)
        {
            try
            {
                if (dal.Delete(maNV) == 0) throw new BusinessException("Không tìm thấy nhân viên để xóa.");
            }
            catch (SqlException ex)
            {
                if (ex.Number == 547)
                    throw new BusinessException("Không thể xóa: nhân viên này đã lập hóa đơn.", ex);
                throw BusinessException.FromSql(ex, "Nhân viên");
            }
        }
    }
}
