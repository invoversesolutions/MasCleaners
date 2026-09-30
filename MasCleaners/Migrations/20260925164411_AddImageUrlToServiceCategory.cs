using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MasCleaners.Migrations
{
    /// <inheritdoc />
    public partial class AddImageUrlToServiceCategory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ImageUrl",
                table: "ServiceCategories",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImageUrl",
                table: "ServiceCategories");
        }
    }
}
