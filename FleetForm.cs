using System;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Linq;

namespace AGDRONE
{
    public class FleetForm : Form
    {
        private DataGridView dgvFleet;
        private Panel pnlAddDrone;
        private TextBox txtID, txtCoords, txtSpecial, txtSearch;
        private ComboBox cmbType, cmbLocation;
        private Button btnAdd, btnDelete, btnExecuteMission, btnEmergencyRecall, btnCompare, btnSwarm;
        private RichTextBox rtbMissionLog;
        private Label lblStatusFeedback;
        private List<Drone> _allDrones;
        private DroneSwarm _currentSwarm;

        public FleetForm()
        {
            InitializeComponent();
            ApplyTheme();
            LoadData();
            _currentSwarm = new DroneSwarm("Alpha Team");
        }

        private void InitializeComponent()
        {
            this.dgvFleet = new DataGridView();
            this.pnlAddDrone = new Panel();
            this.txtID = new TextBox();
            this.cmbType = new ComboBox();
            this.cmbLocation = new ComboBox();
            this.txtCoords = new TextBox();
            this.txtSpecial = new TextBox();
            this.btnAdd = new Button();
            this.btnDelete = new Button();
            this.btnExecuteMission = new Button();
            this.btnEmergencyRecall = new Button();
            this.btnCompare = new Button();
            this.txtSearch = new TextBox();
            this.rtbMissionLog = new RichTextBox();
            this.lblStatusFeedback = new Label();

            Label lblTitle = new Label { Text = "Mission Control & Fleet Operations", Font = Theme.HeaderFont, ForeColor = Theme.Text, Dock = DockStyle.Top, Height = 40 };
            
            Panel pnlTopBar = new Panel { Dock = DockStyle.Top, Height = 50 };
            this.txtSearch.PlaceholderText = "Search ID or Type...";
            this.txtSearch.Location = new Point(10, 10);
            this.txtSearch.Width = 150;
            this.txtSearch.TextChanged += TxtSearch_TextChanged;

            this.btnDelete.Text = "Delete"; this.btnDelete.Location = new Point(170, 8); this.btnDelete.Width = 80; this.btnDelete.Height = 28;
            this.btnDelete.Click += BtnDelete_Click;

            this.btnCompare.Text = "Compare (2)"; this.btnCompare.Location = new Point(260, 8); this.btnCompare.Width = 100; this.btnCompare.Height = 28;
            this.btnCompare.Click += BtnCompare_Click;

            this.btnExecuteMission.Text = "Execute Mission"; this.btnExecuteMission.Location = new Point(370, 8); this.btnExecuteMission.Width = 120; this.btnExecuteMission.Height = 28;
            this.btnExecuteMission.Click += BtnExecuteMission_Click;

            this.btnEmergencyRecall.Text = "Emergency Recall"; this.btnEmergencyRecall.Location = new Point(500, 8); this.btnEmergencyRecall.Width = 130; this.btnEmergencyRecall.Height = 28;
            this.btnEmergencyRecall.Click += BtnEmergencyRecall_Click;

            this.btnSwarm = new Button();
            this.btnSwarm.Text = "Deploy Swarm"; this.btnSwarm.Location = new Point(640, 8); this.btnSwarm.Width = 120; this.btnSwarm.Height = 28;
            this.btnSwarm.Click += BtnSwarm_Click;

            pnlTopBar.Controls.AddRange(new Control[] { txtSearch, btnDelete, btnCompare, btnExecuteMission, btnEmergencyRecall, btnSwarm });

            this.pnlAddDrone.Dock = DockStyle.Bottom;
            this.pnlAddDrone.Height = 180;
            this.pnlAddDrone.Padding = new Padding(20, 10, 20, 10);

            Label lblAddTitle = new Label { Text = "REGISTER NEW DRONE UNIT", Font = new Font("Segoe UI", 10F, FontStyle.Bold), ForeColor = Theme.Accent, AutoSize = true, Location = new Point(20, 10) };
            
            TableLayoutPanel tlpFields = new TableLayoutPanel { 
                Location = new Point(20, 40), 
                Size = new Size(820, 90), 
                ColumnCount = 5, 
                RowCount = 3,
                BackColor = Color.Transparent 
            };
            tlpFields.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20));
            tlpFields.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20));
            tlpFields.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20));
            tlpFields.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20));
            tlpFields.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20));

            tlpFields.Controls.Add(new Label { Text = "Identification ID", ForeColor = Theme.Text, AutoSize = true, Font = new Font("Segoe UI Semibold", 9F) }, 0, 0);
            tlpFields.Controls.Add(new Label { Text = "Operational Type", ForeColor = Theme.Text, AutoSize = true, Font = new Font("Segoe UI Semibold", 9F) }, 1, 0);
            tlpFields.Controls.Add(new Label { Text = "Deployment Zone", ForeColor = Theme.Text, AutoSize = true, Font = new Font("Segoe UI Semibold", 9F) }, 2, 0);
            tlpFields.Controls.Add(new Label { Text = "Coordinates (X,Y)", ForeColor = Theme.Text, AutoSize = true, Font = new Font("Segoe UI Semibold", 9F) }, 3, 0);
            tlpFields.Controls.Add(new Label { Text = "Unit Special Spec", ForeColor = Theme.Text, AutoSize = true, Font = new Font("Segoe UI Semibold", 9F) }, 4, 0);

            this.txtID.Dock = DockStyle.Fill; this.cmbType.Dock = DockStyle.Fill;
            this.cmbLocation.Dock = DockStyle.Fill;
            this.txtCoords.Dock = DockStyle.Fill; this.txtSpecial.Dock = DockStyle.Fill;
            this.cmbType.Items.AddRange(new string[] { "Delivery", "Surveillance", "Agricultural", "Medical", "Racing" });
            this.cmbType.DropDownStyle = ComboBoxStyle.DropDownList;

            this.cmbLocation.Items.AddRange(new string[] { "Main Warehouse", "Hospital Zone A", "Agricultural Field 1", "Emergency Base", "Custom (Manual Entry)" });
            this.cmbLocation.DropDownStyle = ComboBoxStyle.DropDownList;
            this.cmbLocation.SelectedIndexChanged += CmbLocation_SelectedIndexChanged;

            tlpFields.Controls.Add(txtID, 0, 1);
            tlpFields.Controls.Add(cmbType, 1, 1);
            tlpFields.Controls.Add(cmbLocation, 2, 1);
            tlpFields.Controls.Add(txtCoords, 3, 1);
            tlpFields.Controls.Add(txtSpecial, 4, 1);

            tlpFields.Controls.Add(new Label { Text = "e.g., SKY-001", ForeColor = Theme.Subtext, Font = Theme.SmallFont, AutoSize = true }, 0, 2);
            tlpFields.Controls.Add(new Label { Text = "Select mission profile", ForeColor = Theme.Subtext, Font = Theme.SmallFont, AutoSize = true }, 1, 2);
            tlpFields.Controls.Add(new Label { Text = "Select deployment zone", ForeColor = Theme.Subtext, Font = Theme.SmallFont, AutoSize = true }, 2, 2);
            tlpFields.Controls.Add(new Label { Text = "Format: X,Y (e.g., 45,90)", ForeColor = Theme.Subtext, Font = Theme.SmallFont, AutoSize = true }, 3, 2);
            tlpFields.Controls.Add(new Label { Text = "Payload, Resolution, etc.", ForeColor = Theme.Subtext, Font = Theme.SmallFont, AutoSize = true }, 4, 2);

            this.btnAdd.Text = "DEPLOY DRONE"; this.btnAdd.Location = new Point(20, 135); this.btnAdd.Width = 150; this.btnAdd.Height = 35;
            this.btnAdd.Click += BtnAdd_Click;

            this.lblStatusFeedback.Text = ""; this.lblStatusFeedback.AutoSize = true; this.lblStatusFeedback.Location = new Point(180, 145); this.lblStatusFeedback.Font = new Font("Segoe UI", 9F, FontStyle.Italic);
            
            this.pnlAddDrone.Controls.AddRange(new Control[] { lblAddTitle, tlpFields, btnAdd, lblStatusFeedback });

            this.rtbMissionLog.Dock = DockStyle.Bottom;
            this.rtbMissionLog.Height = 100;
            this.rtbMissionLog.ReadOnly = true;
            this.rtbMissionLog.Text = "--- MISSION CONTROL LOG ---\n";

            this.dgvFleet.Dock = DockStyle.Fill;
            Theme.StyleDataGridView(this.dgvFleet);
            this.dgvFleet.CellFormatting += DgvFleet_CellFormatting;

            this.Controls.Add(this.dgvFleet);
            this.Controls.Add(this.rtbMissionLog);
            this.Controls.Add(this.pnlAddDrone);
            this.Controls.Add(pnlTopBar);
            this.Controls.Add(lblTitle);

            // Form settings for MDI child
            this.FormBorderStyle = FormBorderStyle.None;
            this.ControlBox = false;
        }

        private void ApplyTheme()
        {
            this.BackColor = Theme.Background;
            this.pnlAddDrone.BackColor = Theme.Surface;

            Theme.StyleButton(this.btnAdd, Theme.Accent, Theme.Background);
            Theme.StyleButton(this.btnDelete, Theme.SurfaceHighlight, Theme.Text);
            Theme.StyleButton(this.btnCompare, Theme.SurfaceHighlight, Theme.Text);
            Theme.StyleButton(this.btnExecuteMission, Theme.Success, Theme.Background);
            Theme.StyleButton(this.btnEmergencyRecall, Theme.Danger, Theme.Background);
            Theme.StyleButton(this.btnSwarm, Theme.Accent, Theme.Background);

            this.rtbMissionLog.BackColor = Color.Black;
            this.rtbMissionLog.ForeColor = Color.Lime;
            this.rtbMissionLog.Font = new Font("Consolas", 10F);
            this.rtbMissionLog.BorderStyle = BorderStyle.None;

            foreach (Control c in new Control[] { txtID, txtCoords, txtSpecial, txtSearch, cmbType, cmbLocation })
            {
                c.BackColor = Theme.SurfaceHighlight;
                c.ForeColor = Theme.Text;
                c.Font = Theme.NormalFont;
                if (c is ComboBox cmb) cmb.FlatStyle = FlatStyle.Flat;
                if (c is TextBox txt) txt.BorderStyle = BorderStyle.FixedSingle;
            }
        }

        private void LoadData()
        {
            try
            {
                _allDrones = DatabaseManager.LoadFleet();
                FilterGrid();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading data: " + ex.Message, "Error");
            }
        }

        private void FilterGrid()
        {
            if (_allDrones == null) return;
            string search = txtSearch.Text.Trim();

            // Use database-level search with INNER JOIN when a search term is entered
            if (!string.IsNullOrEmpty(search))
            {
                var results = DatabaseManager.SearchFleet(search);
                dgvFleet.DataSource = null;
                dgvFleet.DataSource = results;
            }
            else
            {
                dgvFleet.DataSource = null;
                dgvFleet.DataSource = _allDrones;
            }
        }

        private void TxtSearch_TextChanged(object sender, EventArgs e) => FilterGrid();

        private void DgvFleet_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (dgvFleet.Columns[e.ColumnIndex].Name == "BatteryLevel" && e.Value != null)
            {
                if (int.TryParse(e.Value.ToString(), out int battery))
                {
                    e.CellStyle.Font = new Font(dgvFleet.Font, FontStyle.Bold);
                    if (battery < 20) e.CellStyle.ForeColor = Color.Red;
                    else if (battery < 50) e.CellStyle.ForeColor = Color.Yellow;
                    else e.CellStyle.ForeColor = Color.LightGreen;
                }
            }
        }

        private void BtnExecuteMission_Click(object sender, EventArgs e)
        {
            if (dgvFleet.SelectedRows.Count == 0) return;
            var drone = (Drone)dgvFleet.SelectedRows[0].DataBoundItem;
            string result = drone.PerformMission(); 
            string icon = drone.Type switch { "Delivery" => "📦", "Surveillance" => "📷", "Agricultural" => "🌾", "Medical" => "⚕️", _ => "🚀" };
            string entry = $"[{DateTime.Now:HH:mm:ss}] {icon} {drone.Type}Drone {drone.ID}: {result}";
            Drone.GlobalLog.Add(entry);
            Log(entry);
        }

        private void BtnEmergencyRecall_Click(object sender, EventArgs e)
        {
            if (dgvFleet.SelectedRows.Count == 0) return;
            var drone = (Drone)dgvFleet.SelectedRows[0].DataBoundItem;
            drone.EmergencyRecall(); 
            DatabaseManager.UpdateDroneState(drone);
            LoadData();
            string entry = $"[{DateTime.Now:HH:mm:ss}] ⚠️ EMERGENCY RECALL INITIATED for {drone.ID}.";
            Drone.GlobalLog.Add(entry);
            Log(entry);
        }

        private void BtnCompare_Click(object sender, EventArgs e)
        {
            if (dgvFleet.SelectedRows.Count != 2)
            {
                MessageBox.Show("Select exactly TWO drones to compare.", "Compare Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            var d1 = (Drone)dgvFleet.SelectedRows[0].DataBoundItem;
            var d2 = (Drone)dgvFleet.SelectedRows[1].DataBoundItem;
            string message = (d1 > d2) ? $"{d1.ID} has more battery." : (d1 < d2) ? $"{d2.ID} has more battery." : "Equal battery levels.";
            MessageBox.Show(message, "Operator Overloading Demo");
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            if (dgvFleet.SelectedRows.Count > 0)
            {
                var drone = (Drone)dgvFleet.SelectedRows[0].DataBoundItem;
                if (MessageBox.Show($"Delete {drone.ID}?", "Confirm", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    DatabaseManager.DeleteDrone(drone.ID);
                    LoadData();
                }
            }
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtID.Text) || cmbType.SelectedItem == null)
            {
                ShowFeedback("ID and Type are required!", Theme.Danger); return;
            }

            try
            {
                string id = txtID.Text, type = cmbType.SelectedItem.ToString(), special = txtSpecial.Text;
                string coords = string.IsNullOrWhiteSpace(txtCoords.Text) ? "0,0" : txtCoords.Text;
                
                Drone newDrone = type switch
                {
                    "Delivery" => new DeliveryDrone(id, 100, coords, string.IsNullOrEmpty(special) ? 0 : Convert.ToDouble(special)),
                    "Surveillance" => new SurveillanceDrone(id, 100, coords, special),
                    "Agricultural" => new AgriculturalDrone(id, 100, coords, special),
                    "Medical" => new MedicalDrone(id, 100, coords, special),
                    "Racing" => new RacingDrone(id, 100, coords, string.IsNullOrEmpty(special) ? 0 : Convert.ToInt32(special)),
                    _ => null
                };

                if (newDrone != null)
                {
                    DatabaseManager.RegisterDrone(newDrone);
                    LoadData();
                    ShowFeedback($"Successfully deployed {id}!", Theme.Success);
                    string entry = $"[{DateTime.Now:HH:mm:ss}] ➕ Registered new {type} drone: {id}";
                    Drone.GlobalLog.Add(entry);
                    Log(entry);
                    txtID.Clear(); txtCoords.Clear(); txtSpecial.Clear(); cmbType.SelectedIndex = -1;
                }
            }
            catch (Exception ex)
            {
                ShowFeedback("Error: " + ex.Message, Theme.Danger);
            }
        }

        private void ShowFeedback(string msg, Color color)
        {
            lblStatusFeedback.Text = msg;
            lblStatusFeedback.ForeColor = color;
            System.Windows.Forms.Timer t = new System.Windows.Forms.Timer { Interval = 3000 };
            t.Tick += (s, e) => { lblStatusFeedback.Text = ""; t.Stop(); };
            t.Start();
        }

        private void Log(string msg)
        {
            rtbMissionLog.AppendText(msg + "\n");
            rtbMissionLog.ScrollToCaret();
        }

        private void BtnSwarm_Click(object? sender, EventArgs e)
        {
            if (dgvFleet.SelectedRows.Count < 2)
            {
                MessageBox.Show("Please select at least 2 drones to form a swarm.", "Swarm Protocol", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            _currentSwarm = new DroneSwarm($"Strike Team {DateTime.Now.Second}");
            Drone.GlobalLog.Clear(); // Clear logic for clean output

            foreach (DataGridViewRow row in dgvFleet.SelectedRows)
            {
                if (row.DataBoundItem is Drone d)
                {
                    _currentSwarm.AddDrone(d);
                }
            }

            // Command the swarm
            _currentSwarm.FormUp(FormationPattern.V_Shape);
            _currentSwarm.CheckCollisionAvoidance();
            _currentSwarm.ExecuteSwarmMission("High Priority Recon and Asset Deployment");

            // Output the global log generated by the swarm to the RichTextBox
            Log($"\n--- SWARM OPERATIONS: {_currentSwarm.SwarmName} ---");
            foreach(var entry in Drone.GlobalLog)
            {
                Log(entry);
            }
            Log("-----------------------------------\n");

            // Refresh UI to show updated coordinates
            dgvFleet.Refresh();
        }

        private void CmbLocation_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (cmbLocation.SelectedItem == null) return;
            
            var locations = new Dictionary<string, string>
            {
                { "Main Warehouse", "24.8607,67.0011" },      // Karachi
                { "Hospital Zone A", "24.8700,67.0100" },
                { "Agricultural Field 1", "24.9000,67.0500" },
                { "Emergency Base", "24.8500,66.9900" },
                { "Custom (Manual Entry)", "MANUAL" }
            };

            string selected = cmbLocation.SelectedItem.ToString()!;
            if (locations.ContainsKey(selected) && locations[selected] != "MANUAL")
            {
                txtCoords.Text = locations[selected];
                txtCoords.ReadOnly = true;
            }
            else
            {
                txtCoords.ReadOnly = false;
                txtCoords.Clear();
            }
        }
    }
}
