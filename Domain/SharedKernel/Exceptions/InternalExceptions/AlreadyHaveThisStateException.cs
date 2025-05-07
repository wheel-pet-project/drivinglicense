using System.Diagnostics.CodeAnalysis;

namespace Domain.SharedKernel.Exceptions.InternalExceptions;

[ExcludeFromCodeCoverage]
public class AlreadyHaveThisStateException(string message) : InternalException(message);