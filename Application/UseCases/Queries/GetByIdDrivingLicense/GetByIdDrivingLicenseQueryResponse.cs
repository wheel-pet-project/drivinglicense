using Domain.DrivingLicenceAggregate;

namespace Application.UseCases.Queries.GetByIdDrivingLicense;

public class GetByIdDrivingLicenseQueryResponse
{
    public GetByIdDrivingLicenseQueryResponse(DrivingLicenseView view)
    {
        DrivingLicenseView = view;
    }

    public DrivingLicenseView DrivingLicenseView { get; private set; }
}

public class DrivingLicenseView
{
    public required Guid Id { get; init; }

    public required Guid AccountId { get; init; }

    public required Status Status { get; init; } = null!;

    public required IReadOnlyList<char> CategoryList { get; init; } = null!;

    public required string Number { get; init; } = null!;

    public required string Name { get; init; } = null!;

    public required string CityOfBirth { get; init; } = null!;

    public required DateOnly DateOfBirth { get; init; }

    public required DateOnly DateOfIssue { get; init; }

    public required string CodeOfIssue { get; init; } = null!;

    public required DateOnly DateOfExpiry { get; init; }

    public string? FrontPhotoS3Url { get; private set; }

    public string? BackPhotoS3Url { get; private set; }

    public void AddPhotoUrls(string frontPhotoUrl, string backPhotoUrl)
    {
        if (frontPhotoUrl is null || backPhotoUrl is null) return;

        if (frontPhotoUrl.Length > 0) FrontPhotoS3Url = frontPhotoUrl;
        if (backPhotoUrl.Length > 0) BackPhotoS3Url = backPhotoUrl;
    }
}