using System;
using System.Drawing;
using System.Windows.Forms;
using System.Data;
using MySql.Data.MySqlClient;

namespace QuanLyToaNha
{
    public partial class FrmPortal : Form
    {
        // --- BIẾN LOGIC ---
        private string _currentName;
        private int _currentUserId = -1;

        // --- MÀU SẮC CHỦ ĐẠO (THEME) ---
        private Color colorPrimary = Color.FromArgb(24, 30, 54);      // Xanh đen đậm
        private Color colorActive = Color.FromArgb(0, 126, 249);      // Xanh dương sáng
        private Color colorBg = Color.FromArgb(245, 245, 250);        // Xám rất nhạt
        private Color colorSuccess = Color.FromArgb(46, 204, 113);    // Xanh lá
        private Color colorDanger = Color.FromArgb(231, 76, 60);      // Đỏ

        // --- CONTROLS ---
        private TabControl tabMain;
        private DataGridView dgvInvoices, dgvRequests, dgvContracts;
        private TextBox txtReqTitle, txtReqContent, txtOldPass, txtNewPass, txtConfirmPass;

        // Labels hiển thị thông tin
        private Label lblValName, lblValPhone, lblValRoom, lblValPrice, lblValDate, lblValDebt;

        public FrmPortal(string tenCuDan)
        {
            InitializeComponent();
            _currentName = tenCuDan;

            // 1. Cấu hình Form
            this.Size = new Size(1280, 800);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.WindowState = FormWindowState.Maximized;
            this.Text = "ELITE HOMES - CỔNG CƯ DÂN ĐIỆN TỬ";
            this.BackColor = colorBg;

            // 2. Lấy ID người dùng
            GetUserIdByName();

            // 3. Dựng giao diện
            InitModernUI();

            // 4. Đổ dữ liệu
            LoadAllData();
        }

        // =================================================================================
        // PHẦN 1: LOGIC DỮ LIỆU
        // =================================================================================
        private void GetUserIdByName()
        {
            try
            {
                using (var conn = DatabaseHelper.GetConnection())
                {
                    if (conn.State == ConnectionState.Closed) conn.Open();
                    MySqlCommand cmd = new MySqlCommand("SELECT id FROM users WHERE full_name LIKE @name LIMIT 1", conn);
                    cmd.Parameters.AddWithValue("@name", "%" + _currentName + "%");
                    object result = cmd.ExecuteScalar();
                    if (result != null) _currentUserId = Convert.ToInt32(result);
                }
            }
            catch { }
        }

        private void LoadAllData()
        {
            LoadProfileData();
            LoadInvoiceData();
            LoadRequestData();
            LoadContractData();
        }

        private void LoadProfileData()
        {
            if (_currentUserId == -1) return;
            try
            {
                // 1. Thông tin cá nhân
                DataTable dtUser = DatabaseHelper.GetData($"SELECT full_name, phone FROM users WHERE id={_currentUserId}");
                if (dtUser.Rows.Count > 0)
                {
                    lblValName.Text = dtUser.Rows[0]["full_name"].ToString();
                    lblValPhone.Text = dtUser.Rows[0]["phone"].ToString();
                }

                // 2. Thông tin phòng đang ở (Hợp đồng Active)
                string sql = @"SELECT r.room_number, c.rental_price, c.end_date 
                               FROM contracts c JOIN rooms r ON c.room_id = r.id 
                               WHERE c.customer_id = " + _currentUserId + " AND c.status = 'Active' ORDER BY c.end_date DESC LIMIT 1";
                DataTable dtCon = DatabaseHelper.GetData(sql);

                if (dtCon.Rows.Count > 0)
                {
                    DataRow row = dtCon.Rows[0];
                    lblValRoom.Text = "Phòng " + row["room_number"].ToString();

                    if (row["rental_price"] != DBNull.Value)
                        lblValPrice.Text = Convert.ToDecimal(row["rental_price"]).ToString("N0") + " đ";
                    else
                        lblValPrice.Text = "0 đ";

                    if (row["end_date"] != DBNull.Value)
                        lblValDate.Text = Convert.ToDateTime(row["end_date"]).ToString("dd/MM/yyyy");
                    else
                        lblValDate.Text = "Không thời hạn";
                }
                else
                {
                    lblValRoom.Text = "Chưa thuê"; lblValPrice.Text = "0 đ"; lblValDate.Text = "--/--/----";
                }
            }
            catch { }
        }

        private void LoadContractData()
        {
            if (_currentUserId == -1) return;
            try
            {
                // Truy vấn theo customer_id (An toàn)
                string sql = @"SELECT c.id AS 'Mã HĐ', 
                               r.room_number AS 'Phòng', 
                               c.rental_price AS 'Giá Thuê', 
                               c.start_date AS 'Ngày Thuê', 
                               c.end_date AS 'Ngày Hết Hạn',
                               c.status AS 'Trạng Thái'
                               FROM contracts c 
                               JOIN rooms r ON c.room_id = r.id
                               WHERE c.customer_id = " + _currentUserId + " ORDER BY c.start_date DESC";

                DataTable dt = DatabaseHelper.GetData(sql);

                dt.Columns.Add("Trạng Thái (VN)", typeof(string));
                foreach (DataRow row in dt.Rows)
                {
                    string status = row["Trạng Thái"].ToString();
                    if (status == "Active") row["Trạng Thái (VN)"] = "Đang Hiệu Lực";
                    else if (status == "Terminated") row["Trạng Thái (VN)"] = "Đã Kết Thúc";
                    else row["Trạng Thái (VN)"] = status;
                }

                dgvContracts.DataSource = dt;

                if (dgvContracts.Columns["Mã HĐ"] != null) dgvContracts.Columns["Mã HĐ"].Visible = false;
                if (dgvContracts.Columns["Trạng Thái"] != null) dgvContracts.Columns["Trạng Thái"].Visible = false;

                if (dgvContracts.Columns["Giá Thuê"] != null) dgvContracts.Columns["Giá Thuê"].DefaultCellStyle.Format = "N0";
                if (dgvContracts.Columns["Ngày Thuê"] != null) dgvContracts.Columns["Ngày Thuê"].DefaultCellStyle.Format = "dd/MM/yyyy";
                if (dgvContracts.Columns["Ngày Hết Hạn"] != null) dgvContracts.Columns["Ngày Hết Hạn"].DefaultCellStyle.Format = "dd/MM/yyyy";
            }
            catch (Exception ex) { MessageBox.Show("Lỗi tải hợp đồng: " + ex.Message); }
        }

        private void LoadInvoiceData()
        {
            if (_currentUserId == -1) return;
            try
            {
                string sql = @"SELECT i.id, CONCAT(i.month, '/', i.year) AS 'Tháng', 
                               i.total_amount AS 'Tổng Tiền', i.status AS 'Trạng Thái'
                               FROM invoices i JOIN contracts c ON i.contract_id = c.id
                               WHERE c.customer_id = " + _currentUserId + " ORDER BY i.year DESC, i.month DESC";
                DataTable dt = DatabaseHelper.GetData(sql);
                dgvInvoices.DataSource = dt;

                if (dgvInvoices.Columns["id"] != null) dgvInvoices.Columns["id"].Visible = false;
                if (dgvInvoices.Columns["Tổng Tiền"] != null) dgvInvoices.Columns["Tổng Tiền"].DefaultCellStyle.Format = "N0";

                decimal debt = 0;
                foreach (DataRow r in dt.Rows) if (r["Trạng Thái"].ToString() == "Unpaid") debt += Convert.ToDecimal(r["Tổng Tiền"]);
                lblValDebt.Text = debt > 0 ? debt.ToString("N0") + " đ" : "Không nợ";
                lblValDebt.ForeColor = debt > 0 ? Color.Red : Color.SeaGreen;
            }
            catch { }
        }

        private void LoadRequestData()
        {
            if (_currentUserId == -1) return;
            try
            {
                string sql = $"SELECT title AS 'Tiêu Đề', description AS 'Nội Dung', status AS 'Trạng Thái', created_at AS 'Ngày Gửi' FROM requests WHERE user_id={_currentUserId} ORDER BY created_at DESC";
                dgvRequests.DataSource = DatabaseHelper.GetData(sql);
            }
            catch { }
        }

        // =================================================================================
        // PHẦN 2: THIẾT KẾ GIAO DIỆN
        // =================================================================================

        private void InitModernUI()
        {
            // HEADER
            Panel pnlHeader = new Panel { Dock = DockStyle.Top, Height = 80, BackColor = colorPrimary };
            Label lblTitle = new Label { Text = "ELITE HOMES PORTAL", ForeColor = colorActive, Font = new Font("Segoe UI", 20, FontStyle.Bold), AutoSize = true, Location = new Point(30, 20) };
            Label lblUser = new Label { Text = _currentName.ToUpper(), ForeColor = Color.White, Font = new Font("Segoe UI", 12, FontStyle.Bold), AutoSize = true, Location = new Point(this.Width - 250, 20), Anchor = AnchorStyles.Top | AnchorStyles.Right };

            Button btnLogout = new Button { Text = "Đăng Xuất", BackColor = Color.IndianRed, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Size = new Size(100, 30), Location = new Point(this.Width - 140, 20), Anchor = AnchorStyles.Top | AnchorStyles.Right, Cursor = Cursors.Hand };
            btnLogout.FlatAppearance.BorderSize = 0;
            btnLogout.Click += (s, e) => this.Close();

            pnlHeader.Controls.AddRange(new Control[] { lblTitle, lblUser, btnLogout });
            this.Controls.Add(pnlHeader);

            // TABS
            tabMain = new TabControl { Dock = DockStyle.Fill, Font = new Font("Segoe UI", 11) };
            tabMain.Padding = new Point(20, 8);
            this.Controls.Add(tabMain);
            tabMain.BringToFront();

            BuildTabDashboard();
            BuildTabBill();
            BuildTabRequest();
            BuildTabSetting();
        }

        private void BuildTabDashboard()
        {
            TabPage tab = new TabPage("  🏠 Tổng Quan  ");
            tab.BackColor = colorBg;

            TableLayoutPanel layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1, Padding = new Padding(30) };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40F));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60F));

            // Trái: Thông tin
            Panel cardProfile = CreateCard("HỒ SƠ CƯ DÂN");
            cardProfile.Dock = DockStyle.Fill;
            Panel pnlContent1 = new Panel { Dock = DockStyle.Fill, Padding = new Padding(20) };
            AddInfoRow(pnlContent1, "Họ và tên:", out lblValName, 0);
            AddInfoRow(pnlContent1, "Số điện thoại:", out lblValPhone, 50);
            AddInfoRow(pnlContent1, "Phòng đang ở:", out lblValRoom, 100);
            AddInfoRow(pnlContent1, "Hợp đồng đến:", out lblValDate, 150);
            AddInfoRow(pnlContent1, "Công nợ:", out lblValDebt, 200);
            cardProfile.Controls.Add(pnlContent1);

            // Phải: Hợp đồng
            Panel cardCon = CreateCard("HỢP ĐỒNG CỦA TÔI");
            cardCon.Dock = DockStyle.Fill;
            dgvContracts = CreateNiceGrid();
            dgvContracts.Dock = DockStyle.Fill;
            Panel pnlGridWrap = new Panel { Dock = DockStyle.Fill, Padding = new Padding(0, 50, 0, 0) };
            pnlGridWrap.Controls.Add(dgvContracts);
            cardCon.Controls.Add(pnlGridWrap);

            layout.Controls.Add(cardProfile, 0, 0);
            layout.Controls.Add(cardCon, 1, 0);
            tab.Controls.Add(layout);
            tabMain.TabPages.Add(tab);
        }

        private void BuildTabBill()
        {
            TabPage tab = new TabPage("  💰 Hóa Đơn  ");
            tab.BackColor = colorBg;

            Panel card = CreateCard("LỊCH SỬ THANH TOÁN");
            card.Location = new Point(30, 30);
            card.Size = new Size(1200, 600);
            card.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;

            Button btnRef = CreateButton("Làm Mới", colorActive, card.Width - 130, 10);
            btnRef.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnRef.Click += (s, e) => LoadInvoiceData();
            card.Controls.Add(btnRef);

            dgvInvoices = CreateNiceGrid();
            dgvInvoices.Dock = DockStyle.Fill;
            dgvInvoices.CellFormatting += (s, e) => {
                if (dgvInvoices.Columns[e.ColumnIndex].Name == "Trạng Thái")
                {
                    if (e.Value?.ToString() == "Unpaid") e.CellStyle.ForeColor = colorDanger;
                    else e.CellStyle.ForeColor = colorSuccess;
                }
            };

            Panel pnlGrid = new Panel { Dock = DockStyle.Fill, Padding = new Padding(20, 60, 20, 20) };
            pnlGrid.Controls.Add(dgvInvoices);
            card.Controls.Add(pnlGrid);

            tab.Controls.Add(card);
            tabMain.TabPages.Add(tab);
        }

        private void BuildTabRequest()
        {
            TabPage tab = new TabPage("  🛠️ Gửi Yêu Cầu  ");
            tab.BackColor = colorBg;

            TableLayoutPanel layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1, Padding = new Padding(30) };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 450F));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));

            // Form nhập
            Panel cardForm = CreateCard("NHẬP THÔNG TIN");
            cardForm.Dock = DockStyle.Fill;
            Panel pnlIn = new Panel { Dock = DockStyle.Fill, Padding = new Padding(20, 60, 20, 20) };

            Label l1 = CreateLabel("Tiêu đề sự cố:", 20, 60);
            txtReqTitle = CreateInput(20, 90, 380);

            Label l2 = CreateLabel("Mô tả chi tiết:", 20, 140);
            txtReqContent = CreateInput(20, 170, 380);
            txtReqContent.Multiline = true; txtReqContent.Height = 200;

            Button btnSend = CreateButton("GỬI YÊU CẦU NGAY", colorDanger, 20, 400);
            btnSend.Width = 380;
            btnSend.Click += BtnSend_Click;

            pnlIn.Controls.AddRange(new Control[] { l1, txtReqTitle, l2, txtReqContent, btnSend });
            cardForm.Controls.Add(pnlIn);

            // Grid lịch sử
            Panel cardHist = CreateCard("LỊCH SỬ XỬ LÝ");
            cardHist.Dock = DockStyle.Fill;
            dgvRequests = CreateNiceGrid();
            dgvRequests.Dock = DockStyle.Fill;
            Panel pnlGridWrap = new Panel { Dock = DockStyle.Fill, Padding = new Padding(20, 60, 20, 20) };
            pnlGridWrap.Controls.Add(dgvRequests);
            cardHist.Controls.Add(pnlGridWrap);

            layout.Controls.Add(cardForm, 0, 0);
            layout.Controls.Add(cardHist, 1, 0);
            tab.Controls.Add(layout);
            tabMain.TabPages.Add(tab);
        }

        private void BuildTabSetting()
        {
            TabPage tab = new TabPage("  🔒 Bảo Mật  ");
            tab.BackColor = colorBg;

            Panel card = CreateCard("THIẾT LẬP MẬT KHẨU");
            card.Size = new Size(600, 500);

            tab.Resize += (s, e) => {
                card.Left = (tab.Width - card.Width) / 2;
                card.Top = 60;
            };

            Panel pnlContent = new Panel { Dock = DockStyle.Fill, Padding = new Padding(40) };

            AddNicePassField(pnlContent, "Mật khẩu hiện tại:", out txtOldPass, 40);
            AddNicePassField(pnlContent, "Mật khẩu mới:", out txtNewPass, 130);
            AddNicePassField(pnlContent, "Nhập lại mật khẩu mới:", out txtConfirmPass, 220);

            Button btnSave = CreateButton("CẬP NHẬT MẬT KHẨU", colorSuccess, 40, 350);
            btnSave.Width = 520;
            btnSave.Height = 55;
            btnSave.Click += BtnChangePass_Click;

            pnlContent.Controls.Add(btnSave);
            card.Controls.Add(pnlContent);

            tab.Controls.Add(card);
            tabMain.TabPages.Add(tab);
        }

        // =================================================================================
        // PHẦN 3: ACTIONS
        // =================================================================================
        private void BtnSend_Click(object sender, EventArgs e)
        {
            if (_currentUserId == -1) return;
            if (string.IsNullOrWhiteSpace(txtReqTitle.Text)) { MessageBox.Show("Chưa nhập tiêu đề!"); return; }
            try
            {
                int roomId = 0;
                using (var conn = DatabaseHelper.GetConnection())
                {
                    if (conn.State == ConnectionState.Closed) conn.Open();
                    MySqlCommand cmdR = new MySqlCommand("SELECT room_id FROM contracts WHERE customer_id=@u AND status='Active' LIMIT 1", conn);
                    cmdR.Parameters.AddWithValue("@u", _currentUserId);
                    object rObj = cmdR.ExecuteScalar();
                    if (rObj != null) roomId = Convert.ToInt32(rObj);

                    string sql = "INSERT INTO requests (user_id, room_id, title, description, status) VALUES (@u, @r, @t, @d, 'Pending')";
                    MySqlCommand cmd = new MySqlCommand(sql, conn);
                    cmd.Parameters.AddWithValue("@u", _currentUserId);
                    cmd.Parameters.AddWithValue("@r", roomId);
                    cmd.Parameters.AddWithValue("@t", txtReqTitle.Text);
                    cmd.Parameters.AddWithValue("@d", txtReqContent.Text);
                    cmd.ExecuteNonQuery();
                }
                MessageBox.Show("Gửi thành công!");
                txtReqTitle.Clear(); txtReqContent.Clear();
                LoadRequestData();
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        }

        private void BtnChangePass_Click(object sender, EventArgs e)
        {
            if (txtNewPass.Text != txtConfirmPass.Text) { MessageBox.Show("Mật khẩu không khớp!"); return; }
            try
            {
                using (var conn = DatabaseHelper.GetConnection())
                {
                    if (conn.State == ConnectionState.Closed) conn.Open();
                    MySqlCommand cmdCheck = new MySqlCommand("SELECT count(*) FROM users WHERE id=@id AND password=@pass", conn);
                    cmdCheck.Parameters.AddWithValue("@id", _currentUserId);
                    cmdCheck.Parameters.AddWithValue("@pass", txtOldPass.Text);
                    if (Convert.ToInt32(cmdCheck.ExecuteScalar()) == 0) { MessageBox.Show("Sai mật khẩu cũ!"); return; }

                    MySqlCommand cmdUp = new MySqlCommand("UPDATE users SET password=@pass WHERE id=@id", conn);
                    cmdUp.Parameters.AddWithValue("@id", _currentUserId);
                    cmdUp.Parameters.AddWithValue("@pass", txtNewPass.Text);
                    cmdUp.ExecuteNonQuery();
                }
                MessageBox.Show("Đổi mật khẩu thành công!");
                txtOldPass.Clear(); txtNewPass.Clear(); txtConfirmPass.Clear();
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        }

        // =================================================================================
        // UI HELPERS
        // =================================================================================

        private Panel CreateCard(string title)
        {
            Panel p = new Panel { BackColor = Color.White };
            Label lbl = new Label { Text = title, ForeColor = colorPrimary, Font = new Font("Segoe UI", 12, FontStyle.Bold), Location = new Point(20, 15), AutoSize = true };
            Panel line = new Panel { BackColor = Color.LightGray, Size = new Size(p.Width, 1), Location = new Point(0, 50), Dock = DockStyle.Top };
            p.Controls.Add(line); p.Controls.Add(lbl);
            return p;
        }

        private DataGridView CreateNiceGrid()
        {
            DataGridView d = new DataGridView();
            d.BackgroundColor = Color.White;
            d.BorderStyle = BorderStyle.None;
            d.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            d.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
            d.EnableHeadersVisualStyles = false;
            d.ColumnHeadersHeight = 45;
            d.ColumnHeadersDefaultCellStyle.BackColor = colorPrimary;
            d.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            d.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            d.ColumnHeadersDefaultCellStyle.Padding = new Padding(10, 0, 0, 0);
            d.DefaultCellStyle.Font = new Font("Segoe UI", 10);
            d.DefaultCellStyle.SelectionBackColor = Color.AliceBlue;
            d.DefaultCellStyle.SelectionForeColor = Color.Black;
            d.DefaultCellStyle.Padding = new Padding(10, 5, 5, 5);
            d.RowHeadersVisible = false;
            d.AllowUserToAddRows = false;
            d.ReadOnly = true;
            d.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            d.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            d.RowTemplate.Height = 40;
            return d;
        }

        private void AddInfoRow(Panel p, string title, out Label valLabel, int y)
        {
            Label l = new Label { Text = title, ForeColor = Color.Gray, Font = new Font("Segoe UI", 10), Location = new Point(20, y + 20), AutoSize = true };
            valLabel = new Label { Text = "...", ForeColor = colorPrimary, Font = new Font("Segoe UI", 12, FontStyle.Bold), Location = new Point(150, y + 18), AutoSize = true };
            p.Controls.Add(l); p.Controls.Add(valLabel);
        }

        private void AddNicePassField(Panel p, string title, out TextBox txt, int y)
        {
            Label l = new Label { Text = title, Location = new Point(40, y), AutoSize = true, Font = new Font("Segoe UI", 11), ForeColor = Color.DimGray };
            txt = new TextBox { Location = new Point(40, y + 30), Size = new Size(520, 35), Font = new Font("Segoe UI", 12), UseSystemPasswordChar = true, BorderStyle = BorderStyle.FixedSingle, BackColor = Color.WhiteSmoke };
            p.Controls.Add(l); p.Controls.Add(txt);
        }

        private Button CreateButton(string text, Color bg, int x, int y)
        {
            return new Button { Text = text, BackColor = bg, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 10, FontStyle.Bold), Size = new Size(120, 40), Location = new Point(x, y), Cursor = Cursors.Hand };
        }

        private Label CreateLabel(string t, int x, int y)
        {
            return new Label { Text = t, Location = new Point(x, y), AutoSize = true, Font = new Font("Segoe UI", 10), ForeColor = Color.Gray };
        }

        private TextBox CreateInput(int x, int y, int w)
        {
            return new TextBox { Location = new Point(x, y), Size = new Size(w, 30), Font = new Font("Segoe UI", 11), BorderStyle = BorderStyle.FixedSingle };
        }
    }
}