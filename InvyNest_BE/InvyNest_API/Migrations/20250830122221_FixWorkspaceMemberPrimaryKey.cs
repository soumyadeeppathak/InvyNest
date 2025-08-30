using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InvyNest_API.Migrations
{
    /// <inheritdoc />
    public partial class FixWorkspaceMemberPrimaryKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_WorkspaceMembers",
                table: "WorkspaceMembers");

            migrationBuilder.AlterColumn<string>(
                name: "MemberName",
                table: "WorkspaceMembers",
                type: "character varying(120)",
                maxLength: 120,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "MemberEmail",
                table: "WorkspaceMembers",
                type: "character varying(160)",
                maxLength: 160,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(160)",
                oldMaxLength: 160);

            migrationBuilder.AddColumn<Guid>(
                name: "Id",
                table: "WorkspaceMembers",
                type: "uuid",
                nullable: false,
                defaultValueSql: "gen_random_uuid()");

            migrationBuilder.AddPrimaryKey(
                name: "PK_WorkspaceMembers",
                table: "WorkspaceMembers",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_WorkspaceMembers_WorkspaceId_MemberEmail",
                table: "WorkspaceMembers",
                columns: new[] { "WorkspaceId", "MemberEmail" },
                unique: true,
                filter: "\"MemberEmail\" IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_WorkspaceMembers",
                table: "WorkspaceMembers");

            migrationBuilder.DropIndex(
                name: "IX_WorkspaceMembers_WorkspaceId_MemberEmail",
                table: "WorkspaceMembers");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "WorkspaceMembers");

            migrationBuilder.AlterColumn<string>(
                name: "MemberName",
                table: "WorkspaceMembers",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(120)",
                oldMaxLength: 120);

            migrationBuilder.AlterColumn<string>(
                name: "MemberEmail",
                table: "WorkspaceMembers",
                type: "character varying(160)",
                maxLength: 160,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(160)",
                oldMaxLength: 160,
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_WorkspaceMembers",
                table: "WorkspaceMembers",
                columns: new[] { "WorkspaceId", "MemberEmail" });
        }
    }
}
