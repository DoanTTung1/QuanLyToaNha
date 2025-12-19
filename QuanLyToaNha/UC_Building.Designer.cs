namespace QuanLyToaNha
{
    partial class UC_Building
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.dgvBuilding = new System.Windows.Forms.DataGridView();
            ((System.ComponentModel.ISupportInitialize)(this.dgvBuilding)).BeginInit();
            this.SuspendLayout();
            // 
            // dgvBuilding
            // 
            this.dgvBuilding.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvBuilding.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvBuilding.Location = new System.Drawing.Point(0, 0);
            this.dgvBuilding.Name = "dgvBuilding";
            this.dgvBuilding.RowHeadersWidth = 51;
            this.dgvBuilding.RowTemplate.Height = 24;
            this.dgvBuilding.Size = new System.Drawing.Size(150, 150);
            this.dgvBuilding.TabIndex = 0;
            // 
            // UC_Building
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.dgvBuilding);
            this.Name = "UC_Building";
            ((System.ComponentModel.ISupportInitialize)(this.dgvBuilding)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.DataGridView dgvBuilding;
    }
}
