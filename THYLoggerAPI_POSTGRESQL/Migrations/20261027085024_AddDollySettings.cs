using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace THYLoggerAPI_POSTGRESQL.Migrations
{
    /// <inheritdoc />
    public partial class AddDollySettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Dolly.PanoNo daha once migration'a yazilmamisti; kolon varsa dokunma
            migrationBuilder.Sql("ALTER TABLE \"Dolly\" ADD COLUMN IF NOT EXISTS \"PanoNo\" text NULL;");

            migrationBuilder.CreateTable(
                name: "DollyConfigEvents",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DollyId = table.Column<int>(type: "integer", nullable: false),
                    EventType = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ConfigVersion = table.Column<int>(type: "integer", nullable: true),
                    Message = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DollyConfigEvents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DollyConfigEvents_Dolly_DollyId",
                        column: x => x.DollyId,
                        principalTable: "Dolly",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DollySettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DollyId = table.Column<int>(type: "integer", nullable: false),
                    ConfigVersion = table.Column<int>(type: "integer", nullable: false),
                    ConfigJson = table.Column<string>(type: "jsonb", nullable: false),
                    DeviceType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    AppVersion = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    AppliedConfigVersion = table.Column<int>(type: "integer", nullable: true),
                    AppliedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    LastSeenAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    FetchedConfigVersion = table.Column<int>(type: "integer", nullable: true),
                    FetchedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    LastError = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    LastErrorAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DollySettings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DollySettings_Dolly_DollyId",
                        column: x => x.DollyId,
                        principalTable: "Dolly",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DollyConfigEvents_CreatedAt",
                table: "DollyConfigEvents",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_DollyConfigEvents_DollyId_CreatedAt",
                table: "DollyConfigEvents",
                columns: new[] { "DollyId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_DollySettings_DollyId",
                table: "DollySettings",
                column: "DollyId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DollyConfigEvents");

            migrationBuilder.DropTable(
                name: "DollySettings");
        }
    }
}
