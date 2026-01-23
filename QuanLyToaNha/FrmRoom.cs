using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace QuanLyToaNha
{
    public partial class FrmRoom : Form
    {
        // Chỉ giữ lại TextBox, xóa dgvRoom thừa
        private TextBox txtID, txtBuilding, txtFloor, txtArea, txtPrice, txtStatus;

        public FrmRoom()
        {
            InitializeComponent(); // dgvRoom được tạo ở đây

            DesignInputPanel();

            // Chỉ style lại
            StyleDataGridView();

            LoadData();
        }

        private void LoadData()
        {
            try
            {
                string sql = "SELECT id, building_id, room_number, floor, area, price, status FROM rooms";
                DataTable dt = DatabaseHelper.GetData(sql);
                dgvRoom.DataSource = dt;
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

            CreateInput(pnlInput, "Mã Phòng (Auto):", out txtID, 40, 30, 150);
            txtID.Enabled = false;

            CreateInput(pnlInput, "Mã Tòa Nhà (ID):", out txtBuilding, 220, 30, 250);
            CreateInput(pnlInput, "Tầng:", out txtFloor, 500, 30, 150);

            CreateInput(pnlInput, "Diện Tích (m2):", out txtArea, 40, 100, 150);
            CreateInput(pnlInput, "Giá Thuê ($):", out txtPrice, 220, 100, 250);
            CreateInput(pnlInput, "Trạng Thái:", out txtStatus, 500, 100, 200);
            txtStatus.Text = "Available";

            int btnY = 170;

            Button btnAdd = CreateButton("THÊM PHÒNG", Color.FromArgb(24, 161, 251), 40, btnY);
            btnAdd.Click += (s, e) => {
                if (string.IsNullOrWhiteSpace(txtBuilding.Text)) return;
                try
                {
                    string roomNum = "P" + txtFloor.Text + "-" + new Random().Next(10, 99);
                    string sql = $"INSERT INTO rooms (building_id, room_number, floor, area, price, status) " +
                                 $"VALUES ('{txtBuilding.Text}', '{roomNum}', '{txtFloor.Text}', '{txtArea.Text}', '{txtPrice.Text}', '{txtStatus.Text}')";
                    DatabaseHelper.ExecuteSql(sql);
                    MessageBox.Show("Thêm phòng thành công!");
                    LoadData();
                    ClearInput();
                }
                catch (Exception ex) { MessageBox.Show("Lỗi: " + ex.Message); }
            };
            pnlInput.Controls.Add(btnAdd);

            Button btnEdit = CreateButton("CẬP NHẬT", Color.FromArgb(255, 193, 7), 200, btnY);
            btnEdit.Click += (s, e) => {
                if (string.IsNullOrEmpty(txtID.Text)) return;
                try
                {
                    string sql = $"UPDATE rooms SET floor='{txtFloor.Text}', area='{txtArea.Text}', " +
                                 $"price='{txtPrice.Text}', status='{txtStatus.Text}' WHERE id={txtID.Text}";
                    DatabaseHelper.ExecuteSql(sql);
                    MessageBox.Show("Cập nhật xong!");
                    LoadData();
                    ClearInput();
                }
                catch (Exception ex) { MessageBox.Show("Lỗi: " + ex.Message); }
            };
            pnlInput.Controls.Add(btnEdit);

            Button btnDelete = CreateButton("XÓA PHÒNG", Color.FromArgb(253, 138, 114), 360, btnY);
            btnDelete.Click += (s, e) => {
                if (string.IsNullOrEmpty(txtID.Text)) return;
                if (MessageBox.Show("Xóa phòng này?", "Xác nhận", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    try
                    {
                        string sql = $"DELETE FROM rooms WHERE id={txtID.Text}";
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
            btnClear.Click += (s, e) => ClearInput();
            pnlInput.Controls.Add(btnClear);
        }

        private void ClearInput() { txtID.Clear(); txtBuilding.Clear(); txtFloor.Clear(); txtArea.Clear(); txtPrice.Clear(); txtStatus.Text = "Available"; }

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
            dgvRoom.BorderStyle = BorderStyle.None;
            dgvRoom.BackgroundColor = Color.White;
            dgvRoom.RowTemplate.Height = 50;
            dgvRoom.ColumnHeadersHeight = 50;
            dgvRoom.EnableHeadersVisualStyles = false;
            dgvRoom.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(24, 30, 54);
            dgvRoom.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvRoom.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 11, FontStyle.Bold);
            dgvRoom.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvRoom.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvRoom.RowHeadersVisible = false;
            dgvRoom.ReadOnly = true;
            dgvRoom.AllowUserToAddRows = false;

            dgvRoom.CellClick += (s, e) => {
                if (e.RowIndex >= 0)
                {
                    var r = dgvRoom.Rows[e.RowIndex];
                    txtID.Text = r.Cells["id"].Value.ToString();
                    txtBuilding.Text = r.Cells["building_id"].Value.ToString();
                    txtFloor.Text = r.Cells["floor"].Value.ToString();
                    txtArea.Text = r.Cells["area"].Value.ToString();
                    txtPrice.Text = r.Cells["price"].Value.ToString();
                    txtStatus.Text = r.Cells["status"].Value.ToString();
                }
            };
        }
    }
}