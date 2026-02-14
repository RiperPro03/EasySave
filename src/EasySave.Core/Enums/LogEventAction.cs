namespace EasySave.Core.Enums;

/// <summary>
/// Action performed within a log category.
/// </summary>
public enum LogEventAction
{
    Unknown,
    Create,
    Update,
    Delete,
    Save,
    Transfer,
    Skip,
    DirectoryCreated,
    Summary
}
