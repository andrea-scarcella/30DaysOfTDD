using System;

namespace TddStore.Core
{
    public class Customer
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public Address ShippingAddress { get; private set; }

        public Customer()
        {
            ShippingAddress = new Address();
        }
    }

    public class Address
    {
        public string StreetAddressOne { get; set; }
        public string StreetAddressTwo { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Zip { get; set; }
    }
}