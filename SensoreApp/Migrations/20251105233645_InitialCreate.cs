using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SensoreApp.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FrameMetrics",
                columns: table => new
                {
                    FrameMetricID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FrameID = table.Column<int>(type: "int", nullable: false),
                    PeakPressureIndex = table.Column<decimal>(type: "decimal(10,4)", nullable: false),
                    ContactAreaPercent = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    COV = table.Column<decimal>(type: "decimal(10,4)", nullable: false),
                    ComputedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FrameMetrics", x => x.FrameMetricID);
                });

            migrationBuilder.CreateTable(
                name: "Reports",
                columns: table => new
                {
                    ReportID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RequestedBy = table.Column<int>(type: "int", nullable: false),
                    DateFrom = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DateTo = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ComparisonDateFrom = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ComparisonDateTo = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UserID = table.Column<int>(type: "int", nullable: false),
                    FilePath = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    GeneratedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    ReportType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reports", x => x.ReportID);
                });

            migrationBuilder.CreateTable(
                name: "ReportFrames",
                columns: table => new
                {
                    ReportFrameID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ReportID = table.Column<int>(type: "int", nullable: false),
                    FrameID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReportFrames", x => x.ReportFrameID);
                    table.ForeignKey(
                        name: "FK_ReportFrames_Reports_ReportID",
                        column: x => x.ReportID,
                        principalTable: "Reports",
                        principalColumn: "ReportID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ReportMetrics",
                columns: table => new
                {
                    ReportMetricID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ReportID = table.Column<int>(type: "int", nullable: false),
                    MetricName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    MetricValue = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    ComparisonValue = table.Column<decimal>(type: "decimal(10,2)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReportMetrics", x => x.ReportMetricID);
                    table.ForeignKey(
                        name: "FK_ReportMetrics_Reports_ReportID",
                        column: x => x.ReportID,
                        principalTable: "Reports",
                        principalColumn: "ReportID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ReportFrames_ReportID",
                table: "ReportFrames",
                column: "ReportID");

            migrationBuilder.CreateIndex(
                name: "IX_ReportMetrics_ReportID",
                table: "ReportMetrics",
                column: "ReportID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FrameMetrics");

            migrationBuilder.DropTable(
                name: "ReportFrames");

            migrationBuilder.DropTable(
                name: "ReportMetrics");

            migrationBuilder.DropTable(
                name: "Reports");
        }
    }
}
