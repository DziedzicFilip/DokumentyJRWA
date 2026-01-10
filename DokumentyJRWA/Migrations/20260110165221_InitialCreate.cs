using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DokumentyJRWA.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Dokumenty",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Tytul = table.Column<string>(type: "TEXT", nullable: false),
                    DataWplywu = table.Column<DateTime>(type: "TEXT", nullable: false),
                    JrwaCode = table.Column<string>(type: "TEXT", nullable: false),
                    Podmioty = table.Column<string>(type: "TEXT", nullable: false),
                    FilePath = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Dokumenty", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Dokumenty");
        }
    }
}
