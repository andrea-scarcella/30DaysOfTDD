using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TddStore.Core;
using Telerik.JustMock;

namespace TddStore.UnitTests
{
    [TestFixture]
    public class OrderServiceTests
    {
        [Test]
        public void WhenUserPlacesACorrectOrderThenAnOrderNumberShouldBeReturned()
        {
            //Arrange
            ShoppingCart sc = new ShoppingCart();
            sc.Items.Add(new ShoppingCartItem { ItemId = Guid.NewGuid(), Quantity = 1 });
            var customerId = Guid.NewGuid();
            var expectedOrderId = Guid.NewGuid();
            var ods = Mock.Create<IOrderDataService>();
            Mock.Arrange(() => ods.Save(Arg.IsAny<Order>())).Returns(expectedOrderId);
            OrderService os = new OrderService(ods);
            //Act
            var result = os.PlaceOrder(customerId, sc);
            //Assert
            Assert.AreEqual(expectedOrderId, result);

        }
    }
}
