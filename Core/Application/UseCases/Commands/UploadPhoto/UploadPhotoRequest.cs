using FluentResults;
using MediatR;

namespace Core.Application.UseCases.Commands.UploadPhoto;

public record UploadPhotoRequest(
    Guid DrivingLicenseId,
    byte[] FrontPhotoBytes,
    byte[] BackPhotoBytes)
    : IRequest<Result>;