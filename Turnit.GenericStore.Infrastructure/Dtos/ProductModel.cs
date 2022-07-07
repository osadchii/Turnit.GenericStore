namespace Turnit.GenericStore.Infrastructure.Dtos;

public class ProductModel
{
    public Guid Id { get; set; }

    public string Name { get; set; }

    public IEnumerable<AvailabilityModel> Availability { get; set; }
    
    public class AvailabilityModel
    {
        public Guid StoreId { get; set; }
        
        public int Availability { get; set; }
    }
}

public class ProductCategoryModel
{
    public Guid? CategoryId { get; set; }

    public IEnumerable<ProductModel> Products { get; set; }
}