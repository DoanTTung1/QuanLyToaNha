using System;
using System.Drawing;
using System.Windows.Forms;

namespace QuanLyToaNha
{
    public partial class FrmPortal : Form
    {
        public FrmPortal(string tenCuDan)
        {
            InitializeComponent();
            DesignPortal(tenCuDan);
        }

        private void DesignPortal(string name)
        {
            // 1. Cấu hình Form chính
            this.Size = new Size(1100, 700);
            this.StartPosition = FormStartPosition.CenterScreen;

            // --- THAY ĐỔI Ở ĐÂY: MÀU MIDNIGHT BLUE ---
            this.BackColor = Color.MidnightBlue;
            this.Text = "CỔNG THÔNG TIN CƯ DÂN";
            this.WindowState = FormWindowState.Maximized;

            // 2. HEADER
            Panel pnlTop = new Panel();
            pnlTop.Dock = DockStyle.Top;
            pnlTop.Height = 80;
            // Header giữ màu xanh sáng hoặc chuyển sang màu tối hơn tùy bạn
            // Ở đây mình để màu Brand cũ để tạo điểm nhấn
            pnlTop.BackColor = Color.FromArgb(24, 161, 251);
            this.Controls.Add(pnlTop);

            // Label Chào mừng
            Label lblWelcome = new Label();
            lblWelcome.Text = "Xin chào cư dân: " + name.ToUpper();
            lblWelcome.ForeColor = Color.White;
            lblWelcome.Font = new Font("Segoe UI", 16, FontStyle.Bold);
            lblWelcome.AutoSize = true;
            lblWelcome.Location = new Point(30, 25);
            pnlTop.Controls.Add(lblWelcome);

            // Nút Đăng xuất
            Button btnLogout = new Button();
            btnLogout.Text = "Đăng xuất";
            btnLogout.BackColor = Color.White;
            btnLogout.ForeColor = Color.MidnightBlue; // Chữ màu xanh đậm cho hợp tông
            btnLogout.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            btnLogout.FlatStyle = FlatStyle.Flat;
            btnLogout.Size = new Size(120, 40);
            btnLogout.Location = new Point(this.ClientSize.Width - 150, 20);
            btnLogout.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnLogout.Click += (s, e) => { this.Close(); };
            pnlTop.Controls.Add(btnLogout);


            // 3. LAYOUT CHIA ĐÔI
            TableLayoutPanel tableLayout = new TableLayoutPanel();
            tableLayout.Dock = DockStyle.Fill;
            tableLayout.ColumnCount = 2;
            tableLayout.RowCount = 1;
            tableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tableLayout.Padding = new Padding(20);
            this.Controls.Add(tableLayout);
            tableLayout.BringToFront();


            // --- CỘT TRÁI: THÔNG BÁO ---
            GroupBox grpNoti = new GroupBox();
            grpNoti.Text = "📢 THÔNG BÁO TỪ BAN QUẢN LÝ";
            grpNoti.Dock = DockStyle.Fill;
            grpNoti.Font = new Font("Segoe UI", 12, FontStyle.Bold);

            // QUAN TRỌNG: Chữ tiêu đề màu TRẮNG để nổi trên nền MidnightBlue
            grpNoti.ForeColor = Color.White;

            // Ô nội dung
            TextBox txtNoti = new TextBox();
            txtNoti.Multiline = true;
            txtNoti.Dock = DockStyle.Fill;
            txtNoti.ReadOnly = true;
            txtNoti.BackColor = Color.White; // Nền trắng chữ đen cho dễ đọc
            txtNoti.ForeColor = Color.Black;
            txtNoti.ScrollBars = ScrollBars.Vertical;
            txtNoti.BorderStyle = BorderStyle.None;
            txtNoti.Font = new Font("Segoe UI", 11, FontStyle.Regular);
            txtNoti.Margin = new Padding(10);
            txtNoti.Text = "📅 19/12: Bảo trì thang máy Tòa A từ 13h-15h.\r\n\r\n" +
                           "📅 24/12: Tổ chức tiệc Giáng Sinh tại sảnh chính.\r\n\r\n" +
                           "⚠️ QUAN TRỌNG: Quý cư dân vui lòng đóng phí quản lý trước ngày 05 hàng tháng.\r\n\r\n" +
                           "--------------------------\r\n" +
                           "📞 Hotline: 1900 1000";

            grpNoti.Controls.Add(txtNoti);
            tableLayout.Controls.Add(grpNoti, 0, 0);


            // --- CỘT PHẢI: HÓA ĐƠN ---
            GroupBox grpBill = new GroupBox();
            grpBill.Text = "💰 HÓA ĐƠN CỦA TÔI";
            grpBill.Dock = DockStyle.Fill;
            grpBill.Font = new Font("Segoe UI", 12, FontStyle.Bold);
            grpBill.ForeColor = Color.White; // Tiêu đề màu trắng

            // Bảng hóa đơn
            DataGridView dgvBill = new DataGridView();
            dgvBill.Dock = DockStyle.Fill;
            dgvBill.BackgroundColor = Color.White; // Nền bảng màu trắng
            dgvBill.BorderStyle = BorderStyle.None;
            dgvBill.RowHeadersVisible = false;
            dgvBill.AllowUserToAddRows = false;
            dgvBill.ReadOnly = true;
            dgvBill.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvBill.SelectionMode = DataGridViewSelectionMode.FullRowSelect;

            // Style Header
            dgvBill.EnableHeadersVisualStyles = false;
            dgvBill.ColumnHeadersHeight = 40;
            dgvBill.ColumnHeadersDefaultCellStyle.BackColor = Color.MidnightBlue; // Header bảng trùng màu nền form
            dgvBill.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvBill.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);

            // Style Nội dung
            dgvBill.DefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Regular);
            dgvBill.DefaultCellStyle.ForeColor = Color.Black;
            dgvBill.DefaultCellStyle.SelectionBackColor = Color.CornflowerBlue; // Màu khi chọn dòng
            dgvBill.RowTemplate.Height = 40;

            dgvBill.Columns.Add("Thang", "THÁNG");
            dgvBill.Columns.Add("DichVu", "LOẠI DỊCH VỤ");
            dgvBill.Columns.Add("Sotien", "SỐ TIỀN");
            dgvBill.Columns.Add("TrangThai", "TRẠNG THÁI");

            dgvBill.Rows.Add("12/2024", "Phí Quản Lý", "1.500.000đ", "Chưa thanh toán");
            dgvBill.Rows.Add("12/2024", "Điện/Nước", "850.000đ", "Chưa thanh toán");
            dgvBill.Rows.Add("11/2024", "Phí Quản Lý", "1.500.000đ", "✅ Đã thanh toán");

            grpBill.Controls.Add(dgvBill);
            tableLayout.Controls.Add(grpBill, 1, 0);
        }
    }
}