using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DGC.Sample.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class SeedProducts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "products",
                columns: new[] { "Id", "Name", "UnitPrice", "AvailableStock" },
                values: new object[,]
                {
                    { new Guid("d290f1ee-6c54-4b01-90e6-d701748f0851"), "Laptop", 1500m, 50 },
                    { new Guid("d290f1ee-6c54-4b01-90e6-d701748f0852"), "Mouse", 25m, 200 },
                    { new Guid("d290f1ee-6c54-4b01-90e6-d701748f0853"), "Keyboard", 100m, 100 }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "products",
                keyColumn: "Id",
                keyValue: new Guid("d290f1ee-6c54-4b01-90e6-d701748f0851"));

            migrationBuilder.DeleteData(
                table: "products",
                keyColumn: "Id",
                keyValue: new Guid("d290f1ee-6c54-4b01-90e6-d701748f0852"));

            migrationBuilder.DeleteData(
                table: "products",
                keyColumn: "Id",
                keyValue: new Guid("d290f1ee-6c54-4b01-90e6-d701748f0853"));
        }
    }
}
