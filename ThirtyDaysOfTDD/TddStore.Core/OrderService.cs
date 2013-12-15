using System;
using System.Collections.Generic;
using System.Linq;
using TddStore.Core.Exceptions;
namespace TddStore.Core
{
    public class OrderService
    {
        private IOrderDataService _orderDataService;
        private ICustomerService _customerService;
        private IOrderFulfillmentService _orderFulfillmentService;
        private const string USERNAME = "u";
        private const string PASSWORD = "p";
        public OrderService(IOrderDataService orderDataService, ICustomerService customerService, IOrderFulfillmentService orderFulfillmentService)
        {
            _orderDataService = orderDataService;
            _customerService = customerService;
            _orderFulfillmentService = orderFulfillmentService;
        }

        public object PlaceOrder(Guid customerId, ShoppingCart shoppingCart)
        {

            if (shoppingCart.Items.Where(el => el.Quantity == 0).Count() > 0)
            {
                throw new InvalidOrderException();
            }
            var customer = _customerService.GetCustomer(customerId);

            var orderFulfillmentSessionId = _orderFulfillmentService.OpenSession(USERNAME, PASSWORD);
            var firstItemId = shoppingCart.Items.First().ItemId;
            var firstItemQuantity = shoppingCart.Items.First().Quantity;
            var itemIsInInventory = _orderFulfillmentService.IsInInventory(orderFulfillmentSessionId, firstItemId, firstItemQuantity);
            var orderForFulfillmentService = new Dictionary<Guid, int>();
            orderForFulfillmentService.Add(firstItemId, firstItemQuantity);

            _orderFulfillmentService.PlaceOrder(orderFulfillmentSessionId, orderForFulfillmentService, customer.ShippingAddress.ToString());
            _orderFulfillmentService.CloseSession(orderFulfillmentSessionId);
            var order = new Order();
            return _orderDataService.Save(order);
        }
    }
}