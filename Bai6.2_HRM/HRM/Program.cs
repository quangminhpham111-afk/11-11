using System;
using System.Data.SqlClient;
using System.Windows.Forms;
using HRM.DAL;
using HRM.GUI;

namespace HRM
{
    internal static class Program
    {
        [STAThread]
        private static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

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
                    "Không kết nối được HRMDB.\n" +
                    "Hãy chạy Database/ScriptDB.sql trong SSMS và kiểm tra chuỗi kết nối trong App.config.\n\n" + ex.Message,
                    "Lỗi kết nối", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            Application.Run(new FrmMain());
        }
    }
}
