// After — Clean, safe, maintainable
public async Task ProcessOrderAsync(int orderId, CancellationToken ct = default)
{
    var order = await _orderRepository.GetByIdAsync(orderId, ct)
        ?? throw new ArgumentException($"Order {orderId} not found.");

    var result = order.Status switch
    {
        OrderStatus.Discounted => ApplyDiscount(order.Total, discountRate: 0.10m),
        OrderStatus.Shipped    => order.Total,
        _ => throw new InvalidOperationException(
                 $"Unhandled order status: {order.Status}")
    };

    _logger.LogInformation("Order {OrderId} processed. Final total: {Total}",
        orderId, result);
}

private static decimal ApplyDiscount(decimal total, decimal discountRate)
    => total * (1 - discountRate);
