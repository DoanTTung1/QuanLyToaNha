using System;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using MySql.Data.MySqlClient; // QUAN TRỌNG: Thư viện MySQL

namespace QuanLyToaNha
{
    public partial class FrmLogin : Form
    {
        // --- Code để di chuyển Form không viền ---
        [DllImport("user32.DLL", EntryPoint = "ReleaseCapture")]
        private extern static void ReleaseCapture();
        [DllImport("user32.DLL", EntryPoint = "SendMessage")]
        private extern static void SendMessage(System.IntPtr hwnd, int wmsg, int wparam, int lparam);

        public FrmLogin()
        {
            InitializeComponent();
            this.StartPosition = FormStartPosition.CenterScreen;
            // Đảm bảo ô mật khẩu hiển thị dấu *
            textBox1.UseSystemPasswordChar = true;
        }

        // --- SỰ KIỆN NÚT ĐĂNG NHẬP (ĐÃ SỬA ĐỂ KẾT NỐI DATABASE) ---
        private void btnLogin_Click(object sender, EventArgs e)
        {
            string username = txtUser.Text.Trim();
            string password = textBox1.Text.Trim(); // Ô mật khẩu

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Vui lòng nhập đầy đủ tài khoản và mật khẩu!");
                return;
            }

            try
            {
                // 1. Kết nối Database
                using (MySqlConnection conn = DatabaseHelper.GetConnection())
                {
                    if (conn.State == System.Data.ConnectionState.Closed) conn.Open();

                    // 2. Tạo câu lệnh SQL kiểm tra đăng nhập
                    // Lưu ý: Password nên mã hóa MD5/SHA, nhưng ở đây ta làm đơn giản trước
                    string sql = "SELECT full_name, role FROM users WHERE username = @u AND password = @p";

                    MySqlCommand cmd = new MySqlCommand(sql, conn);
                    cmd.Parameters.AddWithValue("@u", username);
                    cmd.Parameters.AddWithValue("@p", password);

                    // 3. Thực thi và đọc kết quả
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            // Đăng nhập thành công! Lấy thông tin
                            string fullName = reader["full_name"].ToString();
                            string role = reader["role"].ToString();

                            MessageBox.Show($"Xin chào {fullName}!", "Đăng nhập thành công");

                            this.Hide();

                            // 4. Phân quyền chuyển hướng
                            if (role == "admin")
                            {
                                // Nếu là Admin -> Vào trang quản lý chính
                                FrmMain frmAdmin = new FrmMain();
                                frmAdmin.ShowDialog();
                            }
                            else
                            {
                                // Nếu là staff hoặc user -> Vào trang Portal
                                // Truyền tên thật vào Portal
                                FrmPortal frmUser = new FrmPortal(fullName);
                                frmUser.ShowDialog();
                            }

                            this.Close();
                        }
                        else
                        {
                            MessageBox.Show("Sai tài khoản hoặc mật khẩu!", "Lỗi đăng nhập", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi kết nối Server: " + ex.Message);
            }
        }

        // --- SỰ KIỆN NÚT THOÁT (X) ---
        private void btnExit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        // --- SỰ KIỆN LINK ĐĂNG KÝ ---
        private void lnkRegister_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            FrmRegister frm = new FrmRegister();
            this.Hide();
            frm.ShowDialog();
            this.Show();
        }

        // --- SỰ KIỆN KÉO FORM ---
        private void panelLeft_MouseDown(object sender, MouseEventArgs e)
        {
            ReleaseCapture();
            SendMessage(this.Handle, 0x112, 0xf012, 0);
        }

        // --- CÁC HÀM RÁC (Giữ nguyên để không lỗi Designer) ---
        private void panel2_Paint(object sender, PaintEventArgs e) { }
        private void contextMenuStrip1_Opening(object sender, System.ComponentModel.CancelEventArgs e) { }
        private void panel1_Paint(object sender, PaintEventArgs e) { }
        private void label1_Click(object sender, EventArgs e) { }
        private void label2_Click(object sender, EventArgs e) { }
        private void label4_Click(object sender, EventArgs e) { }
        private void panel3_Paint(object sender, PaintEventArgs e) { }
        private void panel4_Paint(object sender, PaintEventArgs e) { }
        private void textBox1_TextChanged(object sender, EventArgs e) { }
        private void txtPass_Click(object sender, EventArgs e) { }
        private void pnUserWrapper_Paint(object sender, PaintEventArgs e) { }
        private void lineUser_Paint(object sender, PaintEventArgs e) { }
        private void txtUser_TextChanged(object sender, EventArgs e) { }
        private void label3_Click(object sender, EventArgs e) { }
        private void pictureBox1_Click(object sender, EventArgs e) { }
    }
}