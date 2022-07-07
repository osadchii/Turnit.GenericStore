using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Turnit.GenericStore.Infrastructure.Mediatr.Stores.Commands;
using Turnit.GenericStore.Infrastructure.Mediatr.Stores.Queries;
using Turnit.GenericStore.Infrastructure.Models.Stores;

namespace Turnit.GenericStore.Api.Features.Sales;

[Route("stores")]
public class StoresController : ApiControllerBase
{
    private readonly IMediator _mediator;

    public StoresController(IMediator mediator)
    {
        _mediator = mediator;
    }
    
    [HttpGet]
    public Task<IEnumerable<StoreModel>> AllStores()
    {
        return _mediator.Send(new GetAllStoresRequest());
    }

    [HttpPost, Route("{storeId:guid}/restock")]
    public async Task<IActionResult> RestockProducts(Guid storeId, [FromBody] IEnumerable<RestockModel> restockModels)
    {
        await _mediator.Send(new RestockProductsRequest(storeId, restockModels));
        return Ok();
    }
}