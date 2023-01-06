using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FreshSight.Data.Migrations
{
    public partial class PostUserRatingchanges : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UserRating",
                table: "Posts");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "UserRating",
                table: "Posts",
                type: "float",
                nullable: true);
        }
    }
}
