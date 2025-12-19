using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace QuanLyToaNha
{
    public partial class FrmMain : Form
    {
        // --- CẤU HÌNH GIAO DIỆN ---
        private Button currentBtn;
        private Panel leftBorderBtn; // Cái vạch màu sáng bên cạnh nút khi chọn
        private Form currentChildForm;

        // --- MÃ MÀU CHUẨN LANDMARK ---
        private struct RGBColors
        {
            public static Color color1 = Color.FromArgb(172, 126, 241);
            public static Color color2 = Color.FromArgb(249, 118, 176);
            public static Color color3 = Color.FromArgb(253, 138, 114);
            public static Color color4 = Color.FromArgb(95, 77, 221);
            public static Color color5 = Color.FromArgb(249, 88, 155);
            public static Color color6 = Color.FromArgb(24, 161, 251);
        }

        // --- DLL ĐỂ KÉO THẢ FORM KHÔNG VIỀN ---
        [DllImport("user32.DLL", EntryPoint = "ReleaseCapture")]
        private extern static void ReleaseCapture();
        [DllImport("user32.DLL", EntryPoint = "SendMessage")]
        private extern static void SendMessage(System.IntPtr hwnd, int wmsg, int wparam, int lparam);

        public FrmMain()
        {
            InitializeComponent();

            // Khởi tạo vạch kẻ trang trí
            leftBorderBtn = new Panel();
            leftBorderBtn.Size = new Size(7, 60); // Rộng 7px
            panelMenu.Controls.Add(leftBorderBtn);

            // Cấu hình Form
            this.Text = string.Empty;
            this.ControlBox = false;
            this.DoubleBuffered = true; // Giảm giật lag
            this.MaximizedBounds = Screen.FromHandle(this.Handle).WorkingArea; // Fix lỗi che Taskbar
        }

        // --- 1. LOGIC MENU ĐẸP (Active Button) ---
        private void ActivateButton(object senderBtn, Color color)
        {
            if (senderBtn != null)
            {
                DisableButton(); // Tắt nút cũ đi

                // Cấu hình nút mới được chọn
                currentBtn = (Button)senderBtn;
                currentBtn.BackColor = Color.FromArgb(37, 42, 64); // Màu nền sáng hơn chút
                currentBtn.ForeColor = color; // Đổi màu chữ theo tone
                currentBtn.TextAlign = ContentAlignment.MiddleCenter; // Đưa chữ ra giữa
                currentBtn.Padding = new Padding(0); // Bỏ padding

                // Di chuyển vạch kẻ sang nút mới
                leftBorderBtn.BackColor = color;
                leftBorderBtn.Location = new Point(0, currentBtn.Location.Y);
                leftBorderBtn.Visible = true;
                leftBorderBtn.BringToFront();

                // Đổi icon tiêu đề (nếu có)
                // iconCurrentChildForm.IconChar = currentBtn.IconChar; 
            }
        }

        private void DisableButton()
        {
            if (currentBtn != null)
            {
                // Trả về trạng thái cũ
                currentBtn.BackColor = Color.FromArgb(24, 30, 54); // Màu gốc menu
                currentBtn.ForeColor = Color.White;
                currentBtn.TextAlign = ContentAlignment.MiddleLeft;
                currentBtn.Padding = new Padding(20, 0, 0, 0); // Trả lại Padding
            }
        }

        // --- 2. LOGIC MỞ FORM CON (SPA) ---
        private void OpenChildForm(Form childForm)
        {
            if (currentChildForm != null)
            {
                currentChildForm.Close(); // Đóng form cũ
            }
            currentChildForm = childForm;

            // Cấu hình để nhúng vào Panel
            childForm.TopLevel = false;
            childForm.FormBorderStyle = FormBorderStyle.None;
            childForm.Dock = DockStyle.Fill;

            panelDesktop.Controls.Add(childForm);
            panelDesktop.Tag = childForm;
            childForm.BringToFront();
            childForm.Show();

            lblTitle.Text = childForm.Text.ToUpper(); // Cập nhật tiêu đề
        }

        // --- 3. SỰ KIỆN CLICK MENU ---

        private void btnDashboard_Click(object sender, EventArgs e)
        {
            ActivateButton(sender, RGBColors.color1);
            OpenChildForm(new FrmDashboard()); // <--- MỞ FORM DASHBOARD
            lblTitle.Text = "TỔNG QUAN HỆ THỐNG";
        }

        private void btnBuilding_Click(object sender, EventArgs e)
        {
            ActivateButton(sender, RGBColors.color2);
            OpenChildForm(new FrmBuilding()); // <--- MỞ FORM BUILDING
            lblTitle.Text = "QUẢN LÝ TÒA NHÀ";
        }

        private void btnRoom_Click(object sender, EventArgs e)
        {
            ActivateButton(sender, RGBColors.color3);
            lblTitle.Text = "QUẢN LÝ MẶT BẰNG";
        }

        private void btnCustomer_Click(object sender, EventArgs e)
        {
            ActivateButton(sender, RGBColors.color4);
            // OpenChildForm(new FrmCustomer());
            lblTitle.Text = "QUẢN LÝ KHÁCH HÀNG";
        }

        private void btnContract_Click(object sender, EventArgs e)
        {
            ActivateButton(sender, RGBColors.color5);
            lblTitle.Text = "HỢP ĐỒNG & GIAO DỊCH";
        }

        private void btnStaff_Click(object sender, EventArgs e)
        {
            ActivateButton(sender, RGBColors.color6);
            lblTitle.Text = "NHÂN SỰ & PHÂN QUYỀN";
        }

        // --- 4. CÁC NÚT HỆ THỐNG ---

        private void btnLogout_Click(object sender, EventArgs e)
        {
            // Trả về màn hình Login
            this.Close();
        }

        private void panelTitleBar_MouseDown(object sender, MouseEventArgs e)
        {
            ReleaseCapture();
            SendMessage(this.Handle, 0x112, 0xf012, 0);
        }

        // Code cho nút X, Phóng to, Thu nhỏ
        private void btnExit_Click(object sender, EventArgs e) { Application.Exit(); }

        private void btnMax_Click(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Normal)
                WindowState = FormWindowState.Maximized;
            else
                WindowState = FormWindowState.Normal;
        }

        private void btnMin_Click(object sender, EventArgs e) { WindowState = FormWindowState.Minimized; }

        private void panelDesktop_Paint(object sender, PaintEventArgs e)
        {

        }
    }
}