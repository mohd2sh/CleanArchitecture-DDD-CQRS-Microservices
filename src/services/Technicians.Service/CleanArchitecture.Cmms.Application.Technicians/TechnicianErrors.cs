using CleanArchitecture.Core.Application.Abstractions.Common;

namespace CleanArchitecture.Cmms.Application.Technicians;

public static class TechnicianErrors
{
    public static readonly Error NotFound = Error.NotFound("Technician.NotFound", "Technician not found.");
}

