using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Globalization; // QUAN TRỌNG: Thư viện để xử lý định dạng số

namespace QuanLyToaNha
{
    public partial class FrmRoom : Form
    {
        private TextBox txtID, txtFloor, txtArea, txtPrice, txtRoomNumber;
        private ComboBox cbbBuilding, cbbStatus;
        

        private Dictionary<string, string> statusDict = new Dictionary<string, string>()
        {
            { "Available", "Còn Trống" },
            { "Occupied", "Đang Thuê" },
            { "Maintenance", "Bảo Trì" }
        };

        public FrmRoom()
        {
            InitializeComponent();
            this.Controls.Clear();
            this.BackColor = Color.White;

            DesignInputPanel();
            StyleDataGridView();
            LoadCombos();
            LoadData();
        }

        // --- LOAD DỮ LIỆU ---
        private void LoadData()
        {
            try
            {
                string sql = @"
                    SELECT r.id, b.name as 'Tòa Nhà', r.room_number as 'Số Phòng', 
                           r.floor as 'Tầng', r.area as 'Diện Tích', 
                           r.price as 'Giá Thuê', r.status as 'Trạng Thái Gốc' 
                    FROM rooms r
                    JOIN buildings b ON r.building_id = b.id
                    ORDER BY b.name, r.floor, r.room_number";

                DataTable dt = DatabaseHelper.GetData(sql);
                dt.Columns.Add("Trạng Thái", typeof(string));
                foreach (DataRow row in dt.Rows)
                {
                    string enStatus = row["Trạng Thái Gốc"].ToString();
                    row["Trạng Thái"] = statusDict.ContainsKey(enStatus) ? statusDict[enStatus] : enStatus;
                }

                dgvRoom.DataSource = dt;

                if (dgvRoom.Columns["id"] != null) dgvRoom.Columns["id"].Visible = false;
                if (dgvRoom.Columns["Trạng Thái Gốc"] != null) dgvRoom.Columns["Trạng Thái Gốc"].Visible = false;
                if (dgvRoom.Columns["Giá Thuê"] != null) dgvRoom.Columns["Giá Thuê"].DefaultCellStyle.Format = "N0";
            }
            catch (Exception ex) { MessageBox.Show("Lỗi tải dữ liệu: " + ex.Message); }
        }

        private void LoadCombos()
        {
            try
            {
                cbbBuilding.DataSource = DatabaseHelper.GetData("SELECT id, name FROM buildings");
                cbbBuilding.DisplayMember = "name";
                cbbBuilding.ValueMember = "id";

                cbbStatus.DataSource = new BindingSource(statusDict, null);
                cbbStatus.DisplayMember = "Value";
                cbbStatus.ValueMember = "Key";
                cbbStatus.SelectedIndex = 0;
            }
            catch { }
        }

        // --- VALIDATE ---
        private bool ValidateInput()
        {
            if (cbbBuilding.SelectedValue == null) { MessageBox.Show("Chọn tòa nhà!"); return false; }
            if (string.IsNullOrWhiteSpace(txtRoomNumber.Text)) { MessageBox.Show("Nhập số phòng!"); return false; }

            decimal price, area;
            if (!decimal.TryParse(txtPrice.Text, out price)) { MessageBox.Show("Giá thuê phải là số!"); return false; }
            if (!decimal.TryParse(txtArea.Text, out area)) { MessageBox.Show("Diện tích phải là số!"); return false; }

            return true;
        }

        // --- CRUD (ĐÃ FIX LỖI SQL DECIMAL) ---
        private void BtnAdd_Click(object sender, EventArgs e)
        {
            if (!ValidateInput()) return;
            try
            {
                string status = cbbStatus.SelectedValue.ToString();

                // SỬA LỖI Ở ĐÂY: Chuyển số sang chuỗi định dạng chuẩn quốc tế (dấu chấm)
                string strArea = decimal.Parse(txtArea.Text).ToString(CultureInfo.InvariantCulture);
                string strPrice = decimal.Parse(txtPrice.Text).ToString(CultureInfo.InvariantCulture);

                string sql = $"INSERT INTO rooms (building_id, room_number, floor, area, price, status) " +
                             $"VALUES ({cbbBuilding.SelectedValue}, '{txtRoomNumber.Text}', '{txtFloor.Text}', {strArea}, {strPrice}, '{status}')";

                DatabaseHelper.ExecuteSql(sql);
                MessageBox.Show("Thêm phòng thành công!");
                LoadData(); ClearInput();
            }
            catch (Exception ex) { MessageBox.Show("Lỗi: " + ex.Message); }
        }

        private void BtnEdit_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtID.Text)) { MessageBox.Show("Vui lòng chọn phòng cần sửa!"); return; }
            if (!ValidateInput()) return;

            try
            {
                string status = cbbStatus.SelectedValue.ToString();

                // SỬA LỖI TƯƠNG TỰ CHO NÚT SỬA
                string strArea = decimal.Parse(txtArea.Text).ToString(CultureInfo.InvariantCulture);
                string strPrice = decimal.Parse(txtPrice.Text).ToString(CultureInfo.InvariantCulture);

                string sql = $"UPDATE rooms SET building_id={cbbBuilding.SelectedValue}, room_number='{txtRoomNumber.Text}', " +
                             $"floor='{txtFloor.Text}', area={strArea}, price={strPrice}, status='{status}' WHERE id={txtID.Text}";

                DatabaseHelper.ExecuteSql(sql);
                MessageBox.Show("Cập nhật thành công!");
                LoadData(); ClearInput();
            }
            catch (Exception ex) { MessageBox.Show("Lỗi: " + ex.Message); }
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtID.Text)) return;
            if (MessageBox.Show("Xóa phòng này?", "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                try
                {
                    DatabaseHelper.ExecuteSql($"DELETE FROM rooms WHERE id={txtID.Text}");
                    LoadData(); ClearInput();
                }
                catch { MessageBox.Show("Không thể xóa phòng đang có hợp đồng!"); }
            }
        }

        // --- UI & HELPERS ---
        private void ClearInput() { txtID.Clear(); txtRoomNumber.Clear(); txtFloor.Clear(); txtArea.Clear(); txtPrice.Clear(); cbbStatus.SelectedIndex = 0; }

        private void DesignInputPanel()
        {
            Panel pnlInput = new Panel { Dock = DockStyle.Top, Height = 260, BackColor = Color.WhiteSmoke };
            this.Controls.Add(pnlInput);

            CreateInput(pnlInput, "Mã (Auto):", out txtID, 40, 20, 80); txtID.Enabled = false;

            pnlInput.Controls.Add(new Label { Text = "Thuộc Tòa Nhà:", Location = new Point(150, 20), AutoSize = true, ForeColor = Color.DimGray });
            cbbBuilding = new ComboBox { Location = new Point(150, 45), Width = 200, DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 11) };
            pnlInput.Controls.Add(cbbBuilding);

            CreateInput(pnlInput, "Số Phòng (VD: P101):", out txtRoomNumber, 380, 20, 150);
            CreateInput(pnlInput, "Tầng:", out txtFloor, 560, 20, 100);

            CreateInput(pnlInput, "Diện Tích (m2):", out txtArea, 40, 90, 150);
            CreateInput(pnlInput, "Giá Thuê (VNĐ):", out txtPrice, 220, 90, 200);

            txtPrice.KeyPress += (s, e) => { if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar)) e.Handled = true; };
            // Area cho phép nhập dấu chấm hoặc phẩy tùy máy
            txtArea.KeyPress += (s, e) => {
                if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) && e.KeyChar != '.' && e.KeyChar != ',')
                    e.Handled = true;
            };

            pnlInput.Controls.Add(new Label { Text = "Trạng Thái:", Location = new Point(450, 90), AutoSize = true, ForeColor = Color.DimGray });
            cbbStatus = new ComboBox { Location = new Point(450, 115), Width = 150, DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 11) };
            pnlInput.Controls.Add(cbbStatus);

            int btnY = 180;
            Button btnAdd = CreateButton("THÊM PHÒNG", Color.FromArgb(24, 161, 251), 40, btnY); btnAdd.Click += BtnAdd_Click;
            Button btnEdit = CreateButton("CẬP NHẬT", Color.FromArgb(255, 193, 7), 200, btnY); btnEdit.Click += BtnEdit_Click;
            Button btnDelete = CreateButton("XÓA PHÒNG", Color.FromArgb(253, 138, 114), 360, btnY); btnDelete.Click += BtnDelete_Click;
            Button btnRefresh = CreateButton("LÀM MỚI", Color.Gray, 520, btnY); btnRefresh.Click += (s, e) => { ClearInput(); LoadData(); };

            pnlInput.Controls.AddRange(new Control[] { btnAdd, btnEdit, btnDelete, btnRefresh });
        }

        private void StyleDataGridView()
        {
            dgvRoom = new DataGridView { Dock = DockStyle.Fill, BorderStyle = BorderStyle.None, BackgroundColor = Color.White, RowTemplate = { Height = 40 }, ColumnHeadersHeight = 50, EnableHeadersVisualStyles = false, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, SelectionMode = DataGridViewSelectionMode.FullRowSelect, RowHeadersVisible = false, ReadOnly = true, AllowUserToAddRows = false };
            dgvRoom.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle { BackColor = Color.FromArgb(24, 30, 54), ForeColor = Color.White, Font = new Font("Segoe UI", 11, FontStyle.Bold) };
            this.Controls.Add(dgvRoom); dgvRoom.BringToFront();

            dgvRoom.CellClick += (s, e) => {
                if (e.RowIndex >= 0)
                {
                    var r = dgvRoom.Rows[e.RowIndex];
                    txtID.Text = r.Cells["id"].Value.ToString();
                    txtRoomNumber.Text = r.Cells["Số Phòng"].Value.ToString();
                    txtFloor.Text = r.Cells["Tầng"].Value.ToString();
                    txtArea.Text = r.Cells["Diện Tích"].Value.ToString();
                    txtPrice.Text = Convert.ToDecimal(r.Cells["Giá Thuê"].Value).ToString("0"); // Lấy số nguyên cho dễ sửa

                    string bName = r.Cells["Tòa Nhà"].Value.ToString();
                    cbbBuilding.SelectedIndex = cbbBuilding.FindStringExact(bName);

                    string vnStatus = r.Cells["Trạng Thái"].Value.ToString();
                    foreach (var item in statusDict) if (item.Value == vnStatus) { cbbStatus.SelectedValue = item.Key; break; }
                }
            };

            dgvRoom.CellFormatting += (s, e) => {
                if (dgvRoom.Columns[e.ColumnIndex].Name == "Trạng Thái")
                {
                    string val = e.Value.ToString();
                    if (val == "Còn Trống") e.CellStyle.ForeColor = Color.Green;
                    else if (val == "Đang Thuê") e.CellStyle.ForeColor = Color.Red;
                    else e.CellStyle.ForeColor = Color.Orange;
                    e.CellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
                }
            };
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
    }
}