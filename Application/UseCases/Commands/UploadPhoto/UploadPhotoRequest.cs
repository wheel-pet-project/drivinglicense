using FluentResults;
using MediatR;

namespace Application.UseCases.Commands.UploadPhoto;

public record UploadPhotoRequest(
    Guid DrivingLicenseId,
    byte[] FrontPhotoBytes,
    byte[] BackPhotoBytes)
    : IRequest<Result>;