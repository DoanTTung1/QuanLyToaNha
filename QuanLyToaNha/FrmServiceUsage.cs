using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace QuanLyToaNha
{
    public partial class FrmServiceUsage : Form
    {
        private DataGridView dgvUsage;
        private ComboBox cbbRoom, cbbService;
        private TextBox txtOldIndex, txtNewIndex;
        private DateTimePicker dtpDate;

        private Color colorPrimary = Color.FromArgb(24, 30, 54);
        private Color colorActive = Color.FromArgb(24, 161, 251);

        public FrmServiceUsage()
        {
            InitializeComponent();
            this.Text = "GHI CHỈ SỐ ĐIỆN NƯỚC";
            this.BackColor = Color.FromArgb(245, 247, 251);
            this.Size = new Size(1100, 700);

            InitUI();
            LoadComboBoxes();
            LoadData();
        }

        // --- VALIDATION (BẮT LỖI) ---
        private bool ValidateInput()
        {
            if (cbbRoom.SelectedIndex == -1)
            {
                MessageBox.Show("Vui lòng chọn Phòng!", "Thiếu thông tin", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                cbbRoom.Focus();
                return false;
            }
            if (cbbService.SelectedIndex == -1)
            {
                MessageBox.Show("Vui lòng chọn Dịch Vụ (Điện/Nước)!", "Thiếu thông tin", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                cbbService.Focus();
                return false;
            }

            // Kiểm tra chỉ số cũ
            if (string.IsNullOrWhiteSpace(txtOldIndex.Text))
            {
                MessageBox.Show("Vui lòng nhập Chỉ số cũ!", "Thiếu thông tin", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtOldIndex.Focus();
                return false;
            }
            if (!decimal.TryParse(txtOldIndex.Text, out decimal oldVal) || oldVal < 0)
            {
                MessageBox.Show("Chỉ số cũ phải là số không âm!", "Lỗi định dạng", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtOldIndex.Focus();
                return false;
            }

            // Kiểm tra chỉ số mới
            if (string.IsNullOrWhiteSpace(txtNewIndex.Text))
            {
                MessageBox.Show("Vui lòng nhập Chỉ số mới!", "Thiếu thông tin", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtNewIndex.Focus();
                return false;
            }
            if (!decimal.TryParse(txtNewIndex.Text, out decimal newVal) || newVal < 0)
            {
                MessageBox.Show("Chỉ số mới phải là số không âm!", "Lỗi định dạng", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtNewIndex.Focus();
                return false;
            }

            // Logic: Mới >= Cũ
            if (newVal < oldVal)
            {
                MessageBox.Show($"Chỉ số mới ({newVal}) KHÔNG ĐƯỢC nhỏ hơn chỉ số cũ ({oldVal})!\nVui lòng kiểm tra lại đồng hồ.", "Sai logic", MessageBoxButtons.OK, MessageBoxIcon.Error);
                txtNewIndex.Focus();
                return false;
            }

            return true;
        }

        // --- LOAD DATA ---
        private void LoadData()
        {
            try
            {
                string sql = @"
                    SELECT 
                        u.id AS 'ID',
                        r.room_number AS 'Phòng',
                        s.name AS 'Dịch Vụ',
                        CONCAT(u.month, '/', u.year) AS 'Tháng/Năm',
                        u.old_index AS 'Chỉ Số Cũ',
                        u.new_index AS 'Chỉ Số Mới',
                        (u.new_index - u.old_index) AS 'Tiêu Thụ'
                    FROM service_usages u
                    JOIN rooms r ON u.room_id = r.id
                    JOIN services s ON u.service_id = s.id
                    ORDER BY u.year DESC, u.month DESC, r.room_number ASC";

                dgvUsage.DataSource = DatabaseHelper.GetData(sql);
                if (dgvUsage.Columns["ID"] != null) dgvUsage.Columns["ID"].Visible = false;

                if (dgvUsage.Columns["Tiêu Thụ"] != null)
                {
                    dgvUsage.Columns["Tiêu Thụ"].DefaultCellStyle.Format = "N0";
                    dgvUsage.Columns["Tiêu Thụ"].DefaultCellStyle.ForeColor = Color.Blue;
                }
                if (dgvUsage.Columns["Chỉ Số Mới"] != null) dgvUsage.Columns["Chỉ Số Mới"].DefaultCellStyle.Format = "N0";
                if (dgvUsage.Columns["Chỉ Số Cũ"] != null) dgvUsage.Columns["Chỉ Số Cũ"].DefaultCellStyle.Format = "N0";
            }
            catch (Exception ex) { MessageBox.Show("Lỗi tải dữ liệu: " + ex.Message); }
        }

        private void LoadComboBoxes()
        {
            try
            {
                cbbRoom.SelectedIndexChanged -= CbbRoom_SelectedIndexChanged;
                cbbService.SelectedIndexChanged -= CbbRoom_SelectedIndexChanged;

                cbbRoom.DataSource = DatabaseHelper.GetData("SELECT id, room_number FROM rooms");
                cbbRoom.DisplayMember = "room_number";
                cbbRoom.ValueMember = "id";

                cbbService.DataSource = DatabaseHelper.GetData("SELECT id, name FROM services");
                cbbService.DisplayMember = "name";
                cbbService.ValueMember = "id";

                cbbRoom.SelectedIndex = -1;
                cbbService.SelectedIndex = -1;

                cbbRoom.SelectedIndexChanged += CbbRoom_SelectedIndexChanged;
                cbbService.SelectedIndexChanged += CbbRoom_SelectedIndexChanged;
            }
            catch { }
        }

        // --- LƯU DỮ LIỆU ---
        private void BtnSave_Click(object sender, EventArgs e)
        {
            // 1. Gọi hàm kiểm tra lỗi trước
            if (!ValidateInput()) return;

            try
            {
                int roomId = GetSelectedId(cbbRoom);
                int serviceId = GetSelectedId(cbbService);
                decimal oldIdx = decimal.Parse(txtOldIndex.Text);
                decimal newIdx = decimal.Parse(txtNewIndex.Text);
                int month = dtpDate.Value.Month;
                int year = dtpDate.Value.Year;

                using (var conn = DatabaseHelper.GetConnection())
                {
                    conn.Open();

                    // Kiểm tra đã nhập cho tháng này chưa
                    string checkSql = "SELECT count(*) FROM service_usages WHERE room_id=@r AND service_id=@s AND month=@m AND year=@y";
                    MySqlCommand cmdCheck = new MySqlCommand(checkSql, conn);
                    cmdCheck.Parameters.AddWithValue("@r", roomId);
                    cmdCheck.Parameters.AddWithValue("@s", serviceId);
                    cmdCheck.Parameters.AddWithValue("@m", month);
                    cmdCheck.Parameters.AddWithValue("@y", year);

                    if (Convert.ToInt32(cmdCheck.ExecuteScalar()) > 0)
                    {
                        // Hỏi xác nhận cập nhật
                        if (MessageBox.Show($"Dữ liệu tháng {month}/{year} của phòng này đã tồn tại.\nBạn có muốn cập nhật lại không?", "Xác nhận cập nhật", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                            return;

                        string updateSql = "UPDATE service_usages SET old_index=@old, new_index=@new WHERE room_id=@r AND service_id=@s AND month=@m AND year=@y";
                        MySqlCommand cmdUp = new MySqlCommand(updateSql, conn);
                        cmdUp.Parameters.AddWithValue("@old", oldIdx);
                        cmdUp.Parameters.AddWithValue("@new", newIdx);
                        cmdUp.Parameters.AddWithValue("@r", roomId);
                        cmdUp.Parameters.AddWithValue("@s", serviceId);
                        cmdUp.Parameters.AddWithValue("@m", month);
                        cmdUp.Parameters.AddWithValue("@y", year);
                        cmdUp.ExecuteNonQuery();
                        MessageBox.Show("Cập nhật thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        string insertSql = "INSERT INTO service_usages (room_id, service_id, month, year, old_index, new_index) VALUES (@r, @s, @m, @y, @old, @new)";
                        MySqlCommand cmdIn = new MySqlCommand(insertSql, conn);
                        cmdIn.Parameters.AddWithValue("@r", roomId);
                        cmdIn.Parameters.AddWithValue("@s", serviceId);
                        cmdIn.Parameters.AddWithValue("@m", month);
                        cmdIn.Parameters.AddWithValue("@y", year);
                        cmdIn.Parameters.AddWithValue("@old", oldIdx);
                        cmdIn.Parameters.AddWithValue("@new", newIdx);
                        cmdIn.ExecuteNonQuery();
                        MessageBox.Show("Thêm mới thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }

                LoadData();
                txtNewIndex.Clear();
                txtOldIndex.Text = newIdx.ToString("0");
                txtNewIndex.Focus();
            }
            catch (Exception ex) { MessageBox.Show("Lỗi hệ thống: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }

        private void CbbRoom_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                int roomId = GetSelectedId(cbbRoom);
                int sId = GetSelectedId(cbbService);

                if (roomId == 0 || sId == 0) return;

                string sql = $"SELECT new_index FROM service_usages WHERE room_id={roomId} AND service_id={sId} ORDER BY year DESC, month DESC LIMIT 1";
                DataTable dt = DatabaseHelper.GetData(sql);
                if (dt.Rows.Count > 0) txtOldIndex.Text = Convert.ToDecimal(dt.Rows[0]["new_index"]).ToString("0");
                else txtOldIndex.Text = "0";
            }
            catch { }
        }

        private int GetSelectedId(ComboBox cbb)
        {
            if (cbb.SelectedValue == null) return 0;
            if (cbb.SelectedValue is int) return (int)cbb.SelectedValue;
            if (cbb.SelectedValue is string) return int.Parse(cbb.SelectedValue.ToString());
            if (cbb.SelectedValue is DataRowView drv) return Convert.ToInt32(drv["id"]);
            return 0;
        }

        // --- GIAO DIỆN ---
        private void InitUI()
        {
            TableLayoutPanel layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1, Padding = new Padding(20) };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 380F));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));

            // PANEL TRÁI
            Panel pnlLeft = CreateCard("NHẬP CHỈ SỐ");
            pnlLeft.Dock = DockStyle.Fill;
            Panel pnlIn = new Panel { Dock = DockStyle.Fill, Padding = new Padding(20, 60, 20, 20) };

            Label l1 = CreateLabel("Chọn Phòng (*):", 20, 10);
            cbbRoom = new ComboBox { Location = new Point(20, 35), Size = new Size(300, 30), Font = new Font("Segoe UI", 11), DropDownStyle = ComboBoxStyle.DropDownList };
            cbbRoom.SelectedIndexChanged += CbbRoom_SelectedIndexChanged;

            Label l2 = CreateLabel("Dịch Vụ (*):", 20, 80);
            cbbService = new ComboBox { Location = new Point(20, 105), Size = new Size(300, 30), Font = new Font("Segoe UI", 11), DropDownStyle = ComboBoxStyle.DropDownList };
            cbbService.SelectedIndexChanged += CbbRoom_SelectedIndexChanged;

            Label l3 = CreateLabel("Tháng Ghi:", 20, 150);
            dtpDate = new DateTimePicker { Location = new Point(20, 175), Size = new Size(300, 30), Font = new Font("Segoe UI", 11), Format = DateTimePickerFormat.Custom, CustomFormat = "MM/yyyy" };

            Label l4 = CreateLabel("Chỉ Số Cũ:", 20, 220);
            txtOldIndex = new TextBox { Location = new Point(20, 245), Size = new Size(140, 30), Font = new Font("Segoe UI", 11) };
            // Chỉ cho nhập số
            txtOldIndex.KeyPress += (s, e) => { if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) && e.KeyChar != '.') e.Handled = true; };

            Label l5 = CreateLabel("Chỉ Số Mới (*):", 180, 220);
            txtNewIndex = new TextBox { Location = new Point(180, 245), Size = new Size(140, 30), Font = new Font("Segoe UI", 11) };
            txtNewIndex.KeyPress += (s, e) => { if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) && e.KeyChar != '.') e.Handled = true; };

            Button btnSave = new Button { Text = "LƯU DỮ LIỆU", Location = new Point(20, 310), Size = new Size(300, 45), FlatStyle = FlatStyle.Flat, BackColor = colorActive, ForeColor = Color.White, Font = new Font("Segoe UI", 10, FontStyle.Bold), Cursor = Cursors.Hand };
            btnSave.Click += BtnSave_Click;

            pnlIn.Controls.AddRange(new Control[] { l1, cbbRoom, l2, cbbService, l3, dtpDate, l4, txtOldIndex, l5, txtNewIndex, btnSave });
            pnlLeft.Controls.Add(pnlIn);

            // PANEL PHẢI
            Panel pnlRight = CreateCard("LỊCH SỬ GHI ĐIỆN NƯỚC");
            pnlRight.Dock = DockStyle.Fill;

            dgvUsage = new DataGridView { Dock = DockStyle.Fill, BackgroundColor = Color.White, BorderStyle = BorderStyle.None, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, RowHeadersVisible = false, AllowUserToAddRows = false, ReadOnly = true, SelectionMode = DataGridViewSelectionMode.FullRowSelect, EnableHeadersVisualStyles = false, RowTemplate = { Height = 40 } };
            dgvUsage.ColumnHeadersHeight = 45;
            dgvUsage.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle { BackColor = colorPrimary, ForeColor = Color.White, Font = new Font("Segoe UI", 10, FontStyle.Bold) };

            Panel pnlGridWrap = new Panel { Dock = DockStyle.Fill, Padding = new Padding(1, 50, 1, 1) };
            pnlGridWrap.Controls.Add(dgvUsage);
            pnlRight.Controls.Add(pnlGridWrap);

            layout.Controls.Add(pnlLeft, 0, 0);
            layout.Controls.Add(pnlRight, 1, 0);
            this.Controls.Add(layout);
        }

        private Panel CreateCard(string title)
        {
            Panel p = new Panel { BackColor = Color.White };
            Label lbl = new Label { Text = title, ForeColor = colorPrimary, Font = new Font("Segoe UI", 12, FontStyle.Bold), Location = new Point(20, 15), AutoSize = true };
            Panel line = new Panel { BackColor = Color.LightGray, Size = new Size(p.Width, 1), Location = new Point(0, 50), Dock = DockStyle.Top };
            p.Controls.Add(line); p.Controls.Add(lbl);
            return p;
        }
        private Label CreateLabel(string text, int x, int y)
        {
            return new Label { Text = text, Location = new Point(x, y), AutoSize = true, Font = new Font("Segoe UI", 10), ForeColor = Color.Gray };
        }
    }
}