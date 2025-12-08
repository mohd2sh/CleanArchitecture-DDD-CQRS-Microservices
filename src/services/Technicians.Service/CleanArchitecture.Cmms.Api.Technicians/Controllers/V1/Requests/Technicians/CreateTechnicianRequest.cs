namespace CleanArchitecture.Cmms.Api.Technicians.Controllers.V1.Requests.Technicians;

public sealed record CreateTechnicianRequest(
    string Name,
    string SkillLevelName,
    int SkillLevelRank);

