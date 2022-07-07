using MediatR;
using NHibernate;
using NHibernate.Linq;
using Turnit.GenericStore.Infrastructure.Database.Entities;
using Turnit.GenericStore.Infrastructure.Models.Stores;

namespace Turnit.GenericStore.Infrastructure.Mediatr.Stores.Queries;

public class GetAllStoresRequest : IRequest<IEnumerable<StoreModel>>
{
    
}

public class GetAllStoresHandler : IRequestHandler<GetAllStoresRequest, IEnumerable<StoreModel>>
{
    private readonly ISession _session;

    public GetAllStoresHandler(ISession session)
    {
        _session = session;
    }

    public async Task<IEnumerable<StoreModel>> Handle(GetAllStoresRequest request, CancellationToken cancellationToken)
    {
        return await _session.Query<Store>()
            .Select(s => new StoreModel
            {
                Id = s.Id,
                Name = s.Name
            })
            .ToListAsync(cancellationToken);
    }
}