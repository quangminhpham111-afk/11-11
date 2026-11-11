using System;
using System.Drawing;
using System.Windows.Forms;
using RetailSales.BUS;

namespace RetailSales.GUI
{
    /// <summary>Màn hình chính / tổng quan hệ thống.</summary>
    public class FrmMain : Form
    {
        private readonly ThongKeBUS thongKe = new ThongKeBUS();
        private readonly Label lblNgay = new Label { AutoSize = true, Font = new Font("Segoe UI", 12f, FontStyle.Bold), ForeColor = Color.Firebrick };
        private readonly Label lblThang = new Label { AutoSize = true, Font = new Font("Segoe UI", 12f, FontStyle.Bold), ForeColor = Color.Firebrick };

        public FrmMain()
        {
            Text = "Hệ thống Quản lý Bán hàng & Siêu thị mini";
            Size = new Size(720, 460);
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;

            var title = new Label
            {
                Text = "RETAIL SALES SYSTEM",
                Font = new Font("Segoe UI", 20f, FontStyle.Bold),
                ForeColor = Color.SeaGreen,
                AutoSize = true, Left = 20, Top = 15
            };

            var grpInfo = new GroupBox { Text = "Tổng quan doanh thu", Left = 20, Top = 70, Width = 665, Height = 90 };
            lblNgay.Location = new Point(15, 25);
            lblThang.Location = new Point(15, 55);
            grpInfo.Controls.AddRange(new Control[] { lblNgay, lblThang });

            Button B(string text, int x, int y, Action open)
            {
                var b = new Button { Text = text, Left = x, Top = y, Width = 210, Height = 70, Font = new Font("Segoe UI", 10f, FontStyle.Bold) };
                b.Click += (s, e) => { open(); LamMoi(); };
                return b;
            }

            Controls.AddRange(new Control[]
            {
                title, grpInfo,
                B("Bán hàng / Lập hóa đơn", 20, 185, () => new FrmBanHang().ShowDialog(this)),
                B("Quản lý Sản phẩm", 245, 185, () => new FrmSanPham().ShowDialog(this)),
                B("Quản lý Khách hàng", 470, 185, () => new FrmKhachHang().ShowDialog(this)),
                B("Quản lý Nhân viên", 20, 275, () => new FrmNhanVien().ShowDialog(this)),
                B("Báo cáo - Thống kê", 245, 275, () => new FrmThongKe().ShowDialog(this)),
                B("Thoát", 470, 275, Close)
            });

            Load += (s, e) => LamMoi();
        }

        private void LamMoi()
        {
            try
            {
                lblNgay.Text = "Doanh thu hôm nay:  " + UiHelper.Money(thongKe.DoanhThuNgay(DateTime.Today));
                lblThang.Text = "Doanh thu tháng " + DateTime.Today.Month + "/" + DateTime.Today.Year + ":  " +
                                UiHelper.Money(thongKe.DoanhThuThang(DateTime.Today.Month, DateTime.Today.Year));
            }
            catch (Exception ex) { UiHelper.Error(ex.Message); }
        }
    }
}
