using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using HRM.DTO;

namespace HRM.DAL
{
    public class PhongBanDAL
    {
        public List<PhongBanDTO> GetAll()
        {
            var list = new List<PhongBanDTO>();
            using (SqlConnection conn = DbHelper.CreateConnection())
            using (SqlCommand cmd = new SqlCommand("SELECT MaPB, TenPB, Mota FROM PhongBan ORDER BY MaPB", conn))
            {
                conn.Open();
                using (SqlDataReader rd = cmd.ExecuteReader())
                {
                    while (rd.Read())
                    {
                        list.Add(new PhongBanDTO
                        {
                            MaPB = rd.GetInt32(0),
                            TenPB = rd.GetString(1),
                            Mota = rd.IsDBNull(2) ? null : rd.GetString(2)
                        });
                    }
                }
            }
            return list;
        }

        public int Insert(PhongBanDTO pb)
        {
            using (SqlConnection conn = DbHelper.CreateConnection())
            using (SqlCommand cmd = new SqlCommand("INSERT INTO PhongBan (TenPB, Mota) VALUES (@ten, @mt)", conn))
            {
                cmd.Parameters.Add("@ten", SqlDbType.NVarChar, 100).Value = pb.TenPB;
                cmd.Parameters.Add("@mt", SqlDbType.NVarChar, 255).Value = DbHelper.NullIfEmpty(pb.Mota);
                conn.Open();
                return cmd.ExecuteNonQuery();
            }
        }

        public int Update(PhongBanDTO pb)
        {
            using (SqlConnection conn = DbHelper.CreateConnection())
            using (SqlCommand cmd = new SqlCommand("UPDATE PhongBan SET TenPB = @ten, Mota = @mt WHERE MaPB = @ma", conn))
            {
                cmd.Parameters.Add("@ma", SqlDbType.Int).Value = pb.MaPB;
                cmd.Parameters.Add("@ten", SqlDbType.NVarChar, 100).Value = pb.TenPB;
                cmd.Parameters.Add("@mt", SqlDbType.NVarChar, 255).Value = DbHelper.NullIfEmpty(pb.Mota);
                conn.Open();
                return cmd.ExecuteNonQuery();
            }
        }

        public int Delete(int maPB)
        {
            using (SqlConnection conn = DbHelper.CreateConnection())
            using (SqlCommand cmd = new SqlCommand("DELETE FROM PhongBan WHERE MaPB = @ma", conn))
            {
                cmd.Parameters.Add("@ma", SqlDbType.Int).Value = maPB;
                conn.Open();
                return cmd.ExecuteNonQuery();
            }
        }
    }
}
