using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Vendix.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixNullRowVersions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Fix null or default RowVersion values in Products table
            // Generate a unique RowVersion using UUID converted to bytea (16 bytes) + another UUID (16 bytes) = 32 bytes
            migrationBuilder.Sql(@"
                UPDATE ""Products""
                SET ""RowVersion"" = 
                    decode(regexp_replace(gen_random_uuid()::text, '-', '', 'g'), 'hex') || 
                    decode(regexp_replace(gen_random_uuid()::text, '-', '', 'g'), 'hex')
                WHERE ""RowVersion"" IS NULL 
                   OR ""RowVersion"" = decode('0000000000000000', 'hex');
            ");

            // Fix null or default RowVersion values in Categories table
            migrationBuilder.Sql(@"
                UPDATE ""Categories""
                SET ""RowVersion"" = 
                    decode(regexp_replace(gen_random_uuid()::text, '-', '', 'g'), 'hex') || 
                    decode(regexp_replace(gen_random_uuid()::text, '-', '', 'g'), 'hex')
                WHERE ""RowVersion"" IS NULL 
                   OR ""RowVersion"" = decode('0000000000000000', 'hex');
            ");

            // Fix null or default RowVersion values in Brands table
            migrationBuilder.Sql(@"
                UPDATE ""Brands""
                SET ""RowVersion"" = 
                    decode(regexp_replace(gen_random_uuid()::text, '-', '', 'g'), 'hex') || 
                    decode(regexp_replace(gen_random_uuid()::text, '-', '', 'g'), 'hex')
                WHERE ""RowVersion"" IS NULL 
                   OR ""RowVersion"" = decode('0000000000000000', 'hex');
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // No rollback needed - this is a data fix migration
        }
    }
}
