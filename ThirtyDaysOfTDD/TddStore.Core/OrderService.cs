using System;
using System.Linq;
using TddStore.Core.Exceptions;
namespace TddStore.Core
{
    public class OrderService
    {
        private IOrderDataService _orderDataService;
        private ICustomerService _customerService;
        public OrderService(IOrderDataService orderDataService, ICustomerService customerService)
        {
            _orderDataService = orderDataService;
            _customerService = customerService;
        }

        public object PlaceOrder(Guid customerId, ShoppingCart shoppingCart)
        {

            if (shoppingCart.Items.Where(el => el.Quantity == 0).Count() > 0)
            {
                throw new InvalidOrderException();
            }
            var customer = _customerService.GetCustomer(customerId);
            var order = new Order();
            return _orderDataService.Save(order);
        }
    }
}