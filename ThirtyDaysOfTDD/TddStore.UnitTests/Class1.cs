using System;
//using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using TddStore.Core;
using TddStore.Core.Exceptions;
using Telerik.JustMock;

namespace TddStore.UnitTests
{
    [TestFixture]
    class OrderServiceTests
    {
        private OrderService _orderService;
        private IOrderDataService _orderDataService;
        private ICustomerService _customerService;
        private IOrderFulfillmentService _orderFulfillmentService;

        [TestFixtureSetUp]
        public void SetupTestFixture()
        {
            _orderDataService = Mock.Create<IOrderDataService>();
            _customerService = Mock.Create<ICustomerService>();
            _orderFulfillmentService = Mock.Create<IOrderFulfillmentService>();
            _orderService = new OrderService(_orderDataService, _customerService, _orderFulfillmentService);
        }

        [Test]
        public void WhenUserPlacesOrderWithItemThatIsInInventoryOrderFulfillmentWorkflowShouldComplete()
        {
            //Arrange
            var shoppingCart = new ShoppingCart();
            var itemId = Guid.NewGuid();
            shoppingCart.Items.Add(new ShoppingCartItem { ItemId = itemId, Quantity = 1 });
            var customerId = Guid.NewGuid();
            var customer = new Customer { Id = customerId };
            var orderFulfillmentSessionId = Guid.NewGuid();

            Mock.Arrange(() => _customerService.GetCustomer(customerId)).Returns(customer).OccursOnce();

            Mock.Arrange(() => _orderFulfillmentService.OpenSession(Arg.IsAny<string>(), Arg.IsAny<string>()))
                .Returns(orderFulfillmentSessionId)
                .InOrder();
            Mock.Arrange(() => _orderFulfillmentService.IsInInventory(orderFulfillmentSessionId, itemId, 1))
                .Returns(true)
                .InOrder();
            Mock.Arrange(() =>
                _orderFulfillmentService.
                    PlaceOrder(orderFulfillmentSessionId, Arg.IsAny<IDictionary<Guid, int>>(), Arg.IsAny<string>()))
                .Returns(true)
                .InOrder();
            Mock.Arrange(() => _orderFulfillmentService.CloseSession(orderFulfillmentSessionId))
                .InOrder();

            //Act
            _orderService.PlaceOrder(customerId, shoppingCart);

            //Assert
            Mock.Assert(_orderFulfillmentService);
        }

        [Test]
        public void WhenUserPlacesACorrectOrderThenAnOrderNumberShouldBeReturned()
        {
            //Arrange
            var shoppingCart = new ShoppingCart();
            var itemId = Guid.NewGuid();
            shoppingCart.Items.Add(new ShoppingCartItem { ItemId = itemId, Quantity = 1 });
            var customerId = Guid.NewGuid();
            var expectedOrderId = Guid.NewGuid();
            var orderFulfillmentSessionId = Guid.NewGuid();
            
            var customer = new Customer { Id = customerId };

            Mock.Arrange(() => _orderDataService.Save(Arg.IsAny<Order>()))
                .Returns(expectedOrderId)
                .OccursOnce();
            Mock.Arrange(() => _customerService.GetCustomer(customerId)).Returns(customer).OccursOnce();
            Mock.Arrange(() => _orderFulfillmentService.OpenSession(Arg.IsAny<string>(), Arg.IsAny<string>()))
                .Returns(orderFulfillmentSessionId);
            Mock.Arrange(() => _orderFulfillmentService.IsInInventory(orderFulfillmentSessionId, itemId, 1))
                .Returns(true)
                .OccursOnce();
            Mock.Arrange(() =>
                _orderFulfillmentService.
                    PlaceOrder(orderFulfillmentSessionId, Arg.IsAny<IDictionary<Guid, int>>(), Arg.IsAny<string>()))
                .Returns(true);
            Mock.Arrange(() => _orderFulfillmentService.CloseSession(orderFulfillmentSessionId));

            //Act
            var result = _orderService.PlaceOrder(customerId, shoppingCart);

            //Assert
            Assert.AreEqual(expectedOrderId, result);
            Mock.Assert(_orderDataService);
            Mock.Assert(_orderFulfillmentService);

        }

        [Test]
        public void WhenAValidCustomerPlacesAValidOrderAnTheOrderServiceSholdBeAbleToGetACustomerFromTheCustomerService()
        {
            //Arrange
            var shoppingCart = new ShoppingCart();
            shoppingCart.Items.Add(new ShoppingCartItem { ItemId = Guid.NewGuid(), Quantity = 1 });
            var customerId = Guid.NewGuid();
            var customerToReturn = new Customer { Id = customerId, FirstName = "Fred", LastName = "Flinstone" };

            Mock.Arrange(() => _customerService.GetCustomer(customerId))
                .Returns(customerToReturn)
                .OccursOnce();

            //Act
            _orderService.PlaceOrder(customerId, shoppingCart);

            //Assert
            Mock.Assert(_customerService);
        }

        [Test]
        public void WhenAUserAttemptsToOrderAnItemWithAQuantityOfZeroThrowInvalidOrderException()
        {
            //Arrange
            var shoppingCart = new ShoppingCart();
            shoppingCart.Items.Add(new ShoppingCartItem { ItemId = Guid.NewGuid(), Quantity = 0 });
            var customerId = Guid.NewGuid();
            var expectedOrderId = Guid.NewGuid();

            Mock.Arrange(() => _orderDataService.Save(Arg.IsAny<Order>()))
                .Returns(expectedOrderId)
                .OccursNever();

            //Act
            try
            {
                _orderService.PlaceOrder(customerId, shoppingCart);
            }
            catch (InvalidOrderException ex)
            {
                //Assert
                Mock.Assert(_orderDataService);
                Assert.Pass();
            }

            //Assert
            Assert.Fail();
        }

        [Test]
        public void WhenUserPlacesACorrectOrderWithMoreThenOneItemThenAnOrderNumberShouldBeReturned()
        {
            //Arrange
            var customerId = Guid.NewGuid();
            var shoppingCart = new ShoppingCart();
            var itemOneId = Guid.NewGuid();
            var itemTwoId = Guid.NewGuid();
            int itemOneQuantity = 1;
            int itemTwoQuantity = 4;

            shoppingCart.Items.Add(new ShoppingCartItem { ItemId = itemOneId, Quantity = itemOneQuantity });
            shoppingCart.Items.Add(new ShoppingCartItem { ItemId = itemTwoId, Quantity = itemTwoQuantity });
            var customerToReturn = new Customer { Id = customerId };

            Mock.Arrange(() => _customerService.GetCustomer(customerId))
                .Returns(customerToReturn)
                .OccursOnce();
            var orderFulfillmentSessionId = Guid.NewGuid();
            Mock.Arrange(() => _orderFulfillmentService.OpenSession(Arg.IsAny<string>(), Arg.IsAny<string>()))
                .Returns(orderFulfillmentSessionId)
                .OccursOnce();
            Mock.Arrange(() => _orderFulfillmentService.IsInInventory(orderFulfillmentSessionId, itemOneId, itemOneQuantity))
                .Returns(true)
                .OccursOnce();
            Mock.Arrange(() => _orderFulfillmentService.IsInInventory(orderFulfillmentSessionId, itemTwoId, itemTwoQuantity))
             .Returns(true)
             .OccursOnce();
            Mock.Arrange(() => _orderFulfillmentService.PlaceOrder(orderFulfillmentSessionId, Arg.IsAny<IDictionary<Guid, int>>(), Arg.IsAny<string>()))
                .OccursOnce();
            var expectedOrderId = Guid.NewGuid();
            Mock.Arrange(() => _orderDataService.Save(Arg.IsAny<Order>()))
                .Returns(expectedOrderId)
                .OccursOnce();
            //Act
            var result = _orderService.PlaceOrder(customerId, shoppingCart);
            //Assert
            Assert.AreEqual(expectedOrderId, result);
            Mock.Assert(_orderDataService);
            Mock.Assert(_orderFulfillmentService);
        }
    }
}