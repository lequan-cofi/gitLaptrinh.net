﻿using BTLtest2.Class;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BTLtest2.function
{
    internal class thongkechiphi
    {
        // ... (Các phương thức hiện có của bạn) ...

        // Chuỗi kết nối CSDL của bạn
        private static string connectionString = "Data Source=DESKTOP-VT5RUI9;Initial Catalog=laptrinh.net;Integrated Security=True;Encrypt=True;TrustServerCertificate=True";
        private static SqlConnection connection = new SqlConnection(connectionString);

        // Chi phí ở đây được hiểu là tổng tiền nhập hàng
        public static List<MatHangThongKe> GetChiPhiTheoMatHang(DateTime fromDate, DateTime toDate, float? filterTren, float? filterDuoi)
        {
            List<MatHangThongKe> list = new List<MatHangThongKe>();
            StringBuilder queryBuilder = new StringBuilder(@"
                SELECT 
                    k.TenSach, 
                    SUM(ct.ThanhTien) as TongChiPhiMatHang
                FROM HoaDonNhap h
                JOIN ChiTietHDNhap ct ON h.SoHDNhap = ct.SoHDNhap
                JOIN KhoSach k ON ct.MaSach = k.MaSach
                WHERE h.NgayNhap BETWEEN @fromDate AND @toDate");

            var parameters = new Dictionary<string, object>
            {
                { "@fromDate", fromDate },
                { "@toDate", toDate }
            };

            queryBuilder.Append(" GROUP BY k.TenSach");

            List<string> havingClauses = new List<string>();
            if (filterTren.HasValue)
            {
                havingClauses.Add("SUM(ct.ThanhTien) >= @filterTren");
                parameters["@filterTren"] = filterTren.Value;
            }
            if (filterDuoi.HasValue)
            {
                havingClauses.Add("SUM(ct.ThanhTien) <= @filterDuoi");
                parameters["@filterDuoi"] = filterDuoi.Value;
            }

            if (havingClauses.Any())
            {
                queryBuilder.Append(" HAVING " + string.Join(" AND ", havingClauses));
            }
            queryBuilder.Append(" ORDER BY TongChiPhiMatHang DESC");

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand(queryBuilder.ToString(), connection))
                {
                    foreach (var param in parameters)
                    {
                        command.Parameters.AddWithValue(param.Key, param.Value);
                    }
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            list.Add(new MatHangThongKe
                            {
                                TenMatHang = reader["TenSach"].ToString(),
                                GiaTri = Convert.ToSingle(reader["TongChiPhiMatHang"])
                            });
                        }
                    }
                }
            }
            return list;
        }
    }
}
