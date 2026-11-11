using System;
using System.Drawing;
using System.Windows.Forms;
using RetailSales.BUS;
using RetailSales.DTO;

namespace RetailSales.GUI
{
    public class FrmNhanVien : Form
    {
        private readonly NhanVienBUS bus = new NhanVienBUS();
        private readonly DataGridView dgv = UiHelper.Grid();
        private readonly TextBox txtSearch = UiHelper.Txt(80, 10, 260);
        private readonly TextBox txtTen = UiHelper.Txt(100, 15, 250);
        private readonly TextBox txtSDT = UiHelper.Txt(460, 15, 150);
        private readonly ComboBox cboChucVu = new ComboBox { Left = 100, Top = 55, Width = 160 };
        private int maNV = 0;

        public FrmNhanVien()
        {
            Text = "Quản lý Nhân viên";
            Size = new Size(860, 560);
            StartPosition = FormStartPosition.CenterScreen;

            cboChucVu.Items.AddRange(new object[] { "Thu ngân", "Quản lý", "Thủ kho", "Bảo vệ" });

            var pnlTop = new Panel { Dock = DockStyle.Top, Height = 48 };
            var btnSearch = UiHelper.Btn("Tìm", 350, 6, 70);
            var btnAll = UiHelper.Btn("Tất cả", 430, 6, 80);
            pnlTop.Controls.AddRange(new Control[] { UiHelper.Lbl("Tìm kiếm:", 10, 10), txtSearch, btnSearch, btnAll });

            var pnlEdit = new Panel { Dock = DockStyle.Bottom, Height = 150 };
            var btnAdd = UiHelper.Btn("Thêm", 100, 100, 90);
            var btnEdit = UiHelper.Btn("Sửa", 200, 100, 90);
            var btnDel = UiHelper.Btn("Xóa", 300, 100, 90);
            var btnClear = UiHelper.Btn("Làm mới", 400, 100, 90);
            pnlEdit.Controls.AddRange(new Control[]
            {
                UiHelper.Lbl("Tên NV:", 5, 15), txtTen,
                UiHelper.Lbl("Điện thoại:", 370, 15), txtSDT,
                UiHelper.Lbl("Chức vụ:", 5, 55), cboChucVu,
                btnAdd, btnEdit, btnDel, btnClear
            });

            Controls.Add(dgv);
            Controls.Add(pnlEdit);
            Controls.Add(pnlTop);

            btnSearch.Click += (s, e) => LoadData(txtSearch.Text);
            btnAll.Click += (s, e) => { txtSearch.Clear(); LoadData(null); };
            txtSearch.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) LoadData(txtSearch.Text); };
            btnAdd.Click += BtnAdd_Click;
            btnEdit.Click += BtnEdit_Click;
            btnDel.Click += BtnDel_Click;
            btnClear.Click += (s, e) => ClearInput();
            dgv.SelectionChanged += Dgv_SelectionChanged;

            LoadData(null);
        }

        private void LoadData(string keyword)
        {
            try
            {
                dgv.DataSource = bus.Search(keyword);
                dgv.Columns["MaNV"].HeaderText = "Mã NV";
                dgv.Columns["TenNV"].HeaderText = "Tên nhân viên";
                dgv.Columns["SoDienThoai"].HeaderText = "Điện thoại";
                dgv.Columns["ChucVu"].HeaderText = "Chức vụ";
            }
            catch (Exception ex) { UiHelper.Error(ex.Message); }
        }

        private NhanVienDTO ReadInput()
        {
            return new NhanVienDTO
            {
                MaNV = maNV,
                TenNV = txtTen.Text,
                SoDienThoai = txtSDT.Text,
                ChucVu = cboChucVu.Text
            };
        }

        private void ClearInput()
        {
            maNV = 0; txtTen.Clear(); txtSDT.Clear(); cboChucVu.Text = "";
            dgv.ClearSelection();
            txtTen.Focus();
        }

        private void Dgv_SelectionChanged(object sender, EventArgs e)
        {
            NhanVienDTO nv = dgv.CurrentRow == null ? null : dgv.CurrentRow.DataBoundItem as NhanVienDTO;
            if (nv == null || !dgv.CurrentRow.Selected) return;
            maNV = nv.MaNV;
            txtTen.Text = nv.TenNV;
            txtSDT.Text = nv.SoDienThoai;
            cboChucVu.Text = nv.ChucVu;
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            try
            {
                bus.Add(ReadInput());
                UiHelper.Info("Đã thêm nhân viên.");
                LoadData(null);
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
                UiHelper.Info("Đã cập nhật nhân viên.");
                LoadData(txtSearch.Text);
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
                LoadData(txtSearch.Text);
                ClearInput();
            }
            catch (BusinessException ex) { UiHelper.Error(ex.Message); }
        }
    }
}
