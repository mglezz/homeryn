namespace Homeryn.Shared.Contracts;

public sealed record ResponseMessage(string Status, string? Error = null);