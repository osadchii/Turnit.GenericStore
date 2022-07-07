using System.Data;
using System.Net;
using MediatR;
using NHibernate;
using NHibernate.Linq;
using Turnit.GenericStore.Infrastructure.Database.Entities;
using Turnit.GenericStore.Infrastructure.Models.Stores;

namespace Turnit.GenericStore.Infrastructure.Mediatr.Stores.Commands;

public class RestockProductsRequest : IRequest<Unit>
{
    public Guid StoreId { get; }
    public IEnumerable<RestockModel> RestockModels { get; }

    public RestockProductsRequest(Guid storeId, IEnumerable<RestockModel> restockModels)
    {
        StoreId = storeId;
        RestockModels = restockModels;
    }
}

public class RestockProductsHandler : IRequestHandler<RestockProductsRequest, Unit>
{
    private readonly ISession _session;
    private const string GreaterThenZeroError = "Quantity must be greater then 0";
    private const string DuplicateProductIdError = "The request cannon contain duplicate product Ids";
    private const string StoreNotFoundError = "Store not found";
    private const string ProductNotFoundError = "One or more product not found";

    public RestockProductsHandler(ISession session)
    {
        _session = session;
    }

    public async Task<Unit> Handle(RestockProductsRequest request, CancellationToken cancellationToken)
    {
        if (request.RestockModels.Any(bm => bm.Quantity <= 0))
        {
            throw new HttpRequestException(GreaterThenZeroError, null, HttpStatusCode.BadRequest);
        }

        var productIds = request.RestockModels
            .Select(bm => bm.ProductId)
            .Distinct()
            .ToArray();

        var restockModelCount = request.RestockModels.Count();

        if (productIds.Length != restockModelCount)
        {
            throw new HttpRequestException(DuplicateProductIdError, null,
                HttpStatusCode.BadRequest);
        }

        var quantityByProduct = request.RestockModels.ToDictionary(bm => bm.ProductId, bm => bm.Quantity);

        using var transaction = _session.BeginTransaction(IsolationLevel.RepeatableRead);

        try
        {
            if (!await _session.Query<Store>().AnyAsync(s => s.Id == request.StoreId, cancellationToken))
            {
                throw new HttpRequestException(StoreNotFoundError, null,
                    HttpStatusCode.NotFound);
            }
            
            if (productIds.Length != (await _session.Query<Product>().CountAsync(s => productIds.Contains(s.Id), cancellationToken)))
            {
                throw new HttpRequestException(ProductNotFoundError, null,
                    HttpStatusCode.NotFound);
            }
            
            var availabilities = await _session.Query<ProductAvailability>()
                .Where(pa => pa.Store.Id == request.StoreId && productIds.Contains(pa.Product.Id))
                .ToListAsync(cancellationToken);

            foreach (var availability in availabilities)
            {
                availability.Availability += quantityByProduct[availability.Product.Id];
                quantityByProduct[availability.Product.Id] = 0;
                await _session.UpdateAsync(availability, cancellationToken);
            }

            foreach (var keyValue in quantityByProduct.Where(kv => kv.Value > 0))
            {
                await _session.PersistAsync(new ProductAvailability
                {
                    Product = new Product() { Id = keyValue.Key },
                    Store = new Store() { Id = request.StoreId },
                    Availability = keyValue.Value
                }, cancellationToken);
            }

            await transaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }

        return Unit.Value;
    }
}