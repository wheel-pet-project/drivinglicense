using Application.Ports.Postgres;
using Application.Ports.S3;
using Domain.PhotoAggregate;
using Domain.SharedKernel.Errors;
using FluentResults;
using MediatR;

namespace Application.UseCases.Commands.UploadPhotos;

public class UploadPhotosHandler(
    IPhotoRepository photoRepository,
    IS3Storage s3Storage,
    IUnitOfWork unitOfWork) 
    : IRequestHandler<UploadPhotosCommand, Result>
{
    public async Task<Result> Handle(UploadPhotosCommand command, CancellationToken cancellationToken)
    {
        var photo = Photo.Create(command.DrivingLicenseId, command.FrontPhotoBytes, command.BackPhotoBytes);

        var isSuccess = await s3Storage.SavePhotos(photo);
        if (isSuccess is false) return Result.Fail(new ObjectStorageUnavailable("Failed to upload photo"));
        
        await photoRepository.Add(photo);
        await unitOfWork.Commit();
        
        return Result.Ok();
    }
}