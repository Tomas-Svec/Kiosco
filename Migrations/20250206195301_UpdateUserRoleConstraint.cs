using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kiosco.Migrations
{
    /// <inheritdoc />
    public partial class UpdateUserRoleConstraint : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AuditLogs_Users_EmpleadoId",
                table: "AuditLogs");

            migrationBuilder.DropIndex(
                name: "IX_AuditLogs_EmpleadoId",
                table: "AuditLogs");

            migrationBuilder.AlterColumn<string>(
                name: "Accion",
                table: "AuditLogs",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(250)",
                oldMaxLength: 250);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Accion",
                table: "AuditLogs",
                type: "nvarchar(250)",
                maxLength: 250,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_EmpleadoId",
                table: "AuditLogs",
                column: "EmpleadoId");

            migrationBuilder.AddForeignKey(
                name: "FK_AuditLogs_Users_EmpleadoId",
                table: "AuditLogs",
                column: "EmpleadoId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
