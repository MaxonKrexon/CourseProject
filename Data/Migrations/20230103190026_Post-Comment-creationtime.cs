using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FreshSight.Data.Migrations
{
    public partial class PostCommentcreationtime : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CreationTime",
                table: "Posts",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreationTime",
                table: "Comments",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreationTime",
                table: "Posts");

            migrationBuilder.DropColumn(
                name: "CreationTime",
                table: "Comments");
        }
    }
}
