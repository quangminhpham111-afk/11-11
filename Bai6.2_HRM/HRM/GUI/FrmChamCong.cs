using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using HRM.BUS;

namespace HRM.GUI
{
    /// <summary>
    /// Chấm công theo mô hình ngắt kết nối: nạp vào DataTable -> sửa trên lưới -> "Lưu chấm công"
    /// gọi SqlCommandBuilder + adapter.Update() trong tầng DAL.
    /// </summary>
    public class FrmChamCong : Form
    {
        private readonly ChamCongBUS bus = new ChamCongBUS();
        private readonly DataGridView dgv = new DataGridView
        {
            Dock = DockStyle.Fill,
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            RowHeadersVisible = false,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            BackgroundColor = Color.White
        };

        private readonly NumericUpDown nudThang = new NumericUpDown { Left = 90, Top = 14, Width = 50, Minimum = 1, Maximum = 12, Value = DateTime.Today.Month };
        private readonly NumericUpDown nudNam = new NumericUpDown { Left = 190, Top = 14, Width = 70, Minimum = 2000, Maximum = 2100, Value = DateTime.Today.Year };
        private readonly Label lblTrangThai = new Label { Left = 560, Top = 18, AutoSize = true, Font = new Font("Segoe UI", 10f, FontStyle.Bold) };
        private readonly Button btnLuu = UiHelper.Btn("Lưu chấm công", 15, 10, 140);

        private DataTable dt;
        private string thangNam;

        public FrmChamCong()
        {
            Text = "Chấm công (Disconnected Model)";
            Size = new Size(900, 600);
            StartPosition = FormStartPosition.CenterScreen;

            var pnlTop = new Panel { Dock = DockStyle.Top, Height = 52 };
            var btnTai = UiHelper.Btn("Tải chấm công", 290, 10, 130);
            pnlTop.Controls.AddRange(new Control[]
            {
                UiHelper.Lbl("Tháng:", 10, 14), nudThang,
                UiHelper.Lbl("Năm:", 150, 14), nudNam, btnTai,
                UiHelper.Lbl("", 0, 0), lblTrangThai
            });

            var pnlBottom = new Panel { Dock = DockStyle.Bottom, Height = 52 };
            pnlBottom.Controls.Add(btnLuu);

            Controls.Add(dgv);
            Controls.Add(pnlBottom);
            Controls.Add(pnlTop);

            btnTai.Click += (s, e) => Tai();
            btnLuu.Click += BtnLuu_Click;
            dgv.DataError += (s, e) =>
            {
                UiHelper.Error("Giá trị nhập không hợp lệ (hãy nhập số, ví dụ 22 hoặc 21.5).");
                e.ThrowException = false;
            };
            FormClosed += (s, e) => bus.Dispose();
            FormClosing += FrmChamCong_FormClosing;

            Tai();
        }

        private void FrmChamCong_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (CoThayDoiChuaLuu() && !bus.DaChotLuong(thangNam))
            {
                e.Cancel = !UiHelper.Confirm("Còn thay đổi chưa lưu. Đóng form và bỏ các thay đổi?");
            }
        }

        /// <summary>Dòng sửa, hoặc dòng thêm mới đã được nhập số liệu (khác 0) thì coi là chưa lưu.</summary>
        private bool CoThayDoiChuaLuu()
        {
            if (dt == null) return false;
            foreach (DataRow r in dt.Rows)
            {
                if (r.RowState == DataRowState.Modified) return true;
                if (r.RowState == DataRowState.Added &&
                    (Convert.ToDouble(r["SoNgayCong"]) != 0 || Convert.ToDouble(r["SoGioOT"]) != 0))
                    return true;
            }
            return false;
        }

        private void Tai()
        {
            try
            {
                thangNam = ChamCongBUS.ThangNamKey((int)nudThang.Value, (int)nudNam.Value);
                dt = bus.Load(thangNam);               // SqlDataAdapter.Fill -> DataTable
                dgv.DataSource = dt;

                dgv.Columns["MaCong"].Visible = false;
                dgv.Columns["ThangNam"].Visible = false;
                dgv.Columns["DaChot"].Visible = false;
                dgv.Columns["MaNV"].HeaderText = "Mã NV";
                dgv.Columns["HoTen"].HeaderText = "Họ tên";
                dgv.Columns["TenPB"].HeaderText = "Phòng ban";
                dgv.Columns["SoNgayCong"].HeaderText = "Số ngày công";
                dgv.Columns["SoGioOT"].HeaderText = "Số giờ OT";

                // Chỉ cho sửa 2 cột chấm công; hiển thị cột nhập liệu trước
                dgv.Columns["HoTen"].DisplayIndex = 1;
                dgv.Columns["TenPB"].DisplayIndex = 2;
                dgv.Columns["SoNgayCong"].DisplayIndex = 3;
                dgv.Columns["SoGioOT"].DisplayIndex = 4;
                foreach (DataGridViewColumn c in dgv.Columns) c.ReadOnly = true;
                dgv.Columns["SoNgayCong"].ReadOnly = false;
                dgv.Columns["SoGioOT"].ReadOnly = false;
                dgv.Columns["SoNgayCong"].DefaultCellStyle.BackColor = Color.LightYellow;
                dgv.Columns["SoGioOT"].DefaultCellStyle.BackColor = Color.LightYellow;

                bool khoa = bus.DaChotLuong(thangNam);
                dgv.ReadOnly = khoa;
                btnLuu.Enabled = !khoa;
                lblTrangThai.Text = khoa
                    ? "Tháng " + thangNam + ": ĐÃ CHỐT LƯƠNG - dữ liệu bị khóa"
                    : "Tháng " + thangNam + ": đang mở, có thể chỉnh sửa";
                lblTrangThai.ForeColor = khoa ? Color.Firebrick : Color.SeaGreen;
            }
            catch (BusinessException ex) { UiHelper.Error(ex.Message); }
        }

        private void BtnLuu_Click(object sender, EventArgs e)
        {
            if (dt == null) return;
            dgv.EndEdit();
            BindingContext[dt].EndCurrentEdit();    // chốt ô đang nhập dở vào DataTable
            try
            {
                int n = bus.Save(dt, thangNam);     // SqlCommandBuilder + adapter.Update()
                UiHelper.Info("Đã lưu chấm công tháng " + thangNam + " (" + n + " dòng được đồng bộ).");
                Tai();                              // nạp lại để lấy khóa MaCong mới sinh
            }
            catch (BusinessException ex) { UiHelper.Error(ex.Message); }
        }
    }
}
