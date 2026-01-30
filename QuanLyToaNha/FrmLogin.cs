using System;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using MySql.Data.MySqlClient; // Ensure MySQL library is referenced

namespace QuanLyToaNha
{
    public partial class FrmLogin : Form
    {
        // --- Code to move borderless Form ---
        [DllImport("user32.DLL", EntryPoint = "ReleaseCapture")]
        private extern static void ReleaseCapture();
        [DllImport("user32.DLL", EntryPoint = "SendMessage")]
        private extern static void SendMessage(System.IntPtr hwnd, int wmsg, int wparam, int lparam);

        public FrmLogin()
        {
            InitializeComponent();
            this.StartPosition = FormStartPosition.CenterScreen;
            // Ensure password box masks characters
            if (textBox1 != null) textBox1.UseSystemPasswordChar = true;
        }

        // --- LOGIN BUTTON EVENT (UPDATED FOR ROLE-BASED ACCESS) ---
        private void btnLogin_Click(object sender, EventArgs e)
        {
            string username = txtUser.Text.Trim();
            string password = textBox1.Text.Trim(); // Password field

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Vui lòng nhập đầy đủ tài khoản và mật khẩu!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                // 1. Connect to Database
                using (MySqlConnection conn = DatabaseHelper.GetConnection())
                {
                    if (conn.State == System.Data.ConnectionState.Closed) conn.Open();

                    // 2. SQL query to check credentials and get user details
                    // Note: In production, use hashed passwords (e.g., BCrypt)
                    string sql = "SELECT id, full_name, role FROM users WHERE username = @u AND password = @p";

                    using (MySqlCommand cmd = new MySqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@u", username);
                        cmd.Parameters.AddWithValue("@p", password);

                        // 3. Execute and read result
                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                // Login Success! Retrieve info
                                int userId = Convert.ToInt32(reader["id"]);
                                string fullName = reader["full_name"].ToString();
                                string role = reader["role"].ToString().ToLower(); // Normalize role to lowercase

                                MessageBox.Show($"Xin chào {fullName}!", "Đăng nhập thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);

                                this.Hide(); // Hide login form

                                // 4. Clear Role-Based Redirection
                                switch (role)
                                {
                                    case "admin":
                                        // Admin gets full access via FrmMain
                                        // Pass the role to FrmMain if it handles dynamic UI based on role
                                        FrmMain frmAdmin = new FrmMain("admin");
                                        frmAdmin.ShowDialog();
                                        break;

                                    case "staff":
                                        // Staff might use FrmMain but with restricted features
                                        FrmMain frmStaff = new FrmMain("staff");
                                        frmStaff.ShowDialog();
                                        break;

                                    case "resident":
                                        // Residents go to a separate Portal
                                        FrmPortal frmUser = new FrmPortal(fullName);
                                        frmUser.ShowDialog();
                                        break;

                                    default:
                                        // Handle unknown roles or generic users
                                        MessageBox.Show("Vai trò không xác định. Vui lòng liên hệ quản trị viên.", "Lỗi quyền hạn", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                        this.Show(); // Re-show login if role is invalid
                                        return;
                                }

                                // Close login form after the main form is closed (application exit logic usually handles this)
                                this.Close();
                            }
                            else
                            {
                                MessageBox.Show("Sai tài khoản hoặc mật khẩu!", "Lỗi đăng nhập", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi kết nối Server: " + ex.Message, "Lỗi hệ thống", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // --- EXIT BUTTON EVENT ---
        private void btnExit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        // --- REGISTER LINK EVENT ---
        private void lnkRegister_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            FrmRegister frm = new FrmRegister();
            this.Hide();
            frm.ShowDialog();
            this.Show(); // Re-show login after registration closes
        }

        // --- DRAG FORM EVENT ---
        private void panelLeft_MouseDown(object sender, MouseEventArgs e)
        {
            ReleaseCapture();
            SendMessage(this.Handle, 0x112, 0xf012, 0);
        }

        // --- UNUSED EVENT HANDLERS (Kept to prevent Designer errors) ---
        private void panel2_Paint(object sender, PaintEventArgs e) { }
        private void contextMenuStrip1_Opening(object sender, System.ComponentModel.CancelEventArgs e) { }
        private void panel1_Paint(object sender, PaintEventArgs e) { }
        private void label1_Click(object sender, EventArgs e) { }
        private void label2_Click(object sender, EventArgs e) { }
        private void label4_Click(object sender, EventArgs e) { }
        private void panel3_Paint(object sender, PaintEventArgs e) { }
        private void panel4_Paint(object sender, PaintEventArgs e) { }
        private void textBox1_TextChanged(object sender, EventArgs e) { }
        private void txtPass_Click(object sender, EventArgs e) { }
        private void pnUserWrapper_Paint(object sender, PaintEventArgs e) { }
        private void lineUser_Paint(object sender, PaintEventArgs e) { }
        private void txtUser_TextChanged(object sender, EventArgs e) { }
        private void label3_Click(object sender, EventArgs e) { }
        private void pictureBox1_Click(object sender, EventArgs e) { }
    }
}