using System.Collections.Generic;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Turnit.GenericStore.Infrastructure.Mediatr.Categories.Queries;
using Turnit.GenericStore.Infrastructure.Models.Products;

namespace Turnit.GenericStore.Api.Features.Sales;

[Route("categories")]
public class CategoriesController : ApiControllerBase
{
    private readonly IMediator _mediator;

    public CategoriesController(IMediator mediator)
    {
        _mediator = mediator;
    }
    
    [HttpGet]
    public Task<IEnumerable<CategoryModel>> AllCategories()
    {
        return _mediator.Send(new GetAllCategoriesRequest());
    }
}