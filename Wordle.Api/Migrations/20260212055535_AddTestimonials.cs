using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Wordle.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddTestimonials : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Testimonials",
                columns: table => new
                {
                    TestimonialId = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Author = table.Column<string>(nullable: false),
                    Content = table.Column<string>(nullable: false),
                    Rating = table.Column<int>(nullable: false),
                    CreatedAt = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Testimonials", x => x.TestimonialId);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Testimonials");
        }
    }
}
