using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using System.Globalization; // Quan trọng để xử lý số tiền

namespace QuanLyToaNha
{
    public partial class FrmService : Form
    {
        private TextBox txtID, txtName, txtPrice, txtUnit;
        private DataGridView dgvService;

        public FrmService()
        {
            InitializeComponent();
            this.Controls.Clear();
            this.BackColor = Color.White;

            DesignInputPanel();
            StyleDataGridView();
            LoadData();
        }

        // --- VALIDATION (BẮT LỖI) ---
        private bool ValidateInput()
        {
            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                MessageBox.Show("Vui lòng nhập Tên Dịch Vụ!", "Thiếu thông tin", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtName.Focus();
                return false;
            }
            if (string.IsNullOrWhiteSpace(txtPrice.Text))
            {
                MessageBox.Show("Vui lòng nhập Đơn Giá!", "Thiếu thông tin", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtPrice.Focus();
                return false;
            }

            // Kiểm tra giá phải là số hợp lệ
            if (!decimal.TryParse(txtPrice.Text, out decimal price) || price < 0)
            {
                MessageBox.Show("Đơn giá phải là số dương hợp lệ!", "Lỗi định dạng", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtPrice.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtUnit.Text))
            {
                MessageBox.Show("Vui lòng nhập Đơn Vị Tính (VD: kWh, m3, tháng)!", "Thiếu thông tin", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtUnit.Focus();
                return false;
            }
            return true;
        }

        private void LoadData()
        {
            try
            {
                // Lấy dữ liệu từ bảng services (Cấu trúc chuẩn theo DB)
                string sql = "SELECT id, name, unit_price, unit_name FROM services";
                DataTable dt = DatabaseHelper.GetData(sql);
                dgvService.DataSource = dt;

                // Đổi tên cột hiển thị
                if (dgvService.Columns["id"] != null) dgvService.Columns["id"].HeaderText = "Mã DV";
                if (dgvService.Columns["name"] != null) dgvService.Columns["name"].HeaderText = "Tên Dịch Vụ";
                if (dgvService.Columns["unit_price"] != null)
                {
                    dgvService.Columns["unit_price"].HeaderText = "Đơn Giá";
                    dgvService.Columns["unit_price"].DefaultCellStyle.Format = "N0"; // Format tiền 100,000
                }
                if (dgvService.Columns["unit_name"] != null) dgvService.Columns["unit_name"].HeaderText = "Đơn Vị Tính";
            }
            catch (Exception ex) { MessageBox.Show("Lỗi tải dữ liệu: " + ex.Message); }
        }

        // --- CRUD ACTIONS ---
        private void BtnAdd_Click(object sender, EventArgs e)
        {
            if (!ValidateInput()) return;

            try
            {
                // Xử lý giá tiền để tránh lỗi dấu phẩy/chấm
                decimal price = decimal.Parse(txtPrice.Text);
                string priceStr = price.ToString(CultureInfo.InvariantCulture); // Chuyển về dạng 350000.00 (dấu chấm)

                string sql = $"INSERT INTO services (name, unit_price, unit_name) VALUES ('{txtName.Text}', {priceStr}, '{txtUnit.Text}')";

                DatabaseHelper.ExecuteSql(sql);
                MessageBox.Show("Thêm dịch vụ thành công!", "Thông báo");
                LoadData(); ClearInput();
            }
            catch (Exception ex) { MessageBox.Show("Lỗi: " + ex.Message); }
        }

        private void BtnEdit_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtID.Text)) { MessageBox.Show("Vui lòng chọn dịch vụ cần sửa!", "Chưa chọn", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
            if (!ValidateInput()) return;

            try
            {
                decimal price = decimal.Parse(txtPrice.Text);
                string priceStr = price.ToString(CultureInfo.InvariantCulture);

                string sql = $"UPDATE services SET name='{txtName.Text}', unit_price={priceStr}, unit_name='{txtUnit.Text}' WHERE id={txtID.Text}";

                DatabaseHelper.ExecuteSql(sql);
                MessageBox.Show("Cập nhật thành công!", "Thông báo");
                LoadData(); ClearInput();
            }
            catch (Exception ex) { MessageBox.Show("Lỗi: " + ex.Message); }
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtID.Text)) return;
            if (MessageBox.Show("Xóa dịch vụ này sẽ ảnh hưởng đến các hóa đơn cũ.\nBạn chắc chắn muốn xóa?", "Cảnh báo", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                try
                {
                    DatabaseHelper.ExecuteSql($"DELETE FROM services WHERE id={txtID.Text}");
                    MessageBox.Show("Đã xóa!", "Thông báo");
                    LoadData(); ClearInput();
                }
                catch { MessageBox.Show("Dịch vụ đang được sử dụng trong Hợp đồng/Hóa đơn, không thể xóa!", "Lỗi ràng buộc"); }
            }
        }

        // --- UI & HELPERS ---
        private void ClearInput() { txtID.Clear(); txtName.Clear(); txtPrice.Clear(); txtUnit.Clear(); txtName.Focus(); }

        private void DesignInputPanel()
        {
            Panel pnlInput = new Panel { Dock = DockStyle.Top, Height = 180, BackColor = Color.WhiteSmoke };
            this.Controls.Add(pnlInput);

            CreateInput(pnlInput, "Mã DV:", out txtID, 40, 20, 80); txtID.Enabled = false;
            CreateInput(pnlInput, "Tên Dịch Vụ (VD: Điện):", out txtName, 150, 20, 250);

            CreateInput(pnlInput, "Đơn Giá (VNĐ):", out txtPrice, 430, 20, 150);
            // Chỉ cho nhập số
            txtPrice.KeyPress += (s, e) => {
                if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar)) e.Handled = true;
            };

            CreateInput(pnlInput, "Đơn Vị Tính (kWh, m3...):", out txtUnit, 610, 20, 150);

            // Buttons
            int btnY = 100;
            Button btnAdd = CreateButton("THÊM DỊCH VỤ", Color.FromArgb(24, 161, 251), 40, btnY);
            btnAdd.Click += BtnAdd_Click;
            pnlInput.Controls.Add(btnAdd);

            Button btnEdit = CreateButton("CẬP NHẬT GIÁ", Color.FromArgb(255, 193, 7), 200, btnY);
            btnEdit.Click += BtnEdit_Click;
            pnlInput.Controls.Add(btnEdit);

            Button btnDelete = CreateButton("XÓA", Color.IndianRed, 360, btnY);
            btnDelete.Click += BtnDelete_Click;
            pnlInput.Controls.Add(btnDelete);

            Button btnRefresh = CreateButton("LÀM MỚI", Color.Gray, 520, btnY);
            btnRefresh.Click += (s, e) => { ClearInput(); LoadData(); };
            pnlInput.Controls.Add(btnRefresh);
        }

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
            dgvService = new DataGridView();
            dgvService.Dock = DockStyle.Fill;
            this.Controls.Add(dgvService);
            dgvService.BringToFront();

            dgvService.BorderStyle = BorderStyle.None;
            dgvService.BackgroundColor = Color.White;
            dgvService.RowTemplate.Height = 40;
            dgvService.ColumnHeadersHeight = 50;
            dgvService.EnableHeadersVisualStyles = false;
            dgvService.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(24, 30, 54);
            dgvService.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvService.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 11, FontStyle.Bold);
            dgvService.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvService.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvService.ReadOnly = true;
            dgvService.AllowUserToAddRows = false;
            dgvService.RowHeadersVisible = false;

            dgvService.CellClick += (s, e) => {
                if (e.RowIndex >= 0)
                {
                    var r = dgvService.Rows[e.RowIndex];
                    txtID.Text = r.Cells["id"].Value.ToString();
                    txtName.Text = r.Cells["name"].Value.ToString();

                    // Lấy giá trị gốc (decimal) rồi convert sang int cho đẹp (bỏ số lẻ .00)
                    if (r.Cells["unit_price"].Value != DBNull.Value)
                    {
                        decimal val = Convert.ToDecimal(r.Cells["unit_price"].Value);
                        txtPrice.Text = val.ToString("0");
                    }

                    txtUnit.Text = r.Cells["unit_name"].Value.ToString();
                }
            };
        }
    }
}