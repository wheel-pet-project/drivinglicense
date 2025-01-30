using Application.Ports.Postgres;
using Domain.DrivingLicenceAggregate;
using Domain.SharedKernel.ValueObjects;
using FluentResults;
using MediatR;

namespace Application.UseCases.Commands.UploadDrivingLicense;

public class UploadDrivingLicenseHandler(
    IDrivingLicenseRepository drivingLicenseRepository, 
    IUnitOfWork unitOfWork) 
    : IRequestHandler<UploadDrivingLicenseRequest, Result<UploadDrivingLicenseResponse>>
{
    public async Task<Result<UploadDrivingLicenseResponse>> Handle(UploadDrivingLicenseRequest request, 
        CancellationToken cancellationToken)
    {
        var name = Name.Create(request.FirstName, request.LastName, request.Patronymic);
        var categoryList = CategoryList.Create(request.CategoryList);
        var licenseNumber = DrivingLicenseNumber.Create(request.Number);
        var cityOfBirth = City.Create(request.CityOfBirth);
        var codeOfIssue = CodeOfIssue.Create(request.CodeOfIssue);
        
        var license = DrivingLicense.Create(request.AccountId, categoryList, licenseNumber, name, cityOfBirth,
            request.DateOfBirth, request.DateOfIssue, codeOfIssue, request.DateOfExpiry);
        
        await drivingLicenseRepository.Add(license);
        await unitOfWork.Commit();
        
        return Result.Ok(new UploadDrivingLicenseResponse(license.Id));
    }
}