using FluentValidation;
using LibraryManagement.Application.Abstractions;
using LibraryManagement.Application.Common;
using LibraryManagement.Application.Dtos;
using LibraryManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagement.Application.Services;

public class EmployeeService : IEmployeeService
{
    private readonly ILibraryDbContext _db;
    private readonly IValidator<EmployeeDto> _validator;

    public EmployeeService(ILibraryDbContext db, IValidator<EmployeeDto> validator)
    {
        _db = db;
        _validator = validator;
    }

    public async Task<IReadOnlyList<EmployeeDto>> GetAllAsync(string? search = null, bool includeInactive = false, CancellationToken cancellationToken = default)
    {
        IQueryable<Employee> query = _db.Employees
            .AsNoTracking()
            .Include(e => e.Position);

        if (!includeInactive)
            query = query.Where(e => e.IsActive);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var pattern = $"%{search.Trim()}%";
            query = query.Where(e =>
                EF.Functions.Like(e.LastName, pattern) ||
                EF.Functions.Like(e.FirstName, pattern) ||
                (e.MiddleName != null && EF.Functions.Like(e.MiddleName, pattern)));
        }

        return await query
            .OrderBy(e => e.LastName).ThenBy(e => e.FirstName)
            .Select(e => new EmployeeDto
            {
                Id = e.Id,
                FirstName = e.FirstName,
                LastName = e.LastName,
                MiddleName = e.MiddleName,
                PositionId = e.PositionId,
                PositionTitle = e.Position.Title,
                Phone = e.Phone,
                Email = e.Email,
                BirthDate = e.BirthDate,
                HireDate = e.HireDate,
                IsActive = e.IsActive
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<EmployeeDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var e = await _db.Employees
            .AsNoTracking()
            .Include(x => x.Position)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (e is null) return null;
        return new EmployeeDto
        {
            Id = e.Id,
            FirstName = e.FirstName,
            LastName = e.LastName,
            MiddleName = e.MiddleName,
            PositionId = e.PositionId,
            PositionTitle = e.Position.Title,
            Phone = e.Phone,
            Email = e.Email,
            BirthDate = e.BirthDate,
            HireDate = e.HireDate,
            IsActive = e.IsActive
        };
    }

    public async Task<Result<int>> SaveAsync(EmployeeDto dto, CancellationToken cancellationToken = default)
    {
        var validation = await _validator.ValidateAsync(dto, cancellationToken);
        if (!validation.IsValid)
        {
            return Result<int>.ValidationFailure(validation.Errors.Select(e => e.ErrorMessage).ToList());
        }

        var lastName = dto.LastName.Trim();
        var firstName = dto.FirstName.Trim();
        var middleName = string.IsNullOrWhiteSpace(dto.MiddleName) ? null : dto.MiddleName.Trim();

        var positionExists = await _db.Positions.AnyAsync(p => p.Id == dto.PositionId, cancellationToken);
        if (!positionExists)
        {
            return Result<int>.Failure("Указанная должность не найдена.");
        }

        Employee entity;
        if (dto.Id == 0)
        {
            entity = new Employee();
            _db.Employees.Add(entity);
        }
        else
        {
            entity = await _db.Employees.FirstOrDefaultAsync(x => x.Id == dto.Id, cancellationToken)
                ?? throw new InvalidOperationException($"Сотрудник Id={dto.Id} не найден.");
            entity.UpdatedAt = DateTime.UtcNow;
        }

        entity.FirstName = firstName;
        entity.LastName = lastName;
        entity.MiddleName = middleName;
        entity.PositionId = dto.PositionId;
        entity.Phone = string.IsNullOrWhiteSpace(dto.Phone) ? null : dto.Phone.Trim();
        entity.Email = string.IsNullOrWhiteSpace(dto.Email) ? null : dto.Email.Trim();
        entity.BirthDate = dto.BirthDate;
        entity.HireDate = dto.HireDate;
        entity.IsActive = dto.IsActive;

        await _db.SaveChangesAsync(cancellationToken);
        return Result<int>.Success(entity.Id);
    }

    public async Task<Result> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await _db.Employees
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);

        if (entity is null)
        {
            return Result.Failure("Сотрудник не найден.");
        }

        _db.Employees.Remove(entity);
        await _db.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
