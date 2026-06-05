using EatsDash.Models;
using Microsoft.EntityFrameworkCore;

namespace EatsDash.Data;

public static class DbInitializer
{
    public static async Task InitializeAsync(ApplicationDbContext context)
    {
        await context.Database.EnsureCreatedAsync();

        await context.Database.ExecuteSqlRawAsync("""
            CREATE TABLE IF NOT EXISTS Couriers (
                Id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
                Nickname TEXT NOT NULL,
                IsActive INTEGER NOT NULL DEFAULT 1
            );
            """);

        await context.Database.ExecuteSqlRawAsync("""
            CREATE TABLE IF NOT EXISTS ReviewReactions (
                Id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
                ReviewId INTEGER NOT NULL,
                UserId TEXT NOT NULL,
                IsLike INTEGER NOT NULL,
                FOREIGN KEY (ReviewId) REFERENCES Reviews (Id) ON DELETE CASCADE
            );
            """);

        await context.Database.ExecuteSqlRawAsync("""
            CREATE UNIQUE INDEX IF NOT EXISTS IX_ReviewReactions_ReviewId_UserId
            ON ReviewReactions (ReviewId, UserId);
            """);

        await context.Database.ExecuteSqlRawAsync("""
            CREATE TABLE IF NOT EXISTS ReviewReports (
                Id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
                ReviewId INTEGER NOT NULL,
                ReporterId TEXT NOT NULL,
                Reason TEXT NOT NULL,
                Comment TEXT NULL,
                CreatedAt TEXT NOT NULL,
                FOREIGN KEY (ReviewId) REFERENCES Reviews (Id) ON DELETE CASCADE
            );
            """);

        await context.Database.ExecuteSqlRawAsync("""
            DROP INDEX IF EXISTS IX_ReviewReports_ReviewId_ReporterId;
            """);

        await context.Database.ExecuteSqlRawAsync("""
            CREATE INDEX IF NOT EXISTS IX_ReviewReports_ReviewId_ReporterId
            ON ReviewReports (ReviewId, ReporterId);
            """);

        await EnsureColumnAsync(context, "Reviews", "CourierId", "INTEGER NOT NULL DEFAULT 1");
        await EnsureColumnAsync(context, "Reviews", "CourierNickname", "TEXT NOT NULL DEFAULT ''");
        await EnsureColumnAsync(context, "Reviews", "Status", "INTEGER NOT NULL DEFAULT 0");

        await SeedCouriersAsync(context);

        await EnsureColumnAsync(context, "AspNetUsers", "CompactReviews", "INTEGER NOT NULL DEFAULT 0");
        await EnsureColumnAsync(context, "AspNetUsers", "IsModerator", "INTEGER NOT NULL DEFAULT 0");
        await EnsureColumnAsync(context, "AspNetUsers", "IsBlocked", "INTEGER NOT NULL DEFAULT 0");

        await context.Database.ExecuteSqlRawAsync("""
            UPDATE Reviews
            SET CourierNickname = (
                SELECT Nickname FROM Couriers WHERE Couriers.Id = Reviews.CourierId
            )
            WHERE CourierNickname = '' OR CourierNickname IS NULL;
            """);
    }

    /// <summary>Список курьеров для выбора в отзыве.</summary>
    private static readonly string[] CourierNicknames =
    [
        "Алексей К.",
        "Артём Л.",
        "Денис Н.",
        "Джек Г.",
        "Дмитрий К.",
        "Коул Б.",
        "Рафаэль Х."
    ];

    private static async Task SeedCouriersAsync(ApplicationDbContext context)
    {
        var existing = await context.Couriers.OrderBy(c => c.Id).ToListAsync();

        if (existing.Count == 0)
        {
            foreach (var nickname in CourierNicknames)
            {
                context.Couriers.Add(new Courier { Nickname = nickname, IsActive = true });
            }

            await context.SaveChangesAsync();
            return;
        }

        for (var i = 0; i < CourierNicknames.Length; i++)
        {
            if (i < existing.Count)
            {
                existing[i].Nickname = CourierNicknames[i];
                existing[i].IsActive = true;
            }
            else
            {
                context.Couriers.Add(new Courier { Nickname = CourierNicknames[i], IsActive = true });
            }
        }

        for (var i = CourierNicknames.Length; i < existing.Count; i++)
        {
            existing[i].IsActive = false;
        }

        await context.SaveChangesAsync();
    }

    private static async Task EnsureColumnAsync(
        ApplicationDbContext context,
        string table,
        string column,
        string definition)
    {
        try
        {
            await context.Database.ExecuteSqlRawAsync(
                $"ALTER TABLE {table} ADD COLUMN {column} {definition}");
        }
        catch
        {
            // Колонка уже существует
        }
    }
}
