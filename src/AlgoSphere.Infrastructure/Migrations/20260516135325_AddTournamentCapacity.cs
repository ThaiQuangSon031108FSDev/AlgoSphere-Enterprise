using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlgoSphere.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTournamentCapacity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MaxParticipants",
                table: "Tournaments",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "MinParticipants",
                table: "Tournaments",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MaxParticipants",
                table: "Tournaments");

            migrationBuilder.DropColumn(
                name: "MinParticipants",
                table: "Tournaments");
        }
    }
}
