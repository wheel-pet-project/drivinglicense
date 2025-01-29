using FluentResults;
using MediatR;

namespace Core.Application.UseCases.Commands.ApproveDrivingLicense;

public record ApproveDrivingLicenseRequest(Guid DrivingLicenseId) : IRequest<Result>;