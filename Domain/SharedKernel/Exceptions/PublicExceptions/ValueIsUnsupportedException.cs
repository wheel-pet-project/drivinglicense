using System.Diagnostics.CodeAnalysis;

namespace Domain.SharedKernel.Exceptions.PublicExceptions;

[ExcludeFromCodeCoverage]
public class ValueIsUnsupportedException(string message = "Value is unsupported") : PublicException(message);