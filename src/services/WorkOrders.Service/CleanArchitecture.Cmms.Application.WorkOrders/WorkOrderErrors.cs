namespace CleanArchitecture.Cmms.Application.WorkOrders;

using CleanArchitecture.Core.Application.Abstractions.Common;
using CleanArchitecture.Core.Domain.Abstractions.Attributes;

/// <summary>
/// Provides centralized error definitions for Work Order operations.
/// </summary>
[ErrorCodeDefinition("WorkOrder")]
public static class WorkOrderErrors
{
    [ApplicationError]
    public static readonly Error NotFound = Error.NotFound(
        "WorkOrder.NotFound",
        "Work order not found.");

    [ApplicationError]
    public static readonly Error TitleRequired = Error.Validation(
        "WorkOrder.TitleRequired",
        CleanArchitecture.Cmms.Domain.WorkOrders.WorkOrderErrors.TitleRequired.Message);

    [ApplicationError]
    public static readonly Error AssetRequired = Error.Validation(
        "WorkOrder.AssetIdRequired",
        CleanArchitecture.Cmms.Domain.WorkOrders.WorkOrderErrors.AssetIdRequired.Message);

    [ApplicationError]
    public static readonly Error TechnicianRequired = Error.Validation(
        "WorkOrder.TechnicianRequired",
        CleanArchitecture.Cmms.Domain.WorkOrders.WorkOrderErrors.TechnicianRequired.Message);

    [ApplicationError]
    public static readonly Error InvalidStateTransition = Error.Validation(
        "WorkOrder.InvalidStateTransition",
        CleanArchitecture.Cmms.Domain.WorkOrders.WorkOrderErrors.InvalidStateTransition.Message);

    [ApplicationError]
    public static readonly Error StepsNotCompleted = Error.Validation(
        "WorkOrder.StepsNotCompleted",
        CleanArchitecture.Cmms.Domain.WorkOrders.WorkOrderErrors.StepsNotCompleted.Message);
}

