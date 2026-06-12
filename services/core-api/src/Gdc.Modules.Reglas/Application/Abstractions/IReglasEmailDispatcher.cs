namespace Gdc.Modules.Reglas.Application.Abstractions;

public sealed record ReglasEmailDispatchRequest(
    Guid TenantId,
    Guid ComparendoId,
    string To,
    string Subject,
    string HtmlBody,
    byte[] PdfContent,
    string PdfFileName);

public sealed record ReglasEmailDispatchResult(bool Success, string? EmailSendRef, string? ErrorMessage);

public interface IReglasEmailDispatcher
{
    Task<ReglasEmailDispatchResult> DispatchAsync(
        ReglasEmailDispatchRequest request,
        CancellationToken cancellationToken);
}
