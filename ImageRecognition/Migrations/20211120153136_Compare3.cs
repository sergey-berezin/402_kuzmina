using Microsoft.EntityFrameworkCore.Migrations;

namespace UIRecognition.Migrations
{
    public partial class Compare3 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Hash",
                table: "Recognitions",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Hash",
                table: "Recognitions");
        }
    }
}
