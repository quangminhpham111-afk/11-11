using System;
using System.Collections.Generic;
using RetailSales.DAL;
using RetailSales.DTO;

namespace RetailSales.BUS
{
    public class ThongKeBUS
    {
        private readonly ThongKeDAL dal = new ThongKeDAL();

        public decimal DoanhThuNgay(DateTime ngay) { return dal.DoanhThuNgay(ngay); }

        public decimal DoanhThuThang(int thang, int nam)
        {
            if (thang < 1 || thang > 12) throw new BusinessException("Tháng phải từ 1 đến 12.");
            if (nam < 2000 || nam > 2100) throw new BusinessException("Năm không hợp lệ.");
            return dal.DoanhThuThang(thang, nam);
        }

        public List<TopSanPhamDTO> Top5BanChay() { return dal.TopSanPhamBanChay(5); }
    }
}
