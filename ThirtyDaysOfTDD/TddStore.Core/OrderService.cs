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
            PlaceOrderWithFulfillmentService(shoppingCart, customer);
            var order = new Order();
            return _orderDataService.Save(order);
        }

        private void PlaceOrderWithFulfillmentService(ShoppingCart shoppingCart, Customer customer)
        {
            //Open Session
            var orderFulfillmentSessionId = OpenOrderFulfillmentSession();
            PlaceOrderWithFulfillmentService(orderFulfillmentSessionId, shoppingCart, customer);
            //Close session
            CloseOrderFulfillmentService(orderFulfillmentSessionId);
        }

        private void PlaceOrderWithFulfillmentService(Guid orderFulfillmentSessionId, ShoppingCart shoppingCart, Customer customer)
        {
            var orderForFulfillmentService = CheckInventoryLevels(orderFulfillmentSessionId, shoppingCart);
            //Place Order
            _orderFulfillmentService.PlaceOrder(orderFulfillmentSessionId, orderForFulfillmentService, customer.ShippingAddress.ToString());
        }

        private Dictionary<Guid, int> CheckInventoryLevels(Guid orderFulfillmentSessionId, ShoppingCart shoppingCart)
        {

            var itemsInInventory = shoppingCart.Items.Where(item => _orderFulfillmentService.IsInInventory(orderFulfillmentSessionId, item.ItemId, item.Quantity));
            var orderForFulfillmentService = new Dictionary<Guid, int>();
            itemsInInventory.ToList().ForEach(item => { orderForFulfillmentService.Add(item.ItemId, item.Quantity); });

            return orderForFulfillmentService;
        }

        private void CloseOrderFulfillmentService(Guid orderFulfillmentSessionId)
        {
            _orderFulfillmentService.CloseSession(orderFulfillmentSessionId);
        }

        private Guid OpenOrderFulfillmentSession()
        {
            var orderFulfillmentSessionId = _orderFulfillmentService.OpenSession(USERNAME, PASSWORD);
            return orderFulfillmentSessionId;
        }
    }
}