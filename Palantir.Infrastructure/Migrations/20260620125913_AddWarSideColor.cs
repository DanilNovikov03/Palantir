using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Palantir.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddWarSideColor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                DO $$
                BEGIN
                    IF to_regclass('public.event') IS NOT NULL
                       AND to_regclass('public.events') IS NULL THEN
                        ALTER TABLE event RENAME TO events;
                    END IF;
                END $$;
                ALTER INDEX IF EXISTS "IX_event_war_side_id" RENAME TO "IX_events_war_side_id";
                ALTER INDEX IF EXISTS "IX_event_operation_id" RENAME TO "IX_events_operation_id";
                """);

            migrationBuilder.AddColumn<string>(
                name: "color_hex",
                table: "war_sides",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.Sql(
                """
                UPDATE war_sides AS ws
                SET color_hex = CASE s.title
                    WHEN 'СССР' THEN '#d32f2f'
                    WHEN 'Германия' THEN '#424242'
                    WHEN 'США' THEN '#1976d2'
                    WHEN 'Великобритания' THEN '#1565c0'
                    WHEN 'Союзники' THEN '#388e3c'
                    ELSE ws.color_hex
                END
                FROM sides AS s
                WHERE ws.side_id = s.side_id
                  AND ws.color_hex IS NULL;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "color_hex",
                table: "war_sides");

            migrationBuilder.Sql(
                """
                DO $$
                BEGIN
                    IF to_regclass('public.events') IS NOT NULL
                       AND to_regclass('public.event') IS NULL THEN
                        ALTER TABLE events RENAME TO event;
                    END IF;
                END $$;
                ALTER INDEX IF EXISTS "IX_events_war_side_id" RENAME TO "IX_event_war_side_id";
                ALTER INDEX IF EXISTS "IX_events_operation_id" RENAME TO "IX_event_operation_id";
                """);
        }
    }
}
