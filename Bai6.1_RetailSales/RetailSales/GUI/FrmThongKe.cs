using System;
using System.Drawing;
using System.Windows.Forms;
using RetailSales.BUS;

namespace RetailSales.GUI
{
    /// <summary>Báo cáo: doanh thu ngày/tháng (ExecuteScalar) và Top 5 bán chạy (SqlDataReader).</summary>
    public class FrmThongKe : Form
    {
        private readonly ThongKeBUS bus = new ThongKeBUS();
        private readonly DateTimePicker dtpNgay = new DateTimePicker { Left = 130, Top = 15, Width = 130, Format = DateTimePickerFormat.Short };
        private readonly NumericUpDown nudThang = new NumericUpDown { Left = 130, Top = 55, Width = 50, Minimum = 1, Maximum = 12, Value = DateTime.Today.Month };
        private readonly NumericUpDown nudNam = new NumericUpDown { Left = 230, Top = 55, Width = 70, Minimum = 2000, Maximum = 2100, Value = DateTime.Today.Year };
        private readonly Label lblNgay = new Label { Left = 420, Top = 17, AutoSize = true, Font = new Font("Segoe UI", 11f, FontStyle.Bold), ForeColor = Color.Firebrick };
        private readonly Label lblThang = new Label { Left = 420, Top = 57, AutoSize = true, Font = new Font("Segoe UI", 11f, FontStyle.Bold), ForeColor = Color.Firebrick };
        private readonly DataGridView dgvTop = UiHelper.Grid();

        public FrmThongKe()
        {
            Text = "Báo cáo - Thống kê";
            Size = new Size(760, 520);
            StartPosition = FormStartPosition.CenterScreen;

            var pnlTop = new Panel { Dock = DockStyle.Top, Height = 100 };
            var btnNgay = UiHelper.Btn("Doanh thu ngày", 270, 12, 130);
            var btnThang = UiHelper.Btn("Doanh thu tháng", 310, 52, 100);
            btnThang.Width = 105;
            pnlTop.Controls.AddRange(new Control[]
            {
                UiHelper.Lbl("Chọn ngày:", 10, 15), dtpNgay, btnNgay, lblNgay,
                UiHelper.Lbl("Tháng / Năm:", 10, 55), nudThang, nudNam, btnThang, lblThang
            });
            btnThang.Left = 310;

            var grp = new GroupBox { Text = "Top 5 sản phẩm bán chạy nhất", Dock = DockStyle.Fill };
            grp.Controls.Add(dgvTop);

            Controls.Add(grp);
            Controls.Add(pnlTop);

            btnNgay.Click += (s, e) => TinhNgay();
            btnThang.Click += (s, e) => TinhThang();

            TinhNgay();
            TinhThang();
            LoadTop5();
        }

        private void TinhNgay()
        {
            try { lblNgay.Text = UiHelper.Money(bus.DoanhThuNgay(dtpNgay.Value)); }
            catch (Exception ex) { UiHelper.Error(ex.Message); }
        }

        private void TinhThang()
        {
            try { lblThang.Text = UiHelper.Money(bus.DoanhThuThang((int)nudThang.Value, (int)nudNam.Value)); }
            catch (Exception ex) { UiHelper.Error(ex.Message); }
        }

        private void LoadTop5()
        {
            try
            {
                dgvTop.DataSource = bus.Top5BanChay();
                dgvTop.Columns["MaSP"].HeaderText = "Mã SP";
                dgvTop.Columns["TenSP"].HeaderText = "Tên sản phẩm";
                dgvTop.Columns["TongSoLuong"].HeaderText = "Số lượng đã bán";
                dgvTop.Columns["DoanhThu"].HeaderText = "Doanh thu";
                dgvTop.Columns["DoanhThu"].DefaultCellStyle.Format = "N0";
            }
            catch (Exception ex) { UiHelper.Error(ex.Message); }
        }
    }
}
