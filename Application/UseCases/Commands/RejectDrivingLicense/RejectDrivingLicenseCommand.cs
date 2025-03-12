using FluentResults;
using MediatR;

namespace Application.UseCases.Commands.RejectDrivingLicense;

public record RejectDrivingLicenseCommand(Guid DrivingLicenseId) : IRequest<Result>;