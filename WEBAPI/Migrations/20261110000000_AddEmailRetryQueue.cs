using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WEBAPI.Migrations;

public partial class AddEmailRetryQueue : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "HtmlBody",
            table: "EmailLog",
            type: "nvarchar(max)",
            nullable: true);

    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(name: "HtmlBody", table: "EmailLog");
    }
}
