using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Vendix.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveRowVersion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "Categories");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "Brands");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "Products",
                type: "bytea",
                nullable: false,
                defaultValueSql: "'\\x0000000000000000'::bytea");

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "Categories",
                type: "bytea",
                nullable: false,
                defaultValueSql: "'\\x0000000000000000'::bytea");

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "Brands",
                type: "bytea",
                nullable: false,
                defaultValueSql: "'\\x0000000000000000'::bytea");
        }
    }
}
