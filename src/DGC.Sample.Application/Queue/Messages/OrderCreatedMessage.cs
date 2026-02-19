namespace DGC.Sample.Application.Queue.Messages;

public sealed class OrderCreatedMessage
{
    public OrderCreatedMessage()
    {
    }

    public OrderCreatedMessage(Guid orderId, string customerName, decimal totalAmount, DateTimeOffset createdAtUtc)
    {
        OrderId = orderId;
        CustomerName = customerName;
        TotalAmount = totalAmount;
        CreatedAtUtc = createdAtUtc;
    }

    public Guid OrderId { get; set; }

    public string CustomerName { get; set; } = string.Empty;

    public decimal TotalAmount { get; set; }

    public DateTimeOffset CreatedAtUtc { get; set; }
}