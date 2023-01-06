using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FreshSight.Data.Migrations
{
    public partial class PostUserRatingIDadded : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UserGrades",
                columns: table => new
                {
                    ID = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    AuthorId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    PostID = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Grade = table.Column<double>(type: "float", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserGrades", x => x.ID);
                    table.ForeignKey(
                        name: "FK_UserGrades_AspNetUsers_AuthorId",
                        column: x => x.AuthorId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_UserGrades_Posts_PostID",
                        column: x => x.PostID,
                        principalTable: "Posts",
                        principalColumn: "ID");
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserGrades_AuthorId",
                table: "UserGrades",
                column: "AuthorId");

            migrationBuilder.CreateIndex(
                name: "IX_UserGrades_PostID",
                table: "UserGrades",
                column: "PostID");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserGrades");
        }
    }
}
