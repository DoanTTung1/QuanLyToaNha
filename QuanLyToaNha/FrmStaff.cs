using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using System.IO;   // Thư viện lưu file
using System.Text; // Thư viện xử lý tiếng Việt

namespace QuanLyToaNha
{
    public partial class FrmStaff : Form
    {
        private TextBox txtUser, txtName, txtRole, txtPhone;

        public FrmStaff()
        {
            InitializeComponent();
            DesignInputPanel();
            StyleDataGridView();
            LoadData();
        }

        private void LoadData()
        {
            try
            {
                // Chỉ lấy Admin và Staff (Bỏ qua Resident)
                string sql = "SELECT id, username, full_name, role, phone FROM users WHERE role != 'resident'";
                DataTable dt = DatabaseHelper.GetData(sql);
                dgvStaff.DataSource = dt;
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

            CreateInput(pnlInput, "Tài Khoản (Username):", out txtUser, 40, 30, 250);
            CreateInput(pnlInput, "Họ Tên Nhân Viên:", out txtName, 320, 30, 300);

            CreateInput(pnlInput, "Chức Vụ (admin/staff):", out txtRole, 40, 100, 250);
            CreateInput(pnlInput, "Số Điện Thoại:", out txtPhone, 320, 100, 300);

            int btnY = 170;

            // --- NÚT THÊM ---
            Button btnAdd = CreateButton("THÊM NHÂN SỰ", Color.FromArgb(24, 161, 251), 40, btnY);
            btnAdd.Click += (s, e) => {
                if (string.IsNullOrWhiteSpace(txtUser.Text)) return;
                try
                {
                    string role = string.IsNullOrWhiteSpace(txtRole.Text) ? "staff" : txtRole.Text;
                    string sql = $"INSERT INTO users (username, password, full_name, role, phone) " +
                                 $"VALUES ('{txtUser.Text}', '123456', '{txtName.Text}', '{role}', '{txtPhone.Text}')";

                    DatabaseHelper.ExecuteSql(sql);
                    MessageBox.Show("Thêm nhân viên thành công!\nMật khẩu mặc định: 123456");
                    LoadData();
                    ClearInput();
                }
                catch (Exception ex) { MessageBox.Show("Lỗi: " + ex.Message); }
            };
            pnlInput.Controls.Add(btnAdd);

            // --- NÚT SỬA ---
            Button btnEdit = CreateButton("SỬA THÔNG TIN", Color.FromArgb(255, 193, 7), 200, btnY);
            btnEdit.Click += (s, e) => {
                if (string.IsNullOrEmpty(txtUser.Text)) return;
                try
                {
                    string sql = $"UPDATE users SET full_name='{txtName.Text}', role='{txtRole.Text}', phone='{txtPhone.Text}' " +
                                 $"WHERE username='{txtUser.Text}'";
                    DatabaseHelper.ExecuteSql(sql);
                    MessageBox.Show("Cập nhật xong!");
                    LoadData();
                    ClearInput();
                }
                catch (Exception ex) { MessageBox.Show("Lỗi: " + ex.Message); }
            };
            pnlInput.Controls.Add(btnEdit);

            // --- NÚT XÓA ---
            Button btnDelete = CreateButton("XÓA NHÂN SỰ", Color.FromArgb(253, 138, 114), 360, btnY);
            btnDelete.Click += (s, e) => {
                if (string.IsNullOrEmpty(txtUser.Text)) return;
                if (MessageBox.Show("Bạn muốn xóa nhân viên này vĩnh viễn?", "Cảnh báo", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
                {
                    try
                    {
                        string sql = $"DELETE FROM users WHERE username='{txtUser.Text}'";
                        DatabaseHelper.ExecuteSql(sql);
                        MessageBox.Show("Đã xóa!");
                        LoadData();
                        ClearInput();
                    }
                    catch (Exception ex) { MessageBox.Show("Lỗi: " + ex.Message); }
                }
            };
            pnlInput.Controls.Add(btnDelete);

            // --- NÚT LÀM MỚI ---
            Button btnClear = CreateButton("LÀM MỚI", Color.Gray, 520, btnY);
            btnClear.Click += (s, e) => ClearInput();
            pnlInput.Controls.Add(btnClear);

            // --- NÚT XUẤT EXCEL (MỚI) ---
            Button btnExcel = CreateButton("XUẤT EXCEL", Color.FromArgb(39, 174, 96), 680, btnY);
            btnExcel.Click += (s, e) => ExportToCSV();
            pnlInput.Controls.Add(btnExcel);
        }

        // --- HÀM XUẤT CSV ---
        private void ExportToCSV()
        {
            if (dgvStaff.Rows.Count == 0)
            {
                MessageBox.Show("Không có dữ liệu để xuất!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "Excel CSV (*.csv)|*.csv";
            sfd.FileName = "DanhSachNhanSu.csv";

            if (sfd.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    using (StreamWriter sw = new StreamWriter(sfd.FileName, false, Encoding.UTF8))
                    {
                        // 1. Ghi tiêu đề
                        string[] header = new string[dgvStaff.Columns.Count];
                        for (int i = 0; i < dgvStaff.Columns.Count; i++)
                        {
                            header[i] = dgvStaff.Columns[i].HeaderText;
                        }
                        sw.WriteLine(string.Join(",", header));

                        // 2. Ghi dữ liệu
                        foreach (DataGridViewRow row in dgvStaff.Rows)
                        {
                            if (!row.IsNewRow)
                            {
                                string[] cells = new string[dgvStaff.Columns.Count];
                                for (int i = 0; i < dgvStaff.Columns.Count; i++)
                                {
                                    string value = row.Cells[i].Value?.ToString() ?? "";
                                    cells[i] = value.Contains(",") ? $"\"{value}\"" : value;
                                }
                                sw.WriteLine(string.Join(",", cells));
                            }
                        }
                    }
                    MessageBox.Show("Xuất file thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    System.Diagnostics.Process.Start(sfd.FileName);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi xuất file: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void ClearInput() { txtUser.Text = ""; txtName.Clear(); txtRole.Clear(); txtPhone.Clear(); txtUser.Focus(); }

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
            if (dgvStaff == null) return;

            dgvStaff.BorderStyle = BorderStyle.None;
            dgvStaff.BackgroundColor = Color.White;
            dgvStaff.RowTemplate.Height = 55;
            dgvStaff.ColumnHeadersHeight = 60;
            dgvStaff.EnableHeadersVisualStyles = false;
            dgvStaff.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(24, 30, 54);
            dgvStaff.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvStaff.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 12, FontStyle.Bold);
            dgvStaff.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvStaff.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvStaff.RowHeadersVisible = false;
            dgvStaff.ReadOnly = true;
            dgvStaff.AllowUserToAddRows = false;

            dgvStaff.CellClick += (s, e) => {
                if (e.RowIndex >= 0)
                {
                    var r = dgvStaff.Rows[e.RowIndex];
                    txtUser.Text = r.Cells["username"].Value.ToString();
                    txtName.Text = r.Cells["full_name"].Value.ToString();
                    txtRole.Text = r.Cells["role"].Value.ToString();
                    txtPhone.Text = r.Cells["phone"].Value.ToString();
                }
            };
        }
    }
}