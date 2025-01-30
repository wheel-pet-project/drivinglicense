using FluentResults;
using MediatR;

namespace Application.UseCases.Commands.ApproveDrivingLicense;

public record ApproveDrivingLicenseRequest(Guid DrivingLicenseId) : IRequest<Result>;