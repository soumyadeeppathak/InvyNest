using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InvyNest_API.Migrations
{
    /// <inheritdoc />
    public partial class AddHolderAndLocationToWorkspaceItem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Holder",
                table: "WorkspaceItems",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LocationNote",
                table: "WorkspaceItems",
                type: "text",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_WorkspaceItems_Holder",
                table: "WorkspaceItems",
                column: "Holder");

            migrationBuilder.CreateIndex(
                name: "IX_WorkspaceItems_LocationNote",
                table: "WorkspaceItems",
                column: "LocationNote");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_WorkspaceItems_Holder",
                table: "WorkspaceItems");

            migrationBuilder.DropIndex(
                name: "IX_WorkspaceItems_LocationNote",
                table: "WorkspaceItems");

            migrationBuilder.DropColumn(
                name: "Holder",
                table: "WorkspaceItems");

            migrationBuilder.DropColumn(
                name: "LocationNote",
                table: "WorkspaceItems");
        }
    }
}
