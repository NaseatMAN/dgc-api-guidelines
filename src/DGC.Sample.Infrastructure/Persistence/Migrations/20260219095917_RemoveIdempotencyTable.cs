using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DGC.Sample.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RemoveIdempotencyTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP TABLE IF EXISTS idempotent_requests;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "idempotent_requests",
                columns: table => new
                {
                    IdempotencyKey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    RequestPath = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    ResponseBody = table.Column<string>(type: "text", nullable: false),
                    StatusCode = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_idempotent_requests", x => x.IdempotencyKey);
                });

            migrationBuilder.CreateIndex(
                name: "IX_idempotent_requests_IdempotencyKey",
                table: "idempotent_requests",
                column: "IdempotencyKey",
                unique: true);
        }
    }
}
