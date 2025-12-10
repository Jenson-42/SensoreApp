using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SensoreApp.Migrations
{
    /// <inheritdoc />
    public partial class SyncSensorDeviceModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "SensorDevices");
        }
    }
}
