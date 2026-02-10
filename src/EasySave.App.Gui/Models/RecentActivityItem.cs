namespace EasySave.App.Gui.Models;

/// <summary>
/// Presentation model for a single recent activity entry.
/// </summary>
public sealed class RecentActivityItem
{
    /// <summary>Primary label for the activity.</summary>
    public string Title { get; }
    /// <summary>Secondary details shown under the title.</summary>
    public string Subtitle { get; }
    /// <summary>Formatted timestamp string.</summary>
    public string Timestamp { get; }
    /// <summary>Glyph shown in the badge.</summary>
    public string BadgeGlyph { get; }
    /// <summary>Badge background color.</summary>
    public string BadgeBackground { get; }
    /// <summary>Badge foreground color.</summary>
    public string BadgeForeground { get; }

    /// <summary>
    /// Creates an immutable activity item for the dashboard.
    /// </summary>
    /// <param name="title">Primary label for the activity.</param>
    /// <param name="subtitle">Secondary details for the activity.</param>
    /// <param name="timestamp">Formatted timestamp string.</param>
    /// <param name="badgeGlyph">Glyph shown in the badge.</param>
    /// <param name="badgeBackground">Badge background color.</param>
    /// <param name="badgeForeground">Badge foreground color.</param>
    public RecentActivityItem(
        string title,
        string subtitle,
        string timestamp,
        string badgeGlyph,
        string badgeBackground,
        string badgeForeground)
    {
        Title = title;
        Subtitle = subtitle;
        Timestamp = timestamp;
        BadgeGlyph = badgeGlyph;
        BadgeBackground = badgeBackground;
        BadgeForeground = badgeForeground;
    }
}
