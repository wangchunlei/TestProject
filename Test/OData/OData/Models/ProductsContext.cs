using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace OData.Models
{
    public class ProductsContext : DbContext
    {
        public ProductsContext()
            : base("ProductsContext")
        {

        }
        public DbSet<Product> Products { get; set; }
    }
}