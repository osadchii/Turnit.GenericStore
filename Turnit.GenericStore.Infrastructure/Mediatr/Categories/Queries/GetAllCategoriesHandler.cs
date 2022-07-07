using MediatR;
using NHibernate;
using NHibernate.Linq;
using Turnit.GenericStore.Infrastructure.Database.Entities;
using Turnit.GenericStore.Infrastructure.Models.Products;

namespace Turnit.GenericStore.Infrastructure.Mediatr.Categories.Queries;

public class GetAllCategoriesRequest : IRequest<IEnumerable<CategoryModel>>
{
    
}

public class GetAllCategoriesHandler : IRequestHandler<GetAllCategoriesRequest, IEnumerable<CategoryModel>>
{
    private readonly ISession _session;

    public GetAllCategoriesHandler(ISession session)
    {
        _session = session;
    }

    public async Task<IEnumerable<CategoryModel>> Handle(GetAllCategoriesRequest request, CancellationToken cancellationToken)
    {
        return await _session
            .Query<Category>()
            .Select(c => new CategoryModel
            {
                Id = c.Id,
                Name = c.Name
            }).ToListAsync(cancellationToken);
    }
}