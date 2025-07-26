using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LoraStatsNet.Database.Migrations
{
    /// <inheritdoc />
    public partial class NodesScreenIndices : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PacketReport_GatewayId",
                table: "PacketReport");

            migrationBuilder.DropIndex(
                name: "IX_Packet_FromNodeId",
                table: "Packet");

            migrationBuilder.CreateIndex(
                name: "IX_PacketReport_GatewayId_ReceptionTime",
                table: "PacketReport",
                columns: new[] { "GatewayId", "ReceptionTime" });

            migrationBuilder.CreateIndex(
                name: "IX_Packet_FromNodeId_FirstSeen_Port",
                table: "Packet",
                columns: new[] { "FromNodeId", "FirstSeen", "Port" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PacketReport_GatewayId_ReceptionTime",
                table: "PacketReport");

            migrationBuilder.DropIndex(
                name: "IX_Packet_FromNodeId_FirstSeen_Port",
                table: "Packet");

            migrationBuilder.CreateIndex(
                name: "IX_PacketReport_GatewayId",
                table: "PacketReport",
                column: "GatewayId");

            migrationBuilder.CreateIndex(
                name: "IX_Packet_FromNodeId",
                table: "Packet",
                column: "FromNodeId");
        }
    }
}
