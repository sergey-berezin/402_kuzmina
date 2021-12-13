using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace UIRecognition.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Recognitions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Image = table.Column<byte[]>(type: "BLOB", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Recognitions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RecognizedObject",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    X1 = table.Column<float>(type: "REAL", nullable: false),
                    Y1 = table.Column<float>(type: "REAL", nullable: false),
                    X2 = table.Column<float>(type: "REAL", nullable: false),
                    Y2 = table.Column<float>(type: "REAL", nullable: false),
                    Class = table.Column<string>(type: "TEXT", nullable: true),
                    RecognizedImageId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RecognizedObject", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RecognizedObject_Recognitions_RecognizedImageId",
                        column: x => x.RecognizedImageId,
                        principalTable: "Recognitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RecognizedObject_RecognizedImageId",
                table: "RecognizedObject",
                column: "RecognizedImageId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RecognizedObject");

            migrationBuilder.DropTable(
                name: "Recognitions");
        }
    }
}
