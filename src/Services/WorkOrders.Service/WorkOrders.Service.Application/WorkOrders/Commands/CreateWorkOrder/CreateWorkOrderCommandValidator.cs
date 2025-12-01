using FluentValidation;

namespace WorkOrders.Service.Application.WorkOrders.Commands.CreateWorkOrder;

internal sealed class CreateWorkOrderCommandValidator : AbstractValidator<CreateWorkOrderCommand>
{
    public CreateWorkOrderCommandValidator()
    {
        RuleFor(x => x.AssetId)
            .NotEmpty()
            .WithMessage("Asset ID is required.");

        RuleFor(x => x.Title)
            .NotEmpty()
            .WithMessage("Title is required.");

        RuleFor(x => x.Building)
            .NotEmpty()
            .WithMessage("Building is required.");

        RuleFor(x => x.Floor)
            .NotEmpty()
            .WithMessage("Floor is required.");

        RuleFor(x => x.Room)
            .NotEmpty()
            .WithMessage("Room is required.");
    }
}







