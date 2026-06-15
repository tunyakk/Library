using LibraryManagement.Application.Dtos;
using LibraryManagement.Application.Services;
using LibraryManagement.WinForms.Helpers;
using LibraryManagement.WinForms.Theme;
using Microsoft.Extensions.DependencyInjection;

namespace LibraryManagement.WinForms.Controls.Content;

public class EmployeesListControl : UserControl
{
    private readonly IServiceProvider _services;
    private readonly IEmployeeService _employeeService;
    private readonly CrudToolbar _toolbar = new();
    private readonly CheckBox _chkIncludeInactive = new() { Text = "Включая уволенных", AutoSize = true };
    private readonly DataGridView _grid = new()
    {
        Dock = DockStyle.Fill,
        AutoGenerateColumns = false,
        ReadOnly = true,
        AllowUserToAddRows = false,
        AllowUserToDeleteRows = false,
        SelectionMode = DataGridViewSelectionMode.FullRowSelect,
        MultiSelect = false,
        RowHeadersVisible = false,
        AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
    };

    public EmployeesListControl(IServiceProvider services)
    {
        _services = services;
        _employeeService = services.GetRequiredService<IEmployeeService>();

        _grid.Columns.AddRange(
            new DataGridViewTextBoxColumn { HeaderText = "ID", DataPropertyName = nameof(EmployeeDto.Id), FillWeight = 25 },
            new DataGridViewTextBoxColumn { HeaderText = "Фамилия", DataPropertyName = nameof(EmployeeDto.LastName), FillWeight = 60 },
            new DataGridViewTextBoxColumn { HeaderText = "Имя", DataPropertyName = nameof(EmployeeDto.FirstName), FillWeight = 60 },
            new DataGridViewTextBoxColumn { HeaderText = "Отчество", DataPropertyName = nameof(EmployeeDto.MiddleName), FillWeight = 60 },
            new DataGridViewTextBoxColumn { HeaderText = "Должность", DataPropertyName = nameof(EmployeeDto.PositionTitle), FillWeight = 60 },
            new DataGridViewTextBoxColumn { HeaderText = "Телефон", DataPropertyName = nameof(EmployeeDto.Phone), FillWeight = 50 },
            new DataGridViewTextBoxColumn { HeaderText = "Email", DataPropertyName = nameof(EmployeeDto.Email), FillWeight = 60 },
            new DataGridViewCheckBoxColumn { HeaderText = "Активен", DataPropertyName = nameof(EmployeeDto.IsActive), FillWeight = 30 }
        );

        _chkIncludeInactive.CheckedChanged += async (_, _) => await ReloadAsync();
        _toolbar.ExtraButtonsPanel.Controls.Add(_chkIncludeInactive);
        _toolbar.AddClicked += async (_, _) => await ShowEditAsync(new EmployeeDto { HireDate = DateTime.UtcNow, IsActive = true });
        _toolbar.EditClicked += async (_, _) => await EditSelectedAsync();
        _toolbar.DeleteClicked += async (_, _) => await DeleteSelectedAsync();
        _toolbar.RefreshClicked += async (_, _) => await ReloadAsync();
        _toolbar.SearchTextChanged += async (_, _) => await ReloadAsync();
        _toolbar.BackClicked += (_, _) => NavigationHelper.GoToDashboard(this);
        _grid.CellDoubleClick += async (_, e) => { if (e.RowIndex >= 0) await EditSelectedAsync(); };

        Controls.Add(_grid);
        Controls.Add(_toolbar);
        ThemeManager.ApplyDarkTheme(this);
    }

    public async Task LoadDataAsync() => await ReloadAsync();

    private async Task ReloadAsync()
    {
        try
        {
            var data = await _employeeService.GetAllAsync(_toolbar.SearchBox.Text, _chkIncludeInactive.Checked);
            _grid.DataSource = data.ToList();
        }
        catch (Exception ex) { Ui.ShowError(FindForm(), ex.Message); }
    }

    private EmployeeDto? GetSelected() => _grid.CurrentRow?.DataBoundItem as EmployeeDto;

    private async Task EditSelectedAsync()
    {
        var current = GetSelected();
        if (current is null) { Ui.ShowInfo(FindForm(), "Выберите сотрудника."); return; }
        var dto = await _employeeService.GetByIdAsync(current.Id) ?? new EmployeeDto();
        await ShowEditAsync(dto);
    }

    private async Task ShowEditAsync(EmployeeDto dto)
    {
        var editForm = new EmployeeEditForm(_services, dto);
        var result = editForm.ShowDialog(FindForm());
        if (result == DialogResult.OK)
            await ReloadAsync();
    }

    private async Task DeleteSelectedAsync()
    {
        var current = GetSelected();
        if (current is null) { Ui.ShowInfo(FindForm(), "Выберите сотрудника."); return; }
        if (!Ui.Confirm(FindForm(), $"Удалить сотрудника «{current.FullName}»?")) return;
        var result = await _employeeService.DeleteAsync(current.Id);
        if (Ui.ReportResult(FindForm(), result, "Сотрудник удалён.")) await ReloadAsync();
    }
}
