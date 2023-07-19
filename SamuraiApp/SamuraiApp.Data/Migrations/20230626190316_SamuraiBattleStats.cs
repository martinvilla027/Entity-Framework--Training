using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SamuraiApp.Data.Migrations
{
    /// <inheritdoc />
    public partial class SamuraiBattleStats : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                @" CREATE FUNCTION[dbo].[EarliestBattleFoughtBySamurai](@samuraiId int)
                RETURNS char(30) AS
                BEGIN
                  DECLARE @ret char(30)
                  SELECT TOP 1 @ret = Name
                  FROM Battles
                  WHERE Battles.BattleId IN(SELECT BattleId
                                     FROM BattleSamurai
                                    WHERE SamuraiId = @samuraiId)
                  ORDER BY StartDate
                  RETURN @ret
                END");

            migrationBuilder.Sql(
                @"CREATE VIEW dbo.SamuraiBattleStats
                AS
                SELECT dbo.Samurais.Name,
                COUNT(dbo.BattleSamurai.BattleId) AS NumberOfBattles,
                        dbo.EarliestBattleFoughtBySamurai(MIN(dbo.Samurais.Id)) 
		  	        AS EarliestBattle
                FROM dbo.BattleSamurai INNER JOIN
                   dbo.Samurais ON dbo.BattleSamurai.SamuraiId = dbo.Samurais.Id
                GROUP BY dbo.Samurais.Name, dbo.BattleSamurai.SamuraiId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP VIEW dbo.SamuraiBattleStats");
            migrationBuilder.Sql("DROP FUNCTION dbo.EarliestBattleFoughtBySamurai");
        }
    }
}
