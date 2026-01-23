using System;
using System.Configuration;
using System.Data;
using MySql.Data.MySqlClient; // Thư viện MySQL

namespace QuanLyToaNha
{
    public class DatabaseHelper
    {
        // Lấy chuỗi kết nối từ App.config
        private static string strCon = ConfigurationManager.ConnectionStrings["AivenMySQL"].ConnectionString;

        // 1. Hàm lấy kết nối
        public static MySqlConnection GetConnection()
        {
            MySqlConnection con = new MySqlConnection(strCon);
            // Không mở ở đây để tránh lỗi Connection pool, để người dùng tự mở khi cần hoặc dùng using
            return con;
        }

        // 2. Hàm lấy dữ liệu (SELECT) - Đã thêm hàm này
        public static DataTable GetData(string sql)
        {
            using (MySqlConnection con = GetConnection())
            {
                con.Open();
                MySqlDataAdapter dap = new MySqlDataAdapter(sql, con);
                DataTable dt = new DataTable();
                dap.Fill(dt);
                return dt;
            }
        }

        // 3. Hàm thực thi lệnh (INSERT, UPDATE, DELETE) - Đã thêm hàm này
        public static void ExecuteSql(string sql)
        {
            using (MySqlConnection con = GetConnection())
            {
                con.Open();
                MySqlCommand cmd = new MySqlCommand(sql, con);
                cmd.ExecuteNonQuery();
            }
        }
    }
}