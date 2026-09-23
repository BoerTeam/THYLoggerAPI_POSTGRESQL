using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace THYLoggerAPI_POSTGRESQL.Migrations
{
    /// <inheritdoc />
    public partial class AddAuthorizationAndUserTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Permissions",
                columns: new[] { "Id", "Code", "CreatedAt", "Description", "IsActive", "Name" },
                values: new object[,]
                {
                    { 1, "DollyView", new DateTime(2026, 9, 23, 8, 52, 48, 44, DateTimeKind.Utc).AddTicks(161), null, true, "Dolly Görüntüleme" },
                    { 2, "DollyEdit", new DateTime(2026, 9, 23, 8, 52, 48, 44, DateTimeKind.Utc).AddTicks(1624), null, true, "Dolly Düzenleme" },
                    { 3, "ExportExcel", new DateTime(2026, 9, 23, 8, 52, 48, 44, DateTimeKind.Utc).AddTicks(1629), null, true, "Excel Çıktısı Alma" },
                    { 4, "UserManagement", new DateTime(2026, 9, 23, 8, 52, 48, 44, DateTimeKind.Utc).AddTicks(1630), null, true, "Kullanıcı Yönetimi" }
                });

            migrationBuilder.InsertData(
                table: "Roles",
                columns: new[] { "Id", "CreatedAt", "Description", "IsActive", "IsSystemRole", "Name", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, new DateTime(2026, 9, 23, 8, 52, 48, 43, DateTimeKind.Utc).AddTicks(536), null, true, false, "Admin", null },
                    { 2, new DateTime(2026, 9, 23, 8, 52, 48, 43, DateTimeKind.Utc).AddTicks(2162), null, true, false, "User", null },
                    { 3, new DateTime(2026, 9, 23, 8, 52, 48, 43, DateTimeKind.Utc).AddTicks(2168), null, true, false, "SahaOperatoru", null }
                });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "CreatedAt", "Email", "FirstName", "IsActive", "LastLoginAt", "LastName", "PasswordHash", "UpdatedAt", "UserName" },
                values: new object[] { 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "admin@boer.com.tr", "", true, null, "", "SYSTEM_DEFAULT_HASH", null, "admin" });

            migrationBuilder.InsertData(
                table: "RolePermissions",
                columns: new[] { "PermissionId", "RoleId" },
                values: new object[,]
                {
                    { 1, 1 },
                    { 2, 1 },
                    { 3, 1 },
                    { 4, 1 },
                    { 1, 2 }
                });

            migrationBuilder.InsertData(
                table: "UserRoles",
                columns: new[] { "RoleId", "UserId" },
                values: new object[] { 1, 1 });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 1, 1 });

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 2, 1 });

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 3, 1 });

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 4, 1 });

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 1, 2 });

            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "UserRoles",
                keyColumns: new[] { "RoleId", "UserId" },
                keyValues: new object[] { 1, 1 });

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1);
        }
    }
}
