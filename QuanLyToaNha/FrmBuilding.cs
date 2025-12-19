using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace QuanLyToaNha
{
    public partial class FrmBuilding : Form
    {
        private DataTable dtBuilding;
        private TextBox txtID, txtName, txtAddress, txtManager, txtPrice;

        public FrmBuilding()
        {
            InitializeComponent();
            DesignInputPanel();
            InitFakeData();
            StyleDataGridView();
        }

        private void DesignInputPanel()
        {
            // Panel cao 240px cho rộng rãi
            Panel pnlInput = new Panel { Dock = DockStyle.Top, Height = 240, BackColor = Color.WhiteSmoke };
            this.Controls.Add(pnlInput);

            // --- DÒNG 1 ---
            CreateInput(pnlInput, "Mã Tòa:", out txtID, 40, 30, 100);
            txtID.Enabled = false; txtID.Text = "AUTO";

            CreateInput(pnlInput, "Tên Tòa Nhà:", out txtName, 180, 30, 250);
            CreateInput(pnlInput, "Quản Lý Phụ Trách:", out txtManager, 460, 30, 250);

            // --- DÒNG 2 (Cách xa dòng 1) ---
            CreateInput(pnlInput, "Địa Chỉ:", out txtAddress, 40, 100, 400); // Ô địa chỉ dài
            CreateInput(pnlInput, "Giá Thuê ($):", out txtPrice, 460, 100, 150);

            // --- NÚT BẤM (Hạ thấp xuống Y=170) ---
            int btnY = 170;
            Button btnAdd = CreateButton("THÊM TÒA NHÀ", Color.FromArgb(24, 161, 251), 40, btnY);
            btnAdd.Click += (s, e) => {
                if (string.IsNullOrWhiteSpace(txtName.Text)) return;
                dtBuilding.Rows.Add("BD" + new Random().Next(100, 999), "🏢 " + txtName.Text, txtAddress.Text, txtManager.Text, "$" + txtPrice.Text);
                ClearInput();
            };
            pnlInput.Controls.Add(btnAdd);

            Button btnEdit = CreateButton("CẬP NHẬT", Color.FromArgb(255, 193, 7), 200, btnY);
            btnEdit.Click += (s, e) => {
                if (dgvBuilding.SelectedRows.Count == 0) return;
                DataRow r = dtBuilding.Rows[dgvBuilding.CurrentRow.Index];
                r["Name"] = "🏢 " + txtName.Text; r["Address"] = txtAddress.Text;
                r["Manager"] = txtManager.Text; r["Price"] = "$" + txtPrice.Text.Replace("$", "");
                MessageBox.Show("Cập nhật xong!"); ClearInput();
            };
            pnlInput.Controls.Add(btnEdit);

            Button btnDelete = CreateButton("XÓA TÒA NHÀ", Color.FromArgb(253, 138, 114), 360, btnY);
            btnDelete.Click += (s, e) => {
                if (dgvBuilding.SelectedRows.Count > 0 && MessageBox.Show("Xóa tòa nhà này?", "Confirm", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    dtBuilding.Rows[dgvBuilding.CurrentRow.Index].Delete(); ClearInput();
                }
            };
            pnlInput.Controls.Add(btnDelete);

            Button btnClear = CreateButton("LÀM MỚI", Color.Gray, 520, btnY);
            btnClear.Click += (s, e) => ClearInput();
            pnlInput.Controls.Add(btnClear);
        }

        // --- CÁC HÀM HỖ TRỢ ---
        private void InitFakeData()
        {
            dtBuilding = new DataTable();
            dtBuilding.Columns.Add("ID"); dtBuilding.Columns.Add("Name"); dtBuilding.Columns.Add("Address");
            dtBuilding.Columns.Add("Manager"); dtBuilding.Columns.Add("Price");
            dtBuilding.Rows.Add("BD001", "🏢 Landmark 81", "720A Điện Biên Phủ", "Nguyễn Văn A", "$5,000");
            dtBuilding.Rows.Add("BD002", "🏢 Bitexco", "2 Hải Triều, Q.1", "Trần Thị B", "$4,500");
            dgvBuilding.DataSource = dtBuilding;

            dgvBuilding.CellClick += (s, e) => {
                if (e.RowIndex >= 0)
                {
                    var r = dgvBuilding.Rows[e.RowIndex];
                    txtID.Text = r.Cells["ID"].Value.ToString();
                    txtName.Text = r.Cells["Name"].Value.ToString().Replace("🏢 ", "");
                    txtAddress.Text = r.Cells["Address"].Value.ToString();
                    txtManager.Text = r.Cells["Manager"].Value.ToString();
                    txtPrice.Text = r.Cells["Price"].Value.ToString().Replace("$", "");
                }
            };
        }
        private void ClearInput() { txtID.Text = "AUTO"; txtName.Clear(); txtAddress.Clear(); txtManager.Clear(); txtPrice.Clear(); txtName.Focus(); }

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
            dgvBuilding.BorderStyle = BorderStyle.None; dgvBuilding.BackgroundColor = Color.White;
            dgvBuilding.RowTemplate.Height = 55; dgvBuilding.ColumnHeadersHeight = 60; // Dòng cao
            dgvBuilding.EnableHeadersVisualStyles = false;
            dgvBuilding.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(24, 30, 54);
            dgvBuilding.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvBuilding.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 12, FontStyle.Bold);
            dgvBuilding.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvBuilding.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvBuilding.RowHeadersVisible = false; dgvBuilding.ReadOnly = true;
            dgvBuilding.Columns["ID"].HeaderText = "MÃ"; dgvBuilding.Columns["Name"].HeaderText = "TÊN TÒA NHÀ";
            dgvBuilding.Columns["Address"].HeaderText = "ĐỊA CHỈ"; dgvBuilding.Columns["Manager"].HeaderText = "QUẢN LÝ";
            dgvBuilding.Columns["Price"].HeaderText = "GIÁ THUÊ";
        }
    }
}