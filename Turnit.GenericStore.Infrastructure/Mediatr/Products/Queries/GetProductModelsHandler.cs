using MediatR;
using NHibernate;
using NHibernate.Linq;
using Turnit.GenericStore.Infrastructure.Database.Entities;
using Turnit.GenericStore.Infrastructure.Models.Products;

namespace Turnit.GenericStore.Infrastructure.Mediatr.Products.Queries;

public class GetProductModelsRequest : IRequest<IEnumerable<ProductModel>>
{
    public bool FilterByCategory { get; }
    public Guid CategoryId { get; init; }

    public GetProductModelsRequest(Guid categoryId)
    {
        CategoryId = categoryId;
        FilterByCategory = true;
    }

    public GetProductModelsRequest()
    {
        FilterByCategory = false;
    }
}

public class GetProductModelsHandler : IRequestHandler<GetProductModelsRequest, IEnumerable<ProductModel>>
{
    private readonly ISession _session;
    private readonly IMediator _mediator;

    public GetProductModelsHandler(ISession session, IMediator mediator)
    {
        _session = session;
        _mediator = mediator;
    }

    public async Task<IEnumerable<ProductModel>> Handle(GetProductModelsRequest request, CancellationToken cancellationToken)
    {
        IQueryable<Product> query;

        if (request.FilterByCategory)
        {
            query = _session
                .Query<ProductCategory>()
                .Where(pc => pc.Category.Id == request.CategoryId)
                .Select(pc => pc.Product);
        }
        else
        {
            query = _session
                .Query<Product>();
        }
        
        var productModels = await query
            .Select(pc => new ProductModel
            {
                Id = pc.Id,
                Name = pc.Name
            })
            .ToListAsync(cancellationToken);

        var availabilities = await _mediator.Send(new GetProductAvailabilitiesRequest
        {
            ProductIds = productModels.Select(pm => pm.Id)
        }, cancellationToken);
        
        foreach (var productModel in productModels)
        {
            productModel.Availability = availabilities.TryGetValue(productModel.Id, out var availabilityModel) ? 
                availabilityModel : 
                Array.Empty<ProductModel.AvailabilityModel>();
        }
        
        return productModels;
    }
}