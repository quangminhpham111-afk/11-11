using System;
using System.Drawing;
using System.Windows.Forms;
using HRM.BUS;
using HRM.DTO;

namespace HRM.GUI
{
    public class FrmPhongBan : Form
    {
        private readonly PhongBanBUS bus = new PhongBanBUS();
        private readonly DataGridView dgv = UiHelper.Grid();
        private readonly TextBox txtTen = UiHelper.Txt(90, 15, 250);
        private readonly TextBox txtMoTa = UiHelper.Txt(90, 55, 450);
        private int maPB = 0;

        public FrmPhongBan()
        {
            Text = "Quản lý Phòng ban";
            Size = new Size(760, 500);
            StartPosition = FormStartPosition.CenterScreen;

            var pnlEdit = new Panel { Dock = DockStyle.Bottom, Height = 150 };
            var btnAdd = UiHelper.Btn("Thêm", 90, 100, 90);
            var btnEdit = UiHelper.Btn("Sửa", 190, 100, 90);
            var btnDel = UiHelper.Btn("Xóa", 290, 100, 90);
            var btnClear = UiHelper.Btn("Làm mới", 390, 100, 90);
            pnlEdit.Controls.AddRange(new Control[]
            {
                UiHelper.Lbl("Tên phòng:", 5, 15), txtTen,
                UiHelper.Lbl("Mô tả:", 5, 55), txtMoTa,
                btnAdd, btnEdit, btnDel, btnClear
            });

            Controls.Add(dgv);
            Controls.Add(pnlEdit);

            btnAdd.Click += BtnAdd_Click;
            btnEdit.Click += BtnEdit_Click;
            btnDel.Click += BtnDel_Click;
            btnClear.Click += (s, e) => ClearInput();
            dgv.SelectionChanged += Dgv_SelectionChanged;

            LoadData();
        }

        private void LoadData()
        {
            try
            {
                dgv.DataSource = bus.GetAll();
                dgv.Columns["MaPB"].HeaderText = "Mã PB";
                dgv.Columns["TenPB"].HeaderText = "Tên phòng ban";
                dgv.Columns["Mota"].HeaderText = "Mô tả";
            }
            catch (Exception ex) { UiHelper.Error(ex.Message); }
        }

        private PhongBanDTO ReadInput()
        {
            return new PhongBanDTO { MaPB = maPB, TenPB = txtTen.Text, Mota = txtMoTa.Text };
        }

        private void ClearInput()
        {
            maPB = 0; txtTen.Clear(); txtMoTa.Clear();
            dgv.ClearSelection();
            txtTen.Focus();
        }

        private void Dgv_SelectionChanged(object sender, EventArgs e)
        {
            PhongBanDTO pb = dgv.CurrentRow == null ? null : dgv.CurrentRow.DataBoundItem as PhongBanDTO;
            if (pb == null || !dgv.CurrentRow.Selected) return;
            maPB = pb.MaPB;
            txtTen.Text = pb.TenPB;
            txtMoTa.Text = pb.Mota;
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            try { bus.Add(ReadInput()); UiHelper.Info("Đã thêm phòng ban."); LoadData(); ClearInput(); }
            catch (BusinessException ex) { UiHelper.Error(ex.Message); }
        }

        private void BtnEdit_Click(object sender, EventArgs e)
        {
            if (maPB == 0) { UiHelper.Error("Hãy chọn phòng ban cần sửa."); return; }
            try { bus.Update(ReadInput()); UiHelper.Info("Đã cập nhật phòng ban."); LoadData(); }
            catch (BusinessException ex) { UiHelper.Error(ex.Message); }
        }

        private void BtnDel_Click(object sender, EventArgs e)
        {
            if (maPB == 0) { UiHelper.Error("Hãy chọn phòng ban cần xóa."); return; }
            if (!UiHelper.Confirm("Xóa phòng ban " + txtTen.Text + "?")) return;
            try { bus.Delete(maPB); LoadData(); ClearInput(); }
            catch (BusinessException ex) { UiHelper.Error(ex.Message); }
        }
    }
}
