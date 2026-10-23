using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OnTrack.Migrations
{
    /// <inheritdoc />
    public partial class AddTravelClassToTicket : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TravelClass",
                table: "Tickets",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "Economy");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TravelClass",
                table: "Tickets");
        }
    }
}
