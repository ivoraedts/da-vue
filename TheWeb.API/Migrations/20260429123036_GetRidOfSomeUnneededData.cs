using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TheWeb.API.Migrations
{
    /// <inheritdoc />
    public partial class GetRidOfSomeUnneededData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HomeId",
                table: "TadoRetrievedData");

            migrationBuilder.DropColumn(
                name: "ScheduleId",
                table: "TadoRetrievedData");

            migrationBuilder.DropColumn(
                name: "ZoneName",
                table: "TadoRetrievedData");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "HomeId",
                table: "TadoRetrievedData",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ScheduleId",
                table: "TadoRetrievedData",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "ZoneName",
                table: "TadoRetrievedData",
                type: "text",
                nullable: false,
                defaultValue: "");
        }
    }
}
