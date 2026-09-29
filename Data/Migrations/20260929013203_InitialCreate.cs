using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FrClassifier.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "documents",
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
                    table.PrimaryKey("PK_documents", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "accounts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DocumentId = table.Column<Guid>(type: "uuid", nullable: false),
                    EntityCode = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    FiscalYear = table.Column<int>(type: "integer", nullable: true),
                    AccountCode = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    AccountName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    PostingDate = table.Column<DateOnly>(type: "date", nullable: true),
                    DocumentDate = table.Column<DateOnly>(type: "date", nullable: true),
                    Amount = table.Column<decimal>(type: "numeric(19,4)", precision: 19, scale: 4, nullable: false),
                    CurrencyCode = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    ReportingAmount = table.Column<decimal>(type: "numeric(19,4)", precision: 19, scale: 4, nullable: true),
                    ReportingCurrencyCode = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    SourceReference = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    SourceLocation = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    DimensionsJson = table.Column<string>(type: "jsonb", nullable: true),
                    SourceRowNumber = table.Column<int>(type: "integer", nullable: true),
                    SourceRowHash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    ClassificationStatus = table.Column<string>(type: "character varying(24)", maxLength: 24, nullable: false),
                    ClassificationRequestId = table.Column<Guid>(type: "uuid", nullable: true),
                    ClassificationRequestedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ClassifiedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_accounts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_accounts_documents_DocumentId",
                        column: x => x.DocumentId,
                        principalTable: "documents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "account_classifications",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AccountId = table.Column<Guid>(type: "uuid", nullable: false),
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
                    table.PrimaryKey("PK_account_classifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_account_classifications_accounts_AccountId",
                        column: x => x.AccountId,
                        principalTable: "accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_account_classifications_AccountId_ReceivedAt",
                table: "account_classifications",
                columns: new[] { "AccountId", "ReceivedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_account_classifications_RequestId",
                table: "account_classifications",
                column: "RequestId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_accounts_ClassificationRequestId",
                table: "accounts",
                column: "ClassificationRequestId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_accounts_ClassificationStatus_PostingDate",
                table: "accounts",
                columns: new[] { "ClassificationStatus", "PostingDate" });

            migrationBuilder.CreateIndex(
                name: "IX_accounts_DocumentId_SourceLocation_SourceRowNumber",
                table: "accounts",
                columns: new[] { "DocumentId", "SourceLocation", "SourceRowNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_documents_Sha256",
                table: "documents",
                column: "Sha256");

            migrationBuilder.CreateIndex(
                name: "IX_documents_UploadedAt",
                table: "documents",
                column: "UploadedAt");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "account_classifications");

            migrationBuilder.DropTable(
                name: "accounts");

            migrationBuilder.DropTable(
                name: "documents");
        }
    }
}
