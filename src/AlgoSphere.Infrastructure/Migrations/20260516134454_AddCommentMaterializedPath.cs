using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlgoSphere.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCommentMaterializedPath : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "MaterializedPath",
                table: "Comments",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "ParentCommentId1",
                table: "Comments",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Comments_ParentCommentId1",
                table: "Comments",
                column: "ParentCommentId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Comments_Comments_ParentCommentId1",
                table: "Comments",
                column: "ParentCommentId1",
                principalTable: "Comments",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Comments_Comments_ParentCommentId1",
                table: "Comments");

            migrationBuilder.DropIndex(
                name: "IX_Comments_ParentCommentId1",
                table: "Comments");

            migrationBuilder.DropColumn(
                name: "MaterializedPath",
                table: "Comments");

            migrationBuilder.DropColumn(
                name: "ParentCommentId1",
                table: "Comments");
        }
    }
}
