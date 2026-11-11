using System;
using System.Data.SqlClient;

namespace RetailSales.BUS
{
    /// <summary>Lỗi nghiệp vụ có thông báo tiếng Việt thân thiện để GUI hiển thị.</summary>
    public class BusinessException : Exception
    {
        public BusinessException(string message) : base(message) { }
        public BusinessException(string message, Exception inner) : base(message, inner) { }

        /// <summary>Chuyển SqlException thành thông báo dễ hiểu.</summary>
        public static BusinessException FromSql(SqlException ex, string doiTuong)
        {
            switch (ex.Number)
            {
                case 2627:
                case 2601:
                    return new BusinessException(doiTuong + " đã tồn tại (trùng khóa hoặc số điện thoại/CCCD).", ex);
                case 547:
                    return new BusinessException(
                        "Thao tác bị từ chối vì " + doiTuong + " đang được dùng ở dữ liệu khác hoặc giá trị không hợp lệ.", ex);
                default:
                    return new BusinessException("Lỗi cơ sở dữ liệu: " + ex.Message, ex);
            }
        }
    }
}
