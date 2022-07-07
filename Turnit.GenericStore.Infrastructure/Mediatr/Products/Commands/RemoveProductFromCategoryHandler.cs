using System.Data;
using System.Net;
using MediatR;
using NHibernate;
using NHibernate.Linq;
using Turnit.GenericStore.Infrastructure.Database.Entities;
using Turnit.GenericStore.Infrastructure.Dtos;
using Turnit.GenericStore.Infrastructure.Mediatr.Products.Queries;

namespace Turnit.GenericStore.Infrastructure.Mediatr.Products.Commands;

public class RemoveProductFromCategoryRequest : IRequest<IEnumerable<ProductModel>>
{
    public Guid ProductId { get;  }
    public Guid CategoryId { get;  }

    public RemoveProductFromCategoryRequest(Guid productId, Guid categoryId)
    {
        ProductId = productId;
        CategoryId = categoryId;
    }
}

public class RemoveProductFromCategoryHandler : IRequestHandler<RemoveProductFromCategoryRequest, IEnumerable<ProductModel>>
{
    private readonly ISession _session;
    private readonly IMediator _mediator;

    public RemoveProductFromCategoryHandler(ISession session, IMediator mediator)
    {
        _session = session;
        _mediator = mediator;
    }

    public async Task<IEnumerable<ProductModel>> Handle(RemoveProductFromCategoryRequest request, CancellationToken cancellationToken)
    {
        using var transaction = _session.BeginTransaction(IsolationLevel.RepeatableRead);

        try
        {
            if (!await _session.Query<Product>().AnyAsync(p => p.Id == request.ProductId, cancellationToken))
            {
                throw new HttpRequestException($"Product with {request.ProductId} id not found", null,
                    HttpStatusCode.NotFound);
            }

            if (!await _session.Query<Category>().AnyAsync(p => p.Id == request.CategoryId, cancellationToken))
            {
                throw new HttpRequestException($"Category with {request.ProductId} id not found", null,
                    HttpStatusCode.NotFound);
            }

            var productCategory = await _session.Query<ProductCategory>()
                .SingleOrDefaultAsync(pc => pc.Category.Id == request.CategoryId && pc.Product.Id == request.ProductId,
                    cancellationToken);
            
            if (productCategory is null)
            {
                throw new HttpRequestException($"Product with {request.ProductId} id not found in the category with {request.CategoryId} id", null,
                    HttpStatusCode.NotFound);
            }

            await _session.DeleteAsync(productCategory, cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }

        return await _mediator.Send(new GetProductModelsRequest(request.CategoryId), cancellationToken);
    }
}