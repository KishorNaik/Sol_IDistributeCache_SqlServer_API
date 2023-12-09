using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Sol_Demo.Features;
using System.Net;
using System.Threading;

namespace Sol_Demo.Controllers;

[Route("api/v1/users")]
[ApiController]
public class UserController : ControllerBase
{
    private readonly IMediator mediator = null;

    public UserController(IMediator mediator)
    {
        this.mediator = mediator;
    }

    [HttpPost("create")]
    public async Task<IActionResult> Create([FromBody] CreateUserCommand createUserCommand, CancellationToken cancellationToken)
    {
        try
        {
            var flag = await mediator.Send(createUserCommand, cancellationToken);
            if (flag == false)
                return base.StatusCode((int)HttpStatusCode.InternalServerError, "Something went wrong");

            return base.StatusCode((int)HttpStatusCode.OK, "Data Succesfully saved.");
        }
        catch (Exception ex)
        {
            return base.StatusCode((int)HttpStatusCode.InternalServerError, ex.Message);
        }
    }

    [HttpGet("get/{id}")]
    public async Task<IActionResult> Get([FromRoute] decimal id, CancellationToken cancellationToken)
    {
        try
        {
            var user = await mediator.Send(new GetUserByIdQuery(id), cancellationToken);
            if (user == null)
                return base.StatusCode((int)HttpStatusCode.InternalServerError, "Not Found");

            return base.StatusCode((int)HttpStatusCode.OK, user);
        }
        catch (Exception ex)
        {
            return base.StatusCode((int)HttpStatusCode.InternalServerError, ex.Message);
        }
    }
}