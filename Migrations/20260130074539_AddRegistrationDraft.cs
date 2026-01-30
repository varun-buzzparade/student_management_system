using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace student_management_system.Migrations
{
    /// <inheritdoc />
    public partial class AddRegistrationDraft : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RegistrationDrafts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FullName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    DateOfBirth = table.Column<DateOnly>(type: "date", nullable: true),
                    HeightCm = table.Column<decimal>(type: "numeric", nullable: true),
                    Gender = table.Column<int>(type: "integer", nullable: true),
                    MobileNumber = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    TempImageId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    TempVideoId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastUpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RegistrationDrafts", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RegistrationDrafts_CreatedAt",
                table: "RegistrationDrafts",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_RegistrationDrafts_LastUpdatedAt",
                table: "RegistrationDrafts",
                column: "LastUpdatedAt");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RegistrationDrafts");
        }
    }
}
