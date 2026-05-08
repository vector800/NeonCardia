using System.Collections.Generic;

public sealed class BattleLog
{
    private readonly int maxLines;
    private readonly List<string> lines = new List<string>();

    public BattleLog(int maxLines)
    {
        this.maxLines = maxLines;
    }

    public string DisplayText { get { return string.Join("\n", lines.ToArray()); } }

    public void Add(string message)
    {
        lines.Add(message);
        if (lines.Count > maxLines)
        {
            lines.RemoveAt(0);
        }
    }
}
