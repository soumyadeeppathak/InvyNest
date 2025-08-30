using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InvyNest_API.Migrations
{
    /// <inheritdoc />
    public partial class AddMemberNameToWorkspaceMember : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "MemberName",
                table: "WorkspaceMembers",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MemberName",
                table: "WorkspaceMembers");
        }
    }
}
