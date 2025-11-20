using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PaginationApi.Migrations
{
    /// <inheritdoc />
    public partial class AddedCreatedAndUpdatedDate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "createdDate",
                table: "data_small",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "updatedDate",
                table: "data_small",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "createdDate",
                table: "data_small");

            migrationBuilder.DropColumn(
                name: "updatedDate",
                table: "data_small");
        }
    }
}
