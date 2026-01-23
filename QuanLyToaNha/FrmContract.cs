using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace QuanLyToaNha
{
    public partial class FrmContract : Form
    {
        // CHỈ KHAI BÁO TEXTBOX, KHÔNG KHAI BÁO GRIDVIEW (Vì Designer đã có rồi)
        private TextBox txtCode, txtCustomer, txtRoom, txtStart, txtEnd, txtDeposit;

        public FrmContract()
        {
            InitializeComponent(); // dgvContract được tạo ra ở đây

            DesignInputPanel();

            // Chỉ gọi hàm Style, không tạo mới GridView
            StyleDataGridView();

            LoadData();
        }

        private void LoadData()
        {
            try
            {
                // CÂU LỆNH SQL NÂNG CẤP:
                // Lấy thông tin từ bảng contracts và lấy thêm Tên Cư Dân từ bảng users
                string sql = @"
                    SELECT 
                        c.id, 
                        c.customer_id, 
                        u.full_name as 'TenKhach', 
                        c.room_id, 
                        c.start_date, 
                        c.end_date, 
                        c.deposit, 
                        c.status 
                    FROM contracts c
                    LEFT JOIN users u ON c.customer_id = u.id";

                DataTable dt = DatabaseHelper.GetData(sql);
                dgvContract.DataSource = dt; // Sử dụng biến dgvContract có sẵn
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

            CreateInput(pnlInput, "Số Hợp Đồng (Auto):", out txtCode, 40, 30, 200);
            txtCode.Enabled = false;

            // Nhập ID của User (Cư dân) lấy từ bảng Users
            CreateInput(pnlInput, "Mã Cư Dân (User ID):", out txtCustomer, 270, 30, 250);
            CreateInput(pnlInput, "Mã Phòng (Room ID):", out txtRoom, 550, 30, 150);

            CreateInput(pnlInput, "Ngày Bắt Đầu (yyyy-MM-dd):", out txtStart, 40, 100, 200);
            CreateInput(pnlInput, "Ngày Kết Thúc (yyyy-MM-dd):", out txtEnd, 270, 100, 250);
            CreateInput(pnlInput, "Tiền Cọc:", out txtDeposit, 550, 100, 200);

            int btnY = 170;

            Button btnAdd = CreateButton("LẬP HỢP ĐỒNG", Color.FromArgb(24, 161, 251), 40, btnY);
            btnAdd.Click += (s, e) => {
                if (string.IsNullOrWhiteSpace(txtCustomer.Text) || string.IsNullOrWhiteSpace(txtRoom.Text)) return;

                // Kiểm tra User ID có tồn tại và là Resident không
                if (!CheckUserExists(txtCustomer.Text))
                {
                    MessageBox.Show("Mã Cư Dân không tồn tại hoặc không đúng quyền!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                try
                {
                    string sql = $"INSERT INTO contracts (customer_id, room_id, start_date, end_date, deposit, status) " +
                                 $"VALUES ('{txtCustomer.Text}', '{txtRoom.Text}', '{txtStart.Text}', '{txtEnd.Text}', '{txtDeposit.Text}', 'Active')";

                    DatabaseHelper.ExecuteSql(sql);
                    MessageBox.Show("Lập hợp đồng thành công!");
                    LoadData();
                    ClearInput();
                }
                catch (Exception ex) { MessageBox.Show("Lỗi: " + ex.Message); }
            };
            pnlInput.Controls.Add(btnAdd);

            Button btnEdit = CreateButton("CẬP NHẬT HĐ", Color.FromArgb(255, 193, 7), 200, btnY);
            btnEdit.Click += (s, e) => {
                if (string.IsNullOrEmpty(txtCode.Text)) return;
                try
                {
                    string sql = $"UPDATE contracts SET customer_id='{txtCustomer.Text}', room_id='{txtRoom.Text}', " +
                                 $"start_date='{txtStart.Text}', end_date='{txtEnd.Text}', deposit='{txtDeposit.Text}' " +
                                 $"WHERE id={txtCode.Text}";

                    DatabaseHelper.ExecuteSql(sql);
                    MessageBox.Show("Đã cập nhật!");
                    LoadData();
                    ClearInput();
                }
                catch (Exception ex) { MessageBox.Show("Lỗi: " + ex.Message); }
            };
            pnlInput.Controls.Add(btnEdit);

            Button btnDelete = CreateButton("HỦY HỢP ĐỒNG", Color.FromArgb(253, 138, 114), 360, btnY);
            btnDelete.Click += (s, e) => {
                if (string.IsNullOrEmpty(txtCode.Text)) return;
                if (MessageBox.Show("Hủy hợp đồng này?", "Xác nhận", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    try
                    {
                        string sql = $"DELETE FROM contracts WHERE id={txtCode.Text}";
                        DatabaseHelper.ExecuteSql(sql);
                        MessageBox.Show("Đã xóa!");
                        LoadData();
                        ClearInput();
                    }
                    catch (Exception ex) { MessageBox.Show("Lỗi: " + ex.Message); }
                }
            };
            pnlInput.Controls.Add(btnDelete);

            Button btnClear = CreateButton("LÀM MỚI", Color.Gray, 520, btnY);
            btnClear.Click += (s, e) => { ClearInput(); LoadData(); };
            pnlInput.Controls.Add(btnClear);
        }

        // Hàm kiểm tra User có tồn tại không (Tránh lỗi khóa ngoại)
        private bool CheckUserExists(string userId)
        {
            try
            {
                // Chỉ cho phép tạo hợp đồng với người có role là resident
                string sql = $"SELECT COUNT(*) FROM users WHERE id = {userId} AND role = 'resident'";
                DataTable dt = DatabaseHelper.GetData(sql);
                if (dt != null && dt.Rows.Count > 0)
                {
                    return Convert.ToInt32(dt.Rows[0][0]) > 0;
                }
            }
            catch { }
            return false;
        }

        private void ClearInput() { txtCode.Clear(); txtCustomer.Clear(); txtRoom.Clear(); txtStart.Clear(); txtEnd.Clear(); txtDeposit.Clear(); }

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
            dgvContract.BorderStyle = BorderStyle.None;
            dgvContract.BackgroundColor = Color.White;
            dgvContract.RowTemplate.Height = 50;
            dgvContract.ColumnHeadersHeight = 50;
            dgvContract.EnableHeadersVisualStyles = false;
            dgvContract.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(24, 30, 54);
            dgvContract.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvContract.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 11, FontStyle.Bold);
            dgvContract.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvContract.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvContract.RowHeadersVisible = false;
            dgvContract.ReadOnly = true;
            dgvContract.AllowUserToAddRows = false;

            dgvContract.CellClick += (s, e) => {
                if (e.RowIndex >= 0)
                {
                    var r = dgvContract.Rows[e.RowIndex];
                    txtCode.Text = r.Cells["id"].Value.ToString();
                    txtCustomer.Text = r.Cells["customer_id"].Value.ToString();
                    txtRoom.Text = r.Cells["room_id"].Value.ToString();

                    if (r.Cells["start_date"].Value != DBNull.Value)
                        txtStart.Text = Convert.ToDateTime(r.Cells["start_date"].Value).ToString("yyyy-MM-dd");

                    if (r.Cells["end_date"].Value != DBNull.Value)
                        txtEnd.Text = Convert.ToDateTime(r.Cells["end_date"].Value).ToString("yyyy-MM-dd");

                    txtDeposit.Text = r.Cells["deposit"].Value.ToString();
                }
            };
        }
    }
}