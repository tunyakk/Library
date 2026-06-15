using LibraryManagement.Application.Dtos;
using LibraryManagement.Application.Services;
using LibraryManagement.WinForms.Controls;
using LibraryManagement.WinForms.Forms;
using LibraryManagement.WinForms.Helpers;
using LibraryManagement.WinForms.Theme;
using Microsoft.Extensions.DependencyInjection;

namespace LibraryManagement.WinForms.Controls.Content;

public class ReportsControl : UserControl
{
    private readonly IServiceProvider _services;
    private readonly IReportService _reportService;

    private readonly DataGridView _gridOverdue = NewGrid();
    private readonly SimpleBarChart _chartOverdue = new() { Dock = DockStyle.Top, Height = 300, Title = "Просроченные выдачи (дни просрочки)" };

    public ReportsControl(IServiceProvider services)
    {
        _services = services;
        _reportService = services.GetRequiredService<IReportService>();

        Dock = DockStyle.Fill;

        _gridOverdue.Columns.AddRange(
            new DataGridViewTextBoxColumn { HeaderText = "Книга", DataPropertyName = nameof(LoanDto.BookTitle), FillWeight = 80 },
            new DataGridViewTextBoxColumn { HeaderText = "Читатель", DataPropertyName = nameof(LoanDto.ReaderFullName), FillWeight = 70 },
            new DataGridViewTextBoxColumn { HeaderText = "Билет", DataPropertyName = nameof(LoanDto.ReaderCardNumber), FillWeight = 35 },
            new DataGridViewTextBoxColumn { HeaderText = "Срок был", DataPropertyName = nameof(LoanDto.DueDate), DefaultCellStyle = { Format = "dd.MM.yyyy" }, FillWeight = 40 },
            new DataGridViewTextBoxColumn { HeaderText = "Дней просрочки", DataPropertyName = nameof(LoanDto.OverdueDays), FillWeight = 30 },
            new DataGridViewTextBoxColumn { HeaderText = "Штраф", DataPropertyName = nameof(LoanDto.FineAmount), DefaultCellStyle = { Format = "0.00 ₽" }, FillWeight = 30 }
        );

        var topPanel = new Panel { Dock = DockStyle.Top, Height = 44, Padding = new Padding(8) };
        var btnBack = new Button
        {
            Text = "← Назад",
            Width = 90,
            Height = 28,
            Dock = DockStyle.Left,
            FlatStyle = FlatStyle.Flat,
            BackColor = ThemeManager.Accent,
            ForeColor = ThemeManager.TextHeader,
            Tag = "NoTheme"
        };
        btnBack.FlatAppearance.BorderColor = ThemeManager.Accent;
        btnBack.FlatAppearance.MouseOverBackColor = ThemeManager.AccentLight;
        btnBack.Click += (_, _) => NavigationHelper.GoToDashboard(this);

        var btnLoansList = new Button
        {
            Text = "К списку выдач",
            Width = 140,
            Height = 28,
            Dock = DockStyle.Left,
            FlatStyle = FlatStyle.Flat,
            BackColor = ThemeManager.Accent,
            ForeColor = ThemeManager.TextHeader,
            Tag = "NoTheme"
        };
        btnLoansList.FlatAppearance.BorderColor = ThemeManager.Accent;
        btnLoansList.FlatAppearance.MouseOverBackColor = ThemeManager.AccentLight;
        btnLoansList.Click += (_, _) =>
        {
            var mainForm = FindForm();
            if (mainForm is MainForm mf) mf.NavigateTo("loans_list");
        };

        topPanel.Controls.Add(btnLoansList);
        topPanel.Controls.Add(btnBack);

        var contentPanel = new Panel { Dock = DockStyle.Fill };
        contentPanel.Controls.Add(_gridOverdue);
        contentPanel.Controls.Add(_chartOverdue);

        Controls.Add(contentPanel);
        Controls.Add(topPanel);

        ThemeManager.ApplyDarkTheme(this);
    }

    public async Task LoadDataAsync()
    {
        try
        {
            var overdueLoans = await _reportService.GetOverdueLoansAsync();
            _gridOverdue.DataSource = overdueLoans.ToList();
            foreach (DataGridViewRow row in _gridOverdue.Rows)
            {
                row.DefaultCellStyle.ForeColor = Color.DarkRed;
            }

            var overdueColors = new[]
            {
                Color.FromArgb(200, 80, 80), Color.FromArgb(180, 60, 60), Color.FromArgb(160, 50, 50),
                Color.FromArgb(200, 80, 80), Color.FromArgb(180, 60, 60), Color.FromArgb(160, 50, 50)
            };
            _chartOverdue.SetData(overdueLoans.Select((l, idx) => new SimpleBarChart.BarItem(
                l.ReaderFullName, l.OverdueDays, overdueColors[idx % overdueColors.Length])));
        }
        catch (Exception ex)
        {
            Ui.ShowError(this, "Не удалось загрузить отчёт: " + ex.Message);
        }
    }

    private static DataGridView NewGrid() => new()
    {
        Dock = DockStyle.Fill,
        AutoGenerateColumns = false,
        ReadOnly = true,
        AllowUserToAddRows = false,
        AllowUserToDeleteRows = false,
        SelectionMode = DataGridViewSelectionMode.FullRowSelect,
        MultiSelect = false,
        RowHeadersVisible = false,
        AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
        BackgroundColor = ThemeManager.GridBackground
    };
}
