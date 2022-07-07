using FluentNHibernate.Mapping;
using Turnit.GenericStore.Infrastructure.Database.Entities;

namespace Turnit.GenericStore.Infrastructure.Database.Mappings;

public class ProductMap : ClassMap<Product>
{
    public ProductMap()
    {
        Schema("public");
        Table("product");

        Id(x => x.Id, "id");
        Map(x => x.Name, "name");
        Map(x => x.Description, "description");
    }
}