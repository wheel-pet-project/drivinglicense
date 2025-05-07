using Dapper;
using Domain.DrivingLicenceAggregate;
using FluentResults;
using MediatR;
using Npgsql;

namespace Application.UseCases.Queries.GetAllDrivingLicenses;

public class GetAllDrivingLicensesQueryHandler(NpgsqlDataSource dataSource)
    : IRequestHandler<GetAllDrivingLicensesQuery, Result<GetAllDrivingLicensesQueryResponse>>
{
    public async Task<Result<GetAllDrivingLicensesQueryResponse>> Handle(
        GetAllDrivingLicensesQuery query,
        CancellationToken cancellationToken)
    {
        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
        var command = new CommandDefinition(_sql,
            new
            {
                StatusId = query.FilteringStatus.Id,
                Offset = CalculateOffset(query.Page, query.PageSize),
                Limit = query.PageSize
            },
            cancellationToken: cancellationToken);
        var models = (await connection.QueryAsync<DapperDrivingLicenseShortModel>(command)).AsList();

        return MapToResponse(models);
    }
    
    private int CalculateOffset(int? page, int? pageSize)
    {
        page ??= 1;
        pageSize ??= 10;
        
        return page.Value < 1
            ? 1
            : (page.Value - 1) * pageSize.Value;
    }

    private GetAllDrivingLicensesQueryResponse MapToResponse(List<DapperDrivingLicenseShortModel> models)
    {
        return new GetAllDrivingLicensesQueryResponse(models.Select(x =>
                new GetAllDrivingLicensesQueryResponse.DrivingLicenseShortView(
                    x.Id,
                    x.AccountId,
                    string.Join(' ', new List<string>(x.Patronymic is null
                        ? [x.FirstName, x.LastName]
                        : [x.FirstName, x.LastName, x.Patronymic])),
                    Status.FromId(x.StatusId)))
            .ToList());
    }

    private class DapperDrivingLicenseShortModel
    {
        public required Guid Id { get; init; }

        public required Guid AccountId { get; init; }

        public required int StatusId { get; init; }

        public required string FirstName { get; init; }

        public required string LastName { get; init; }

        public required string? Patronymic { get; init; }
    }


    private readonly string _sql =
        """
        SELECT id AS Id, 
               account_id AS AccountId, 
               status_id AS StatusId, 
               first_name AS FirstName, 
               last_name AS LastName, 
               patronymic AS Patronymic
        FROM driving_license
        WHERE status_id = @StatusId
        ORDER BY number
        OFFSET @Offset
        LIMIT @Limit
        """;
}