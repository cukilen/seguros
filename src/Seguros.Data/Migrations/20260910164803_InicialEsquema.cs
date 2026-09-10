using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Seguros.Data.Migrations
{
    /// <inheritdoc />
    public partial class InicialEsquema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Companias",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Nombre = table.Column<string>(type: "TEXT", nullable: false),
                    Telefono = table.Column<string>(type: "TEXT", nullable: true),
                    Email = table.Column<string>(type: "TEXT", nullable: true),
                    Activa = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Companias", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Productores",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    NombreUsuario = table.Column<string>(type: "TEXT", nullable: false),
                    PasswordHash = table.Column<string>(type: "TEXT", nullable: false),
                    Nombre = table.Column<string>(type: "TEXT", nullable: false),
                    Activo = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Productores", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Ramos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Nombre = table.Column<string>(type: "TEXT", nullable: false, collation: "NOCASE")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ramos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Asegurados",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ProductorId = table.Column<int>(type: "INTEGER", nullable: false),
                    Nombre = table.Column<string>(type: "TEXT", nullable: false),
                    TipoDocumento = table.Column<int>(type: "INTEGER", nullable: true),
                    NroDocumento = table.Column<string>(type: "TEXT", nullable: true),
                    Telefono = table.Column<string>(type: "TEXT", nullable: true),
                    Email = table.Column<string>(type: "TEXT", nullable: true),
                    Domicilio = table.Column<string>(type: "TEXT", nullable: true),
                    Activo = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Asegurados", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Asegurados_Productores_ProductorId",
                        column: x => x.ProductorId,
                        principalTable: "Productores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Liquidaciones",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ProductorId = table.Column<int>(type: "INTEGER", nullable: false),
                    PeriodoDesde = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    PeriodoHasta = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    MontoTotal = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    FechaGeneracion = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Liquidaciones", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Liquidaciones_Productores_ProductorId",
                        column: x => x.ProductorId,
                        principalTable: "Productores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CompaniaRamos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CompaniaId = table.Column<int>(type: "INTEGER", nullable: false),
                    RamoId = table.Column<int>(type: "INTEGER", nullable: false),
                    PorcentajeComision = table.Column<decimal>(type: "decimal(5,2)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompaniaRamos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CompaniaRamos_Companias_CompaniaId",
                        column: x => x.CompaniaId,
                        principalTable: "Companias",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CompaniaRamos_Ramos_RamoId",
                        column: x => x.RamoId,
                        principalTable: "Ramos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Polizas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ProductorId = table.Column<int>(type: "INTEGER", nullable: false),
                    AseguradoId = table.Column<int>(type: "INTEGER", nullable: false),
                    CompaniaId = table.Column<int>(type: "INTEGER", nullable: false),
                    RamoId = table.Column<int>(type: "INTEGER", nullable: false),
                    Numero = table.Column<string>(type: "TEXT", nullable: false),
                    VigenciaDesde = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    VigenciaHasta = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    Prima = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Producto = table.Column<string>(type: "TEXT", nullable: true),
                    Premio = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Cobertura = table.Column<string>(type: "TEXT", nullable: true),
                    DescripcionRiesgo = table.Column<string>(type: "TEXT", nullable: true),
                    Estado = table.Column<int>(type: "INTEGER", nullable: false),
                    MotivoAnulacion = table.Column<string>(type: "TEXT", nullable: true),
                    EsFlota = table.Column<bool>(type: "INTEGER", nullable: false),
                    PolizaOrigenId = table.Column<int>(type: "INTEGER", nullable: true),
                    VencimientoGestionado = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Polizas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Polizas_Asegurados_AseguradoId",
                        column: x => x.AseguradoId,
                        principalTable: "Asegurados",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Polizas_Companias_CompaniaId",
                        column: x => x.CompaniaId,
                        principalTable: "Companias",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Polizas_Polizas_PolizaOrigenId",
                        column: x => x.PolizaOrigenId,
                        principalTable: "Polizas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Polizas_Productores_ProductorId",
                        column: x => x.ProductorId,
                        principalTable: "Productores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Polizas_Ramos_RamoId",
                        column: x => x.RamoId,
                        principalTable: "Ramos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Endosos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PolizaId = table.Column<int>(type: "INTEGER", nullable: false),
                    Fecha = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    Tipo = table.Column<string>(type: "TEXT", nullable: false),
                    Detalle = table.Column<string>(type: "TEXT", nullable: false),
                    Anulado = table.Column<bool>(type: "INTEGER", nullable: false),
                    MotivoAnulacion = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Endosos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Endosos_Polizas_PolizaId",
                        column: x => x.PolizaId,
                        principalTable: "Polizas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LiquidacionDetalles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    LiquidacionId = table.Column<int>(type: "INTEGER", nullable: false),
                    PolizaId = table.Column<int>(type: "INTEGER", nullable: false),
                    MontoComision = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LiquidacionDetalles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LiquidacionDetalles_Liquidaciones_LiquidacionId",
                        column: x => x.LiquidacionId,
                        principalTable: "Liquidaciones",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LiquidacionDetalles_Polizas_PolizaId",
                        column: x => x.PolizaId,
                        principalTable: "Polizas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UnidadesFlota",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PolizaId = table.Column<int>(type: "INTEGER", nullable: false),
                    Patente = table.Column<string>(type: "TEXT", nullable: false),
                    Marca = table.Column<string>(type: "TEXT", nullable: false),
                    Modelo = table.Column<string>(type: "TEXT", nullable: false),
                    Uso = table.Column<string>(type: "TEXT", nullable: false),
                    Activa = table.Column<bool>(type: "INTEGER", nullable: false),
                    FechaBaja = table.Column<DateOnly>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UnidadesFlota", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UnidadesFlota_Polizas_PolizaId",
                        column: x => x.PolizaId,
                        principalTable: "Polizas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Ramos",
                columns: new[] { "Id", "Nombre" },
                values: new object[,]
                {
                    { 1, "Autos" },
                    { 2, "Motos" },
                    { 3, "Vida" },
                    { 4, "Vida Colectivo" },
                    { 5, "Vida Individual" },
                    { 6, "Vida Obligatorio" },
                    { 7, "Incendio" },
                    { 8, "Combinado Familiar" },
                    { 9, "Responsabilidad Civil" },
                    { 10, "Accidentes Personales" },
                    { 11, "Integral de Comercio" },
                    { 12, "Caución" },
                    { 13, "Transporte / Cascos" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Asegurados_ProductorId_TipoDocumento_NroDocumento",
                table: "Asegurados",
                columns: new[] { "ProductorId", "TipoDocumento", "NroDocumento" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CompaniaRamos_CompaniaId_RamoId",
                table: "CompaniaRamos",
                columns: new[] { "CompaniaId", "RamoId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CompaniaRamos_RamoId",
                table: "CompaniaRamos",
                column: "RamoId");

            migrationBuilder.CreateIndex(
                name: "IX_Endosos_PolizaId",
                table: "Endosos",
                column: "PolizaId");

            migrationBuilder.CreateIndex(
                name: "IX_LiquidacionDetalles_LiquidacionId",
                table: "LiquidacionDetalles",
                column: "LiquidacionId");

            migrationBuilder.CreateIndex(
                name: "IX_LiquidacionDetalles_PolizaId",
                table: "LiquidacionDetalles",
                column: "PolizaId");

            migrationBuilder.CreateIndex(
                name: "IX_Liquidaciones_ProductorId",
                table: "Liquidaciones",
                column: "ProductorId");

            migrationBuilder.CreateIndex(
                name: "IX_Polizas_AseguradoId",
                table: "Polizas",
                column: "AseguradoId");

            migrationBuilder.CreateIndex(
                name: "IX_Polizas_CompaniaId_Numero",
                table: "Polizas",
                columns: new[] { "CompaniaId", "Numero" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Polizas_PolizaOrigenId",
                table: "Polizas",
                column: "PolizaOrigenId");

            migrationBuilder.CreateIndex(
                name: "IX_Polizas_ProductorId",
                table: "Polizas",
                column: "ProductorId");

            migrationBuilder.CreateIndex(
                name: "IX_Polizas_RamoId",
                table: "Polizas",
                column: "RamoId");

            migrationBuilder.CreateIndex(
                name: "IX_Productores_NombreUsuario",
                table: "Productores",
                column: "NombreUsuario",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Ramos_Nombre",
                table: "Ramos",
                column: "Nombre",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UnidadesFlota_PolizaId",
                table: "UnidadesFlota",
                column: "PolizaId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CompaniaRamos");

            migrationBuilder.DropTable(
                name: "Endosos");

            migrationBuilder.DropTable(
                name: "LiquidacionDetalles");

            migrationBuilder.DropTable(
                name: "UnidadesFlota");

            migrationBuilder.DropTable(
                name: "Liquidaciones");

            migrationBuilder.DropTable(
                name: "Polizas");

            migrationBuilder.DropTable(
                name: "Asegurados");

            migrationBuilder.DropTable(
                name: "Companias");

            migrationBuilder.DropTable(
                name: "Ramos");

            migrationBuilder.DropTable(
                name: "Productores");
        }
    }
}
