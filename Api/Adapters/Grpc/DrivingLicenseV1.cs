using Application.UseCases.Commands.ApproveDrivingLicense;
using Application.UseCases.Commands.RejectDrivingLicense;
using Application.UseCases.Commands.UploadDrivingLicense;
using Application.UseCases.Commands.UploadPhotos;
using Application.UseCases.Queries.GetAllDrivingLicenses;
using Application.UseCases.Queries.GetByIdDrivingLicense;
using Domain.SharedKernel.Errors;
using FluentResults;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using MediatR;
using Proto.DrivingLicenseV1;
using Status = Grpc.Core.Status;

namespace Api.Adapters.Grpc;

public class DrivingLicenseV1(IMediator mediator, Mapper.Mapper mapper) : DrivingLicense.DrivingLicenseBase
{
    public override async Task<GetLicenseByIdResponse> GetLicenseById(GetLicenseByIdRequest request, 
        ServerCallContext context)
    {
        var getDrivingLicenseByIdQuery = new GetByIdDrivingLicenseQuery(
            CorrelationId: Guid.Parse(request.CorId.AsSpan()), 
            Id: Guid.Parse(request.Id.AsSpan()));
        
        var result = await mediator.Send(getDrivingLicenseByIdQuery, context.CancellationToken);
        
        if (result.IsFailed) 
            return ParseErrorToRpcException<GetLicenseByIdResponse>(result.Errors);
        
        var resultView = result.Value.DrivingLicenseView;
        var response = new GetLicenseByIdResponse
        {
            Id = resultView.Id.ToString(),
            AccId = resultView.AccountId.ToString(),
            Status = mapper.DomainStatusToProtoStatus(resultView.Status),
            Name = resultView.Name,
            Number = resultView.Number,
            CityOfBirth = resultView.CityOfBirth,
            DateOfBirth = Timestamp.FromDateTime(
                resultView.DateOfBirth.ToDateTime(new TimeOnly(), DateTimeKind.Utc)),
            CodeOfIssue = resultView.CodeOfIssue,
            DateOfIssue = Timestamp.FromDateTime(
                resultView.DateOfIssue.ToDateTime(new TimeOnly(), DateTimeKind.Utc)),
            DateOfExpiry = Timestamp.FromDateTime(
                resultView.DateOfExpiry.ToDateTime(new TimeOnly(), DateTimeKind.Utc))
        };

        if (resultView is { FrontPhotoBytes: not null, BackPhotoBytes: not null })
        {
            response.FrontPhoto = await ByteString.FromStreamAsync(new MemoryStream(resultView.FrontPhotoBytes));
            response.BackPhoto = await ByteString.FromStreamAsync(new MemoryStream(resultView.BackPhotoBytes));
        }
            
        var categories = new RepeatedField<string>
            { resultView.CategoryList.Select(x => new string(x, 1)).AsEnumerable() };
        response.Categories.AddRange(categories);
            
        return response;
    }

    public override async Task<GetAllLicensesResponse> GetAllLicensesById(GetAllLicensesRequest request, 
        ServerCallContext context)
    {
        var getAllDrivingLicensesQuery = new GetAllDrivingLicensesQuery(
            CorrelationId: Guid.Parse(request.CorId.AsSpan()),
            Page: request.Page, 
            PageSize: request.PageSize,
            FilteringStatus: mapper.ProtoStatusToDomainStatus(request.FilteringStatus));

        var result = await mediator.Send(getAllDrivingLicensesQuery, context.CancellationToken);

        var response = new GetAllLicensesResponse();
        if (result.Value.DrivingLicenses.Count > 0)
        {
            response.Licenses.AddRange(result.Value.DrivingLicenses.Select(x =>
                new GetAllLicensesResponse.Types.LicenseShortView
                {
                    Id = x.Id.ToString(),
                    AccountId = x.AccountId.ToString(),
                    Name = x.Name,
                    Status = mapper.DomainStatusToProtoStatus(x.Status),
                }));
        }
        return response;
    }

    public override async Task<UploadLicenseResponse> UploadLicense(UploadLicenseRequest request, 
        ServerCallContext context)
    {
        var uploadDrivingLicenseCommand = new UploadDrivingLicenseCommand(
            CorrelationId: Guid.Parse(request.CorId.AsSpan()),
            AccountId: Guid.Parse(request.AccId.AsSpan()), 
            CategoryList: [..request.Categories.Select(char.Parse)],
            Number: request.Number, 
            FirstName: request.FirstName, 
            LastName: request.LastName, 
            Patronymic: request.Patronymic == string.Empty ? null : request.Patronymic, 
            CityOfBirth: request.CityOfBirth,
            DateOfBirth: DateOnly.FromDateTime(request.DateOfBirth.ToDateTime()),
            DateOfIssue: DateOnly.FromDateTime(request.DateOfIssue.ToDateTime()), 
            CodeOfIssue: request.CodeOfIssue,
            DateOfExpiry: DateOnly.FromDateTime(request.DateOfExpiry.ToDateTime()));
        
        var result = await mediator.Send(uploadDrivingLicenseCommand, context.CancellationToken);
        
        return result.IsSuccess 
            ? new UploadLicenseResponse { Id = result.Value.DrivingLicenseId.ToString() }
            : ParseErrorToRpcException<UploadLicenseResponse>(result.Errors);
    }

    public override async Task<UploadPhotosResponse> UploadPhotos(UploadPhotosRequest request, 
        ServerCallContext context)
    {
        var uploadPhotosCommand = new UploadPhotosCommand(
            CorrelationId: Guid.Parse(request.CorId.AsSpan()),
            DrivingLicenseId: Guid.Parse(request.LicenseId.AsSpan()), 
            FrontPhotoBytes: request.FrontPhoto.ToByteArray(), 
            BackPhotoBytes: request.BackPhoto.ToByteArray());
        
        var result = await mediator.Send(uploadPhotosCommand, context.CancellationToken);
        
        return result.IsSuccess
            ? new UploadPhotosResponse()
            : ParseErrorToRpcException<UploadPhotosResponse>(result.Errors);
    }

    public override async Task<ApproveLicenseResponse> ApproveLicense(ApproveLicenseRequest request, 
        ServerCallContext context)
    {
        var approveDrivingLicenseCommand = new ApproveDrivingLicenseCommand(
            CorrelationId: Guid.Parse(request.CorId.AsSpan()),
            DrivingLicenseId: Guid.Parse(request.LicenseId.AsSpan()));
        
        var result = await mediator.Send(approveDrivingLicenseCommand, context.CancellationToken);
        
        return result.IsSuccess
            ? new ApproveLicenseResponse()
            : ParseErrorToRpcException<ApproveLicenseResponse>(result.Errors);
    }

    public override async Task<RejectLicenseResponse> RejectLicense(RejectLicenseRequest request, 
        ServerCallContext context)
    {
        var rejectDrivingLicenseCommand = new RejectDrivingLicenseCommand(
            CorrelationId: Guid.Parse(request.CorId.AsSpan()), 
            DrivingLicenseId: Guid.Parse(request.LicenseId.AsSpan()));
        
        var result = await mediator.Send(rejectDrivingLicenseCommand, context.CancellationToken);
        
        return result.IsSuccess
            ? new RejectLicenseResponse()
            : ParseErrorToRpcException<RejectLicenseResponse>(result.Errors);
    }
    
    private T ParseErrorToRpcException<T>(List<IError> errors)
    {
        if (errors.Exists(x => x is NotFound))
            throw new RpcException(new Status(StatusCode.NotFound, string.Join(' ', errors.Select(x => x.Message))));
        
        if (errors.Exists(x => x is ObjectStorageUnavailable))
            throw new RpcException(new Status(StatusCode.Unavailable, string.Join(' ', errors.Select(x => x.Message))));
        
        throw new RpcException(new Status(StatusCode.InvalidArgument, string.Join(' ', errors.Select(x => x.Message))));
    }
}