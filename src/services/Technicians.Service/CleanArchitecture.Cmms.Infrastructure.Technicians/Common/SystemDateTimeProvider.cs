using CleanArchitecture.Core.Application.Abstractions.Common;

namespace CleanArchitecture.Cmms.Infrastructure.Technicians.Common;

internal sealed class SystemDateTimeProvider : IDateTimeProvider
{
    public DateTime UtcNow => DateTime.UtcNow;

    public DateTime Now => throw new NotImplementedException();

    public DateOnly Today => throw new NotImplementedException();
}

