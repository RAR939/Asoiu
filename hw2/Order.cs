class Order
{

    public int Id { get; set; }

    public int ShopId { get; set; }

    public string Name { get; set; }

    private decimal _amount;

    public decimal Amount
    {
        get => _amount;
        set
        {
            if (value < 0)
                throw new ArgumentException("Сумма заказа не может быть отрицательной");
            _amount = value;
        }
    }

    public Order(int id, int shopId, string name, decimal amount)
    {
        Id = id;
        ShopId = shopId;
        Name = name;
        Amount = amount;   
    }

    public Order() : this(0, 0, "", 0) { }

    public override string ToString() =>
        $"[{Id}] {Name}, магазин #{ShopId}, сумма: {Amount} руб.";
}