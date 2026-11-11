using System;
using System.Data.SqlClient;

namespace HRM.BUS
{
    /// <summary>Lỗi nghiệp vụ có thông báo tiếng Việt để GUI hiển thị.</summary>
    public class BusinessException : Exception
    {
        public BusinessException(string message) : base(message) { }
        public BusinessException(string message, Exception inner) : base(message, inner) { }

        public static BusinessException FromSql(SqlException ex, string doiTuong)
        {
            switch (ex.Number)
            {
                case 2627:
                case 2601:
                    return new BusinessException(doiTuong + " đã tồn tại (trùng CCCD hoặc dữ liệu duy nhất khác).", ex);
                case 547:
                    return new BusinessException(
                        "Thao tác bị từ chối vì " + doiTuong + " đang được dùng ở dữ liệu khác hoặc giá trị không hợp lệ.", ex);
                default:
                    return new BusinessException("Lỗi cơ sở dữ liệu: " + ex.Message, ex);
            }
        }
    }
}
