using Terminal.Gui;
using Top2000.Features.AllEditions;

namespace Top2000.Apps.Teminal.Views.SelectEdition;

public class EditionsDataSource : ITableSource
{
    private readonly string[][] items;

    public EditionsDataSource(SortedSet<Edition> editions)
    {
        this.Columns = 5;
        this.Rows = editions.Count / this.Columns;

        this.items = new string[this.Rows][];
        for (var row = 0; row < this.Rows; row++)
        {
            this.items[row] = new string[this.Columns];
        }

        var index = 0;
        foreach (var edition in editions.Reverse())
        {
            var row = this.Rows - (index / this.Columns) - 1;
            var col = index % this.Columns;
            this.items[row][col] = $" {edition.Year} ";

            index++;
        }
    }

    public object this[int row, int col] => this.items[row][col];

    public string[] ColumnNames => ["", "", "", "", ""];

    public int Columns { get; }

    public int Rows { get; }
}
