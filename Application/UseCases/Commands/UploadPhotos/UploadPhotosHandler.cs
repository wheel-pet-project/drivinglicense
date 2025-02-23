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
    IImageFormatValidator formatValidator,
    IImageSizeValidator sizeValidator,
    IUnitOfWork unitOfWork)
    : IRequestHandler<UploadPhotosCommand, Result>
{
    public async Task<Result> Handle(UploadPhotosCommand command, CancellationToken cancellationToken)
    {
        if (formatValidator.IsSupportedFormat(command.FrontPhotoBytes) is false ||
            formatValidator.IsSupportedFormat(command.BackPhotoBytes) is false)
            return Result.Fail("Image format is not supported");

        if (sizeValidator.IsSupportedSize(command.FrontPhotoBytes.Count) is false ||
            sizeValidator.IsSupportedSize(command.BackPhotoBytes.Count) is false)
            return Result.Fail("Image size is too large");
        
        var uploadingToS3Result = await s3Storage.SavePhotos(command.FrontPhotoBytes, command.BackPhotoBytes);
        if (uploadingToS3Result.IsFailed) return Result.Fail(uploadingToS3Result.Errors);
        var keys = uploadingToS3Result.Value; 

        var photo = Photos.Create(command.DrivingLicenseId, keys.frontPhotoBucketAndKey, keys.backPhotoBucketAndKey);

        await photoRepository.Add(photo);
        
        return await unitOfWork.Commit();
    }
}