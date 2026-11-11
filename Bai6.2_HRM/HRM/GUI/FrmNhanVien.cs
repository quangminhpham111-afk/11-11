using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using HRM.BUS;
using HRM.DTO;

namespace HRM.GUI
{
    /// <summary>Quản lý hồ sơ nhân sự: CRUD, chuyển phòng ban, tìm theo tên / CCCD / phòng ban.</summary>
    public class FrmNhanVien : Form
    {
        private readonly NhanVienBUS bus = new NhanVienBUS();
        private readonly PhongBanBUS pbBus = new PhongBanBUS();
        private readonly DataGridView dgv = UiHelper.Grid();

        private readonly TextBox txtSearch = UiHelper.Txt(150, 10, 220);
        private readonly ComboBox cboLoc = new ComboBox { Left = 470, Top = 10, Width = 200, DropDownStyle = ComboBoxStyle.DropDownList };

        private readonly TextBox txtTen = UiHelper.Txt(100, 15, 240);
        private readonly DateTimePicker dtpNgaySinh = new DateTimePicker { Left = 450, Top = 15, Width = 130, Format = DateTimePickerFormat.Short, ShowCheckBox = true };
        private readonly TextBox txtCCCD = UiHelper.Txt(690, 15, 140);
        private readonly ComboBox cboPB = new ComboBox { Left = 100, Top = 55, Width = 240, DropDownStyle = ComboBoxStyle.DropDownList };
        private readonly NumericUpDown nudLuong = UiHelper.Num(450, 55, 130, 1000000000m, 0);
        private readonly NumericUpDown nudHeSo = UiHelper.Num(690, 55, 70, 20m, 2);
        private int maNV = 0;

        public FrmNhanVien()
        {
            Text = "Quản lý Hồ sơ Nhân sự";
            Size = new Size(1000, 640);
            StartPosition = FormStartPosition.CenterScreen;
            nudHeSo.Minimum = 0.1m;
            nudHeSo.Value = 1.0m;

            var pnlTop = new Panel { Dock = DockStyle.Top, Height = 48 };
            var btnSearch = UiHelper.Btn("Tìm", 685, 6, 70);
            var btnAll = UiHelper.Btn("Tất cả", 765, 6, 80);
            pnlTop.Controls.AddRange(new Control[]
            {
                UiHelper.Lbl("Tìm tên / CCCD / PB:", 10, 10), txtSearch,
                UiHelper.Lbl("Phòng ban:", 395, 10), cboLoc, btnSearch, btnAll
            });

            var pnlEdit = new Panel { Dock = DockStyle.Bottom, Height = 160 };
            var btnAdd = UiHelper.Btn("Thêm", 100, 110, 90);
            var btnEdit = UiHelper.Btn("Sửa", 200, 110, 90);
            var btnDel = UiHelper.Btn("Xóa", 300, 110, 90);
            var btnMove = UiHelper.Btn("Chuyển phòng ban", 400, 110, 150);
            var btnClear = UiHelper.Btn("Làm mới", 560, 110, 90);
            pnlEdit.Controls.AddRange(new Control[]
            {
                UiHelper.Lbl("Họ tên:", 5, 15), txtTen,
                UiHelper.Lbl("Ngày sinh:", 370, 15), dtpNgaySinh,
                UiHelper.Lbl("CCCD:", 620, 15), txtCCCD,
                UiHelper.Lbl("Phòng ban:", 5, 55), cboPB,
                UiHelper.Lbl("Lương cơ bản:", 360, 55), nudLuong,
                UiHelper.Lbl("Hệ số:", 620, 55), nudHeSo,
                btnAdd, btnEdit, btnDel, btnMove, btnClear
            });

            Controls.Add(dgv);
            Controls.Add(pnlEdit);
            Controls.Add(pnlTop);

            btnSearch.Click += (s, e) => LoadData();
            btnAll.Click += (s, e) => { txtSearch.Clear(); cboLoc.SelectedIndex = 0; LoadData(); };
            txtSearch.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) LoadData(); };
            btnAdd.Click += BtnAdd_Click;
            btnEdit.Click += BtnEdit_Click;
            btnDel.Click += BtnDel_Click;
            btnMove.Click += BtnMove_Click;
            btnClear.Click += (s, e) => ClearInput();
            dgv.SelectionChanged += Dgv_SelectionChanged;

            NapPhongBan();
            LoadData();
        }

        private void NapPhongBan()
        {
            try
            {
                cboLoc.DisplayMember = "TenPB"; cboLoc.ValueMember = "MaPB";
                cboLoc.DataSource = pbBus.GetForFilter();

                cboPB.DisplayMember = "TenPB"; cboPB.ValueMember = "MaPB";
                cboPB.DataSource = pbBus.GetAll();
            }
            catch (Exception ex) { UiHelper.Error(ex.Message); }
        }

        private static int Ma(ComboBox cbo)
        {
            return (cbo.SelectedValue is int) ? (int)cbo.SelectedValue : 0;
        }

        private void LoadData()
        {
            try
            {
                dgv.DataSource = bus.Search(txtSearch.Text, Ma(cboLoc));
                dgv.Columns["MaNV"].HeaderText = "Mã NV";
                dgv.Columns["HoTen"].HeaderText = "Họ tên";
                dgv.Columns["NgaySinh"].HeaderText = "Ngày sinh";
                dgv.Columns["NgaySinh"].DefaultCellStyle.Format = "dd/MM/yyyy";
                dgv.Columns["CCCD"].HeaderText = "CCCD";
                dgv.Columns["MaPB"].Visible = false;
                dgv.Columns["TenPB"].HeaderText = "Phòng ban";
                dgv.Columns["LuongCoBan"].HeaderText = "Lương cơ bản";
                dgv.Columns["LuongCoBan"].DefaultCellStyle.Format = "N0";
                dgv.Columns["HeSoLuong"].HeaderText = "Hệ số";
            }
            catch (Exception ex) { UiHelper.Error(ex.Message); }
        }

        private NhanVienDTO ReadInput()
        {
            return new NhanVienDTO
            {
                MaNV = maNV,
                HoTen = txtTen.Text,
                NgaySinh = dtpNgaySinh.Checked ? (DateTime?)dtpNgaySinh.Value.Date : null,
                CCCD = txtCCCD.Text,
                MaPB = Ma(cboPB),
                LuongCoBan = nudLuong.Value,
                HeSoLuong = (double)nudHeSo.Value
            };
        }

        private void ClearInput()
        {
            maNV = 0; txtTen.Clear(); txtCCCD.Clear();
            dtpNgaySinh.Checked = false;
            nudLuong.Value = 0; nudHeSo.Value = 1.0m;
            if (cboPB.Items.Count > 0) cboPB.SelectedIndex = 0;
            dgv.ClearSelection();
            txtTen.Focus();
        }

        private void Dgv_SelectionChanged(object sender, EventArgs e)
        {
            NhanVienDTO nv = dgv.CurrentRow == null ? null : dgv.CurrentRow.DataBoundItem as NhanVienDTO;
            if (nv == null || !dgv.CurrentRow.Selected) return;
            maNV = nv.MaNV;
            txtTen.Text = nv.HoTen;
            txtCCCD.Text = nv.CCCD;
            if (nv.NgaySinh.HasValue) { dtpNgaySinh.Checked = true; dtpNgaySinh.Value = nv.NgaySinh.Value; }
            else dtpNgaySinh.Checked = false;
            cboPB.SelectedValue = nv.MaPB;
            nudLuong.Value = Math.Min(nv.LuongCoBan, nudLuong.Maximum);
            nudHeSo.Value = Math.Max(nudHeSo.Minimum, Math.Min((decimal)nv.HeSoLuong, nudHeSo.Maximum));
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            try
            {
                bus.Add(ReadInput());
                UiHelper.Info("Đã thêm nhân viên.");
                LoadData();
                ClearInput();
            }
            catch (BusinessException ex) { UiHelper.Error(ex.Message); }
        }

        private void BtnEdit_Click(object sender, EventArgs e)
        {
            if (maNV == 0) { UiHelper.Error("Hãy chọn nhân viên cần sửa."); return; }
            try
            {
                bus.Update(ReadInput());
                UiHelper.Info("Đã cập nhật hồ sơ nhân viên.");
                LoadData();
            }
            catch (BusinessException ex) { UiHelper.Error(ex.Message); }
        }

        private void BtnDel_Click(object sender, EventArgs e)
        {
            if (maNV == 0) { UiHelper.Error("Hãy chọn nhân viên cần xóa."); return; }
            if (!UiHelper.Confirm("Xóa nhân viên " + txtTen.Text + "?")) return;
            try
            {
                bus.Delete(maNV);
                LoadData();
                ClearInput();
            }
            catch (BusinessException ex) { UiHelper.Error(ex.Message); }
        }

        /// <summary>Chỉ đổi phòng ban theo mục đang chọn ở ô "Phòng ban" (không sửa các thông tin khác).</summary>
        private void BtnMove_Click(object sender, EventArgs e)
        {
            if (maNV == 0) { UiHelper.Error("Hãy chọn nhân viên cần chuyển phòng ban."); return; }
            if (!UiHelper.Confirm("Chuyển " + txtTen.Text + " sang \"" + cboPB.Text + "\"?")) return;
            try
            {
                bus.ChuyenPhongBan(maNV, Ma(cboPB));
                UiHelper.Info("Đã chuyển phòng ban.");
                LoadData();
            }
            catch (BusinessException ex) { UiHelper.Error(ex.Message); }
        }
    }
}
