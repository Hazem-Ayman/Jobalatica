using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Jobalatica.Data.Migrations
{
    /// <inheritdoc />
    public partial class RemoveProfilePicture : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ProfilePicturePath",
                table: "AspNetUsers");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ProfilePicturePath",
                table: "AspNetUsers",
                type: "TEXT",
                nullable: true);
        }
    }
}
