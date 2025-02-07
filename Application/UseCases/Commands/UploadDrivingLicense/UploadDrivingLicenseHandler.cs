using Application.Ports.Postgres;
using Domain.DrivingLicenceAggregate;
using Domain.SharedKernel.ValueObjects;
using FluentResults;
using MediatR;

namespace Application.UseCases.Commands.UploadDrivingLicense;

public class UploadDrivingLicenseHandler(
    IDrivingLicenseRepository drivingLicenseRepository, 
    TimeProvider timeProvider,
    IUnitOfWork unitOfWork) 
    : IRequestHandler<UploadDrivingLicenseCommand, Result<UploadDrivingLicenseResponse>>
{
    public async Task<Result<UploadDrivingLicenseResponse>> Handle(UploadDrivingLicenseCommand command, 
        CancellationToken cancellationToken)
    {
        var name = Name.Create(command.FirstName, command.LastName, command.Patronymic);
        var categoryList = CategoryList.Create(command.CategoryList);
        var licenseNumber = DrivingLicenseNumber.Create(command.Number);
        var cityOfBirth = City.Create(command.CityOfBirth);
        var codeOfIssue = CodeOfIssue.Create(command.CodeOfIssue);
        
        var license = DrivingLicense.Create(command.AccountId, categoryList, licenseNumber, name, cityOfBirth,
            command.DateOfBirth, command.DateOfIssue, codeOfIssue, command.DateOfExpiry, timeProvider);
        
        await drivingLicenseRepository.Add(license);
        await unitOfWork.Commit();
        
        return Result.Ok(new UploadDrivingLicenseResponse(license.Id));
    }
}