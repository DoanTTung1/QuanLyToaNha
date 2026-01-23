using System;
using System.Drawing;
using System.Windows.Forms;
using System.Data;
using MySql.Data.MySqlClient; // QUAN TRỌNG: Thư viện MySQL

namespace QuanLyToaNha
{
    public partial class FrmPortal : Form
    {
        private DataGridView dgvBill; // Khai báo biến toàn cục để nạp dữ liệu

        public FrmPortal(string tenCuDan)
        {
            InitializeComponent();
            DesignPortal(tenCuDan);

            // Tải dữ liệu hóa đơn/hợp đồng của cư dân này
            LoadMyData(tenCuDan);
        }

        // --- HÀM LOAD DỮ LIỆU TỪ DB ---
        private void LoadMyData(string name)
        {
            try
            {
                // Logic: Tìm trong bảng Hợp Đồng (contracts) xem có khách hàng nào tên giống người đang đăng nhập không
                // Kết nối 3 bảng: contracts (Hợp đồng) -> customers (Khách) -> rooms (Phòng)
                string sql = @"
                    SELECT 
                        c.id AS 'Mã HĐ',
                        r.room_number AS 'Phòng',
                        r.price AS 'Giá Thuê',
                        DATE_FORMAT(c.start_date, '%d/%m/%Y') AS 'Ngày Bắt Đầu',
                        c.status AS 'Trạng Thái'
                    FROM contracts c
                    JOIN customers cus ON c.customer_id = cus.id
                    JOIN rooms r ON c.room_id = r.id
                    WHERE cus.full_name LIKE @name";

                using (MySqlConnection conn = DatabaseHelper.GetConnection())
                {
                    if (conn.State == ConnectionState.Closed) conn.Open();
                    MySqlCommand cmd = new MySqlCommand(sql, conn);
                    // Dùng LIKE để tìm kiếm gần đúng tên (cho linh hoạt)
                    cmd.Parameters.AddWithValue("@name", "%" + name + "%");

                    MySqlDataAdapter dap = new MySqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    dap.Fill(dt);

                    // Nếu tìm thấy dữ liệu thì đổ vào bảng, không thì thôi
                    if (dt.Rows.Count > 0)
                    {
                        dgvBill.DataSource = dt;
                        // Format lại cột giá tiền
                        dgvBill.Columns["Giá Thuê"].DefaultCellStyle.Format = "N0";
                    }
                }
            }
            catch (Exception ex)
            {
                // Lỗi này thường do chưa có dữ liệu khớp tên, không cần báo lỗi quá gắt
                // MessageBox.Show("Lỗi tải dữ liệu cá nhân: " + ex.Message);
            }
        }

        private void DesignPortal(string name)
        {
            // 1. Cấu hình Form chính
            this.Size = new Size(1100, 700);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.MidnightBlue;
            this.Text = "CỔNG THÔNG TIN CƯ DÂN";
            this.WindowState = FormWindowState.Maximized;

            // 2. HEADER
            Panel pnlTop = new Panel();
            pnlTop.Dock = DockStyle.Top;
            pnlTop.Height = 80;
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
            btnLogout.ForeColor = Color.MidnightBlue;
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

            // --- CỘT TRÁI: THÔNG BÁO (Giữ tĩnh vì DB chưa có bảng thông báo) ---
            GroupBox grpNoti = new GroupBox();
            grpNoti.Text = "📢 THÔNG BÁO TỪ BAN QUẢN LÝ";
            grpNoti.Dock = DockStyle.Fill;
            grpNoti.Font = new Font("Segoe UI", 12, FontStyle.Bold);
            grpNoti.ForeColor = Color.White;

            TextBox txtNoti = new TextBox();
            txtNoti.Multiline = true;
            txtNoti.Dock = DockStyle.Fill;
            txtNoti.ReadOnly = true;
            txtNoti.BackColor = Color.White;
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

            // --- CỘT PHẢI: HÓA ĐƠN / HỢP ĐỒNG ---
            GroupBox grpBill = new GroupBox();
            grpBill.Text = "💰 DỊCH VỤ & HỢP ĐỒNG CỦA TÔI";
            grpBill.Dock = DockStyle.Fill;
            grpBill.Font = new Font("Segoe UI", 12, FontStyle.Bold);
            grpBill.ForeColor = Color.White;

            dgvBill = new DataGridView();
            dgvBill.Dock = DockStyle.Fill;
            dgvBill.BackgroundColor = Color.White;
            dgvBill.BorderStyle = BorderStyle.None;
            dgvBill.RowHeadersVisible = false;
            dgvBill.AllowUserToAddRows = false;
            dgvBill.ReadOnly = true;
            dgvBill.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvBill.SelectionMode = DataGridViewSelectionMode.FullRowSelect;

            // Style
            dgvBill.EnableHeadersVisualStyles = false;
            dgvBill.ColumnHeadersHeight = 40;
            dgvBill.ColumnHeadersDefaultCellStyle.BackColor = Color.MidnightBlue;
            dgvBill.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvBill.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            dgvBill.DefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Regular);
            dgvBill.DefaultCellStyle.ForeColor = Color.Black;
            dgvBill.DefaultCellStyle.SelectionBackColor = Color.CornflowerBlue;
            dgvBill.RowTemplate.Height = 40;

            // Mặc định tạo cột rỗng để giữ giao diện đẹp nếu chưa load được data
            dgvBill.Columns.Add("Info", "Thông tin");
            dgvBill.Rows.Add("Đang tải dữ liệu từ máy chủ...");

            grpBill.Controls.Add(dgvBill);
            tableLayout.Controls.Add(grpBill, 1, 0);
        }
    }
}