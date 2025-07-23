using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LoraStatsNet.Database.Migrations
{
    /// <inheritdoc />
    public partial class NodeIdIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Node_NodeId",
                table: "Node",
                column: "NodeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Node_NodeId",
                table: "Node");
        }
    }
}
