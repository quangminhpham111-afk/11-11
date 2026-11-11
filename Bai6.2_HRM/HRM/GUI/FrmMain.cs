using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using HRM.BUS;

namespace HRM.GUI
{
    /// <summary>Màn hình chính / tổng quan hệ thống nhân sự.</summary>
    public class FrmMain : Form
    {
        private readonly Label lblNV = new Label { AutoSize = true, Font = new Font("Segoe UI", 11f, FontStyle.Bold), ForeColor = Color.Firebrick };
        private readonly Label lblPB = new Label { AutoSize = true, Font = new Font("Segoe UI", 11f, FontStyle.Bold), ForeColor = Color.Firebrick };
        private readonly Label lblLuong = new Label { AutoSize = true, Font = new Font("Segoe UI", 11f, FontStyle.Bold), ForeColor = Color.Firebrick };

        public FrmMain()
        {
            Text = "Hệ thống Quản lý Nhân sự & Tính lương";
            Size = new Size(720, 470);
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;

            var title = new Label
            {
                Text = "HRM & PAYROLL SYSTEM",
                Font = new Font("Segoe UI", 20f, FontStyle.Bold),
                ForeColor = Color.SteelBlue,
                AutoSize = true, Left = 20, Top = 15
            };

            var grp = new GroupBox { Text = "Tổng quan", Left = 20, Top = 70, Width = 665, Height = 115 };
            lblNV.Location = new Point(15, 25);
            lblPB.Location = new Point(15, 55);
            lblLuong.Location = new Point(15, 85);
            grp.Controls.AddRange(new Control[] { lblNV, lblPB, lblLuong });

            Button B(string text, int x, int y, Action open)
            {
                var b = new Button { Text = text, Left = x, Top = y, Width = 210, Height = 70, Font = new Font("Segoe UI", 10f, FontStyle.Bold) };
                b.Click += (s, e) => { open(); LamMoi(); };
                return b;
            }

            Controls.AddRange(new Control[]
            {
                title, grp,
                B("Hồ sơ Nhân sự", 20, 205, () => new FrmNhanVien().ShowDialog(this)),
                B("Phòng ban", 245, 205, () => new FrmPhongBan().ShowDialog(this)),
                B("Chấm công", 470, 205, () => new FrmChamCong().ShowDialog(this)),
                B("Tính lương & Chốt lương", 20, 295, () => new FrmLuong().ShowDialog(this)),
                B("Thoát", 470, 295, Close)
            });

            Load += (s, e) => LamMoi();
        }

        private void LamMoi()
        {
            try
            {
                lblNV.Text = "Tổng số nhân viên:  " + new NhanVienBUS().GetAll().Count;
                lblPB.Text = "Tổng số phòng ban:  " + new PhongBanBUS().GetAll().Count;

                string tn = ChamCongBUS.ThangNamKey(DateTime.Today.Month, DateTime.Today.Year);
                var luong = new LuongBUS();
                lblLuong.Text = luong.DaChot(tn)
                    ? "Lương tháng " + tn + " (đã chốt):  " + UiHelper.Money(luong.GetBangLuong(tn).Sum(x => x.ThucLinh))
                    : "Lương tháng " + tn + ":  chưa chốt";
            }
            catch (Exception ex) { UiHelper.Error(ex.Message); }
        }
    }
}
