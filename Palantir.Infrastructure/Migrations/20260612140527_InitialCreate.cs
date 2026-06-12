using System;
using Microsoft.EntityFrameworkCore.Migrations;
using NetTopologySuite.Geometries;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Palantir.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:postgis", ",,");

            migrationBuilder.CreateTable(
                name: "sides",
                columns: table => new
                {
                    side_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    title = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("sides_pkey", x => x.side_id);
                });

            migrationBuilder.CreateTable(
                name: "wars",
                columns: table => new
                {
                    war_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    title = table.Column<string>(type: "text", nullable: false),
                    start_date = table.Column<DateOnly>(type: "date", nullable: false),
                    end_date = table.Column<DateOnly>(type: "date", nullable: true),
                    summary = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("wars_pkey", x => x.war_id);
                });

            migrationBuilder.CreateTable(
                name: "theaters",
                columns: table => new
                {
                    theater_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    war_id = table.Column<int>(type: "integer", nullable: false),
                    title = table.Column<string>(type: "text", nullable: false),
                    summary = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("theaters_pkey", x => x.theater_id);
                    table.ForeignKey(
                        name: "theaters_war_id_fkey",
                        column: x => x.war_id,
                        principalTable: "wars",
                        principalColumn: "war_id");
                });

            migrationBuilder.CreateTable(
                name: "war_sides",
                columns: table => new
                {
                    war_side_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    war_id = table.Column<int>(type: "integer", nullable: false),
                    side_id = table.Column<int>(type: "integer", nullable: false),
                    joined_date = table.Column<DateOnly>(type: "date", nullable: true),
                    out_date = table.Column<DateOnly>(type: "date", nullable: true),
                    note = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("war_sides_pkey", x => x.war_side_id);
                    table.ForeignKey(
                        name: "war_sides_side_id_fkey",
                        column: x => x.side_id,
                        principalTable: "sides",
                        principalColumn: "side_id");
                    table.ForeignKey(
                        name: "war_sides_war_id_fkey",
                        column: x => x.war_id,
                        principalTable: "wars",
                        principalColumn: "war_id");
                });

            migrationBuilder.CreateTable(
                name: "operations",
                columns: table => new
                {
                    operation_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    theater_id = table.Column<int>(type: "integer", nullable: false),
                    title = table.Column<string>(type: "text", nullable: false),
                    start_date = table.Column<DateOnly>(type: "date", nullable: false),
                    end_date = table.Column<DateOnly>(type: "date", nullable: false),
                    summary = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("operations_pkey", x => x.operation_id);
                    table.ForeignKey(
                        name: "operations_theater_id_fkey",
                        column: x => x.theater_id,
                        principalTable: "theaters",
                        principalColumn: "theater_id");
                });

            migrationBuilder.CreateTable(
                name: "armies",
                columns: table => new
                {
                    army_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    war_side_id = table.Column<int>(type: "integer", nullable: false),
                    name_arm = table.Column<string>(type: "text", nullable: false),
                    type_arm = table.Column<string>(type: "text", nullable: true),
                    start_date = table.Column<DateOnly>(type: "date", nullable: true),
                    end_date = table.Column<DateOnly>(type: "date", nullable: true),
                    summary = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("armies_pkey", x => x.army_id);
                    table.ForeignKey(
                        name: "armies_war_side_id_fkey",
                        column: x => x.war_side_id,
                        principalTable: "war_sides",
                        principalColumn: "war_side_id");
                });

            migrationBuilder.CreateTable(
                name: "control_zones",
                columns: table => new
                {
                    control_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    war_id = table.Column<int>(type: "integer", nullable: false),
                    war_side_id = table.Column<int>(type: "integer", nullable: false),
                    date_control = table.Column<DateOnly>(type: "date", nullable: false),
                    precision_control = table.Column<string>(type: "text", nullable: false),
                    geom = table.Column<MultiPolygon>(type: "geometry(MultiPolygon,4326)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("control_zones_pkey", x => x.control_id);
                    table.ForeignKey(
                        name: "control_zones_war_id_fkey",
                        column: x => x.war_id,
                        principalTable: "wars",
                        principalColumn: "war_id");
                    table.ForeignKey(
                        name: "control_zones_war_side_id_fkey",
                        column: x => x.war_side_id,
                        principalTable: "war_sides",
                        principalColumn: "war_side_id");
                });

            migrationBuilder.CreateTable(
                name: "event",
                columns: table => new
                {
                    event_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    war_id = table.Column<int>(type: "integer", nullable: false),
                    operation_id = table.Column<int>(type: "integer", nullable: true),
                    war_side_id = table.Column<int>(type: "integer", nullable: true),
                    title = table.Column<string>(type: "text", nullable: false),
                    text_ev = table.Column<string>(type: "text", nullable: true),
                    type_ev = table.Column<string>(type: "text", nullable: true),
                    date_ev = table.Column<DateOnly>(type: "date", nullable: false),
                    coordinate = table.Column<Point>(type: "geometry(Point,4326)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("events_pkey", x => x.event_id);
                    table.ForeignKey(
                        name: "events_operation_id_fkey",
                        column: x => x.operation_id,
                        principalTable: "operations",
                        principalColumn: "operation_id");
                    table.ForeignKey(
                        name: "events_war_id_fkey",
                        column: x => x.war_id,
                        principalTable: "wars",
                        principalColumn: "war_id");
                    table.ForeignKey(
                        name: "events_war_side_id_fkey",
                        column: x => x.war_side_id,
                        principalTable: "war_sides",
                        principalColumn: "war_side_id");
                });

            migrationBuilder.CreateTable(
                name: "operation_sides",
                columns: table => new
                {
                    operation_id = table.Column<int>(type: "integer", nullable: false),
                    war_side_id = table.Column<int>(type: "integer", nullable: false),
                    role_side = table.Column<string>(type: "text", nullable: true),
                    note = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("operation_sides_pkey", x => new { x.operation_id, x.war_side_id });
                    table.ForeignKey(
                        name: "operation_sides_operation_id_fkey",
                        column: x => x.operation_id,
                        principalTable: "operations",
                        principalColumn: "operation_id");
                    table.ForeignKey(
                        name: "operation_sides_war_side_id_fkey",
                        column: x => x.war_side_id,
                        principalTable: "war_sides",
                        principalColumn: "war_side_id");
                });

            migrationBuilder.CreateTable(
                name: "army_positions",
                columns: table => new
                {
                    army_position_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    army_id = table.Column<int>(type: "integer", nullable: false),
                    date_position = table.Column<DateOnly>(type: "date", nullable: false),
                    coordinate = table.Column<Point>(type: "geometry(Point,4326)", nullable: false),
                    note = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("army_positions_pkey", x => x.army_position_id);
                    table.ForeignKey(
                        name: "army_positions_army_id_fkey",
                        column: x => x.army_id,
                        principalTable: "armies",
                        principalColumn: "army_id");
                });

            migrationBuilder.CreateIndex(
                name: "idx_armies_war_side_id",
                table: "armies",
                column: "war_side_id");

            migrationBuilder.CreateIndex(
                name: "army_positions_army_id_date_position_key",
                table: "army_positions",
                columns: new[] { "army_id", "date_position" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_army_positions_date",
                table: "army_positions",
                column: "date_position");

            migrationBuilder.CreateIndex(
                name: "idx_army_positions_geom",
                table: "army_positions",
                column: "coordinate")
                .Annotation("Npgsql:IndexMethod", "gist");

            migrationBuilder.CreateIndex(
                name: "idx_control_zones_geom",
                table: "control_zones",
                column: "geom")
                .Annotation("Npgsql:IndexMethod", "gist");

            migrationBuilder.CreateIndex(
                name: "idx_control_zones_war_date",
                table: "control_zones",
                columns: new[] { "war_id", "date_control" });

            migrationBuilder.CreateIndex(
                name: "IX_control_zones_war_side_id",
                table: "control_zones",
                column: "war_side_id");

            migrationBuilder.CreateIndex(
                name: "idx_events_geom",
                table: "event",
                column: "coordinate")
                .Annotation("Npgsql:IndexMethod", "gist");

            migrationBuilder.CreateIndex(
                name: "idx_events_war_date",
                table: "event",
                columns: new[] { "war_id", "date_ev" });

            migrationBuilder.CreateIndex(
                name: "IX_event_operation_id",
                table: "event",
                column: "operation_id");

            migrationBuilder.CreateIndex(
                name: "IX_event_war_side_id",
                table: "event",
                column: "war_side_id");

            migrationBuilder.CreateIndex(
                name: "idx_operation_sides_war_side_id",
                table: "operation_sides",
                column: "war_side_id");

            migrationBuilder.CreateIndex(
                name: "idx_operations_theater_id",
                table: "operations",
                column: "theater_id");

            migrationBuilder.CreateIndex(
                name: "idx_theaters_war_id",
                table: "theaters",
                column: "war_id");

            migrationBuilder.CreateIndex(
                name: "idx_war_sides_side_id",
                table: "war_sides",
                column: "side_id");

            migrationBuilder.CreateIndex(
                name: "idx_war_sides_war_id",
                table: "war_sides",
                column: "war_id");

            migrationBuilder.CreateIndex(
                name: "war_sides_war_id_side_id_key",
                table: "war_sides",
                columns: new[] { "war_id", "side_id" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "army_positions");

            migrationBuilder.DropTable(
                name: "control_zones");

            migrationBuilder.DropTable(
                name: "event");

            migrationBuilder.DropTable(
                name: "operation_sides");

            migrationBuilder.DropTable(
                name: "armies");

            migrationBuilder.DropTable(
                name: "operations");

            migrationBuilder.DropTable(
                name: "war_sides");

            migrationBuilder.DropTable(
                name: "theaters");

            migrationBuilder.DropTable(
                name: "sides");

            migrationBuilder.DropTable(
                name: "wars");
        }
    }
}
