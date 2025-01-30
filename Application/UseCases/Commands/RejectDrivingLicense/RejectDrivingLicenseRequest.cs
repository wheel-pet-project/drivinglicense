using FluentResults;
using MediatR;

namespace Application.UseCases.Commands.RejectDrivingLicense;

public record RejectDrivingLicenseRequest(
    Guid CorrelationId,
    Guid DrivingLicenseId) 
    : BaseRequest(CorrelationId), IRequest<Result>;