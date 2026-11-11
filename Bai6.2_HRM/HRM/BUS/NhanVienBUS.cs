using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text.RegularExpressions;
using HRM.DAL;
using HRM.DTO;

namespace HRM.BUS
{
    public class NhanVienBUS
    {
        private readonly NhanVienDAL dal = new NhanVienDAL();

        public List<NhanVienDTO> GetAll() { return dal.GetAll(); }

        public List<NhanVienDTO> Search(string keyword, int maPB)
        {
            return dal.Search(keyword ?? string.Empty, maPB);
        }

        private static void Validate(NhanVienDTO nv)
        {
            if (nv == null || string.IsNullOrWhiteSpace(nv.HoTen))
                throw new BusinessException("Họ tên không được để trống.");
            if (nv.MaPB <= 0) throw new BusinessException("Vui lòng chọn phòng ban.");
            if (nv.LuongCoBan <= 0) throw new BusinessException("Lương cơ bản phải lớn hơn 0.");
            if (nv.HeSoLuong <= 0) throw new BusinessException("Hệ số lương phải lớn hơn 0.");
            if (nv.NgaySinh.HasValue && nv.NgaySinh.Value.Date >= DateTime.Today)
                throw new BusinessException("Ngày sinh phải nhỏ hơn ngày hiện tại.");
            if (!string.IsNullOrWhiteSpace(nv.CCCD) && !Regex.IsMatch(nv.CCCD.Trim(), @"^(\d{9}|\d{12})$"))
                throw new BusinessException("CCCD phải gồm 12 chữ số (hoặc CMND 9 chữ số).");
            nv.HoTen = nv.HoTen.Trim();
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

        public void ChuyenPhongBan(int maNV, int maPBMoi)
        {
            if (maNV <= 0) throw new BusinessException("Hãy chọn nhân viên cần chuyển phòng ban.");
            if (maPBMoi <= 0) throw new BusinessException("Hãy chọn phòng ban mới.");
            try
            {
                if (dal.ChuyenPhongBan(maNV, maPBMoi) == 0) throw new BusinessException("Không tìm thấy nhân viên.");
            }
            catch (SqlException ex) { throw BusinessException.FromSql(ex, "Phòng ban"); }
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
                    throw new BusinessException("Không thể xóa: nhân viên này đã có dữ liệu chấm công / bảng lương.", ex);
                throw BusinessException.FromSql(ex, "Nhân viên");
            }
        }
    }
}
