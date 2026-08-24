namespace Apem.Services;

/// <summary>
/// Resolves a display medal from confirmed <c>rank_tier</c>, OpenDota MMR estimate, or lobby average.
/// MMR→medal bands are approximate (~770 MMR per medal).
/// </summary>
public static class RankEstimator
{
    private const int MedalWidthMmr = 770;
    private const int StarWidthMmr = 154;
    private const int ImmortalFloorMmr = 5420;

    public static ResolvedRank Resolve(
        int? rankTier,
        int? leaderboardRank,
        int? mmrEstimate,
        int? lobbyAverageTiers)
    {
        if (rankTier is > 0)
        {
            return new ResolvedRank(
                rankTier.Value,
                OpenDotaService.FormatRankTier(rankTier, leaderboardRank),
                IsEstimated: false);
        }

        if (mmrEstimate is > 0)
        {
            var fromMmr = RankTierFromMmr(mmrEstimate.Value);
            if (fromMmr is > 0)
            {
                var name = OpenDotaService.FormatRankTier(fromMmr);
                return new ResolvedRank(
                    fromMmr.Value,
                    $"Estimated: {name} (~{mmrEstimate.Value:N0} MMR)",
                    IsEstimated: true);
            }
        }

        if (lobbyAverageTiers is > 0)
        {
            var name = OpenDotaService.FormatRankTier(lobbyAverageTiers);
            return new ResolvedRank(
                lobbyAverageTiers.Value,
                $"Lobby avg: ~{name}",
                IsEstimated: true);
        }

        return new ResolvedRank(null, string.Empty, IsEstimated: false);
    }

    public static int? AverageRankTier(IEnumerable<int> confirmedTiers)
    {
        var mmrs = confirmedTiers
            .Where(static tier => tier > 0)
            .Select(ApproximateMmrFromRankTier)
            .Where(static mmr => mmr.HasValue)
            .Select(static mmr => mmr!.Value)
            .ToList();

        if (mmrs.Count == 0)
        {
            return null;
        }

        return RankTierFromMmr(mmrs.Average());
    }

    public static int? RankTierFromMmr(double mmr)
    {
        if (mmr < 0)
        {
            return null;
        }

        if (mmr >= ImmortalFloorMmr)
        {
            return 80;
        }

        var clamped = (int)Math.Min(Math.Floor(mmr), ImmortalFloorMmr - 1);
        var medal = Math.Clamp(clamped / MedalWidthMmr + 1, 1, 7);
        var stars = Math.Clamp(clamped % MedalWidthMmr / StarWidthMmr + 1, 1, 5);
        return medal * 10 + stars;
    }

    public static double? ApproximateMmrFromRankTier(int rankTier)
    {
        var medal = rankTier / 10;
        var stars = rankTier % 10;
        if (medal <= 0)
        {
            return null;
        }

        if (medal >= 8)
        {
            return ImmortalFloorMmr + 300;
        }

        stars = Math.Clamp(stars <= 0 ? 1 : stars, 1, 5);
        return (medal - 1) * MedalWidthMmr + (stars - 1) * StarWidthMmr + StarWidthMmr / 2.0;
    }
}

public readonly record struct ResolvedRank(int? RankTier, string Label, bool IsEstimated);
