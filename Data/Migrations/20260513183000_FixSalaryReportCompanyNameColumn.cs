using Jobalatica.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Jobalatica.Data.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20260513183000_FixSalaryReportCompanyNameColumn")]
    public class FixSalaryReportCompanyNameColumn : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CompanyName",
                table: "SalaryReports",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CompanyName",
                table: "SalaryReports");
        }
    }
}
