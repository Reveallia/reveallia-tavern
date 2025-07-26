using Characters;

namespace Core
{
    public class OrderFactory
    {
        public Order CreateOrder(Customer customer)
        {
            return new Order(customer);
        }
    }
}