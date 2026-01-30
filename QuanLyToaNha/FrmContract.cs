using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using System.Globalization; // Xử lý định dạng số tiền

namespace QuanLyToaNha
{
    public partial class FrmContract : Form
    {
        private ComboBox cbbCustomer, cbbBuilding, cbbRoom; // Thêm cbbBuilding
        private DateTimePicker dtpStart, dtpEnd;
        private TextBox txtPrice, txtDeposit, txtID;
        

        // CẤU HÌNH: Số người tối đa trong 1 phòng
        private const int MAX_PEOPLE_PER_ROOM = 4;

        public FrmContract()
        {
            InitializeComponent();
            this.Controls.Clear();
            this.BackColor = Color.White;

            DesignInputPanel();
            StyleDataGridView();

            LoadCombos(); // Load Customer và Building (Chưa load Room)
            LoadData();
        }

        // --- VALIDATION ---
        private bool ValidateInput()
        {
            if (cbbCustomer.SelectedIndex == -1) { MessageBox.Show("Vui lòng chọn Khách Hàng!", "Thiếu thông tin", MessageBoxButtons.OK, MessageBoxIcon.Warning); return false; }
            if (cbbBuilding.SelectedIndex == -1) { MessageBox.Show("Vui lòng chọn Tòa Nhà!", "Thiếu thông tin", MessageBoxButtons.OK, MessageBoxIcon.Warning); return false; }
            if (cbbRoom.SelectedIndex == -1) { MessageBox.Show("Vui lòng chọn Phòng!", "Thiếu thông tin", MessageBoxButtons.OK, MessageBoxIcon.Warning); return false; }

            if (string.IsNullOrWhiteSpace(txtPrice.Text) || !decimal.TryParse(txtPrice.Text, out decimal p) || p < 0) { MessageBox.Show("Giá thuê không hợp lệ!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning); return false; }
            if (dtpEnd.Value.Date <= dtpStart.Value.Date) { MessageBox.Show("Ngày kết thúc phải lớn hơn ngày bắt đầu!", "Lỗi ngày tháng", MessageBoxButtons.OK, MessageBoxIcon.Warning); return false; }
            return true;
        }

        // --- LOAD DATA ---
        private void LoadData()
        {
            try
            {
                // Lấy thêm tên tòa nhà để hiển thị trong Grid
                string sql = @"SELECT c.id, u.full_name as 'Khách Hàng', 
                               b.name as 'Tòa Nhà',
                               r.room_number as 'Phòng', 
                               c.start_date as 'Ngày Bắt Đầu', c.end_date as 'Ngày Kết Thúc', 
                               c.rental_price as 'Giá Thuê', c.deposit as 'Tiền Cọc', c.status as 'Trạng Thái'
                               FROM contracts c
                               JOIN users u ON c.customer_id = u.id
                               JOIN rooms r ON c.room_id = r.id
                               JOIN buildings b ON r.building_id = b.id
                               ORDER BY c.status, c.id DESC";

                DataTable dt = DatabaseHelper.GetData(sql);

                // Việt hóa trạng thái
                foreach (DataRow row in dt.Rows)
                {
                    string status = row["Trạng Thái"].ToString();
                    if (status == "Active") row["Trạng Thái"] = "Đang Hiệu Lực";
                    else if (status == "Terminated") row["Trạng Thái"] = "Đã Thanh Lý";
                }

                dgvContract.DataSource = dt;

                if (dgvContract.Columns["id"] != null) dgvContract.Columns["id"].Visible = false;
                if (dgvContract.Columns["Giá Thuê"] != null) dgvContract.Columns["Giá Thuê"].DefaultCellStyle.Format = "N0";
                if (dgvContract.Columns["Tiền Cọc"] != null) dgvContract.Columns["Tiền Cọc"].DefaultCellStyle.Format = "N0";
                if (dgvContract.Columns["Ngày Bắt Đầu"] != null) dgvContract.Columns["Ngày Bắt Đầu"].DefaultCellStyle.Format = "dd/MM/yyyy";
                if (dgvContract.Columns["Ngày Kết Thúc"] != null) dgvContract.Columns["Ngày Kết Thúc"].DefaultCellStyle.Format = "dd/MM/yyyy";
            }
            catch (Exception ex) { MessageBox.Show("Lỗi tải dữ liệu: " + ex.Message); }
        }

        private void LoadCombos()
        {
            try
            {
                // 1. Load Khách Hàng
                cbbCustomer.DataSource = DatabaseHelper.GetData("SELECT id, full_name FROM users WHERE role='resident'");
                cbbCustomer.DisplayMember = "full_name"; cbbCustomer.ValueMember = "id"; cbbCustomer.SelectedIndex = -1;

                // 2. Load Tòa Nhà (Sự kiện chọn tòa nhà sẽ load phòng)
                cbbBuilding.SelectedIndexChanged -= CbbBuilding_SelectedIndexChanged; // Gỡ sự kiện để tránh lỗi khi load
                cbbBuilding.DataSource = DatabaseHelper.GetData("SELECT id, name FROM buildings");
                cbbBuilding.DisplayMember = "name";
                cbbBuilding.ValueMember = "id";
                cbbBuilding.SelectedIndex = -1;
                cbbBuilding.SelectedIndexChanged += CbbBuilding_SelectedIndexChanged;
            }
            catch { }
        }

        // Sự kiện: Khi chọn Tòa Nhà -> Load danh sách Phòng của tòa đó
        private void CbbBuilding_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cbbBuilding.SelectedValue == null) return;

            try
            {
                // Reset ô phòng và giá
                cbbRoom.DataSource = null;
                txtPrice.Clear();

                string buildingId = cbbBuilding.SelectedValue.ToString();

                // Load phòng thuộc tòa nhà đã chọn (Lấy cả phòng đang ở để xem, lúc thuê sẽ check sau)
                string sqlRooms = $"SELECT id, room_number, price FROM rooms WHERE building_id={buildingId}";
                DataTable dtRooms = DatabaseHelper.GetData(sqlRooms);

                cbbRoom.SelectedIndexChanged -= CbbRoom_SelectedIndexChanged;
                cbbRoom.DataSource = dtRooms;
                cbbRoom.DisplayMember = "room_number";
                cbbRoom.ValueMember = "id";
                cbbRoom.SelectedIndex = -1;
                cbbRoom.SelectedIndexChanged += CbbRoom_SelectedIndexChanged;
            }
            catch { }
        }

        // Sự kiện: Khi chọn Phòng -> Tự điền Giá
        private void CbbRoom_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cbbRoom.SelectedValue != null && cbbRoom.SelectedItem is DataRowView row)
            {
                try
                {
                    decimal price = Convert.ToDecimal(row["price"]);
                    txtPrice.Text = price.ToString("0");
                }
                catch { }
            }
        }

        // --- CRUD ACTIONS ---
        private void BtnAdd_Click(object sender, EventArgs e)
        {
            if (!ValidateInput()) return;
            try
            {
                int roomId = Convert.ToInt32(cbbRoom.SelectedValue);
                // Check số người
                long currentPeople = Convert.ToInt64(DatabaseHelper.GetData($"SELECT count(*) FROM contracts WHERE room_id={roomId} AND status='Active'").Rows[0][0]);
                if (currentPeople >= MAX_PEOPLE_PER_ROOM)
                {
                    MessageBox.Show($"Phòng này đã ĐỦ NGƯỜI ({currentPeople}/{MAX_PEOPLE_PER_ROOM})!\nVui lòng chọn phòng khác.", "Phòng kín", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                string strPrice = decimal.Parse(txtPrice.Text).ToString(CultureInfo.InvariantCulture);
                string strDeposit = string.IsNullOrEmpty(txtDeposit.Text) ? "0" : decimal.Parse(txtDeposit.Text).ToString(CultureInfo.InvariantCulture);

                string sql = $"INSERT INTO contracts (customer_id, room_id, start_date, end_date, rental_price, deposit, status) VALUES ({cbbCustomer.SelectedValue}, {roomId}, '{dtpStart.Value:yyyy-MM-dd}', '{dtpEnd.Value:yyyy-MM-dd}', {strPrice}, {strDeposit}, 'Active')";
                DatabaseHelper.ExecuteSql(sql);

                // Update trạng thái phòng
                if (currentPeople + 1 >= MAX_PEOPLE_PER_ROOM) DatabaseHelper.ExecuteSql($"UPDATE rooms SET status='Occupied' WHERE id={roomId}");
                else DatabaseHelper.ExecuteSql($"UPDATE rooms SET status='Available' WHERE id={roomId}");

                MessageBox.Show("Tạo hợp đồng thành công!"); LoadData(); ClearInput();
            }
            catch (Exception ex) { MessageBox.Show("Lỗi: " + ex.Message); }
        }

        private void BtnTerminate_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtID.Text)) return;
            if (MessageBox.Show("Thanh lý hợp đồng này?", "Xác nhận", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                try
                {
                    DataTable dt = DatabaseHelper.GetData($"SELECT room_id FROM contracts WHERE id={txtID.Text}");
                    if (dt.Rows.Count > 0)
                    {
                        string roomId = dt.Rows[0][0].ToString();
                        DatabaseHelper.ExecuteSql($"UPDATE contracts SET status='Terminated', end_date='{DateTime.Now:yyyy-MM-dd}' WHERE id={txtID.Text}");
                        DatabaseHelper.ExecuteSql($"UPDATE rooms SET status='Available' WHERE id={roomId}");
                    }
                    MessageBox.Show("Đã thanh lý!"); LoadData(); ClearInput();
                }
                catch (Exception ex) { MessageBox.Show(ex.Message); }
            }
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtID.Text)) { MessageBox.Show("Vui lòng chọn hợp đồng cần xóa!"); return; }
            if (MessageBox.Show("Cảnh báo: Xóa hợp đồng sẽ xóa cả hóa đơn liên quan.\nTiếp tục?", "Cảnh báo xóa", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                try
                {
                    string contractId = txtID.Text;
                    DataTable dt = DatabaseHelper.GetData($"SELECT room_id FROM contracts WHERE id={contractId}");
                    string roomId = dt.Rows.Count > 0 ? dt.Rows[0]["room_id"].ToString() : "0";

                    DatabaseHelper.ExecuteSql($"DELETE FROM invoices WHERE contract_id={contractId}");
                    DatabaseHelper.ExecuteSql($"DELETE FROM contracts WHERE id={contractId}");

                    if (roomId != "0") DatabaseHelper.ExecuteSql($"UPDATE rooms SET status='Available' WHERE id={roomId}");

                    MessageBox.Show("Đã xóa!"); LoadData(); ClearInput();
                }
                catch (Exception ex) { MessageBox.Show(ex.Message); }
            }
        }

        // --- UI & HELPERS ---
        private void ClearInput() { txtID.Clear(); txtPrice.Clear(); txtDeposit.Clear(); cbbCustomer.SelectedIndex = -1; cbbBuilding.SelectedIndex = -1; cbbRoom.DataSource = null; }

        private void DesignInputPanel()
        {
            Panel pnlInput = new Panel { Dock = DockStyle.Top, Height = 260, BackColor = Color.WhiteSmoke };
            this.Controls.Add(pnlInput);

            CreateInput(pnlInput, "Mã HĐ:", out txtID, 40, 20, 100); txtID.Enabled = false;

            pnlInput.Controls.Add(new Label { Text = "Khách Hàng:", Location = new Point(40, 90), AutoSize = true, ForeColor = Color.DimGray });
            cbbCustomer = new ComboBox { Location = new Point(40, 115), Width = 250, DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 11) };
            pnlInput.Controls.Add(cbbCustomer);

            // --- TÁCH RIÊNG TÒA NHÀ VÀ PHÒNG ---
            // 1. Tòa Nhà
            pnlInput.Controls.Add(new Label { Text = "Chọn Tòa Nhà:", Location = new Point(330, 90), AutoSize = true, ForeColor = Color.DimGray });
            cbbBuilding = new ComboBox { Location = new Point(330, 115), Width = 110, DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 11) };
            pnlInput.Controls.Add(cbbBuilding);

            // 2. Phòng (Vị trí bên cạnh tòa nhà)
            pnlInput.Controls.Add(new Label { Text = "Chọn Phòng:", Location = new Point(450, 90), AutoSize = true, ForeColor = Color.DimGray });
            cbbRoom = new ComboBox { Location = new Point(450, 115), Width = 100, DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 11) };
            pnlInput.Controls.Add(cbbRoom);

            pnlInput.Controls.Add(new Label { Text = "Ngày Bắt Đầu:", Location = new Point(180, 20), AutoSize = true, ForeColor = Color.DimGray });
            dtpStart = new DateTimePicker { Location = new Point(180, 45), Width = 140, Format = DateTimePickerFormat.Custom, CustomFormat = "dd/MM/yyyy", Font = new Font("Segoe UI", 11) };
            pnlInput.Controls.Add(dtpStart);

            pnlInput.Controls.Add(new Label { Text = "Ngày Kết Thúc:", Location = new Point(340, 20), AutoSize = true, ForeColor = Color.DimGray });
            dtpEnd = new DateTimePicker { Location = new Point(340, 45), Width = 140, Format = DateTimePickerFormat.Custom, CustomFormat = "dd/MM/yyyy", Font = new Font("Segoe UI", 11) };
            dtpEnd.Value = DateTime.Now.AddYears(1);
            pnlInput.Controls.Add(dtpEnd);

            CreateInput(pnlInput, "Giá Thuê (Tháng):", out txtPrice, 580, 20, 150);
            CreateInput(pnlInput, "Tiền Cọc:", out txtDeposit, 580, 90, 150);

            txtPrice.TextChanged += (s, e) => { if (string.IsNullOrEmpty(txtDeposit.Text)) txtDeposit.Text = txtPrice.Text; };
            txtPrice.KeyPress += (s, e) => { if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar)) e.Handled = true; };
            txtDeposit.KeyPress += (s, e) => { if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar)) e.Handled = true; };

            int y = 180;
            Button btnAdd = CreateButton("TẠO HỢP ĐỒNG", Color.FromArgb(39, 174, 96), 40, y); btnAdd.Click += BtnAdd_Click;
            Button btnTer = CreateButton("THANH LÝ HĐ", Color.FromArgb(243, 156, 18), 200, y); btnTer.Click += BtnTerminate_Click;
            Button btnDel = CreateButton("XÓA HĐ", Color.FromArgb(231, 76, 60), 360, y); btnDel.Click += BtnDelete_Click;
            Button btnRef = CreateButton("LÀM MỚI", Color.DimGray, 520, y); btnRef.Click += (s, e) => { ClearInput(); LoadData(); };

            pnlInput.Controls.AddRange(new Control[] { btnAdd, btnTer, btnDel, btnRef });
        }

        private void StyleDataGridView()
        {
            dgvContract = new DataGridView { Dock = DockStyle.Fill, BorderStyle = BorderStyle.None, BackgroundColor = Color.White, RowTemplate = { Height = 40 }, ColumnHeadersHeight = 50, EnableHeadersVisualStyles = false, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, SelectionMode = DataGridViewSelectionMode.FullRowSelect, RowHeadersVisible = false, ReadOnly = true, AllowUserToAddRows = false };
            dgvContract.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle { BackColor = Color.FromArgb(24, 30, 54), ForeColor = Color.White, Font = new Font("Segoe UI", 11, FontStyle.Bold) };
            this.Controls.Add(dgvContract); dgvContract.BringToFront();

            dgvContract.CellClick += (s, e) => {
                if (e.RowIndex >= 0)
                {
                    var r = dgvContract.Rows[e.RowIndex];
                    txtID.Text = r.Cells["id"].Value.ToString();
                    if (r.Cells["Giá Thuê"].Value != DBNull.Value) txtPrice.Text = Convert.ToDecimal(r.Cells["Giá Thuê"].Value).ToString("0");
                    if (r.Cells["Tiền Cọc"].Value != DBNull.Value) txtDeposit.Text = Convert.ToDecimal(r.Cells["Tiền Cọc"].Value).ToString("0");
                    dtpStart.Value = Convert.ToDateTime(r.Cells["Ngày Bắt Đầu"].Value);
                    dtpEnd.Value = Convert.ToDateTime(r.Cells["Ngày Kết Thúc"].Value);

                    cbbCustomer.SelectedIndex = cbbCustomer.FindStringExact(r.Cells["Khách Hàng"].Value.ToString());

                    // Logic chọn Tòa nhà -> sau đó mới chọn Phòng
                    string buildingName = r.Cells["Tòa Nhà"].Value.ToString();
                    cbbBuilding.SelectedIndex = cbbBuilding.FindStringExact(buildingName);

                    // Vì chọn building sẽ trigger sự kiện load room, ta cần đợi hoặc set luôn
                    // Ở đây sự kiện chạy đồng bộ nên set Room ngay sau đó ok
                    string roomNum = r.Cells["Phòng"].Value.ToString();
                    cbbRoom.SelectedIndex = cbbRoom.FindStringExact(roomNum);
                }
            };

            dgvContract.CellFormatting += (s, e) => {
                if (dgvContract.Columns[e.ColumnIndex].Name == "Trạng Thái")
                {
                    string val = e.Value.ToString();
                    if (val == "Đang Hiệu Lực") { e.CellStyle.ForeColor = Color.Green; e.CellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold); }
                    else { e.CellStyle.ForeColor = Color.Gray; e.CellStyle.Font = new Font("Segoe UI", 10, FontStyle.Italic); }
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