using System.Diagnostics.CodeAnalysis;

namespace Domain.SharedKernel.Exceptions.InternalExceptions;

[ExcludeFromCodeCoverage]
public class DataConsistencyViolationException(string message = "Data consistency violation") 
    : InternalException(message);