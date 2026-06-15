using LibraryManagement.Application.Dtos;
using LibraryManagement.Application.Services;
using LibraryManagement.WinForms.Helpers;
using LibraryManagement.WinForms.Theme;
using Microsoft.Extensions.DependencyInjection;

namespace LibraryManagement.WinForms.Controls.Content;

public class PositionsListControl : UserControl
{
    private readonly IServiceProvider _services;
    private readonly IPositionService _positionService;
    private readonly CrudToolbar _toolbar = new();
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

    public PositionsListControl(IServiceProvider services)
    {
        _services = services;
        _positionService = services.GetRequiredService<IPositionService>();

        _grid.Columns.AddRange(
            new DataGridViewTextBoxColumn { HeaderText = "ID", DataPropertyName = nameof(PositionDto.Id), FillWeight = 30 },
            new DataGridViewTextBoxColumn { HeaderText = "Название", DataPropertyName = nameof(PositionDto.Title), FillWeight = 100 },
            new DataGridViewTextBoxColumn { HeaderText = "Описание", DataPropertyName = nameof(PositionDto.Description), FillWeight = 150 }
        );

        _toolbar.AddClicked += async (_, _) => await ShowEditAsync(new PositionDto());
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
            var data = await _positionService.GetAllAsync(_toolbar.SearchBox.Text);
            _grid.DataSource = data.ToList();
        }
        catch (Exception ex) { Ui.ShowError(FindForm(), ex.Message); }
    }

    private PositionDto? GetSelected() => _grid.CurrentRow?.DataBoundItem as PositionDto;

    private async Task EditSelectedAsync()
    {
        var current = GetSelected();
        if (current is null) { Ui.ShowInfo(FindForm(), "Выберите должность."); return; }
        var dto = await _positionService.GetByIdAsync(current.Id) ?? new PositionDto();
        await ShowEditAsync(dto);
    }

    private async Task ShowEditAsync(PositionDto dto)
    {
        var editForm = new PositionEditForm(_services, dto);
        var result = editForm.ShowDialog(FindForm());
        if (result == DialogResult.OK)
            await ReloadAsync();
    }

    private async Task DeleteSelectedAsync()
    {
        var current = GetSelected();
        if (current is null) { Ui.ShowInfo(FindForm(), "Выберите должность."); return; }
        if (!Ui.Confirm(FindForm(), $"Удалить должность «{current.Title}»?")) return;
        var result = await _positionService.DeleteAsync(current.Id);
        if (Ui.ReportResult(FindForm(), result, "Должность удалена.")) await ReloadAsync();
    }
}
