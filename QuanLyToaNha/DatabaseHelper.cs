using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuanLyToaNha
{
    public class DatabaseHelper
    {
        // CHÚ Ý: Bạn sửa lại chuỗi kết nối này cho đúng máy bạn
        // Server=.; nghĩa là máy local. Nếu không chạy, thử đổi dấu chấm thành tên máy bạn (VD: LAPTOP-XYZ\SQLEXPRESS)
        // Database=estatebasic; là tên CSDL trong file SQL bạn gửi
        public static string strConnect = "Server=.;Database=estatebasic;Integrated Security=True";

        public static SqlConnection GetConnection()
        {
            return new SqlConnection(strConnect);
        }
    }
}
