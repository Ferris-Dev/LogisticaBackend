using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Tracking.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Inicial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "pilotos",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    codigo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    nombre = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    activo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_pilotos", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "paquetes",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    guia = table.Column<string>(type: "character varying(9)", maxLength: 9, nullable: false),
                    piloto_id = table.Column<int>(type: "integer", nullable: false),
                    cliente = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    telefono = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    direccion = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    zona = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    estado = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    observaciones = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    fecha_asignacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ultima_actualizacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_paquetes", x => x.id);
                    table.ForeignKey(
                        name: "fk_paquetes_pilotos_piloto_id",
                        column: x => x.piloto_id,
                        principalTable: "pilotos",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "historial_estados",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    paquete_id = table.Column<int>(type: "integer", nullable: false),
                    estado_anterior = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    estado_nuevo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    fecha_cambio = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    fecha_registro = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    origen = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_historial_estados", x => x.id);
                    table.ForeignKey(
                        name: "fk_historial_estados_paquetes_paquete_id",
                        column: x => x.paquete_id,
                        principalTable: "paquetes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "pilotos",
                columns: new[] { "id", "activo", "codigo", "nombre" },
                values: new object[,]
                {
                    { 1, true, "PIL-001", "Carlos Méndez" },
                    { 2, true, "PIL-002", "Ana López" }
                });

            migrationBuilder.InsertData(
                table: "paquetes",
                columns: new[] { "id", "cliente", "direccion", "estado", "fecha_asignacion", "guia", "observaciones", "piloto_id", "telefono", "ultima_actualizacion", "zona" },
                values: new object[,]
                {
                    { 1, "Ferretería El Constructor", "12 Calle 4-50 Zona 10", "PENDIENTE", new DateTime(2026, 9, 26, 14, 0, 0, 0, DateTimeKind.Utc), "GUA-10001", null, 1, "5555-1234", new DateTime(2026, 9, 26, 14, 0, 0, 0, DateTimeKind.Utc), "Zona 10" },
                    { 2, "Farmacia La Salud", "6a Avenida 8-20 Zona 1", "EN_RUTA", new DateTime(2026, 9, 26, 14, 0, 0, 0, DateTimeKind.Utc), "GUA-10002", null, 1, "5555-2345", new DateTime(2026, 9, 26, 14, 0, 0, 0, DateTimeKind.Utc), "Zona 1" },
                    { 3, "Librería Cervantes", "Calzada Roosevelt 22-43 Zona 11", "ENTREGADO", new DateTime(2026, 9, 26, 14, 0, 0, 0, DateTimeKind.Utc), "GUA-10003", null, 1, "5555-3456", new DateTime(2026, 9, 26, 14, 0, 0, 0, DateTimeKind.Utc), "Zona 11" },
                    { 4, "Supermercado Don Pedro", "Boulevard Los Próceres 18-60 Zona 10", "NO_ENTREGADO", new DateTime(2026, 9, 26, 14, 0, 0, 0, DateTimeKind.Utc), "GUA-10004", "Cliente ausente, se dejó aviso", 1, "5555-4567", new DateTime(2026, 9, 26, 14, 0, 0, 0, DateTimeKind.Utc), "Zona 10" },
                    { 5, "Panadería San Martín", "Avenida Petapa 45-12 Zona 12", "PENDIENTE", new DateTime(2026, 9, 26, 14, 0, 0, 0, DateTimeKind.Utc), "GUA-10005", null, 2, "5555-5678", new DateTime(2026, 9, 26, 14, 0, 0, 0, DateTimeKind.Utc), "Zona 12" },
                    { 6, "Óptica Visión Clara", "7a Avenida 3-33 Zona 9", "EN_RUTA", new DateTime(2026, 9, 26, 14, 0, 0, 0, DateTimeKind.Utc), "GUA-10006", null, 2, null, new DateTime(2026, 9, 26, 14, 0, 0, 0, DateTimeKind.Utc), "Zona 9" },
                    { 7, "Taller Mecánico Rápido", "Calzada Aguilar Batres 34-70 Zona 11", "PENDIENTE", new DateTime(2026, 9, 26, 14, 0, 0, 0, DateTimeKind.Utc), "GUA-10007", null, 2, "5555-7890", new DateTime(2026, 9, 26, 14, 0, 0, 0, DateTimeKind.Utc), "Zona 11" },
                    { 8, "Tienda Deportiva Campeón", "20 Calle 10-15 Zona 15", "ENTREGADO", new DateTime(2026, 9, 26, 14, 0, 0, 0, DateTimeKind.Utc), "GUA-10008", null, 2, "5555-8901", new DateTime(2026, 9, 26, 14, 0, 0, 0, DateTimeKind.Utc), "Zona 15" }
                });

            migrationBuilder.CreateIndex(
                name: "ix_historial_estados_paquete_id",
                table: "historial_estados",
                column: "paquete_id");

            migrationBuilder.CreateIndex(
                name: "ix_paquetes_guia",
                table: "paquetes",
                column: "guia",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_paquetes_piloto_id",
                table: "paquetes",
                column: "piloto_id");

            migrationBuilder.CreateIndex(
                name: "ix_pilotos_codigo",
                table: "pilotos",
                column: "codigo",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "historial_estados");

            migrationBuilder.DropTable(
                name: "paquetes");

            migrationBuilder.DropTable(
                name: "pilotos");
        }
    }
}
