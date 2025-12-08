namespace CleanArchitecture.Cmms.Api.Technicians.Controllers.V1.Requests.Technicians;

public sealed record AddCertificationRequest(
    string Code,
    DateTime IssuedOn,
    DateTime? ExpiresOn);

