using System;
using System.Drawing;
using System.Windows.Forms;
using RetailSales.BUS;
using RetailSales.DTO;

namespace RetailSales.GUI
{
    /// <summary>CRUD Sản phẩm + tìm kiếm theo tên / mã vạch (SqlParameter + %).</summary>
    public class FrmSanPham : Form
    {
        private readonly SanPhamBUS bus = new SanPhamBUS();
        private readonly DataGridView dgv = UiHelper.Grid();
        private readonly TextBox txtSearch = UiHelper.Txt(80, 10, 260);
        private readonly TextBox txtMa = UiHelper.Txt(100, 15, 150);
        private readonly TextBox txtTen = UiHelper.Txt(360, 15, 300);
        private readonly NumericUpDown nudGia = UiHelper.Num(100, 55, 150, 1000000000m);
        private readonly NumericUpDown nudTon = UiHelper.Num(360, 55, 120, 1000000m);

        public FrmSanPham()
        {
            Text = "Quản lý Sản phẩm";
            Size = new Size(920, 600);
            StartPosition = FormStartPosition.CenterScreen;

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
                UiHelper.Lbl("Mã SP/Barcode:", 5, 15), txtMa,
                UiHelper.Lbl("Tên SP:", 290, 15), txtTen,
                UiHelper.Lbl("Giá bán:", 5, 55), nudGia,
                UiHelper.Lbl("Tồn kho:", 290, 55), nudTon,
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
                dgv.Columns["MaSP"].HeaderText = "Mã SP / Barcode";
                dgv.Columns["TenSP"].HeaderText = "Tên sản phẩm";
                dgv.Columns["GiaBan"].HeaderText = "Giá bán";
                dgv.Columns["GiaBan"].DefaultCellStyle.Format = "N0";
                dgv.Columns["SoLuongTon"].HeaderText = "Tồn kho";
                dgv.Columns["HienThi"].Visible = false;
            }
            catch (Exception ex) { UiHelper.Error(ex.Message); }
        }

        private SanPhamDTO ReadInput()
        {
            return new SanPhamDTO
            {
                MaSP = txtMa.Text,
                TenSP = txtTen.Text,
                GiaBan = nudGia.Value,
                SoLuongTon = (int)nudTon.Value
            };
        }

        private void ClearInput()
        {
            txtMa.Clear(); txtTen.Clear(); nudGia.Value = 0; nudTon.Value = 0;
            txtMa.ReadOnly = false;
            dgv.ClearSelection();
            txtMa.Focus();
        }

        private void Dgv_SelectionChanged(object sender, EventArgs e)
        {
            SanPhamDTO sp = dgv.CurrentRow == null ? null : dgv.CurrentRow.DataBoundItem as SanPhamDTO;
            if (sp == null || !dgv.CurrentRow.Selected) return;
            txtMa.Text = sp.MaSP;
            txtMa.ReadOnly = true;             // khóa chính không cho sửa
            txtTen.Text = sp.TenSP;
            nudGia.Value = Math.Min(sp.GiaBan, nudGia.Maximum);
            nudTon.Value = Math.Min(sp.SoLuongTon, nudTon.Maximum);
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            try
            {
                bus.Add(ReadInput());
                UiHelper.Info("Đã thêm sản phẩm.");
                LoadData(null);
                ClearInput();
            }
            catch (BusinessException ex) { UiHelper.Error(ex.Message); }
        }

        private void BtnEdit_Click(object sender, EventArgs e)
        {
            try
            {
                bus.Update(ReadInput());
                UiHelper.Info("Đã cập nhật sản phẩm.");
                LoadData(txtSearch.Text);
            }
            catch (BusinessException ex) { UiHelper.Error(ex.Message); }
        }

        private void BtnDel_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtMa.Text)) { UiHelper.Error("Hãy chọn sản phẩm cần xóa."); return; }
            if (!UiHelper.Confirm("Xóa sản phẩm " + txtMa.Text + "?")) return;
            try
            {
                bus.Delete(txtMa.Text.Trim());
                LoadData(txtSearch.Text);
                ClearInput();
            }
            catch (BusinessException ex) { UiHelper.Error(ex.Message); }
        }
    }
}
