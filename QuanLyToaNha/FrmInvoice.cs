using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace QuanLyToaNha
{
    public partial class FrmInvoice : Form
    {
        private ComboBox cbbRoom, cbbStatus;
        private DateTimePicker dtpMonth;
        private TextBox txtRentFee, txtServiceFee, txtTotal, txtPaid, txtID;
        private DataGridView dgvInvoice;

        // Từ điển trạng thái Việt - Anh
        private Dictionary<string, string> statusDict = new Dictionary<string, string>()
        {
            { "Unpaid", "Chưa Thanh Toán" },
            { "Paid", "Đã Thanh Toán" }
        };

        public FrmInvoice()
        {
            InitializeComponent();
            this.Controls.Clear();
            this.BackColor = Color.White;

            DesignInputPanel();
            StyleDataGridView();

            LoadRooms();
            LoadStatusComboBox();
            LoadData();
        }

        // --- 1. LOAD DỮ LIỆU & CẤU HÌNH ---

        private void LoadStatusComboBox()
        {
            cbbStatus.DataSource = new BindingSource(statusDict, null);
            cbbStatus.DisplayMember = "Value"; // Hiển thị tiếng Việt
            cbbStatus.ValueMember = "Key";     // Lưu tiếng Anh
            cbbStatus.SelectedIndex = 0;
        }

        private void LoadRooms()
        {
            try
            {
                // Chỉ lấy phòng đang có hợp đồng Active
                string sql = @"SELECT DISTINCT r.id, r.room_number 
                               FROM rooms r JOIN contracts c ON r.id = c.room_id 
                               WHERE c.status = 'Active'";
                DataTable dt = DatabaseHelper.GetData(sql);
                cbbRoom.DataSource = dt;
                cbbRoom.DisplayMember = "room_number";
                cbbRoom.ValueMember = "id";
                cbbRoom.SelectedIndex = -1;
            }
            catch { }
        }

        private void LoadData()
        {
            try
            {
                string sql = @"SELECT i.id, r.room_number as 'Phòng', 
                               CONCAT(i.month, '/', i.year) as 'Tháng',
                               i.room_fee as 'Tiền Phòng', 
                               i.service_fee as 'Dịch Vụ', 
                               i.total_amount as 'Tổng Cộng', 
                               i.status as 'Trạng Thái Gốc'
                               FROM invoices i
                               JOIN contracts c ON i.contract_id = c.id
                               JOIN rooms r ON c.room_id = r.id
                               ORDER BY i.year DESC, i.month DESC, i.status ASC";

                DataTable dt = DatabaseHelper.GetData(sql);
                dt.Columns.Add("Trạng Thái", typeof(string)); // Cột hiển thị tiếng Việt

                foreach (DataRow row in dt.Rows)
                {
                    string en = row["Trạng Thái Gốc"].ToString();
                    row["Trạng Thái"] = statusDict.ContainsKey(en) ? statusDict[en] : en;
                }

                dgvInvoice.DataSource = dt;

                // Ẩn cột ID, Trạng Thái Gốc
                if (dgvInvoice.Columns["id"] != null) dgvInvoice.Columns["id"].Visible = false;
                if (dgvInvoice.Columns["Trạng Thái Gốc"] != null) dgvInvoice.Columns["Trạng Thái Gốc"].Visible = false;

                // Format tiền
                if (dgvInvoice.Columns["Tiền Phòng"] != null) dgvInvoice.Columns["Tiền Phòng"].DefaultCellStyle.Format = "N0";
                if (dgvInvoice.Columns["Dịch Vụ"] != null) dgvInvoice.Columns["Dịch Vụ"].DefaultCellStyle.Format = "N0";
                if (dgvInvoice.Columns["Tổng Cộng"] != null)
                {
                    dgvInvoice.Columns["Tổng Cộng"].DefaultCellStyle.Format = "N0";
                    dgvInvoice.Columns["Tổng Cộng"].DefaultCellStyle.ForeColor = Color.Blue;
                    dgvInvoice.Columns["Tổng Cộng"].DefaultCellStyle.Font = new Font("Segoe UI", 9, FontStyle.Bold);
                }
            }
            catch (Exception ex) { MessageBox.Show("Lỗi tải danh sách: " + ex.Message); }
        }

        // --- 2. TÍNH TOÁN TIỀN (AN TOÀN) ---
        private void BtnCalculate_Click(object sender, EventArgs e)
        {
            if (cbbRoom.SelectedValue == null) { MessageBox.Show("Vui lòng chọn phòng!", "Chưa chọn", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }

            string roomId = cbbRoom.SelectedValue.ToString();
            int month = dtpMonth.Value.Month;
            int year = dtpMonth.Value.Year;

            try
            {
                // 1. Lấy Tiền Phòng từ Hợp đồng
                string sqlContract = $"SELECT id, rental_price FROM contracts WHERE room_id={roomId} AND status='Active'";
                DataTable dtContract = DatabaseHelper.GetData(sqlContract);

                if (dtContract.Rows.Count == 0) { MessageBox.Show("Phòng này hiện KHÔNG có hợp đồng thuê!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error); return; }

                decimal rentPrice = Convert.ToDecimal(dtContract.Rows[0]["rental_price"]);
                txtRentFee.Text = rentPrice.ToString("N0");
                txtRentFee.Tag = dtContract.Rows[0]["id"]; // Lưu contract_id ẩn

                // 2. Tính Tiền Dịch Vụ: (Mới - Cũ) * Đơn Giá
                // Dùng COALESCE để trả về 0 nếu chưa có dữ liệu điện nước
                string sqlService = $@"
                    SELECT COALESCE(SUM((u.new_index - u.old_index) * s.unit_price), 0)
                    FROM service_usages u
                    JOIN services s ON u.service_id = s.id
                    WHERE u.room_id = {roomId} AND u.month = {month} AND u.year = {year}";

                decimal serviceFee = Convert.ToDecimal(DatabaseHelper.GetData(sqlService).Rows[0][0]);

                // *** CHẶN LỖI SỐ KHỦNG KHIẾP ***
                // Nếu > 100 tỷ -> Chắc chắn dữ liệu rác -> Reset về 0
                if (serviceFee > 100000000000)
                {
                    MessageBox.Show($"Phát hiện dữ liệu bất thường (Tiền dịch vụ quá lớn: {serviceFee:N0}).\nĐã tự động đặt về 0 để tránh lỗi.\nVui lòng kiểm tra lại phần 'Ghi Điện Nước'.", "Cảnh báo dữ liệu", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    serviceFee = 0;
                }

                txtServiceFee.Text = serviceFee.ToString("N0");

                // 3. Tổng Cộng
                decimal total = rentPrice + serviceFee;
                txtTotal.Text = total.ToString("N0");
                txtPaid.Text = "0";
            }
            catch (Exception ex) { MessageBox.Show("Lỗi tính toán: " + ex.Message); }
        }

        // --- 3. LƯU HÓA ĐƠN ---
        private void BtnSave_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtTotal.Text) || txtRentFee.Tag == null)
            {
                MessageBox.Show("Vui lòng bấm nút 'TÍNH TOÁN' trước!", "Thiếu bước", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                string contractId = txtRentFee.Tag.ToString();
                int month = dtpMonth.Value.Month;
                int year = dtpMonth.Value.Year;

                // Xử lý chuỗi tiền tệ (xóa dấu phẩy hiển thị)
                decimal rent = decimal.Parse(txtRentFee.Text, NumberStyles.AllowThousands | NumberStyles.AllowDecimalPoint);
                decimal service = decimal.Parse(txtServiceFee.Text, NumberStyles.AllowThousands | NumberStyles.AllowDecimalPoint);
                decimal total = decimal.Parse(txtTotal.Text, NumberStyles.AllowThousands | NumberStyles.AllowDecimalPoint);

                // Kiểm tra giới hạn DB (Double check)
                if (total > 99999999999999)
                {
                    MessageBox.Show("Tổng tiền quá lớn, không thể lưu!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                    return;
                }

                decimal paid = 0;
                if (!string.IsNullOrEmpty(txtPaid.Text))
                    decimal.TryParse(txtPaid.Text, NumberStyles.AllowThousands | NumberStyles.AllowDecimalPoint, null, out paid);

                // Convert sang chuỗi SQL chuẩn quốc tế (dấu chấm)
                string strRent = rent.ToString(CultureInfo.InvariantCulture);
                string strService = service.ToString(CultureInfo.InvariantCulture);
                string strTotal = total.ToString(CultureInfo.InvariantCulture);
                string strPaid = paid.ToString(CultureInfo.InvariantCulture);

                string status = cbbStatus.SelectedValue.ToString(); // Lấy 'Unpaid' hoặc 'Paid'

                // Kiểm tra đã có hóa đơn chưa
                string checkSql = $"SELECT id FROM invoices WHERE contract_id={contractId} AND month={month} AND year={year}";
                DataTable dtCheck = DatabaseHelper.GetData(checkSql);

                if (dtCheck.Rows.Count > 0)
                {
                    if (MessageBox.Show("Hóa đơn tháng này đã tồn tại. Cập nhật lại?", "Xác nhận", MessageBoxButtons.YesNo) == DialogResult.Yes)
                    {
                        string id = dtCheck.Rows[0][0].ToString();
                        string sql = $"UPDATE invoices SET room_fee={strRent}, service_fee={strService}, total_amount={strTotal}, amount_paid={strPaid}, status='{status}' WHERE id={id}";
                        DatabaseHelper.ExecuteSql(sql);
                        MessageBox.Show("Cập nhật thành công!");
                    }
                }
                else
                {
                    string sql = $"INSERT INTO invoices (contract_id, month, year, room_fee, service_fee, total_amount, amount_paid, status) " +
                                 $"VALUES ({contractId}, {month}, {year}, {strRent}, {strService}, {strTotal}, {strPaid}, '{status}')";
                    DatabaseHelper.ExecuteSql(sql);
                    MessageBox.Show("Tạo hóa đơn thành công!");
                }

                LoadData(); ClearInput();
            }
            catch (Exception ex) { MessageBox.Show("Lỗi lưu DB: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtID.Text)) { MessageBox.Show("Chọn hóa đơn cần xóa!"); return; }
            if (MessageBox.Show("Xóa hóa đơn này?", "Xác nhận", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                DatabaseHelper.ExecuteSql($"DELETE FROM invoices WHERE id={txtID.Text}");
                LoadData(); ClearInput();
            }
        }

        private void BtnExport_Click(object sender, EventArgs e)
        {
            if (dgvInvoice.Rows.Count == 0) return;
            SaveFileDialog sfd = new SaveFileDialog { Filter = "CSV (*.csv)|*.csv", FileName = "HoaDon.csv" };
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    using (StreamWriter sw = new StreamWriter(sfd.FileName, false, Encoding.UTF8))
                    {
                        sw.WriteLine("Phong,Thang,TienPhong,DichVu,TongCong,TrangThai");
                        foreach (DataGridViewRow r in dgvInvoice.Rows)
                            sw.WriteLine($"{r.Cells["Phòng"].Value},{r.Cells["Tháng"].Value},{r.Cells["Tiền Phòng"].Value},{r.Cells["Dịch Vụ"].Value},{r.Cells["Tổng Cộng"].Value},{r.Cells["Trạng Thái"].Value}");
                    }
                    MessageBox.Show("Xuất file thành công!");
                }
                catch { }
            }
        }

        // --- UI HELPER ---
        private void ClearInput() { txtID.Clear(); txtRentFee.Clear(); txtServiceFee.Clear(); txtTotal.Clear(); txtPaid.Text = "0"; }

        private void DesignInputPanel()
        {
            Panel pnlInput = new Panel { Dock = DockStyle.Top, Height = 280, BackColor = Color.WhiteSmoke };
            this.Controls.Add(pnlInput);

            CreateLabel(pnlInput, "Chọn Phòng:", 40, 20);
            cbbRoom = new ComboBox { Location = new Point(40, 45), Width = 150, DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 11) };
            pnlInput.Controls.Add(cbbRoom);

            CreateLabel(pnlInput, "Tháng:", 220, 20);
            dtpMonth = new DateTimePicker { Location = new Point(220, 45), Width = 120, Format = DateTimePickerFormat.Custom, CustomFormat = "MM/yyyy", Font = new Font("Segoe UI", 11) };
            pnlInput.Controls.Add(dtpMonth);

            Button btnCalc = CreateButton("🔍 TÍNH TOÁN", Color.FromArgb(24, 161, 251), 360, 40);
            btnCalc.Click += BtnCalculate_Click;
            pnlInput.Controls.Add(btnCalc);

            CreateInput(pnlInput, "Mã HĐ:", out txtID, 40, 90, 80); txtID.Enabled = false;
            CreateInput(pnlInput, "Tiền Phòng:", out txtRentFee, 150, 90, 150); txtRentFee.ReadOnly = true;
            CreateInput(pnlInput, "Tiền Dịch Vụ:", out txtServiceFee, 330, 90, 180); txtServiceFee.ReadOnly = true;
            CreateInput(pnlInput, "TỔNG CỘNG:", out txtTotal, 540, 90, 200);
            txtTotal.ReadOnly = true; txtTotal.ForeColor = Color.Red; txtTotal.Font = new Font("Segoe UI", 12, FontStyle.Bold);

            CreateLabel(pnlInput, "Trạng Thái:", 40, 160);
            cbbStatus = new ComboBox { Location = new Point(40, 185), Width = 150, DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 11) };
            pnlInput.Controls.Add(cbbStatus);

            CreateInput(pnlInput, "Đã Trả:", out txtPaid, 220, 160, 150); txtPaid.Text = "0";
            txtPaid.KeyPress += (s, e) => { if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar)) e.Handled = true; };

            int y = 220;
            Button btnSave = CreateButton("LƯU HÓA ĐƠN", Color.FromArgb(39, 174, 96), 40, y); btnSave.Click += BtnSave_Click;
            Button btnExp = CreateButton("XUẤT CSV", Color.Teal, 200, y); btnExp.Click += BtnExport_Click;
            Button btnDel = CreateButton("XÓA", Color.IndianRed, 360, y); btnDel.Click += BtnDelete_Click;
            pnlInput.Controls.AddRange(new Control[] { btnSave, btnExp, btnDel });
        }

        private void StyleDataGridView()
        {
            dgvInvoice = new DataGridView { Dock = DockStyle.Fill, BorderStyle = BorderStyle.None, BackgroundColor = Color.White, RowTemplate = { Height = 40 }, ColumnHeadersHeight = 50, EnableHeadersVisualStyles = false, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, SelectionMode = DataGridViewSelectionMode.FullRowSelect, RowHeadersVisible = false, ReadOnly = true, AllowUserToAddRows = false };
            dgvInvoice.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle { BackColor = Color.FromArgb(24, 30, 54), ForeColor = Color.White, Font = new Font("Segoe UI", 11, FontStyle.Bold) };
            this.Controls.Add(dgvInvoice); dgvInvoice.BringToFront();

            dgvInvoice.CellClick += (s, e) => {
                if (e.RowIndex >= 0)
                {
                    var r = dgvInvoice.Rows[e.RowIndex];
                    txtID.Text = r.Cells["id"].Value.ToString();
                    txtTotal.Text = Convert.ToDecimal(r.Cells["Tổng Cộng"].Value).ToString("N0");

                    // Chọn đúng item theo tiếng Việt
                    string vnStatus = r.Cells["Trạng Thái"].Value.ToString();
                    cbbStatus.SelectedIndex = cbbStatus.FindStringExact(vnStatus);
                }
            };

            dgvInvoice.CellFormatting += (s, e) => {
                if (dgvInvoice.Columns[e.ColumnIndex].Name == "Trạng Thái")
                {
                    string val = e.Value.ToString();
                    if (val == "Đã Thanh Toán") e.CellStyle.ForeColor = Color.Green;
                    else e.CellStyle.ForeColor = Color.Red;
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
        private void CreateLabel(Panel p, string t, int x, int y)
        {
            p.Controls.Add(new Label { Text = t, Location = new Point(x, y), AutoSize = true, ForeColor = Color.DimGray });
        }
        private Button CreateButton(string t, Color c, int x, int y)
        {
            return new Button { Text = t, BackColor = c, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Size = new Size(140, 40), Location = new Point(x, y), Font = new Font("Segoe UI", 10, FontStyle.Bold), Cursor = Cursors.Hand };
        }
    }
}