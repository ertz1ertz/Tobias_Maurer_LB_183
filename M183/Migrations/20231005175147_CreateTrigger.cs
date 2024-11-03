using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace M183.Migrations
{
    /// <inheritdoc />
    public partial class CreateTrigger : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
               name: "NewsAudit",
               columns: table => new
               {
                   Id = table.Column<int>(type: "int", nullable: false)
                       .Annotation("SqlServer:Identity", "1, 1"),
                   NewsId = table.Column<int>(type: "int", nullable: false),
                   Action = table.Column<string>(type: "nvarchar(max)", nullable: false),
                   AuthorId = table.Column<int>(type: "int", nullable: false)
               },
               constraints: table =>
               {
                   table.PrimaryKey("PK_NewsAudit", x => x.Id);
               });

            migrationBuilder.Sql(@"CREATE TRIGGER news_insert ON dbo.News
                AFTER INSERT
                AS DECLARE
                  @NewsId INT,
                  @AuthorId INT;
                SELECT @NewsId = ins.ID FROM INSERTED ins;
                SELECT @AuthorId = ins.AuthorId FROM INSERTED ins;

                INSERT INTO NewsAudit (NewsId, Action, AuthorId) VALUES (@NewsId, 'Create', @AuthorId);");

            migrationBuilder.Sql(@"CREATE TRIGGER news_update ON dbo.News
                AFTER UPDATE
                AS DECLARE
                  @NewsId INT,
                  @AuthorId INT;
                SELECT @NewsId = ins.ID FROM INSERTED ins;
                SELECT @AuthorId = ins.AuthorId FROM INSERTED ins;

                INSERT INTO NewsAudit (NewsId, Action, AuthorId) VALUES (@NewsId, 'Update', @AuthorId);");


            migrationBuilder.Sql(@"CREATE TRIGGER news_delete ON dbo.News
                AFTER DELETE
                AS DECLARE
                  @NewsId INT,
                  @AuthorId INT;
                SELECT @NewsId = del.ID FROM DELETED del;
                SELECT @AuthorId = del.AuthorId FROM DELETED del;

                INSERT INTO NewsAudit (NewsId, Action, AuthorId) VALUES (@NewsId, 'Delete', @AuthorId);");

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "NewsAudit");
            migrationBuilder.Sql("DROP TRIGGER IF EXISTS news_insert");
            migrationBuilder.Sql("DROP TRIGGER IF EXISTS news_update");
            migrationBuilder.Sql("DROP TRIGGER IF EXISTS news_delete");
        }
    }
}
