using System.Diagnostics.CodeAnalysis;

namespace Domain.SharedKernel.Exceptions.InternalExceptions;

[ExcludeFromCodeCoverage]
public class DomainRulesViolationException(string message) : InternalException(message);