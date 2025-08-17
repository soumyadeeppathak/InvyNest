using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InvyNest_API.Migrations
{
    /// <inheritdoc />
    public partial class Move_CheckConstraints_To_TableBuilder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ChildCount",
                table: "ItemComponents",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddCheckConstraint(
                name: "ck_item_components_count_positive",
                table: "ItemComponents",
                sql: "\"ChildCount\" > 0");

            migrationBuilder.AddCheckConstraint(
                name: "ck_item_components_no_self",
                table: "ItemComponents",
                sql: "\"ParentItemId\" <> \"ChildItemId\"");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "ck_item_components_count_positive",
                table: "ItemComponents");

            migrationBuilder.DropCheckConstraint(
                name: "ck_item_components_no_self",
                table: "ItemComponents");

            migrationBuilder.DropColumn(
                name: "ChildCount",
                table: "ItemComponents");
        }
    }
}
