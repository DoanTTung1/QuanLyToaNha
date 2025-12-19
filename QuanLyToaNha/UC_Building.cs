using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace QuanLyToaNha
{
    public partial class UC_Building : UserControl
    {
        // --- ĐÂY LÀ HÀM KHỞI TẠO (CHẠY ĐẦU TIÊN) ---
        public UC_Building()
        {
            InitializeComponent();

            // CHUYỂN 2 DÒNG NÀY VÀO ĐÂY LÀ ĐƯỢC
            LoadDummyData();
            StyleDataGridView();
        }

        // --- Hàm tạo dữ liệu ảo (Giữ nguyên) ---
        private void LoadDummyData()
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("ID", typeof(int));
            dt.Columns.Add("Tên Tòa Nhà", typeof(string));
            dt.Columns.Add("Địa Chỉ", typeof(string));
            dt.Columns.Add("Kết Cấu", typeof(string));
            dt.Columns.Add("Giá Thuê ($)", typeof(decimal));
            dt.Columns.Add("Quản Lý", typeof(string));
            dt.Columns.Add("SĐT", typeof(string));

            dt.Rows.Add(1, "Nam Giao Building Tower", "59 Phan Xích Long, Q1", "2 Hầm", 15, "Anh Nam", "0915354727");
            dt.Rows.Add(2, "ACM Tower", "96 Cao Thắng, Q3", "2 Hầm", 18, "Chú Thuận", "0173546263");
            dt.Rows.Add(3, "Alpha 2 Building", "153 Nguyễn Đình Chiểu, Q3", "1 Hầm", 20, "Cô Lý", "0555532578");
            dt.Rows.Add(4, "IDD 1 Building", "111 Lý Chính Thắng, Q3", "1 Hầm", 12, "Anh Long", "017345253");

            // Lưu ý: Đảm bảo tên DataGridView bên Designer đúng là dgvBuilding
            dgvBuilding.DataSource = dt;
        }

        // --- Hàm trang trí (Giữ nguyên) ---
        private void StyleDataGridView()
        {
            // Kiểm tra tránh lỗi nếu chưa tạo GridView
            if (dgvBuilding != null)
            {
                dgvBuilding.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                dgvBuilding.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
                dgvBuilding.BackgroundColor = Color.White;
            }
        }
    }
}