using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Turnit.GenericStore.Infrastructure.Mediatr.Products.Commands;
using Turnit.GenericStore.Infrastructure.Mediatr.Products.Queries;
using Turnit.GenericStore.Infrastructure.Mediatr.Stores.Commands;
using Turnit.GenericStore.Infrastructure.Models.Products;
using Turnit.GenericStore.Infrastructure.Models.Stores;

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

    [HttpPut, Route("{productId:guid}/category/{categoryId:guid}")]
    public Task<IEnumerable<ProductModel>> AddProductToCategory(Guid productId, Guid categoryId)
    {
        return _mediator.Send(new AddProductToCategoryRequest(productId, categoryId));
    }

    [HttpDelete, Route("{productId:guid}/category/{categoryId:guid}")]
    public Task<IEnumerable<ProductModel>> RemoveProductFromCategory(Guid productId, Guid categoryId)
    {
        return _mediator.Send(new RemoveProductFromCategoryRequest(productId, categoryId));
    }

    [HttpPost, Route("{productId:guid}/book")]
    public async Task<IActionResult> BookProducts(Guid productId, [FromBody] IEnumerable<BookModel> bookModels)
    {
        await _mediator.Send(new BookProductsRequest(productId, bookModels));
        return Ok();
    }
}