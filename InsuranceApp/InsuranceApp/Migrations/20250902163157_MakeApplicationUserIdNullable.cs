using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InsuranceApp.Migrations
{
    /// <inheritdoc />
    public partial class MakeApplicationUserIdNullable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_InsuredPersons_ApplicationUserId",
                table: "InsuredPersons");

            migrationBuilder.AlterColumn<string>(
                name: "ApplicationUserId",
                table: "InsuredPersons",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.CreateIndex(
                name: "IX_InsuredPersons_ApplicationUserId",
                table: "InsuredPersons",
                column: "ApplicationUserId",
                unique: true,
                filter: "[ApplicationUserId] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_InsuredPersons_ApplicationUserId",
                table: "InsuredPersons");

            migrationBuilder.AlterColumn<string>(
                name: "ApplicationUserId",
                table: "InsuredPersons",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_InsuredPersons_ApplicationUserId",
                table: "InsuredPersons",
                column: "ApplicationUserId",
                unique: true);
        }
    }
}
