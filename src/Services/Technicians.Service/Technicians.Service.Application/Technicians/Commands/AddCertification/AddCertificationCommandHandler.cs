using Technicians.Service.Domain.Technicians;
using Technicians.Service.Domain.Technicians.ValueObjects;
using CleanArchitecture.Core.Application.Abstractions.Common;
using CleanArchitecture.Core.Application.Abstractions.Persistence.Repositories;

namespace Technicians.Service.Application.Technicians.Commands.AddCertification;

internal sealed class AddCertificationCommandHandler
    : ICommandHandler<AddCertificationCommand, Result>
{
    private readonly IRepository<Technician, Guid> _repository;

    public AddCertificationCommandHandler(IRepository<Technician, Guid> repository)
    {
        _repository = repository;
    }

    public async Task<Result> Handle(AddCertificationCommand request, CancellationToken cancellationToken = default)
    {
        var technician = await _repository.GetByIdAsync(request.TechnicianId, cancellationToken);

        if (technician is null)
            return TechnicianErrors.NotFound;

        var certification = Certification.Create(request.Code, request.IssuedOn, request.ExpiresOn);

        technician.AddCertification(certification);

        await _repository.UpdateAsync(technician, cancellationToken);

        return Result.Success();
    }
}


