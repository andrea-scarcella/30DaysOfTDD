using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TddStore.Core;
using TddStore.Core.Exceptions;
using Telerik.JustMock;

namespace TddStore.UnitTests
{
    [TestFixture]
    public class OrderServiceTests
    {
        private OrderService _os;
        private IOrderDataService _ods;
        private ICustomerService _cs;
        private IOrderFulfillmentService _ofs;
        [Test]
        public void WhenUserPlacesACorrectOrderThenAnOrderNumberShouldBeReturned()
        {
            //Arrange
            ShoppingCart sc = new ShoppingCart();
            sc.Items.Add(new ShoppingCartItem { ItemId = Guid.NewGuid(), Quantity = 1 });
            var customerId = Guid.NewGuid();
            var expectedOrderId = Guid.NewGuid();
            var orderFulfillmentSessionId = Guid.NewGuid();
            Mock.Arrange(() => _ofs.OpenSession(Arg.IsAny<string>(), Arg.IsAny<string>())).Returns(orderFulfillmentSessionId);
            var itemId = Guid.NewGuid();
            Mock.Arrange(() => _ofs.IsInInventory(orderFulfillmentSessionId, itemId, 1)).Returns(true);

            Mock.Arrange(() => _ofs.PlaceOrder(orderFulfillmentSessionId, Arg.IsAny<IDictionary<Guid, int>>(), Arg.IsAny<string>())).Returns(true);
            Mock.Arrange(() => _ofs.CloseSession(orderFulfillmentSessionId));
            Mock.Arrange(() => _ods.Save(Arg.IsAny<Order>())).Returns(expectedOrderId).OccursOnce();
            var customerToReturn = new Customer() { Id = customerId, FirstName = "f", LastName = "l" };
            Mock.Arrange(() => _cs.GetCustomer(customerId)).Returns(customerToReturn).OccursOnce();
            //Act
            var result = _os.PlaceOrder(customerId, sc);
            //Assert
            Assert.AreEqual(expectedOrderId, result);
            Mock.Assert(_ods);


        }
        [Test]
        //#AS:2013/12/14: Mock.Assert is never run (exception is handled by nunit)
        //[ExpectedException(typeof(InvalidOrderException))]
        public void WhenAUserAttemptsToOrderAnItemWithAQuantityOfZeroThrowInvalidOrderException()
        {
            try
            {

                var sc = new ShoppingCart();
                sc.Items.Add(new ShoppingCartItem { Quantity = 0, ItemId = Guid.NewGuid() });

                var expectedOrderId = Guid.NewGuid();
                Mock.Arrange(() => _ods.Save(Arg.IsAny<Order>())).Returns(expectedOrderId).OccursNever();

                var customerId = Guid.NewGuid();
                //Act
                var result = _os.PlaceOrder(customerId, sc);
            }
            //Assert
            catch (InvalidOrderException)
            {
                Mock.Assert(_ods);
                Assert.Pass();
            }
            Assert.Fail();


        }
        [Test]
        public void WhenAValidCustomerPlacesAValidOrderAnTheOrderServiceShouldBeAbleToGetACustomerFromTheCustomerService()
        {
            //Arrange
            var shoppingCart = new ShoppingCart();
            shoppingCart.Items.Add(new ShoppingCartItem { ItemId = Guid.NewGuid(), Quantity = 1 });
            var customerId = Guid.NewGuid();
            var expectedOrderId = Guid.NewGuid();
            var customerToReturn = new Customer() { Id = customerId, FirstName = "f", LastName = "l" };
            Mock.Arrange(() => _cs.GetCustomer(Arg.IsAny<Guid>())).Returns(customerToReturn).OccursOnce();
            //Act
            _os.PlaceOrder(customerId, shoppingCart);
            Mock.Assert(_cs);
        }
        [Test]
        public void WhenUserPlacesOrderWithItemThatIsInInventoryOrderFulfillmentWorkflowShouldComplete()
        {
            //Arrange
            var shoppingCart = new ShoppingCart();
            var itemId = Guid.NewGuid();

            shoppingCart.Items.Add(new ShoppingCartItem { ItemId = itemId, Quantity = 1 });
            var customerId = Guid.NewGuid();
            var expectedOrderId = Guid.NewGuid();
            var customerToReturn = new Customer() { Id = customerId, FirstName = "f", LastName = "l" };
            //Mock.Arrange(() => _ods.Save(Arg.IsAny<Order>())).Returns(expectedOrderId).OccursOnce();
            Mock.Arrange(() => _cs.GetCustomer(Arg.IsAny<Guid>())).Returns(customerToReturn).OccursOnce();
            var orderFulfillmentSessionId = Guid.NewGuid();
            Mock.Arrange(() => _ofs.OpenSession(Arg.IsAny<string>(), Arg.IsAny<string>())).Returns(orderFulfillmentSessionId).InOrder();
            Mock.Arrange(() => _ofs.IsInInventory(orderFulfillmentSessionId, itemId, 1)).Returns(true).InOrder();
            Mock.Arrange(() => _ofs.PlaceOrder(orderFulfillmentSessionId, Arg.IsAny<IDictionary<Guid, int>>(), Arg.IsAny<string>())).Returns(true).InOrder();
            Mock.Arrange(() => _ofs.CloseSession(orderFulfillmentSessionId)).InOrder();
            //Act
            var result = _os.PlaceOrder(customerId, shoppingCart);
            //Assert
            Mock.Assert(_ofs);
        }
        [TestFixtureSetUp]
        public void Setup()
        {
            _ods = Mock.Create<IOrderDataService>();
            _cs = Mock.Create<ICustomerService>();
            _ofs = Mock.Create<IOrderFulfillmentService>();
            _os = new OrderService(_ods, _cs, _ofs);

        }
    }
}
