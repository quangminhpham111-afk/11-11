using System;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using RetailSales.BUS;
using RetailSales.DTO;

namespace RetailSales.GUI
{
    /// <summary>Màn hình Bán hàng / Lập hóa đơn. Nút "Thanh toán" chạy SqlTransaction (xem HoaDonDAL).</summary>
    public class FrmBanHang : Form
    {
        private readonly SanPhamBUS spBus = new SanPhamBUS();
        private readonly KhachHangBUS khBus = new KhachHangBUS();
        private readonly NhanVienBUS nvBus = new NhanVienBUS();
        private readonly BanHangBUS bhBus = new BanHangBUS();

        private readonly BindingList<CartItemDTO> gio = new BindingList<CartItemDTO>();
        private readonly DataGridView dgv = UiHelper.Grid();

        private readonly ComboBox cboNV = new ComboBox { Left = 85, Top = 10, Width = 220, DropDownStyle = ComboBoxStyle.DropDownList };
        private readonly ComboBox cboKH = new ComboBox { Left = 400, Top = 10, Width = 300, DropDownStyle = ComboBoxStyle.DropDownList };
        private readonly TextBox txtBarcode = UiHelper.Txt(125, 48, 160);
        private readonly ComboBox cboSP = new ComboBox { Left = 365, Top = 48, Width = 340, DropDownStyle = ComboBoxStyle.DropDownList };
        private readonly NumericUpDown nudSL = new NumericUpDown { Left = 750, Top = 48, Width = 60, Minimum = 1, Maximum = 100000, Value = 1 };
        private readonly Label lblTong = new Label { AutoSize = true, Left = 15, Top = 12, Font = new Font("Segoe UI", 14f, FontStyle.Bold), ForeColor = Color.Firebrick };
        private readonly Label lblDiem = new Label { AutoSize = true, Left = 15, Top = 48, Font = new Font("Segoe UI", 10f) };

        public FrmBanHang()
        {
            Text = "Bán hàng / Lập hóa đơn";
            Size = new Size(960, 640);
            StartPosition = FormStartPosition.CenterScreen;

            var pnlTop = new Panel { Dock = DockStyle.Top, Height = 90 };
            var btnThem = UiHelper.Btn("Thêm vào giỏ", 825, 44, 100);
            pnlTop.Controls.AddRange(new Control[]
            {
                UiHelper.Lbl("Thu ngân:", 10, 10), cboNV,
                UiHelper.Lbl("Khách hàng:", 320, 10), cboKH,
                UiHelper.Lbl("Mã SP / Barcode:", 10, 48), txtBarcode,
                UiHelper.Lbl("Sản phẩm:", 295, 48), cboSP,
                UiHelper.Lbl("SL:", 715, 48), nudSL,
                btnThem
            });

            var pnlBottom = new Panel { Dock = DockStyle.Bottom, Height = 90 };
            var btnXoa = UiHelper.Btn("Xóa dòng", 560, 40, 100);
            var btnHuy = UiHelper.Btn("Hủy giỏ", 670, 40, 100);
            var btnPay = UiHelper.Btn("THANH TOÁN", 780, 30, 140);
            btnPay.Height = 45;
            btnPay.BackColor = Color.SeaGreen;
            btnPay.ForeColor = Color.White;
            btnPay.Font = new Font("Segoe UI", 10f, FontStyle.Bold);
            pnlBottom.Controls.AddRange(new Control[] { lblTong, lblDiem, btnXoa, btnHuy, btnPay });

            Controls.Add(dgv);
            Controls.Add(pnlBottom);
            Controls.Add(pnlTop);

            // Lưới giỏ hàng: chỉ cho sửa cột Số lượng
            dgv.ReadOnly = false;
            dgv.DataSource = gio;
            dgv.Columns["MaSP"].HeaderText = "Mã SP";
            dgv.Columns["TenSP"].HeaderText = "Tên sản phẩm";
            dgv.Columns["SoLuong"].HeaderText = "Số lượng";
            dgv.Columns["DonGia"].HeaderText = "Đơn giá";
            dgv.Columns["ThanhTien"].HeaderText = "Thành tiền";
            dgv.Columns["DonGia"].DefaultCellStyle.Format = "N0";
            dgv.Columns["ThanhTien"].DefaultCellStyle.Format = "N0";
            foreach (string c in new[] { "MaSP", "TenSP", "DonGia", "ThanhTien" })
                dgv.Columns[c].ReadOnly = true;

            btnThem.Click += (s, e) => ThemTuCombo();
            txtBarcode.KeyDown += TxtBarcode_KeyDown;
            btnXoa.Click += BtnXoa_Click;
            btnHuy.Click += (s, e) => { gio.Clear(); TinhLai(); };
            btnPay.Click += BtnPay_Click;
            dgv.CellEndEdit += Dgv_CellEndEdit;
            dgv.DataError += (s, e) => { UiHelper.Error("Số lượng không hợp lệ."); e.ThrowException = false; };
            cboKH.SelectedIndexChanged += (s, e) => TinhLai();

            NapDanhMuc();
            TinhLai();
        }

        private void NapDanhMuc()
        {
            try
            {
                cboNV.DisplayMember = "TenNV"; cboNV.ValueMember = "MaNV";
                cboNV.DataSource = nvBus.GetAll();

                cboKH.DisplayMember = "HienThi"; cboKH.ValueMember = "MaKH";
                cboKH.DataSource = khBus.GetForSale();

                NapSanPham();
            }
            catch (Exception ex) { UiHelper.Error(ex.Message); }
        }

        private void NapSanPham()
        {
            cboSP.DisplayMember = "HienThi"; cboSP.ValueMember = "MaSP";
            cboSP.DataSource = spBus.GetAll();
        }

        private void ThemTuCombo()
        {
            try
            {
                SanPhamDTO sp = cboSP.SelectedItem as SanPhamDTO;
                if (sp != null) sp = spBus.GetById(sp.MaSP);        // lấy tồn kho mới nhất
                BanHangBUS.ThemVaoGio(gio, sp, (int)nudSL.Value);
                dgv.Refresh();
                TinhLai();
            }
            catch (BusinessException ex) { UiHelper.Error(ex.Message); }
        }

        /// <summary>Quét mã vạch (máy quét gõ mã rồi Enter) -> thêm ngay vào giỏ.</summary>
        private void TxtBarcode_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Enter) return;
            e.SuppressKeyPress = true;
            try
            {
                SanPhamDTO sp = spBus.GetById(txtBarcode.Text);
                if (sp == null) { UiHelper.Error("Không tìm thấy mã sản phẩm: " + txtBarcode.Text); return; }
                BanHangBUS.ThemVaoGio(gio, sp, (int)nudSL.Value);
                dgv.Refresh();
                TinhLai();
                txtBarcode.Clear();
            }
            catch (BusinessException ex) { UiHelper.Error(ex.Message); }
        }

        private void Dgv_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            CartItemDTO item = dgv.Rows[e.RowIndex].DataBoundItem as CartItemDTO;
            if (item != null && item.SoLuong <= 0) item.SoLuong = 1;
            dgv.Refresh();
            TinhLai();
        }

        private void BtnXoa_Click(object sender, EventArgs e)
        {
            CartItemDTO item = dgv.CurrentRow == null ? null : dgv.CurrentRow.DataBoundItem as CartItemDTO;
            if (item == null) return;
            gio.Remove(item);
            TinhLai();
        }

        private void TinhLai()
        {
            decimal tong = BanHangBUS.TinhTongTien(gio);
            lblTong.Text = "Tổng tiền: " + UiHelper.Money(tong);

            int maKH = (cboKH.SelectedValue is int) ? (int)cboKH.SelectedValue : 0;
            lblDiem.Text = maKH > 0
                ? "Điểm tích lũy sẽ cộng: +" + BanHangBUS.TinhDiemTichLuy(tong)
                : "Khách lẻ - không tích điểm";
        }

        private void BtnPay_Click(object sender, EventArgs e)
        {
            if (gio.Count == 0) { UiHelper.Error("Giỏ hàng đang trống."); return; }
            int maNV = (cboNV.SelectedValue is int) ? (int)cboNV.SelectedValue : 0;
            int maKH = (cboKH.SelectedValue is int) ? (int)cboKH.SelectedValue : 0;
            try
            {
                int maHD = bhBus.ThanhToan(maNV, maKH, gio.ToList());   // SqlTransaction nằm trong DAL
                UiHelper.Info("Thanh toán thành công!\nMã hóa đơn: " + maHD +
                              "\nTổng tiền: " + UiHelper.Money(BanHangBUS.TinhTongTien(gio)));
                gio.Clear();
                NapDanhMuc();            // cập nhật tồn kho + điểm tích lũy mới
                TinhLai();
            }
            catch (BusinessException ex)
            {
                MessageBox.Show(ex.Message, "Thanh toán thất bại", MessageBoxButtons.OK, MessageBoxIcon.Error);
                NapSanPham();
            }
        }
    }
}
