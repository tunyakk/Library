using LibraryManagement.WinForms.Theme;

namespace LibraryManagement.WinForms.Controls;

public class CrudToolbar : Panel
{
    public TextBox SearchBox { get; }
    public Button BackButton { get; }
    public Button AddButton { get; }
    public Button EditButton { get; }
    public Button DeleteButton { get; }
    public Button RefreshButton { get; }
    public FlowLayoutPanel ExtraButtonsPanel { get; }

    public event EventHandler? SearchTextChanged;
    public event EventHandler? AddClicked;
    public event EventHandler? EditClicked;
    public event EventHandler? DeleteClicked;
    public event EventHandler? RefreshClicked;
    public event EventHandler? BackClicked;

    public CrudToolbar()
    {
        Dock = DockStyle.Top;
        Height = 50;
        Padding = new Padding(8);
        BackColor = ThemeManager.FormBackground;

        BackButton = new Button
        {
            Text = "← Назад",
            Width = 90,
            Height = 28,
            Location = new Point(8, 11),
            FlatStyle = FlatStyle.Flat,
            BackColor = ThemeManager.Accent,
            ForeColor = ThemeManager.TextHeader,
            Tag = "NoTheme",
            FlatAppearance =
            {
                BorderSize = 1,
                BorderColor = ThemeManager.Accent,
                MouseOverBackColor = ThemeManager.AccentLight
            }
        };
        BackButton.Click += (s, e) => BackClicked?.Invoke(this, EventArgs.Empty);

        SearchBox = new TextBox
        {
            PlaceholderText = "Поиск...",
            Width = 200,
            Location = new Point(108, 14),
            BackColor = ThemeManager.TextBoxBackground,
            ForeColor = ThemeManager.TextPrimary,
            BorderStyle = BorderStyle.FixedSingle
        };
        SearchBox.TextChanged += (s, e) => SearchTextChanged?.Invoke(this, EventArgs.Empty);

        AddButton = MakeToolButton("Добавить", 320, () => AddClicked?.Invoke(this, EventArgs.Empty));
        EditButton = MakeToolButton("Изменить", 412, () => EditClicked?.Invoke(this, EventArgs.Empty));
        DeleteButton = MakeToolButton("Удалить", 504, () => DeleteClicked?.Invoke(this, EventArgs.Empty));
        RefreshButton = MakeToolButton("Обновить", 596, () => RefreshClicked?.Invoke(this, EventArgs.Empty));

        ExtraButtonsPanel = new FlowLayoutPanel
        {
            FlowDirection = FlowDirection.LeftToRight,
            AutoSize = true,
            Anchor = AnchorStyles.Left | AnchorStyles.Top,
            Location = new Point(690, 10),
            Margin = new Padding(0)
        };

        Controls.Add(SearchBox);
        Controls.Add(BackButton);
        Controls.Add(AddButton);
        Controls.Add(EditButton);
        Controls.Add(DeleteButton);
        Controls.Add(RefreshButton);
        Controls.Add(ExtraButtonsPanel);
    }

    private Button MakeToolButton(string text, int x, Action click)
    {
        var btn = new Button
        {
            Text = text,
            Width = 88,
            Height = 28,
            Location = new Point(x, 11),
            FlatStyle = FlatStyle.Flat,
            BackColor = ThemeManager.Accent,
            ForeColor = ThemeManager.TextHeader,
            Tag = "NoTheme",
            FlatAppearance =
            {
                BorderSize = 1,
                BorderColor = ThemeManager.Accent,
                MouseOverBackColor = ThemeManager.AccentLight
            }
        };
        btn.Click += (s, e) => click();
        return btn;
    }
}
