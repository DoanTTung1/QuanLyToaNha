using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace QuanLyToaNha
{
    public partial class FrmCustomer : Form
    {
        private DataTable dtCustomer;
        private TextBox txtID, txtName, txtPhone, txtCompany, txtEmail;

        public FrmCustomer()
        {
            InitializeComponent();
            DesignInputPanel();
            InitFakeData();
            StyleDataGridView();
        }

        private void DesignInputPanel()
        {
            Panel pnlInput = new Panel { Dock = DockStyle.Top, Height = 240, BackColor = Color.WhiteSmoke };
            this.Controls.Add(pnlInput);

            // --- DÒNG 1 ---
            CreateInput(pnlInput, "Mã KH:", out txtID, 40, 30, 100); txtID.Enabled = false; txtID.Text = "AUTO";
            CreateInput(pnlInput, "Họ Tên Khách:", out txtName, 180, 30, 250);
            CreateInput(pnlInput, "Số Điện Thoại:", out txtPhone, 460, 30, 200);

            // --- DÒNG 2 ---
            CreateInput(pnlInput, "Công Ty / Đơn Vị:", out txtCompany, 40, 100, 390);
            CreateInput(pnlInput, "Email Liên Hệ:", out txtEmail, 460, 100, 250);

            // --- NÚT BẤM ---
            int btnY = 170;
            Button btnAdd = CreateButton("THÊM KHÁCH", Color.FromArgb(24, 161, 251), 40, btnY);
            btnAdd.Click += (s, e) => {
                if (string.IsNullOrWhiteSpace(txtName.Text)) return;
                dtCustomer.Rows.Add("KH" + new Random().Next(1000, 9999), txtName.Text, txtPhone.Text, txtCompany.Text, txtEmail.Text);
                ClearInput();
            };
            pnlInput.Controls.Add(btnAdd);

            Button btnEdit = CreateButton("CẬP NHẬT", Color.FromArgb(255, 193, 7), 200, btnY);
            btnEdit.Click += (s, e) => {
                if (dgvCustomer.SelectedRows.Count == 0) return;
                DataRow r = dtCustomer.Rows[dgvCustomer.CurrentRow.Index];
                r["Name"] = txtName.Text; r["Phone"] = txtPhone.Text;
                r["Company"] = txtCompany.Text; r["Email"] = txtEmail.Text;
                MessageBox.Show("Đã lưu!"); ClearInput();
            };
            pnlInput.Controls.Add(btnEdit);

            Button btnDelete = CreateButton("XÓA KHÁCH", Color.FromArgb(253, 138, 114), 360, btnY);
            btnDelete.Click += (s, e) => {
                if (dgvCustomer.SelectedRows.Count > 0 && MessageBox.Show("Xóa khách này?", "Confirm", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    dtCustomer.Rows[dgvCustomer.CurrentRow.Index].Delete(); ClearInput();
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
            dtCustomer = new DataTable();
            dtCustomer.Columns.Add("ID"); dtCustomer.Columns.Add("Name");
            dtCustomer.Columns.Add("Phone"); dtCustomer.Columns.Add("Company"); dtCustomer.Columns.Add("Email");
            dtCustomer.Rows.Add("KH001", "Nguyễn Văn A", "0909.111.222", "FPT Software", "a.nguyen@fpt.com");
            dtCustomer.Rows.Add("KH002", "Trần Thị B", "0912.333.444", "Viettel Global", "b.tran@viettel.com");
            dgvCustomer.DataSource = dtCustomer;

            dgvCustomer.CellClick += (s, e) => {
                if (e.RowIndex >= 0)
                {
                    var r = dgvCustomer.Rows[e.RowIndex];
                    txtID.Text = r.Cells["ID"].Value.ToString(); txtName.Text = r.Cells["Name"].Value.ToString();
                    txtPhone.Text = r.Cells["Phone"].Value.ToString(); txtCompany.Text = r.Cells["Company"].Value.ToString();
                    txtEmail.Text = r.Cells["Email"].Value.ToString();
                }
            };
        }
        private void ClearInput() { txtID.Text = "AUTO"; txtName.Clear(); txtPhone.Clear(); txtCompany.Clear(); txtEmail.Clear(); txtName.Focus(); }

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
            dgvCustomer.BorderStyle = BorderStyle.None; dgvCustomer.BackgroundColor = Color.White;
            dgvCustomer.RowTemplate.Height = 55; dgvCustomer.ColumnHeadersHeight = 60;
            dgvCustomer.EnableHeadersVisualStyles = false;
            dgvCustomer.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(24, 30, 54);
            dgvCustomer.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvCustomer.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 12, FontStyle.Bold);
            dgvCustomer.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvCustomer.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvCustomer.RowHeadersVisible = false; dgvCustomer.ReadOnly = true;
            dgvCustomer.Columns["ID"].HeaderText = "MÃ KH"; dgvCustomer.Columns["Name"].HeaderText = "TÊN KHÁCH";
            dgvCustomer.Columns["Phone"].HeaderText = "SỐ ĐIỆN THOẠI"; dgvCustomer.Columns["Company"].HeaderText = "CÔNG TY";
            dgvCustomer.Columns["Email"].HeaderText = "EMAIL LIÊN HỆ";
        }
    }
}