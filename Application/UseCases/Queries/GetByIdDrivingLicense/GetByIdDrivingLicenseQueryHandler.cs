using System.Data.Common;
using Application.UseCases.Queries.DapperMappingExtensions;
using Dapper;
using Domain.DrivingLicenceAggregate;
using FluentResults;
using MediatR;

namespace Application.UseCases.Queries.GetByIdDrivingLicense;

public class GetByIdDrivingLicenseQueryHandler(DbDataSource dataSource)
    : IRequestHandler<GetByIdDrivingLicenseQuery, Result<GetByIdDrivingLicenseQueryResponse>>
{
    public async Task<Result<GetByIdDrivingLicenseQueryResponse>> Handle(GetByIdDrivingLicenseQuery request, CancellationToken cancellationToken)
    {
        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
        
        SqlMapper.AddTypeHandler(new DateOnlyMapper());
        
        var command = new CommandDefinition(_sql, new { Id = request.Id }, cancellationToken: cancellationToken);

        var dapperModel = await connection.QuerySingleOrDefaultAsync<DapperDrivingLicenseModel>(command);
        if (dapperModel is null) return Result.Fail("Driving license not found");

        var responseModel = new GetByIdDrivingLicenseQueryResponse(new DrivingLicenseView
            {
                Id = dapperModel.Id,
                AccountId = dapperModel.AccountId,
                CategoryList = dapperModel.CategoryList.Select(x => x[0]).ToArray(),
                Name = string.Join(' ', dapperModel.FirstName, dapperModel.LastName, dapperModel.Patronymic),
                Number = dapperModel.Number,
                CityOfBirth = dapperModel.CityOfBirth,
                DateOfBirth = dapperModel.DateOfBirth,
                CodeOfIssue = dapperModel.CodeOfIssue,
                DateOfIssue = dapperModel.DateOfIssue,
                DateOfExpiry = dapperModel.DateOfExpiry,
                Status = Status.FromId(dapperModel.StatusId)
            });
        
        return Result.Ok(responseModel);
    }

    private class DapperDrivingLicenseModel
    {
        public Guid Id { get; private set; }
    
        public Guid AccountId { get; private set; }

        public int StatusId { get; private set; }

        public string[] CategoryList { get; private set; } = null!;

        public string Number { get; private set; } = null!;
    
        public string FirstName { get; private set; } = null!;
        
        public string LastName { get; private set; } = null!;
        
        public string Patronymic { get; private set; } = null!;
    
        public string CityOfBirth { get; private set; } = null!;

        public DateOnly DateOfBirth { get; private set; }
    
        public DateOnly DateOfIssue { get; private set; }
    
        public string CodeOfIssue { get; private set; } = null!;
    
        public DateOnly DateOfExpiry { get; private set; }
    }
    
    private readonly string _sql =
        """
        SELECT id AS Id, account_id AS AccountId, status_id AS StatusId, first_name AS FirstName, last_name AS LastName, 
               patronymic AS Patronymic, categories AS CategoryList, number AS Number, city_of_birth AS CityOfBirth, 
               date_of_birth AS DateOfBirth, date_of_issue AS DateOfIssue, code_of_issue AS CodeOfIssue, 
               date_of_expiry AS DateOfExpiry
        FROM driving_license
        WHERE id = @Id
        """;
}