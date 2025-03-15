using Domain.DrivingLicenceAggregate.DomainEvents;
using Domain.SharedKernel;
using Domain.SharedKernel.ValueObjects;
using Infrastructure.Adapters.Postgres.Outbox;
using JetBrains.Annotations;
using MediatR;
using Microsoft.EntityFrameworkCore;
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
    private readonly DomainEvent _domainEvent =
        new DrivingLicenseApprovedDomainEvent(Guid.NewGuid(), [CategoryList.BCategory]);

    [Fact]
    public async Task CallMediatorPublishMethod()
    {
        // Arrange
        await AddDomainEventToDb(_domainEvent);
        var jobExecutionContextMock = new Mock<IJobExecutionContext>();
        var jobBuilder = new JobBuilder();
        var job = jobBuilder.Build(DataSource);

        // Act
        await job.Execute(jobExecutionContextMock.Object);

        // Assert
        jobBuilder.VerifyMediatorCalls(1);
    }

    [Fact]
    public async Task SetProcessedField()
    {
        // Arrange
        await AddDomainEventToDb(_domainEvent);
        var jobExecutionContextMock = new Mock<IJobExecutionContext>();
        var jobBuilder = new JobBuilder();
        var job = jobBuilder.Build(DataSource);

        // Act
        await job.Execute(jobExecutionContextMock.Object);

        // Assert
        var eventFromDb = await Context.Outbox.FirstOrDefaultAsync(TestContext.Current.CancellationToken);
        Assert.NotNull(eventFromDb.ProcessedOnUtc);
    }

    private async Task AddDomainEventToDb(DomainEvent domainEvent)
    {
        var jsonSerializerSettings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All };
        var outboxEvent = new OutboxEvent
        {
            EventId = domainEvent.EventId,
            Type = domainEvent.GetType().ToString(),
            Content = JsonConvert.SerializeObject(domainEvent, jsonSerializerSettings),
            OccurredOnUtc = DateTime.UtcNow
        };

        await Context.Outbox.AddAsync(outboxEvent);
        await Context.SaveChangesAsync();
        Context.ChangeTracker.Clear();
    }

    private class JobBuilder
    {
        private readonly Mock<IMediator> _mediatorMock = new();
        private readonly Mock<ILogger<OutboxBackgroundJob>> _loggerMock = new();

        public OutboxBackgroundJob Build(NpgsqlDataSource dataSource)
        {
            return new OutboxBackgroundJob(dataSource, _mediatorMock.Object, _loggerMock.Object);
        }

        public void VerifyMediatorCalls(int times)
        {
            _mediatorMock.Verify(x => x.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()),
                Times.Exactly(times));
        }
    }
}