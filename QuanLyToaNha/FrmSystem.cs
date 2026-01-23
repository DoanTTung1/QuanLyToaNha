using System;
using System.Drawing;
using System.Windows.Forms;
using System.IO; // Dùng để ghi file backup
using System.Data;
using MySql.Data.MySqlClient; // QUAN TRỌNG: Thư viện MySQL

namespace QuanLyToaNha
{
    public partial class FrmSystem : Form
    {
        // Khai báo các biến TextBox để lưu trữ tham chiếu
        private TextBox txtCompany, txtAddress, txtHotline, txtWeb;
        private TextBox txtVat, txtCutDate, txtDueDate;
        private CheckBox chkAutoMail;

        public FrmSystem()
        {
            InitializeComponent();

            // 1. Tạo giao diện
            DesignSystemSettings();

            // 2. Tự động tạo bảng Config nếu chưa có & Load dữ liệu
            InitDatabaseAndLoad();
        }

        private void InitDatabaseAndLoad()
        {
            try
            {
                // Tạo bảng system_config nếu chưa tồn tại
                string createTableSql = @"
                    CREATE TABLE IF NOT EXISTS system_config (
                        cfg_key VARCHAR(50) PRIMARY KEY,
                        cfg_value TEXT
                    );";
                DatabaseHelper.ExecuteSql(createTableSql);

                // Load dữ liệu cũ lên màn hình
                DataTable dt = DatabaseHelper.GetData("SELECT * FROM system_config");
                foreach (DataRow row in dt.Rows)
                {
                    string key = row["cfg_key"].ToString();
                    string val = row["cfg_value"].ToString();

                    if (key == "Company") txtCompany.Text = val;
                    if (key == "Address") txtAddress.Text = val;
                    if (key == "Hotline") txtHotline.Text = val;
                    if (key == "Web") txtWeb.Text = val;
                    if (key == "VAT") txtVat.Text = val;
                }
            }
            catch { } // Bỏ qua lỗi nếu lần đầu chạy
        }

        private void DesignSystemSettings()
        {
            this.BackColor = Color.FromArgb(240, 243, 250);
            this.AutoScroll = true;

            // --- CỤM 1: THÔNG TIN VẬN HÀNH ---
            Panel pnlInfo = CreateSectionPanel("THÔNG TIN BAN QUẢN LÝ (BQL)", 20, 20);

            AddInputGroup(pnlInfo, "Tên Đơn Vị Vận Hành:", "Công Ty CP Quản Lý BĐS ABC", out txtCompany, 20, 50);
            AddInputGroup(pnlInfo, "Trụ sở chính:", "Tầng 1, Tòa nhà Center, Q.1", out txtAddress, 20, 100);
            AddInputGroup(pnlInfo, "Hotline Khẩn Cấp (24/7):", "1900 1000", out txtHotline, 20, 150);
            AddInputGroup(pnlInfo, "Website / Fanpage:", "www.quanlytoanha.com", out txtWeb, 20, 200);

            this.Controls.Add(pnlInfo);

            // --- CỤM 2: CẤU HÌNH TÀI CHÍNH ---
            Panel pnlFinance = CreateSectionPanel("THIẾT LẬP THAM SỐ", 450, 20);

            AddInputGroup(pnlFinance, "Thuế VAT Mặc định (%):", "10", out txtVat, 20, 50);
            AddInputGroup(pnlFinance, "Ngày chốt điện nước:", "20", out txtCutDate, 20, 100);
            AddInputGroup(pnlFinance, "Hạn đóng tiền (Ngày):", "05", out txtDueDate, 20, 150);

            chkAutoMail = new CheckBox();
            chkAutoMail.Text = "Gửi thông báo tự động qua Email";
            chkAutoMail.Font = new Font("Segoe UI", 10);
            chkAutoMail.Location = new Point(20, 210);
            chkAutoMail.AutoSize = true;
            chkAutoMail.Checked = true;
            pnlFinance.Controls.Add(chkAutoMail);

            this.Controls.Add(pnlFinance);

            // --- CỤM 3: CÁC NÚT CHỨC NĂNG ---
            Button btnSave = CreateButton("LƯU THIẾT LẬP", Color.FromArgb(24, 161, 251), 20, 350);
            btnSave.Click += BtnSave_Click;
            this.Controls.Add(btnSave);

            Button btnBackup = CreateButton("SAO LƯU DỮ LIỆU", Color.FromArgb(46, 51, 73), 220, 350);
            btnBackup.Click += BtnBackup_Click;
            this.Controls.Add(btnBackup);
        }

        // --- XỬ LÝ LƯU SETTING ---
        private void BtnSave_Click(object sender, EventArgs e)
        {
            try
            {
                // Lưu từng dòng vào bảng system_config
                SaveConfig("Company", txtCompany.Text);
                SaveConfig("Address", txtAddress.Text);
                SaveConfig("Hotline", txtHotline.Text);
                SaveConfig("Web", txtWeb.Text);
                SaveConfig("VAT", txtVat.Text);

                MessageBox.Show("Đã lưu cấu hình vào Database thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi lưu trữ: " + ex.Message);
            }
        }

        private void SaveConfig(string key, string val)
        {
            // Dùng cú pháp INSERT ... ON DUPLICATE KEY UPDATE của MySQL
            string sql = $"INSERT INTO system_config (cfg_key, cfg_value) VALUES ('{key}', '{val}') " +
                         $"ON DUPLICATE KEY UPDATE cfg_value = '{val}'";
            DatabaseHelper.ExecuteSql(sql);
        }

        // --- XỬ LÝ BACKUP (Xuất file CSV đơn giản) ---
        private void BtnBackup_Click(object sender, EventArgs e)
        {
            try
            {
                SaveFileDialog sfd = new SaveFileDialog();
                sfd.Filter = "CSV File|*.csv";
                sfd.FileName = "Backup_KhachHang_" + DateTime.Now.ToString("ddMMyyyy") + ".csv";

                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    // Lấy dữ liệu khách hàng để backup
                    DataTable dt = DatabaseHelper.GetData("SELECT * FROM customers");

                    using (StreamWriter sw = new StreamWriter(sfd.FileName))
                    {
                        // Viết Header
                        sw.WriteLine("ID,Full Name,Phone,Address,Email");

                        // Viết Data
                        foreach (DataRow row in dt.Rows)
                        {
                            string line = $"{row["id"]},{row["full_name"]},{row["phone"]},{row["address"]},{row["email"]}";
                            sw.WriteLine(line);
                        }
                    }
                    MessageBox.Show("Đã sao lưu danh sách Khách Hàng thành công!", "Backup", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi backup: " + ex.Message);
            }
        }

        // --- CÁC HÀM HỖ TRỢ UI ---
        private Panel CreateSectionPanel(string title, int x, int y)
        {
            Panel pnl = new Panel();
            pnl.Size = new Size(400, 300);
            pnl.Location = new Point(x, y);
            pnl.BackColor = Color.White;

            Label lblTitle = new Label();
            lblTitle.Text = title;
            lblTitle.Font = new Font("Segoe UI", 12, FontStyle.Bold);
            lblTitle.ForeColor = Color.FromArgb(24, 30, 54);
            lblTitle.Location = new Point(15, 15);
            lblTitle.AutoSize = true;
            pnl.Controls.Add(lblTitle);

            Panel line = new Panel();
            line.Size = new Size(370, 2);
            line.BackColor = Color.LightGray;
            line.Location = new Point(15, 45);
            pnl.Controls.Add(line);

            return pnl;
        }

        // Sửa hàm này để trả về TextBox (out) giúp ta lưu tham chiếu
        private void AddInputGroup(Panel parent, string labelText, string valueText, out TextBox txtOut, int x, int y)
        {
            Label lbl = new Label();
            lbl.Text = labelText;
            lbl.Font = new Font("Segoe UI", 10, FontStyle.Regular);
            lbl.ForeColor = Color.Gray;
            lbl.Location = new Point(x, y);
            lbl.AutoSize = true;
            parent.Controls.Add(lbl);

            TextBox txt = new TextBox();
            txt.Text = valueText;
            txt.Font = new Font("Segoe UI", 11, FontStyle.Regular);
            txt.Location = new Point(x, y + 25);
            txt.Size = new Size(350, 30);
            txt.BorderStyle = BorderStyle.FixedSingle;
            parent.Controls.Add(txt);

            txtOut = txt; // Gán tham chiếu ra ngoài để dùng sau này
        }

        private Button CreateButton(string text, Color bg, int x, int y)
        {
            Button btn = new Button();
            btn.Text = text;
            btn.Size = new Size(180, 50);
            btn.Location = new Point(x, y);
            btn.BackColor = bg;
            btn.ForeColor = Color.White;
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 0;
            btn.Font = new Font("Segoe UI", 11, FontStyle.Bold);
            btn.Cursor = Cursors.Hand;
            return btn;
        }
    }
}