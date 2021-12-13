using Microsoft.EntityFrameworkCore.Migrations;

namespace UIRecognition.Migrations
{
    public partial class TwoEntities : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RecognizedObject_Recognitions_RecognizedImageId",
                table: "RecognizedObject");

            migrationBuilder.DropPrimaryKey(
                name: "PK_RecognizedObject",
                table: "RecognizedObject");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Recognitions",
                table: "Recognitions");

            migrationBuilder.RenameTable(
                name: "RecognizedObject",
                newName: "RecognizedObjects");

            migrationBuilder.RenameTable(
                name: "Recognitions",
                newName: "RecognizedImages");

            migrationBuilder.RenameIndex(
                name: "IX_RecognizedObject_RecognizedImageId",
                table: "RecognizedObjects",
                newName: "IX_RecognizedObjects_RecognizedImageId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_RecognizedObjects",
                table: "RecognizedObjects",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_RecognizedImages",
                table: "RecognizedImages",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_RecognizedObjects_RecognizedImages_RecognizedImageId",
                table: "RecognizedObjects",
                column: "RecognizedImageId",
                principalTable: "RecognizedImages",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RecognizedObjects_RecognizedImages_RecognizedImageId",
                table: "RecognizedObjects");

            migrationBuilder.DropPrimaryKey(
                name: "PK_RecognizedObjects",
                table: "RecognizedObjects");

            migrationBuilder.DropPrimaryKey(
                name: "PK_RecognizedImages",
                table: "RecognizedImages");

            migrationBuilder.RenameTable(
                name: "RecognizedObjects",
                newName: "RecognizedObject");

            migrationBuilder.RenameTable(
                name: "RecognizedImages",
                newName: "Recognitions");

            migrationBuilder.RenameIndex(
                name: "IX_RecognizedObjects_RecognizedImageId",
                table: "RecognizedObject",
                newName: "IX_RecognizedObject_RecognizedImageId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_RecognizedObject",
                table: "RecognizedObject",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Recognitions",
                table: "Recognitions",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_RecognizedObject_Recognitions_RecognizedImageId",
                table: "RecognizedObject",
                column: "RecognizedImageId",
                principalTable: "Recognitions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
