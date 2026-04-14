using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TheWeb.API.Migrations
{
    /// <inheritdoc />
    public partial class AddSomeInitialTodoData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "TodoItems",
                columns: new[] { "Id", "IsCompleted", "Task" },
                values: new object[,]
                {
                    { 1L, false, "Buy groceries" },
                    { 2L, true, "Walk the dog" },
                    { 3L, false, "Finish homework" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }
    }
}
