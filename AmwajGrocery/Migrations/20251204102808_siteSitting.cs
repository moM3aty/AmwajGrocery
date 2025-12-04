using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AmwajGrocery.Migrations
{
    /// <inheritdoc />
    public partial class siteSitting : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SiteSettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BannerTextAr = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BannerTextEn = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SiteSettings", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "SiteSettings",
                columns: new[] { "Id", "BannerTextAr", "BannerTextEn" },
                values: new object[] { 1, "✨ عروض موسمية وتخفيضات خاصة بانتظارك! تسوق الآن ووفر المزيد. ✨", "✨ Seasonal offers and special discounts await! Shop now and save more. ✨" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SiteSettings");
        }
    }
}
