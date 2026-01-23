using System;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using MySql.Data.MySqlClient;
using System.Data;

namespace QuanLyToaNha
{
    public partial class FrmRegister : Form
    {
        public FrmRegister()
        {
            InitializeComponent();
            this.StartPosition = FormStartPosition.CenterScreen;
            // Đảm bảo ô mật khẩu hiển thị dấu *
            if (txtPass != null) txtPass.UseSystemPasswordChar = true;
            if (txtConfirmPass != null) txtConfirmPass.UseSystemPasswordChar = true;
        }

        private void btnRegister_Click(object sender, EventArgs e)
        {
            // 1. Kiểm tra nhập liệu
            if (txtUser.Text == "" || txtPass.Text == "" || txtConfirmPass.Text == "" || txtFullName.Text == "")
            {
                MessageBox.Show("Vui lòng điền đầy đủ thông tin (bao gồm Họ tên)!", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 2. Kiểm tra khớp mật khẩu
            if (txtPass.Text != txtConfirmPass.Text)
            {
                MessageBox.Show("Mật khẩu nhập lại không khớp!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // 3. Xử lý lưu vào Database
            try
            {
                using (MySqlConnection conn = DatabaseHelper.GetConnection())
                {
                    if (conn.State == ConnectionState.Closed) conn.Open();

                    // Bước 3.1: Kiểm tra trùng tên tài khoản
                    string checkSql = "SELECT COUNT(*) FROM users WHERE username = @u";
                    MySqlCommand checkCmd = new MySqlCommand(checkSql, conn);
                    checkCmd.Parameters.AddWithValue("@u", txtUser.Text.Trim());

                    long count = Convert.ToInt64(checkCmd.ExecuteScalar());
                    if (count > 0)
                    {
                        MessageBox.Show("Tài khoản này đã tồn tại. Vui lòng chọn tên khác!", "Trùng lặp", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    // Bước 3.2: Thêm tài khoản mới (Role resident)
                    string insertSql = "INSERT INTO users (username, password, full_name, role) VALUES (@u, @p, @fn, 'resident')";

                    MySqlCommand cmd = new MySqlCommand(insertSql, conn);
                    cmd.Parameters.AddWithValue("@u", txtUser.Text.Trim());
                    cmd.Parameters.AddWithValue("@p", txtPass.Text.Trim());
                    cmd.Parameters.AddWithValue("@fn", txtFullName.Text.Trim());

                    int rows = cmd.ExecuteNonQuery();
                    if (rows > 0)
                    {
                        MessageBox.Show("Đăng ký thành công! Hãy đăng nhập để truy cập Cổng cư dân.", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        this.Close(); // Đóng form
                    }
                    else
                    {
                        MessageBox.Show("Đăng ký thất bại, vui lòng thử lại.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi kết nối: " + ex.Message, "Lỗi Server", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // --- CÁC NÚT KHÁC ---
        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void lnkLogin_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            this.Close();
        }

        // --- KÉO THẢ FORM ---
        [DllImport("user32.DLL", EntryPoint = "ReleaseCapture")]
        private extern static void ReleaseCapture();
        [DllImport("user32.DLL", EntryPoint = "SendMessage")]
        private extern static void SendMessage(System.IntPtr hwnd, int wmsg, int wparam, int lparam);

        private void Form_MouseDown(object sender, MouseEventArgs e)
        {
            ReleaseCapture();
            SendMessage(this.Handle, 0x112, 0xf012, 0);
        }

        // --- CÁC HÀM RÁC (Giữ lại để không lỗi Designer) ---
        // Đã thêm label7_Click vào đây để sửa lỗi của bạn
        private void label7_Click(object sender, EventArgs e) { }
        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e) { }
        private void panel1_Paint(object sender, PaintEventArgs e) { }
        private void pictureBox1_Click(object sender, EventArgs e) { }
        private void label4_Click(object sender, EventArgs e) { }
        private void panel2_Paint(object sender, PaintEventArgs e) { }
        private void txtPass_TextChanged(object sender, EventArgs e) { }
    }
}