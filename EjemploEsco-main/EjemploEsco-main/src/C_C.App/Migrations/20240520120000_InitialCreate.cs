using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace C_C.App.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Email = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    PasswordHash = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    DisplayName = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    DateOfBirth = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsBlocked = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Matches",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    UserIdA = table.Column<Guid>(type: "TEXT", nullable: false),
                    UserIdB = table.Column<Guid>(type: "TEXT", nullable: false),
                    IsMutual = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ChatId = table.Column<Guid>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Matches", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Matches_Users_UserIdA",
                        column: x => x.UserIdA,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Matches_Users_UserIdB",
                        column: x => x.UserIdB,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Reportes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ReportanteId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ReportadoId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Motivo = table.Column<string>(type: "TEXT", maxLength: 512, nullable: false),
                    CreadoEnUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reportes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Reportes_Users_ReportadoId",
                        column: x => x.ReportadoId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Reportes_Users_ReportanteId",
                        column: x => x.ReportanteId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Perfiles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    UserId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Bio = table.Column<string>(type: "TEXT", maxLength: 512, nullable: false),
                    Ciudad = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    FotoUrl = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    Intereses = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Perfiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Perfiles_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Preferencias",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    UserId = table.Column<Guid>(type: "TEXT", nullable: false),
                    EdadMinima = table.Column<int>(type: "INTEGER", nullable: false),
                    EdadMaxima = table.Column<int>(type: "INTEGER", nullable: false),
                    GeneroBuscado = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    DistanciaMaximaKm = table.Column<double>(type: "REAL", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Preferencias", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Preferencias_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Chats",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    MatchId = table.Column<Guid>(type: "TEXT", nullable: false),
                    UserIdA = table.Column<Guid>(type: "TEXT", nullable: false),
                    UserIdB = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Chats", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Chats_Matches_MatchId",
                        column: x => x.MatchId,
                        principalTable: "Matches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Mensajes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ChatId = table.Column<Guid>(type: "TEXT", nullable: false),
                    RemitenteId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Contenido = table.Column<string>(type: "TEXT", maxLength: 1024, nullable: false),
                    EnviadoEnUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Leido = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Mensajes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Mensajes_Chats_ChatId",
                        column: x => x.ChatId,
                        principalTable: "Chats",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Chats_MatchId",
                table: "Chats",
                column: "MatchId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Matches_UserIdA_UserIdB",
                table: "Matches",
                columns: new[] { "UserIdA", "UserIdB" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Mensajes_ChatId",
                table: "Mensajes",
                column: "ChatId");

            migrationBuilder.CreateIndex(
                name: "IX_Perfiles_UserId",
                table: "Perfiles",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Preferencias_UserId",
                table: "Preferencias",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Reportes_ReportadoId",
                table: "Reportes",
                column: "ReportadoId");

            migrationBuilder.CreateIndex(
                name: "IX_Reportes_ReportanteId",
                table: "Reportes",
                column: "ReportanteId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "Mensajes");
            migrationBuilder.DropTable(name: "Preferencias");
            migrationBuilder.DropTable(name: "Reportes");
            migrationBuilder.DropTable(name: "Chats");
            migrationBuilder.DropTable(name: "Perfiles");
            migrationBuilder.DropTable(name: "Matches");
            migrationBuilder.DropTable(name: "Users");
        }
    }
}
