using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ParkSmart.Migrations
{
    /// <inheritdoc />
    public partial class MigracionInicial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Usuarios",
                columns: table => new
                {
                    usuarioId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    nombreCompleto = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    passwordHash = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    rol = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    fechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Usuarios", x => x.usuarioId);
                });

            migrationBuilder.CreateTable(
                name: "Sedes",
                columns: table => new
                {
                    sedeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    direccion = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    estado = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    passwordAcceso = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    tarifaPorHora = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    multaPorHora = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    multaConTope = table.Column<bool>(type: "bit", nullable: false),
                    montoMaximoMulta = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    fechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    creadoPorUsuarioId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sedes", x => x.sedeId);
                    table.ForeignKey(
                        name: "FK_Sedes_Usuarios_creadoPorUsuarioId",
                        column: x => x.creadoPorUsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "usuarioId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Niveles",
                columns: table => new
                {
                    nivelId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    numeroPiso = table.Column<int>(type: "int", nullable: false),
                    capacidad = table.Column<int>(type: "int", nullable: false),
                    sedeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Niveles", x => x.nivelId);
                    table.ForeignKey(
                        name: "FK_Niveles_Sedes_sedeId",
                        column: x => x.sedeId,
                        principalTable: "Sedes",
                        principalColumn: "sedeId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Cajones",
                columns: table => new
                {
                    cajonId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    numeroCajon = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    tipo = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    estadoActual = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    nivelId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cajones", x => x.cajonId);
                    table.ForeignKey(
                        name: "FK_Cajones_Niveles_nivelId",
                        column: x => x.nivelId,
                        principalTable: "Niveles",
                        principalColumn: "nivelId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Reservas",
                columns: table => new
                {
                    reservaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    placaVehiculo = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    fechaReserva = table.Column<DateTime>(type: "datetime2", nullable: false),
                    duracionEstimadaHoras = table.Column<int>(type: "int", nullable: false),
                    estado = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    cajonId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    creadoPorUsuarioId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reservas", x => x.reservaId);
                    table.ForeignKey(
                        name: "FK_Reservas_Cajones_cajonId",
                        column: x => x.cajonId,
                        principalTable: "Cajones",
                        principalColumn: "cajonId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Reservas_Usuarios_creadoPorUsuarioId",
                        column: x => x.creadoPorUsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "usuarioId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Tickets",
                columns: table => new
                {
                    ticketId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    placaVehiculo = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    horaEntrada = table.Column<DateTime>(type: "datetime2", nullable: false),
                    horaSalida = table.Column<DateTime>(type: "datetime2", nullable: true),
                    montoTotal = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    estado = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    cajonId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tickets", x => x.ticketId);
                    table.ForeignKey(
                        name: "FK_Tickets_Cajones_cajonId",
                        column: x => x.cajonId,
                        principalTable: "Cajones",
                        principalColumn: "cajonId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Cajones_nivelId_numeroCajon",
                table: "Cajones",
                columns: new[] { "nivelId", "numeroCajon" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Niveles_sedeId",
                table: "Niveles",
                column: "sedeId");

            migrationBuilder.CreateIndex(
                name: "IX_Reservas_cajonId",
                table: "Reservas",
                column: "cajonId");

            migrationBuilder.CreateIndex(
                name: "IX_Reservas_creadoPorUsuarioId",
                table: "Reservas",
                column: "creadoPorUsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_Sedes_creadoPorUsuarioId",
                table: "Sedes",
                column: "creadoPorUsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_cajonId",
                table: "Tickets",
                column: "cajonId");

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_email",
                table: "Usuarios",
                column: "email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Reservas");

            migrationBuilder.DropTable(
                name: "Tickets");

            migrationBuilder.DropTable(
                name: "Cajones");

            migrationBuilder.DropTable(
                name: "Niveles");

            migrationBuilder.DropTable(
                name: "Sedes");

            migrationBuilder.DropTable(
                name: "Usuarios");
        }
    }
}
