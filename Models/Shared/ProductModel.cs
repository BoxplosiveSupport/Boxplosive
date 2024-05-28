using MessagePack;

namespace nl.boxplosive.BackOffice.Mvc.Models.Shared
{
    public enum ProductType
    {
        Product,
        ProductGroup
    }

    [MessagePackObject(keyAsPropertyName: true)]
    public class ProductModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public bool Remove { get; set; }

        public ProductType Type { get; set; }
    }
}