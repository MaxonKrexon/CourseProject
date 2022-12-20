using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FreshSight.Data.Migrations
{
    public partial class Postmodelratingseparation : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Rating",
                table: "Posts",
                newName: "UserRating");

            migrationBuilder.AddColumn<double>(
                name: "AuthorGrade",
                table: "Posts",
                type: "float",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AuthorGrade",
                table: "Posts");

            migrationBuilder.RenameColumn(
                name: "UserRating",
                table: "Posts",
                newName: "Rating");
        }
    }
}
