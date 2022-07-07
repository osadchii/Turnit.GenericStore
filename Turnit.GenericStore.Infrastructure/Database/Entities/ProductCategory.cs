namespace Turnit.GenericStore.Infrastructure.Database.Entities;

public class ProductCategory
{
    public virtual Guid Id { get; set; }

    public virtual Product Product { get; set; }

    public virtual Category Category { get; set; }
}

