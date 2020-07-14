namespace AccountRPD.Views
{
    partial class MainView
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainView));
            this.tsMainPanel = new System.Windows.Forms.ToolStrip();
            this.tsbOpenManager = new System.Windows.Forms.ToolStripButton();
            this.tsMainPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // tsMainPanel
            // 
            this.tsMainPanel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.tsMainPanel.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.tsMainPanel.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsbOpenManager});
            this.tsMainPanel.Location = new System.Drawing.Point(0, 0);
            this.tsMainPanel.Name = "tsMainPanel";
            this.tsMainPanel.Size = new System.Drawing.Size(1104, 56);
            this.tsMainPanel.TabIndex = 2;
            // 
            // tsbOpenManager
            // 
            this.tsbOpenManager.Font = new System.Drawing.Font("Segoe UI", 10.25F);
            this.tsbOpenManager.Image = ((System.Drawing.Image)(resources.GetObject("tsbOpenManager.Image")));
            this.tsbOpenManager.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.tsbOpenManager.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbOpenManager.Name = "tsbOpenManager";
            this.tsbOpenManager.Size = new System.Drawing.Size(113, 53);
            this.tsbOpenManager.Text = "Менеджер РПД";
            this.tsbOpenManager.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.tsbOpenManager.Click += new System.EventHandler(this.OpenManagerExecute);
            // 
            // MainView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 17F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1104, 528);
            this.Controls.Add(this.tsMainPanel);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.IsMdiContainer = true;
            this.KeyPreview = true;
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "MainView";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Электронные РПД (СФ БашГУ)";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.MainView_KeyDown);
            this.tsMainPanel.ResumeLayout(false);
            this.tsMainPanel.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ToolStrip tsMainPanel;
        private System.Windows.Forms.ToolStripButton tsbOpenManager;
    }
}