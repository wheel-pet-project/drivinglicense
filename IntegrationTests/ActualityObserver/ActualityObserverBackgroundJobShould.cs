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
using IMediator = MediatR.IMediator;

namespace IntegrationTests.ActualityObserver;

[TestSubject(typeof(ActualityObserverBackgroundJob))]
public class ActualityObserverBackgroundJobShould : IntegrationTestBase
{
    [Fact]
    public async Task AddExpiredDomainEventToOutbox()
    {
        // Arrange
        await AddExpiredDrivingLicense();

        var jobExecutionContextMock = new Mock<IJobExecutionContext>();

        var jobBuilder = new JobBuilder();
        jobBuilder.ConfigureContext(Context);
        var job = jobBuilder.Build();

        // Act
        await job.Execute(jobExecutionContextMock.Object);

        // Assert
        var domainEvent = await Context.Outbox.FirstOrDefaultAsync(TestContext.Current.CancellationToken);
        Assert.NotNull(domainEvent);
    }

    [Fact]
    public async Task DontAddExpiredDomainEventToOutbox()
    {
        // Arrange
        var jobExecutionContextMock = new Mock<IJobExecutionContext>();

        var jobBuilder = new JobBuilder();
        jobBuilder.ConfigureContext(Context);
        var job = jobBuilder.Build();

        // Act
        await job.Execute(jobExecutionContextMock.Object);

        // Assert
        var domainEvent = await Context.Outbox.FirstOrDefaultAsync(TestContext.Current.CancellationToken);
        Assert.Null(domainEvent);
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
        private readonly TimeProvider _timeProvider = TimeProvider.System;
        private readonly Mock<ILogger<ActualityObserverBackgroundJob>> _logger = new();
        
        private DataContext _context = null!;
        private Infrastructure.Adapters.Postgres.UnitOfWork _unitOfWork = null!;

        public ActualityObserverBackgroundJob Build()
        {
            return new ActualityObserverBackgroundJob(_context,
                new Infrastructure.Adapters.Postgres.UnitOfWork(_context), _timeProvider, _logger.Object);
        }

        public void ConfigureContext(DataContext context)
        {
            _context = context;
        }
    }
}