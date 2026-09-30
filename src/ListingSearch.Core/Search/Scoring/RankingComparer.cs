namespace ListingSearch.Core.Search.Scoring;

// Score descending (unrounded), then lower price, then key in ordinal order,
// so the same listings always come out in the same order.
public sealed class RankingComparer : IComparer<ScoredListing>
{
    public static readonly RankingComparer Instance = new();

    public int Compare(ScoredListing? x, ScoredListing? y)
    {
        if (ReferenceEquals(x, y))
            return 0;
        if (x is null)
            return 1;
        if (y is null)
            return -1;

        var byScore = y.Score.CompareTo(x.Score);
        if (byScore != 0)
            return byScore;

        var byPrice = x.Listing.Price.CompareTo(y.Listing.Price);
        if (byPrice != 0)
            return byPrice;

        return string.CompareOrdinal(x.Listing.Key, y.Listing.Key);
    }
}
