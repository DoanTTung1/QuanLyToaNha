using System;
using System.Drawing;
using System.Windows.Forms;

namespace QuanLyToaNha
{
    public partial class FrmDashboard : Form
    {
        public FrmDashboard()
        {
            InitializeComponent();
            DesignDashboard();
        }

        private void DesignDashboard()
        {
            this.BackColor = Color.FromArgb(240, 243, 250);
            this.Padding = new Padding(30); // Cách lề màn hình 30px

            // Tiêu đề
            Label lblHeader = new Label();
            lblHeader.Text = "Dashboard - Cập nhật lúc " + DateTime.Now.ToString("HH:mm");
            lblHeader.Font = new Font("Segoe UI", 12, FontStyle.Bold);
            lblHeader.ForeColor = Color.DarkGray;
            lblHeader.AutoSize = true;
            lblHeader.Location = new Point(30, 10);
            this.Controls.Add(lblHeader);

            // TÍNH TOÁN VỊ TRÍ ĐỂ CÁC Ô RỘNG HƠN
            // Card cũ: 260x140 -> Card mới: 300x160 (To bự)

            // Card 1: Tòa nhà
            CreateCard("TỔNG TÒA NHÀ", "15", "🏢", Color.FromArgb(172, 126, 241), 30, 50);

            // Card 2: Khách hàng (Cách Card 1 khoảng 330px)
            CreateCard("KHÁCH HÀNG", "128", "👥", Color.FromArgb(249, 118, 176), 360, 50);

            // Card 3: Doanh thu (Cách Card 2 khoảng 330px)
            CreateCard("DOANH THU", "5.2 Tỷ", "💰", Color.FromArgb(253, 138, 114), 690, 50);

            // Card 4: Hợp đồng (Xuống dòng nếu màn hình bé, hoặc xếp tiếp nếu màn to)
            // Ở đây mình xếp tiếp cho màn hình rộng (1300px)
            CreateCard("SẮP HẾT HẠN", "03", "⚠️", Color.FromArgb(24, 161, 251), 1020, 50);
        }

        private void CreateCard(string title, string value, string icon, Color bgColor, int x, int y)
        {
            Panel pnl = new Panel();
            pnl.Size = new Size(300, 160); // <--- Tăng kích thước ô ở đây
            pnl.Location = new Point(x, y);
            pnl.BackColor = bgColor;

            // Icon to
            Label lblIcon = new Label();
            lblIcon.Text = icon;
            lblIcon.Font = new Font("Segoe UI", 45, FontStyle.Regular); // Icon to hơn
            lblIcon.ForeColor = Color.FromArgb(60, 255, 255, 255);
            lblIcon.AutoSize = true;
            lblIcon.Location = new Point(190, 20);
            lblIcon.BackColor = Color.Transparent;
            pnl.Controls.Add(lblIcon);

            // Giá trị
            Label lblValue = new Label();
            lblValue.Text = value;
            lblValue.ForeColor = Color.White;
            lblValue.Font = new Font("Segoe UI", 26, FontStyle.Bold); // Số to hơn
            lblValue.AutoSize = true;
            lblValue.Location = new Point(20, 55);
            pnl.Controls.Add(lblValue);

            // Tiêu đề
            Label lblTitle = new Label();
            lblTitle.Text = title;
            lblTitle.ForeColor = Color.WhiteSmoke;
            lblTitle.Font = new Font("Segoe UI", 11, FontStyle.Bold);
            lblTitle.AutoSize = true;
            lblTitle.Location = new Point(20, 20);
            pnl.Controls.Add(lblTitle);

            // Chữ nhỏ dưới đáy
            Label lblDesc = new Label();
            lblDesc.Text = "Xem chi tiết ->";
            lblDesc.ForeColor = Color.White;
            lblDesc.Cursor = Cursors.Hand;
            lblDesc.Font = new Font("Segoe UI", 9, FontStyle.Underline);
            lblDesc.AutoSize = true;
            lblDesc.Location = new Point(22, 120);
            pnl.Controls.Add(lblDesc);

            this.Controls.Add(pnl);
            lblIcon.BringToFront();
        }
    }
}