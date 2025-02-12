using FluentResults;
using MediatR;

namespace Application.UseCases.Commands.ApproveDrivingLicense;

public record ApproveDrivingLicenseCommand(
    Guid CorrelationId,
    Guid DrivingLicenseId)
    : BaseRequest(CorrelationId), IRequest<Result>;