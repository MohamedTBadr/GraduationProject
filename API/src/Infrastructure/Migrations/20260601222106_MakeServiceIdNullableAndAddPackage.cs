using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class MakeServiceIdNullableAndAddPackage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EventItems_Services_ServiceId",
                table: "EventItems");

            migrationBuilder.AlterColumn<Guid>(
                name: "ServiceId",
                table: "EventItems",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AddColumn<Guid>(
                name: "PackageId",
                table: "EventItems",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_EventItems_PackageId",
                table: "EventItems",
                column: "PackageId");

            migrationBuilder.AddForeignKey(
                name: "FK_EventItems_Packages_PackageId",
                table: "EventItems",
                column: "PackageId",
                principalTable: "Packages",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_EventItems_Services_ServiceId",
                table: "EventItems",
                column: "ServiceId",
                principalTable: "Services",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EventItems_Packages_PackageId",
                table: "EventItems");

            migrationBuilder.DropForeignKey(
                name: "FK_EventItems_Services_ServiceId",
                table: "EventItems");

            migrationBuilder.DropIndex(
                name: "IX_EventItems_PackageId",
                table: "EventItems");

            migrationBuilder.DropColumn(
                name: "PackageId",
                table: "EventItems");

            migrationBuilder.AlterColumn<Guid>(
                name: "ServiceId",
                table: "EventItems",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_EventItems_Services_ServiceId",
                table: "EventItems",
                column: "ServiceId",
                principalTable: "Services",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
