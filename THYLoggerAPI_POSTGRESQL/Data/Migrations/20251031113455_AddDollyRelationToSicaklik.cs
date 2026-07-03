using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace THYLoggerAPI_POSTGRESQL.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddDollyRelationToSicaklik : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DollyId",
                table: "Sicaklik",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "DollyId",
                table: "Nem",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "DollyId",
                table: "Gpsdatum",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "DollyId",
                table: "BosDolu",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "Dolly",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SerialNumber = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Dolly", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Sicaklik_DollyId",
                table: "Sicaklik",
                column: "DollyId");

            migrationBuilder.CreateIndex(
                name: "IX_Nem_DollyId",
                table: "Nem",
                column: "DollyId");

            migrationBuilder.CreateIndex(
                name: "IX_Gpsdatum_DollyId",
                table: "Gpsdatum",
                column: "DollyId");

            migrationBuilder.CreateIndex(
                name: "IX_BosDolu_DollyId",
                table: "BosDolu",
                column: "DollyId");

            migrationBuilder.AddForeignKey(
                name: "FK_BosDolu_Dolly_DollyId",
                table: "BosDolu",
                column: "DollyId",
                principalTable: "Dolly",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Gpsdatum_Dolly_DollyId",
                table: "Gpsdatum",
                column: "DollyId",
                principalTable: "Dolly",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Nem_Dolly_DollyId",
                table: "Nem",
                column: "DollyId",
                principalTable: "Dolly",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Sicaklik_Dolly_DollyId",
                table: "Sicaklik",
                column: "DollyId",
                principalTable: "Dolly",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BosDolu_Dolly_DollyId",
                table: "BosDolu");

            migrationBuilder.DropForeignKey(
                name: "FK_Gpsdatum_Dolly_DollyId",
                table: "Gpsdatum");

            migrationBuilder.DropForeignKey(
                name: "FK_Nem_Dolly_DollyId",
                table: "Nem");

            migrationBuilder.DropForeignKey(
                name: "FK_Sicaklik_Dolly_DollyId",
                table: "Sicaklik");

            migrationBuilder.DropTable(
                name: "Dolly");

            migrationBuilder.DropIndex(
                name: "IX_Sicaklik_DollyId",
                table: "Sicaklik");

            migrationBuilder.DropIndex(
                name: "IX_Nem_DollyId",
                table: "Nem");

            migrationBuilder.DropIndex(
                name: "IX_Gpsdatum_DollyId",
                table: "Gpsdatum");

            migrationBuilder.DropIndex(
                name: "IX_BosDolu_DollyId",
                table: "BosDolu");

            migrationBuilder.DropColumn(
                name: "DollyId",
                table: "Sicaklik");

            migrationBuilder.DropColumn(
                name: "DollyId",
                table: "Nem");

            migrationBuilder.DropColumn(
                name: "DollyId",
                table: "Gpsdatum");

            migrationBuilder.DropColumn(
                name: "DollyId",
                table: "BosDolu");
        }
    }
}
