using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ServiceRatingMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "VendorRatings");

            migrationBuilder.CreateTable(
                name: "ServiceRatings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ServiceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Rating = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Review = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    VendorUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServiceRatings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ServiceRatings_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ServiceRatings_Services_ServiceId",
                        column: x => x.ServiceId,
                        principalTable: "Services",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ServiceRatings_Vendors_VendorUserId",
                        column: x => x.VendorUserId,
                        principalTable: "Vendors",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateIndex(
                name: "IX_ServiceRatings_ServiceId",
                table: "ServiceRatings",
                column: "ServiceId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceRatings_UserId",
                table: "ServiceRatings",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceRatings_VendorUserId",
                table: "ServiceRatings",
                column: "VendorUserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ServiceRatings");

            migrationBuilder.CreateTable(
                name: "VendorRatings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    VendorId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Rating = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Review = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VendorRatings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VendorRatings_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_VendorRatings_Vendors_VendorId",
                        column: x => x.VendorId,
                        principalTable: "Vendors",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_VendorRatings_UserId",
                table: "VendorRatings",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_VendorRatings_VendorId",
                table: "VendorRatings",
                column: "VendorId");
        }
    }
}
