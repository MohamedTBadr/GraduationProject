using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Edits : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EventItems_AspNetUsers_VendorId",
                table: "EventItems");

            migrationBuilder.DropColumn(
                name: "ServiceImage",
                table: "EventItems");

            migrationBuilder.DropColumn(
                name: "ServiceName",
                table: "EventItems");

            migrationBuilder.DropColumn(
                name: "VendorName",
                table: "EventItems");

            migrationBuilder.RenameColumn(
                name: "VendorId",
                table: "EventItems",
                newName: "ServiceId");

            migrationBuilder.RenameIndex(
                name: "IX_EventItems_VendorId",
                table: "EventItems",
                newName: "IX_EventItems_ServiceId");

            migrationBuilder.AddForeignKey(
                name: "FK_EventItems_Services_ServiceId",
                table: "EventItems",
                column: "ServiceId",
                principalTable: "Services",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EventItems_Services_ServiceId",
                table: "EventItems");

            migrationBuilder.RenameColumn(
                name: "ServiceId",
                table: "EventItems",
                newName: "VendorId");

            migrationBuilder.RenameIndex(
                name: "IX_EventItems_ServiceId",
                table: "EventItems",
                newName: "IX_EventItems_VendorId");

            migrationBuilder.AddColumn<string>(
                name: "ServiceImage",
                table: "EventItems",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ServiceName",
                table: "EventItems",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "VendorName",
                table: "EventItems",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddForeignKey(
                name: "FK_EventItems_AspNetUsers_VendorId",
                table: "EventItems",
                column: "VendorId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
