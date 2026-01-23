using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using System.IO;   // Thêm thư viện để lưu file
using System.Text; // Thêm thư viện để xử lý tiếng Việt

namespace QuanLyToaNha
{
    public partial class FrmBuilding : Form
    {
        private TextBox txtID, txtName, txtAddress, txtDescription, txtFloors;

        public FrmBuilding()
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
                string sql = "SELECT id, name, address, description, total_floors FROM buildings";
                DataTable dt = DatabaseHelper.GetData(sql);
                dgvBuilding.DataSource = dt;
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

            CreateInput(pnlInput, "Mã Tòa (Auto):", out txtID, 40, 30, 100);
            txtID.Enabled = false;

            CreateInput(pnlInput, "Tên Tòa Nhà:", out txtName, 180, 30, 250);
            CreateInput(pnlInput, "Mô tả / Ghi chú:", out txtDescription, 460, 30, 250);

            CreateInput(pnlInput, "Địa Chỉ:", out txtAddress, 40, 100, 400);
            CreateInput(pnlInput, "Tổng Số Tầng:", out txtFloors, 460, 100, 150);

            int btnY = 170;

            // Nút THÊM
            Button btnAdd = CreateButton("THÊM TÒA NHÀ", Color.FromArgb(24, 161, 251), 40, btnY);
            btnAdd.Click += (s, e) => {
                if (string.IsNullOrWhiteSpace(txtName.Text)) return;
                try
                {
                    string sql = $"INSERT INTO buildings (name, address, description, total_floors) VALUES ('{txtName.Text}', '{txtAddress.Text}', '{txtDescription.Text}', '{txtFloors.Text}')";
                    DatabaseHelper.ExecuteSql(sql);
                    MessageBox.Show("Thêm thành công!");
                    LoadData();
                    ClearInput();
                }
                catch (Exception ex) { MessageBox.Show("Lỗi: " + ex.Message); }
            };
            pnlInput.Controls.Add(btnAdd);

            // Nút SỬA
            Button btnEdit = CreateButton("CẬP NHẬT", Color.FromArgb(255, 193, 7), 200, btnY);
            btnEdit.Click += (s, e) => {
                if (string.IsNullOrEmpty(txtID.Text)) return;
                try
                {
                    string sql = $"UPDATE buildings SET name='{txtName.Text}', address='{txtAddress.Text}', description='{txtDescription.Text}', total_floors='{txtFloors.Text}' WHERE id={txtID.Text}";
                    DatabaseHelper.ExecuteSql(sql);
                    MessageBox.Show("Cập nhật xong!");
                    LoadData();
                    ClearInput();
                }
                catch (Exception ex) { MessageBox.Show("Lỗi: " + ex.Message); }
            };
            pnlInput.Controls.Add(btnEdit);

            // Nút XÓA
            Button btnDelete = CreateButton("XÓA TÒA NHÀ", Color.FromArgb(253, 138, 114), 360, btnY);
            btnDelete.Click += (s, e) => {
                if (string.IsNullOrEmpty(txtID.Text)) return;
                if (MessageBox.Show("Bạn chắc chắn muốn xóa?", "Cảnh báo", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    try
                    {
                        string sql = $"DELETE FROM buildings WHERE id={txtID.Text}";
                        DatabaseHelper.ExecuteSql(sql);
                        MessageBox.Show("Đã xóa!");
                        LoadData();
                        ClearInput();
                    }
                    catch (Exception ex) { MessageBox.Show("Lỗi: " + ex.Message); }
                }
            };
            pnlInput.Controls.Add(btnDelete);

            // Nút LÀM MỚI
            Button btnClear = CreateButton("LÀM MỚI", Color.Gray, 520, btnY);
            btnClear.Click += (s, e) => { ClearInput(); LoadData(); };
            pnlInput.Controls.Add(btnClear);

            // --- NÚT XUẤT EXCEL (MỚI) ---
            Button btnExcel = CreateButton("XUẤT EXCEL", Color.FromArgb(39, 174, 96), 680, btnY); // Màu xanh lá Excel
            btnExcel.Click += (s, e) => ExportToCSV(); // Gọi hàm xuất file
            pnlInput.Controls.Add(btnExcel);
        }

        // --- HÀM XUẤT RA FILE CSV (Excel mở được) ---
        private void ExportToCSV()
        {
            if (dgvBuilding.Rows.Count == 0)
            {
                MessageBox.Show("Không có dữ liệu để xuất!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "Excel Documents (*.csv)|*.csv";
            sfd.FileName = "DanhSachToaNha.csv"; // Tên file mặc định

            if (sfd.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    using (StreamWriter sw = new StreamWriter(sfd.FileName, false, Encoding.UTF8))
                    {
                        // 1. Ghi tiêu đề cột
                        string[] header = new string[dgvBuilding.Columns.Count];
                        for (int i = 0; i < dgvBuilding.Columns.Count; i++)
                        {
                            header[i] = dgvBuilding.Columns[i].HeaderText;
                        }
                        sw.WriteLine(string.Join(",", header));

                        // 2. Ghi dữ liệu dòng
                        foreach (DataGridViewRow row in dgvBuilding.Rows)
                        {
                            if (!row.IsNewRow)
                            {
                                string[] cells = new string[dgvBuilding.Columns.Count];
                                for (int i = 0; i < dgvBuilding.Columns.Count; i++)
                                {
                                    // Xử lý nếu dữ liệu có dấu phẩy thì bọc trong ngoặc kép ""
                                    string value = row.Cells[i].Value?.ToString() ?? "";
                                    cells[i] = value.Contains(",") ? $"\"{value}\"" : value;
                                }
                                sw.WriteLine(string.Join(",", cells));
                            }
                        }
                    }
                    MessageBox.Show("Xuất file Excel thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    // Mở file lên luôn
                    System.Diagnostics.Process.Start(sfd.FileName);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi khi xuất file: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void StyleDataGridView()
        {
            dgvBuilding.BorderStyle = BorderStyle.None;
            dgvBuilding.BackgroundColor = Color.White;
            dgvBuilding.RowTemplate.Height = 40;
            dgvBuilding.ColumnHeadersHeight = 50;
            dgvBuilding.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvBuilding.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvBuilding.ReadOnly = true;
            dgvBuilding.AllowUserToAddRows = false;

            dgvBuilding.CellClick += (s, e) => {
                if (e.RowIndex >= 0)
                {
                    var r = dgvBuilding.Rows[e.RowIndex];
                    txtID.Text = r.Cells["id"].Value.ToString();
                    txtName.Text = r.Cells["name"].Value.ToString();
                    txtAddress.Text = r.Cells["address"].Value.ToString();
                    txtDescription.Text = r.Cells["description"].Value.ToString();
                    txtFloors.Text = r.Cells["total_floors"].Value.ToString();
                }
            };
        }

        private void ClearInput() { txtID.Clear(); txtName.Clear(); txtAddress.Clear(); txtDescription.Clear(); txtFloors.Clear(); txtName.Focus(); }

        private void CreateInput(Panel p, string l, out TextBox t, int x, int y, int w)
        {
            p.Controls.Add(new Label { Text = l, Location = new Point(x, y), AutoSize = true, Font = new Font("Segoe UI", 10), ForeColor = Color.DimGray });
            t = new TextBox { Location = new Point(x, y + 25), Size = new Size(w, 30), Font = new Font("Segoe UI", 11) };
            p.Controls.Add(t);
        }

        private Button CreateButton(string t, Color c, int x, int y)
        {
            return new Button { Text = t, BackColor = c, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Size = new Size(140, 40), Location = new Point(x, y), Font = new Font("Segoe UI", 10, FontStyle.Bold), Cursor = Cursors.Hand };
        }
    }
}