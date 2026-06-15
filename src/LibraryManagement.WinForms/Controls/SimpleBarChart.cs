using LibraryManagement.WinForms.Theme;

namespace LibraryManagement.WinForms.Controls;

public class SimpleBarChart : Control
{
    public record BarItem(string Label, double Value, Color? Color = null);

    private List<BarItem> _items = new();
    private string _title = string.Empty;

    public SimpleBarChart()
    {
        DoubleBuffered = true;
        BackColor = ThemeManager.GridBackground;
        Font = new Font("Segoe UI", 9F);
    }

    [System.ComponentModel.DefaultValue("")]
    public string Title
    {
        get => _title;
        set { _title = value ?? string.Empty; Invalidate(); }
    }

    public void SetData(IEnumerable<BarItem> items)
    {
        _items = items?.ToList() ?? new List<BarItem>();
        Invalidate();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        var g = e.Graphics;
        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
        g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

        if (Width <= 0 || Height <= 0) return;

        var titleFont = new Font(Font.FontFamily, 11F, FontStyle.Bold);
        int titleHeight = 0;
        if (!string.IsNullOrEmpty(_title))
        {
            titleHeight = (int)g.MeasureString(_title, titleFont).Height + 10;
            using var titleBrush = new SolidBrush(ThemeManager.Accent);
            g.DrawString(_title, titleFont, titleBrush, new PointF(10, 6));
        }

        if (_items.Count == 0)
        {
            var msg = "Нет данных для отображения.";
            var sz = g.MeasureString(msg, Font);
            using var noDataBrush = new SolidBrush(ThemeManager.TextPrimary);
            g.DrawString(msg, Font, noDataBrush,
                new PointF((Width - sz.Width) / 2, (Height - sz.Height) / 2));
            titleFont.Dispose();
            return;
        }

        int top = titleHeight + 8;
        int bottom = 30;
        int left = 14;
        int right = 70;
        int chartHeight = Height - top - bottom;
        int chartWidth = Width - left - right;
        int barGap = 2;

        double max = _items.Max(i => Math.Abs(i.Value));
        if (max <= 0) max = 1;

        int barWidth = Math.Max(8, (chartWidth - barGap * (_items.Count - 1)) / _items.Count);

        var stepColors = new[]
        {
            Color.FromArgb(50, 80, 140),
            Color.FromArgb(70, 110, 180),
            Color.FromArgb(90, 140, 200),
            Color.FromArgb(120, 160, 210),
            Color.FromArgb(150, 130, 190),
            Color.FromArgb(180, 110, 170),
        };

        for (int i = 0; i < _items.Count; i++)
        {
            var item = _items[i];
            int x = left + i * (barWidth + barGap);
            int barH = (int)(chartHeight * (Math.Abs(item.Value) / max));
            int y = top + chartHeight - barH;

            var baseColor = item.Color ?? stepColors[i % stepColors.Length];

            using var brush = new System.Drawing.Drawing2D.LinearGradientBrush(
                new Rectangle(x, y, barWidth, barH),
                ControlPaint.Light(baseColor, 0.3f),
                baseColor,
                System.Drawing.Drawing2D.LinearGradientMode.Vertical);
            g.FillRectangle(brush, x, y, barWidth, barH);

            using var borderPen = new Pen(ControlPaint.Dark(baseColor, 0.15f), 1);
            g.DrawRectangle(borderPen, x, y, barWidth, barH);

            if (i > 0)
            {
                var prevItem = _items[i - 1];
                int prevH = (int)(chartHeight * (Math.Abs(prevItem.Value) / max));
                int prevY = top + chartHeight - prevH;
                int prevX = left + (i - 1) * (barWidth + barGap) + barWidth;

                using var stepPen = new Pen(baseColor, 2);
                g.DrawLine(stepPen, prevX, prevY, x, prevY);
                g.DrawLine(stepPen, x, prevY, x, y);
            }

            using var valBrush = new SolidBrush(ThemeManager.TextPrimary);
            var valText = item.Value.ToString("0.##");
            var valSize = g.MeasureString(valText, Font);
            g.DrawString(valText, Font, valBrush,
                new PointF(x + (barWidth - valSize.Width) / 2, y - valSize.Height - 2));

            using var lblBrush = new SolidBrush(ThemeManager.TextPrimary);
            var lblRect = new RectangleF(x - 4, top + chartHeight + 4, barWidth + 8, bottom);
            using var sf = new StringFormat
            {
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Near,
                Trimming = StringTrimming.EllipsisCharacter
            };
            g.DrawString(item.Label, Font, lblBrush, lblRect, sf);
        }

        using var axisPen = new Pen(ControlPaint.Dark(ThemeManager.GridBackground, 0.2f), 1);
        g.DrawLine(axisPen, left, top + chartHeight, left + chartWidth, top + chartHeight);

        titleFont.Dispose();
    }
}
