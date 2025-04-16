using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Turnos.Common;

#nullable disable

namespace Turnos.Migrations
{
    /// <inheritdoc />
    public partial class Add_Fila_Turno : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:turnos.estado_fila", "abierta,cerrada")
                .Annotation("Npgsql:Enum:turnos.estado_turno", "atendido,atendiendo,cancelado,pendiente");

            migrationBuilder.CreateTable(
                name: "Filas",
                schema: "turnos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PersonalId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: false),
                    Estado = table.Column<EstadoFila>(type: "turnos.estado_fila", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Filas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Filas_Usuarios_PersonalId",
                        column: x => x.PersonalId,
                        principalSchema: "turnos",
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Turnos",
                schema: "turnos",
                columns: table => new
                {
                    FilaId = table.Column<Guid>(type: "uuid", nullable: false),
                    AlumnoId = table.Column<Guid>(type: "uuid", nullable: false),
                    Lugar = table.Column<long>(type: "bigint", nullable: false),
                    Estado = table.Column<EstadoTurno>(type: "turnos.estado_turno", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Turnos", x => new { x.AlumnoId, x.FilaId, x.Lugar });
                    table.ForeignKey(
                        name: "FK_Turnos_Filas_FilaId",
                        column: x => x.FilaId,
                        principalSchema: "turnos",
                        principalTable: "Filas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Turnos_Usuarios_AlumnoId",
                        column: x => x.AlumnoId,
                        principalSchema: "turnos",
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Filas_PersonalId",
                schema: "turnos",
                table: "Filas",
                column: "PersonalId");

            migrationBuilder.CreateIndex(
                name: "IX_Turnos_FilaId",
                schema: "turnos",
                table: "Turnos",
                column: "FilaId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Turnos",
                schema: "turnos");

            migrationBuilder.DropTable(
                name: "Filas",
                schema: "turnos");

            migrationBuilder.AlterDatabase()
                .OldAnnotation("Npgsql:Enum:turnos.estado_fila", "abierta,cerrada")
                .OldAnnotation("Npgsql:Enum:turnos.estado_turno", "atendido,atendiendo,cancelado,pendiente");
        }
    }
}
