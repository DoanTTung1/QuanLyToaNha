namespace QuanLyToaNha
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.panelMenu = new System.Windows.Forms.Panel();
            this.btnCuDan = new System.Windows.Forms.Button();
            this.btnTrangChu = new System.Windows.Forms.Button();
            this.panelTop = new System.Windows.Forms.Panel();
            this.panelContent = new System.Windows.Forms.Panel();
            this.btnToaNha = new System.Windows.Forms.Button();
            this.panelMenu.SuspendLayout();
            this.SuspendLayout();
            // 
            // panelMenu
            // 
            this.panelMenu.BackColor = System.Drawing.SystemColors.ActiveBorder;
            this.panelMenu.Controls.Add(this.btnToaNha);
            this.panelMenu.Controls.Add(this.btnCuDan);
            this.panelMenu.Controls.Add(this.btnTrangChu);
            this.panelMenu.Dock = System.Windows.Forms.DockStyle.Left;
            this.panelMenu.Location = new System.Drawing.Point(0, 0);
            this.panelMenu.Name = "panelMenu";
            this.panelMenu.Size = new System.Drawing.Size(200, 450);
            this.panelMenu.TabIndex = 0;
            // 
            // btnCuDan
            // 
            this.btnCuDan.Dock = System.Windows.Forms.DockStyle.Top;
            this.btnCuDan.Location = new System.Drawing.Point(0, 50);
            this.btnCuDan.Name = "btnCuDan";
            this.btnCuDan.Size = new System.Drawing.Size(200, 61);
            this.btnCuDan.TabIndex = 1;
            this.btnCuDan.Text = "Quản Lý Cư Dân";
            this.btnCuDan.UseVisualStyleBackColor = true;
            this.btnCuDan.Click += new System.EventHandler(this.btnCuDan_Click);
            // 
            // btnTrangChu
            // 
            this.btnTrangChu.Dock = System.Windows.Forms.DockStyle.Top;
            this.btnTrangChu.Location = new System.Drawing.Point(0, 0);
            this.btnTrangChu.Name = "btnTrangChu";
            this.btnTrangChu.Size = new System.Drawing.Size(200, 50);
            this.btnTrangChu.TabIndex = 0;
            this.btnTrangChu.Text = "Trang Chủ\r\n";
            this.btnTrangChu.UseVisualStyleBackColor = true;
            // 
            // panelTop
            // 
            this.panelTop.BackColor = System.Drawing.SystemColors.ActiveCaption;
            this.panelTop.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelTop.Location = new System.Drawing.Point(200, 0);
            this.panelTop.Name = "panelTop";
            this.panelTop.Size = new System.Drawing.Size(600, 50);
            this.panelTop.TabIndex = 1;
            // 
            // panelContent
            // 
            this.panelContent.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelContent.Location = new System.Drawing.Point(200, 50);
            this.panelContent.Name = "panelContent";
            this.panelContent.Size = new System.Drawing.Size(600, 400);
            this.panelContent.TabIndex = 2;
            // 
            // btnToaNha
            // 
            this.btnToaNha.Dock = System.Windows.Forms.DockStyle.Top;
            this.btnToaNha.Location = new System.Drawing.Point(0, 111);
            this.btnToaNha.Name = "btnToaNha";
            this.btnToaNha.Size = new System.Drawing.Size(200, 53);
            this.btnToaNha.TabIndex = 2;
            this.btnToaNha.Text = "Quản Lý Tòa Nhà";
            this.btnToaNha.UseVisualStyleBackColor = true;
            this.btnToaNha.Click += new System.EventHandler(this.btnToaNha_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.panelContent);
            this.Controls.Add(this.panelTop);
            this.Controls.Add(this.panelMenu);
            this.Name = "Form1";
            this.Text = "Form1";
            this.panelMenu.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        public System.Windows.Forms.Panel panelMenu;
        private System.Windows.Forms.Panel panelTop;
        private System.Windows.Forms.Panel panelContent;
        private System.Windows.Forms.Button btnCuDan;
        private System.Windows.Forms.Button btnTrangChu;
        private System.Windows.Forms.Button btnToaNha;
    }
}

