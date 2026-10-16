using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OnTrack.Migrations
{
    /// <inheritdoc />
    public partial class Add_Capacity_Property_To_Train_Entity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Capacity",
                table: "Trains",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Capacity",
                table: "Trains");
        }
    }
}
