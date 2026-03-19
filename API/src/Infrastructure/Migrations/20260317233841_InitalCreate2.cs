using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitalCreate2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Package_Vendors_VendorId",
                table: "Package");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Package",
                table: "Package");

            migrationBuilder.RenameTable(
                name: "Package",
                newName: "Packages");

            migrationBuilder.RenameIndex(
                name: "IX_Package_VendorId",
                table: "Packages",
                newName: "IX_Packages_VendorId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Packages",
                table: "Packages",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Packages_Vendors_VendorId",
                table: "Packages",
                column: "VendorId",
                principalTable: "Vendors",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Packages_Vendors_VendorId",
                table: "Packages");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Packages",
                table: "Packages");

            migrationBuilder.RenameTable(
                name: "Packages",
                newName: "Package");

            migrationBuilder.RenameIndex(
                name: "IX_Packages_VendorId",
                table: "Package",
                newName: "IX_Package_VendorId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Package",
                table: "Package",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Package_Vendors_VendorId",
                table: "Package",
                column: "VendorId",
                principalTable: "Vendors",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
