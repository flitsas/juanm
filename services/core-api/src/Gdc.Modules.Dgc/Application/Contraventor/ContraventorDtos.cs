namespace Gdc.Modules.Dgc.Application.Contraventor;

public sealed record UpsertContraventorRequest(string Nombre, string Documento, string? Correo);

public sealed record ContraventorResponse(
    Guid Id,
    Guid ComparendoId,
    string Nombre,
    string Documento,
    string? Correo,
    bool AsociacionAutomatica);
