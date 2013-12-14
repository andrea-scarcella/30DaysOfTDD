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
        [Test]
        public void WhenUserPlacesACorrectOrderThenAnOrderNumberShouldBeReturned()
        {
            //Arrange
            ShoppingCart sc = new ShoppingCart();
            sc.Items.Add(new ShoppingCartItem { ItemId = Guid.NewGuid(), Quantity = 1 });
            var customerId = Guid.NewGuid();
            var expectedOrderId = Guid.NewGuid();

            Mock.Arrange(() => _ods.Save(Arg.IsAny<Order>())).Returns(expectedOrderId).OccursOnce();

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
        [TestFixtureSetUp]
        public void Setup()
        {
            _ods = Mock.Create<IOrderDataService>();
            _cs = Mock.Create<ICustomerService>();
            _os = new OrderService(_ods, _cs);
        }
    }
}
