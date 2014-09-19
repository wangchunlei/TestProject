using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ODataClient.OData.Models;

namespace ODataClient
{
    class Program
    {
        static void Main(string[] args)
        {
            var serviceUri = "http://192.168.70.118:8123/odata";
            var container = new Default.Container(new Uri(serviceUri));

            var product = new Product()
            {
                Name = "Yo-yo",
                Category = "Toys",
                Price = 4.95M
            };
            AddProduct(container, product);
            ListAllProducts(container);
            //var pr=container.Products.FirstOrDefault(p => p.Id == 1);
            //Console.WriteLine("{0} {1} {2}", pr.Name, pr.Price, pr.Category);
            Console.ReadKey(false);
        }

        static void ListAllProducts(Default.Container container)
        {
            foreach (var product in container.Products)
            {
                Console.WriteLine("{0} {1} {2}", product.Name, product.Price, product.Category);
            }
        }

        static void AddProduct(Default.Container container, ODataClient.OData.Models.Product product)
        {
            container.AddToProducts(product);
            var serviceResponse = container.SaveChanges();
            foreach (var operationResponse in serviceResponse)
            {
                Console.WriteLine("Response: {0}", operationResponse.StatusCode);
            }
        }
    }
}
