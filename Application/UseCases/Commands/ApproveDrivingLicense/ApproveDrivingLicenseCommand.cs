using FluentResults;
using MediatR;

namespace Application.UseCases.Commands.ApproveDrivingLicense;

public record ApproveDrivingLicenseCommand(Guid DrivingLicenseId) : IRequest<Result>;