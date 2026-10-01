namespace ListingSearch.Core.Search.Scoring;

// The best `count` listings in ranking order, without sorting the rest. A heap holds the best seen so far with the
// worst of them on top, so each listing costs O(log count) instead of a full O(n log n) sort.
// RankingComparer never ties two different listings (keys are unique), so the result is exactly the first
// `count` of a full sort.
public static class TopRanked
{
    private static readonly IComparer<ScoredListing> WorstFirst =
        Comparer<ScoredListing>.Create((x, y) => RankingComparer.Instance.Compare(y, x));

    public static IReadOnlyList<ScoredListing> Select(IEnumerable<ScoredListing> scored, int count)
    {
        if (count <= 0)
            return [];

        var best = new PriorityQueue<ScoredListing, ScoredListing>(WorstFirst);
        foreach (var listing in scored)
        {
            if (best.Count < count)
                best.Enqueue(listing, listing);
            else if (RankingComparer.Instance.Compare(listing, best.Peek()) < 0)
                best.DequeueEnqueue(listing, listing);
        }

        // The heap gives them worst first.
        var ranked = new ScoredListing[best.Count];
        for (var i = ranked.Length - 1; i >= 0; i--)
            ranked[i] = best.Dequeue();

        return ranked;
    }
}
