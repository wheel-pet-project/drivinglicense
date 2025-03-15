using Domain.DrivingLicenceAggregate;
using Domain.SharedKernel.ValueObjects;
using Infrastructure.Adapters.Postgres;
using Infrastructure.Adapters.Postgres.ActualityObserver;
using JetBrains.Annotations;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Time.Testing;
using Moq;
using Npgsql;
using Quartz;
using Xunit;

namespace IntegrationTests.ActualityObserver;

[TestSubject(typeof(ActualityObserverBackgroundJob))]
public class ActualityObserverBackgroundJobShould : IntegrationTestBase
{
    [Fact]
    public async Task CallMediatorIfFoundExpiredDrivingLicense()
    {
        // Arrange
        await AddExpiredDrivingLicense();

        var timeProvider = TimeProvider.System;
        var jobBuilder = new JobBuilder();
        var job = jobBuilder.Build(DataSource, timeProvider);
        var jobExecutionContextMock = new Mock<IJobExecutionContext>();

        // Act
        await job.Execute(jobExecutionContextMock.Object);

        // Assert
        jobBuilder.VerifyMediatorCalls(1);
    }

    [Fact]
    public async Task NotCallMediatorIfNotFoundExpiredDrivingLicense()
    {
        var timeProvider = TimeProvider.System;
        var jobBuilder = new JobBuilder();
        var job = jobBuilder.Build(DataSource, timeProvider);
        var jobExecutionContextMock = new Mock<IJobExecutionContext>();

        // Act
        await job.Execute(jobExecutionContextMock.Object);

        // Assert
        jobBuilder.VerifyMediatorCalls(0);
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
            DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1)),
            fakeTimeProvider);

        drivingLicense.MarkAsPendingProcessing();
        drivingLicense.Approve();

        Context.Attach(drivingLicense.Status);
        Context.Attach(drivingLicense.CategoryList);
        await Context.DrivingLicenses.AddAsync(drivingLicense, TestContext.Current.CancellationToken);
        await Context.SaveChangesAsync();
    }

    private class JobBuilder
    {
        private readonly Mock<IMediator> _mediatorMock = new();
        private readonly Mock<ILogger<ActualityObserverBackgroundJob>> _loggerMock = new();

        public ActualityObserverBackgroundJob Build(NpgsqlDataSource dataSource, TimeProvider timeProvider)
        {
            return new ActualityObserverBackgroundJob(dataSource, _mediatorMock.Object, timeProvider,
                _loggerMock.Object);
        }

        public void VerifyMediatorCalls(int times)
        {
            _mediatorMock.Verify(x => x.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()),
                Times.Exactly(times));
        }
    }
}