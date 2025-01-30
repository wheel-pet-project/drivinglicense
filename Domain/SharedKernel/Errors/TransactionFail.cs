using System.Diagnostics.CodeAnalysis;
using FluentResults;

namespace Domain.SharedKernel.Errors;

[ExcludeFromCodeCoverage]
public class TransactionFail(string message) : Error(message);