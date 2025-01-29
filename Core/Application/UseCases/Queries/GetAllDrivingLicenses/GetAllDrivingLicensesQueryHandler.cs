using System.Data.Common;
using Core.Domain.DrivingLicenceAggregate;
using Dapper;
using MediatR;

namespace Core.Application.UseCases.Queries.GetAllDrivingLicenses;

public class GetAllDrivingLicensesQueryHandler(DbDataSource dataSource) 
    : IRequestHandler<GetAllDrivingLicensesQuery, GetAllDrivingLicensesQueryResponse>
{
    public async Task<GetAllDrivingLicensesQueryResponse> Handle(GetAllDrivingLicensesQuery request, 
        CancellationToken cancellationToken)
    {
        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);

        var command = new CommandDefinition(
            commandText: request.FilterStatus is null ? _sql : _sqlWithStatusFilter,
            parameters: request.FilterStatus is null
                ? new
                {
                    Offset = (request.Page - 1) * request.PageSize, 
                    Limit = request.PageSize
                }
                : new
                {
                    StatusId = request.FilterStatus.Id, 
                    Offset = (request.Page - 1) * request.PageSize,
                    Limit = request.PageSize
                }, 
            cancellationToken: cancellationToken);

        var modelsEnumerable = await connection.QueryAsync<DapperDrivingLicenseShortModel>(command);
        var modelList = modelsEnumerable.AsList();

        var viewList = modelList.Select(x => new GetAllDrivingLicensesQueryResponse.DrivingLicenseShortView
        {
            Id = x.Id,
            AccountId = x.AccountId,
            Name = string.Join(' ', x.FirstName, x.LastName, x.Patronymic),
            Status = Status.FromId(x.StatusId)
        }).ToList();

        return new GetAllDrivingLicensesQueryResponse(viewList);
    }
    
    private class DapperDrivingLicenseShortModel
    {
        public Guid Id { get; set; }
        
        public Guid AccountId { get; set; }
        
        public int StatusId { get; set; }
        
        public string FirstName { get; set; } = null!;
        
        public string LastName { get; set; } = null!;
        
        public string Patronymic { get; set; } = null!;
    }

    private readonly string _sql =
        """
        SELECT id AS Id, account_id AS AccountId, status_id AS StatusId, first_name AS FirstName, 
               last_name AS LastName, patronymic AS Patronymic
        FROM driving_license
        ORDER BY number
        OFFSET @Offset
        LIMIT @Limit
        """;

    private readonly string _sqlWithStatusFilter =
        """
        SELECT id AS Id, account_id AS AccountId, status_id AS StatusId, first_name AS FirstName, 
               last_name AS LastName, patronymic AS Patronymic
        FROM driving_license
        WHERE status_id = @StatusId
        ORDER BY number
        OFFSET @Offset
        LIMIT @Limit
        """;
}