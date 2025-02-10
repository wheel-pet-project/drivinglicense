using Application.Ports.S3;
using Application.UseCases.Queries.DapperMappingExtensions;
using Dapper;
using Domain.DrivingLicenceAggregate;
using FluentResults;
using MediatR;
using Npgsql;

namespace Application.UseCases.Queries.GetByIdDrivingLicense;

public class GetByIdDrivingLicenseQueryHandler(
    NpgsqlDataSource dataSource,
    IS3Storage s3Storage)
    : IRequestHandler<GetByIdDrivingLicenseQuery, Result<GetByIdDrivingLicenseQueryResponse>>
{
    public async Task<Result<GetByIdDrivingLicenseQueryResponse>> Handle(GetByIdDrivingLicenseQuery request, CancellationToken cancellationToken)
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
        
        var photoIdsModel = await connection.QuerySingleOrDefaultAsync<DapperPhotoIdsModel>(
            _getPhotoIdsSql, 
            new { LicenseId = request.Id });
        if (photoIdsModel is null) return Result.Ok(responseModel);

        var getPhotosResult = await s3Storage.GetPhotos(
            photoIdsModel.PhotoId, 
            photoIdsModel.FrontPhotoId, 
            photoIdsModel.BackPhotoId);
        if (getPhotosResult is null) return Result.Fail("Photos for license not found");
        
        var (frontPhoto, backPhoto) = getPhotosResult.Value;
        responseModel.DrivingLicenseView.AddPhotos(frontPhoto, backPhoto);
        
        return Result.Ok(responseModel);
    }

    private class DapperPhotoIdsModel
    {
        public required Guid PhotoId { get; init; }
        
        public required Guid FrontPhotoId { get; init; }
        
        public required Guid BackPhotoId { get; init; }
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
        SELECT id AS PhotoId, front_photo_storage_id AS FrontPhotoId, back_photo_storage_id AS BackPhotoId
        FROM photo
        WHERE driving_license_id = @LicenseId
        """;
    
    private readonly string _getLicenseSql =
        """
        SELECT id AS Id, account_id AS AccountId, status_id AS StatusId, first_name AS FirstName, last_name AS LastName, 
               patronymic AS Patronymic, categories AS CategoryList, number AS Number, city_of_birth AS CityOfBirth, 
               date_of_birth AS DateOfBirth, date_of_issue AS DateOfIssue, code_of_issue AS CodeOfIssue, 
               date_of_expiry AS DateOfExpiry
        FROM driving_license
        WHERE id = @Id
        """;
}