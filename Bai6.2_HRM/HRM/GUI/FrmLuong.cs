using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using HRM.BUS;
using HRM.DTO;

namespace HRM.GUI
{
    /// <summary>Tính lương (xem trước) và Chốt lương tháng (SqlTransaction trong LuongDAL).</summary>
    public class FrmLuong : Form
    {
        private readonly LuongBUS bus = new LuongBUS();
        private readonly DataGridView dgv = UiHelper.Grid();
        private readonly NumericUpDown nudThang = new NumericUpDown { Left = 90, Top = 14, Width = 50, Minimum = 1, Maximum = 12, Value = DateTime.Today.Month };
        private readonly NumericUpDown nudNam = new NumericUpDown { Left = 190, Top = 14, Width = 70, Minimum = 2000, Maximum = 2100, Value = DateTime.Today.Year };
        private readonly Label lblTrangThai = new Label { Left = 15, Top = 16, AutoSize = true, Font = new Font("Segoe UI", 10f, FontStyle.Bold) };
        private readonly Label lblTong = new Label { Left = 15, Top = 16, AutoSize = true, Font = new Font("Segoe UI", 11f, FontStyle.Bold), ForeColor = Color.Firebrick };
        private readonly Button btnChot = UiHelper.Btn("CHỐT LƯƠNG THÁNG", 700, 8, 190);

        public FrmLuong()
        {
            Text = "Tính lương & Chốt lương";
            Size = new Size(960, 600);
            StartPosition = FormStartPosition.CenterScreen;

            var pnlTop = new Panel { Dock = DockStyle.Top, Height = 92 };
            var btnTinh = UiHelper.Btn("Tính lương (xem trước)", 290, 10, 180);
            var btnXem = UiHelper.Btn("Xem bảng lương đã chốt", 480, 10, 190);
            lblTrangThai.Top = 56;
            pnlTop.Controls.AddRange(new Control[]
            {
                UiHelper.Lbl("Tháng:", 10, 14), nudThang,
                UiHelper.Lbl("Năm:", 150, 14), nudNam, btnTinh, btnXem, lblTrangThai
            });

            var pnlBottom = new Panel { Dock = DockStyle.Bottom, Height = 52 };
            btnChot.BackColor = Color.SeaGreen;
            btnChot.ForeColor = Color.White;
            btnChot.Font = new Font("Segoe UI", 9.5f, FontStyle.Bold);
            btnChot.Height = 36;
            pnlBottom.Controls.AddRange(new Control[] { lblTong, btnChot });

            Controls.Add(dgv);
            Controls.Add(pnlBottom);
            Controls.Add(pnlTop);

            btnTinh.Click += (s, e) => XemTruoc();
            btnXem.Click += (s, e) => XemDaChot();
            btnChot.Click += BtnChot_Click;
            nudThang.ValueChanged += (s, e) => CapNhatTrangThai();
            nudNam.ValueChanged += (s, e) => CapNhatTrangThai();

            CapNhatTrangThai();
        }

        private string Key()
        {
            return ChamCongBUS.ThangNamKey((int)nudThang.Value, (int)nudNam.Value);
        }

        private void CapNhatTrangThai()
        {
            try
            {
                string tn = Key();
                bool daChot = bus.DaChot(tn);
                lblTrangThai.Text = daChot ? "Tháng " + tn + ": ĐÃ CHỐT LƯƠNG" : "Tháng " + tn + ": chưa chốt lương";
                lblTrangThai.ForeColor = daChot ? Color.Firebrick : Color.SeaGreen;
                btnChot.Enabled = !daChot;
            }
            catch (Exception ex) { UiHelper.Error(ex.Message); }
        }

        private void HienThi(List<BangLuongDTO> ds)
        {
            dgv.DataSource = ds;
            dgv.Columns["MaNV"].HeaderText = "Mã NV";
            dgv.Columns["HoTen"].HeaderText = "Họ tên";
            dgv.Columns["ThangNam"].HeaderText = "Tháng/Năm";
            dgv.Columns["SoNgayCong"].HeaderText = "Ngày công";
            dgv.Columns["SoGioOT"].HeaderText = "Giờ OT";
            dgv.Columns["ThucLinh"].HeaderText = "Thực lĩnh";
            dgv.Columns["ThucLinh"].DefaultCellStyle.Format = "N0";
            lblTong.Text = "Tổng thực lĩnh: " + UiHelper.Money(ds.Sum(x => x.ThucLinh));
        }

        private void XemTruoc()
        {
            try { HienThi(bus.XemTruoc(Key())); }
            catch (BusinessException ex) { UiHelper.Error(ex.Message); }
        }

        private void XemDaChot()
        {
            try
            {
                List<BangLuongDTO> ds = bus.GetBangLuong(Key());
                if (ds.Count == 0) { UiHelper.Info("Tháng này chưa có bảng lương đã chốt."); return; }
                HienThi(ds);
            }
            catch (BusinessException ex) { UiHelper.Error(ex.Message); }
        }

        private void BtnChot_Click(object sender, EventArgs e)
        {
            string tn;
            try { tn = Key(); }
            catch (BusinessException ex) { UiHelper.Error(ex.Message); return; }

            if (!UiHelper.Confirm("Chốt lương tháng " + tn + "?\nSau khi chốt, dữ liệu chấm công của tháng sẽ bị khóa.")) return;
            try
            {
                List<BangLuongDTO> ds = bus.ChotLuong(tn);       // SqlTransaction trong DAL
                HienThi(ds);
                UiHelper.Info("Đã chốt lương tháng " + tn + " cho " + ds.Count + " nhân viên.");
                CapNhatTrangThai();
            }
            catch (BusinessException ex)
            {
                MessageBox.Show(ex.Message, "Chốt lương thất bại", MessageBoxButtons.OK, MessageBoxIcon.Error);
                CapNhatTrangThai();
            }
        }
    }
}
