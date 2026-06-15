using LibraryManagement.Application.Abstractions;
using LibraryManagement.Application.Dtos;
using LibraryManagement.Application.Services;
using LibraryManagement.Domain.Enums;
using LibraryManagement.WinForms.Controls;
using LibraryManagement.WinForms.Controls.Content;
using LibraryManagement.WinForms.Helpers;
using LibraryManagement.WinForms.Theme;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;

namespace LibraryManagement.WinForms.Forms;

public partial class MainForm : Form
{
    private readonly IConfiguration _configuration;
    private readonly IServiceProvider _services;

    private Panel _dashboardPanel = null!;
    private ContentPanel _contentPanel = null!;
    private Panel _footerBar = null!;
    private PictureBox _pictureBox = null!;
    private ComboBox _cmbLibrarian = null!;
    private Label _lblTitle = null!;

    public MainForm(IConfiguration configuration, IServiceProvider services)
    {
        _configuration = configuration;
        _services = services;
        InitializeComponent();
        BuildLayout();
        ApplyTheme();
        Shown += OnFirstShown;
    }

    private void BuildLayout()
    {
        _footerBar = new Panel
        {
            Dock = DockStyle.Bottom,
            Height = 30,
            BackColor = ThemeManager.FooterBackground
        };

        var lblStudent = new Label
        {
            Text = "Петин Евгений",
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleRight,
            Padding = new Padding(0, 0, 15, 0),
            ForeColor = ThemeManager.TextHeader,
            Font = new Font("Segoe UI", 9F)
        };
        _footerBar.Controls.Add(lblStudent);

        var lblUser = new Label
        {
            Text = "  Библиотека",
            Dock = DockStyle.Left,
            Width = 250,
            TextAlign = ContentAlignment.MiddleLeft,
            ForeColor = ThemeManager.AccentLight,
            Font = new Font("Segoe UI", 9F),
            Padding = new Padding(10, 0, 0, 0)
        };
        _footerBar.Controls.Add(lblUser);

        _contentPanel = new ContentPanel();
        _dashboardPanel = new Panel { Dock = DockStyle.Fill };

        BuildDashboard();

        Controls.Add(_contentPanel);
        Controls.Add(_dashboardPanel);
        Controls.Add(_footerBar);

        _contentPanel.SendToBack();
    }

    private void BuildDashboard()
    {
        var leftPanel = new Panel
        {
            Dock = DockStyle.Left,
            Width = 310,
            BackColor = ThemeManager.FormBackground,
            Padding = new Padding(0)
        };

        var imgBorder = new Panel
        {
            Dock = DockStyle.None,
            Width = 290,
            Height = 290,
            BackColor = ThemeManager.Accent,
            Padding = new Padding(2),
            Tag = "NoTheme",
            Anchor = AnchorStyles.None
        };

        _pictureBox = new PictureBox
        {
            Dock = DockStyle.Fill,
            SizeMode = PictureBoxSizeMode.Zoom,
            BackColor = Color.White
        };
        var bgImage = LoadBackgroundImage();
        if (bgImage != null) _pictureBox.Image = bgImage;
        imgBorder.Controls.Add(_pictureBox);
        leftPanel.Controls.Add(imgBorder);

        leftPanel.Resize += (_, _) =>
        {
            imgBorder.Left = (leftPanel.ClientSize.Width - imgBorder.Width) / 2;
            imgBorder.Top = (leftPanel.ClientSize.Height - imgBorder.Height) / 2;
        };

        var rightPanel = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = ThemeManager.FormBackground,
            Padding = new Padding(15)
        };

        BuildRightPanel(rightPanel);

        _dashboardPanel.Controls.Add(rightPanel);
        _dashboardPanel.Controls.Add(leftPanel);
    }

    private void BuildRightPanel(Panel rightPanel)
    {
        _lblTitle = new Label
        {
            Text = "Библиотека",
            Font = new Font("Segoe UI Semibold", 14F),
            ForeColor = ThemeManager.Accent,
            BackColor = Color.Transparent,
            Dock = DockStyle.Top,
            Height = 36,
            TextAlign = ContentAlignment.MiddleCenter,
            Tag = "NoTheme"
        };

        var buttonsPanel = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            ColumnCount = 2,
            RowCount = 4,
            Padding = new Padding(6),
            BackColor = Color.Transparent,
            AutoSize = true,
            Height = 200
        };
        buttonsPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
        buttonsPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));

        buttonsPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 44));
        buttonsPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 44));
        buttonsPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 44));
        buttonsPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 44));

        var btnPositions = MakeMenuButton("Должности");
        btnPositions.Dock = DockStyle.Fill;
        btnPositions.Click += (_, _) => NavigateTo("positions_list");

        var btnEmployees = MakeMenuButton("Сотрудники");
        btnEmployees.Dock = DockStyle.Fill;
        btnEmployees.Click += (_, _) => NavigateTo("employees_list");

        var btnAuthors = MakeMenuButton("Авторы");
        btnAuthors.Dock = DockStyle.Fill;
        btnAuthors.Click += (_, _) => NavigateTo("authors_list");

        var btnBooks = MakeMenuButton("Книги");
        btnBooks.Dock = DockStyle.Fill;
        btnBooks.Click += (_, _) => NavigateTo("books_list");

        var btnPublishers = MakeMenuButton("Издательства");
        btnPublishers.Dock = DockStyle.Fill;
        btnPublishers.Click += (_, _) => NavigateTo("publishers_list");

        var btnLoans = MakeMenuButton("Выдачи");
        btnLoans.Dock = DockStyle.Fill;
        btnLoans.Click += (_, _) => NavigateTo("loans_list");

        var btnReaders = MakeMenuButton("Читатели");
        btnReaders.Dock = DockStyle.Fill;
        btnReaders.Click += (_, _) => NavigateTo("readers_list");

        buttonsPanel.Controls.Add(btnPositions, 0, 0);
        buttonsPanel.Controls.Add(btnEmployees, 1, 0);
        buttonsPanel.Controls.Add(btnAuthors, 0, 1);
        buttonsPanel.Controls.Add(btnBooks, 1, 1);
        buttonsPanel.Controls.Add(btnPublishers, 0, 2);
        buttonsPanel.Controls.Add(btnLoans, 1, 2);
        buttonsPanel.Controls.Add(btnReaders, 0, 3);

        var librarianPanel = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            Height = 38,
            BackColor = Color.Transparent,
            Padding = new Padding(8, 3, 8, 3),
            ColumnCount = 3,
            RowCount = 1
        };
        librarianPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 110));
        librarianPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        librarianPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));
        librarianPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

        var lblLibrarian = new Label
        {
            Text = "Библиотекарь:",
            Font = new Font("Segoe UI", 10F),
            ForeColor = ThemeManager.TextPrimary,
            BackColor = Color.Transparent,
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleLeft,
            Tag = "NoTheme"
        };

        _cmbLibrarian = new ComboBox
        {
            DropDownStyle = ComboBoxStyle.DropDownList,
            Font = new Font("Segoe UI", 10F),
            Dock = DockStyle.Fill,
            BackColor = ThemeManager.TextBoxBackground,
            ForeColor = ThemeManager.TextPrimary,
            FlatStyle = FlatStyle.Flat,
            Tag = "NoTheme",
            Margin = new Padding(5, 5, 5, 5)
        };

        var btnIssue = new Button
        {
            Text = "Выдача",
            Dock = DockStyle.Fill,
            Font = new Font("Segoe UI", 10F, FontStyle.Bold),
            FlatStyle = FlatStyle.Flat,
            BackColor = ThemeManager.Accent,
            ForeColor = ThemeManager.TextHeader,
            TextAlign = ContentAlignment.MiddleCenter,
            Tag = "NoTheme",
            Margin = new Padding(0)
        };
        btnIssue.FlatAppearance.BorderColor = ThemeManager.Accent;
        btnIssue.FlatAppearance.MouseOverBackColor = ThemeManager.AccentLight;
        btnIssue.Click += (_, _) => NavigateTo("issue_loan");

        librarianPanel.Controls.Add(lblLibrarian, 0, 0);
        librarianPanel.Controls.Add(_cmbLibrarian, 1, 0);
        librarianPanel.Controls.Add(btnIssue, 2, 0);

        var reportsPanel = new Panel
        {
            Dock = DockStyle.Top,
            Height = 80,
            BackColor = Color.Transparent,
            Padding = new Padding(5)
        };

        var reportsGroupBox = new GroupBox
        {
            Text = "Отчёты",
            Dock = DockStyle.Fill,
            Font = new Font("Segoe UI", 9F, FontStyle.Bold),
            ForeColor = ThemeManager.Accent,
            BackColor = Color.Transparent,
            Padding = new Padding(6, 1, 6, 2),
            Tag = "NoTheme"
        };

        var reportsButtons = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false,
            AutoSize = false,
            Padding = new Padding(2, 14, 2, 2),
            BackColor = Color.Transparent,
            Tag = "NoTheme"
        };

        var btnReportEditions = MakeMenuButton("Список изданий");
        btnReportEditions.Width = 150;
        btnReportEditions.Height = 30;
        btnReportEditions.Click += (_, _) => NavigateTo("books_list");

        var btnReportReaders = MakeMenuButton("Список читателей");
        btnReportReaders.Width = 150;
        btnReportReaders.Height = 30;
        btnReportReaders.Click += (_, _) => NavigateTo("readers_list");

        var btnReportOverdue = MakeMenuButton("Просроченные книги");
        btnReportOverdue.Width = 170;
        btnReportOverdue.Height = 30;
        btnReportOverdue.Click += (_, _) => NavigateTo("reports");

        reportsButtons.Controls.Add(btnReportEditions);
        reportsButtons.Controls.Add(btnReportReaders);
        reportsButtons.Controls.Add(btnReportOverdue);

        reportsGroupBox.Controls.Add(reportsButtons);
        reportsPanel.Controls.Add(reportsGroupBox);

        rightPanel.Controls.Add(reportsPanel);
        rightPanel.Controls.Add(librarianPanel);
        rightPanel.Controls.Add(buttonsPanel);
        rightPanel.Controls.Add(_lblTitle);
    }

    private static Button MakeMenuButton(string text)
    {
        return new Button
        {
            Text = text,
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 10F),
            ForeColor = ThemeManager.TextButtonDark,
            BackColor = ThemeManager.ButtonBackground,
            Dock = DockStyle.None,
            Height = 36,
            Margin = new Padding(5),
            Tag = "NoTheme",
            FlatAppearance =
            {
                BorderSize = 2,
                BorderColor = ThemeManager.ButtonBorderColor,
                MouseOverBackColor = ThemeManager.SideButtonHover
            }
        };
    }

    public void NavigateTo(string actionKey)
    {
        var control = CreateContentControl(actionKey);
        if (control != null)
        {
            _dashboardPanel.Visible = false;
            _contentPanel.BringToFront();
            _contentPanel.ShowContent(control);
        }
    }

    public void NavigateToDashboard()
    {
        _contentPanel.ClearContent();
        _contentPanel.SendToBack();
        _dashboardPanel.Visible = true;
        _dashboardPanel.BringToFront();
    }

    private Control CreateContentControl(string actionKey)
    {
        Control control = actionKey switch
        {
            "positions_list" => new PositionsListControl(_services) { Dock = DockStyle.Fill },
            "employees_list" => new EmployeesListControl(_services) { Dock = DockStyle.Fill },
            "authors_list" => new AuthorsListControl(_services) { Dock = DockStyle.Fill },
            "genres_list" => new GenresListControl(_services) { Dock = DockStyle.Fill },
            "publishers_list" => new PublishersListControl(_services) { Dock = DockStyle.Fill },
            "readers_list" => new ReadersListControl(_services) { Dock = DockStyle.Fill },
            "books_list" => new BooksListControl(_services) { Dock = DockStyle.Fill },
            "book_search" => new BooksListControl(_services) { Dock = DockStyle.Fill },
            "issue_loan" => new IssueLoanControl(_services) { Dock = DockStyle.Fill },
            "return_loan" => new ReturnLoanControl(_services) { Dock = DockStyle.Fill },
            "loans_list" => new LoansListControl(_services) { Dock = DockStyle.Fill },
            "reports" => new ReportsControl(_services) { Dock = DockStyle.Fill },
            _ => new BooksListControl(_services) { Dock = DockStyle.Fill }
        };

        switch (control)
        {
            case PositionsListControl c: _ = c.LoadDataAsync(); break;
            case EmployeesListControl c: _ = c.LoadDataAsync(); break;
            case AuthorsListControl c: _ = c.LoadDataAsync(); break;
            case GenresListControl c: _ = c.LoadDataAsync(); break;
            case PublishersListControl c: _ = c.LoadDataAsync(); break;
            case ReadersListControl c: _ = c.LoadDataAsync(); break;
            case BooksListControl c: _ = c.LoadDataAsync(); break;
            case LoansListControl c: _ = c.LoadDataAsync(); break;
            case IssueLoanControl c: _ = c.LoadDataAsync(); break;
            case ReportsControl c: _ = c.LoadDataAsync(); break;
        }

        return control;
    }

    private Image? LoadBackgroundImage()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Resources", "library_bg.jpg");
        if (File.Exists(path))
        {
            try { return Image.FromFile(path); }
            catch { }
        }
        return null;
    }

    private async void OnFirstShown(object? sender, EventArgs e)
    {
        Shown -= OnFirstShown;
        await LoadLibrarianComboBoxAsync();
    }

    private async Task LoadLibrarianComboBoxAsync()
    {
        try
        {
            using var scope = _services.CreateScope();
            var employeeService = scope.ServiceProvider.GetRequiredService<IEmployeeService>();
            var employees = await employeeService.GetAllAsync();
            var list = new List<EmployeeDto> { new() { Id = 0, FirstName = "— выберите —", LastName = "" } };
            list.AddRange(employees);
            _cmbLibrarian.DisplayMember = nameof(EmployeeDto.FullName);
            _cmbLibrarian.ValueMember = nameof(EmployeeDto.Id);
            _cmbLibrarian.DataSource = list;
        }
        catch
        {
            _cmbLibrarian.DataSource = new List<string> { "Не загружено" };
        }
    }

    private void ApplyTheme()
    {
        ThemeManager.ApplyDarkTheme(this);
        ThemeManager.ApplyDarkTheme(_footerBar);
    }
}
