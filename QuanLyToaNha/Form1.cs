using System;
using System.Windows.Forms;
using System.Runtime.InteropServices; // 1. Thêm thư viện này để di chuyển form

namespace QuanLyToaNha
{
    public partial class Form1 : Form
    {
        // 2. Code để di chuyển Form (Giống hệt bên Login)
        [DllImport("user32.DLL", EntryPoint = "ReleaseCapture")]
        private extern static void ReleaseCapture();
        [DllImport("user32.DLL", EntryPoint = "SendMessage")]
        private extern static void SendMessage(System.IntPtr hwnd, int wmsg, int wparam, int lparam);

        public Form1()
        {
            InitializeComponent();

            // Cấu hình Form không viền (nếu chưa chỉnh bên Properties)
            this.FormBorderStyle = FormBorderStyle.None;
            this.StartPosition = FormStartPosition.CenterScreen;

            // Load trang chủ đầu tiên
            ShowUserControl(new UC_TrangChu());
        }

        // --- HÀM CHUYỂN TRANG (Của bạn) ---
        private void ShowUserControl(UserControl uc)
        {
            uc.Dock = DockStyle.Fill;
            panelContent.Controls.Clear();
            panelContent.Controls.Add(uc);
            uc.BringToFront();
        }

        // --- CÁC SỰ KIỆN NÚT MENU ---
        private void btnTrangChu_Click(object sender, EventArgs e)
        {
            ShowUserControl(new UC_TrangChu());
        }

        private void btnCuDan_Click(object sender, EventArgs e)
        {
            ShowUserControl(new UC_CuDan()); // Hiển thị khách hàng
        }

        private void btnToaNha_Click(object sender, EventArgs e)
        {
            ShowUserControl(new UC_Building()); // Hiển thị tòa nhà
        }

        // --- NÚT ĐĂNG XUẤT / THOÁT ---
        // Bạn nhớ tạo nút btnThoat hoặc btnDangXuat trong menu nhé
        private void btnThoat_Click(object sender, EventArgs e)
        {
            DialogResult dr = MessageBox.Show("Bạn có muốn đăng xuất không?", "Thông báo", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (dr == DialogResult.Yes)
            {
                // Hiện lại form đăng nhập
                FrmLogin login = new FrmLogin();
                login.Show();

                // Đóng form chính này đi
                this.Close();
            }
        }

        // --- SỰ KIỆN KÉO FORM (Gắn vào Panel Menu bên trái hoặc Thanh tiêu đề) ---
        // Nhớ quay ra Design, chọn Panel Menu -> Tia sét -> MouseDown -> Chọn hàm này
        private void panelMenu_MouseDown(object sender, MouseEventArgs e)
        {
            ReleaseCapture();
            SendMessage(this.Handle, 0x112, 0xf012, 0);
        }
    }
}