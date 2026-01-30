using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting; // Thư viện biểu đồ

namespace QuanLyToaNha
{
    public partial class FrmDashboard : Form
    {
        // Các Panel hiển thị số liệu
        private Panel pnlStats;
        private Label lblTotalRooms, lblOccupied, lblAvailable, lblPendingRequests;
        private Chart chartRevenue;

        public FrmDashboard()
        {
            InitializeComponent();
            this.Controls.Clear();
            this.BackColor = Color.FromArgb(245, 247, 251); // Màu nền sáng nhẹ

            DesignDashboard();
            LoadStatistics();
            LoadRevenueChart();
        }

        // --- 1. THIẾT KẾ GIAO DIỆN (Code tay cho đẹp) ---
        private void DesignDashboard()
        {
            // Title
            Label lblHeader = new Label
            {
                Text = "TỔNG QUAN HOẠT ĐỘNG",
                Location = new Point(20, 20),
                AutoSize = true,
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = Color.FromArgb(24, 30, 54)
            };
            this.Controls.Add(lblHeader);

            // Container cho 4 thẻ thống kê
            pnlStats = new Panel { Location = new Point(20, 60), Size = new Size(1200, 150), Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right };
            this.Controls.Add(pnlStats);

            // Tạo 4 thẻ (Card)
            CreateStatCard(0, "Tổng Số Phòng", out lblTotalRooms, Color.FromArgb(24, 161, 251)); // Xanh dương
            CreateStatCard(300, "Phòng Đang Thuê", out lblOccupied, Color.FromArgb(39, 174, 96)); // Xanh lá
            CreateStatCard(600, "Phòng Trống", out lblAvailable, Color.FromArgb(255, 193, 7)); // Vàng
            CreateStatCard(900, "Sự Cố Chờ Xử Lý", out lblPendingRequests, Color.FromArgb(231, 76, 60)); // Đỏ

            // Biểu đồ Doanh Thu
            Label lblChartTitle = new Label
            {
                Text = "BIỂU ĐỒ DOANH THU 6 THÁNG GẦN NHẤT",
                Location = new Point(20, 230),
                AutoSize = true,
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = Color.DimGray
            };
            this.Controls.Add(lblChartTitle);

            chartRevenue = new Chart();
            chartRevenue.Location = new Point(20, 260);
            chartRevenue.Size = new Size(800, 400);
            chartRevenue.BackColor = Color.White;

            // Cấu hình Chart Area
            ChartArea ca = new ChartArea();
            ca.AxisX.MajorGrid.LineColor = Color.LightGray;
            ca.AxisY.MajorGrid.LineColor = Color.LightGray;
            ca.AxisX.LabelStyle.Font = new Font("Segoe UI", 9);
            ca.AxisY.LabelStyle.Format = "{0:N0}"; // Định dạng số tiền
            chartRevenue.ChartAreas.Add(ca);

            // Cấu hình Series
            Series series = new Series("DoanhThu");
            series.ChartType = SeriesChartType.Column; // Biểu đồ cột
            series.Color = Color.FromArgb(108, 99, 255); // Màu tím đẹp
            series.IsValueShownAsLabel = true; // Hiện số tiền trên cột
            series.LabelFormat = "{0:N0}";
            chartRevenue.Series.Add(series);

            this.Controls.Add(chartRevenue);

            // Nút Refresh
            Button btnRefresh = new Button
            {
                Text = "Cập Nhật Dữ Liệu",
                Location = new Point(850, 260),
                Size = new Size(150, 40),
                BackColor = Color.FromArgb(24, 30, 54),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnRefresh.Click += (s, e) => { LoadStatistics(); LoadRevenueChart(); };
            this.Controls.Add(btnRefresh);
        }

        private void CreateStatCard(int x, string title, out Label lblValue, Color color)
        {
            Panel pnl = new Panel
            {
                Location = new Point(x, 0),
                Size = new Size(280, 140),
                BackColor = Color.White
            };
            // Bo viền màu bên trái
            Panel pnlBorder = new Panel { Dock = DockStyle.Left, Width = 8, BackColor = color };
            pnl.Controls.Add(pnlBorder);

            Label lblTitle = new Label
            {
                Text = title,
                Location = new Point(20, 20),
                AutoSize = true,
                Font = new Font("Segoe UI", 11, FontStyle.Regular),
                ForeColor = Color.Gray
            };
            pnl.Controls.Add(lblTitle);

            lblValue = new Label
            {
                Text = "0",
                Location = new Point(20, 50),
                AutoSize = true,
                Font = new Font("Segoe UI", 24, FontStyle.Bold),
                ForeColor = Color.Black
            };
            pnl.Controls.Add(lblValue);

            pnlStats.Controls.Add(pnl);
        }

        // --- 2. LOGIC LOAD SỐ LIỆU ---
        private void LoadStatistics()
        {
            try
            {
                // 1. Tổng số phòng
                DataTable dtTotal = DatabaseHelper.GetData("SELECT COUNT(*) FROM rooms");
                lblTotalRooms.Text = dtTotal.Rows[0][0].ToString();

                // 2. Phòng đang thuê (Occupied)
                DataTable dtOccupied = DatabaseHelper.GetData("SELECT COUNT(*) FROM rooms WHERE status='Occupied'");
                lblOccupied.Text = dtOccupied.Rows[0][0].ToString();

                // 3. Phòng trống (Available)
                DataTable dtAvailable = DatabaseHelper.GetData("SELECT COUNT(*) FROM rooms WHERE status='Available'");
                lblAvailable.Text = dtAvailable.Rows[0][0].ToString();

                // 4. Sự cố chưa xử lý (Pending)
                DataTable dtPending = DatabaseHelper.GetData("SELECT COUNT(*) FROM requests WHERE status='Pending'");
                lblPendingRequests.Text = dtPending.Rows[0][0].ToString();
            }
            catch (Exception ex) { MessageBox.Show("Lỗi thống kê: " + ex.Message); }
        }

        // --- 3. VẼ BIỂU ĐỒ DOANH THU ---
        private void LoadRevenueChart()
        {
            try
            {
                chartRevenue.Series["DoanhThu"].Points.Clear();

                // Query lấy tổng tiền theo tháng (6 tháng gần nhất)
                // Lưu ý: Query này gom nhóm theo Tháng/Năm
                string sql = @"
                    SELECT month, year, SUM(total_amount) as Revenue 
                    FROM invoices 
                    GROUP BY year, month 
                    ORDER BY year DESC, month DESC 
                    LIMIT 6";

                DataTable dt = DatabaseHelper.GetData(sql);

                // Dữ liệu lấy ra đang là từ mới nhất -> cũ nhất.
                // Khi vẽ biểu đồ cần đảo ngược lại (Cũ -> Mới) để hiển thị từ trái sang phải
                for (int i = dt.Rows.Count - 1; i >= 0; i--)
                {
                    DataRow r = dt.Rows[i];
                    string label = r["month"] + "/" + r["year"];
                    decimal value = r["Revenue"] != DBNull.Value ? Convert.ToDecimal(r["Revenue"]) : 0;

                    chartRevenue.Series["DoanhThu"].Points.AddXY(label, value);
                }
            }
            catch (Exception ex) { MessageBox.Show("Lỗi vẽ biểu đồ: " + ex.Message); }
        }
    }
}