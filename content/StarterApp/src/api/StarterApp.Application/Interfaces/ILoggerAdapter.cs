namespace StarterApp.Application.Interfaces;

/// <summary>
/// Defines the contract for a logger adapter.
/// This interface abstracts the ASP.NET Core logging types, allowing for easier testing and decoupling.
/// </summary>
/// <typeparam name="T">The type of the logger adapter.</typeparam>
public interface ILoggerAdapter<T>
{
    /// <summary>
    /// Logs a debug message.
    /// </summary>
    /// <param name="message">The message to log.</param>
    /// <param name="args">The arguments to format the message with.</param>
    void LogDebug(string message, params object[] args);

    /// <summary>
    /// Logs an error message.
    /// </summary>
    /// <param name="message">The message to log.</param>
    /// <param name="args">The arguments to format the message with.</param>
    void LogError(string message, params object[] args);

    /// <summary>
    /// Logs an information message.
    /// </summary>
    /// <param name="message">The message to log.</param>
    /// <param name="args">The arguments to format the message with.</param>
    void LogInformation(string message, params object[] args);

    /// <summary>
    /// Logs a trace message.
    /// </summary>
    /// <param name="message">The message to log.</param>
    /// <param name="args">The arguments to format the message with.</param>
    void LogTrace(string message, params object[] args);

    /// <summary>
    /// Logs a warning message.
    /// </summary>
    /// <param name="message">The message to log.</param>
    /// <param name="args">The arguments to format the message with.</param>
    void LogWarning(string message, params object[] args);
}