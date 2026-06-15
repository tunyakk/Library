using LibraryManagement.Domain.Common;

namespace LibraryManagement.Domain.Entities;

public class Position : Entity
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }

    public ICollection<Employee> Employees { get; set; } = new List<Employee>();
}
