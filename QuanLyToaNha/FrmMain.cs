using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace QuanLyToaNha
{
    public partial class FrmMain : Form
    {
        // --- CONTROL GIAO DIỆN ---
        private Panel pnlMenu;
        private Panel pnlLogo;
        private Panel pnlTitleBar;
        private Panel pnlDesktop;
        private Label lblTitle;
        private Label lblLogo;
        private Button currentBtn;
        private Panel leftBorderBtn;
        private Form currentChildForm;

        // --- BIẾN HỆ THỐNG ---
        private bool isLogout = false; // Cờ kiểm tra đăng xuất
        private string currentRole;    // Lưu quyền hạn hiện tại (admin/staff)

        // --- CẤU HÌNH MÀU SẮC ---
        private struct ThemeColor
        {
            public static Color Primary = Color.FromArgb(24, 30, 54);
            public static Color Secondary = Color.FromArgb(46, 51, 73);
            public static Color Background = Color.FromArgb(245, 247, 251);
            public static Color TextColor = Color.Gainsboro;

            public static Color Purple = Color.FromArgb(172, 126, 241);
            public static Color Pink = Color.FromArgb(249, 118, 176);
            public static Color Orange = Color.FromArgb(253, 138, 114);
            public static Color Blue = Color.FromArgb(24, 161, 251);
            public static Color Green = Color.FromArgb(39, 174, 96);
            public static Color Yellow = Color.FromArgb(255, 193, 7);
        }

        // Kéo thả form không viền
        [DllImport("user32.DLL", EntryPoint = "ReleaseCapture")]
        private extern static void ReleaseCapture();
        [DllImport("user32.DLL", EntryPoint = "SendMessage")]
        private extern static void SendMessage(System.IntPtr hwnd, int wmsg, int wparam, int lparam);

        // --- CONSTRUCTOR (NHẬN ROLE TỪ ĐĂNG NHẬP) ---
        public FrmMain(string role = "staff")
        {
            InitializeComponent();
            this.Controls.Clear();

            // 1. Lưu quyền hạn
            this.currentRole = role.ToLower();

            // 2. Cấu hình Form
            this.Text = "Hệ Thống Quản Lý Tòa Nhà";
            this.Size = new Size(1280, 720);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.None;
            this.DoubleBuffered = true;
            this.MaximizedBounds = Screen.FromHandle(this.Handle).WorkingArea;

            // 3. Vẽ giao diện (Dựa trên quyền)
            BuildModernLayout();

            // 4. Mở trang đầu tiên
            OpenChildForm(new FrmDashboard());
            HighlightButton("Tổng Quan", ThemeColor.Purple);

            // 5. Xử lý sự kiện đóng Form (Logic Đăng xuất)
            this.FormClosed += (s, e) =>
            {
                if (isLogout)
                {
                    // Nếu là đăng xuất -> Mở lại form đăng nhập
                    FrmLogin login = new FrmLogin();
                    login.Show();
                }
                else
                {
                    // Nếu tắt bình thường -> Thoát hẳn chương trình
                    Application.Exit();
                }
            };
        }

        private void BuildModernLayout()
        {
            // --- A. MENU TRÁI ---
            pnlMenu = new Panel { Dock = DockStyle.Left, Width = 230, BackColor = ThemeColor.Primary };
            this.Controls.Add(pnlMenu);

            // A1. Nút Đăng Xuất (Dính đáy)
            AddLogoutButton();

            // A2. MENU CHÍNH (PHÂN QUYỀN: Add ngược từ dưới lên do Dock=Top)

            // --- Nhóm ADMIN (Chỉ Admin mới thấy) ---
            if (currentRole == "admin")
            {
                AddMenuButton("Nhân Sự", ThemeColor.Blue, (s, e) => OpenChildForm(new FrmStaff()));
                AddMenuButton("Quản Lý Tòa Nhà", ThemeColor.Blue, (s, e) => OpenChildForm(new FrmBuilding()));
            }

            // --- Nhóm CHUNG (Admin & Staff đều thấy) ---
            AddMenuButton("Yêu Cầu / Sự Cố", ThemeColor.Pink, (s, e) => OpenChildForm(new FrmRequest()));
            AddMenuButton("Hóa Đơn & Tiền", ThemeColor.Yellow, (s, e) => OpenChildForm(new FrmInvoice()));
            // Lưu ý: Nếu bạn đã update FrmInvoice nhận role, hãy sửa thành: new FrmInvoice(currentRole)

            AddMenuButton("Hợp Đồng Thuê", ThemeColor.Green, (s, e) => OpenChildForm(new FrmContract()));
            AddMenuButton("Ghi Điện Nước", ThemeColor.Blue, (s, e) => OpenChildForm(new FrmServiceUsage()));
            AddMenuButton("Dịch Vụ & Phí", ThemeColor.Purple, (s, e) => OpenChildForm(new FrmService()));
            AddMenuButton("Quản Lý Cư Dân", ThemeColor.Pink, (s, e) => OpenChildForm(new FrmCustomer()));
            AddMenuButton("Quản Lý Phòng", ThemeColor.Orange, (s, e) => OpenChildForm(new FrmRoom()));
            AddMenuButton("Tổng Quan", ThemeColor.Purple, (s, e) => OpenChildForm(new FrmDashboard()));

            // A3. LOGO (Hiển thị vai trò)
            pnlLogo = new Panel { Dock = DockStyle.Top, Height = 80 };
            lblLogo = new Label
            {
                Text = "ADMIN SYSTEM\n(" + currentRole.ToUpper() + ")", // Hiện STAFF hoặc ADMIN
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 14, FontStyle.Bold)
            };
            pnlLogo.Controls.Add(lblLogo);
            pnlMenu.Controls.Add(pnlLogo);

            leftBorderBtn = new Panel { Size = new Size(5, 50) };
            pnlMenu.Controls.Add(leftBorderBtn);

            // --- B. THANH TIÊU ĐỀ ---
            pnlTitleBar = new Panel { Dock = DockStyle.Top, Height = 30, BackColor = ThemeColor.Background };
            pnlTitleBar.MouseDown += (s, e) => { ReleaseCapture(); SendMessage(this.Handle, 0x112, 0xf012, 0); };
            this.Controls.Add(pnlTitleBar);

            lblTitle = new Label
            {
                Text = "DASHBOARD",
                AutoSize = true,
                ForeColor = Color.Gray,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Location = new Point(20, 7)
            };
            pnlTitleBar.Controls.Add(lblTitle);

            // B2. Nút điều khiển
            CreateControlButtons();

            // --- C. DESKTOP PANEL ---
            pnlDesktop = new Panel { Dock = DockStyle.Fill, BackColor = ThemeColor.Background };
            this.Controls.Add(pnlDesktop);
            pnlDesktop.BringToFront();
        }

        private void CreateControlButtons()
        {
            Font btnFont = new Font("Segoe UI", 9, FontStyle.Regular);

            // 1. Min
            Button btnMin = new Button
            {
                Text = "_",
                Dock = DockStyle.Right,
                Width = 35,
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.DimGray,
                BackColor = Color.Transparent,
                Cursor = Cursors.Hand,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                TextAlign = ContentAlignment.TopCenter
            };
            btnMin.FlatAppearance.BorderSize = 0;
            btnMin.Click += (s, e) => WindowState = FormWindowState.Minimized;
            pnlTitleBar.Controls.Add(btnMin);

            // 2. Max
            Button btnMax = new Button
            {
                Text = "◻",
                Dock = DockStyle.Right,
                Width = 35,
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.DimGray,
                BackColor = Color.Transparent,
                Cursor = Cursors.Hand,
                Font = new Font("Segoe UI", 11, FontStyle.Regular),
                TextAlign = ContentAlignment.MiddleCenter
            };
            btnMax.FlatAppearance.BorderSize = 0;
            btnMax.Click += (s, e) => {
                if (WindowState == FormWindowState.Normal) WindowState = FormWindowState.Maximized;
                else WindowState = FormWindowState.Normal;
            };
            pnlTitleBar.Controls.Add(btnMax);

            // 3. Close
            Button btnClose = new Button
            {
                Text = "✕",
                Dock = DockStyle.Right,
                Width = 35,
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.DimGray,
                BackColor = Color.Transparent,
                Cursor = Cursors.Hand,
                Font = btnFont,
                TextAlign = ContentAlignment.MiddleCenter
            };
            btnClose.FlatAppearance.BorderSize = 0;
            btnClose.Click += (s, e) => this.Close();
            btnClose.MouseEnter += (s, e) => { btnClose.BackColor = Color.FromArgb(232, 17, 35); btnClose.ForeColor = Color.White; };
            btnClose.MouseLeave += (s, e) => { btnClose.BackColor = Color.Transparent; btnClose.ForeColor = Color.DimGray; };
            pnlTitleBar.Controls.Add(btnClose);
        }

        private void AddMenuButton(string text, Color color, EventHandler onClick)
        {
            Button btn = new Button();
            btn.Dock = DockStyle.Top;
            btn.Height = 50;
            btn.Text = "  " + text;
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 0;
            btn.ForeColor = ThemeColor.TextColor;
            btn.Font = new Font("Segoe UI", 10, FontStyle.Regular);
            btn.TextAlign = ContentAlignment.MiddleLeft;
            btn.Padding = new Padding(15, 0, 0, 0);
            btn.Cursor = Cursors.Hand;

            btn.Click += onClick;
            btn.Click += (s, e) => {
                ActivateButton(s, color);
                lblTitle.Text = text.ToUpper();
            };

            pnlMenu.Controls.Add(btn);
        }

        private void AddLogoutButton()
        {
            Button btn = new Button();
            btn.Dock = DockStyle.Bottom;
            btn.Height = 50;
            btn.Text = "  Đăng Xuất";
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 0;
            btn.ForeColor = ThemeColor.Orange;
            btn.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            btn.TextAlign = ContentAlignment.MiddleLeft;
            btn.Padding = new Padding(15, 0, 0, 0);
            btn.Cursor = Cursors.Hand;

            // LOGIC ĐĂNG XUẤT
            btn.Click += (s, e) =>
            {
                if (MessageBox.Show("Bạn có chắc chắn muốn đăng xuất?", "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    isLogout = true; // Bật cờ
                    this.Close();    // Đóng form
                }
            };

            Panel line = new Panel { Dock = DockStyle.Bottom, Height = 1, BackColor = ThemeColor.Secondary };
            pnlMenu.Controls.Add(line);
            pnlMenu.Controls.Add(btn);
        }

        private void HighlightButton(string text, Color color)
        {
            foreach (Control c in pnlMenu.Controls)
            {
                if (c is Button && c.Text.Trim() == text)
                {
                    ActivateButton(c, color);
                    break;
                }
            }
        }

        private void ActivateButton(object senderBtn, Color color)
        {
            if (senderBtn != null)
            {
                DisableButton();
                currentBtn = (Button)senderBtn;
                currentBtn.BackColor = ThemeColor.Secondary;
                currentBtn.ForeColor = color;

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
                currentBtn.BackColor = ThemeColor.Primary;
                currentBtn.ForeColor = ThemeColor.TextColor;
                if (currentBtn.Text.Contains("Đăng Xuất")) currentBtn.ForeColor = ThemeColor.Orange;
            }
        }

        private void OpenChildForm(Form childForm)
        {
            if (currentChildForm != null) currentChildForm.Close();
            currentChildForm = childForm;
            childForm.TopLevel = false;
            childForm.FormBorderStyle = FormBorderStyle.None;
            childForm.Dock = DockStyle.Fill;
            pnlDesktop.Controls.Add(childForm);
            pnlDesktop.Tag = childForm;
            childForm.BringToFront();
            childForm.Show();
        }
    }
}