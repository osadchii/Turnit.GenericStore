using System.Data;
using System.Net;
using MediatR;
using NHibernate;
using NHibernate.Linq;
using Turnit.GenericStore.Infrastructure.Database.Entities;
using Turnit.GenericStore.Infrastructure.Dtos;
using Turnit.GenericStore.Infrastructure.Mediatr.Products.Queries;

namespace Turnit.GenericStore.Infrastructure.Mediatr.Products.Commands;

public class AddProductToCategoryRequest : IRequest<IEnumerable<ProductModel>>
{
    public Guid ProductId { get; set; }
    public Guid CategoryId { get; set; }

    public AddProductToCategoryRequest(Guid productId, Guid categoryId)
    {
        ProductId = productId;
        CategoryId = categoryId;
    }
}

public class AddProductToCategoryHandler : IRequestHandler<AddProductToCategoryRequest, IEnumerable<ProductModel>>
{
    private readonly ISession _session;
    private readonly IMediator _mediator;

    public AddProductToCategoryHandler(ISession session, IMediator mediator)
    {
        _session = session;
        _mediator = mediator;
    }

    public async Task<IEnumerable<ProductModel>> Handle(AddProductToCategoryRequest request,
        CancellationToken cancellationToken)
    {
        using var transaction = _session.BeginTransaction(IsolationLevel.RepeatableRead);

        try
        {
            if (!await _session.Query<ProductCategory>().AnyAsync(pc =>
                    pc.Product.Id == request.ProductId && pc.Category.Id == request.CategoryId, cancellationToken))
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

                await _session.PersistAsync(new ProductCategory
                {
                    Category = new Category() { Id = request.CategoryId },
                    Product = new Product() { Id = request.ProductId }
                }, cancellationToken);
            }

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