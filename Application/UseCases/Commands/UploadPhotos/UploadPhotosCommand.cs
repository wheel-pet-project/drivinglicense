using FluentResults;
using MediatR;

namespace Application.UseCases.Commands.UploadPhotos;

public record UploadPhotosCommand(
    Guid CorrelationId,
    Guid DrivingLicenseId,
    List<byte> FrontPhotoBytes,
    List<byte> BackPhotoBytes)
    : BaseRequest(CorrelationId), IRequest<Result>;