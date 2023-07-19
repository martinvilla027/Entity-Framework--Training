using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SamuraiApp.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddNewSprocs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                @"CREATE PROCEDURE dbo.SamuraisWhoSaidAWord
                    @text VARCHAR(20)
                    AS
                    SELECT      Samurais.Id, Samurais.Name
                    FROM        Samurais INNER JOIN
                                Quotes ON Samurais.Id = Quotes.SamuraiId
                    WHERE      (Quotes.Text LIKE '%'+@text+'%')");

            migrationBuilder.Sql(
                @"CREATE PROCEDURE dbo.DeleteQuotesForSamurai
                    @samuraiId int
                    AS
                    DELETE FROM Quotes
                    WHERE Quotes.SamuraiId=@samuraiId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
