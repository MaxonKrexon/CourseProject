using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FreshSight.Data.Migrations
{
    public partial class Postmodelchanges : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Adress",
                table: "Post",
                newName: "Topic");

            migrationBuilder.AddColumn<string>(
                name: "Category",
                table: "Post",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Category",
                table: "Post");

            migrationBuilder.RenameColumn(
                name: "Topic",
                table: "Post",
                newName: "Adress");
        }
    }
}
