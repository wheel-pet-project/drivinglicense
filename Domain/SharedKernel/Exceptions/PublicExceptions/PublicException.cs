using System.Diagnostics.CodeAnalysis;

namespace Domain.SharedKernel.Exceptions.PublicExceptions;

[ExcludeFromCodeCoverage]
public class PublicException(string message) : Exception(message);