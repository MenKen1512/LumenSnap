namespace LumenSnap;

internal static class TrayTheme
{
    internal static System.Windows.Forms.ContextMenuStrip Menu() => new()
    {
        ShowImageMargin = false, ShowCheckMargin = false,
        BackColor = System.Drawing.Color.FromArgb(20, 30, 43), ForeColor = System.Drawing.Color.FromArgb(229, 239, 250),
        Padding = new System.Windows.Forms.Padding(4), Renderer = new System.Windows.Forms.ToolStripProfessionalRenderer(new Colors()) { RoundedEdges = true }
    };
    private sealed class Colors : System.Windows.Forms.ProfessionalColorTable
    {
        public override System.Drawing.Color ToolStripDropDownBackground => System.Drawing.Color.FromArgb(20, 30, 43);
        public override System.Drawing.Color MenuBorder => System.Drawing.Color.FromArgb(59, 82, 107);
        public override System.Drawing.Color MenuItemSelectedGradientBegin => System.Drawing.Color.FromArgb(41, 68, 87);
        public override System.Drawing.Color MenuItemSelectedGradientEnd => System.Drawing.Color.FromArgb(27, 48, 72);
        public override System.Drawing.Color MenuItemSelected => MenuItemSelectedGradientBegin;
        public override System.Drawing.Color MenuItemBorder => System.Drawing.Color.FromArgb(107, 185, 216);
    }
}
