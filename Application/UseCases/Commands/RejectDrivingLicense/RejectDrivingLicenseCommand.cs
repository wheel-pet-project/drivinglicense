using FluentResults;
using MediatR;

namespace Application.UseCases.Commands.RejectDrivingLicense;

public record RejectDrivingLicenseCommand(
    Guid CorrelationId,
    Guid DrivingLicenseId) 
    : BaseRequest(CorrelationId), IRequest<Result>;