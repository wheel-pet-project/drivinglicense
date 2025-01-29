using Core.Ports.Postgres;
using FluentResults;
using MediatR;

namespace Core.Application.UseCases.Commands.RejectDrivingLicense;

public class RejectDrivingLicenseHandler(
    IDrivingLicenseRepository drivingLicenseRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<RejectDrivingLicenseRequest, Result>
{
    public async Task<Result> Handle(RejectDrivingLicenseRequest request, CancellationToken cancellationToken)
    {
        var license = await drivingLicenseRepository.GetById(request.DrivingLicenseId);
        if (license is null) return Result.Fail("Driving license not found");
        
        license.Reject();
        
        drivingLicenseRepository.Update(license);
        await unitOfWork.Commit();
        
        return Result.Ok();
    }
}