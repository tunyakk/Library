using FluentValidation;
using LibraryManagement.Application.Abstractions;
using LibraryManagement.Application.Common;
using LibraryManagement.Application.Dtos;
using LibraryManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagement.Application.Services;

public class PositionService : IPositionService
{
    private readonly ILibraryDbContext _db;
    private readonly IValidator<PositionDto> _validator;

    public PositionService(ILibraryDbContext db, IValidator<PositionDto> validator)
    {
        _db = db;
        _validator = validator;
    }

    public async Task<IReadOnlyList<PositionDto>> GetAllAsync(string? search = null, CancellationToken cancellationToken = default)
    {
        var query = _db.Positions.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var pattern = $"%{search.Trim()}%";
            query = query.Where(p =>
                EF.Functions.Like(p.Title, pattern) ||
                (p.Description != null && EF.Functions.Like(p.Description, pattern)));
        }

        return await query
            .OrderBy(p => p.Title)
            .Select(p => new PositionDto
            {
                Id = p.Id,
                Title = p.Title,
                Description = p.Description
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<PositionDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var p = await _db.Positions.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (p is null) return null;
        return new PositionDto
        {
            Id = p.Id,
            Title = p.Title,
            Description = p.Description
        };
    }

    public async Task<Result<int>> SaveAsync(PositionDto dto, CancellationToken cancellationToken = default)
    {
        var validation = await _validator.ValidateAsync(dto, cancellationToken);
        if (!validation.IsValid)
        {
            return Result<int>.ValidationFailure(validation.Errors.Select(e => e.ErrorMessage).ToList());
        }

        var title = dto.Title.Trim();

        var duplicate = await _db.Positions.FirstOrDefaultAsync(p =>
            p.Title == title && p.Id != dto.Id,
            cancellationToken);

        if (duplicate != null)
        {
            return Result<int>.Failure($"Должность «{duplicate.Title}» уже существует.");
        }

        Position entity;
        if (dto.Id == 0)
        {
            entity = new Position();
            _db.Positions.Add(entity);
        }
        else
        {
            entity = await _db.Positions.FirstOrDefaultAsync(x => x.Id == dto.Id, cancellationToken)
                ?? throw new InvalidOperationException($"Должность Id={dto.Id} не найдена.");
            entity.UpdatedAt = DateTime.UtcNow;
        }

        entity.Title = title;
        entity.Description = string.IsNullOrWhiteSpace(dto.Description) ? null : dto.Description.Trim();

        await _db.SaveChangesAsync(cancellationToken);
        return Result<int>.Success(entity.Id);
    }

    public async Task<Result> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await _db.Positions
            .Include(p => p.Employees)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

        if (entity is null)
        {
            return Result.Failure("Должность не найдена.");
        }

        if (entity.Employees.Any())
        {
            return Result.Failure("Нельзя удалить должность, на которой состоят сотрудники. Сначала переведите или удалите сотрудников.");
        }

        _db.Positions.Remove(entity);
        await _db.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
