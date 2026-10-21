using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace THYLoggerAPI_POSTGRESQL.Migrations
{
    /// <inheritdoc />
    public partial class AddDollyRequests : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DollyRequests",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ReferenceNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    OperationType = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    AwbNumber = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    FlightNumber = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    FlightDate = table.Column<DateOnly>(type: "date", nullable: false),
                    DepartureStation = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    ArrivalStation = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    SccCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    UldNumber = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    DollyId = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    TerminatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DollyRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DollyRequests_Dolly_DollyId",
                        column: x => x.DollyId,
                        principalTable: "Dolly",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 10, 21, 8, 7, 7, 598, DateTimeKind.Utc).AddTicks(5426));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 10, 21, 8, 7, 7, 598, DateTimeKind.Utc).AddTicks(6666));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 10, 21, 8, 7, 7, 598, DateTimeKind.Utc).AddTicks(6671));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 10, 21, 8, 7, 7, 598, DateTimeKind.Utc).AddTicks(6672));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 10, 21, 8, 7, 7, 597, DateTimeKind.Utc).AddTicks(7404));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 10, 21, 8, 7, 7, 597, DateTimeKind.Utc).AddTicks(8764));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 10, 21, 8, 7, 7, 597, DateTimeKind.Utc).AddTicks(8769));

            migrationBuilder.CreateIndex(
                name: "IX_DollyRequests_AwbNumber",
                table: "DollyRequests",
                column: "AwbNumber");

            migrationBuilder.CreateIndex(
                name: "IX_DollyRequests_FlightNumber_FlightDate",
                table: "DollyRequests",
                columns: new[] { "FlightNumber", "FlightDate" });

            migrationBuilder.CreateIndex(
                name: "IX_DollyRequests_ReferenceNumber",
                table: "DollyRequests",
                column: "ReferenceNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DollyRequests_UldNumber",
                table: "DollyRequests",
                column: "UldNumber");

            migrationBuilder.CreateIndex(
                name: "UX_DollyRequests_OpenPerDolly",
                table: "DollyRequests",
                column: "DollyId",
                unique: true,
                filter: "\"Status\" = 'Open'");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DollyRequests");

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 9, 23, 8, 52, 48, 44, DateTimeKind.Utc).AddTicks(161));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 9, 23, 8, 52, 48, 44, DateTimeKind.Utc).AddTicks(1624));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 9, 23, 8, 52, 48, 44, DateTimeKind.Utc).AddTicks(1629));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 9, 23, 8, 52, 48, 44, DateTimeKind.Utc).AddTicks(1630));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 9, 23, 8, 52, 48, 43, DateTimeKind.Utc).AddTicks(536));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 9, 23, 8, 52, 48, 43, DateTimeKind.Utc).AddTicks(2162));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 9, 23, 8, 52, 48, 43, DateTimeKind.Utc).AddTicks(2168));
        }
    }
}
