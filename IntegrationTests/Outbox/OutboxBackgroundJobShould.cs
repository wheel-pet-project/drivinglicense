using Domain.DrivingLicenceAggregate.DomainEvents;
using Domain.SharedKernel.ValueObjects;
using Infrastructure.Adapters.Postgres.Outbox;
using JetBrains.Annotations;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using Npgsql;
using Quartz;
using Xunit;

namespace IntegrationTests.Outbox;

[TestSubject(typeof(OutboxBackgroundJob))]
public class OutboxBackgroundJobShould : IntegrationTestBase
{
    private readonly IReadOnlyList<OutboxEvent> _outboxEvents = new List<OutboxEvent>
    {
        new()
        {
            EventId = Guid.NewGuid(),
            Type = typeof(DrivingLicenseApprovedDomainEvent).ToString(),
            Content = JsonConvert.SerializeObject(
                new DrivingLicenseApprovedDomainEvent(Guid.NewGuid(), [CategoryList.BCategory]),
                new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All }),
            OccurredOnUtc = DateTime.UtcNow
        },
        new()
        {
            EventId = Guid.NewGuid(),
            Type = typeof(DrivingLicenseExpiredDomainEvent).ToString(),
            Content = JsonConvert.SerializeObject(
                new DrivingLicenseExpiredDomainEvent(Guid.NewGuid()),
                new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All }),
            OccurredOnUtc = DateTime.UtcNow
        }
    }.AsReadOnly();

    [Fact]
    public async Task MarkAsProcessedOutboxEvents()
    {
        // Arrange
        await Context.Outbox.AddRangeAsync(_outboxEvents, TestContext.Current.CancellationToken);
        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var jobExecutionContextMock = new Mock<IJobExecutionContext>();
        var jobBuilder = new JobBuilder();
        jobBuilder.ConfigureDataSource(DataSource);
        var job = jobBuilder.Build();

        // Act
        await job.Execute(jobExecutionContextMock.Object);

        // Assert
        var outboxEvents = Context.Outbox.ToList();
        Assert.True(outboxEvents.All(x => x.ProcessedOnUtc != null));
    }

    [Fact]
    private async Task MediatorPublishTwoTimesCall()
    {
        // Arrange
        await Context.Outbox.AddRangeAsync(_outboxEvents, TestContext.Current.CancellationToken);
        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var jobExecutionContextMock = new Mock<IJobExecutionContext>();
        var jobBuilder = new JobBuilder();
        jobBuilder.ConfigureDataSource(DataSource);
        var job = jobBuilder.Build();

        // Act
        await job.Execute(jobExecutionContextMock.Object);

        // Assert
        jobBuilder.VerifyMediatorPublishCall();
    }

    private class JobBuilder
    {
        private readonly Mock<IMediator> _mediatorMock = new();
        private readonly Mock<ILogger<OutboxBackgroundJob>> _logger = new();
        private NpgsqlDataSource _dataSource = null!;

        public OutboxBackgroundJob Build()
        {
            return new OutboxBackgroundJob(_dataSource, _mediatorMock.Object, _logger.Object);
        }

        public void ConfigureDataSource(NpgsqlDataSource dataSource)
        {
            _dataSource = dataSource;
        }

        public void VerifyMediatorPublishCall()
        {
            _mediatorMock.Verify(m => m.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()),
                Times.Exactly(2));
        }
    }
}