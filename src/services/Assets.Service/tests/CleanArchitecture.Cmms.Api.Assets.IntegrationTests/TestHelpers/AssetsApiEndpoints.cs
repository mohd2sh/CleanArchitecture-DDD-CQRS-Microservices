namespace CleanArchitecture.Cmms.Api.Assets.IntegrationTests.TestHelpers;

public static class AssetsApiEndpoints
{
    private const string BaseV1 = "/api/v1";

    public static class Assets
    {
        private const string Base = $"{BaseV1}/assets";

        public static string Create() => Base;
        public static string GetById(Guid id) => $"{Base}/{id}";
        public static string GetActive(int pageNumber = 1, int pageSize = 20)
            => $"{Base}/active?pageNumber={pageNumber}&pageSize={pageSize}";
        public static string UpdateLocation(Guid id) => $"{Base}/{id}/location";
    }
}

