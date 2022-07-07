using FluentNHibernate.Mapping;
using Turnit.GenericStore.Infrastructure.Database.Entities;

namespace Turnit.GenericStore.Infrastructure.Database.Mappings;

public class StoreMap : ClassMap<Store>
{
    public StoreMap()
    {
        Schema("public");
        Table("product");

        Id(x => x.Id, "id");
        Map(x => x.Name, "name");
    }
}