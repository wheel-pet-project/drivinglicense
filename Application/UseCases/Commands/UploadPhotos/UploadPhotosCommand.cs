using FluentResults;
using MediatR;

namespace Application.UseCases.Commands.UploadPhotos;

public record UploadPhotosCommand(
    Guid CorrelationId,
    Guid DrivingLicenseId,
    byte[] FrontPhotoBytes,
    byte[] BackPhotoBytes)
    : BaseRequest(CorrelationId), IRequest<Result>;