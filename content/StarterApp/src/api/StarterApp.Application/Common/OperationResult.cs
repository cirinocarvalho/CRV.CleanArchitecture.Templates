namespace StarterApp.Application.Common;

/// <summary>
/// Generic operation result for service-layer responses.
/// </summary>
public class OperationResult
{
    public bool Success { get; set; } = true;

    public List<string> Messages { get; private set; } = [];

    public void AddMessage(string message) => Messages.Add(message);

    public void ClearMessages() => Messages.Clear();

    public void SetUnauthorized()
    {
        Success = false;
        AddMessage("Unauthorized");
    }
}
