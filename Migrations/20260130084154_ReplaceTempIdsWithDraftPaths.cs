using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace student_management_system.Migrations
{
    /// <inheritdoc />
    public partial class ReplaceTempIdsWithDraftPaths : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "TempImageId", table: "RegistrationDrafts");
            migrationBuilder.DropColumn(name: "TempVideoId", table: "RegistrationDrafts");
            migrationBuilder.AddColumn<string>(
                name: "ProfileImagePath",
                table: "RegistrationDrafts",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);
            migrationBuilder.AddColumn<string>(
                name: "ProfileVideoPath",
                table: "RegistrationDrafts",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "ProfileImagePath", table: "RegistrationDrafts");
            migrationBuilder.DropColumn(name: "ProfileVideoPath", table: "RegistrationDrafts");
            migrationBuilder.AddColumn<string>(
                name: "TempImageId",
                table: "RegistrationDrafts",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);
            migrationBuilder.AddColumn<string>(
                name: "TempVideoId",
                table: "RegistrationDrafts",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);
        }
    }
}
