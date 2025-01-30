using Application.Ports.Postgres;
using Domain.SharedKernel.Errors;
using FluentResults;
using MediatR;

namespace Application.UseCases.Commands.ApproveDrivingLicense;

public class ApproveDrivingLicenseHandler(
    IDrivingLicenseRepository drivingLicenseRepository,
    IUnitOfWork unitOfWork) 
    : IRequestHandler<ApproveDrivingLicenseRequest, Result>
{
    public async Task<Result> Handle(ApproveDrivingLicenseRequest request, CancellationToken cancellationToken)
    {
        var license = await drivingLicenseRepository.GetById(request.DrivingLicenseId);
        if (license is null) return Result.Fail(new NotFound("Driving license not found"));
        
        license.Approve();
        
        drivingLicenseRepository.Update(license);
        await unitOfWork.Commit();
        
        return Result.Ok();
    }
}