using System;
using System.Linq;
using TddStore.Core.Exceptions;
namespace TddStore.Core
{
    public class OrderService
    {
        private IOrderDataService _orderDataService;

        public OrderService(IOrderDataService orderDataService)
        {
            _orderDataService = orderDataService;
        }

        public object PlaceOrder(Guid customerId, ShoppingCart shoppingCart)
        {
            var order = new Order();
            if (shoppingCart.Items.Where(el => el.Quantity == 0).Count() > 0)
            {
                throw new InvalidOrderException();
            }
            return _orderDataService.Save(order);
        }
    }
}