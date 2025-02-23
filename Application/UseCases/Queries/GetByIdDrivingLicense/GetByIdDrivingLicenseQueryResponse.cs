using Domain.DrivingLicenceAggregate;

namespace Application.UseCases.Queries.GetByIdDrivingLicense;

public class GetByIdDrivingLicenseQueryResponse
{
    public GetByIdDrivingLicenseQueryResponse(DrivingLicenseView view)
    {
        DrivingLicense = view;
    }

    public DrivingLicenseView DrivingLicense { get; private set; }

    public record DrivingLicenseView(
        Guid Id,
        Guid AccountId,
        Status Status,
        List<char> CategoryList,
        string Number,
        string Name,
        string CityOfBirth,
        DateOnly DateOfBirth,
        DateOnly DateOfIssue,
        string CodeOfIssue,
        DateOnly DateOfExpiry,
        string? FrontPhotoS3Url = null,
        string? BackPhotoS3Url = null);
}