namespace EasySave.Core.DTO;

/// <summary>
/// DTO standard de retour d'opération (succès / échec).
/// Centralise les messages et codes d'erreur sans exposer la logique métier.
/// </summary>
public class ResultDto
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? ErrorCode { get; set; }

    public ResultDto() { }

    public static ResultDto Ok(string message = "")
        => new ResultDto { Success = true, Message = message };

    public static ResultDto Fail(string message, string? errorCode = null)
        => new ResultDto { Success = false, Message = message, ErrorCode = errorCode };
}