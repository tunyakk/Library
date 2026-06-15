namespace LibraryManagement.WinForms.Theme;

public static class ThemeManager
{
    public static readonly Color FormBackground = Color.FromArgb(210, 175, 110);
    public static readonly Color TopMenuBackground = Color.FromArgb(50, 80, 140);
    public static readonly Color SidePanelBackground = Color.FromArgb(210, 175, 110);
    public static readonly Color SideButtonNormal = Color.FromArgb(210, 175, 110);
    public static readonly Color SideButtonHover = Color.FromArgb(225, 190, 130);
    public static readonly Color SideButtonSelected = Color.FromArgb(50, 80, 140);
    public static readonly Color Accent = Color.FromArgb(50, 80, 140);
    public static readonly Color AccentLight = Color.FromArgb(70, 110, 180);
    public static readonly Color TextPrimary = Color.FromArgb(40, 40, 40);
    public static readonly Color TextHeader = Color.White;
    public static readonly Color TextOnAccent = Color.White;
    public static readonly Color TextButtonDark = Color.FromArgb(50, 80, 140);
    public static readonly Color GridBackground = Color.FromArgb(250, 245, 235);
    public static readonly Color GridHeaderBackground = Color.FromArgb(50, 80, 140);
    public static readonly Color GridRowAlternate = Color.FromArgb(240, 235, 225);
    public static readonly Color TextBoxBackground = Color.White;
    public static readonly Color ControlBorder = Color.FromArgb(50, 80, 140);
    public static readonly Color FooterBackground = Color.FromArgb(50, 80, 140);
    public static readonly Color ButtonBackground = Color.FromArgb(235, 220, 180);
    public static readonly Color ButtonBorderColor = Color.FromArgb(50, 80, 140);

    public static readonly Font DefaultFont = new("Segoe UI", 9F);
    public static readonly Font HeaderFont = new("Segoe UI Semibold", 11F);
    public static readonly Font IconFont = new("Segoe MDL2 Assets", 16F);

    public static void ApplyDarkTheme(Form form)
    {
        form.BackColor = FormBackground;
        form.ForeColor = TextPrimary;
        form.Font = DefaultFont;
        ApplyDarkTheme((Control)form);
    }

    public static void ApplyDarkTheme(Control parent)
    {
        foreach (Control control in parent.Controls)
        {
            if (control.Tag?.ToString() == "NoTheme")
            {
                if (control.HasChildren)
                    ApplyDarkTheme(control);
                continue;
            }

            switch (control)
            {
                case Button btn:
                    btn.FlatStyle = FlatStyle.Flat;
                    btn.BackColor = ButtonBackground;
                    btn.ForeColor = TextButtonDark;
                    btn.FlatAppearance.BorderColor = ButtonBorderColor;
                    btn.FlatAppearance.MouseOverBackColor = SideButtonHover;
                    btn.Font = DefaultFont;
                    btn.FlatAppearance.BorderSize = 2;
                    break;

                case TextBox tb:
                    tb.BackColor = TextBoxBackground;
                    tb.ForeColor = TextPrimary;
                    tb.BorderStyle = BorderStyle.FixedSingle;
                    tb.Font = DefaultFont;
                    break;

                case DataGridView dgv:
                    dgv.BackgroundColor = GridBackground;
                    dgv.DefaultCellStyle.BackColor = GridBackground;
                    dgv.DefaultCellStyle.ForeColor = TextPrimary;
                    dgv.DefaultCellStyle.SelectionBackColor = Accent;
                    dgv.DefaultCellStyle.SelectionForeColor = TextHeader;
                    dgv.DefaultCellStyle.Font = DefaultFont;
                    dgv.ColumnHeadersDefaultCellStyle.BackColor = GridHeaderBackground;
                    dgv.ColumnHeadersDefaultCellStyle.ForeColor = TextHeader;
                    dgv.ColumnHeadersDefaultCellStyle.Font = DefaultFont;
                    dgv.EnableHeadersVisualStyles = false;
                    dgv.RowHeadersVisible = false;
                    dgv.GridColor = ControlBorder;
                    dgv.BorderStyle = BorderStyle.FixedSingle;
                    break;

                case TabPage tp:
                    tp.BackColor = FormBackground;
                    tp.ForeColor = TextPrimary;
                    break;

                case Panel panel:
                    panel.BackColor = FormBackground;
                    break;

                case UserControl uc:
                    uc.BackColor = FormBackground;
                    uc.ForeColor = TextPrimary;
                    break;

                case Label lbl:
                    lbl.ForeColor = TextPrimary;
                    lbl.BackColor = Color.Transparent;
                    break;

                case TreeView tv:
                    tv.BackColor = FormBackground;
                    tv.ForeColor = TextPrimary;
                    tv.BorderStyle = BorderStyle.None;
                    tv.Font = DefaultFont;
                    break;

                case TabControl tc:
                    tc.BackColor = FormBackground;
                    tc.ForeColor = TextPrimary;
                    tc.Font = DefaultFont;
                    break;

                case PictureBox pb:
                    pb.BackColor = Color.Transparent;
                    break;

                case CheckBox cb:
                    cb.ForeColor = TextPrimary;
                    cb.BackColor = Color.Transparent;
                    break;

                case ComboBox cb:
                    cb.BackColor = TextBoxBackground;
                    cb.ForeColor = TextPrimary;
                    cb.FlatStyle = FlatStyle.Flat;
                    break;

                case GroupBox gb:
                    gb.ForeColor = Accent;
                    gb.BackColor = Color.Transparent;
                    break;

                case NumericUpDown num:
                    num.BackColor = TextBoxBackground;
                    num.ForeColor = TextPrimary;
                    break;

                case DateTimePicker dtp:
                    dtp.BackColor = TextBoxBackground;
                    dtp.ForeColor = TextPrimary;
                    break;
            }

            if (control.HasChildren)
            {
                ApplyDarkTheme(control);
            }
        }
    }
}
