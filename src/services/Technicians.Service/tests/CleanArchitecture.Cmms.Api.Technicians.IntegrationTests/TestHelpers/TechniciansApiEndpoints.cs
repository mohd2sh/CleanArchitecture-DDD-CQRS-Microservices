namespace CleanArchitecture.Cmms.Api.Technicians.IntegrationTests.TestHelpers;

public static class TechniciansApiEndpoints
{
    private const string BaseV1 = "/api/v1";

    public static class Technicians
    {
        private const string Base = $"{BaseV1}/technicians";

        public static string Create() => Base;
        public static string GetById(Guid id) => $"{Base}/{id}";
        public static string GetAvailable(int pageNumber = 1, int pageSize = 20)
            => $"{Base}/available?pageNumber={pageNumber}&pageSize={pageSize}";
        public static string AddCertification(Guid id) => $"{Base}/{id}/certifications";
        public static string SetAvailable(Guid id) => $"{Base}/{id}/set-available";
        public static string SetUnavailable(Guid id) => $"{Base}/{id}/set-unavailable";
    }
}

