using HostWorker.Models;
using MediatR;
using Microsoft.Extensions.Configuration;
using System.Net;

namespace Application.UseCases.Operation.Commands.Create;

public class CreateCommand : IRequest<Response<CreateResponse>>
{
}

public class CreateCommandHandler(
    IConfiguration configuration) :
    IRequestHandler<CreateCommand, Response<CreateResponse>>
{
    public async Task<Response<CreateResponse>> Handle(CreateCommand request, CancellationToken cancellationToken)
    {
        if (request != null)
        {
            return new Response<CreateResponse>
            {
                StatusCode = HttpStatusCode.NotFound,
                Content = new CreateResponse
                {
                    Result = true
                }
            };

        }

        return new Response<CreateResponse>
        {
            StatusCode = HttpStatusCode.NotFound,
            Content = new CreateResponse
            {
                Result = false
            }
        };

    }
}
