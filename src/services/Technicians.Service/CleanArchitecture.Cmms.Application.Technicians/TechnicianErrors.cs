using CleanArchitecture.Core.Application.Abstractions.Common;
using CleanArchitecture.Core.Domain.Abstractions.Attributes;

namespace CleanArchitecture.Cmms.Application.Technicians;

[ErrorCodeDefinition("Technician")]
public static class TechnicianErrors
{
    [ApplicationError]
    public static readonly Error NotFound = Error.NotFound(
        "Technician.NotFound",
        "Technician not found.");
}

