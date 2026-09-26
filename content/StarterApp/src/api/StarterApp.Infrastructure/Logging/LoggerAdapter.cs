using StarterApp.Application.Interfaces;
using Microsoft.Extensions.Logging;


namespace StarterApp.Infrastructure.Logging;

/// <summary>
/// Provides a logger adapter for the specified type.
/// </summary>
/// <typeparam name="T">The type of the logger adapter.</typeparam>
/// <param name="loggerFactory">The logger factory to create loggers.</param>
public class LoggerAdapter<T>(ILoggerFactory loggerFactory) : ILoggerAdapter<T>
{
    private readonly ILogger<T> _logger = loggerFactory.CreateLogger<T>();

    /// <summary>
    /// Logs a debug message.
    /// </summary>
    /// <param name="message">The message to log.</param>
    /// <param name="args">The arguments to format the message with.</param>
    public void LogDebug(string message, params object[] args)
    {
        _logger.LogDebug(message, args);
    }

    /// <summary>
    /// Logs an error message.
    /// </summary>
    /// <param name="message">The message to log.</param>
    /// <param name="args">The arguments to format the message with.</param>
    public void LogError(string message, params object[] args)
    {
        _logger.LogError(message, args);
    }

    /// <summary>
    /// Logs an information message.
    /// </summary>
    /// <param name="message">The message to log.</param>
    /// <param name="args">The arguments to format the message with.</param>
    public void LogInformation(string message, params object[] args)
    {
        _logger.LogInformation(message, args);
    }

    /// <summary>
    /// Logs a trace message.
    /// </summary>
    /// <param name="message">The message to log.</param>
    /// <param name="args">The arguments to format the message with.</param>
    public void LogTrace(string message, params object[] args)
    {
        _logger.LogTrace(message, args);
    }

    /// <summary>
    /// Logs a warning message.
    /// </summary>
    /// <param name="message">The message to log.</param>
    /// <param name="args">The arguments to format the message with.</param>
    public void LogWarning(string message, params object[] args)
    {
        _logger.LogWarning(message, args);
    }
}
