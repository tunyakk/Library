using LibraryManagement.Application.Abstractions;
using LibraryManagement.Data;
using LibraryManagement.Domain.Entities;
using LibraryManagement.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace LibraryManagement.WinForms.Services;

public static class AppDbInitializer
{
    public const string DefaultAdminUsername = "admin";
    public const string DefaultAdminPassword = "admin";

    public static async Task InitializeAsync(IServiceProvider services, CancellationToken cancellationToken = default)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<LibraryDbContext>();
        var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();

        await db.Database.MigrateAsync(cancellationToken);

        if (await db.Users.AnyAsync(cancellationToken)) return;

        await SeedPositionsAsync(db, cancellationToken);
        await SeedGenresAsync(db, cancellationToken);
        await SeedPublishersAsync(db, cancellationToken);
        await SeedAuthorsAsync(db, cancellationToken);
        await SeedBooksAsync(db, cancellationToken);
        await SeedEmployeesAsync(db, hasher, cancellationToken);
        await SeedReadersAsync(db, cancellationToken);
        await SeedLoansAsync(db, cancellationToken);
    }

    private static async Task SeedPositionsAsync(LibraryDbContext db, CancellationToken ct)
    {
        db.Positions.AddRange(
            new Position { Title = "Библиотекарь", Description = "Обслуживание читателей, учёт книг", CreatedAt = DateTime.UtcNow },
            new Position { Title = "Старший библиотекарь", Description = "Контроль работы библиотеки", CreatedAt = DateTime.UtcNow },
            new Position { Title = "Директор", Description = "Управление библиотекой", CreatedAt = DateTime.UtcNow }
        );
        await db.SaveChangesAsync(ct);
    }

    private static async Task SeedGenresAsync(LibraryDbContext db, CancellationToken ct)
    {
        db.Genres.AddRange(
            new Genre { Name = "Художественная литература", Description = "Романы, повести, рассказы", CreatedAt = DateTime.UtcNow },
            new Genre { Name = "Научная литература", Description = "Научные исследования и популярная наука", CreatedAt = DateTime.UtcNow },
            new Genre { Name = "Детская литература", Description = "Книги для детей и подростков", CreatedAt = DateTime.UtcNow },
            new Genre { Name = "Учебная литература", Description = "Учебники и пособия", CreatedAt = DateTime.UtcNow },
            new Genre { Name = "Фантастика", Description = "Научная фантастика и фэнтези", CreatedAt = DateTime.UtcNow }
        );
        await db.SaveChangesAsync(ct);
    }

    private static async Task SeedPublishersAsync(LibraryDbContext db, CancellationToken ct)
    {
        db.Publishers.AddRange(
            new Publisher { Name = "Эксмо", City = "Москва", Description = "Крупное российское издательство", CreatedAt = DateTime.UtcNow },
            new Publisher { Name = "АСТ", City = "Москва", Description = "Издательская группа", CreatedAt = DateTime.UtcNow },
            new Publisher { Name = "Просвещение", City = "Москва", Description = "Учебная и научная литература", CreatedAt = DateTime.UtcNow },
            new Publisher { Name = "Росмэн", City = "Москва", Description = "Детская литература", CreatedAt = DateTime.UtcNow }
        );
        await db.SaveChangesAsync(ct);
    }

    private static async Task SeedAuthorsAsync(LibraryDbContext db, CancellationToken ct)
    {
        db.Authors.AddRange(
            new Author { FirstName = "Лев", LastName = "Толстой", BirthDate = new DateTime(1828, 9, 9), Biography = "Русский писатель", CreatedAt = DateTime.UtcNow },
            new Author { FirstName = "Фёдор", LastName = "Достоевский", BirthDate = new DateTime(1821, 11, 11), Biography = "Русский писатель", CreatedAt = DateTime.UtcNow },
            new Author { FirstName = "Антуан", LastName = "Сент-Экзюпери", BirthDate = new DateTime(1900, 6, 29), Biography = "Французский писатель и лётчик", CreatedAt = DateTime.UtcNow },
            new Author { FirstName = "Николай", LastName = "Гоголь", BirthDate = new DateTime(1809, 3, 20), Biography = "Русский писатель", CreatedAt = DateTime.UtcNow },
            new Author { FirstName = "Илья", LastName = "Ильф", MiddleName = "Аркадьевич", BirthDate = new DateTime(1897, 10, 15), Biography = "Русский советский писатель", CreatedAt = DateTime.UtcNow },
            new Author { FirstName = "Евгений", LastName = "Петров", BirthDate = new DateTime(1903, 12, 13), Biography = "Русский советский писатель", CreatedAt = DateTime.UtcNow }
        );
        await db.SaveChangesAsync(ct);
    }

    private static async Task SeedBooksAsync(LibraryDbContext db, CancellationToken ct)
    {
        var tolstoy = await db.Authors.FirstAsync(a => a.LastName == "Толстой", ct);
        var dostoevsky = await db.Authors.FirstAsync(a => a.LastName == "Достоевский", ct);
        var eksupery = await db.Authors.FirstAsync(a => a.LastName == "Сент-Экзюпери", ct);
        var gogol = await db.Authors.FirstAsync(a => a.LastName == "Гоголь", ct);
        var ilf = await db.Authors.FirstAsync(a => a.LastName == "Ильф", ct);

        var fiction = await db.Genres.FirstAsync(g => g.Name == "Художественная литература", ct);
        var sciFi = await db.Genres.FirstAsync(g => g.Name == "Фантастика", ct);
        var kids = await db.Genres.FirstAsync(g => g.Name == "Детская литература", ct);

        var exmo = await db.Publishers.FirstAsync(p => p.Name == "Эксмо", ct);
        var ast = await db.Publishers.FirstAsync(p => p.Name == "АСТ", ct);
        var pros = await db.Publishers.FirstAsync(p => p.Name == "Просвещение", ct);

        db.Books.AddRange(
            new Book { Title = "Война и мир", AuthorId = tolstoy.Id, GenreId = fiction.Id, PublisherId = exmo.Id, Isbn = "978-5-699-00000-1", TotalCopies = 5, AvailableCopies = 3, PublicationDate = new DateTime(1869, 1, 1), Description = "Роман-эпопея о наполеоновских войнах", CreatedAt = DateTime.UtcNow },
            new Book { Title = "Анна Каренина", AuthorId = tolstoy.Id, GenreId = fiction.Id, PublisherId = ast.Id, Isbn = "978-5-699-00000-2", TotalCopies = 3, AvailableCopies = 2, PublicationDate = new DateTime(1877, 1, 1), Description = "Роман о трагической любви", CreatedAt = DateTime.UtcNow },
            new Book { Title = "Преступление и наказание", AuthorId = dostoevsky.Id, GenreId = fiction.Id, PublisherId = exmo.Id, Isbn = "978-5-699-00000-3", TotalCopies = 4, AvailableCopies = 3, PublicationDate = new DateTime(1866, 1, 1), Description = "Роман о моральных дилеммах", CreatedAt = DateTime.UtcNow },
            new Book { Title = "Идиот", AuthorId = dostoevsky.Id, GenreId = fiction.Id, PublisherId = ast.Id, Isbn = "978-5-699-00000-4", TotalCopies = 2, AvailableCopies = 2, PublicationDate = new DateTime(1869, 1, 1), Description = "Роман о «абсолютно прекрасном человеке»", CreatedAt = DateTime.UtcNow },
            new Book { Title = "Малый принц", AuthorId = eksupery.Id, GenreId = kids.Id, PublisherId = exmo.Id, Isbn = "978-5-699-00000-5", TotalCopies = 6, AvailableCopies = 4, PublicationDate = new DateTime(1943, 4, 1), Description = "Сказка для взрослых", CreatedAt = DateTime.UtcNow },
            new Book { Title = "Мёртвые души", AuthorId = gogol.Id, GenreId = fiction.Id, PublisherId = pros.Id, Isbn = "978-5-699-00000-6", TotalCopies = 3, AvailableCopies = 3, PublicationDate = new DateTime(1842, 1, 1), Description = "Поэма в прозе", CreatedAt = DateTime.UtcNow },
            new Book { Title = "Двенадцать стульев", AuthorId = ilf.Id, GenreId = fiction.Id, PublisherId = ast.Id, Isbn = "978-5-699-00000-7", TotalCopies = 4, AvailableCopies = 2, PublicationDate = new DateTime(1928, 1, 1), Description = "Сатирический роман", CreatedAt = DateTime.UtcNow }
        );
        await db.SaveChangesAsync(ct);
    }

    private static async Task SeedEmployeesAsync(LibraryDbContext db, IPasswordHasher hasher, CancellationToken ct)
    {
        var librarianPos = await db.Positions.FirstAsync(p => p.Title == "Библиотекарь", ct);
        var directorPos = await db.Positions.FirstAsync(p => p.Title == "Директор", ct);

        db.Employees.AddRange(
            new Employee
            {
                FirstName = "Париса", LastName = "Иванова", MiddleName = "Андреевна",
                PositionId = librarianPos.Id, Phone = "+7 (900) 123-45-67", Email = "ivanova@library.ru",
                HireDate = new DateTime(2020, 3, 15), IsActive = true, CreatedAt = DateTime.UtcNow
            },
            new Employee
            {
                FirstName = "Сергей", LastName = "Петров", MiddleName = "Николаевич",
                PositionId = directorPos.Id, Phone = "+7 (900) 765-43-21", Email = "petrov@library.ru",
                HireDate = new DateTime(2018, 1, 10), IsActive = true, CreatedAt = DateTime.UtcNow
            },
            new Employee
            {
                FirstName = "Мария", LastName = "Сидорова", MiddleName = "Ивановна",
                PositionId = librarianPos.Id, Phone = "+7 (900) 111-22-33", Email = "sidorova@library.ru",
                HireDate = new DateTime(2022, 6, 1), IsActive = true, CreatedAt = DateTime.UtcNow
            }
        );
        await db.SaveChangesAsync(ct);

        var adminEmployee = await db.Employees.FirstAsync(e => e.LastName == "Иванова", ct);

        db.Users.Add(new User
        {
            Username = DefaultAdminUsername,
            FullName = "Администратор системы",
            PasswordHash = hasher.Hash(DefaultAdminPassword),
            Role = UserRole.Administrator,
            IsActive = true,
            EmployeeId = adminEmployee.Id,
            CreatedAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync(ct);
    }

    private static async Task SeedReadersAsync(LibraryDbContext db, CancellationToken ct)
    {
        db.Readers.AddRange(
            new Reader { CardNumber = "Б-001", FirstName = "Алексей", LastName = "Козлов", MiddleName = "Сергеевич", Phone = "+7 (910) 111-11-11", Email = "kozlov@mail.ru", RegistrationDate = new DateTime(2023, 1, 10), IsBlocked = false, CreatedAt = DateTime.UtcNow },
            new Reader { CardNumber = "Б-002", FirstName = "Елена", LastName = "Новикова", MiddleName = "Петровна", Phone = "+7 (910) 222-22-22", Email = "novikova@mail.ru", RegistrationDate = new DateTime(2023, 2, 15), IsBlocked = false, CreatedAt = DateTime.UtcNow },
            new Reader { CardNumber = "Б-003", FirstName = "Дмитрий", LastName = "Морозов", MiddleName = "Андреевич", Phone = "+7 (910) 333-33-33", Email = "morozov@mail.ru", RegistrationDate = new DateTime(2023, 3, 20), IsBlocked = false, CreatedAt = DateTime.UtcNow },
            new Reader { CardNumber = "Б-004", FirstName = "Ольга", LastName = "Волкова", MiddleName = "Игоревна", Phone = "+7 (910) 444-44-44", Email = "volkova@mail.ru", RegistrationDate = new DateTime(2023, 4, 5), IsBlocked = false, CreatedAt = DateTime.UtcNow },
            new Reader { CardNumber = "Б-005", FirstName = "Иван", LastName = "Соколов", Phone = "+7 (910) 555-55-55", Email = "sokolov@mail.ru", RegistrationDate = new DateTime(2023, 5, 12), IsBlocked = true, CreatedAt = DateTime.UtcNow }
        );
        await db.SaveChangesAsync(ct);
    }

    private static async Task SeedLoansAsync(LibraryDbContext db, CancellationToken ct)
    {
        var admin = await db.Users.FirstAsync(u => u.Username == DefaultAdminUsername, ct);
        var kozlov = await db.Readers.FirstAsync(r => r.CardNumber == "Б-001", ct);
        var novikova = await db.Readers.FirstAsync(r => r.CardNumber == "Б-002", ct);
        var morozov = await db.Readers.FirstAsync(r => r.CardNumber == "Б-003", ct);

        var warAndPeace = await db.Books.FirstAsync(b => b.Title == "Война и мир", ct);
        var anna = await db.Books.FirstAsync(b => b.Title == "Анна Каренина", ct);
        var crime = await db.Books.FirstAsync(b => b.Title == "Преступление и наказание", ct);
        var prince = await db.Books.FirstAsync(b => b.Title == "Малый принц", ct);
        var twelve = await db.Books.FirstAsync(b => b.Title == "Двенадцать стульев", ct);

        db.Loans.AddRange(
            new Loan
            {
                BookId = warAndPeace.Id, ReaderId = kozlov.Id, IssuedByUserId = admin.Id,
                LoanDate = DateTime.Today.AddDays(-20), DueDate = DateTime.Today.AddDays(-6),
                FineAmount = 50.00m, Status = LoanStatus.Active, CreatedAt = DateTime.UtcNow
            },
            new Loan
            {
                BookId = anna.Id, ReaderId = novikova.Id, IssuedByUserId = admin.Id,
                LoanDate = DateTime.Today.AddDays(-10), DueDate = DateTime.Today.AddDays(4),
                Status = LoanStatus.Active, CreatedAt = DateTime.UtcNow
            },
            new Loan
            {
                BookId = crime.Id, ReaderId = morozov.Id, IssuedByUserId = admin.Id,
                LoanDate = DateTime.Today.AddDays(-30), DueDate = DateTime.Today.AddDays(-16),
                FineAmount = 150.00m, Status = LoanStatus.Active, CreatedAt = DateTime.UtcNow
            },
            new Loan
            {
                BookId = prince.Id, ReaderId = kozlov.Id, IssuedByUserId = admin.Id,
                LoanDate = DateTime.Today.AddDays(-40), DueDate = DateTime.Today.AddDays(-26),
                ReturnedAt = DateTime.Today.AddDays(-28), Status = LoanStatus.Returned,
                CreatedAt = DateTime.UtcNow
            },
            new Loan
            {
                BookId = twelve.Id, ReaderId = novikova.Id, IssuedByUserId = admin.Id,
                LoanDate = DateTime.Today.AddDays(-50), DueDate = DateTime.Today.AddDays(-36),
                ReturnedAt = DateTime.Today.AddDays(-30), Status = LoanStatus.Returned,
                CreatedAt = DateTime.UtcNow
            }
        );
        await db.SaveChangesAsync(ct);
    }
}
