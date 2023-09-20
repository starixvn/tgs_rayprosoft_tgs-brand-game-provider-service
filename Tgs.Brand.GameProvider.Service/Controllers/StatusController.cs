using MediatR;
using Microsoft.AspNetCore.Mvc;
using Tgs.Brand.GameProvider.Core.Models;

namespace Tgs.Brand.GameProvider.Service.Controllers;

[Route("api/[controller]")]
[ApiController]
public class StatusController : ControllerBase
{
    private readonly IMediator _mediator;

    public StatusController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPut("change-by-schedule")]
    public Task ChangeByScheduleAsync()
    {
        return _mediator.Send(new BrandGameProviderScheduleRequest());
    }
}