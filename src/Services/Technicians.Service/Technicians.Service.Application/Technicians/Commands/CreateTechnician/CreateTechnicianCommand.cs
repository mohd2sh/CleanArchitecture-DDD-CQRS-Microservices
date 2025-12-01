using CleanArchitecture.Core.Application.Abstractions.Common;

namespace Technicians.Service.Application.Technicians.Commands.CreateTechnician;

public sealed record CreateTechnicianCommand(string Name, string SkillLevelName, int SkillLevelRank) : ICommand<Result<Guid>>;


