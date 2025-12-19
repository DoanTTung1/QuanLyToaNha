using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace QuanLyToaNha
{
    public partial class FrmStaff : Form
    {
        private DataTable dtStaff;
        private TextBox txtUser, txtName, txtRole, txtStatus;

        public FrmStaff()
        {
            InitializeComponent();
            DesignInputPanel();
            InitFakeData();
            StyleDataGridView();
        }

        private void DesignInputPanel()
        {
            // Panel cao 240px cho thoáng (giống form Building/Customer)
            Panel pnlInput = new Panel { Dock = DockStyle.Top, Height = 240, BackColor = Color.WhiteSmoke };
            this.Controls.Add(pnlInput);

            // --- DÒNG 1 ---
            // Tài khoản là định danh duy nhất (như ID)
            CreateInput(pnlInput, "Tài Khoản (Username):", out txtUser, 40, 30, 250);

            // Họ tên hiển thị
            CreateInput(pnlInput, "Họ Tên Nhân Viên:", out txtName, 320, 30, 300);

            // --- DÒNG 2 (Cách 70px) ---
            CreateInput(pnlInput, "Chức Vụ / Phân Quyền:", out txtRole, 40, 100, 250);
            CreateInput(pnlInput, "Trạng Thái Hoạt Động:", out txtStatus, 320, 100, 300);

            // --- HÀNG NÚT BẤM (Hạ xuống Y=170) ---
            int btnY = 170;
            Button btnAdd = CreateButton("THÊM NHÂN SỰ", Color.FromArgb(24, 161, 251), 40, btnY);
            btnAdd.Click += (s, e) => {
                if (string.IsNullOrWhiteSpace(txtUser.Text)) return;
                // Thêm user mới vào bảng
                dtStaff.Rows.Add(txtUser.Text, txtName.Text, txtRole.Text, txtStatus.Text);
                MessageBox.Show("Thêm nhân sự thành công!");
                ClearInput();
            };
            pnlInput.Controls.Add(btnAdd);

            Button btnEdit = CreateButton("SỬA THÔNG TIN", Color.FromArgb(255, 193, 7), 200, btnY);
            btnEdit.Click += (s, e) => {
                if (dgvStaff.SelectedRows.Count == 0) return;
                DataRow r = dtStaff.Rows[dgvStaff.CurrentRow.Index];
                // Không cho sửa Username (txtUser) vì là khóa chính
                r["Name"] = txtName.Text;
                r["Role"] = txtRole.Text;
                r["Status"] = txtStatus.Text;
                MessageBox.Show("Cập nhật xong!");
                ClearInput();
            };
            pnlInput.Controls.Add(btnEdit);

            Button btnDelete = CreateButton("XÓA / KHÓA", Color.FromArgb(253, 138, 114), 360, btnY);
            btnDelete.Click += (s, e) => {
                if (dgvStaff.SelectedRows.Count > 0 && MessageBox.Show("Bạn muốn xóa nhân viên này khỏi hệ thống?", "Cảnh báo", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
                {
                    dtStaff.Rows[dgvStaff.CurrentRow.Index].Delete();
                    ClearInput();
                }
            };
            pnlInput.Controls.Add(btnDelete);

            Button btnClear = CreateButton("LÀM MỚI", Color.Gray, 520, btnY);
            btnClear.Click += (s, e) => ClearInput();
            pnlInput.Controls.Add(btnClear);
        }

        // --- CÁC HÀM HỖ TRỢ ---
        private void InitFakeData()
        {
            dtStaff = new DataTable();
            dtStaff.Columns.Add("User");
            dtStaff.Columns.Add("Name");
            dtStaff.Columns.Add("Role");
            dtStaff.Columns.Add("Status");

            // Dữ liệu mẫu quan trọng
            dtStaff.Rows.Add("admin", "Quản Trị Viên Hệ Thống", "⭐ Quản trị (Admin)", "Đang hoạt động");
            dtStaff.Rows.Add("sale01", "Nguyễn Thị Thu Thảo", "Nhân viên Kinh Doanh", "Đang hoạt động");
            dtStaff.Rows.Add("ketoan", "Trần Văn Tính", "Kế toán trưởng", "Đang hoạt động");
            dtStaff.Rows.Add("baove", "Lê Văn Hùng", "An ninh", "Tạm khóa");

            dgvStaff.DataSource = dtStaff;

            dgvStaff.CellClick += (s, e) => {
                if (e.RowIndex >= 0)
                {
                    var r = dgvStaff.Rows[e.RowIndex];
                    txtUser.Text = r.Cells["User"].Value.ToString();
                    txtName.Text = r.Cells["Name"].Value.ToString();
                    txtRole.Text = r.Cells["Role"].Value.ToString();
                    txtStatus.Text = r.Cells["Status"].Value.ToString();
                }
            };
        }

        private void ClearInput()
        {
            txtUser.Text = "";
            txtName.Clear();
            txtRole.Clear();
            txtStatus.Clear();
            txtUser.Focus();
        }

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

            dgvStaff.Columns["User"].HeaderText = "TÀI KHOẢN";
            dgvStaff.Columns["Name"].HeaderText = "HỌ TÊN NHÂN VIÊN";
            dgvStaff.Columns["Role"].HeaderText = "PHÂN QUYỀN";
            dgvStaff.Columns["Status"].HeaderText = "TRẠNG THÁI";
        }
    }
}