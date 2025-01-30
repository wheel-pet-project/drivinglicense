using System.Diagnostics.CodeAnalysis;

namespace Domain.SharedKernel.Exceptions.DataConsistencyViolationException;

[ExcludeFromCodeCoverage]
public class DataConsistencyViolationException(string message = "Data consistency violation") : Exception(message);