using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Vendix.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixRowVersionForPostgreSQL : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<byte[]>(
                name: "RowVersion",
                table: "Products",
                type: "bytea",
                nullable: false,
                defaultValueSql: "'\\x0000000000000000'::bytea",
                oldClrType: typeof(byte[]),
                oldType: "bytea",
                oldRowVersion: true);

            migrationBuilder.AlterColumn<byte[]>(
                name: "RowVersion",
                table: "Categories",
                type: "bytea",
                nullable: false,
                defaultValueSql: "'\\x0000000000000000'::bytea",
                oldClrType: typeof(byte[]),
                oldType: "bytea",
                oldRowVersion: true);

            migrationBuilder.AlterColumn<byte[]>(
                name: "RowVersion",
                table: "Brands",
                type: "bytea",
                nullable: false,
                defaultValueSql: "'\\x0000000000000000'::bytea",
                oldClrType: typeof(byte[]),
                oldType: "bytea",
                oldRowVersion: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<byte[]>(
                name: "RowVersion",
                table: "Products",
                type: "bytea",
                rowVersion: true,
                nullable: false,
                oldClrType: typeof(byte[]),
                oldType: "bytea",
                oldDefaultValueSql: "'\\x0000000000000000'::bytea");

            migrationBuilder.AlterColumn<byte[]>(
                name: "RowVersion",
                table: "Categories",
                type: "bytea",
                rowVersion: true,
                nullable: false,
                oldClrType: typeof(byte[]),
                oldType: "bytea",
                oldDefaultValueSql: "'\\x0000000000000000'::bytea");

            migrationBuilder.AlterColumn<byte[]>(
                name: "RowVersion",
                table: "Brands",
                type: "bytea",
                rowVersion: true,
                nullable: false,
                oldClrType: typeof(byte[]),
                oldType: "bytea",
                oldDefaultValueSql: "'\\x0000000000000000'::bytea");
        }
    }
}
