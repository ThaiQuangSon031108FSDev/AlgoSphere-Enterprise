using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace AlgoSphere.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Phase3And4 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TournamentParticipant_Tournaments_TournamentId",
                table: "TournamentParticipant");

            migrationBuilder.DropForeignKey(
                name: "FK_TournamentParticipant_Users_UserId",
                table: "TournamentParticipant");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TournamentParticipant",
                table: "TournamentParticipant");

            migrationBuilder.RenameTable(
                name: "TournamentParticipant",
                newName: "TournamentParticipants");

            migrationBuilder.RenameIndex(
                name: "IX_TournamentParticipant_UserId",
                table: "TournamentParticipants",
                newName: "IX_TournamentParticipants_UserId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TournamentParticipants",
                table: "TournamentParticipants",
                columns: new[] { "TournamentId", "UserId" });

            migrationBuilder.InsertData(
                table: "Roles",
                columns: new[] { "Id", "CreatedAt", "RoleName", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Admin", null },
                    { 2, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Student", null }
                });

            migrationBuilder.AddForeignKey(
                name: "FK_TournamentParticipants_Tournaments_TournamentId",
                table: "TournamentParticipants",
                column: "TournamentId",
                principalTable: "Tournaments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TournamentParticipants_Users_UserId",
                table: "TournamentParticipants",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TournamentParticipants_Tournaments_TournamentId",
                table: "TournamentParticipants");

            migrationBuilder.DropForeignKey(
                name: "FK_TournamentParticipants_Users_UserId",
                table: "TournamentParticipants");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TournamentParticipants",
                table: "TournamentParticipants");

            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.RenameTable(
                name: "TournamentParticipants",
                newName: "TournamentParticipant");

            migrationBuilder.RenameIndex(
                name: "IX_TournamentParticipants_UserId",
                table: "TournamentParticipant",
                newName: "IX_TournamentParticipant_UserId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TournamentParticipant",
                table: "TournamentParticipant",
                columns: new[] { "TournamentId", "UserId" });

            migrationBuilder.AddForeignKey(
                name: "FK_TournamentParticipant_Tournaments_TournamentId",
                table: "TournamentParticipant",
                column: "TournamentId",
                principalTable: "Tournaments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TournamentParticipant_Users_UserId",
                table: "TournamentParticipant",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
