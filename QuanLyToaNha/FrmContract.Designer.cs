namespace QuanLyToaNha
{
    partial class FrmContract
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
            this.dgvContract = new System.Windows.Forms.DataGridView();
            ((System.ComponentModel.ISupportInitialize)(this.dgvContract)).BeginInit();
            this.SuspendLayout();
            // 
            // dgvContract
            // 
            this.dgvContract.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvContract.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvContract.Location = new System.Drawing.Point(0, 0);
            this.dgvContract.Name = "dgvContract";
            this.dgvContract.RowHeadersWidth = 51;
            this.dgvContract.RowTemplate.Height = 24;
            this.dgvContract.Size = new System.Drawing.Size(800, 450);
            this.dgvContract.TabIndex = 0;
            // 
            // FrmContract
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.dgvContract);
            this.Name = "FrmContract";
            this.Text = "FrmContract";
            ((System.ComponentModel.ISupportInitialize)(this.dgvContract)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.DataGridView dgvContract;
    }
}