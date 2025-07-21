using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LoraStatsNet.Database.Migrations
{
    /// <inheritdoc />
    public partial class InitialStructure : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Community",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    UrlName = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Community", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CommunityArea",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CommunityId = table.Column<long>(type: "INTEGER", nullable: false),
                    LatitudeMin = table.Column<double>(type: "REAL", nullable: false),
                    LatitudeMax = table.Column<double>(type: "REAL", nullable: false),
                    LongitudeMin = table.Column<double>(type: "REAL", nullable: false),
                    LongitudeMax = table.Column<double>(type: "REAL", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CommunityArea", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CommunityArea_Community_CommunityId",
                        column: x => x.CommunityId,
                        principalTable: "Community",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Node",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CommunityId = table.Column<long>(type: "INTEGER", nullable: true),
                    NodeId = table.Column<uint>(type: "INTEGER", nullable: false),
                    ShortName = table.Column<string>(type: "TEXT", nullable: true),
                    LongName = table.Column<string>(type: "TEXT", nullable: true),
                    Role = table.Column<int>(type: "INTEGER", nullable: true),
                    HwModel = table.Column<int>(type: "INTEGER", nullable: true),
                    LastSeen = table.Column<DateTime>(type: "TEXT", nullable: false),
                    LastLongitude = table.Column<double>(type: "REAL", nullable: true),
                    LastLatitude = table.Column<double>(type: "REAL", nullable: true),
                    LastElevation = table.Column<double>(type: "REAL", nullable: true),
                    LastPositionPrecision = table.Column<int>(type: "INTEGER", nullable: true),
                    LastPositionUpdate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    LastBoot = table.Column<DateTime>(type: "TEXT", nullable: true),
                    PublicKey = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Node", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Node_Community_CommunityId",
                        column: x => x.CommunityId,
                        principalTable: "Community",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Packet",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PacketId = table.Column<uint>(type: "INTEGER", nullable: false),
                    FromNodeId = table.Column<long>(type: "INTEGER", nullable: false),
                    ToNodeId = table.Column<long>(type: "INTEGER", nullable: true),
                    FirstSeen = table.Column<DateTime>(type: "TEXT", nullable: false),
                    HopStart = table.Column<byte>(type: "INTEGER", nullable: false),
                    WantAck = table.Column<bool>(type: "INTEGER", nullable: false),
                    WantResponse = table.Column<bool>(type: "INTEGER", nullable: false),
                    Port = table.Column<int>(type: "INTEGER", nullable: false),
                    BitField = table.Column<byte>(type: "INTEGER", nullable: false),
                    ParsedPayload = table.Column<string>(type: "TEXT", nullable: true),
                    Channel = table.Column<byte>(type: "INTEGER", nullable: false),
                    RequestId = table.Column<uint>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Packet", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Packet_Node_FromNodeId",
                        column: x => x.FromNodeId,
                        principalTable: "Node",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Packet_Node_ToNodeId",
                        column: x => x.ToNodeId,
                        principalTable: "Node",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PacketReport",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PacketId = table.Column<long>(type: "INTEGER", nullable: false),
                    GatewayId = table.Column<long>(type: "INTEGER", nullable: false),
                    ReceptionTime = table.Column<DateTime>(type: "TEXT", nullable: false),
                    SNR = table.Column<float>(type: "REAL", nullable: false),
                    RSSI = table.Column<byte>(type: "INTEGER", nullable: false),
                    HopLimit = table.Column<byte>(type: "INTEGER", nullable: false),
                    RelayNode = table.Column<byte>(type: "INTEGER", nullable: false),
                    NextHop = table.Column<byte>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PacketReport", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PacketReport_Node_GatewayId",
                        column: x => x.GatewayId,
                        principalTable: "Node",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PacketReport_Packet_PacketId",
                        column: x => x.PacketId,
                        principalTable: "Packet",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PacketData",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PacketReportId = table.Column<long>(type: "INTEGER", nullable: false),
                    RawData = table.Column<byte[]>(type: "BLOB", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PacketData", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PacketData_PacketReport_PacketReportId",
                        column: x => x.PacketReportId,
                        principalTable: "PacketReport",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CommunityArea_CommunityId",
                table: "CommunityArea",
                column: "CommunityId");

            migrationBuilder.CreateIndex(
                name: "IX_Node_CommunityId",
                table: "Node",
                column: "CommunityId");

            migrationBuilder.CreateIndex(
                name: "IX_Packet_FromNodeId",
                table: "Packet",
                column: "FromNodeId");

            migrationBuilder.CreateIndex(
                name: "IX_Packet_ToNodeId",
                table: "Packet",
                column: "ToNodeId");

            migrationBuilder.CreateIndex(
                name: "IX_PacketData_PacketReportId",
                table: "PacketData",
                column: "PacketReportId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PacketReport_GatewayId",
                table: "PacketReport",
                column: "GatewayId");

            migrationBuilder.CreateIndex(
                name: "IX_PacketReport_PacketId",
                table: "PacketReport",
                column: "PacketId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CommunityArea");

            migrationBuilder.DropTable(
                name: "PacketData");

            migrationBuilder.DropTable(
                name: "PacketReport");

            migrationBuilder.DropTable(
                name: "Packet");

            migrationBuilder.DropTable(
                name: "Node");

            migrationBuilder.DropTable(
                name: "Community");
        }
    }
}
