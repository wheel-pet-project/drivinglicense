using System.Diagnostics.CodeAnalysis;

namespace Core.Domain.SharedKernel.Exceptions.ArgumentException;

[ExcludeFromCodeCoverage]
public class ValueIsInvalidException(string message) : ArgumentException(message);