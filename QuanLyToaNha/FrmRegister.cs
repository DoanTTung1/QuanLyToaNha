using System;
using System.Windows.Forms;
using System.Runtime.InteropServices;
namespace QuanLyToaNha
{
    public partial class FrmRegister : Form
    {
        public FrmRegister()
        {
            InitializeComponent();
            this.StartPosition = FormStartPosition.CenterScreen;
        }

        // --- SỰ KIỆN NÚT ĐĂNG KÝ ---
        private void btnRegister_Click(object sender, EventArgs e)
        {
            // 1. Kiểm tra nhập liệu (Chỉ cần check User, Pass, ConfirmPass)
            if (txtUser.Text == "" || txtPass.Text == "" || txtConfirmPass.Text == "")
            {
                MessageBox.Show("Vui lòng điền đầy đủ thông tin!", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 2. Kiểm tra mật khẩu nhập lại
            if (txtPass.Text != txtConfirmPass.Text)
            {
                MessageBox.Show("Mật khẩu nhập lại không khớp!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // 3. GIẢ LẬP ĐĂNG KÝ THÀNH CÔNG
            string thongBao = "Đăng ký thành công tài khoản: " + txtUser.Text + "\n(Hãy quay lại đăng nhập)";

            MessageBox.Show(thongBao, "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);

            // 4. Đóng form đăng ký
            this.Close();
        }

        // --- SỰ KIỆN NÚT HỦY ---
        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {

        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }
        // Sự kiện khi bấm vào dòng "Đăng nhập ngay"
        private void lnkLogin_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            this.Close(); // Chỉ cần đóng Form Đăng ký lại, Form Login cũ sẽ tự hiện ra
        }

        private void label4_Click(object sender, EventArgs e)
        {

        }
        [DllImport("user32.DLL", EntryPoint = "ReleaseCapture")]
        private extern static void ReleaseCapture();
        [DllImport("user32.DLL", EntryPoint = "SendMessage")]
        private extern static void SendMessage(System.IntPtr hwnd, int wmsg, int wparam, int lparam);

        // Hàm xử lý sự kiện khi ấn chuột xuống
        private void Form_MouseDown(object sender, MouseEventArgs e)
        {
            ReleaseCapture();
            SendMessage(this.Handle, 0x112, 0xf012, 0);
        }

        private void panel2_Paint(object sender, PaintEventArgs e)
        {

        }

        private void txtPass_TextChanged(object sender, EventArgs e)
        {

        }
    }
}