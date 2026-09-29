using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FrClassifier.Migrations
{
    /// <inheritdoc />
    public partial class AddSapLedger : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Ledger",
                table: "financial_accounts",
                type: "character varying(5)",
                maxLength: 5,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Ledger",
                table: "financial_accounts");
        }
    }
}
