using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InfoCollectorAPI.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Messages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    GroupOrUserName = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    MessageContent = table.Column<string>(type: "TEXT", nullable: false),
                    ReceivedDateTime = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Messages", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Messages_GroupOrUserName",
                table: "Messages",
                column: "GroupOrUserName");

            migrationBuilder.CreateIndex(
                name: "IX_Messages_ReceivedDateTime",
                table: "Messages",
                column: "ReceivedDateTime");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Messages");
        }
    }
}
