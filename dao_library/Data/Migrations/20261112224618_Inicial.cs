using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace dao_library.Data.Migrations
{
    /// <inheritdoc />
    public partial class Inicial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Divisiones",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Anio = table.Column<int>(type: "int", nullable: false),
                    Letra = table.Column<string>(type: "nvarchar(1)", maxLength: 1, nullable: false),
                    Turno = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: false),
                    Cupo = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Divisiones", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Materias",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    HorasSemanales = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Materias", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Personas",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    Apellido = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    Dni = table.Column<string>(type: "nvarchar(8)", maxLength: 8, nullable: false),
                    FechaNacimiento = table.Column<DateOnly>(type: "date", nullable: false),
                    Rol = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Legajo = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    DivisionId = table.Column<long>(type: "bigint", nullable: true),
                    Titulo = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Personas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Personas_Divisiones_DivisionId",
                        column: x => x.DivisionId,
                        principalTable: "Divisiones",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Calificaciones",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nota = table.Column<decimal>(type: "decimal(4,2)", precision: 4, scale: 2, nullable: false),
                    Fecha = table.Column<DateOnly>(type: "date", nullable: false),
                    Tipo = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Observacion = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    AlumnoId = table.Column<long>(type: "bigint", nullable: false),
                    MateriaId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Calificaciones", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Calificaciones_Materias_MateriaId",
                        column: x => x.MateriaId,
                        principalTable: "Materias",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Calificaciones_Personas_AlumnoId",
                        column: x => x.AlumnoId,
                        principalTable: "Personas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DocenteMateria",
                columns: table => new
                {
                    DocentesId = table.Column<long>(type: "bigint", nullable: false),
                    MateriasId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocenteMateria", x => new { x.DocentesId, x.MateriasId });
                    table.ForeignKey(
                        name: "FK_DocenteMateria_Materias_MateriasId",
                        column: x => x.MateriasId,
                        principalTable: "Materias",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DocenteMateria_Personas_DocentesId",
                        column: x => x.DocentesId,
                        principalTable: "Personas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Calificaciones_AlumnoId",
                table: "Calificaciones",
                column: "AlumnoId");

            migrationBuilder.CreateIndex(
                name: "IX_Calificaciones_MateriaId",
                table: "Calificaciones",
                column: "MateriaId");

            migrationBuilder.CreateIndex(
                name: "IX_Divisiones_Anio_Letra_Turno",
                table: "Divisiones",
                columns: new[] { "Anio", "Letra", "Turno" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DocenteMateria_MateriasId",
                table: "DocenteMateria",
                column: "MateriasId");

            migrationBuilder.CreateIndex(
                name: "IX_Materias_Nombre",
                table: "Materias",
                column: "Nombre",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Personas_DivisionId",
                table: "Personas",
                column: "DivisionId");

            migrationBuilder.CreateIndex(
                name: "IX_Personas_Dni",
                table: "Personas",
                column: "Dni",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Personas_Legajo",
                table: "Personas",
                column: "Legajo",
                unique: true,
                filter: "[Legajo] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Calificaciones");

            migrationBuilder.DropTable(
                name: "DocenteMateria");

            migrationBuilder.DropTable(
                name: "Materias");

            migrationBuilder.DropTable(
                name: "Personas");

            migrationBuilder.DropTable(
                name: "Divisiones");
        }
    }
}
