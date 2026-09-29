using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FrClassifier.Migrations
{
    /// <inheritdoc />
    public partial class InitialFinancialClassification : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "financial_documents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FileName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    ContentType = table.Column<string>(type: "character varying(127)", maxLength: 127, nullable: false),
                    FileSizeBytes = table.Column<long>(type: "bigint", nullable: false),
                    Sha256 = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    StorageKey = table.Column<string>(type: "text", nullable: false),
                    ImportStatus = table.Column<string>(type: "character varying(24)", maxLength: 24, nullable: false),
                    ImportError = table.Column<string>(type: "text", nullable: true),
                    UploadedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_financial_documents", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "financial_accounts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FinancialDocumentId = table.Column<Guid>(type: "uuid", nullable: false),
                    CompanyCode = table.Column<string>(type: "character varying(4)", maxLength: 4, nullable: false),
                    FiscalYear = table.Column<int>(type: "integer", nullable: false),
                    AccountingDocumentNumber = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    LedgerLineNumber = table.Column<string>(type: "character varying(6)", maxLength: 6, nullable: false),
                    GLAccount = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    GLAccountName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    LineDescription = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    PostingDate = table.Column<DateOnly>(type: "date", nullable: false),
                    DocumentDate = table.Column<DateOnly>(type: "date", nullable: true),
                    AmountInTransactionCurrency = table.Column<decimal>(type: "numeric(19,4)", precision: 19, scale: 4, nullable: false),
                    TransactionCurrencyCode = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: false),
                    AmountInCompanyCodeCurrency = table.Column<decimal>(type: "numeric(19,4)", precision: 19, scale: 4, nullable: true),
                    CompanyCodeCurrencyCode = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: true),
                    ProfitCenter = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    CostCenter = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    Segment = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    SourceWorksheet = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    SourceRowNumber = table.Column<int>(type: "integer", nullable: false),
                    SourceRowHash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    ClassificationStatus = table.Column<string>(type: "character varying(24)", maxLength: 24, nullable: false),
                    ClassificationRequestId = table.Column<Guid>(type: "uuid", nullable: true),
                    ClassificationRequestedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ClassifiedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_financial_accounts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_financial_accounts_financial_documents_FinancialDocumentId",
                        column: x => x.FinancialDocumentId,
                        principalTable: "financial_documents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "financial_account_classifications",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FinancialAccountId = table.Column<Guid>(type: "uuid", nullable: false),
                    RequestId = table.Column<Guid>(type: "uuid", nullable: false),
                    Category = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Confidence = table.Column<decimal>(type: "numeric(6,5)", precision: 6, scale: 5, nullable: true),
                    Rationale = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    ClassifierName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ModelVersion = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ResultJson = table.Column<string>(type: "jsonb", nullable: true),
                    ReceivedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_financial_account_classifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_financial_account_classifications_financial_accounts_Financ~",
                        column: x => x.FinancialAccountId,
                        principalTable: "financial_accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_financial_account_classifications_FinancialAccountId_Receiv~",
                table: "financial_account_classifications",
                columns: new[] { "FinancialAccountId", "ReceivedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_financial_account_classifications_RequestId",
                table: "financial_account_classifications",
                column: "RequestId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_financial_accounts_ClassificationRequestId",
                table: "financial_accounts",
                column: "ClassificationRequestId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_financial_accounts_ClassificationStatus_PostingDate",
                table: "financial_accounts",
                columns: new[] { "ClassificationStatus", "PostingDate" });

            migrationBuilder.CreateIndex(
                name: "IX_financial_accounts_FinancialDocumentId_SourceWorksheet_Sour~",
                table: "financial_accounts",
                columns: new[] { "FinancialDocumentId", "SourceWorksheet", "SourceRowNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_financial_documents_Sha256",
                table: "financial_documents",
                column: "Sha256");

            migrationBuilder.CreateIndex(
                name: "IX_financial_documents_UploadedAt",
                table: "financial_documents",
                column: "UploadedAt");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "financial_account_classifications");

            migrationBuilder.DropTable(
                name: "financial_accounts");

            migrationBuilder.DropTable(
                name: "financial_documents");
        }
    }
}
