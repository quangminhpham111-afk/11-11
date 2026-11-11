using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace HRM.DAL
{
    public static class DbHelper
    {
        public static string ConnectionString
        {
            get { return ConfigurationManager.ConnectionStrings["HRMDB"].ConnectionString; }
        }

        public static SqlConnection CreateConnection()
        {
            return new SqlConnection(ConnectionString);
        }

        public static object NullIfEmpty(string s)
        {
            return string.IsNullOrWhiteSpace(s) ? (object)System.DBNull.Value : s.Trim();
        }

        public static SqlParameter AddDecimal(SqlCommand cmd, string name, decimal value)
        {
            SqlParameter p = cmd.Parameters.Add(name, SqlDbType.Decimal);
            p.Precision = 18;
            p.Scale = 2;
            p.Value = value;
            return p;
        }

        /// <summary>Mẫu LIKE an toàn (escape % _ [ \). Dùng kèm "LIKE @kw ESCAPE '\'".</summary>
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
