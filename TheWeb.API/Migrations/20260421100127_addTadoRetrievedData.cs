using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace TheWeb.API.Migrations
{
    /// <inheritdoc />
    public partial class addTadoRetrievedData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TadoRetrievalSchedules",
                columns: table => new
                {
                    ScheduleId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TokenId = table.Column<int>(type: "integer", nullable: false),
                    Interval = table.Column<int>(type: "integer", nullable: false),
                    NextRetrievalTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastRetrievalTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    HomeId = table.Column<int>(type: "integer", nullable: false),
                    ZoneName = table.Column<string>(type: "text", nullable: false),
                    LastError = table.Column<string>(type: "text", nullable: false),
                    ConsecutiveFailures = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TadoRetrievalSchedules", x => x.ScheduleId);
                });

            migrationBuilder.CreateTable(
                name: "TadoRetrievedData",
                columns: table => new
                {
                    RetrievalId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ScheduleId = table.Column<int>(type: "integer", nullable: false),
                    HomeId = table.Column<int>(type: "integer", nullable: false),
                    ZoneName = table.Column<string>(type: "text", nullable: false),
                    InsideTemperatureCelsius = table.Column<double>(type: "double precision", nullable: false),
                    HumidityPercentage = table.Column<double>(type: "double precision", nullable: false),
                    RetrievedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TadoRetrievedData", x => x.RetrievalId);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TadoRetrievalSchedules");

            migrationBuilder.DropTable(
                name: "TadoRetrievedData");
        }
    }
}
