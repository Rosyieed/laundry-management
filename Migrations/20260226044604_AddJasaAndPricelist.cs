using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LaundryManagement.Migrations
{
    /// <inheritdoc />
    public partial class AddJasaAndPricelist : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Jasas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NamaJasa = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Satuan = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Jasas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PricelistJasas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    JasaId = table.Column<int>(type: "int", nullable: false),
                    TipeLayanan = table.Column<int>(type: "int", nullable: false),
                    Harga = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PricelistJasas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PricelistJasas_Jasas_JasaId",
                        column: x => x.JasaId,
                        principalTable: "Jasas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PricelistJasas_JasaId",
                table: "PricelistJasas",
                column: "JasaId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PricelistJasas");

            migrationBuilder.DropTable(
                name: "Jasas");
        }
    }
}
