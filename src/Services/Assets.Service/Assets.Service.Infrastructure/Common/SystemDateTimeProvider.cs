using CleanArchitecture.Core.Application.Abstractions.Common;

namespace Assets.Service.Infrastructure.Common;

internal sealed class SystemDateTimeProvider : IDateTimeProvider
{
    public DateTime UtcNow => DateTime.UtcNow;

    public DateTime Now => throw new NotImplementedException();

    public DateOnly Today => throw new NotImplementedException();
}


