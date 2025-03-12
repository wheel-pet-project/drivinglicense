using FluentResults;
using MediatR;

namespace Application.UseCases.Commands.UploadPhotos;

public record UploadPhotosCommand(
    Guid DrivingLicenseId,
    List<byte> FrontPhotoBytes,
    List<byte> BackPhotoBytes)
    : IRequest<Result>;