using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Turnos.Migrations
{
    /// <inheritdoc />
    public partial class SeedRoles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                schema: "turnos",
                table: "Roles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { new Guid("0196db06-2330-7fb8-84f1-2d6bc594269e"), null, "Alumno", "ALUMNO" },
                    { new Guid("0196db06-2334-7969-9f83-0e1b575a7878"), null, "Personal", "PERSONAL" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                schema: "turnos",
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("0196db06-2330-7fb8-84f1-2d6bc594269e"));

            migrationBuilder.DeleteData(
                schema: "turnos",
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("0196db06-2334-7969-9f83-0e1b575a7878"));
        }
    }
}
