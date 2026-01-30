using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;

namespace QuanLyToaNha
{
    public partial class FrmRequest : Form
    {
        private TextBox txtID, txtResident, txtRoom, txtTitle, txtContent, txtDate;
        private ComboBox cbbStatus;
        private DataGridView dgvRequest;

        // Từ điển trạng thái (Anh -> Việt)
        private Dictionary<string, string> statusDict = new Dictionary<string, string>()
        {
            { "Pending", "⏳ Chờ Xử Lý" },
            { "Processing", "🔧 Đang Xử Lý" },
            { "Done", "✅ Đã Hoàn Thành" },
            { "Cancelled", "❌ Đã Hủy" }
        };

        public FrmRequest()
        {
            InitializeComponent();
            this.Controls.Clear();
            this.BackColor = Color.White;

            DesignInputPanel();
            StyleDataGridView();
            LoadStatusComboBox();
            LoadData();
        }

        // --- 1. LOAD DỮ LIỆU ---
        private void LoadStatusComboBox()
        {
            cbbStatus.DataSource = new BindingSource(statusDict, null);
            cbbStatus.DisplayMember = "Value"; // Hiển thị tiếng Việt
            cbbStatus.ValueMember = "Key";     // Lưu xuống DB tiếng Anh
        }

        private void LoadData()
        {
            try
            {
                // Lấy dữ liệu yêu cầu + Tên cư dân + Số phòng
                string sql = @"
                    SELECT req.id, u.full_name AS 'Cư Dân', r.room_number AS 'Phòng',
                           req.title AS 'Tiêu Đề', req.description AS 'Nội Dung',
                           req.created_at AS 'Ngày Gửi', req.status AS 'Trạng Thái Gốc'
                    FROM requests req
                    LEFT JOIN users u ON req.user_id = u.id
                    LEFT JOIN rooms r ON req.room_id = r.id
                    ORDER BY req.created_at DESC";

                DataTable dt = DatabaseHelper.GetData(sql);

                // Thêm cột hiển thị tiếng Việt
                dt.Columns.Add("Trạng Thái", typeof(string));
                foreach (DataRow row in dt.Rows)
                {
                    string en = row["Trạng Thái Gốc"].ToString();
                    row["Trạng Thái"] = statusDict.ContainsKey(en) ? statusDict[en] : en;
                }

                dgvRequest.DataSource = dt;

                // Ẩn cột ID và Status Gốc
                if (dgvRequest.Columns["id"] != null) dgvRequest.Columns["id"].Visible = false;
                if (dgvRequest.Columns["Trạng Thái Gốc"] != null) dgvRequest.Columns["Trạng Thái Gốc"].Visible = false;

                // Format ngày giờ
                if (dgvRequest.Columns["Ngày Gửi"] != null)
                    dgvRequest.Columns["Ngày Gửi"].DefaultCellStyle.Format = "dd/MM/yyyy HH:mm";
            }
            catch (Exception ex) { MessageBox.Show("Lỗi tải dữ liệu: " + ex.Message); }
        }

        // --- 2. XỬ LÝ SỰ KIỆN ---

        // Cập nhật trạng thái xử lý
        private void BtnUpdate_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtID.Text)) { MessageBox.Show("Vui lòng chọn yêu cầu cần xử lý!"); return; }

            try
            {
                string newStatus = cbbStatus.SelectedValue.ToString(); // Lấy Key (Pending/Done...)
                string sql = $"UPDATE requests SET status='{newStatus}' WHERE id={txtID.Text}";

                DatabaseHelper.ExecuteSql(sql);
                MessageBox.Show($"Đã cập nhật trạng thái thành công!", "Thành công");
                LoadData();
            }
            catch (Exception ex) { MessageBox.Show("Lỗi: " + ex.Message); }
        }

        // Xóa yêu cầu (Chỉ dùng cho tin spam hoặc đã xong lâu)
        private void BtnDelete_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtID.Text)) return;
            if (MessageBox.Show("Bạn muốn xóa vĩnh viễn yêu cầu này?", "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                DatabaseHelper.ExecuteSql($"DELETE FROM requests WHERE id={txtID.Text}");
                MessageBox.Show("Đã xóa!");
                LoadData(); ClearInput();
            }
        }

        // --- UI & HELPERS ---
        private void ClearInput()
        {
            txtID.Clear(); txtResident.Clear(); txtRoom.Clear(); txtTitle.Clear(); txtContent.Clear(); txtDate.Clear();
        }

        private void DesignInputPanel()
        {
            Panel pnlInput = new Panel { Dock = DockStyle.Top, Height = 300, BackColor = Color.WhiteSmoke };
            this.Controls.Add(pnlInput);

            // Cột 1: Thông tin người gửi (Chỉ xem)
            GroupBox grpInfo = new GroupBox { Text = "Thông Tin Người Gửi", Location = new Point(20, 15), Size = new Size(300, 150), Font = new Font("Segoe UI", 10, FontStyle.Bold), ForeColor = Color.DimGray };
            pnlInput.Controls.Add(grpInfo);

            CreateReadOnlyInput(grpInfo, "Mã YC:", out txtID, 15, 30, 60);
            CreateReadOnlyInput(grpInfo, "Cư Dân:", out txtResident, 90, 30, 190);
            CreateReadOnlyInput(grpInfo, "Phòng:", out txtRoom, 15, 90, 80);
            CreateReadOnlyInput(grpInfo, "Ngày Gửi:", out txtDate, 110, 90, 170);

            // Cột 2: Nội dung sự cố (Chỉ xem)
            GroupBox grpContent = new GroupBox { Text = "Chi Tiết Sự Cố", Location = new Point(340, 15), Size = new Size(400, 260), Font = new Font("Segoe UI", 10, FontStyle.Bold), ForeColor = Color.DimGray };
            pnlInput.Controls.Add(grpContent);

            CreateReadOnlyInput(grpContent, "Tiêu Đề:", out txtTitle, 15, 30, 370);

            Label lblDesc = new Label { Text = "Nội Dung:", Location = new Point(15, 90), AutoSize = true, Font = new Font("Segoe UI", 10, FontStyle.Regular) };
            grpContent.Controls.Add(lblDesc);

            txtContent = new TextBox { Location = new Point(15, 115), Size = new Size(370, 130), Multiline = true, ScrollBars = ScrollBars.Vertical, ReadOnly = true, BackColor = Color.White, Font = new Font("Segoe UI", 11, FontStyle.Regular) };
            grpContent.Controls.Add(txtContent);

            // Cột 3: Hành động Admin
            GroupBox grpAction = new GroupBox { Text = "Xử Lý (Admin)", Location = new Point(760, 15), Size = new Size(250, 260), Font = new Font("Segoe UI", 10, FontStyle.Bold), ForeColor = Color.DimGray };
            pnlInput.Controls.Add(grpAction);

            Label lblStt = new Label { Text = "Cập Nhật Trạng Thái:", Location = new Point(20, 40), AutoSize = true, Font = new Font("Segoe UI", 10, FontStyle.Regular) };
            grpAction.Controls.Add(lblStt);

            cbbStatus = new ComboBox { Location = new Point(20, 65), Width = 210, DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 11) };
            grpAction.Controls.Add(cbbStatus);

            Button btnUpdate = CreateButton("LƯU TRẠNG THÁI", Color.FromArgb(39, 174, 96), 20, 120);
            btnUpdate.Click += BtnUpdate_Click;
            grpAction.Controls.Add(btnUpdate);

            Button btnDel = CreateButton("XÓA YÊU CẦU", Color.IndianRed, 20, 180);
            btnDel.Click += BtnDelete_Click;
            grpAction.Controls.Add(btnDel);
        }

        private void StyleDataGridView()
        {
            dgvRequest = new DataGridView { Dock = DockStyle.Fill, BorderStyle = BorderStyle.None, BackgroundColor = Color.White, RowTemplate = { Height = 40 }, ColumnHeadersHeight = 50, EnableHeadersVisualStyles = false, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, SelectionMode = DataGridViewSelectionMode.FullRowSelect, RowHeadersVisible = false, ReadOnly = true, AllowUserToAddRows = false };
            dgvRequest.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle { BackColor = Color.FromArgb(24, 30, 54), ForeColor = Color.White, Font = new Font("Segoe UI", 11, FontStyle.Bold) };
            this.Controls.Add(dgvRequest); dgvRequest.BringToFront();

            // Click bảng -> Đổ dữ liệu
            dgvRequest.CellClick += (s, e) => {
                if (e.RowIndex >= 0)
                {
                    var r = dgvRequest.Rows[e.RowIndex];
                    txtID.Text = r.Cells["id"].Value.ToString();
                    txtResident.Text = r.Cells["Cư Dân"].Value.ToString();
                    txtRoom.Text = r.Cells["Phòng"].Value.ToString();
                    txtDate.Text = r.Cells["Ngày Gửi"].Value.ToString();
                    txtTitle.Text = r.Cells["Tiêu Đề"].Value.ToString();
                    txtContent.Text = r.Cells["Nội Dung"].Value.ToString();

                    // Chọn đúng item trong combobox theo trạng thái tiếng Anh
                    string enStatus = r.Cells["Trạng Thái Gốc"].Value.ToString();
                    cbbStatus.SelectedValue = enStatus;
                }
            };

            // Tô màu trạng thái
            dgvRequest.CellFormatting += (s, e) => {
                if (dgvRequest.Columns[e.ColumnIndex].Name == "Trạng Thái")
                {
                    string val = e.Value.ToString();
                    if (val.Contains("Chờ")) e.CellStyle.ForeColor = Color.Red;
                    else if (val.Contains("Đang")) e.CellStyle.ForeColor = Color.OrangeRed;
                    else if (val.Contains("Hoàn Thành")) e.CellStyle.ForeColor = Color.Green;
                    e.CellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
                }
            };
        }

        // Helper tạo ô nhập chỉ đọc (ReadOnly) - Dành cho thông tin hiển thị
        private void CreateReadOnlyInput(GroupBox g, string l, out TextBox t, int x, int y, int w)
        {
            g.Controls.Add(new Label { Text = l, Location = new Point(x, y), AutoSize = true, Font = new Font("Segoe UI", 10, FontStyle.Regular) });
            t = new TextBox { Location = new Point(x, y + 25), Size = new Size(w, 30), Font = new Font("Segoe UI", 11), ReadOnly = true, BackColor = Color.White };
            g.Controls.Add(t);
        }

        private Button CreateButton(string t, Color c, int x, int y)
        {
            return new Button { Text = t, BackColor = c, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Size = new Size(210, 45), Location = new Point(x, y), Font = new Font("Segoe UI", 10, FontStyle.Bold), Cursor = Cursors.Hand };
        }
    }
}