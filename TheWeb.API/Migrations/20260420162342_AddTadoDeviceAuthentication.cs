using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace TheWeb.API.Migrations
{
    /// <inheritdoc />
    public partial class AddTadoDeviceAuthentication : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TadoDeviceAuthentications",
                columns: table => new
                {
                    CommunicationId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Creation = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DeviceCode = table.Column<string>(type: "text", nullable: false),
                    ExpiresIn = table.Column<short>(type: "smallint", nullable: false),
                    Interval = table.Column<short>(type: "smallint", nullable: false),
                    UserCode = table.Column<string>(type: "text", nullable: false),
                    VerificationUri = table.Column<string>(type: "text", nullable: false),
                    VerificationUriComplete = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TadoDeviceAuthentications", x => x.CommunicationId);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TadoDeviceAuthentications");
        }
    }
}
