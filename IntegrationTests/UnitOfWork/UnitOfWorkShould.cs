using Application.Ports.Postgres;
using Domain.DrivingLicenceAggregate;
using Domain.SharedKernel;
using Domain.SharedKernel.ValueObjects;
using Infrastructure.Adapters.Postgres;
using JetBrains.Annotations;
using JsonNet.ContractResolvers;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Xunit;

namespace IntegrationTests.UnitOfWork;

[TestSubject(typeof(Infrastructure.Adapters.Postgres.UnitOfWork))]
public class UnitOfWorkShould : IntegrationTestBase
{
    private readonly JsonSerializerSettings _jsonSerializerSettings = new()
    {
        TypeNameHandling = TypeNameHandling.All,
        ContractResolver = new PrivateSetterContractResolver()
    };

    private readonly DrivingLicense _drivingLicense = DrivingLicense.Create(
        Guid.NewGuid(),
        CategoryList.Create([CategoryList.BCategory]),
        DrivingLicenseNumber.Create("1234 567891"),
        Name.Create("Иван", "Иванов", "Иванович"),
        City.Create("Москва"),
        new DateOnly(1990, 1, 1),
        new DateOnly(2020, 1, 1),
        CodeOfIssue.Create("1234"),
        new DateOnly(2030, 1, 1),
        TimeProvider.System);

    [Fact]
    public async Task SaveDomainEvent()
    {
        // Arrange
        _drivingLicense.MarkAsPendingProcessing();
        _drivingLicense.Approve();
        var expectedEvent = _drivingLicense.DomainEvents[0];
        var uowBuilder = new UnitOfWorkBuilder();
        uowBuilder.ConfigureContext(Context);

        var uow = uowBuilder.Build();
        Context.Attach(_drivingLicense);

        // Act
        await uow.Commit();

        // Assert
        var outboxEvent = await Context.Outbox.FirstOrDefaultAsync(TestContext.Current.CancellationToken);
        Assert.NotNull(outboxEvent);
        var parsedEvent = JsonConvert.DeserializeObject<DomainEvent>(outboxEvent.Content, _jsonSerializerSettings);
        Assert.Equivalent(expectedEvent, parsedEvent);
    }

    private class UnitOfWorkBuilder
    {
        private DataContext _context = null!;

        public IUnitOfWork Build()
        {
            return new Infrastructure.Adapters.Postgres.UnitOfWork(_context);
        }

        public void ConfigureContext(DataContext context)
        {
            _context = context;
        }
    }
}