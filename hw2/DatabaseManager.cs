using Microsoft.Data.Sqlite;

class DatabaseManager
{
    private string _connectionString;

    public DatabaseManager(string dbPath)
    {
        _connectionString = $"Data Source={dbPath}";
    }


    public void InitializeDatabase(string shopsCsvPath, string ordersCsvPath)
    {
        CreateTables();

        if (GetAllShops().Count == 0 && File.Exists(shopsCsvPath))
        {
            ImportShopsFromCsv(shopsCsvPath);
            Console.WriteLine($"[OK] Загружены магазины из {shopsCsvPath}");
        }
        if (GetAllOrders().Count == 0 && File.Exists(ordersCsvPath))
        {
            ImportOrdersFromCsv(ordersCsvPath);
            Console.WriteLine($"[OK] Загружены заказы из {ordersCsvPath}");
        }
    }


    private void CreateTables()
    {
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();
        var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            CREATE TABLE IF NOT EXISTS shop (
                shop_id INTEGER PRIMARY KEY AUTOINCREMENT,
                shop_name TEXT NOT NULL
            );
            CREATE TABLE IF NOT EXISTS ""order"" (
                order_id INTEGER PRIMARY KEY AUTOINCREMENT,
                shop_id INTEGER NOT NULL,
                order_name TEXT NOT NULL,
                amount REAL NOT NULL,
                FOREIGN KEY (shop_id) REFERENCES shop(shop_id)
            );";
        cmd.ExecuteNonQuery();
    }


    private void ImportShopsFromCsv(string path)
    {
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();
        string[] lines = File.ReadAllLines(path);
        for (int i = 1; i < lines.Length; i++)
        {
            string[] parts = lines[i].Split(';');
            if (parts.Length < 2) continue;
            var cmd = conn.CreateCommand();
            cmd.CommandText = "INSERT INTO shop (shop_id, shop_name) VALUES (@id, @name)";
            cmd.Parameters.AddWithValue("@id", int.Parse(parts[0]));
            cmd.Parameters.AddWithValue("@name", parts[1]);
            cmd.ExecuteNonQuery();
        }
    }

    private void ImportOrdersFromCsv(string path)
    {
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();
        string[] lines = File.ReadAllLines(path);
        for (int i = 1; i < lines.Length; i++)
        {
            string[] parts = lines[i].Split(';');
            if (parts.Length < 4) continue;
            var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                INSERT INTO ""order"" (order_id, shop_id, order_name, amount)
                VALUES (@id, @shopId, @name, @amount)";
            cmd.Parameters.AddWithValue("@id", int.Parse(parts[0]));
            cmd.Parameters.AddWithValue("@shopId", int.Parse(parts[1]));
            cmd.Parameters.AddWithValue("@name", parts[2]);
            cmd.Parameters.AddWithValue("@amount", decimal.Parse(parts[3],
                System.Globalization.CultureInfo.InvariantCulture));
            cmd.ExecuteNonQuery();
        }
    }


    public List<Shop> GetAllShops()
    {
        var result = new List<Shop>();
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();
        var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT shop_id, shop_name FROM shop ORDER BY shop_id";
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            result.Add(new Shop(reader.GetInt32(0), reader.GetString(1)));
        }
        return result;
    }

    public List<Order> GetAllOrders()
    {
        var result = new List<Order>();
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();
        var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            SELECT order_id, shop_id, order_name, amount 
            FROM ""order"" 
            ORDER BY order_id";
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            result.Add(new Order(
                reader.GetInt32(0), reader.GetInt32(1),
                reader.GetString(2), reader.GetDecimal(3)));
        }
        return result;
    }

    public Order? GetOrderById(int id)
    {
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();
        var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            SELECT order_id, shop_id, order_name, amount 
            FROM ""order"" 
            WHERE order_id = @id";
        cmd.Parameters.AddWithValue("@id", id);
        using var reader = cmd.ExecuteReader();
        if (reader.Read())
        {
            return new Order(
                reader.GetInt32(0), reader.GetInt32(1),
                reader.GetString(2), reader.GetDecimal(3));
        }
        return null;
    }

    public void AddOrder(Order order)
    {
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();
        var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            INSERT INTO ""order"" (shop_id, order_name, amount)
            VALUES (@shopId, @name, @amount)";
        cmd.Parameters.AddWithValue("@shopId", order.ShopId);
        cmd.Parameters.AddWithValue("@name", order.Name);
        cmd.Parameters.AddWithValue("@amount", order.Amount);
        cmd.ExecuteNonQuery();
    }

    public void UpdateOrder(Order order)
    {
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();
        var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            UPDATE ""order"" 
            SET shop_id = @shopId, order_name = @name, amount = @amount
            WHERE order_id = @id";
        cmd.Parameters.AddWithValue("@id", order.Id);
        cmd.Parameters.AddWithValue("@shopId", order.ShopId);
        cmd.Parameters.AddWithValue("@name", order.Name);
        cmd.Parameters.AddWithValue("@amount", order.Amount);
        cmd.ExecuteNonQuery();
    }

    public void DeleteOrder(int id)
    {
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();
        var cmd = conn.CreateCommand();
        cmd.CommandText = @"DELETE FROM ""order"" WHERE order_id = @id";
        cmd.Parameters.AddWithValue("@id", id);
        cmd.ExecuteNonQuery();
    }

    public (string[] columns, List<string[]> rows) ExecuteQuery(string sql)
    {
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();
        var cmd = conn.CreateCommand();
        cmd.CommandText = sql;
        using var reader = cmd.ExecuteReader();

        string[] columns = new string[reader.FieldCount];
        for (int i = 0; i < reader.FieldCount; i++)
            columns[i] = reader.GetName(i);

        var rows = new List<string[]>();
        while (reader.Read())
        {
            string[] row = new string[reader.FieldCount];
            for (int i = 0; i < reader.FieldCount; i++)
                row[i] = reader.GetValue(i)?.ToString() ?? "";
            rows.Add(row);
        }
        return (columns, rows);
    }
}