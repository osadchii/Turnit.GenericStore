using MediatR;
using NHibernate;
using NHibernate.Linq;
using Turnit.GenericStore.Infrastructure.Database.Entities;
using Turnit.GenericStore.Infrastructure.Dtos;

namespace Turnit.GenericStore.Infrastructure.Mediatr.Products.Queries;

public class GetAllProductsRequest : IRequest<IEnumerable<ProductCategoryModel>>
{
}

public class GetAllProductsHandler : IRequestHandler<GetAllProductsRequest, IEnumerable<ProductCategoryModel>>
{
    private readonly ISession _session;
    private readonly IMediator _mediator;

    public GetAllProductsHandler(ISession session, IMediator mediator)
    {
        _session = session;
        _mediator = mediator;
    }

    public async Task<IEnumerable<ProductCategoryModel>> Handle(GetAllProductsRequest request,
        CancellationToken cancellationToken)
    {
        var products = (await _mediator.Send(new GetProductModelsRequest(), cancellationToken))
            .ToDictionary(p => p.Id, p => p);

        var categories = await _session.Query<ProductCategory>()
            .GroupBy(c => c.Category.Id)
            .ToListAsync(cancellationToken);

        var result = categories.Select(category => new ProductCategoryModel
        {
            CategoryId = category.Key,
            Products = category.Select(productCategory => products[productCategory.Product.Id])
        });

        return result;
    }
}