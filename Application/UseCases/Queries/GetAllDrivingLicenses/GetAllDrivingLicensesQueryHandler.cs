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
        GetAllDrivingLicensesQuery request,
        CancellationToken cancellationToken)
    {
        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);

        var command = new CommandDefinition(_sql,
            new
            {
                StatusId = request.FilteringStatus.Id,
                Offset = (request.Page - 1) * request.PageSize,
                Limit = request.PageSize
            },
            cancellationToken: cancellationToken);

        var modelsEnumerable = await connection.QueryAsync<DapperDrivingLicenseShortModel>(command);
        var modelList = modelsEnumerable.AsList();

        var viewList = modelList.Select(x => new GetAllDrivingLicensesQueryResponse.DrivingLicenseShortView(
                x.Id,
                x.AccountId,
                string.Join(' ', new List<string>(x.Patronymic is null
                    ? [x.FirstName, x.LastName]
                    : [x.FirstName, x.LastName, x.Patronymic])),
                Status.FromId(x.StatusId)))
            .ToList();

        return new GetAllDrivingLicensesQueryResponse(viewList);
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