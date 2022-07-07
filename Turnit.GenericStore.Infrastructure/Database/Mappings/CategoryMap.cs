using FluentNHibernate.Mapping;
using Turnit.GenericStore.Infrastructure.Database.Entities;

namespace Turnit.GenericStore.Infrastructure.Database.Mappings;

public class CategoryMap : ClassMap<Category>
{
    public CategoryMap()
    {
        Schema("public");
        Table("category");

        Id(x => x.Id, "id");
        Map(x => x.Name, "name");
    }
}