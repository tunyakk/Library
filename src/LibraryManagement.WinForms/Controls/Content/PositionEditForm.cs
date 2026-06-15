using LibraryManagement.Application.Dtos;
using LibraryManagement.Application.Services;
using LibraryManagement.WinForms.Helpers;
using LibraryManagement.WinForms.Theme;
using Microsoft.Extensions.DependencyInjection;

namespace LibraryManagement.WinForms.Controls.Content;

public class PositionEditForm : Form
{
    private readonly IServiceProvider _services;
    private readonly IPositionService _positionService;
    private readonly PositionDto _position;

    private readonly TextBox _txtTitle = new() { Width = 300 };
    private readonly TextBox _txtDescription = new() { Multiline = true, ScrollBars = ScrollBars.Vertical, Width = 300, Height = 80 };
    private readonly Button _btnSave = new() { Text = "Сохранить", Width = 100, Height = 30 };
    private readonly Button _btnCancel = new() { Text = "Отмена", Width = 100, Height = 30 };

    public PositionEditForm(IServiceProvider services, PositionDto position)
    {
        _services = services;
        _positionService = services.GetRequiredService<IPositionService>();
        _position = position;

        Text = position.Id == 0 ? "Новая должность" : "Редактирование должности";
        Size = new Size(450, 300);
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;

        BuildLayout();
        LoadData();

        ThemeManager.ApplyDarkTheme(this);
    }

    private void BuildLayout()
    {
        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 2,
            Padding = new Padding(15)
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));

        layout.Controls.Add(Ui.MakeLabel("Название:"), 0, 0);
        layout.Controls.Add(_txtTitle, 1, 0);
        layout.Controls.Add(Ui.MakeLabel("Описание:"), 0, 1);
        layout.Controls.Add(_txtDescription, 1, 1);

        var buttons = new Panel { Dock = DockStyle.Bottom, Height = 50 };
        _btnSave.Location = new Point(220, 10);
        _btnCancel.Location = new Point(330, 10);
        _btnSave.Click += async (_, _) => await SaveAsync();
        _btnCancel.Click += (_, _) => DialogResult = DialogResult.Cancel;
        buttons.Controls.Add(_btnSave);
        buttons.Controls.Add(_btnCancel);

        Controls.Add(layout);
        Controls.Add(buttons);
    }

    private void LoadData()
    {
        _txtTitle.Text = _position.Title;
        _txtDescription.Text = _position.Description ?? string.Empty;
    }

    private async Task SaveAsync()
    {
        _btnSave.Enabled = false;
        try
        {
            _position.Title = _txtTitle.Text;
            _position.Description = string.IsNullOrWhiteSpace(_txtDescription.Text) ? null : _txtDescription.Text;
            var result = await _positionService.SaveAsync(_position);
            if (Ui.ReportResult(this, result))
                DialogResult = DialogResult.OK;
        }
        finally
        {
            _btnSave.Enabled = true;
        }
    }
}
