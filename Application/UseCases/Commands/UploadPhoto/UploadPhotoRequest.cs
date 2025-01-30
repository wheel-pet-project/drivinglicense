using FluentResults;
using MediatR;

namespace Application.UseCases.Commands.UploadPhoto;

public record UploadPhotoRequest(
    Guid CorrelationId,
    Guid DrivingLicenseId,
    byte[] FrontPhotoBytes,
    byte[] BackPhotoBytes)
    : BaseRequest(CorrelationId), IRequest<Result>;