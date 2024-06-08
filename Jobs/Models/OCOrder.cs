using System.Runtime.Serialization;

namespace IPBSyncAppNetCore.Jobs.Models
{
    public class OCOrder
    {
        [DataMember(Name = "order_id")]
        public int OrderId { get; set; }

        [DataMember(Name = "first_name")]
        public string? FirstName { get; set; }

        [DataMember(Name = "last_name")]
        public string? LastName { get; set; }

        [DataMember(Name = "email")]
        public string? Email { get; set; }

        [DataMember(Name = "telephone")]
        public string? Phone { get; set; }

        [DataMember(Name = "products")]
        public List<OCOrderProduct>? Products { get; set; }
        [DataMember(Name = "totals")]
        public List<OCOrderTotal>? Totals { get; set; }
    }

    public class OCOrderProduct
    {
        [DataMember(Name = "product_id")]
        public int Id { get; set; }

        [DataMember(Name = "name")]
        public string Name { get; set; }

        [DataMember(Name = "model")]
        public string Model { get; set; }

        [DataMember(Name = "quantity")]
        public int Quantity { get; set; }

        [DataMember(Name = "price")]
        public decimal Price { get; set; }

        [DataMember(Name = "total")]
        public decimal Total { get; set; }
        [DataMember(Name = "tax")]
        public decimal Tax { get; set; }
    }

    public class OCOrderTotal
    {
        [DataMember(Name = "code")]
        public string Code { get; set; }

        [DataMember(Name = "title")]
        public string Title { get; set; }

        [DataMember(Name = "value")]
        public decimal Value { get; set; }
    }
}
