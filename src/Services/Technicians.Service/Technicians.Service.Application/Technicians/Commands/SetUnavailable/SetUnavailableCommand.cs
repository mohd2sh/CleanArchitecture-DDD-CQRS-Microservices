using CleanArchitecture.Core.Application.Abstractions.Common;

namespace Technicians.Service.Application.Technicians.Commands.SetUnavailable;

public sealed record SetUnavailableCommand(Guid TechnicianId) : ICommand<Result>;


