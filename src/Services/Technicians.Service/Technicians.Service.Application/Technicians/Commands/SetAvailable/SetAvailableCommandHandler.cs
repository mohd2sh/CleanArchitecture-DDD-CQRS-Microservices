using Technicians.Service.Domain.Technicians;
using CleanArchitecture.Core.Application.Abstractions.Common;
using CleanArchitecture.Core.Application.Abstractions.Persistence.Repositories;

namespace Technicians.Service.Application.Technicians.Commands.SetAvailable;

internal sealed class SetAvailableCommandHandler
    : ICommandHandler<SetAvailableCommand, Result>
{
    private readonly IRepository<Technician, Guid> _repository;

    public SetAvailableCommandHandler(IRepository<Technician, Guid> repository)
    {
        _repository = repository;
    }

    public async Task<Result> Handle(SetAvailableCommand request, CancellationToken cancellationToken = default)
    {
        var technician = await _repository.GetByIdAsync(request.TechnicianId, cancellationToken);

        if (technician is null)
            return TechnicianErrors.NotFound;

        technician.SetAvailable();

        await _repository.UpdateAsync(technician, cancellationToken);

        return Result.Success();
    }
}


