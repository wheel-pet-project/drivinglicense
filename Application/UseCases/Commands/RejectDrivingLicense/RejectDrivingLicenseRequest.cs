using FluentResults;
using MediatR;

namespace Application.UseCases.Commands.RejectDrivingLicense;

public record RejectDrivingLicenseRequest(Guid DrivingLicenseId) : IRequest<Result>;