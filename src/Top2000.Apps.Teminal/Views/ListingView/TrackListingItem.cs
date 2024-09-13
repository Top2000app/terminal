namespace Top2000.Apps.Teminal.Views.ListingView;

public class TrackListingItem
{
    public int? TrackId { get; init; }
    public required Type ItemType { get; init; }
    public required string Content { get; init; }
    public override string ToString() => this.Content;

    public enum Type
    {
        Group,
        Track,
        Artist
    }
}
