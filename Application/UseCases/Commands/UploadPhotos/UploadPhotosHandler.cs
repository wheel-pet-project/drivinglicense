using Application.Ports.ImageValidators;
using Application.Ports.Postgres;
using Application.Ports.S3;
using Domain.PhotoAggregate;
using FluentResults;
using MediatR;

namespace Application.UseCases.Commands.UploadPhotos;

public class UploadPhotosHandler(
    IPhotoRepository photoRepository,
    IS3Storage s3Storage,
    IImageValidator imageValidator,
    IUnitOfWork unitOfWork)
    : IRequestHandler<UploadPhotosCommand, Result>
{
    public async Task<Result> Handle(UploadPhotosCommand command, CancellationToken cancellationToken)
    {
        var validatingResult = ValidatePhotos(command);
        if (validatingResult.IsFailed) return validatingResult;

        var uploadingToS3Result = await s3Storage.SavePhotos(command.FrontPhotoBytes, command.BackPhotoBytes);
        if (uploadingToS3Result.IsFailed) return Result.Fail(uploadingToS3Result.Errors);
        var (frontPhotoBucketAndKey, backPhotoBucketAndKey) = uploadingToS3Result.Value;

        var photo = Photos.Create(command.DrivingLicenseId, frontPhotoBucketAndKey, backPhotoBucketAndKey);

        await photoRepository.Add(photo);

        return await unitOfWork.Commit();
    }

    private Result ValidatePhotos(UploadPhotosCommand command)
    {
        if (imageValidator.IsSupportedFormat(command.FrontPhotoBytes) is false ||
            imageValidator.IsSupportedFormat(command.BackPhotoBytes) is false)
            return Result.Fail("Image format is not supported");

        if (imageValidator.IsSupportedSize(command.FrontPhotoBytes.Count) is false ||
            imageValidator.IsSupportedSize(command.BackPhotoBytes.Count) is false)
            return Result.Fail("Image size is too large");
        
        return Result.Ok();
    }
}