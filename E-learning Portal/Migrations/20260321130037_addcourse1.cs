using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace E_learning_Portal.Migrations
{
    /// <inheritdoc />
    public partial class addcourse1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "VideoOriginalName",
                table: "Courses",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "VideoOriginalName",
                table: "Courses");
        }
    }
}
