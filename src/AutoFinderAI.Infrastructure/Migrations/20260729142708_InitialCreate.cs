using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutoFinderAI.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ChatSessions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    UserId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Title = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    LastMessageAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChatSessions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CrawlRuns",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    SourceKey = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    Category = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false),
                    StartedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    FinishedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Status = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false),
                    ItemsFound = table.Column<int>(type: "INTEGER", nullable: false),
                    ItemsSaved = table.Column<int>(type: "INTEGER", nullable: false),
                    Error = table.Column<string>(type: "TEXT", maxLength: 2048, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CrawlRuns", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Email = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    PasswordHash = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Vehicles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    SourceKey = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    ExternalId = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    Url = table.Column<string>(type: "TEXT", maxLength: 1024, nullable: false),
                    Title = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    Price_Amount = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    Price_Currency = table.Column<string>(type: "TEXT", maxLength: 3, nullable: false),
                    Make = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    Model = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    Version = table.Column<string>(type: "TEXT", maxLength: 128, nullable: true),
                    ProductionYear = table.Column<int>(type: "INTEGER", nullable: false),
                    Mileage = table.Column<int>(type: "INTEGER", nullable: true),
                    FuelType = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false),
                    Transmission = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false),
                    EnginePowerHp = table.Column<int>(type: "INTEGER", nullable: true),
                    EngineCapacityCm3 = table.Column<int>(type: "INTEGER", nullable: true),
                    Location = table.Column<string>(type: "TEXT", maxLength: 128, nullable: true),
                    ThumbnailUrl = table.Column<string>(type: "TEXT", maxLength: 1024, nullable: true),
                    PublishedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ScrapedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    VehicleType = table.Column<string>(type: "TEXT", maxLength: 8, nullable: false),
                    BodyType = table.Column<string>(type: "TEXT", maxLength: 32, nullable: true),
                    Doors = table.Column<int>(type: "INTEGER", nullable: true),
                    Seats = table.Column<int>(type: "INTEGER", nullable: true),
                    DriveType = table.Column<string>(type: "TEXT", maxLength: 32, nullable: true),
                    Color = table.Column<string>(type: "TEXT", maxLength: 64, nullable: true),
                    IsDamaged = table.Column<bool>(type: "INTEGER", nullable: true),
                    IsFirstOwner = table.Column<bool>(type: "INTEGER", nullable: true),
                    CountryOfOrigin = table.Column<string>(type: "TEXT", maxLength: 64, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Vehicles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ChatMessages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    SessionId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Role = table.Column<string>(type: "TEXT", maxLength: 16, nullable: false),
                    Content = table.Column<string>(type: "TEXT", nullable: false),
                    CriteriaJson = table.Column<string>(type: "TEXT", nullable: true),
                    ResultVehicleIdsJson = table.Column<string>(type: "TEXT", nullable: true),
                    ModelUsed = table.Column<string>(type: "TEXT", maxLength: 128, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChatMessages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChatMessages_ChatSessions_SessionId",
                        column: x => x.SessionId,
                        principalTable: "ChatSessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ChatMessages_SessionId",
                table: "ChatMessages",
                column: "SessionId");

            migrationBuilder.CreateIndex(
                name: "IX_ChatSessions_UserId",
                table: "ChatSessions",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_CrawlRuns_StartedAt",
                table: "CrawlRuns",
                column: "StartedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Vehicles_Make",
                table: "Vehicles",
                column: "Make");

            migrationBuilder.CreateIndex(
                name: "IX_Vehicles_Price_Amount",
                table: "Vehicles",
                column: "Price_Amount");

            migrationBuilder.CreateIndex(
                name: "IX_Vehicles_ProductionYear",
                table: "Vehicles",
                column: "ProductionYear");

            migrationBuilder.CreateIndex(
                name: "IX_Vehicles_PublishedAt",
                table: "Vehicles",
                column: "PublishedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Vehicles_SourceKey_ExternalId",
                table: "Vehicles",
                columns: new[] { "SourceKey", "ExternalId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ChatMessages");

            migrationBuilder.DropTable(
                name: "CrawlRuns");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Vehicles");

            migrationBuilder.DropTable(
                name: "ChatSessions");
        }
    }
}
