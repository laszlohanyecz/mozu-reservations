using Mozu.Api.Contracts.CommerceRuntime.Orders;
using Mozu.Api.Contracts.ProductAdmin;
using Mozu.Api.Resources.Commerce;
using Mozu.Api.Resources.Commerce.Catalog.Admin;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace MozuProductReservationsReport
{
    class ReservationsReport
    {
        public void GenerateReport(MozuContext mozuContext, string outputFilePath)
        {
            List<ReservationRecord> records = new List<ReservationRecord>();

            // get all reservations
            var reservations = GetProductReservations(mozuContext);
            Console.WriteLine("Loaded {0} reservations.", reservations == null ? 0 : reservations.Length);

            int ix = 0;
            // loop through reservations
            foreach (var reservation in reservations)
            {
                if (ix++ % 5 == 0)
                {
                    Console.Write(".");
                }

                ReservationRecord r = new ReservationRecord()
                {
                    ProductReservationId = reservation.Id,
                    LocationCode = reservation.LocationCode,
                    OrderId = reservation.OrderId,
                    OrderNumber = null, // filled in below
                    ProductCode = reservation.ProductCode,
                    Quantity = reservation.Quantity,
                    ProductName = null, // filled in below
                    ReservationCreateDate = reservation.AuditInfo == null ? null : reservation.AuditInfo.CreateDate,
                    OrderSubmittedDate = null, // filled in below
                    OrderStatus = null, // filled in below
                    OrderFulfillmentStatus = null // filled in below
                };

                if (reservation.AuditInfo != null)
                {
                    r.ReservationCreateDate = reservation.AuditInfo.CreateDate;
                }

                // look up order
                var order = GetOrderById(mozuContext, reservation.OrderId);
                if (order == null)
                {
                    Console.WriteLine("Order {0} not found", reservation.OrderId);
                }
                else
                {
                    r.OrderNumber = order.OrderNumber.Value.ToString();
                    r.OrderSubmittedDate = order.SubmittedDate;
                    r.OrderStatus = order.Status;
                    r.OrderFulfillmentStatus = order.FulfillmentStatus;

                    // find the order line
                    var orderLine = order.Items.Where(o => o.Id == reservation.OrderItemId).FirstOrDefault();
                    if (orderLine == null)
                    {
                        Console.WriteLine("OrderItemId {0} not found", reservation.OrderItemId);
                        r.ProductName = String.Format("[OrderItemId = {0} not found]", reservation.OrderItemId);
                    }
                    else
                    {
                        r.Quantity = orderLine.Quantity;

                        if (orderLine.Product != null)
                        {
                            r.ProductName = orderLine.Product.Name;

                            if (orderLine.Product.BundledProducts != null && orderLine.Product.BundledProducts.Count > 0)
                            {
                                // bundle component
                                var product = GetProductById(mozuContext, reservation.ProductCode);
                                if (product != null && product.Content != null)
                                {
                                    r.ProductName = String.Format("{0} ({1})", product.Content.ProductName, orderLine.Product.Name);
                                }
                            }
                        }
                    }
                }

                records.Add(r);
            }
            Console.WriteLine(".");

            string s = CSVUtil.ToCSV(records.ToArray());
            File.WriteAllText(outputFilePath, s, Encoding.UTF8);
        }

        #region ReservationRecord
        public class ReservationRecord
        {
            public int? ProductReservationId { get; set; }
            public string LocationCode { get; set; }
            public string OrderId { get; set; }
            public string OrderNumber { get; set; }
            public string ProductCode { get; set; }
            public int Quantity { get; set; }
            public string ProductName { get; set; }
            public DateTime? ReservationCreateDate { get; set; }
            public DateTime? OrderSubmittedDate { get; set; }
            public string OrderStatus { get; set; }
            public string OrderFulfillmentStatus { get; set; }
        }
        #endregion

        #region Mozu webservices

#pragma warning disable 618

        #region GetProductReservations
        public ProductReservation[] GetProductReservations(MozuContext ctx, string filter = null)
        {
            List<ProductReservation> list = new List<ProductReservation>();

            ProductReservationResource resource = new ProductReservationResource(ctx.GetApiContext());
            int pageSize = 1000; // will clamp to 200
            int startIndex = 0;
            int pageCount = 0;
            int currentPage = 0;
            do
            {
                ProductReservationCollection collection = resource.GetProductReservations(pageSize: pageSize, startIndex: startIndex, filter: filter);

                if (collection == null || collection.Items == null || collection.Items.Count < 1)
                {
                    return null;
                }
                if (collection.PageSize != pageSize)
                {
                    // clamp our page size if it was too big
                    pageSize = collection.PageSize;
                }
                if (pageCount == 0)
                {
                    pageCount = collection.PageCount;
                }

                foreach (ProductReservation p in collection.Items)
                {
                    list.Add(p);
                }

                currentPage++;
                startIndex += pageSize;
            }
            while (currentPage < pageCount);

            return list.ToArray();
        }
        #endregion

        #region GetOrderById
        private Dictionary<string, Order> _orders = new Dictionary<string, Order>();
        private Order GetOrderById(MozuContext mozuContext, string id)
        {
            if (!_orders.ContainsKey(id))
            {
                OrderResource resource = new OrderResource(mozuContext.GetApiContext());
                Order o = resource.GetOrder(id);
                _orders[id] = o;
            }
            return _orders[id];
        }
        #endregion

        #region GetProductById
        private Dictionary<string, Product> _products = new Dictionary<string, Product>();
        private Product GetProductById(MozuContext mozuContext, string productCode)
        {
            if (!_products.ContainsKey(productCode))
            {
                ProductResource resource = new ProductResource(mozuContext.GetApiContext());
                Product p = resource.GetProduct(productCode);
                _products[productCode] = p;
            }
            return _products[productCode];
        }
        #endregion

#pragma warning restore 618

        #endregion
    }
}
