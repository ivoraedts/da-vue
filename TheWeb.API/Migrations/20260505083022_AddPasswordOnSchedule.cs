using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TheWeb.API.Migrations
{
    /// <inheritdoc />
    public partial class AddPasswordOnSchedule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Password",
                table: "TadoRetrievalSchedules",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Password",
                table: "TadoRetrievalSchedules");
        }
    }
}
