using System;
using System.Drawing;
using System.Windows.Forms;
using System.Data;

namespace QuanLyToaNha
{
    public partial class FrmDashboard : Form
    {
        private FlowLayoutPanel flowPanel;

        public FrmDashboard()
        {
            InitializeComponent();
            this.Text = "Dashboard Admin";
            this.Size = new Size(1000, 600); // Kích thước mặc định to hơn chút
            this.StartPosition = FormStartPosition.CenterScreen;
            this.DoubleBuffered = true;
            this.BackColor = Color.FromArgb(245, 247, 251);

            DesignModernDashboard();
        }

        private void DesignModernDashboard()
        {
            // Xóa các control cũ nếu có (để tránh bị chồng lấn khi tải lại)
            this.Controls.Clear();

            // 1. HEADER
            Panel pnlHeader = new Panel { Dock = DockStyle.Top, Height = 60, BackColor = Color.Transparent };
            Label lblTitle = new Label { Text = "TỔNG QUAN HỆ THỐNG", Font = new Font("Segoe UI", 16, FontStyle.Bold), ForeColor = Color.DimGray, AutoSize = true, Location = new Point(20, 15) };
            pnlHeader.Controls.Add(lblTitle);
            this.Controls.Add(pnlHeader);

            // 2. CONTAINER
            flowPanel = new FlowLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(20), AutoScroll = true };
            this.Controls.Add(flowPanel);
            flowPanel.BringToFront();

            // 3. LẤY SỐ LIỆU
            long countBuilding = GetCount("SELECT COUNT(*) FROM buildings");
            long countCustomer = GetCount("SELECT COUNT(*) FROM users WHERE role = 'resident'");
            decimal totalDeposit = GetSum("SELECT SUM(deposit) FROM contracts");
            long countExpiring = GetCount("SELECT COUNT(*) FROM contracts WHERE end_date <= DATE_ADD(NOW(), INTERVAL 30 DAY)");
            string strMoney = totalDeposit >= 1000000000 ? (totalDeposit / 1000000000).ToString("0.##") + " Tỷ" : totalDeposit.ToString("N0") + " đ";

            // 4. TẠO CARD VÀ GẮN SỰ KIỆN CLICK (QUAN TRỌNG)

            // Card Tòa Nhà -> Mở FrmBuilding
            flowPanel.Controls.Add(CreateModernCard("Tòa Nhà", countBuilding.ToString("N0"), "🏢", Color.FromArgb(113, 96, 232), Color.FromArgb(240, 238, 255),
                () => { new FrmBuilding().ShowDialog(); }));

            // Card Cư Dân -> Mở FrmCustomer
            flowPanel.Controls.Add(CreateModernCard("Cư Dân", countCustomer.ToString("N0"), "👥", Color.FromArgb(235, 87, 87), Color.FromArgb(255, 235, 235),
                () => { new FrmCustomer().ShowDialog(); }));

            // Card Tiền Cọc -> Mở FrmContract (Hợp đồng)
            flowPanel.Controls.Add(CreateModernCard("Tiền Cọc", strMoney, "💰", Color.FromArgb(39, 174, 96), Color.FromArgb(232, 248, 245),
                () => { new FrmContract().ShowDialog(); }));

            // Card Hết Hạn -> Mở FrmContract
            flowPanel.Controls.Add(CreateModernCard("Sắp Hết Hạn", countExpiring.ToString("00"), "⚠️", Color.FromArgb(242, 153, 74), Color.FromArgb(255, 245, 230),
                () => { new FrmContract().ShowDialog(); }));
        }

        // --- HÀM TẠO CARD (Đã thêm tham số Action onClick) ---
        private Panel CreateModernCard(string title, string value, string icon, Color accentColor, Color iconBgColor, Action onClick)
        {
            Panel card = new Panel { Size = new Size(280, 140), Margin = new Padding(0, 0, 20, 20), BackColor = Color.White, Cursor = Cursors.Hand };

            // Trang trí
            Panel borderLeft = new Panel { Dock = DockStyle.Left, Width = 5, BackColor = accentColor };
            card.Controls.Add(borderLeft);

            Label lblIcon = new Label { Text = icon, Font = new Font("Segoe UI Emoji", 24), Size = new Size(60, 60), TextAlign = ContentAlignment.MiddleCenter, Location = new Point(200, 20), BackColor = iconBgColor, ForeColor = accentColor };
            card.Controls.Add(lblIcon);

            Label lblTitle = new Label { Text = title.ToUpper(), Font = new Font("Segoe UI", 9, FontStyle.Bold), ForeColor = Color.DarkGray, AutoSize = true, Location = new Point(20, 25) };
            card.Controls.Add(lblTitle);

            Label lblValue = new Label { Text = value, Font = new Font("Segoe UI", 22, FontStyle.Bold), ForeColor = Color.FromArgb(50, 50, 50), AutoSize = true, Location = new Point(15, 50) };
            card.Controls.Add(lblValue);

            Label lblFooter = new Label { Text = "Xem chi tiết →", Font = new Font("Segoe UI", 9, FontStyle.Underline), ForeColor = accentColor, AutoSize = true, Location = new Point(20, 110), Cursor = Cursors.Hand };
            card.Controls.Add(lblFooter);

            // --- GẮN SỰ KIỆN CLICK CHO CÁC THÀNH PHẦN ---
            // Để bấm vào đâu trong thẻ cũng mở được Form
            card.Click += (s, e) => onClick?.Invoke();
            lblTitle.Click += (s, e) => onClick?.Invoke();
            lblValue.Click += (s, e) => onClick?.Invoke();
            lblFooter.Click += (s, e) => onClick?.Invoke();
            lblIcon.Click += (s, e) => onClick?.Invoke();

            // Hiệu ứng Hover
            card.MouseEnter += (s, e) => card.BackColor = Color.FromArgb(250, 250, 250);
            card.MouseLeave += (s, e) => card.BackColor = Color.White;

            return card;
        }

        private long GetCount(string sql)
        {
            try
            {
                DataTable dt = DatabaseHelper.GetData(sql);
                if (dt != null && dt.Rows.Count > 0 && dt.Rows[0][0] != DBNull.Value) return Convert.ToInt64(dt.Rows[0][0]);
            }
            catch { }
            return 0;
        }

        private decimal GetSum(string sql)
        {
            try
            {
                DataTable dt = DatabaseHelper.GetData(sql);
                if (dt != null && dt.Rows.Count > 0 && dt.Rows[0][0] != DBNull.Value) return Convert.ToDecimal(dt.Rows[0][0]);
            }
            catch { }
            return 0;
        }
    }
}