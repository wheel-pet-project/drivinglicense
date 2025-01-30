using System.Diagnostics.CodeAnalysis;
using FluentResults;

namespace Domain.SharedKernel.Errors;

[ExcludeFromCodeCoverage]
public class NotFound(string message) : Error(message);