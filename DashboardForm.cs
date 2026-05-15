using System;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Linq;

namespace AGDRONE
{
    public class DashboardForm : Form
    {
        private TableLayoutPanel tlpKPI;
        private Panel pnlHealth, pnlLog, pnlActions;
        private Label lblTotalFleet, lblActiveMissions, lblCriticalAlerts, lblSystemStatus;
        private ProgressBar pbAvgBattery;
        private ListBox lbUrgentDrones;
        private RichTextBox rtbRecentActivity;
        private Button btnEmergencyStop, btnRefresh;

        public DashboardForm()
        {
            InitializeComponent();
            ApplyTheme();
            RefreshDashboard();
        }

        private void InitializeComponent()
        {
            this.tlpKPI = new TableLayoutPanel { Dock = DockStyle.Top, Height = 150, ColumnCount = 4, RowCount = 1 };
            this.pnlHealth = new Panel { Dock = DockStyle.Left, Width = 400, Padding = new Padding(10) };
            this.pnlLog = new Panel { Dock = DockStyle.Fill, Padding = new Padding(10) };
            this.pnlActions = new Panel { Dock = DockStyle.Bottom, Height = 100, Padding = new Padding(10) };

            // KPI Cards
            lblTotalFleet = CreateKPICard("TOTAL FLEET SIZE", "0", 0);
            lblActiveMissions = CreateKPICard("ACTIVE MISSIONS", "0", 1);
            lblCriticalAlerts = CreateKPICard("CRITICAL ALERTS", "0", 2);
            lblSystemStatus = CreateKPICard("SYSTEM STATUS", "ONLINE", 3);

            // Health Section
            Label lblHealthTitle = new Label { Text = "Battery Health Monitor", Font = Theme.HeaderFont, ForeColor = Theme.Text, Dock = DockStyle.Top, Height = 40 };
            pbAvgBattery = new ProgressBar { Dock = DockStyle.Top, Height = 30, Style = ProgressBarStyle.Continuous };
            Label lblUrgentTitle = new Label { Text = "Urgent Charging Required:", Font = Theme.NormalFont, ForeColor = Theme.Subtext, Dock = DockStyle.Top, Height = 30, Margin = new Padding(0, 20, 0, 0) };
            lbUrgentDrones = new ListBox { Dock = DockStyle.Fill, BackColor = Theme.Surface, ForeColor = Theme.Danger, BorderStyle = BorderStyle.None, Font = Theme.NormalFont };
            
            pnlHealth.Controls.Add(lbUrgentDrones);
            pnlHealth.Controls.Add(lblUrgentTitle);
            pnlHealth.Controls.Add(pbAvgBattery);
            pnlHealth.Controls.Add(lblHealthTitle);

            // Activity Log
            Label lblLogTitle = new Label { Text = "Recent Activity Feed", Font = Theme.HeaderFont, ForeColor = Theme.Text, Dock = DockStyle.Top, Height = 40 };
            rtbRecentActivity = new RichTextBox { Dock = DockStyle.Fill, ReadOnly = true, BackColor = Color.Black, ForeColor = Color.Lime, Font = new Font("Consolas", 10F), BorderStyle = BorderStyle.None };
            pnlLog.Controls.Add(rtbRecentActivity);
            pnlLog.Controls.Add(lblLogTitle);

            // Action Buttons
            btnEmergencyStop = new Button { Text = "EMERGENCY STOP (ALL)", Width = 250, Height = 50, Location = new Point(20, 25) };
            btnEmergencyStop.Click += BtnEmergencyStop_Click;
            btnRefresh = new Button { Text = "REFRESH DASHBOARD", Width = 200, Height = 50, Location = new Point(290, 25) };
            btnRefresh.Click += (s, e) => RefreshDashboard();
            
            pnlActions.Controls.Add(btnEmergencyStop);
            pnlActions.Controls.Add(btnRefresh);

            this.Controls.Add(pnlLog);
            this.Controls.Add(pnlHealth);
            this.Controls.Add(pnlActions);
            this.Controls.Add(tlpKPI);

            // Form settings for MDI child
            this.FormBorderStyle = FormBorderStyle.None;
            this.ControlBox = false;
        }

        private Label CreateKPICard(string title, string initialValue, int column)
        {
            Panel pnl = new Panel { Dock = DockStyle.Fill, Margin = new Padding(10), BackColor = Theme.Surface };
            Label lblTitle = new Label { Text = title, Dock = DockStyle.Top, Height = 30, TextAlign = ContentAlignment.MiddleCenter, ForeColor = Theme.Subtext, Font = Theme.SmallFont };
            Label lblVal = new Label { Text = initialValue, Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleCenter, ForeColor = Theme.Accent, Font = new Font("Segoe UI", 24F, FontStyle.Bold) };
            
            pnl.Controls.Add(lblVal);
            pnl.Controls.Add(lblTitle);
            tlpKPI.Controls.Add(pnl, column, 0);
            return lblVal;
        }

        private void ApplyTheme()
        {
            this.BackColor = Theme.Background;
            Theme.StyleButton(btnEmergencyStop, Theme.Danger, Theme.Background);
            Theme.StyleButton(btnRefresh, Theme.SurfaceHighlight, Theme.Text);
        }

        private void RefreshDashboard()
        {
            Drone.TotalDronesDeployed = 0;
            var fleet = DatabaseManager.LoadFleet();

            lblTotalFleet.Text = Drone.TotalDronesDeployed.ToString();
            lblActiveMissions.Text = fleet.Count(d => d.GPS.Coordinates != "Base Station").ToString();
            
            var criticalDrones = fleet.Where(d => d.BatteryLevel < 20).ToList();
            lblCriticalAlerts.Text = criticalDrones.Count.ToString();
            lblCriticalAlerts.ForeColor = criticalDrones.Count > 0 ? Theme.Danger : Theme.Accent;

            if (fleet.Count > 0)
            {
                int avgBattery = (int)fleet.Average(d => d.BatteryLevel);
                pbAvgBattery.Value = avgBattery;
                
                lbUrgentDrones.Items.Clear();
                foreach (var d in criticalDrones)
                {
                    lbUrgentDrones.Items.Add($"{d.ID} ({d.BatteryLevel}%) - URGENT");
                }
            }

            rtbRecentActivity.Clear();
            var recentLog = Drone.GlobalLog.AsEnumerable().Reverse().Take(10).ToList();
            foreach (var line in recentLog)
            {
                rtbRecentActivity.AppendText(line + "\n");
            }
        }



        private void BtnEmergencyStop_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("INITIATE GLOBAL EMERGENCY RECALL FOR ALL DRONES?", "SYSTEM BYPASS", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                var fleet = DatabaseManager.LoadFleet();
                foreach (var d in fleet)
                {
                    d.EmergencyRecall();
                    DatabaseManager.UpdateDroneState(d);
                }
                Drone.GlobalLog.Add($"[{DateTime.Now:HH:mm:ss}] 🛑 GLOBAL EMERGENCY STOP EXECUTED BY COMMANDER.");
                RefreshDashboard();
            }
        }
    }
}
