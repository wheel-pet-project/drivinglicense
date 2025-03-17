namespace Infrastructure.Adapters.Kafka;

public class KafkaTopicsConfiguration
{
    public required string DrivingLicenseApprovedTopic { get; set; }
    public required string DrivingLicenseExpiredTopic { get; set; }
}