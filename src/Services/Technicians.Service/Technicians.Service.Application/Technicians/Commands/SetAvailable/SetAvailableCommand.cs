using CleanArchitecture.Core.Application.Abstractions.Common;

namespace Technicians.Service.Application.Technicians.Commands.SetAvailable;

public sealed record SetAvailableCommand(Guid TechnicianId) : ICommand<Result>;


