using FrClassifier.Entities;
using Microsoft.EntityFrameworkCore;

namespace FrClassifier.Data;

public sealed class FrClassifierDbContext(DbContextOptions<FrClassifierDbContext> options)
    : DbContext(options)
{
    public DbSet<FinancialDocument> FinancialDocuments => Set<FinancialDocument>();
    public DbSet<FinancialAccount> FinancialAccounts => Set<FinancialAccount>();
    public DbSet<FinancialAccountClassification> FinancialAccountClassifications =>
        Set<FinancialAccountClassification>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<FinancialDocument>(entity =>
        {
            entity.ToTable("financial_documents");
            entity.HasKey(document => document.Id);
            entity.Property(document => document.FileName).HasMaxLength(255).IsRequired();
            entity.Property(document => document.ContentType).HasMaxLength(127).IsRequired();
            entity.Property(document => document.Sha256).HasMaxLength(64).IsRequired();
            entity.Property(document => document.ImportStatus).HasConversion<string>().HasMaxLength(24);
            entity.HasIndex(document => document.Sha256);
            entity.HasIndex(document => document.UploadedAt);
        });

        modelBuilder.Entity<FinancialAccount>(entity =>
        {
            entity.ToTable("financial_accounts");
            entity.HasKey(account => account.Id);
            entity.Property(account => account.CompanyCode).HasMaxLength(4).IsRequired();
            entity.Property(account => account.Ledger).HasMaxLength(5).IsRequired();
            entity.Property(account => account.AccountingDocumentNumber).HasMaxLength(10).IsRequired();
            entity.Property(account => account.LedgerLineNumber).HasMaxLength(6).IsRequired();
            entity.Property(account => account.GLAccount).HasMaxLength(10).IsRequired();
            entity.Property(account => account.GLAccountName).HasMaxLength(100);
            entity.Property(account => account.LineDescription).HasMaxLength(200);
            entity.Property(account => account.TransactionCurrencyCode).HasMaxLength(5).IsRequired();
            entity.Property(account => account.CompanyCodeCurrencyCode).HasMaxLength(5);
            entity.Property(account => account.AmountInTransactionCurrency).HasPrecision(19, 4);
            entity.Property(account => account.AmountInCompanyCodeCurrency).HasPrecision(19, 4);
            entity.Property(account => account.ProfitCenter).HasMaxLength(10);
            entity.Property(account => account.CostCenter).HasMaxLength(10);
            entity.Property(account => account.Segment).HasMaxLength(10);
            entity.Property(account => account.SourceWorksheet).HasMaxLength(100).IsRequired();
            entity.Property(account => account.SourceRowHash).HasMaxLength(64).IsRequired();
            entity.Property(account => account.ClassificationStatus)
                .HasConversion<string>()
                .HasMaxLength(24);
            entity.HasIndex(account => new
            {
                account.FinancialDocumentId,
                account.SourceWorksheet,
                account.SourceRowNumber
            }).IsUnique();
            entity.HasIndex(account => new
            {
                account.ClassificationStatus,
                account.PostingDate
            });
            entity.HasIndex(account => account.ClassificationRequestId).IsUnique();
            entity.HasOne(account => account.FinancialDocument)
                .WithMany(document => document.Accounts)
                .HasForeignKey(account => account.FinancialDocumentId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<FinancialAccountClassification>(entity =>
        {
            entity.ToTable("financial_account_classifications");
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
                classification.FinancialAccountId,
                classification.ReceivedAt
            });
            entity.HasOne(classification => classification.FinancialAccount)
                .WithMany(account => account.Classifications)
                .HasForeignKey(classification => classification.FinancialAccountId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}