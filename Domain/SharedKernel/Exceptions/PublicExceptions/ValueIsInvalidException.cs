using System.Diagnostics.CodeAnalysis;

namespace Domain.SharedKernel.Exceptions.PublicExceptions;

[ExcludeFromCodeCoverage]
public class ValueIsInvalidException(string message) : PublicException(message);