using FluentResults;

namespace Domain.SharedKernel.Errors;

public class ObjectStorageUnavailable(string message) : Error(message);