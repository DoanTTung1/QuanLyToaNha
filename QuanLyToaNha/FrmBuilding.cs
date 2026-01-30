using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using System.Text;
using System.Drawing.Drawing2D; // Thêm thư viện này để vẽ bo góc

namespace QuanLyToaNha
{
    public partial class FrmBuilding : Form
    {
        private TextBox txtID, txtName, txtAddress, txtDescription, txtFloors;
        private PictureBox picBuilding;
        
        private string currentImagePath = "";
        private Label lblNoImage; // Label hiển thị khi không có ảnh

        public FrmBuilding()
        {
            InitializeComponent();
            this.Controls.Clear();
            this.BackColor = Color.White;

            DesignInputPanel();
            StyleDataGridView();
            LoadData();
        }

        // --- VALIDATION ---
        private bool ValidateInput()
        {
            if (string.IsNullOrWhiteSpace(txtName.Text)) { MessageBox.Show("Vui lòng nhập Tên Tòa Nhà!", "Thiếu thông tin", MessageBoxButtons.OK, MessageBoxIcon.Warning); txtName.Focus(); return false; }
            if (string.IsNullOrWhiteSpace(txtFloors.Text)) { MessageBox.Show("Vui lòng nhập Số Tầng!", "Thiếu thông tin", MessageBoxButtons.OK, MessageBoxIcon.Warning); txtFloors.Focus(); return false; }
            if (!int.TryParse(txtFloors.Text, out int f) || f <= 0) { MessageBox.Show("Số tầng phải > 0!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning); txtFloors.Focus(); return false; }
            if (string.IsNullOrWhiteSpace(txtAddress.Text)) { MessageBox.Show("Vui lòng nhập Địa Chỉ!", "Thiếu thông tin", MessageBoxButtons.OK, MessageBoxIcon.Warning); txtAddress.Focus(); return false; }
            return true;
        }

        // --- LOAD DATA ---
        private void LoadData()
        {
            try
            {
                string sql = "SELECT id, name, address, description, total_floors, image_path FROM buildings";
                dgvBuilding.DataSource = DatabaseHelper.GetData(sql);
                dgvBuilding.Columns["id"].HeaderText = "Mã Tòa";
                dgvBuilding.Columns["name"].HeaderText = "Tên Tòa Nhà";
                dgvBuilding.Columns["address"].HeaderText = "Địa Chỉ";
                dgvBuilding.Columns["description"].HeaderText = "Mô Tả";
                dgvBuilding.Columns["total_floors"].HeaderText = "Số Tầng";
                if (dgvBuilding.Columns.Contains("image_path")) dgvBuilding.Columns["image_path"].Visible = false;
            }
            catch
            {
                try
                {
                    string sql2 = "SELECT id, name, address, description, total_floors FROM buildings";
                    dgvBuilding.DataSource = DatabaseHelper.GetData(sql2);
                }
                catch { }
            }
        }

        // --- ACTIONS ---
        private void BtnAdd_Click(object sender, EventArgs e)
        {
            if (!ValidateInput()) return;
            try
            {
                string imgName = (!string.IsNullOrEmpty(currentImagePath) && File.Exists(currentImagePath)) ? SaveImageToFolder(currentImagePath) : "";
                string sql = dgvBuilding.Columns.Contains("image_path")
                    ? $"INSERT INTO buildings (name, address, description, total_floors, image_path) VALUES ('{txtName.Text}', '{txtAddress.Text}', '{txtDescription.Text}', '{txtFloors.Text}', '{imgName}')"
                    : $"INSERT INTO buildings (name, address, description, total_floors) VALUES ('{txtName.Text}', '{txtAddress.Text}', '{txtDescription.Text}', '{txtFloors.Text}')";

                DatabaseHelper.ExecuteSql(sql);
                MessageBox.Show("Thêm thành công!"); LoadData(); ClearInput();
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        }

        private void BtnEdit_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtID.Text)) return;
            if (!ValidateInput()) return;
            try
            {
                string imgName = currentImagePath.Contains(":") ? SaveImageToFolder(currentImagePath) : currentImagePath;
                string sql = dgvBuilding.Columns.Contains("image_path")
                    ? $"UPDATE buildings SET name='{txtName.Text}', address='{txtAddress.Text}', description='{txtDescription.Text}', total_floors='{txtFloors.Text}', image_path='{imgName}' WHERE id={txtID.Text}"
                    : $"UPDATE buildings SET name='{txtName.Text}', address='{txtAddress.Text}', description='{txtDescription.Text}', total_floors='{txtFloors.Text}' WHERE id={txtID.Text}";

                DatabaseHelper.ExecuteSql(sql);
                MessageBox.Show("Cập nhật xong!"); LoadData(); ClearInput();
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtID.Text)) return;
            if (MessageBox.Show("Xóa tòa nhà này?", "Xác nhận", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                try
                {
                    DatabaseHelper.ExecuteSql($"DELETE FROM buildings WHERE id={txtID.Text}");
                    MessageBox.Show("Đã xóa!"); LoadData(); ClearInput();
                }
                catch (Exception ex) { MessageBox.Show(ex.Message); }
            }
        }

        // --- IMAGE HELPERS ---
        private void BtnUpload_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog { Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp" };
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                picBuilding.Image = Image.FromFile(ofd.FileName);
                currentImagePath = ofd.FileName;
                lblNoImage.Visible = false; // Ẩn chữ "Chưa có ảnh"
            }
        }

        private void BtnRemoveImg_Click(object sender, EventArgs e)
        {
            picBuilding.Image = null;
            currentImagePath = "";
            lblNoImage.Visible = true; // Hiện lại chữ
        }

        private string SaveImageToFolder(string src)
        {
            try
            {
                string folder = Path.Combine(Application.StartupPath, "BuildingImages");
                if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);
                string newName = "B_" + DateTime.Now.Ticks + Path.GetExtension(src);
                File.Copy(src, Path.Combine(folder, newName), true);
                return newName;
            }
            catch { return ""; }
        }

        private void ClearInput()
        {
            txtID.Clear(); txtName.Clear(); txtAddress.Clear(); txtDescription.Clear(); txtFloors.Clear();
            picBuilding.Image = null; currentImagePath = ""; lblNoImage.Visible = true;
            txtName.Focus();
        }

        // --- UI DESIGN ---
        private void DesignInputPanel()
        {
            Panel pnlInput = new Panel { Dock = DockStyle.Top, Height = 320, BackColor = Color.WhiteSmoke };
            this.Controls.Add(pnlInput);

            // Cột Trái (Inputs)
            CreateInput(pnlInput, "Mã Tòa:", out txtID, 40, 20, 100); txtID.Enabled = false;
            CreateInput(pnlInput, "Tên Tòa Nhà (*):", out txtName, 180, 20, 250);
            CreateInput(pnlInput, "Số Tầng (*):", out txtFloors, 460, 20, 150);
            txtFloors.KeyPress += (s, e) => { if (!char.IsDigit(e.KeyChar) && !char.IsControl(e.KeyChar)) e.Handled = true; };

            CreateInput(pnlInput, "Địa Chỉ (*):", out txtAddress, 40, 90, 400);
            CreateInput(pnlInput, "Mô Tả / Tiện Ích:", out txtDescription, 460, 90, 250);

            // Cột Phải (ẢNH ĐẸP HƠN)
            GroupBox grpImg = new GroupBox { Text = "Hình Ảnh Minh Họa", Location = new Point(750, 15), Size = new Size(220, 230), ForeColor = Color.DimGray };
            pnlInput.Controls.Add(grpImg);

            // Khung ảnh
            picBuilding = new PictureBox { Location = new Point(10, 25), Size = new Size(200, 150), BorderStyle = BorderStyle.FixedSingle, SizeMode = PictureBoxSizeMode.Zoom, BackColor = Color.White };
            grpImg.Controls.Add(picBuilding);

            // Label "Chưa có ảnh" nằm giữa PictureBox
            lblNoImage = new Label { Text = "Chưa có ảnh", AutoSize = false, TextAlign = ContentAlignment.MiddleCenter, Dock = DockStyle.Fill, ForeColor = Color.LightGray, Font = new Font("Segoe UI", 10, FontStyle.Italic) };
            picBuilding.Controls.Add(lblNoImage);

            // Nút Upload (Icon style)
            Button btnUp = new Button { Text = "📁 Chọn Ảnh", Location = new Point(10, 185), Size = new Size(95, 35), BackColor = Color.FromArgb(52, 152, 219), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9, FontStyle.Bold), Cursor = Cursors.Hand };
            btnUp.FlatAppearance.BorderSize = 0;
            btnUp.Click += BtnUpload_Click;
            grpImg.Controls.Add(btnUp);

            // Nút Xóa Ảnh
            Button btnDelImg = new Button { Text = "❌ Xóa", Location = new Point(115, 185), Size = new Size(95, 35), BackColor = Color.FromArgb(231, 76, 60), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9, FontStyle.Bold), Cursor = Cursors.Hand };
            btnDelImg.FlatAppearance.BorderSize = 0;
            btnDelImg.Click += BtnRemoveImg_Click;
            grpImg.Controls.Add(btnDelImg);

            // Buttons Chức Năng Chính
            int y = 260;
            Button btnAdd = CreateButton("THÊM MỚI", Color.FromArgb(46, 204, 113), 40, y); btnAdd.Click += BtnAdd_Click;
            Button btnEdit = CreateButton("CẬP NHẬT", Color.FromArgb(243, 156, 18), 200, y); btnEdit.Click += BtnEdit_Click;
            Button btnDel = CreateButton("XÓA BỎ", Color.FromArgb(231, 76, 60), 360, y); btnDel.Click += BtnDelete_Click;
            Button btnRef = CreateButton("LÀM MỚI", Color.Gray, 520, y); btnRef.Click += (s, e) => { ClearInput(); LoadData(); };

            pnlInput.Controls.AddRange(new Control[] { btnAdd, btnEdit, btnDel, btnRef });
        }

        private void StyleDataGridView()
        {
            dgvBuilding = new DataGridView { Dock = DockStyle.Fill, BackgroundColor = Color.White, BorderStyle = BorderStyle.None, RowHeadersVisible = false, AllowUserToAddRows = false, ReadOnly = true, SelectionMode = DataGridViewSelectionMode.FullRowSelect, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, RowTemplate = { Height = 40 }, ColumnHeadersHeight = 45, EnableHeadersVisualStyles = false };
            dgvBuilding.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(24, 30, 54);
            dgvBuilding.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvBuilding.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            dgvBuilding.DefaultCellStyle.Font = new Font("Segoe UI", 10);

            this.Controls.Add(dgvBuilding);
            dgvBuilding.BringToFront();

            dgvBuilding.CellClick += (s, e) => {
                if (e.RowIndex >= 0)
                {
                    var r = dgvBuilding.Rows[e.RowIndex];
                    txtID.Text = r.Cells["id"].Value.ToString();
                    txtName.Text = r.Cells["name"].Value.ToString();
                    txtAddress.Text = r.Cells["address"].Value.ToString();
                    txtDescription.Text = r.Cells["description"].Value.ToString();
                    txtFloors.Text = r.Cells["total_floors"].Value.ToString();

                    if (dgvBuilding.Columns.Contains("image_path"))
                    {
                        string img = r.Cells["image_path"].Value?.ToString();
                        currentImagePath = img;
                        if (!string.IsNullOrEmpty(img))
                        {
                            string p = Path.Combine(Application.StartupPath, "BuildingImages", img);
                            if (File.Exists(p)) { picBuilding.Image = Image.FromFile(p); lblNoImage.Visible = false; }
                            else { picBuilding.Image = null; lblNoImage.Visible = true; }
                        }
                        else { picBuilding.Image = null; lblNoImage.Visible = true; }
                    }
                }
            };
        }

        private void CreateInput(Panel p, string l, out TextBox t, int x, int y, int w)
        {
            p.Controls.Add(new Label { Text = l, Location = new Point(x, y), AutoSize = true, ForeColor = Color.DimGray, Font = new Font("Segoe UI", 9, FontStyle.Bold) });
            t = new TextBox { Location = new Point(x, y + 25), Size = new Size(w, 30), Font = new Font("Segoe UI", 11), BorderStyle = BorderStyle.FixedSingle };
            p.Controls.Add(t);
        }

        private Button CreateButton(string t, Color c, int x, int y)
        {
            return new Button { Text = t, BackColor = c, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Size = new Size(140, 40), Location = new Point(x, y), Font = new Font("Segoe UI", 10, FontStyle.Bold), Cursor = Cursors.Hand };
        }
    }
}