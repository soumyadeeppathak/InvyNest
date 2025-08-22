using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InvyNest_API.Migrations
{
    /// <inheritdoc />
    public partial class WorkspaceItemSingleKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_WorkspaceItems",
                table: "WorkspaceItems");

            migrationBuilder.AddColumn<Guid>(
                name: "Id",
                table: "WorkspaceItems",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "ParentWorkspaceItemId",
                table: "WorkspaceItems",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_WorkspaceItems",
                table: "WorkspaceItems",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_WorkspaceItems_ParentWorkspaceItemId",
                table: "WorkspaceItems",
                column: "ParentWorkspaceItemId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkspaceItems_WorkspaceId",
                table: "WorkspaceItems",
                column: "WorkspaceId");

            migrationBuilder.AddForeignKey(
                name: "FK_WorkspaceItems_WorkspaceItems_ParentWorkspaceItemId",
                table: "WorkspaceItems",
                column: "ParentWorkspaceItemId",
                principalTable: "WorkspaceItems",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WorkspaceItems_WorkspaceItems_ParentWorkspaceItemId",
                table: "WorkspaceItems");

            migrationBuilder.DropPrimaryKey(
                name: "PK_WorkspaceItems",
                table: "WorkspaceItems");

            migrationBuilder.DropIndex(
                name: "IX_WorkspaceItems_ParentWorkspaceItemId",
                table: "WorkspaceItems");

            migrationBuilder.DropIndex(
                name: "IX_WorkspaceItems_WorkspaceId",
                table: "WorkspaceItems");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "WorkspaceItems");

            migrationBuilder.DropColumn(
                name: "ParentWorkspaceItemId",
                table: "WorkspaceItems");

            migrationBuilder.AddPrimaryKey(
                name: "PK_WorkspaceItems",
                table: "WorkspaceItems",
                columns: new[] { "WorkspaceId", "ItemId" });
        }
    }
}
