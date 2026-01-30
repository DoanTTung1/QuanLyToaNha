using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace QuanLyToaNha
{
    public partial class FrmStaff : Form
    {
        private TextBox txtID, txtFullname, txtPhone, txtUsername, txtPassword;
        private ComboBox cbbRole;
        

        public FrmStaff()
        {
            InitializeComponent();
            this.Controls.Clear();

            DesignInputPanel();
            StyleDataGridView();
            LoadData();
        }

        private void LoadData()
        {
            try
            {
                // Lấy danh sách Admin và Staff (loại trừ Resident)
                string sql = "SELECT id, full_name, phone, username, password, role FROM users WHERE role IN ('admin', 'staff')";
                DataTable dt = DatabaseHelper.GetData(sql);
                dgvStaff.DataSource = dt;
            }
            catch (Exception ex) { MessageBox.Show("Lỗi: " + ex.Message); }
        }

        private void DesignInputPanel()
        {
            Panel pnlInput = new Panel { Dock = DockStyle.Top, Height = 220, BackColor = Color.WhiteSmoke };
            this.Controls.Add(pnlInput);

            // --- CỘT 1 ---
            CreateInput(pnlInput, "Mã NV (Auto):", out txtID, 40, 20, 100);
            txtID.Enabled = false;

            CreateInput(pnlInput, "Họ và Tên:", out txtFullname, 180, 20, 250);
            CreateInput(pnlInput, "Số Điện Thoại:", out txtPhone, 460, 20, 200);

            // --- CỘT 2 ---
            CreateInput(pnlInput, "Tên Đăng Nhập:", out txtUsername, 40, 90, 200);
            CreateInput(pnlInput, "Mật Khẩu:", out txtPassword, 280, 90, 200);

            pnlInput.Controls.Add(new Label { Text = "Vai Trò:", Location = new Point(520, 90), AutoSize = true, ForeColor = Color.DimGray });
            cbbRole = new ComboBox { Location = new Point(520, 115), Width = 140, DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 11) };
            cbbRole.Items.AddRange(new string[] { "admin", "staff" });
            cbbRole.SelectedIndex = 1; // Mặc định là Staff
            pnlInput.Controls.Add(cbbRole);

            // --- BUTTONS ---
            int btnY = 160;
            Button btnAdd = CreateButton("THÊM NHÂN VIÊN", Color.FromArgb(24, 161, 251), 40, btnY);
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

        // --- CRUD ---
        private void BtnAdd_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtUsername.Text)) return;
            try
            {
                // Kiểm tra trùng username
                if (DatabaseHelper.GetData($"SELECT * FROM users WHERE username='{txtUsername.Text}'").Rows.Count > 0)
                {
                    MessageBox.Show("Tên đăng nhập đã tồn tại!"); return;
                }

                string role = cbbRole.SelectedItem.ToString();
                string sql = $"INSERT INTO users (full_name, phone, username, password, role) " +
                             $"VALUES ('{txtFullname.Text}', '{txtPhone.Text}', '{txtUsername.Text}', '{txtPassword.Text}', '{role}')";

                DatabaseHelper.ExecuteSql(sql);
                MessageBox.Show("Thêm nhân viên thành công!");
                LoadData(); ClearInput();
            }
            catch (Exception ex) { MessageBox.Show("Lỗi: " + ex.Message); }
        }

        private void BtnEdit_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtID.Text)) return;
            try
            {
                string role = cbbRole.SelectedItem.ToString();
                string sql = $"UPDATE users SET full_name='{txtFullname.Text}', phone='{txtPhone.Text}', " +
                             $"username='{txtUsername.Text}', password='{txtPassword.Text}', role='{role}' WHERE id={txtID.Text}";

                DatabaseHelper.ExecuteSql(sql);
                MessageBox.Show("Đã cập nhật thông tin!");
                LoadData(); ClearInput();
            }
            catch (Exception ex) { MessageBox.Show("Lỗi: " + ex.Message); }
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtID.Text)) return;
            // Không cho phép xóa chính mình (nếu làm kỹ hơn sẽ check session)
            // Ở đây tạm thời cảnh báo
            if (MessageBox.Show("Bạn chắc chắn muốn xóa nhân viên này?", "Cảnh báo", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                DatabaseHelper.ExecuteSql($"DELETE FROM users WHERE id={txtID.Text}");
                LoadData(); ClearInput();
            }
        }

        // --- HELPERS ---
        private void ClearInput() { txtID.Clear(); txtFullname.Clear(); txtPhone.Clear(); txtUsername.Clear(); txtPassword.Clear(); cbbRole.SelectedIndex = 1; }

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
            dgvStaff = new DataGridView();
            dgvStaff.Dock = DockStyle.Fill;
            this.Controls.Add(dgvStaff);
            dgvStaff.BringToFront();

            dgvStaff.BorderStyle = BorderStyle.None;
            dgvStaff.BackgroundColor = Color.White;
            dgvStaff.RowTemplate.Height = 40;
            dgvStaff.ColumnHeadersHeight = 50;
            dgvStaff.EnableHeadersVisualStyles = false;
            dgvStaff.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(24, 30, 54);
            dgvStaff.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvStaff.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 11, FontStyle.Bold);
            dgvStaff.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvStaff.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvStaff.ReadOnly = true;
            dgvStaff.AllowUserToAddRows = false;
            dgvStaff.RowHeadersVisible = false;

            dgvStaff.CellClick += (s, e) => {
                if (e.RowIndex >= 0)
                {
                    var r = dgvStaff.Rows[e.RowIndex];
                    txtID.Text = r.Cells["id"].Value.ToString();
                    txtFullname.Text = r.Cells["full_name"].Value.ToString();
                    txtPhone.Text = r.Cells["phone"].Value.ToString();
                    txtUsername.Text = r.Cells["username"].Value.ToString();
                    txtPassword.Text = r.Cells["password"].Value.ToString();
                    cbbRole.Text = r.Cells["role"].Value.ToString();
                }
            };
        }
    }
}