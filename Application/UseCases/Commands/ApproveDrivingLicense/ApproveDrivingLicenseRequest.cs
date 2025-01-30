using FluentResults;
using MediatR;

namespace Application.UseCases.Commands.ApproveDrivingLicense;

public record ApproveDrivingLicenseRequest(
    Guid CorrelationId, 
    Guid DrivingLicenseId) 
    : BaseRequest(CorrelationId), IRequest<Result>;