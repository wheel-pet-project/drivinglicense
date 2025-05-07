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
    public async Task<Result<UploadDrivingLicenseResponse>> Handle(
        UploadDrivingLicenseCommand command,
        CancellationToken cancellationToken)
    {
        var (name, categoryList, licenseNumber, cityOfBirth, codeOfIssue) = Name(command);

        var license = DrivingLicense.Create(command.AccountId, categoryList, licenseNumber, name, cityOfBirth,
            command.DateOfBirth, command.DateOfIssue, codeOfIssue, command.DateOfExpiry, timeProvider);

        await drivingLicenseRepository.Add(license);
        var commitResult = await unitOfWork.Commit();

        return commitResult.IsSuccess ? Result.Ok(new UploadDrivingLicenseResponse(license.Id)) : commitResult;
    }

    private (Name name, CategoryList categoryList, DrivingLicenseNumber licenseNumber, City cityOfBirth, CodeOfIssue
        codeOfIssue) Name(UploadDrivingLicenseCommand command)
    {
        var name = Domain.SharedKernel.ValueObjects.Name.Create(command.FirstName, command.LastName, command.Patronymic);
        var categoryList = CategoryList.Create(command.CategoryList);
        var licenseNumber = DrivingLicenseNumber.Create(command.Number);
        var cityOfBirth = City.Create(command.CityOfBirth);
        var codeOfIssue = CodeOfIssue.Create(command.CodeOfIssue);
        
        return (name, categoryList, licenseNumber, cityOfBirth, codeOfIssue);
    }
}