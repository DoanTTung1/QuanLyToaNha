using System;
using System.Windows.Forms;
using System.Runtime.InteropServices;

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
            // Đảm bảo ô mật khẩu hiển thị dấu * (nếu chưa chỉnh trong Properties)
            textBox1.UseSystemPasswordChar = true;
        }

        // --- SỰ KIỆN NÚT ĐĂNG NHẬP ---
        private void btnLogin_Click(object sender, EventArgs e)
        {
            // 1. Lấy thông tin (Giữ nguyên textBox1 như ý bạn)
            string username = txtUser.Text.Trim();
            string password = textBox1.Text.Trim();

            // 2. CHECK TÀI KHOẢN ADMIN / 123
            if (username == "admin" && password == "123")
            {
                MessageBox.Show("Đăng nhập thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);

                // 3. Mở Form Main
                // Lưu ý: Đảm bảo bạn đã tạo FrmMain như bài trước
                FrmMain frm = new FrmMain();

                this.Hide();      // Ẩn form login
                frm.ShowDialog(); // Hiện form Dashboard
                this.Close();     // Đóng app sau khi thoát Dashboard
            }
            else
            {
                MessageBox.Show("Sai tài khoản! (Thử: admin / 123)", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
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

        // --- CÁC HÀM RÁC (Giữ nguyên để không bị lỗi giao diện) ---
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