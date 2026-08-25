namespace Mango.Models;

public enum PlayerNoteVote
{
    Like,
    Dislike,
}

public static class PlayerNotesMatch
{
    public static bool IsUsableMatchId(string? matchId)
    {
        var id = matchId?.Trim() ?? string.Empty;
        return id.Length > 0 && id != "0";
    }

    public static string NormalizeMatchId(string? matchId)
    {
        var id = matchId?.Trim() ?? string.Empty;
        return IsUsableMatchId(id) ? id : string.Empty;
    }
}
