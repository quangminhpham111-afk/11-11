using System.Collections.Generic;
using System.Data.SqlClient;
using HRM.DAL;
using HRM.DTO;

namespace HRM.BUS
{
    public class PhongBanBUS
    {
        private readonly PhongBanDAL dal = new PhongBanDAL();

        public List<PhongBanDTO> GetAll() { return dal.GetAll(); }

        /// <summary>Danh sách cho ComboBox lọc: thêm mục "Tất cả" (MaPB = 0) ở đầu.</summary>
        public List<PhongBanDTO> GetForFilter()
        {
            var list = dal.GetAll();
            list.Insert(0, new PhongBanDTO { MaPB = 0, TenPB = "— Tất cả —" });
            return list;
        }

        private static void Validate(PhongBanDTO pb)
        {
            if (pb == null || string.IsNullOrWhiteSpace(pb.TenPB))
                throw new BusinessException("Tên phòng ban không được để trống.");
            pb.TenPB = pb.TenPB.Trim();
        }

        public void Add(PhongBanDTO pb)
        {
            Validate(pb);
            try { dal.Insert(pb); }
            catch (SqlException ex) { throw BusinessException.FromSql(ex, "Phòng ban"); }
        }

        public void Update(PhongBanDTO pb)
        {
            Validate(pb);
            try
            {
                if (dal.Update(pb) == 0) throw new BusinessException("Không tìm thấy phòng ban để sửa.");
            }
            catch (SqlException ex) { throw BusinessException.FromSql(ex, "Phòng ban"); }
        }

        public void Delete(int maPB)
        {
            try
            {
                if (dal.Delete(maPB) == 0) throw new BusinessException("Không tìm thấy phòng ban để xóa.");
            }
            catch (SqlException ex)
            {
                if (ex.Number == 547)
                    throw new BusinessException("Không thể xóa: phòng ban này vẫn còn nhân viên.", ex);
                throw BusinessException.FromSql(ex, "Phòng ban");
            }
        }
    }
}
