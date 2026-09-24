using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CampusLostAndFound.Migrations
{
    /// <inheritdoc />
    public partial class AddCampusAreaToFoundItemReports : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CampusArea",
                table: "FoundItemReports",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CampusArea",
                table: "FoundItemReports");
        }
    }
}
