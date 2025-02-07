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
        accountId: Guid.NewGuid(), 
        categoryList: CategoryList.Create([CategoryList.BCategory]),
        number: DrivingLicenseNumber.Create(input: "1234 567891"), 
        name: Name.Create(firstName: "Иван", lastName: "Иванов", patronymic: "Иванович"), 
        cityOfBirth: City.Create("Москва"), 
        dateOfBirth: new DateOnly(year: 1990, month: 1, day: 1), 
        dateOfIssue: new DateOnly(year: 2020, month: 1, day: 1), 
        codeOfIssue: CodeOfIssue.Create(input: "1234"), 
        dateOfExpiry: new DateOnly(year: 2030, month: 1, day: 1), 
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
        
        public IUnitOfWork Build() => new Infrastructure.Adapters.Postgres.UnitOfWork(_context);
        
        public void ConfigureContext(DataContext context) => _context = context;
    }
}