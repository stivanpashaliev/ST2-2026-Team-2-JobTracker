using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ST2_2026_Team_2_JobTracker.Migrations
{
    /// <inheritdoc />
    public partial class InitialCommit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Companies",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Website = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Location = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Companies", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "JobOffers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompanyId = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", maxLength: 10000, nullable: false),
                    Location = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    Salary = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: true),
                    RemoteType = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JobOffers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_JobOffers_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Applications",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    JobOfferId = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    AppliedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Applications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Applications_JobOffers_JobOfferId",
                        column: x => x.JobOfferId,
                        principalTable: "JobOffers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "Companies",
                columns: new[] { "Id", "Location", "Name", "Website" },
                values: new object[,]
                {
                    { 1, "Sofia, Bulgaria", "TechNova Solutions", "https://example.com" },
                    { 2, "Plovdiv, Bulgaria", "CodeBridge", "https://example.org" },
                    { 3, "Remote", "Digital Horizon", "https://example.net" }
                });

            migrationBuilder.InsertData(
                table: "JobOffers",
                columns: new[] { "Id", "CompanyId", "CreatedAt", "Description", "Location", "RemoteType", "Salary", "Title" },
                values: new object[,]
                {
                    { 1, 1, new DateTime(2026, 9, 20, 10, 0, 0, 0, DateTimeKind.Utc), "Develop ASP.NET Core applications using C#, SQL Server and Entity Framework Core.", "Sofia, Bulgaria", 1, 2500m, "Junior .NET Developer" },
                    { 2, 2, new DateTime(2026, 9, 25, 9, 0, 0, 0, DateTimeKind.Utc), "Build backend services using Python, Django, REST APIs and PostgreSQL.", "Plovdiv, Bulgaria", 0, 2300m, "Junior Python Developer" },
                    { 3, 3, new DateTime(2026, 9, 28, 14, 0, 0, 0, DateTimeKind.Utc), "Develop modern web interfaces using React, TypeScript, HTML and CSS.", "Remote", 2, 3200m, "Frontend React Developer" }
                });

            migrationBuilder.InsertData(
                table: "Applications",
                columns: new[] { "Id", "AppliedAt", "JobOfferId", "Notes", "Status" },
                values: new object[,]
                {
                    { 1, new DateTime(2026, 10, 1, 9, 0, 0, 0, DateTimeKind.Utc), 1, "Applied through the company website.", 1 },
                    { 2, new DateTime(2026, 10, 3, 11, 30, 0, 0, DateTimeKind.Utc), 2, "Technical interview scheduled.", 2 },
                    { 3, null, 3, "Review the requirements before applying.", 0 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Applications_JobOfferId",
                table: "Applications",
                column: "JobOfferId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_JobOffers_CompanyId",
                table: "JobOffers",
                column: "CompanyId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Applications");

            migrationBuilder.DropTable(
                name: "JobOffers");

            migrationBuilder.DropTable(
                name: "Companies");
        }
    }
}
