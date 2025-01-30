using System.Diagnostics.CodeAnalysis;

namespace Domain.SharedKernel.Exceptions.DomainRulesViolationException;

[ExcludeFromCodeCoverage]
public class DomainRulesViolationException(string message) : Exception(message);