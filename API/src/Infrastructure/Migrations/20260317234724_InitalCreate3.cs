using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitalCreate3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Events_ServiceTypes_ServiceTypeId",
                table: "Events");

            migrationBuilder.RenameColumn(
                name: "ServiceTypeId",
                table: "Events",
                newName: "CategoryId");

            migrationBuilder.RenameIndex(
                name: "IX_Events_ServiceTypeId",
                table: "Events",
                newName: "IX_Events_CategoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_Events_Categories_CategoryId",
                table: "Events",
                column: "CategoryId",
                principalTable: "Categories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Events_Categories_CategoryId",
                table: "Events");

            migrationBuilder.RenameColumn(
                name: "CategoryId",
                table: "Events",
                newName: "ServiceTypeId");

            migrationBuilder.RenameIndex(
                name: "IX_Events_CategoryId",
                table: "Events",
                newName: "IX_Events_ServiceTypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_Events_ServiceTypes_ServiceTypeId",
                table: "Events",
                column: "ServiceTypeId",
                principalTable: "ServiceTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
