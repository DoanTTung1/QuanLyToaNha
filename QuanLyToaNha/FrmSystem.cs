using System;
using System.Drawing;
using System.Windows.Forms;

namespace QuanLyToaNha
{
    public partial class FrmSystem : Form
    {
        public FrmSystem()
        {
            InitializeComponent();
            DesignSystemSettings();
        }

        private void DesignSystemSettings()
        {
            this.BackColor = Color.FromArgb(240, 243, 250);
            this.AutoScroll = true;

            // --- SỬA LẠI: CẤU HÌNH ĐƠN VỊ VẬN HÀNH (Thay vì Tòa nhà) ---
            // Mục đích: Thông tin này dùng để in lên Header hóa đơn/báo cáo
            Panel pnlInfo = CreateSectionPanel("THÔNG TIN BAN QUẢN LÝ (BQL)", 20, 20);

            AddInputGroup(pnlInfo, "Tên Đơn Vị Vận Hành:", "Công Ty CP Quản Lý BĐS ABC", 20, 50);
            AddInputGroup(pnlInfo, "Trụ sở chính:", "Tầng 1, Tòa nhà Center, Q.1", 20, 100);
            AddInputGroup(pnlInfo, "Hotline Khẩn Cấp (24/7):", "1900 1000", 20, 150);
            AddInputGroup(pnlInfo, "Website / Fanpage:", "www.quanlytoanha.com", 20, 200);

            this.Controls.Add(pnlInfo);

            // --- CỤM 2: CẤU HÌNH TÀI CHÍNH (VAT, Tiền tệ) ---
            Panel pnlFinance = CreateSectionPanel("THIẾT LẬP THAM SỐ", 450, 20);

            AddInputGroup(pnlFinance, "Thuế VAT Mặc định (%):", "10", 20, 50);
            AddInputGroup(pnlFinance, "Ngày chốt điện nước:", "Ngày 20 hàng tháng", 20, 100);
            AddInputGroup(pnlFinance, "Hạn đóng tiền:", "Ngày 05 tháng sau", 20, 150);

            CheckBox chkAutoMail = new CheckBox();
            chkAutoMail.Text = "Gửi thông báo qua Zalo/Email";
            chkAutoMail.Font = new Font("Segoe UI", 10);
            chkAutoMail.Location = new Point(20, 210);
            chkAutoMail.AutoSize = true;
            chkAutoMail.Checked = true;
            pnlFinance.Controls.Add(chkAutoMail);

            this.Controls.Add(pnlFinance);

            // --- CỤM 3: CÁC NÚT CHỨC NĂNG ---
            Button btnSave = new Button();
            btnSave.Text = "LƯU THIẾT LẬP";
            btnSave.Size = new Size(180, 50);
            btnSave.Location = new Point(20, 350);
            btnSave.BackColor = Color.FromArgb(24, 161, 251);
            btnSave.ForeColor = Color.White;
            btnSave.FlatStyle = FlatStyle.Flat;
            btnSave.FlatAppearance.BorderSize = 0;
            btnSave.Font = new Font("Segoe UI", 11, FontStyle.Bold);
            btnSave.Cursor = Cursors.Hand;
            btnSave.Click += BtnSave_Click;
            this.Controls.Add(btnSave);

            Button btnBackup = new Button();
            btnBackup.Text = "SAO LƯU DỮ LIỆU";
            btnBackup.Size = new Size(180, 50);
            btnBackup.Location = new Point(220, 350);
            btnBackup.BackColor = Color.FromArgb(46, 51, 73);
            btnBackup.ForeColor = Color.White;
            btnBackup.FlatStyle = FlatStyle.Flat;
            btnBackup.FlatAppearance.BorderSize = 0;
            btnBackup.Font = new Font("Segoe UI", 11, FontStyle.Bold);
            btnBackup.Cursor = Cursors.Hand;
            btnBackup.Click += BtnBackup_Click;
            this.Controls.Add(btnBackup);
        }

        // --- GIỮ NGUYÊN CÁC HÀM VẼ GIAO DIỆN CŨ ---
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

        private void AddInputGroup(Panel parent, string labelText, string valueText, int x, int y)
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
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Đã cập nhật thông tin Ban Quản Lý thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void BtnBackup_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Đã tạo bản sao lưu Database!", "Backup", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}