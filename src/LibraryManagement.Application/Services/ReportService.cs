using LibraryManagement.Application.Abstractions;
using LibraryManagement.Application.Dtos;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagement.Application.Services;

public class ReportService : IReportService
{
    private readonly ILibraryDbContext _db;

    public ReportService(ILibraryDbContext db)
    {
        _db = db;
    }

    public async Task<LibraryStats> GetStatsAsync(CancellationToken cancellationToken = default)
    {
        var todayDate = DateTime.UtcNow.Date;
        return new LibraryStats
        {
            TotalBooks = await _db.Books.CountAsync(cancellationToken),
            TotalCopies = await _db.Books.SumAsync(b => (int?)b.TotalCopies, cancellationToken) ?? 0,
            AvailableCopies = await _db.Books.SumAsync(b => (int?)b.AvailableCopies, cancellationToken) ?? 0,
            TotalReaders = await _db.Readers.CountAsync(cancellationToken),
            ActiveLoans = await _db.Loans.CountAsync(l => l.ReturnedAt == null, cancellationToken),
            OverdueLoans = await _db.Loans.CountAsync(l => l.ReturnedAt == null && l.DueDate.Date < todayDate, cancellationToken)
        };
    }

    public async Task<IReadOnlyList<PopularBookRow>> GetPopularBooksAsync(int top = 10, CancellationToken cancellationToken = default)
    {
        return await _db.Loans
            .AsNoTracking()
            .GroupBy(l => l.BookId)
            .Select(g => new
            {
                BookId = g.Key,
                LoanCount = g.Count()
            })
            .OrderByDescending(x => x.LoanCount)
            .Take(top)
            .Join(_db.Books.Include(b => b.Author),
                  agg => agg.BookId,
                  b => b.Id,
                  (agg, b) => new PopularBookRow
                  {
                      BookId = b.Id,
                      Title = b.Title,
                      AuthorFullName = (b.Author.MiddleName == null || b.Author.MiddleName == "")
                          ? b.Author.LastName + " " + b.Author.FirstName
                          : b.Author.LastName + " " + b.Author.FirstName + " " + b.Author.MiddleName,
                      LoanCount = agg.LoanCount
                  })
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<ActiveReaderRow>> GetActiveReadersAsync(int top = 10, CancellationToken cancellationToken = default)
    {
        return await _db.Loans
            .AsNoTracking()
            .GroupBy(l => l.ReaderId)
            .Select(g => new
            {
                ReaderId = g.Key,
                LoanCount = g.Count()
            })
            .OrderByDescending(x => x.LoanCount)
            .Take(top)
            .Join(_db.Readers,
                  agg => agg.ReaderId,
                  r => r.Id,
                  (agg, r) => new ActiveReaderRow
                  {
                      ReaderId = r.Id,
                      CardNumber = r.CardNumber,
                      FullName = (r.MiddleName == null || r.MiddleName == "")
                          ? r.LastName + " " + r.FirstName
                          : r.LastName + " " + r.FirstName + " " + r.MiddleName,
                      LoanCount = agg.LoanCount
                  })
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<LoanDto>> GetOverdueLoansAsync(CancellationToken cancellationToken = default)
    {
        var todayDate = DateTime.UtcNow.Date;
        return await _db.Loans
            .AsNoTracking()
            .Include(l => l.Book)
            .Include(l => l.Reader)
            .Include(l => l.IssuedByUser)
            .Where(l => l.ReturnedAt == null && l.DueDate.Date < todayDate)
            .OrderBy(l => l.DueDate)
            .Select(l => new LoanDto
            {
                Id = l.Id,
                BookId = l.BookId,
                BookTitle = l.Book.Title,
                ReaderId = l.ReaderId,
                ReaderFullName = (l.Reader.MiddleName == null || l.Reader.MiddleName == "")
                    ? l.Reader.LastName + " " + l.Reader.FirstName
                    : l.Reader.LastName + " " + l.Reader.FirstName + " " + l.Reader.MiddleName,
                ReaderCardNumber = l.Reader.CardNumber,
                LoanDate = l.LoanDate,
                DueDate = l.DueDate,
                ReturnedAt = l.ReturnedAt,
                Status = l.Status,
                FineAmount = l.FineAmount,
                Notes = l.Notes,
                IssuedByUserId = l.IssuedByUserId,
                IssuedByUserName = l.IssuedByUser.FullName
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<MonthlyLoanRow>> GetMonthlyLoansAsync(int months = 12, CancellationToken cancellationToken = default)
    {
        var allLoans = await _db.Loans
            .AsNoTracking()
            .Select(l => new { l.LoanDate.Year, l.LoanDate.Month })
            .ToListAsync(cancellationToken);

        var grouped = allLoans
            .GroupBy(x => new { x.Year, x.Month })
            .ToDictionary(g => (g.Key.Year, g.Key.Month), g => g.Count());

        var result = new List<MonthlyLoanRow>();
        var now = DateTime.Now;
        for (int i = months - 1; i >= 0; i--)
        {
            var d = now.AddMonths(-i);
            grouped.TryGetValue((d.Year, d.Month), out var count);
            result.Add(new MonthlyLoanRow { Year = d.Year, Month = d.Month, LoanCount = count });
        }
        return result;
    }

    public async Task<IReadOnlyList<GenreDistributionRow>> GetGenreDistributionAsync(CancellationToken cancellationToken = default)
    {
        return await _db.Books
            .AsNoTracking()
            .Include(b => b.Genre)
            .GroupBy(b => new { GenreId = b.GenreId, GenreName = b.Genre.Name })
            .Select(g => new GenreDistributionRow
            {
                GenreName = g.Key.GenreName,
                TotalCopies = g.Sum(b => b.TotalCopies),
                AvailableCopies = g.Sum(b => b.AvailableCopies)
            })
            .OrderByDescending(r => r.TotalCopies)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<FineReportRow>> GetFineReportAsync(CancellationToken cancellationToken = default)
    {
        var todayDate = DateTime.UtcNow.Date;
        var overdueLoans = await _db.Loans
            .AsNoTracking()
            .Include(l => l.Reader)
            .Where(l => l.ReturnedAt == null && l.DueDate.Date < todayDate)
            .ToListAsync(cancellationToken);

        return overdueLoans
            .GroupBy(l => new { l.ReaderId, l.Reader!.CardNumber, l.Reader.FirstName, l.Reader.LastName, l.Reader.MiddleName })
            .Select(g => new FineReportRow
            {
                ReaderId = g.Key.ReaderId,
                CardNumber = g.Key.CardNumber,
                FullName = (g.Key.MiddleName == null || g.Key.MiddleName == "")
                    ? g.Key.LastName + " " + g.Key.FirstName
                    : g.Key.LastName + " " + g.Key.FirstName + " " + g.Key.MiddleName,
                OverdueLoansCount = g.Count(),
                TotalFines = g.Sum(l => l.FineAmount ?? 0m),
                MaxOverdueDays = g.Max(l => (int)(todayDate - l.DueDate.Date).TotalDays)
            })
            .OrderByDescending(r => r.TotalFines)
            .ToList();
    }
}
