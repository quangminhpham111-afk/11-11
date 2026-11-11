using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace RetailSales.DAL
{
    /// <summary>Tiện ích dùng chung cho tầng DAL: chuỗi kết nối, tham số, escape LIKE.</summary>
    public static class DbHelper
    {
        public static string ConnectionString
        {
            get { return ConfigurationManager.ConnectionStrings["SalesDB"].ConnectionString; }
        }

        public static SqlConnection CreateConnection()
        {
            return new SqlConnection(ConnectionString);
        }

        /// <summary>Chuỗi rỗng -> DBNull (để cột UNIQUE nullable không bị trùng chuỗi rỗng).</summary>
        public static object NullIfEmpty(string s)
        {
            return string.IsNullOrWhiteSpace(s) ? (object)DBNull.Value : s.Trim();
        }

        /// <summary>Thêm tham số decimal(18,2) đúng precision/scale.</summary>
        public static SqlParameter AddDecimal(SqlCommand cmd, string name, decimal value)
        {
            SqlParameter p = cmd.Parameters.Add(name, SqlDbType.Decimal);
            p.Precision = 18;
            p.Scale = 2;
            p.Value = value;
            return p;
        }

        /// <summary>
        /// Tạo mẫu LIKE an toàn: escape các ký tự đặc biệt của LIKE (% _ [ \)
        /// rồi bao bằng % ở hai đầu. Dùng kèm "LIKE @kw ESCAPE '\'".
        /// </summary>
        public static string LikePattern(string keyword)
        {
            keyword = (keyword ?? string.Empty).Trim()
                .Replace("\\", "\\\\")
                .Replace("%", "\\%")
                .Replace("_", "\\_")
                .Replace("[", "\\[");
            return "%" + keyword + "%";
        }
    }
}
