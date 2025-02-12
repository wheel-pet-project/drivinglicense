using FluentResults;
using MediatR;

namespace Application.UseCases.Commands.UploadDrivingLicense;

public record UploadDrivingLicenseCommand(
    Guid CorrelationId,
    Guid AccountId,
    List<char> CategoryList,
    string Number,
    string FirstName,
    string LastName,
    string? Patronymic,
    string CityOfBirth,
    DateOnly DateOfBirth,
    DateOnly DateOfIssue,
    string CodeOfIssue,
    DateOnly DateOfExpiry)
    : BaseRequest(CorrelationId), IRequest<Result<UploadDrivingLicenseResponse>>;