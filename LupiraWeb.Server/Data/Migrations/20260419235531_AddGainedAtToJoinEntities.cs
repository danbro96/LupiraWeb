using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LupiraWeb.Server.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddGainedAtToJoinEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "GainedAt",
                table: "ProjectSkills",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "GainedAt",
                table: "EmploymentSkills",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GainedAt",
                table: "ProjectSkills");

            migrationBuilder.DropColumn(
                name: "GainedAt",
                table: "EmploymentSkills");
        }
    }
}
