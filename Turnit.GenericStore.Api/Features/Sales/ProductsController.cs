using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Turnit.GenericStore.Infrastructure.Dtos;
using Turnit.GenericStore.Infrastructure.Mediatr.Products.Queries;

namespace Turnit.GenericStore.Api.Features.Sales;

[Route("products")]
public class ProductsController : ApiControllerBase
{
    private readonly IMediator _mediator;

    public ProductsController(IMediator mediator)
    {
        _mediator = mediator;
    }
    
    [HttpGet, Route("by-category/{categoryId:guid}")]
    public Task<IEnumerable<ProductModel>> ProductsByCategory(Guid categoryId)
    {
        return _mediator.Send(new GetProductModelsRequest(categoryId));
    }
    
    [HttpGet]
    public Task<IEnumerable<ProductCategoryModel>> AllProducts()
    {
        return _mediator.Send(new GetAllProductsRequest());
    }
}