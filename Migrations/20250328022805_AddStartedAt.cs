using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pomodoro.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddStartedAt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
     name: "StartedAt",
     table: "PomodoroSessions",
     type: "timestamp with time zone",
     nullable: true
 );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
         name: "StartedAt",
         table: "PomodoroSessions"
     );
        }

    }
}
