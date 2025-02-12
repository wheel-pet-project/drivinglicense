using Application.Ports.Postgres;
using Domain.SharedKernel.Errors;
using FluentResults;
using MediatR;

namespace Application.UseCases.Commands.RejectDrivingLicense;

public class RejectDrivingLicenseHandler(
    IDrivingLicenseRepository drivingLicenseRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<RejectDrivingLicenseCommand, Result>
{
    public async Task<Result> Handle(RejectDrivingLicenseCommand command, CancellationToken cancellationToken)
    {
        var license = await drivingLicenseRepository.GetById(command.DrivingLicenseId);
        if (license is null) return Result.Fail(new NotFound("Driving license not found"));

        license.Reject();

        drivingLicenseRepository.Update(license);
        await unitOfWork.Commit();

        return Result.Ok();
    }
}