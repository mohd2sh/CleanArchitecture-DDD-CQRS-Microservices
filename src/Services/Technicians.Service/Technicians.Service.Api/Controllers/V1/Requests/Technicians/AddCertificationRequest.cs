namespace Technicians.Service.Api.Controllers.V1.Requests.Technicians;

public sealed record AddCertificationRequest(
    string Code,
    DateTime IssuedOn,
    DateTime? ExpiresOn);


