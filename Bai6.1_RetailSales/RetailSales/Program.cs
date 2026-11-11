using System;
using System.Data.SqlClient;
using System.Windows.Forms;
using RetailSales.DAL;
using RetailSales.GUI;

namespace RetailSales
{
    internal static class Program
    {
        [STAThread]
        private static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Kiểm tra kết nối trước khi mở ứng dụng
            try
            {
                using (SqlConnection conn = DbHelper.CreateConnection())
                {
                    conn.Open();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Không kết nối được SalesDB.\n" +
                    "Hãy chạy Database/ScriptDB.sql trong SSMS và kiểm tra chuỗi kết nối trong App.config.\n\n" + ex.Message,
                    "Lỗi kết nối", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            Application.Run(new FrmMain());
        }
    }
}
