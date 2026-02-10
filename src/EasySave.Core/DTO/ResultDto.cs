namespace EasySave.Core.DTO;

/// <summary>
/// Standard DTO for operation results (success/failure).
/// Centralizes messages and error codes without exposing domain logic.
/// </summary>
public class ResultDto
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? ErrorCode { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ResultDto"/> class.
    /// </summary>
    public ResultDto() { }

    /// <summary>
    /// Creates a successful result.
    /// </summary>
    /// <param name="message">Optional success message.</param>
    /// <returns>A success result DTO.</returns>
    public static ResultDto Ok(string message = "")
        => new ResultDto { Success = true, Message = message };

    /// <summary>
    /// Creates a failure result.
    /// </summary>
    /// <param name="message">Failure message.</param>
    /// <param name="errorCode">Optional error code.</param>
    /// <returns>A failure result DTO.</returns>
    public static ResultDto Fail(string message, string? errorCode = null)
        => new ResultDto { Success = false, Message = message, ErrorCode = errorCode };
}
