using System.Drawing;
using System.Globalization;
using System.Windows.Forms;

namespace HRM.GUI
{
    internal static class UiHelper
    {
        public static readonly CultureInfo Vn = new CultureInfo("vi-VN");

        public static string Money(decimal v) { return v.ToString("N0", Vn) + " đ"; }

        public static Label Lbl(string text, int x, int y)
        {
            return new Label { Text = text, Left = x, Top = y + 4, AutoSize = true };
        }

        public static TextBox Txt(int x, int y, int w)
        {
            return new TextBox { Left = x, Top = y, Width = w };
        }

        public static NumericUpDown Num(int x, int y, int w, decimal max, int decimals)
        {
            return new NumericUpDown
            {
                Left = x, Top = y, Width = w, Minimum = 0, Maximum = max,
                DecimalPlaces = decimals, ThousandsSeparator = true
            };
        }

        public static Button Btn(string text, int x, int y, int w)
        {
            return new Button { Text = text, Left = x, Top = y, Width = w, Height = 30 };
        }

        public static DataGridView Grid()
        {
            return new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                MultiSelect = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                RowHeadersVisible = false,
                BackgroundColor = Color.White
            };
        }

        public static void Error(string msg)
        {
            MessageBox.Show(msg, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        public static void Info(string msg)
        {
            MessageBox.Show(msg, "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        public static bool Confirm(string msg)
        {
            return MessageBox.Show(msg, "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question)
                   == DialogResult.Yes;
        }
    }
}
