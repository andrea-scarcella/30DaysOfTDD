using System;

namespace TddStore.Core
{
    public interface ICustomerService
    {
        Customer GetCustomer(Guid customerId);
    }
}