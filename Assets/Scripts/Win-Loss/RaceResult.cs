// Simple enum listing the possible race outcomes
public enum RaceResult
{
    None,         // not finished yet
    Win,          // first hamster out
    LoseEscaped,  // escaped but not first
    LoseStuck     // DNF / timed out
}
