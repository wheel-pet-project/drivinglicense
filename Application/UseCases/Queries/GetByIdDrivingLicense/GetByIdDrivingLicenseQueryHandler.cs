using Application.UseCases.Queries.DapperMappingExtensions;
using Dapper;
using Domain.DrivingLicenceAggregate;
using FluentResults;
using MediatR;
using Npgsql;

namespace Application.UseCases.Queries.GetByIdDrivingLicense;

public class GetByIdDrivingLicenseQueryHandler(
    NpgsqlDataSource dataSource,
    string yandexS3StorageHost)
    : IRequestHandler<GetByIdDrivingLicenseQuery, Result<GetByIdDrivingLicenseQueryResponse>>
{
    public async Task<Result<GetByIdDrivingLicenseQueryResponse>> Handle(
        GetByIdDrivingLicenseQuery request,
        CancellationToken cancellationToken)
    {
        SqlMapper.AddTypeHandler(new DateOnlyMapper());
        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);

        var command = new CommandDefinition(_getLicenseSql, new { request.Id }, cancellationToken: cancellationToken);
        var dapperModel = await connection.QuerySingleOrDefaultAsync<DapperDrivingLicenseModel>(command);
        if (dapperModel is null) return Result.Fail("Driving license not found");

        var responseModel = new GetByIdDrivingLicenseQueryResponse(new DrivingLicenseView
        {
            Id = dapperModel.Id,
            AccountId = dapperModel.AccountId,
            CategoryList = dapperModel.CategoryList.Select(x => x[0]).ToArray(),
            Name = string.Join(' ', new List<string>(dapperModel.Patronymic is null
                ? [dapperModel.FirstName, dapperModel.LastName]
                : [dapperModel.FirstName, dapperModel.LastName, dapperModel.Patronymic])),
            Number = dapperModel.Number,
            CityOfBirth = dapperModel.CityOfBirth,
            DateOfBirth = dapperModel.DateOfBirth,
            CodeOfIssue = dapperModel.CodeOfIssue,
            DateOfIssue = dapperModel.DateOfIssue,
            DateOfExpiry = dapperModel.DateOfExpiry,
            Status = Status.FromId(dapperModel.StatusId)
        });

        var photoKeysModel = await connection.QuerySingleOrDefaultAsync<DapperPhotoIdsModel>(
            _getPhotoIdsSql,
            new { LicenseId = request.Id });
        if (photoKeysModel is null) return Result.Ok(responseModel);
        
        responseModel.DrivingLicenseView.AddPhotoUrls(
            $"{yandexS3StorageHost}/{photoKeysModel.FrontPhotoStorageBucketAndKey}",
            $"{yandexS3StorageHost}/{photoKeysModel.BackPhotoStorageBucketAndKey}");

        return Result.Ok(responseModel);
    }

    private class DapperPhotoIdsModel
    {
        public required Guid PhotoId { get; init; }

        public required string FrontPhotoStorageBucketAndKey { get; init; }

        public required string BackPhotoStorageBucketAndKey { get; init; }
    }

    private class DapperDrivingLicenseModel
    {
        public required Guid Id { get; init; }

        public required Guid AccountId { get; init; }

        public required int StatusId { get; init; }

        public required string[] CategoryList { get; init; }

        public required string Number { get; init; }

        public required string FirstName { get; init; }

        public required string LastName { get; init; }

        public required string? Patronymic { get; init; }

        public required string CityOfBirth { get; init; }

        public required DateOnly DateOfBirth { get; init; }

        public required DateOnly DateOfIssue { get; init; }

        public required string CodeOfIssue { get; init; }

        public required DateOnly DateOfExpiry { get; init; }
    }

    private readonly string _getPhotoIdsSql =
        """
        SELECT id AS PhotoId, 
               front_photo_storage_bucket_and_key AS FrontPhotoStorageBucketAndKey, 
               back_photo_storage_bucket_and_key AS BackPhotoStorageBucketAndKey
        FROM photos
        WHERE driving_license_id = @LicenseId
        """;

    private readonly string _getLicenseSql =
        """
        SELECT id AS Id, 
               account_id AS AccountId, 
               status_id AS StatusId, 
               first_name AS FirstName, 
               last_name AS LastName, 
               patronymic AS Patronymic, 
               categories AS CategoryList, 
               number AS Number, 
               city_of_birth AS CityOfBirth, 
               date_of_birth AS DateOfBirth, 
               date_of_issue AS DateOfIssue, 
               code_of_issue AS CodeOfIssue, 
               date_of_expiry AS DateOfExpiry
        FROM driving_license
        WHERE id = @Id
        """;
}