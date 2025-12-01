using Technicians.Service.Domain.Technicians;
using CleanArchitecture.Core.Application.Abstractions.Common;
using CleanArchitecture.Core.Application.Abstractions.Persistence.Repositories;

namespace Technicians.Service.Application.Technicians.Commands.SetUnavailable;

internal sealed class SetUnavailableCommandHandler
    : ICommandHandler<SetUnavailableCommand, Result>
{
    private readonly IRepository<Technician, Guid> _repository;

    public SetUnavailableCommandHandler(IRepository<Technician, Guid> repository)
    {
        _repository = repository;
    }

    public async Task<Result> Handle(SetUnavailableCommand request, CancellationToken cancellationToken = default)
    {
        var technician = await _repository.GetByIdAsync(request.TechnicianId, cancellationToken);

        if (technician is null)
            return TechnicianErrors.NotFound;

        technician.SetUnavailable();

        await _repository.UpdateAsync(technician, cancellationToken);

        return Result.Success();
    }
}


