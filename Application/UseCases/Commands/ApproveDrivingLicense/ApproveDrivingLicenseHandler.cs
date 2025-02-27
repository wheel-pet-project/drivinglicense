using Application.Ports.Postgres;
using Domain.SharedKernel.Errors;
using FluentResults;
using MediatR;

namespace Application.UseCases.Commands.ApproveDrivingLicense;

public class ApproveDrivingLicenseHandler(
    IDrivingLicenseRepository drivingLicenseRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<ApproveDrivingLicenseCommand, Result>
{
    public async Task<Result> Handle(ApproveDrivingLicenseCommand command, CancellationToken cancellationToken)
    {
        var license = await drivingLicenseRepository.GetById(command.DrivingLicenseId);
        if (license is null) return Result.Fail(new NotFound("Driving license not found"));

        license.Approve();

        drivingLicenseRepository.Update(license);

        return await unitOfWork.Commit();
    }
}