using System;
using System.Drawing;
using System.Windows.Forms;
using RetailSales.BUS;
using RetailSales.DTO;

namespace RetailSales.GUI
{
    public class FrmKhachHang : Form
    {
        private readonly KhachHangBUS bus = new KhachHangBUS();
        private readonly DataGridView dgv = UiHelper.Grid();
        private readonly TextBox txtSearch = UiHelper.Txt(80, 10, 260);
        private readonly TextBox txtTen = UiHelper.Txt(100, 15, 250);
        private readonly TextBox txtDT = UiHelper.Txt(460, 15, 150);
        private readonly NumericUpDown nudDiem = UiHelper.Num(100, 55, 100, 1000000m);
        private int maKH = 0;   // 0 = chưa chọn dòng nào

        public FrmKhachHang()
        {
            Text = "Quản lý Khách hàng";
            Size = new Size(860, 560);
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
                UiHelper.Lbl("Tên KH:", 5, 15), txtTen,
                UiHelper.Lbl("Điện thoại:", 370, 15), txtDT,
                UiHelper.Lbl("Điểm tích lũy:", 5, 55), nudDiem,
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
                dgv.Columns["MaKH"].HeaderText = "Mã KH";
                dgv.Columns["TenKH"].HeaderText = "Tên khách hàng";
                dgv.Columns["DienThoai"].HeaderText = "Điện thoại";
                dgv.Columns["DiemTichLuy"].HeaderText = "Điểm tích lũy";
                dgv.Columns["HienThi"].Visible = false;
            }
            catch (Exception ex) { UiHelper.Error(ex.Message); }
        }

        private KhachHangDTO ReadInput()
        {
            return new KhachHangDTO
            {
                MaKH = maKH,
                TenKH = txtTen.Text,
                DienThoai = txtDT.Text,
                DiemTichLuy = (int)nudDiem.Value
            };
        }

        private void ClearInput()
        {
            maKH = 0; txtTen.Clear(); txtDT.Clear(); nudDiem.Value = 0;
            dgv.ClearSelection();
            txtTen.Focus();
        }

        private void Dgv_SelectionChanged(object sender, EventArgs e)
        {
            KhachHangDTO kh = dgv.CurrentRow == null ? null : dgv.CurrentRow.DataBoundItem as KhachHangDTO;
            if (kh == null || !dgv.CurrentRow.Selected) return;
            maKH = kh.MaKH;
            txtTen.Text = kh.TenKH;
            txtDT.Text = kh.DienThoai;
            nudDiem.Value = Math.Min(kh.DiemTichLuy, nudDiem.Maximum);
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            try
            {
                bus.Add(ReadInput());
                UiHelper.Info("Đã thêm khách hàng.");
                LoadData(null);
                ClearInput();
            }
            catch (BusinessException ex) { UiHelper.Error(ex.Message); }
        }

        private void BtnEdit_Click(object sender, EventArgs e)
        {
            if (maKH == 0) { UiHelper.Error("Hãy chọn khách hàng cần sửa."); return; }
            try
            {
                bus.Update(ReadInput());
                UiHelper.Info("Đã cập nhật khách hàng.");
                LoadData(txtSearch.Text);
            }
            catch (BusinessException ex) { UiHelper.Error(ex.Message); }
        }

        private void BtnDel_Click(object sender, EventArgs e)
        {
            if (maKH == 0) { UiHelper.Error("Hãy chọn khách hàng cần xóa."); return; }
            if (!UiHelper.Confirm("Xóa khách hàng " + txtTen.Text + "?")) return;
            try
            {
                bus.Delete(maKH);
                LoadData(txtSearch.Text);
                ClearInput();
            }
            catch (BusinessException ex) { UiHelper.Error(ex.Message); }
        }
    }
}
