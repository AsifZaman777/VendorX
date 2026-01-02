namespace VendorX.Models.Enums
{
    public enum UserRole
    {
        SuperAdmin,
        ShopKeeper,
        Customer
    }

    public enum OrderStatus
    {
        Pending,
        Delivered,
        Cancelled
    }

    public enum BakiStatus
    {
        Due,
        Settled
    }

    public enum TransactionType
    {
        Purchase,
        Payment,
        Refund
    }
}
