namespace CleanArchitecture.Cmms.Api.WorkOrders.IntegrationTests.TestHelpers;

public static class WorkOrdersApiEndpoints
{
    private const string BaseV1 = "/api/v1";

    public static class WorkOrders
    {
        private const string Base = $"{BaseV1}/workorders";

        public static string Create() => Base;
        public static string GetById(Guid id) => $"{Base}/{id}";
        public static string GetActive(int pageNumber = 1, int pageSize = 20)
            => $"{Base}?pageNumber={pageNumber}&pageSize={pageSize}";
        public static string Assign(Guid id) => $"{Base}/{id}/assign";
        public static string Start(Guid id) => $"{Base}/{id}/start";
        public static string Complete(Guid id) => $"{Base}/{id}/complete";
        public static string AddStep(Guid id) => $"{Base}/{id}/steps";
        public static string CompleteStep(Guid id, Guid stepId) => $"{Base}/{id}/steps/{stepId}/complete";
    }
}

