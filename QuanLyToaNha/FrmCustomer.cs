using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using System.IO;   // Thư viện để lưu file
using System.Text; // Thư viện xử lý Tiếng Việt

namespace QuanLyToaNha
{
    public partial class FrmCustomer : Form
    {
        // CHỈ KHAI BÁO TEXTBOX (Vì Designer đã có dgvCustomer)
        private TextBox txtID, txtName, txtPhone, txtCompany, txtEmail;

        public FrmCustomer()
        {
            InitializeComponent();

            DesignInputPanel();

            // Style bảng lưới
            StyleDataGridView();

            LoadData();
        }

        private void LoadData()
        {
            try
            {
                string sql = "SELECT id, full_name, phone, address, email FROM users WHERE role = 'resident'";
                DataTable dt = DatabaseHelper.GetData(sql);
                dgvCustomer.DataSource = dt;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải dữ liệu: " + ex.Message);
            }
        }

        private void DesignInputPanel()
        {
            Panel pnlInput = new Panel { Dock = DockStyle.Top, Height = 240, BackColor = Color.WhiteSmoke };
            this.Controls.Add(pnlInput);

            CreateInput(pnlInput, "Mã Cư Dân (Auto):", out txtID, 40, 30, 100);
            txtID.Enabled = false;

            CreateInput(pnlInput, "Họ Tên Cư Dân:", out txtName, 180, 30, 250);

            // --- Ô NHẬP SỐ ĐIỆN THOẠI (CHẶN CHỮ) ---
            CreateInput(pnlInput, "Số Điện Thoại:", out txtPhone, 460, 30, 200);
            txtPhone.KeyPress += (s, e) => {
                if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar)) e.Handled = true;
            };

            CreateInput(pnlInput, "Địa Chỉ / Căn Hộ:", out txtCompany, 40, 100, 390);
            CreateInput(pnlInput, "Email Liên Hệ:", out txtEmail, 460, 100, 250);

            int btnY = 170;

            // --- NÚT THÊM ---
            Button btnAdd = CreateButton("THÊM CƯ DÂN", Color.FromArgb(24, 161, 251), 40, btnY);
            btnAdd.Click += (s, e) => {
                if (string.IsNullOrWhiteSpace(txtName.Text)) return;
                try
                {
                    string autoUser = string.IsNullOrWhiteSpace(txtPhone.Text) ? "user" + new Random().Next(1000, 9999) : txtPhone.Text;
                    string sql = $"INSERT INTO users (username, password, full_name, phone, address, email, role) " +
                                 $"VALUES ('{autoUser}', '123456', '{txtName.Text}', '{txtPhone.Text}', '{txtCompany.Text}', '{txtEmail.Text}', 'resident')";
                    DatabaseHelper.ExecuteSql(sql);
                    MessageBox.Show($"Thêm thành công!\nTK: {autoUser} | MK: 123456");
                    LoadData();
                    ClearInput();
                }
                catch (Exception ex) { MessageBox.Show("Lỗi: " + ex.Message); }
            };
            pnlInput.Controls.Add(btnAdd);

            // --- NÚT SỬA ---
            Button btnEdit = CreateButton("CẬP NHẬT", Color.FromArgb(255, 193, 7), 200, btnY);
            btnEdit.Click += (s, e) => {
                if (string.IsNullOrEmpty(txtID.Text)) return;
                try
                {
                    string sql = $"UPDATE users SET full_name='{txtName.Text}', phone='{txtPhone.Text}', " +
                                 $"address='{txtCompany.Text}', email='{txtEmail.Text}' WHERE id={txtID.Text}";
                    DatabaseHelper.ExecuteSql(sql);
                    MessageBox.Show("Đã cập nhật!");
                    LoadData();
                    ClearInput();
                }
                catch (Exception ex) { MessageBox.Show("Lỗi: " + ex.Message); }
            };
            pnlInput.Controls.Add(btnEdit);

            // --- NÚT XÓA ---
            Button btnDelete = CreateButton("XÓA CƯ DÂN", Color.FromArgb(253, 138, 114), 360, btnY);
            btnDelete.Click += (s, e) => {
                if (string.IsNullOrEmpty(txtID.Text)) return;
                if (MessageBox.Show("CẢNH BÁO: Xóa cư dân sẽ xóa luôn Tài khoản và Hợp đồng thuê nhà.\nBạn có chắc chắn?",
                    "Xác nhận xóa", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
                {
                    try
                    {
                        string sqlDelContract = $"DELETE FROM contracts WHERE customer_id={txtID.Text}";
                        DatabaseHelper.ExecuteSql(sqlDelContract);
                        string sqlDelUser = $"DELETE FROM users WHERE id={txtID.Text}";
                        DatabaseHelper.ExecuteSql(sqlDelUser);
                        MessageBox.Show("Đã xóa hoàn toàn dữ liệu!");
                        LoadData();
                        ClearInput();
                    }
                    catch (Exception ex) { MessageBox.Show("Lỗi: " + ex.Message); }
                }
            };
            pnlInput.Controls.Add(btnDelete);

            // --- NÚT LÀM MỚI ---
            Button btnClear = CreateButton("LÀM MỚI", Color.Gray, 520, btnY);
            btnClear.Click += (s, e) => { ClearInput(); LoadData(); };
            pnlInput.Controls.Add(btnClear);

            // --- NÚT XUẤT EXCEL (MỚI) ---
            Button btnExcel = CreateButton("XUẤT EXCEL", Color.FromArgb(39, 174, 96), 680, btnY);
            btnExcel.Click += (s, e) => ExportToCSV();
            pnlInput.Controls.Add(btnExcel);
        }

        // --- HÀM XUẤT CSV ---
        private void ExportToCSV()
        {
            if (dgvCustomer.Rows.Count == 0)
            {
                MessageBox.Show("Không có dữ liệu để xuất!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "Excel CSV (*.csv)|*.csv";
            sfd.FileName = "DanhSachCuDan.csv";

            if (sfd.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    // Encoding.UTF8 để hỗ trợ tiếng Việt
                    using (StreamWriter sw = new StreamWriter(sfd.FileName, false, Encoding.UTF8))
                    {
                        // 1. Ghi tiêu đề cột
                        string[] header = new string[dgvCustomer.Columns.Count];
                        for (int i = 0; i < dgvCustomer.Columns.Count; i++)
                        {
                            header[i] = dgvCustomer.Columns[i].HeaderText;
                        }
                        sw.WriteLine(string.Join(",", header));

                        // 2. Ghi dữ liệu dòng
                        foreach (DataGridViewRow row in dgvCustomer.Rows)
                        {
                            if (!row.IsNewRow)
                            {
                                string[] cells = new string[dgvCustomer.Columns.Count];
                                for (int i = 0; i < dgvCustomer.Columns.Count; i++)
                                {
                                    string value = row.Cells[i].Value?.ToString() ?? "";
                                    // Xử lý nếu dữ liệu có dấu phẩy (ví dụ địa chỉ) thì bọc trong ngoặc kép
                                    cells[i] = value.Contains(",") ? $"\"{value}\"" : value;
                                }
                                sw.WriteLine(string.Join(",", cells));
                            }
                        }
                    }
                    MessageBox.Show("Xuất file thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    // Mở file lên ngay
                    System.Diagnostics.Process.Start(sfd.FileName);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi khi xuất file: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void ClearInput() { txtID.Clear(); txtName.Clear(); txtPhone.Clear(); txtCompany.Clear(); txtEmail.Clear(); txtName.Focus(); }

        private void CreateInput(Panel p, string l, out TextBox t, int x, int y, int w)
        {
            p.Controls.Add(new Label { Text = l, Location = new Point(x, y), AutoSize = true, Font = new Font("Segoe UI", 10), ForeColor = Color.DimGray });
            t = new TextBox { Location = new Point(x, y + 28), Size = new Size(w, 35), Font = new Font("Segoe UI", 11) };
            p.Controls.Add(t);
        }

        private Button CreateButton(string t, Color c, int x, int y)
        {
            return new Button { Text = t, BackColor = c, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Size = new Size(140, 40), Location = new Point(x, y), Font = new Font("Segoe UI", 10, FontStyle.Bold), Cursor = Cursors.Hand };
        }

        private void StyleDataGridView()
        {
            if (dgvCustomer == null) return;

            dgvCustomer.BorderStyle = BorderStyle.None;
            dgvCustomer.BackgroundColor = Color.White;
            dgvCustomer.RowTemplate.Height = 55;
            dgvCustomer.ColumnHeadersHeight = 60;
            dgvCustomer.EnableHeadersVisualStyles = false;
            dgvCustomer.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(24, 30, 54);
            dgvCustomer.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvCustomer.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 12, FontStyle.Bold);
            dgvCustomer.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvCustomer.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvCustomer.RowHeadersVisible = false;
            dgvCustomer.ReadOnly = true;
            dgvCustomer.AllowUserToAddRows = false;

            dgvCustomer.CellClick += (s, e) => {
                if (e.RowIndex >= 0)
                {
                    var r = dgvCustomer.Rows[e.RowIndex];
                    txtID.Text = r.Cells["id"].Value.ToString();
                    txtName.Text = r.Cells["full_name"].Value.ToString();
                    txtPhone.Text = r.Cells["phone"].Value.ToString();
                    txtCompany.Text = r.Cells["address"].Value != DBNull.Value ? r.Cells["address"].Value.ToString() : "";
                    txtEmail.Text = r.Cells["email"].Value != DBNull.Value ? r.Cells["email"].Value.ToString() : "";
                }
            };
        }
    }
}