using LibraryManagement.Application.Dtos;
using LibraryManagement.Application.Services;
using LibraryManagement.WinForms.Helpers;
using LibraryManagement.WinForms.Theme;
using Microsoft.Extensions.DependencyInjection;

namespace LibraryManagement.WinForms.Controls.Content;

public class EmployeeEditForm : Form
{
    private readonly IServiceProvider _services;
    private readonly IEmployeeService _employeeService;
    private readonly IPositionService _positionService;
    private readonly EmployeeDto _employee;

    private readonly TextBox _txtFirstName = new() { Width = 300 };
    private readonly TextBox _txtLastName = new() { Width = 300 };
    private readonly TextBox _txtMiddleName = new() { Width = 300 };
    private readonly ComboBox _cmbPosition = new() { DropDownStyle = ComboBoxStyle.DropDownList, Width = 300 };
    private readonly TextBox _txtPhone = new() { Width = 300 };
    private readonly TextBox _txtEmail = new() { Width = 300 };
    private readonly DateTimePicker _dtBirthDate = new() { Format = DateTimePickerFormat.Short, ShowCheckBox = true, Checked = false, Width = 200 };
    private readonly DateTimePicker _dtHireDate = new() { Format = DateTimePickerFormat.Short, Width = 200 };
    private readonly CheckBox _chkIsActive = new() { Text = "Активен", AutoSize = true, Checked = true };
    private readonly Button _btnSave = new() { Text = "Сохранить", Width = 100, Height = 30 };
    private readonly Button _btnCancel = new() { Text = "Отмена", Width = 100, Height = 30 };

    public EmployeeEditForm(IServiceProvider services, EmployeeDto employee)
    {
        _services = services;
        _employeeService = services.GetRequiredService<IEmployeeService>();
        _positionService = services.GetRequiredService<IPositionService>();
        _employee = employee;

        Text = employee.Id == 0 ? "Новый сотрудник" : "Редактирование сотрудника";
        Size = new Size(480, 480);
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;

        BuildLayout();
        _ = LoadDataAsync();

        ThemeManager.ApplyDarkTheme(this);
    }

    private void BuildLayout()
    {
        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 8,
            Padding = new Padding(15)
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));

        layout.Controls.Add(Ui.MakeLabel("Фамилия:"), 0, 0); layout.Controls.Add(_txtLastName, 1, 0);
        layout.Controls.Add(Ui.MakeLabel("Имя:"), 0, 1); layout.Controls.Add(_txtFirstName, 1, 1);
        layout.Controls.Add(Ui.MakeLabel("Отчество:"), 0, 2); layout.Controls.Add(_txtMiddleName, 1, 2);
        layout.Controls.Add(Ui.MakeLabel("Должность:"), 0, 3); layout.Controls.Add(_cmbPosition, 1, 3);
        layout.Controls.Add(Ui.MakeLabel("Телефон:"), 0, 4); layout.Controls.Add(_txtPhone, 1, 4);
        layout.Controls.Add(Ui.MakeLabel("Email:"), 0, 5); layout.Controls.Add(_txtEmail, 1, 5);
        layout.Controls.Add(Ui.MakeLabel("Дата рождения:"), 0, 6); layout.Controls.Add(_dtBirthDate, 1, 6);
        layout.Controls.Add(Ui.MakeLabel("Дата найма:"), 0, 7); layout.Controls.Add(_dtHireDate, 1, 7);

        var buttons = new Panel { Dock = DockStyle.Bottom, Height = 50 };
        _btnSave.Location = new Point(240, 10);
        _btnCancel.Location = new Point(350, 10);
        _btnSave.Click += async (_, _) => await SaveAsync();
        _btnCancel.Click += (_, _) => DialogResult = DialogResult.Cancel;

        var chkPanel = new Panel { Dock = DockStyle.Bottom, Height = 30, Padding = new Padding(115, 5, 0, 0) };
        chkPanel.Controls.Add(_chkIsActive);

        buttons.Controls.Add(_btnSave);
        buttons.Controls.Add(_btnCancel);

        Controls.Add(layout);
        Controls.Add(chkPanel);
        Controls.Add(buttons);
    }

    private async Task LoadDataAsync()
    {
        try
        {
            var positions = await _positionService.GetAllAsync();
            _cmbPosition.DisplayMember = nameof(PositionDto.Title);
            _cmbPosition.ValueMember = nameof(PositionDto.Id);
            _cmbPosition.DataSource = positions.ToList();

            _txtLastName.Text = _employee.LastName;
            _txtFirstName.Text = _employee.FirstName;
            _txtMiddleName.Text = _employee.MiddleName ?? string.Empty;
            _txtPhone.Text = _employee.Phone ?? string.Empty;
            _txtEmail.Text = _employee.Email ?? string.Empty;
            _chkIsActive.Checked = _employee.IsActive;
            _dtHireDate.Value = _employee.HireDate == default ? DateTime.Today : _employee.HireDate;

            if (_employee.BirthDate.HasValue)
            {
                _dtBirthDate.Value = _employee.BirthDate.Value;
                _dtBirthDate.Checked = true;
            }

            if (_employee.PositionId > 0)
                _cmbPosition.SelectedValue = _employee.PositionId;
        }
        catch (Exception ex)
        {
            Ui.ShowError(this, "Не удалось загрузить должности: " + ex.Message);
        }
    }

    private async Task SaveAsync()
    {
        _btnSave.Enabled = false;
        try
        {
            _employee.LastName = _txtLastName.Text;
            _employee.FirstName = _txtFirstName.Text;
            _employee.MiddleName = string.IsNullOrWhiteSpace(_txtMiddleName.Text) ? null : _txtMiddleName.Text;
            _employee.PositionId = _cmbPosition.SelectedValue is int pid ? pid : 0;
            _employee.Phone = string.IsNullOrWhiteSpace(_txtPhone.Text) ? null : _txtPhone.Text;
            _employee.Email = string.IsNullOrWhiteSpace(_txtEmail.Text) ? null : _txtEmail.Text;
            _employee.BirthDate = _dtBirthDate.Checked ? _dtBirthDate.Value.Date : null;
            _employee.HireDate = _dtHireDate.Value.Date;
            _employee.IsActive = _chkIsActive.Checked;

            var result = await _employeeService.SaveAsync(_employee);
            if (Ui.ReportResult(this, result))
                DialogResult = DialogResult.OK;
        }
        finally
        {
            _btnSave.Enabled = true;
        }
    }
}
