using System;
using System.Drawing;
using System.Windows.Forms;

namespace QuanLyToaNha
{
    public partial class FrmBuilding : Form
    {
        public FrmBuilding()
        {
            InitializeComponent();
            LoadFakeData();
            StyleDataGridView();
        }

        private void StyleDataGridView()
        {
            // 1. Cấu hình chung
            dgvBuilding.BorderStyle = BorderStyle.None;
            dgvBuilding.BackgroundColor = Color.White;
            dgvBuilding.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;

            // 2. Tăng chiều cao dòng & Header (RỘNG RA LÀ ĐÂY)
            dgvBuilding.RowTemplate.Height = 55; // <--- Tăng từ 40 lên 55 (Rất thoáng)
            dgvBuilding.ColumnHeadersHeight = 60; // <--- Header cũng cao lên

            // 3. Màu sắc & Font
            dgvBuilding.DefaultCellStyle.SelectionBackColor = Color.FromArgb(46, 51, 73);
            dgvBuilding.DefaultCellStyle.SelectionForeColor = Color.WhiteSmoke;

            // Header
            dgvBuilding.EnableHeadersVisualStyles = false;
            dgvBuilding.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
            dgvBuilding.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(24, 30, 54);
            dgvBuilding.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvBuilding.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 12, FontStyle.Bold); // Font to hơn xíu

            // 4. Thêm Padding (Khoảng cách bên trong ô) cho chữ dễ thở
            dgvBuilding.DefaultCellStyle.Padding = new Padding(10, 0, 0, 0);
        }

        private void LoadFakeData()
        {
            dgvBuilding.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvBuilding.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvBuilding.RowHeadersVisible = false;
            dgvBuilding.AllowUserToAddRows = false;
            dgvBuilding.ReadOnly = true;

            dgvBuilding.Columns.Add("ID", "MÃ");
            dgvBuilding.Columns.Add("Name", "TÊN TÒA NHÀ");
            dgvBuilding.Columns.Add("Address", "ĐỊA CHỈ");
            dgvBuilding.Columns.Add("Manager", "QUẢN LÝ");
            dgvBuilding.Columns.Add("Price", "GIÁ THUÊ");

            // Thêm dữ liệu
            dgvBuilding.Rows.Add("BD001", "🏢 Landmark 81", "720A Điện Biên Phủ, HCM", "Nguyễn Văn A", "$5,000");
            dgvBuilding.Rows.Add("BD002", "🏢 Bitexco Financial", "2 Hải Triều, Q.1", "Trần Thị B", "$4,500");
            dgvBuilding.Rows.Add("BD003", "🏢 Keangnam Hanoi", "Phạm Hùng, Hà Nội", "Lê Văn C", "$4,200");
            dgvBuilding.Rows.Add("BD004", "🏢 Lotte Center", "54 Liễu Giai, Ba Đình", "Phạm Thị D", "$4,000");
            dgvBuilding.Rows.Add("BD005", "🏢 Vincom Center", "72 Lê Thánh Tôn, Q.1", "Hoàng Văn E", "$3,800");

            dgvBuilding.DefaultCellStyle.Font = new Font("Segoe UI", 11, FontStyle.Regular); // Font chữ nội dung cũng to lên

            dgvBuilding.Columns["ID"].Width = 80; // Cột mã nhỏ lại chút
            dgvBuilding.Columns["Price"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
        }
    }
}