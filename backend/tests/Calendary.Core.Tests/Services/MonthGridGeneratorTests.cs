using System.Linq;
using Calendary.Core.Services;

namespace Calendary.Core.Tests.Services;

public class MonthGridGeneratorTests
{
    private readonly MonthGridGenerator _generator = new();

    [Fact]
    public void Generate_January2026_Has31DaysAndStartsOnThursday()
    {
        var grid = _generator.Generate(2026, 1, DayOfWeek.Monday);
        var monthDays = grid.Weeks.SelectMany(w => w).Where(cell => cell.IsCurrentMonth).ToArray();

        Assert.Equal(31, monthDays.Length);
        Assert.Equal(new DateTime(2026, 1, 1), monthDays.First().Date);
        Assert.Equal(DayOfWeek.Thursday, monthDays.First().Date.DayOfWeek);
    }

    [Fact]
    public void Generate_February2026_Has28Days()
    {
        var grid = _generator.Generate(2026, 2, DayOfWeek.Monday);
        var monthDays = grid.Weeks.SelectMany(w => w).Where(cell => cell.IsCurrentMonth).ToArray();

        Assert.Equal(28, monthDays.Length);
        Assert.Equal(new DateTime(2026, 2, 28), monthDays.Last().Date);
    }

    [Theory]
    [InlineData(DayOfWeek.Monday, 3)]
    [InlineData(DayOfWeek.Sunday, 4)]
    public void Generate_RespectsStartDay(DayOfWeek startDay, int expectedIndex)
    {
        var grid = _generator.Generate(2026, 1, startDay);
        var firstWeek = grid.Weeks.First();
        var januaryFirstIndex = firstWeek
            .Select((cell, index) => (cell, index))
            .First(pair => pair.cell.IsCurrentMonth && pair.cell.Date.Day == 1)
            .index;

        Assert.Equal(expectedIndex, januaryFirstIndex);
    }

    [Fact]
    public void GenerateYear_Returns12Months()
    {
        var grids = _generator.GenerateYear(2026, DayOfWeek.Monday);

        Assert.Equal(12, grids.Count);
        Assert.Equal(Enumerable.Range(1, 12), grids.Select(g => g.Month));
    }
}
