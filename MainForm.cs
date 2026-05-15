using System;
using System.Drawing;
using System.Windows.Forms;

namespace AGDRONE
{
    public class MainForm : Form
    {
        private Panel pnlSidebar;
        private Label lblLogo;
        private Button btnDashboard;
        private Button btnFleet;
        
        private Form currentChildForm;

        public MainForm()
        {
            InitializeComponent();
            ApplyTheme();
            
            // Load initial view
            OpenChildForm(new DashboardForm());
        }

        private void InitializeComponent()
        {
            this.pnlSidebar = new Panel();
            this.lblLogo = new Label();
            this.btnDashboard = new Button();
            this.btnFleet = new Button();

            this.SuspendLayout();

            // MDI Configuration
            this.IsMdiContainer = true;

            // Sidebar
            this.pnlSidebar.Dock = DockStyle.Left;
            this.pnlSidebar.Width = 250;
            this.pnlSidebar.Controls.Add(this.btnFleet);
            this.pnlSidebar.Controls.Add(this.btnDashboard);
            this.pnlSidebar.Controls.Add(this.lblLogo);

            // Logo
            this.lblLogo.Dock = DockStyle.Top;
            this.lblLogo.Height = 100;
            this.lblLogo.Text = "SkyFleet Commander";
            this.lblLogo.TextAlign = ContentAlignment.MiddleCenter;

            // Dashboard Button
            this.btnDashboard.Dock = DockStyle.Top;
            this.btnDashboard.Height = 50;
            this.btnDashboard.Text = "Dashboard";
            this.btnDashboard.Click += (s, e) => OpenChildForm(new DashboardForm());

            // Fleet Button
            this.btnFleet.Dock = DockStyle.Top;
            this.btnFleet.Height = 50;
            this.btnFleet.Text = "Fleet Management";
            this.btnFleet.Click += (s, e) => OpenChildForm(new FleetForm());

            // MainForm
            this.ClientSize = new Size(1280, 800);
            this.Controls.Add(this.pnlSidebar);
            this.Name = "MainForm";
            this.Text = "SkyFleet Commander";
            this.StartPosition = FormStartPosition.CenterScreen;

            this.ResumeLayout(false);
        }

        private void ApplyTheme()
        {
            this.BackColor = Theme.Background;
            this.pnlSidebar.BackColor = Theme.Surface;
            
            this.lblLogo.ForeColor = Theme.Accent;
            this.lblLogo.Font = new Font("Segoe UI", 20F, FontStyle.Bold);

            Theme.StyleButton(this.btnDashboard, Theme.Surface, Theme.Text);
            Theme.StyleButton(this.btnFleet, Theme.Surface, Theme.Text);

            // Hover effects
            btnFleet.MouseEnter += (s, e) => btnFleet.BackColor = Theme.SurfaceHighlight;
            btnFleet.MouseLeave += (s, e) => btnFleet.BackColor = Theme.Surface;
            btnDashboard.MouseEnter += (s, e) => btnDashboard.BackColor = Theme.SurfaceHighlight;
            btnDashboard.MouseLeave += (s, e) => btnDashboard.BackColor = Theme.Surface;

            // Optional: Color the MDI client background
            foreach (Control c in this.Controls)
            {
                if (c is MdiClient)
                {
                    c.BackColor = Theme.Background;
                }
            }
        }

        private void OpenChildForm(Form childForm)
        {
            if (currentChildForm != null)
            {
                currentChildForm.Close();
            }

            currentChildForm = childForm;
            childForm.MdiParent = this;
            childForm.FormBorderStyle = FormBorderStyle.None;
            childForm.Dock = DockStyle.Fill;
            childForm.Show();
        }
    }
}
