namespace Libreria
{
    partial class FormDashboard
    {
        private System.ComponentModel.IContainer components = null;
        private DashboardDesignPreview dashboardDesignPreview;

        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null)
                components.Dispose();

            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.dashboardDesignPreview = new Libreria.DashboardDesignPreview();
            this.SuspendLayout();
            // 
            // dashboardDesignPreview
            // 
            this.dashboardDesignPreview.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(8)))), ((int)(((byte)(18)))), ((int)(((byte)(34)))));
            this.dashboardDesignPreview.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dashboardDesignPreview.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(226)))), ((int)(((byte)(234)))), ((int)(((byte)(247)))));
            this.dashboardDesignPreview.Location = new System.Drawing.Point(0, 0);
            this.dashboardDesignPreview.Margin = new System.Windows.Forms.Padding(4);
            this.dashboardDesignPreview.Name = "dashboardDesignPreview";
            this.dashboardDesignPreview.Size = new System.Drawing.Size(1753, 744);
            this.dashboardDesignPreview.TabIndex = 0;
            // 
            // FormDashboard
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1753, 744);
            this.Controls.Add(this.dashboardDesignPreview);
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "FormDashboard";
            this.Text = "Librería - Dashboard de Operaciones";
            this.ResumeLayout(false);

        }
    }
}
