using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Pomodoro.Api.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreatePostgre : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CyclesCompleted",
                table: "PomodoroSessions");

            migrationBuilder.DropColumn(
                name: "Date",
                table: "PomodoroSessions");

            migrationBuilder.DropColumn(
                name: "Playlist",
                table: "PomodoroSessions");

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "PomodoroSessions",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddColumn<DateTime>(
                name: "CompletedAt",
                table: "PomodoroSessions",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "PomodoroSessions",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CompletedAt",
                table: "PomodoroSessions");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "PomodoroSessions");

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "PomodoroSessions",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer")
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddColumn<int>(
                name: "CyclesCompleted",
                table: "PomodoroSessions",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Date",
                table: "PomodoroSessions",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Playlist",
                table: "PomodoroSessions",
                type: "TEXT",
                nullable: true);
        }
    }
}
