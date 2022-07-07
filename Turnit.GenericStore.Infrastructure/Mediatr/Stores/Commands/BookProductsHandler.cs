using System.Data;
using System.Net;
using MediatR;
using NHibernate;
using NHibernate.Linq;
using Turnit.GenericStore.Infrastructure.Database.Entities;
using Turnit.GenericStore.Infrastructure.Models.Stores;

namespace Turnit.GenericStore.Infrastructure.Mediatr.Stores.Commands;

public class BookProductsRequest : IRequest<Unit>
{
    public Guid ProductId { get; }
    public IEnumerable<BookModel> BookModels { get; }

    public BookProductsRequest(Guid productId, IEnumerable<BookModel> bookModels)
    {
        ProductId = productId;
        BookModels = bookModels;
    }

}

public class BookProductsHandler : IRequestHandler<BookProductsRequest, Unit>
{
    private readonly ISession _session;
    private const string GreaterThenZeroError = "Quantity must be greater then 0";
    private const string DuplicateStoreIdError = "The request cannon contain duplicate store Ids";
    private const string NotEnoughError = "Not enough product in one or more stores";
    private const string ProductNotFoundError = "Product not found";
    private const string StoreNotFoundError = "One or more store not found";

    public BookProductsHandler(ISession session)
    {
        _session = session;
    }

    public async Task<Unit> Handle(BookProductsRequest request, CancellationToken cancellationToken)
    {
        if (request.BookModels.Any(bm => bm.Quantity <= 0))
        {
            throw new HttpRequestException(GreaterThenZeroError, null, HttpStatusCode.BadRequest);
        }

        var storeIds = request.BookModels
            .Select(bm => bm.StoreId)
            .Distinct()
            .ToArray();

        var bookModelsCount = request.BookModels.Count();

        if (storeIds.Length != bookModelsCount)
        {
            throw new HttpRequestException(DuplicateStoreIdError, null,
                HttpStatusCode.BadRequest);
        }

        var quantityByStock = request.BookModels.ToDictionary(bm => bm.StoreId, bm => bm.Quantity);

        using var transaction = _session.BeginTransaction(IsolationLevel.RepeatableRead);

        try
        {
            if (!await _session.Query<Product>().AnyAsync(s => s.Id == request.ProductId, cancellationToken))
            {
                throw new HttpRequestException(ProductNotFoundError, null,
                    HttpStatusCode.NotFound);
            }
            
            if (storeIds.Length != (await _session.Query<Store>().CountAsync(s => storeIds.Contains(s.Id), cancellationToken)))
            {
                throw new HttpRequestException(StoreNotFoundError, null,
                    HttpStatusCode.NotFound);
            }
            
            var availabilities = await _session.Query<ProductAvailability>()
                .Where(pa => pa.Product.Id == request.ProductId && storeIds.Contains(pa.Store.Id))
                .ToListAsync(cancellationToken);

            if (availabilities.Count < bookModelsCount)
            {
                throw new HttpRequestException(NotEnoughError, null,
                    HttpStatusCode.BadRequest);
            }

            foreach (var availability in availabilities)
            {
                availability.Availability -= quantityByStock[availability.Store.Id];

                if (availability.Availability < 0)
                {
                    throw new HttpRequestException(NotEnoughError, null, HttpStatusCode.BadRequest);
                }
                
                await _session.UpdateAsync(availability, cancellationToken);
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