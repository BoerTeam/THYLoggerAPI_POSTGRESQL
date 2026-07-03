using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace THYLoggerAPI_POSTGRESQL.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialPersistedGrantDbMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BosDolu",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Time = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    SensorDegeri = table.Column<bool>(type: "boolean", nullable: true),
                    LoggerId = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BosDolu", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Gpsdatum",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: true),
                    Latitude = table.Column<float>(type: "real", nullable: true),
                    Longitude = table.Column<float>(type: "real", nullable: true),
                    Time = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Altitude = table.Column<float>(type: "real", nullable: true),
                    SpeedKnots = table.Column<float>(type: "real", nullable: true),
                    SpeedMph = table.Column<float>(type: "real", nullable: true),
                    SpeedKmh = table.Column<float>(type: "real", nullable: true),
                    Course = table.Column<string>(type: "text", nullable: true),
                    Fix = table.Column<int>(type: "integer", nullable: true),
                    FixAsString = table.Column<string>(type: "text", nullable: true),
                    NumberOfSatellites = table.Column<int>(type: "integer", nullable: true),
                    GpsFixAvailable = table.Column<bool>(type: "boolean", nullable: true),
                    Hdop = table.Column<float>(type: "real", nullable: true),
                    QualityType = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Gpsdatum", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Nem",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Time = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Nem1 = table.Column<float>(type: "real", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Nem", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Sicaklik",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Time = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Sicaklik1 = table.Column<float>(type: "real", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sicaklik", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BosDolu");

            migrationBuilder.DropTable(
                name: "Gpsdatum");

            migrationBuilder.DropTable(
                name: "Nem");

            migrationBuilder.DropTable(
                name: "Sicaklik");
        }
    }
}
