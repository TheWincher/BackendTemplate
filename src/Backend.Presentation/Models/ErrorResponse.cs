namespace Backend.Presentation.Models;

public sealed record ErrorResponse(
    string Type,
    string Title,
    int Status,
    IEnumerable<string> Errors);