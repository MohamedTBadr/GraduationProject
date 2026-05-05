using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class EditServiceArea : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ServiceAreas_Vendors_VendorUserId",
                table: "ServiceAreas");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ServiceAreas",
                table: "ServiceAreas");

            migrationBuilder.DropIndex(
                name: "IX_ServiceAreas_VendorUserId",
                table: "ServiceAreas");

            migrationBuilder.RenameColumn(
                name: "VendorUserId",
                table: "ServiceAreas",
                newName: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ServiceAreas",
                table: "ServiceAreas",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceAreas_VendorId",
                table: "ServiceAreas",
                column: "VendorId");

            migrationBuilder.AddForeignKey(
                name: "FK_ServiceAreas_Vendors_VendorId",
                table: "ServiceAreas",
                column: "VendorId",
                principalTable: "Vendors",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ServiceAreas_Vendors_VendorId",
                table: "ServiceAreas");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ServiceAreas",
                table: "ServiceAreas");

            migrationBuilder.DropIndex(
                name: "IX_ServiceAreas_VendorId",
                table: "ServiceAreas");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "ServiceAreas",
                newName: "VendorUserId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ServiceAreas",
                table: "ServiceAreas",
                column: "VendorId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceAreas_VendorUserId",
                table: "ServiceAreas",
                column: "VendorUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_ServiceAreas_Vendors_VendorUserId",
                table: "ServiceAreas",
                column: "VendorUserId",
                principalTable: "Vendors",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
