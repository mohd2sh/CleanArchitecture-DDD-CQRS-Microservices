namespace Technicians.Service.Api.Controllers.V1.Requests.Technicians;

public sealed record CreateTechnicianRequest(
    string Name,
    string SkillLevelName,
    int SkillLevelRank);


