using System.Drawing;
using System.Windows.Forms;

namespace AGDRONE
{
    public static class Theme
    {
        // Catppuccin Mocha inspired palette
        public static readonly Color Background = ColorTranslator.FromHtml("#1E1E2E");
        public static readonly Color Surface = ColorTranslator.FromHtml("#313244");
        public static readonly Color SurfaceHighlight = ColorTranslator.FromHtml("#45475A");
        public static readonly Color Text = ColorTranslator.FromHtml("#CDD6F4");
        public static readonly Color Subtext = ColorTranslator.FromHtml("#A6ADC8");
        public static readonly Color Accent = ColorTranslator.FromHtml("#89B4FA"); // Blue
        public static readonly Color AccentHover = ColorTranslator.FromHtml("#74C7EC");
        public static readonly Color Danger = ColorTranslator.FromHtml("#F38BA8");
        public static readonly Color Success = ColorTranslator.FromHtml("#A6E3A1");
        public static readonly Color Warning = ColorTranslator.FromHtml("#F9E2AF");

        // Typography
        public static readonly Font HeaderFont = new Font("Segoe UI Semibold", 16F, FontStyle.Regular, GraphicsUnit.Point);
        public static readonly Font NormalFont = new Font("Segoe UI", 10F, FontStyle.Regular, GraphicsUnit.Point);
        public static readonly Font SmallFont = new Font("Segoe UI", 8F, FontStyle.Regular, GraphicsUnit.Point);

        // Styling helpers
        public static void StyleButton(Button btn, Color backColor, Color foreColor)
        {
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 0;
            btn.BackColor = backColor;
            btn.ForeColor = foreColor;
            btn.Font = new Font("Segoe UI Semibold", 10F);
            btn.Cursor = Cursors.Hand;
        }

        public static void StyleDataGridView(DataGridView dgv)
        {
            dgv.BackgroundColor = Surface;
            dgv.BorderStyle = BorderStyle.None;
            dgv.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            dgv.DefaultCellStyle.SelectionBackColor = Accent;
            dgv.DefaultCellStyle.SelectionForeColor = Background;
            dgv.DefaultCellStyle.BackColor = Surface;
            dgv.DefaultCellStyle.ForeColor = Text;
            dgv.DefaultCellStyle.Font = NormalFont;
            
            dgv.EnableHeadersVisualStyles = false;
            dgv.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
            dgv.ColumnHeadersDefaultCellStyle.BackColor = Background;
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = Subtext;
            dgv.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI Semibold", 10F);
            dgv.ColumnHeadersHeight = 40;

            dgv.RowHeadersVisible = false;
            dgv.AllowUserToAddRows = false;
            dgv.AllowUserToDeleteRows = false;
            dgv.AllowUserToResizeRows = false;
            dgv.ReadOnly = true;
            dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgv.RowTemplate.Height = 40;
        }
    }
}
