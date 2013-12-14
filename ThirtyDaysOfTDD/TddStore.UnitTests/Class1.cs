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
        [ExpectedException(typeof(InvalidOrderException))]
        public void WhenAUserAttemptsToOrderAnItemWithAQuantityOfZeroThrowInvalidOrderException()
        {

            var sc = new ShoppingCart();
            sc.Items.Add(new ShoppingCartItem { Quantity = 0, ItemId = Guid.NewGuid() });
        
            var expectedOrderId = Guid.NewGuid();
            Mock.Arrange(() => _ods.Save(Arg.IsAny<Order>())).Returns(expectedOrderId).OccursNever();

            var customerId = Guid.NewGuid();
            //Act
            var result = _os.PlaceOrder(customerId, sc);
            //Assert
            Mock.Assert(_ods);

        }
        [TestFixtureSetUp]
        public void Setup()
        {
            _ods = Mock.Create<IOrderDataService>();
            _os = new OrderService(_ods);

        }
    }
}
