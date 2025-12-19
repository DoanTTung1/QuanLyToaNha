using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace QuanLyToaNha
{
    public partial class FrmContract : Form
    {
        private DataTable dtContract;
        private TextBox txtCode, txtCustomer, txtRoom, txtStart, txtEnd, txtDeposit;

        public FrmContract()
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

            // Dòng 1
            CreateInput(pnlInput, "Số Hợp Đồng:", out txtCode, 40, 30, 200); txtCode.Enabled = false; txtCode.Text = "AUTO";
            CreateInput(pnlInput, "Khách Hàng:", out txtCustomer, 270, 30, 250);
            CreateInput(pnlInput, "Phòng Thuê:", out txtRoom, 550, 30, 150);

            // Dòng 2
            CreateInput(pnlInput, "Ngày Bắt Đầu:", out txtStart, 40, 100, 200);
            CreateInput(pnlInput, "Ngày Kết Thúc:", out txtEnd, 270, 100, 250);
            CreateInput(pnlInput, "Tiền Cọc:", out txtDeposit, 550, 100, 200);

            // Nút bấm (Rộng rãi)
            int btnY = 170;
            Button btnAdd = CreateButton("LẬP HỢP ĐỒNG", Color.FromArgb(24, 161, 251), 40, btnY);
            btnAdd.Click += (s, e) => {
                dtContract.Rows.Add("HĐ-" + new Random().Next(1000, 9999), txtCustomer.Text, txtRoom.Text, txtStart.Text, txtEnd.Text, txtDeposit.Text, "🟢 Hiệu lực"); ClearInput();
            };
            pnlInput.Controls.Add(btnAdd);

            Button btnEdit = CreateButton("CẬP NHẬT HĐ", Color.FromArgb(255, 193, 7), 200, btnY);
            btnEdit.Click += (s, e) => {
                if (dgvContract.SelectedRows.Count == 0) return;
                DataRow r = dtContract.Rows[dgvContract.CurrentRow.Index];
                r["Customer"] = txtCustomer.Text; r["Room"] = txtRoom.Text;
                r["StartDate"] = txtStart.Text; r["EndDate"] = txtEnd.Text; r["Deposit"] = txtDeposit.Text;
                MessageBox.Show("Đã lưu!"); ClearInput();
            };
            pnlInput.Controls.Add(btnEdit);

            Button btnDelete = CreateButton("HỦY HỢP ĐỒNG", Color.FromArgb(253, 138, 114), 360, btnY);
            btnDelete.Click += (s, e) => { if (dgvContract.SelectedRows.Count > 0) dtContract.Rows[dgvContract.CurrentRow.Index].Delete(); };
            pnlInput.Controls.Add(btnDelete);
        }

        // --- CÁC HÀM DƯỚI GIỮ NGUYÊN (Copy lại cho chắc) ---
        private void InitFakeData()
        {
            dtContract = new DataTable();
            dtContract.Columns.Add("Code"); dtContract.Columns.Add("Customer"); dtContract.Columns.Add("Room");
            dtContract.Columns.Add("StartDate"); dtContract.Columns.Add("EndDate"); dtContract.Columns.Add("Deposit"); dtContract.Columns.Add("Status");
            dtContract.Rows.Add("HĐ-2024-001", "Nguyễn Văn A", "R102", "01/01/2024", "01/01/2025", "50 Triệu", "🟢 Hiệu lực");
            dgvContract.DataSource = dtContract;
            dgvContract.CellClick += (s, e) => {
                if (e.RowIndex >= 0)
                {
                    var r = dgvContract.Rows[e.RowIndex];
                    txtCode.Text = r.Cells["Code"].Value.ToString(); txtCustomer.Text = r.Cells["Customer"].Value.ToString();
                    txtRoom.Text = r.Cells["Room"].Value.ToString(); txtStart.Text = r.Cells["StartDate"].Value.ToString();
                    txtEnd.Text = r.Cells["EndDate"].Value.ToString(); txtDeposit.Text = r.Cells["Deposit"].Value.ToString();
                }
            };
        }
        private void ClearInput() { txtCode.Text = "AUTO"; txtCustomer.Clear(); txtRoom.Clear(); txtStart.Clear(); txtEnd.Clear(); txtDeposit.Clear(); }
        private void CreateInput(Panel p, string l, out TextBox t, int x, int y, int w)
        {
            p.Controls.Add(new Label { Text = l, Location = new Point(x, y), AutoSize = true, Font = new Font("Segoe UI", 10, FontStyle.Regular), ForeColor = Color.DimGray });
            t = new TextBox { Location = new Point(x, y + 28), Size = new Size(w, 35), Font = new Font("Segoe UI", 11) };
            p.Controls.Add(t);
        }
        private Button CreateButton(string t, Color c, int x, int y)
        {
            return new Button { Text = t, BackColor = c, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Size = new Size(140, 40), Location = new Point(x, y), Font = new Font("Segoe UI", 10, FontStyle.Bold), Cursor = Cursors.Hand };
        }
        private void StyleDataGridView()
        {
            dgvContract.BorderStyle = BorderStyle.None; dgvContract.BackgroundColor = Color.White;
            dgvContract.RowTemplate.Height = 50; dgvContract.ColumnHeadersHeight = 50;
            dgvContract.EnableHeadersVisualStyles = false;
            dgvContract.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(24, 30, 54);
            dgvContract.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvContract.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 11, FontStyle.Bold);
            dgvContract.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvContract.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvContract.RowHeadersVisible = false; dgvContract.ReadOnly = true;
            dgvContract.Columns["Code"].HeaderText = "SỐ HĐ"; dgvContract.Columns["Customer"].HeaderText = "KHÁCH HÀNG";
            dgvContract.Columns["Room"].HeaderText = "PHÒNG"; dgvContract.Columns["StartDate"].HeaderText = "BẮT ĐẦU";
            dgvContract.Columns["EndDate"].HeaderText = "KẾT THÚC"; dgvContract.Columns["Deposit"].HeaderText = "CỌC";
        }
    }
}