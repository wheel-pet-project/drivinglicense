using System.Diagnostics.CodeAnalysis;

namespace Domain.SharedKernel.Exceptions.AlreadyHaveThisState;

[ExcludeFromCodeCoverage]
public class AlreadyHaveThisStateException(string message) : Exception(message);