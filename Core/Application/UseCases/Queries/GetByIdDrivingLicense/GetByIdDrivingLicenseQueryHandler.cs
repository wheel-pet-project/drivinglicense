using System.Data.Common;
using Core.Domain.DrivingLicenceAggregate;
using Dapper;
using FluentResults;
using MediatR;

namespace Core.Application.UseCases.Queries.GetByIdDrivingLicense;

public class GetByIdDrivingLicenseQueryHandler(DbDataSource dataSource)
    : IRequestHandler<GetByIdDrivingLicenseQuery, Result<GetByIdDrivingLicenseQueryResponse>>
{
    public async Task<Result<GetByIdDrivingLicenseQueryResponse>> Handle(GetByIdDrivingLicenseQuery request, CancellationToken cancellationToken)
    {
        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
        
        var command = new CommandDefinition(_sql, new { Id = request.Id }, cancellationToken: cancellationToken);

        var dapperModel = await connection.QuerySingleOrDefaultAsync<DapperDrivingLicenseModel>(command);
        if (dapperModel is null) return Result.Fail("Driving license not found");

        var responseModel = new GetByIdDrivingLicenseQueryResponse(new DrivingLicenseView
            {
                Id = dapperModel.Id,
                AccountId = dapperModel.AccountId,
                CategoryList = dapperModel.CategoryList,
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
        public required Guid Id { get; init; }
    
        public required Guid AccountId { get; init; }

        public required int StatusId { get; init; }

        public required List<char> CategoryList { get; init; } = null!;

        public required string Number { get; init; } = null!;
    
        public string FirstName { get; set; } = null!;
        
        public string LastName { get; set; } = null!;
        
        public string Patronymic { get; set; } = null!;
    
        public required string CityOfBirth { get; init; } = null!;

        public required DateOnly DateOfBirth { get; init; }
    
        public required DateOnly DateOfIssue { get; init; }
    
        public required string CodeOfIssue { get; init; } = null!;
    
        public required DateOnly DateOfExpiry { get; init; }
    }
    
    private readonly string _sql =
        """
        SELECT id, account_id, status_id, categories, number, first_name, last_name, patronymic, city_of_birth, 
               date_of_birth, date_of_issue, code_of_issue, date_of_expiry
        FROM driving_license
        WHERE id = @Id
        """;
}