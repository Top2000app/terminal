using Top2000.Features.TrackInformation;

namespace Top2000.Apps.Teminal.Views.TrackInformation;

public class ListingInformationSource : ITableSource
{
    private readonly SortedSet<ListingInformation> listings;
    private readonly object[][] rowcolumn;

    public ListingInformationSource(TrackDetails trackDetails)
    {
        listings = trackDetails.Listings;

        rowcolumn = listings
            .Select(x =>
            {
                var offset = x.Status switch
                {
                    ListingStatus.NotAvailable => "",
                    ListingStatus.NotListed => "",
                    ListingStatus.Increased => $"{Symbols.Up} {Math.Abs(x.Offset!.Value)}",
                    ListingStatus.New => Symbols.New,
                    ListingStatus.Unchanged => Symbols.Same,
                    ListingStatus.Back => Symbols.BackInList,
                    ListingStatus.Decreased => $"{Symbols.Down} {Math.Abs(x.Offset!.Value)}",
                    _ => "\uFAAF"
                };

                return new object[] { x.Edition.ToString().PadRight(5, ' '), x.Position?.ToString().PadRight(5, ' ') ?? "-", offset };
            })
            .ToArray();
    }



    public object this[int row, int col] => rowcolumn[row][col];

    public string[] ColumnNames => ["", "", ""];

    public int Columns => ColumnNames.Length;

    public int Rows => listings.Count;
}
