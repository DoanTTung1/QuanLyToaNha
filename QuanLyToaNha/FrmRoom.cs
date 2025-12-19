using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace QuanLyToaNha
{
    public partial class FrmRoom : Form
    {
        private DataTable dtRoom;
        private TextBox txtID, txtBuilding, txtFloor, txtArea, txtPrice, txtStatus;

        public FrmRoom()
        {
            InitializeComponent();
            DesignInputPanel();
            InitFakeData();
            StyleDataGridView();
        }

        private void DesignInputPanel()
        {
            // Tăng chiều cao lên 240 cho thoáng
            Panel pnlInput = new Panel { Dock = DockStyle.Top, Height = 240, BackColor = Color.WhiteSmoke };
            this.Controls.Add(pnlInput);

            // --- DÒNG 1 ---
            CreateInput(pnlInput, "Mã Phòng:", out txtID, 40, 30, 150);
            txtID.Enabled = false; txtID.Text = "AUTO";

            CreateInput(pnlInput, "Thuộc Tòa Nhà:", out txtBuilding, 220, 30, 250);
            CreateInput(pnlInput, "Tầng:", out txtFloor, 500, 30, 150);

            // --- DÒNG 2 (Cách dòng 1 70px) ---
            CreateInput(pnlInput, "Diện Tích:", out txtArea, 40, 100, 150);
            CreateInput(pnlInput, "Giá Thuê:", out txtPrice, 220, 100, 250);
            CreateInput(pnlInput, "Trạng Thái:", out txtStatus, 500, 100, 200);

            // --- HÀNG NÚT BẤM (Cách dòng 2 70px) ---
            int btnY = 170; // Tọa độ Y của nút

            Button btnAdd = CreateButton("THÊM PHÒNG", Color.FromArgb(24, 161, 251), 40, btnY);
            btnAdd.Click += (s, e) => {
                if (string.IsNullOrWhiteSpace(txtBuilding.Text)) return;
                dtRoom.Rows.Add("R" + new Random().Next(100, 999), txtBuilding.Text, txtFloor.Text, txtArea.Text, txtPrice.Text, txtStatus.Text);
                ClearInput();
            };
            pnlInput.Controls.Add(btnAdd);

            Button btnEdit = CreateButton("CẬP NHẬT", Color.FromArgb(255, 193, 7), 200, btnY); // Cách nút trước 160px
            btnEdit.Click += (s, e) => {
                if (dgvRoom.SelectedRows.Count == 0) return;
                DataRow row = dtRoom.Rows[dgvRoom.CurrentRow.Index];
                row["Building"] = txtBuilding.Text; row["Floor"] = txtFloor.Text;
                row["Area"] = txtArea.Text; row["Price"] = txtPrice.Text; row["Status"] = txtStatus.Text;
                MessageBox.Show("Đã cập nhật!"); ClearInput();
            };
            pnlInput.Controls.Add(btnEdit);

            Button btnDelete = CreateButton("XÓA PHÒNG", Color.FromArgb(253, 138, 114), 360, btnY);
            btnDelete.Click += (s, e) => {
                if (dgvRoom.SelectedRows.Count > 0 && MessageBox.Show("Xóa phòng này?", "Xác nhận", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    dtRoom.Rows[dgvRoom.CurrentRow.Index].Delete(); ClearInput();
                }
            };
            pnlInput.Controls.Add(btnDelete);

            Button btnClear = CreateButton("LÀM MỚI", Color.Gray, 520, btnY);
            btnClear.Click += (s, e) => ClearInput();
            pnlInput.Controls.Add(btnClear);
        }

        // --- GIỮ NGUYÊN CODE DƯỚI ---
        private void InitFakeData()
        {
            dtRoom = new DataTable();
            dtRoom.Columns.Add("ID"); dtRoom.Columns.Add("Building"); dtRoom.Columns.Add("Floor");
            dtRoom.Columns.Add("Area"); dtRoom.Columns.Add("Price"); dtRoom.Columns.Add("Status");
            dtRoom.Rows.Add("R101", "Landmark 81", "Tầng 1", "100m2", "$2,000", "✅ Còn trống");
            dtRoom.Rows.Add("R102", "Landmark 81", "Tầng 1", "150m2", "$3,000", "⛔ Đã thuê");
            dgvRoom.DataSource = dtRoom;
            dgvRoom.CellClick += (s, e) => {
                if (e.RowIndex >= 0)
                {
                    var r = dgvRoom.Rows[e.RowIndex];
                    txtID.Text = r.Cells["ID"].Value.ToString(); txtBuilding.Text = r.Cells["Building"].Value.ToString();
                    txtFloor.Text = r.Cells["Floor"].Value.ToString(); txtArea.Text = r.Cells["Area"].Value.ToString();
                    txtPrice.Text = r.Cells["Price"].Value.ToString(); txtStatus.Text = r.Cells["Status"].Value.ToString();
                }
            };
        }
        private void ClearInput() { txtID.Text = "AUTO"; txtBuilding.Clear(); txtFloor.Clear(); txtArea.Clear(); txtPrice.Clear(); txtStatus.Clear(); }
        private void CreateInput(Panel p, string l, out TextBox t, int x, int y, int w)
        {
            p.Controls.Add(new Label { Text = l, Location = new Point(x, y), AutoSize = true, Font = new Font("Segoe UI", 10, FontStyle.Regular), ForeColor = Color.DimGray });
            t = new TextBox { Location = new Point(x, y + 28), Size = new Size(w, 35), Font = new Font("Segoe UI", 11) }; // Ô nhập cao hơn chút
            p.Controls.Add(t);
        }
        private Button CreateButton(string t, Color c, int x, int y)
        {
            return new Button { Text = t, BackColor = c, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Size = new Size(140, 40), Location = new Point(x, y), Font = new Font("Segoe UI", 10, FontStyle.Bold), Cursor = Cursors.Hand };
        }
        private void StyleDataGridView()
        {
            dgvRoom.BorderStyle = BorderStyle.None; dgvRoom.BackgroundColor = Color.White;
            dgvRoom.RowTemplate.Height = 50; dgvRoom.ColumnHeadersHeight = 50;
            dgvRoom.EnableHeadersVisualStyles = false;
            dgvRoom.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(24, 30, 54);
            dgvRoom.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvRoom.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 11, FontStyle.Bold);
            dgvRoom.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvRoom.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvRoom.RowHeadersVisible = false; dgvRoom.ReadOnly = true;
            dgvRoom.Columns["ID"].HeaderText = "MÃ"; dgvRoom.Columns["Building"].HeaderText = "TÒA NHÀ";
            dgvRoom.Columns["Floor"].HeaderText = "TẦNG"; dgvRoom.Columns["Area"].HeaderText = "DIỆN TÍCH";
            dgvRoom.Columns["Price"].HeaderText = "GIÁ"; dgvRoom.Columns["Status"].HeaderText = "TRẠNG THÁI";
        }
    }
}