using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IkiForma.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddStintTrackingFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "appearances",
                table: "stint",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "end_date_precision",
                table: "stint",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Unknown");

            migrationBuilder.AddColumn<int>(
                name: "goals",
                table: "stint",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "source_statement_id",
                table: "stint",
                type: "character varying(64)",
                maxLength: 64,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "start_date_precision",
                table: "stint",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Unknown");

            migrationBuilder.AddColumn<string>(
                name: "stint_type",
                table: "stint",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Unknown");

            migrationBuilder.CreateIndex(
                name: "ix_stint_source_statement_id",
                table: "stint",
                column: "source_statement_id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_stint_source_statement_id",
                table: "stint");

            migrationBuilder.DropColumn(
                name: "appearances",
                table: "stint");

            migrationBuilder.DropColumn(
                name: "end_date_precision",
                table: "stint");

            migrationBuilder.DropColumn(
                name: "goals",
                table: "stint");

            migrationBuilder.DropColumn(
                name: "source_statement_id",
                table: "stint");

            migrationBuilder.DropColumn(
                name: "start_date_precision",
                table: "stint");

            migrationBuilder.DropColumn(
                name: "stint_type",
                table: "stint");
        }
    }
}
