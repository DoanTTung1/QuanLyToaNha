using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace QuanLyToaNha
{
    public partial class FrmCustomer : Form
    {
        private TextBox txtID, txtFullname, txtPhone, txtUsername, txtPassword;
        

        public FrmCustomer()
        {
            InitializeComponent();
            this.Controls.Clear();
            this.BackColor = Color.White;

            DesignInputPanel();
            StyleDataGridView();
            LoadData();
        }

        // --- VALIDATION (KIỂM TRA LỖI) ---
        private bool ValidateInput()
        {
            if (string.IsNullOrWhiteSpace(txtFullname.Text))
            {
                MessageBox.Show("Vui lòng nhập Họ và Tên!", "Thiếu thông tin", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtFullname.Focus();
                return false;
            }
            if (string.IsNullOrWhiteSpace(txtPhone.Text))
            {
                MessageBox.Show("Vui lòng nhập Số Điện Thoại!", "Thiếu thông tin", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtPhone.Focus();
                return false;
            }
            if (string.IsNullOrWhiteSpace(txtUsername.Text))
            {
                MessageBox.Show("Vui lòng nhập Tên Đăng Nhập!", "Thiếu thông tin", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtUsername.Focus();
                return false;
            }
            if (string.IsNullOrWhiteSpace(txtPassword.Text))
            {
                MessageBox.Show("Vui lòng nhập Mật Khẩu!", "Thiếu thông tin", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtPassword.Focus();
                return false;
            }
            return true;
        }

        private void LoadData()
        {
            try
            {
                string sql = "SELECT id, full_name, phone, username, password FROM users WHERE role = 'resident'";
                DataTable dt = DatabaseHelper.GetData(sql);
                dgvCustomer.DataSource = dt;

                // Đổi tên cột hiển thị cho đẹp
                if (dgvCustomer.Columns["id"] != null) dgvCustomer.Columns["id"].HeaderText = "Mã Cư Dân";
                if (dgvCustomer.Columns["full_name"] != null) dgvCustomer.Columns["full_name"].HeaderText = "Họ và Tên";
                if (dgvCustomer.Columns["phone"] != null) dgvCustomer.Columns["phone"].HeaderText = "SĐT";
                if (dgvCustomer.Columns["username"] != null) dgvCustomer.Columns["username"].HeaderText = "Tài Khoản";
                if (dgvCustomer.Columns["password"] != null) dgvCustomer.Columns["password"].HeaderText = "Mật Khẩu";
            }
            catch (Exception ex) { MessageBox.Show("Lỗi tải dữ liệu: " + ex.Message); }
        }

        private void DesignInputPanel()
        {
            Panel pnlInput = new Panel { Dock = DockStyle.Top, Height = 220, BackColor = Color.WhiteSmoke };
            this.Controls.Add(pnlInput);

            CreateInput(pnlInput, "Mã Cư Dân:", out txtID, 40, 20, 100); txtID.Enabled = false;
            CreateInput(pnlInput, "Họ và Tên (*):", out txtFullname, 180, 20, 250);
            CreateInput(pnlInput, "Số Điện Thoại (*):", out txtPhone, 460, 20, 200);

            // CHẶN NHẬP CHỮ VÀO SĐT
            txtPhone.KeyPress += (s, e) => {
                if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar)) e.Handled = true;
            };

            CreateInput(pnlInput, "Tên Đăng Nhập (*):", out txtUsername, 40, 90, 200);
            CreateInput(pnlInput, "Mật Khẩu (*):", out txtPassword, 280, 90, 200);

            // Buttons
            int btnY = 160;
            Button btnAdd = CreateButton("THÊM CƯ DÂN", Color.FromArgb(24, 161, 251), 40, btnY);
            btnAdd.Click += BtnAdd_Click;
            pnlInput.Controls.Add(btnAdd);

            Button btnEdit = CreateButton("CẬP NHẬT", Color.FromArgb(255, 193, 7), 200, btnY);
            btnEdit.Click += BtnEdit_Click;
            pnlInput.Controls.Add(btnEdit);

            Button btnDelete = CreateButton("XÓA", Color.IndianRed, 360, btnY);
            btnDelete.Click += BtnDelete_Click;
            pnlInput.Controls.Add(btnDelete);

            Button btnRefresh = CreateButton("LÀM MỚI", Color.Gray, 520, btnY);
            btnRefresh.Click += (s, e) => { ClearInput(); LoadData(); };
            pnlInput.Controls.Add(btnRefresh);
        }

        // --- CRUD ACTIONS ---
        private void BtnAdd_Click(object sender, EventArgs e)
        {
            if (!ValidateInput()) return;

            try
            {
                // Check trùng username
                DataTable dt = DatabaseHelper.GetData($"SELECT * FROM users WHERE username='{txtUsername.Text}'");
                if (dt.Rows.Count > 0)
                {
                    MessageBox.Show("Tên đăng nhập đã tồn tại! Vui lòng chọn tên khác.", "Trùng lặp", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtUsername.Focus();
                    return;
                }

                string sql = $"INSERT INTO users (full_name, phone, username, password, role) " +
                             $"VALUES ('{txtFullname.Text}', '{txtPhone.Text}', '{txtUsername.Text}', '{txtPassword.Text}', 'resident')";

                DatabaseHelper.ExecuteSql(sql);
                MessageBox.Show("Thêm cư dân thành công!");
                LoadData(); ClearInput();
            }
            catch (Exception ex) { MessageBox.Show("Lỗi: " + ex.Message); }
        }

        private void BtnEdit_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtID.Text)) { MessageBox.Show("Vui lòng chọn cư dân cần sửa!"); return; }
            if (!ValidateInput()) return;

            try
            {
                string sql = $"UPDATE users SET full_name='{txtFullname.Text}', phone='{txtPhone.Text}', " +
                             $"username='{txtUsername.Text}', password='{txtPassword.Text}' WHERE id={txtID.Text}";

                DatabaseHelper.ExecuteSql(sql);
                MessageBox.Show("Cập nhật thành công!");
                LoadData(); ClearInput();
            }
            catch (Exception ex) { MessageBox.Show("Lỗi: " + ex.Message); }
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtID.Text)) return;
            if (MessageBox.Show("Xóa cư dân này sẽ xóa luôn Hợp đồng & Hóa đơn liên quan.\nBạn chắc chắn chứ?", "Cảnh báo", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                try
                {
                    DatabaseHelper.ExecuteSql($"DELETE FROM users WHERE id={txtID.Text}");
                    MessageBox.Show("Đã xóa!");
                    LoadData(); ClearInput();
                }
                catch (Exception ex) { MessageBox.Show("Lỗi xóa: " + ex.Message); }
            }
        }

        // --- HELPERS ---
        private void ClearInput() { txtID.Clear(); txtFullname.Clear(); txtPhone.Clear(); txtUsername.Clear(); txtPassword.Clear(); txtFullname.Focus(); }

        private void CreateInput(Panel p, string l, out TextBox t, int x, int y, int w)
        {
            p.Controls.Add(new Label { Text = l, Location = new Point(x, y), AutoSize = true, ForeColor = Color.DimGray });
            t = new TextBox { Location = new Point(x, y + 25), Size = new Size(w, 30), Font = new Font("Segoe UI", 11) };
            p.Controls.Add(t);
        }

        private Button CreateButton(string t, Color c, int x, int y)
        {
            return new Button { Text = t, BackColor = c, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Size = new Size(140, 40), Location = new Point(x, y), Font = new Font("Segoe UI", 10, FontStyle.Bold), Cursor = Cursors.Hand };
        }

        private void StyleDataGridView()
        {
            dgvCustomer = new DataGridView();
            dgvCustomer.Dock = DockStyle.Fill;
            this.Controls.Add(dgvCustomer);
            dgvCustomer.BringToFront();

            dgvCustomer.BorderStyle = BorderStyle.None;
            dgvCustomer.BackgroundColor = Color.White;
            dgvCustomer.RowTemplate.Height = 40;
            dgvCustomer.ColumnHeadersHeight = 50;
            dgvCustomer.EnableHeadersVisualStyles = false;
            dgvCustomer.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(24, 30, 54);
            dgvCustomer.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvCustomer.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 11, FontStyle.Bold);
            dgvCustomer.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvCustomer.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvCustomer.ReadOnly = true;
            dgvCustomer.AllowUserToAddRows = false;
            dgvCustomer.RowHeadersVisible = false;

            dgvCustomer.CellClick += (s, e) => {
                if (e.RowIndex >= 0)
                {
                    var r = dgvCustomer.Rows[e.RowIndex];
                    txtID.Text = r.Cells["id"].Value.ToString();
                    txtFullname.Text = r.Cells["full_name"].Value.ToString();
                    txtPhone.Text = r.Cells["phone"].Value.ToString();
                    txtUsername.Text = r.Cells["username"].Value.ToString();
                    txtPassword.Text = r.Cells["password"].Value.ToString();
                }
            };
        }
    }
}