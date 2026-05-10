using System.Collections.Generic;

public sealed class DeckValidationResult
{
    public bool IsValid { get { return Errors.Count == 0; } }
    public List<string> Errors { get; private set; } = new List<string>();
    public int TotalCount { get; set; }
    public int NormalCount { get; set; }
    public int HighClassCount { get; set; }
    public int GigantCount { get; set; }

    public string Message
    {
        get
        {
            return IsValid ? "デッキは有効です" : string.Join("\n", Errors.ToArray());
        }
    }
}
