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
        private Panel leftBorderBtn;
        private Form currentChildForm;

        // --- MÃ MÀU CHUẨN ---
        private struct RGBColors
        {
            public static Color color1 = Color.FromArgb(172, 126, 241); // Tím
            public static Color color2 = Color.FromArgb(249, 118, 176); // Hồng
            public static Color color3 = Color.FromArgb(253, 138, 114); // Cam
            public static Color color4 = Color.FromArgb(95, 77, 221);   // Xanh đậm
            public static Color color5 = Color.FromArgb(249, 88, 155);  // Hồng đậm
            public static Color color6 = Color.FromArgb(24, 161, 251);  // Xanh sáng
        }

        // --- DLL KÉO THẢ FORM KHÔNG VIỀN ---
        [DllImport("user32.DLL", EntryPoint = "ReleaseCapture")]
        private extern static void ReleaseCapture();
        [DllImport("user32.DLL", EntryPoint = "SendMessage")]
        private extern static void SendMessage(System.IntPtr hwnd, int wmsg, int wparam, int lparam);

        public FrmMain()
        {
            InitializeComponent();

            // Khởi tạo vạch kẻ màu bên cạnh nút
            leftBorderBtn = new Panel();
            leftBorderBtn.Size = new Size(7, 60);
            panelMenu.Controls.Add(leftBorderBtn);

            // Cấu hình Form
            this.Text = string.Empty;
            this.ControlBox = false;
            this.DoubleBuffered = true;
            this.MaximizedBounds = Screen.FromHandle(this.Handle).WorkingArea;

            // QUAN TRỌNG: Mở Dashboard ngay khi khởi động
            // Giả lập bấm nút Dashboard
            if (btnDashboard != null) // Kiểm tra null để tránh lỗi Designer
            {
                btnDashboard_Click(btnDashboard, null);
            }
        }

        // --- 1. LOGIC MENU ĐẸP (Active Button) ---
        private void ActivateButton(object senderBtn, Color color)
        {
            if (senderBtn != null)
            {
                DisableButton();
                currentBtn = (Button)senderBtn;
                currentBtn.BackColor = Color.FromArgb(37, 42, 64);
                currentBtn.ForeColor = color;
                currentBtn.TextAlign = ContentAlignment.MiddleCenter;

                // Lưu lại Padding cũ nếu cần, ở đây set cứng
                // currentBtn.Padding = new Padding(0); 

                // Di chuyển vạch kẻ
                leftBorderBtn.BackColor = color;
                leftBorderBtn.Location = new Point(0, currentBtn.Location.Y);
                leftBorderBtn.Visible = true;
                leftBorderBtn.BringToFront();
            }
        }

        private void DisableButton()
        {
            if (currentBtn != null)
            {
                currentBtn.BackColor = Color.FromArgb(24, 30, 54);
                currentBtn.ForeColor = Color.White; // Hoặc màu bạc
                currentBtn.TextAlign = ContentAlignment.MiddleLeft;
                // currentBtn.Padding = new Padding(10, 0, 0, 0); 
            }
        }

        // --- 2. LOGIC MỞ FORM CON ---
        private void OpenChildForm(Form childForm)
        {
            if (currentChildForm != null)
            {
                currentChildForm.Close(); // Đóng form cũ
            }
            currentChildForm = childForm;

            childForm.TopLevel = false;
            childForm.FormBorderStyle = FormBorderStyle.None;
            childForm.Dock = DockStyle.Fill;

            panelDesktop.Controls.Add(childForm);
            panelDesktop.Tag = childForm;
            childForm.BringToFront();
            childForm.Show();
        }

        // --- 3. SỰ KIỆN CLICK MENU ---

        private void btnDashboard_Click(object sender, EventArgs e)
        {
            ActivateButton(sender, RGBColors.color1);
            OpenChildForm(new FrmDashboard());
            if (lblTitle != null) lblTitle.Text = "TỔNG QUAN HỆ THỐNG";
        }

        private void btnBuilding_Click(object sender, EventArgs e)
        {
            ActivateButton(sender, RGBColors.color2);
            OpenChildForm(new FrmBuilding());
            if (lblTitle != null) lblTitle.Text = "QUẢN LÝ TÒA NHÀ";
        }

        private void btnRoom_Click(object sender, EventArgs e)
        {
            ActivateButton(sender, RGBColors.color3);
            OpenChildForm(new FrmRoom());
            if (lblTitle != null) lblTitle.Text = "QUẢN LÝ MẶT BẰNG";
        }

        private void btnCustomer_Click(object sender, EventArgs e)
        {
            ActivateButton(sender, RGBColors.color4);
            OpenChildForm(new FrmCustomer());
            if (lblTitle != null) lblTitle.Text = "QUẢN LÝ CƯ DÂN";
        }

        private void btnContract_Click(object sender, EventArgs e)
        {
            ActivateButton(sender, RGBColors.color5);
            OpenChildForm(new FrmContract());
            if (lblTitle != null) lblTitle.Text = "HỢP ĐỒNG & GIAO DỊCH";
        }

        private void btnStaff_Click(object sender, EventArgs e)
        {
            ActivateButton(sender, RGBColors.color6);
            OpenChildForm(new FrmStaff());
            if (lblTitle != null) lblTitle.Text = "NHÂN SỰ & PHÂN QUYỀN";
        }

        // --- 4. CÁC NÚT HỆ THỐNG ---

        private void btnLogout_Click(object sender, EventArgs e)
        {
            this.Close(); // Đóng FrmMain -> Quay về Login
        }

        private void panelTitleBar_MouseDown(object sender, MouseEventArgs e)
        {
            ReleaseCapture();
            SendMessage(this.Handle, 0x112, 0xf012, 0);
        }

        private void btnExit_Click(object sender, EventArgs e) { Application.Exit(); }

        private void btnMax_Click(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Normal)
                WindowState = FormWindowState.Maximized;
            else
                WindowState = FormWindowState.Normal;
        }

        private void btnMin_Click(object sender, EventArgs e) { WindowState = FormWindowState.Minimized; }

        // --- Các hàm rác giữ nguyên ---
        private void panelDesktop_Paint(object sender, PaintEventArgs e) { }
        private void btnSystem_Click(object sender, EventArgs e) { } // Nếu chưa dùng thì để trống
    }
}