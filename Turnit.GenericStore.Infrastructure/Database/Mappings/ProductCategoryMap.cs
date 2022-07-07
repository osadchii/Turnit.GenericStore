using FluentNHibernate.Mapping;
using Turnit.GenericStore.Infrastructure.Database.Entities;

namespace Turnit.GenericStore.Infrastructure.Database.Mappings;

public class ProductCategoryMap : ClassMap<ProductCategory>
{
    public ProductCategoryMap()
    {
        Schema("public");
        Table("product_category");

        Id(x => x.Id, "id");
        References(x => x.Category, "category_id");
        References(x => x.Product, "product_id");
    }
}