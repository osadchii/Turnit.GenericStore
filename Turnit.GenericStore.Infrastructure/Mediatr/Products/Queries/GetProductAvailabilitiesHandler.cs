using MediatR;
using NHibernate;
using NHibernate.Linq;
using Turnit.GenericStore.Infrastructure.Database.Entities;
using Turnit.GenericStore.Infrastructure.Models.Products;

namespace Turnit.GenericStore.Infrastructure.Mediatr.Products.Queries;

public class GetProductAvailabilitiesRequest : IRequest<Dictionary<Guid, IEnumerable<ProductModel.AvailabilityModel>>>
{
    public IEnumerable<Guid> ProductIds { get; set; } = null!;
}

public class GetProductAvailabilitiesHandler : IRequestHandler<GetProductAvailabilitiesRequest, Dictionary<Guid, IEnumerable<ProductModel.AvailabilityModel>>>
{
    private readonly ISession _session;

    public GetProductAvailabilitiesHandler(ISession session)
    {
        _session = session;
    }

    public async Task<Dictionary<Guid, IEnumerable<ProductModel.AvailabilityModel>>> Handle(GetProductAvailabilitiesRequest request, CancellationToken cancellationToken)
    {
        return (await _session
                .Query<ProductAvailability>()
                .Where(av => request.ProductIds.Contains(av.Product.Id))
                .GroupBy(av => av.Product.Id)
                .ToListAsync(cancellationToken))
            .ToDictionary(g => g.Key, group => group.Select(v => new ProductModel.AvailabilityModel
            {
                Availability = v.Availability,
                StoreId = v.Store.Id
            }));
    }
}