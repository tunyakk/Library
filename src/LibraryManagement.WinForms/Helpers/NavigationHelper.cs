using LibraryManagement.WinForms.Forms;

namespace LibraryManagement.WinForms.Helpers;

public static class NavigationHelper
{
    public static void GoToDashboard(Control control)
    {
        var form = control.FindForm();
        if (form is MainForm mainForm)
        {
            mainForm.NavigateToDashboard();
        }
    }
}
