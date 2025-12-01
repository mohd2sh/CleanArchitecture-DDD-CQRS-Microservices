using CleanArchitecture.Core.Application.Abstractions.Common;

namespace Technicians.Service.Application.Technicians;

public static class TechnicianErrors
{
    public static readonly Error NotFound = Error.NotFound("Technician.NotFound", "Technician not found.");
}


