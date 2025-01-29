using Core.Domain.DrivingLicenceAggregate;
using FluentResults;
using MediatR;

namespace Core.Application.UseCases.Commands.UploadDrivingLicense;

public record UploadDrivingLicenseRequest(
    Guid Id,
    Guid AccountId,
    Status Status,
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
    : IRequest<Result<UploadDrivingLicenseResponse>>;