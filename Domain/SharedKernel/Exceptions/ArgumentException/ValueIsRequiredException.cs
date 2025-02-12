using System.Diagnostics.CodeAnalysis;

namespace Domain.SharedKernel.Exceptions.ArgumentException;

[ExcludeFromCodeCoverage]
public class ValueIsRequiredException(string message = "Value is required") : ArgumentException(message);