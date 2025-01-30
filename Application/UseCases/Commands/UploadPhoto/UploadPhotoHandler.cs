using Application.Ports.Postgres;
using Application.Ports.S3;
using Domain.PhotoAggregate;
using FluentResults;
using MediatR;

namespace Application.UseCases.Commands.UploadPhoto;

public class UploadPhotoHandler(
    IPhotoRepository photoRepository,
    IS3Storage s3Storage,
    IUnitOfWork unitOfWork) 
    : IRequestHandler<UploadPhotoRequest, Result>
{
    public async Task<Result> Handle(UploadPhotoRequest request, CancellationToken cancellationToken)
    {
        var photo = Photo.Create(request.DrivingLicenseId, request.FrontPhotoBytes, request.BackPhotoBytes);

        var isSuccess = await s3Storage.SavePhotos(photo);
        if (isSuccess is false) return Result.Fail("Failed to upload photo");
        
        await photoRepository.Add(photo);
        await unitOfWork.Commit();
        
        return Result.Ok();
    }
}