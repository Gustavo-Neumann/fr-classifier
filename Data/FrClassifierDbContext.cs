using FrClassifier.Entities;
using Microsoft.EntityFrameworkCore;

namespace FrClassifier.Data;

public sealed class FrClassifierDbContext(DbContextOptions<FrClassifierDbContext> options)
    : DbContext(options)
{
    public DbSet<Document> Documents => Set<Document>();
    public DbSet<Account> Accounts => Set<Account>();
    public DbSet<AccountClassification> AccountClassifications =>
        Set<AccountClassification>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Document>(entity =>
        {
            entity.ToTable("documents");
            entity.HasKey(document => document.Id);
            entity.Property(document => document.FileName).HasMaxLength(255).IsRequired();
            entity.Property(document => document.ContentType).HasMaxLength(127).IsRequired();
            entity.Property(document => document.Sha256).HasMaxLength(64).IsRequired();
            entity.Property(document => document.ImportStatus).HasConversion<string>().HasMaxLength(24);
            entity.HasIndex(document => document.Sha256);
            entity.HasIndex(document => document.UploadedAt);
        });

        modelBuilder.Entity<Account>(entity =>
        {
            entity.ToTable("accounts");
            entity.HasKey(account => account.Id);
            entity.Property(account => account.EntityCode).HasMaxLength(100);
            entity.Property(account => account.AccountCode).HasMaxLength(100);
            entity.Property(account => account.AccountName).HasMaxLength(200);
            entity.Property(account => account.Description).HasMaxLength(1000);
            entity.Property(account => account.CurrencyCode).HasMaxLength(10).IsRequired();
            entity.Property(account => account.ReportingCurrencyCode).HasMaxLength(10);
            entity.Property(account => account.Amount).HasPrecision(19, 4);
            entity.Property(account => account.ReportingAmount).HasPrecision(19, 4);
            entity.Property(account => account.SourceReference).HasMaxLength(500);
            entity.Property(account => account.SourceLocation).HasMaxLength(500);
            entity.Property(account => account.DimensionsJson).HasColumnType("jsonb");
            entity.Property(account => account.SourceRowHash).HasMaxLength(64).IsRequired();
            entity.Property(account => account.ClassificationStatus)
                .HasConversion<string>()
                .HasMaxLength(24);
            entity.HasIndex(account => new
            {
                account.DocumentId,
                account.SourceLocation,
                account.SourceRowNumber
            }).IsUnique();
            entity.HasIndex(account => new
            {
                account.ClassificationStatus,
                account.PostingDate
            });
            entity.HasIndex(account => account.ClassificationRequestId).IsUnique();
            entity.HasOne(account => account.Document)
                .WithMany(document => document.Accounts)
                .HasForeignKey(account => account.DocumentId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<AccountClassification>(entity =>
        {
            entity.ToTable("account_classifications");
            entity.HasKey(classification => classification.Id);
            entity.Property(classification => classification.Category)
                .HasConversion<string>()
                .HasMaxLength(32);
            entity.Property(classification => classification.Confidence).HasPrecision(6, 5);
            entity.Property(classification => classification.Rationale).HasMaxLength(2000);
            entity.Property(classification => classification.ClassifierName).HasMaxLength(100).IsRequired();
            entity.Property(classification => classification.ModelVersion).HasMaxLength(100);
            entity.Property(classification => classification.ResultJson).HasColumnType("jsonb");
            entity.HasIndex(classification => classification.RequestId).IsUnique();
            entity.HasIndex(classification => new
            {
                classification.AccountId,
                classification.ReceivedAt
            });
            entity.HasOne(classification => classification.Account)
                .WithMany(account => account.Classifications)
                .HasForeignKey(classification => classification.AccountId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}