namespace EasySave.App.Console.Models;

/// <summary>
/// Represents a menu option with an id and label.
/// </summary>
/// <param name="Id">The option identifier.</param>
/// <param name="Label">The option label.</param>
public sealed record MenuOption(int Id, string Label);
