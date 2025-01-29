using FluentResults;
using MediatR;

namespace Core.Application.UseCases.Commands.RejectDrivingLicense;

public record RejectDrivingLicenseRequest(Guid DrivingLicenseId) : IRequest<Result>;