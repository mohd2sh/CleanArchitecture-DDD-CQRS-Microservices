namespace CleanArchitecture.Cmms.Api.WorkOrders.IntegrationTests.TestHelpers;

public record ResultDto<T>
{
    public bool IsSuccess { get; init; }
    public T? Value { get; init; }
    public string? Error { get; init; }
}

public record ResultDto
{
    public bool IsSuccess { get; init; }
    public string? Error { get; init; }
}

