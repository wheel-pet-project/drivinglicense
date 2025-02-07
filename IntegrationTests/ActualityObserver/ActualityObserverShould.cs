using Domain.DrivingLicenceAggregate;
using Domain.SharedKernel.ValueObjects;
using Infrastructure.Adapters.Postgres.ActualityObserver;
using MediatR;
using Microsoft.Extensions.Time.Testing;
using Moq;
using Npgsql;
using Quartz;
using Xunit;
using IMediator = MediatR.IMediator;

namespace IntegrationTests.ActualityObserver;

public class ActualityObserverShould : IntegrationTestBase
{
    [Fact]
    public async Task CallPublish()
    {
        // Arrange
        await AddExpiredDrivingLicense();
        
        var jobExecutionContextMock = new Mock<IJobExecutionContext>();
        
        var jobBuilder = new JobBuilder();
        jobBuilder.ConfigureDataSource(DataSource);
        var job = jobBuilder.Build();

        // Act
        await job.Execute(jobExecutionContextMock.Object);

        // Assert
        jobBuilder.VerifyCalls(1);
    }

    [Fact]
    public async Task DontCallPublishIfNotFoundExpiredDrivingLicenses()
    {
        // Arrange
        var jobExecutionContextMock = new Mock<IJobExecutionContext>();
        
        var jobBuilder = new JobBuilder();
        jobBuilder.ConfigureDataSource(DataSource);
        var job = jobBuilder.Build();

        // Act
        await job.Execute(jobExecutionContextMock.Object);

        // Assert
        jobBuilder.VerifyCalls(0);
    }

    private async Task AddExpiredDrivingLicense()
    {
        FakeTimeProvider fakeTimeProvider = new();
        fakeTimeProvider.SetUtcNow(new DateTimeOffset(DateTime.UtcNow.AddDays(-10)));
        
        var drivingLicense = DrivingLicense.Create(Guid.NewGuid(),
            CategoryList.Create([CategoryList.BCategory]),
            DrivingLicenseNumber.Create("1234 567891"), Name.Create("Иван", "Иванов", "Иванович"),
            City.Create("Москва"), new DateOnly(1990, 1, 1), new DateOnly(2020, 1, 1),
            CodeOfIssue.Create("1234"), 
            dateOfExpiry: DateOnly.FromDateTime(DateTime.UtcNow), 
            fakeTimeProvider);
        
        Context.Attach(drivingLicense.Status);
        Context.Attach(drivingLicense.CategoryList);
        await Context.DrivingLicenses.AddAsync(drivingLicense, TestContext.Current.CancellationToken);
        await Context.SaveChangesAsync();
    }

    
    private class JobBuilder
    {
        private NpgsqlDataSource _dataSource = null!;
        private readonly Mock<TimeProvider> _timeProviderMock = new();
        private readonly Mock<IMediator> _mediatorMock = new();

        public ActualityObserverBackgroundJob Build() =>
            new(_dataSource, _timeProviderMock.Object, _mediatorMock.Object);
        
        public void ConfigureDataSource(NpgsqlDataSource dataSource) => _dataSource = dataSource;

        public void VerifyCalls(int times) =>
            _mediatorMock.Verify(x => x.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()),
                Times.Exactly(times));
    }
}