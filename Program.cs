using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

// 1. Клас для даних товарів
namespace MarketPlaceProject
{
    /// <summary>
    /// Клас для зберігання початкових даних про товари
    /// </summary>
    public static class GoodsData
    {
        private static int _nextId = 1;

        public static List<Goods> GetAllGoods()
        {
            var goods = new List<Goods>
            {
                new Goods(_nextId++, "iPhone 15 Pro", 45999, 10, "Смартфони"),
                new Goods(_nextId++, "Samsung Galaxy S23", 34999, 15, "Смартфони"),
                new Goods(_nextId++, "Xiaomi 13 Pro", 28999, 8, "Смартфони"),
                new Goods(_nextId++, "PlayStation 5", 20999, 3, "Консолі"),
                new Goods(_nextId++, "Xbox Series X", 19999, 4, "Консолі"),
                new Goods(_nextId++, "LG Холодильник", 48999, 7, "Холодильники"),
                new Goods(_nextId++, "Dyson Пилосос", 25999, 9, "Пилососи"),
                new Goods(_nextId++, "MacBook Pro M3", 74999, 4, "Ноутбуки")
            };
            return goods;
        }

        public static Dictionary<string, List<string>> GetCategories()
        {
            return new Dictionary<string, List<string>>
            {
                ["Електроніка"] = new List<string> { "Смартфони", "Ноутбуки", "Телевізори" },
                ["Геймінг"] = new List<string> { "Консолі", "Ігри", "Аксесуари" },
                ["Побутова техніка"] = new List<string> { "Холодильники", "Пилососи", "Пральні машини" }
            };
        }
    }

    // 2. ІНТЕРФЕЙСИ та ООП
    public interface ISerializableEntity { string Serialize(); }

    // 3. ДЕЛЕГАТИ та ПОДІЇ
    public delegate void StockEventHandler(object sender, string message);
    public delegate void CartEventHandler(object sender, CartEventArgs e);

    public class CartEventArgs : EventArgs
    {
        public string Message { get; }
        public CartEventArgs(string message) => Message = message;
    }

    // 4. АБСТРАКТНИЙ КЛАС
    [Serializable]
    public abstract class ProductBase
    {
        public int Id { get; protected set; }
        public string Name { get; protected set; }
        public decimal Price { get; protected set; }
        public string Category { get; protected set; }

        protected ProductBase(int id, string name, decimal price, string category)
        {
            Id = id; Name = name; Price = price; Category = category;
        }

        public abstract string GetDescription();
    }

    // 5. КЛАС ТОВАРУ
    [Serializable]
    public class Goods : ProductBase, ICloneable, ISerializableEntity
    {
        private int _quantity;
        public int Quantity
        {
            get => _quantity;
            set => _quantity = value >= 0 ? value : throw new ArgumentException("Кількість не може бути від'ємною");
        }

        public Goods(int id, string name, decimal price, int quantity, string category)
            : base(id, name, price, category) => Quantity = quantity;

        public override string ToString() => $"{Name} - {Price} грн ({Quantity} шт.)";
        public override string GetDescription() => $"Товар: {Name}, Категорія: {Category}";
        public object Clone() => new Goods(Id, Name, Price, Quantity, Category);
        public string Serialize() => $"{Id}|{Name}|{Price}|{Quantity}|{Category}";

        public static Goods operator +(Goods g, int qty)
        {
            var result = (Goods)g.Clone();
            result.Quantity += qty;
            return result;
        }

        public static bool operator ==(Goods a, Goods b)
        {
            if (ReferenceEquals(a, b)) return true;
            if (a is null || b is null) return false;
            return a.Id == b.Id;
        }

        public static bool operator !=(Goods a, Goods b) => !(a == b);
        public override bool Equals(object obj) => obj is Goods goods && this == goods;
        public override int GetHashCode() => Id.GetHashCode();
    }

    // 6. GENERICS КОЛЕКЦІЯ
    public class GoodsCollection<T> : IEnumerable<T> where T : Goods
    {
        private List<T> _items = new List<T>();
        public T this[int id] => _items.FirstOrDefault(g => g.Id == id);
        public IEnumerable<T> this[string name] => _items.Where(g => g.Name.Contains(name, StringComparison.OrdinalIgnoreCase));

        public void Add(T item) => _items.Add(item);
        public void AddRange(IEnumerable<T> items) => _items.AddRange(items);
        public IEnumerator<T> GetEnumerator() => _items.GetEnumerator();
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
        public int Count => _items.Count;
        public decimal TotalValue => _items.Sum(g => g.Price * g.Quantity);
        public IEnumerable<T> Where(Func<T, bool> predicate) => _items.Where(predicate);
        public IEnumerable<T> OrderBy<TKey>(Func<T, TKey> keySelector) => _items.OrderBy(keySelector);
    }

    // 7. КОРЗИНА
    public class ShoppingCart
    {
        private List<Goods> _items = new List<Goods>();
        public event CartEventHandler CartChanged;
        public IReadOnlyList<Goods> Items => _items.AsReadOnly();
        public decimal TotalPrice => _items.Sum(i => i.Price * i.Quantity);

        public void AddItem(Goods goods, int quantity)
        {
            if (goods.Quantity < quantity)
                throw new InvalidOperationException($"Недостатньо товару. Доступно: {goods.Quantity}");

            var existing = _items.FirstOrDefault(i => i.Id == goods.Id);
            if (existing != null) existing.Quantity += quantity;
            else _items.Add((Goods)goods.Clone());

            goods.Quantity -= quantity;
            OnCartChanged($"Додано: {goods.Name} x{quantity}");
        }

        public void Clear()
        {
            _items.Clear();
            OnCartChanged("Корзина очищена");
        }

        protected virtual void OnCartChanged(string message) =>
            CartChanged?.Invoke(this, new CartEventArgs(message));
    }

    // 8. SINGLETON покупець
    public sealed class Customer
    {
        private static Customer _instance;
        public static Customer Instance => _instance ??= new Customer();

        public string Name { get; set; }
        public ShoppingCart Cart { get; } = new ShoppingCart();
        public List<Order> Orders { get; } = new List<Order>();
        private Customer() => Name = "Гість";
    }

    // 9. ЗАМОВЛЕННЯ
    [Serializable]
    public class Order
    {
        public int Id { get; }
        public DateTime Date { get; }
        public List<Goods> Items { get; }
        public decimal Total { get; }

        public Order(int id, List<Goods> items)
        {
            Id = id;
            Date = DateTime.Now;
            Items = items.Select(i => (Goods)i.Clone()).ToList();
            Total = items.Sum(i => i.Price * i.Quantity);
        }

        public override string ToString() =>
            $"Замовлення #{Id} від {Date:dd.MM.yyyy} - {Total} грн";
    }

    // 10. SHOP MANAGER
    public sealed class ShopManager
    {
        private static ShopManager _instance;
        public static ShopManager Instance => _instance ??= new ShopManager();
        public GoodsCollection<Goods> Goods { get; } = new GoodsCollection<Goods>();
        public event StockEventHandler LowStockAlert;

        private ShopManager()
        {
            Goods.AddRange(GoodsData.GetAllGoods());
            CheckStock();
        }

        public void CheckStock()
        {
            foreach (var g in Goods.Where(g => g.Quantity <= 3))
                LowStockAlert?.Invoke(this, $"Увага! Закінчується: {g.Name} (залишилось: {g.Quantity})");
        }

        public IEnumerable<Goods> Search(string query) => Goods[query];
        public IEnumerable<Goods> FilterByPrice(decimal min, decimal max) =>
            Goods.Where(g => g.Price >= min && g.Price <= max).OrderBy(g => g.Price);
    }

    // 11. FILE MANAGER
    /*public static class FileManager
    {
        private const string FilePath = "marketplace_data.txt";
        private static bool _isFirstRun = true;

        public static void SaveData(Customer customer)
        {
            var sb = new StringBuilder();

            // Якщо перший запуск після старту програми, очищаємо файл
            if (_isFirstRun)
            {
                File.WriteAllText(FilePath, string.Empty, Encoding.UTF8);
                _isFirstRun = false;
            }

            // --- Товари (ті, що були замовлені) ---
            sb.AppendLine("#OrderedGoods");
            var orderedGoods = customer.Orders
                .SelectMany(o => o.Items)
                .GroupBy(g => g.Name)
                .Select(g => new { Name = g.Key, Quantity = g.Sum(x => x.Quantity) });

            foreach (var g in orderedGoods)
            {
                sb.AppendLine($"{g.Name}|{g.Quantity}");
            }

            // --- Замовлення ---
            sb.AppendLine("#Orders");
            foreach (var o in customer.Orders)
            {
                foreach (var item in o.Items)
                {
                    sb.AppendLine($"{o.Id}|{o.Date:yyyy-MM-dd HH:mm}|{customer.Name}|{item.Name}|{item.Quantity}|{item.Price * item.Quantity}");
                }
            }

            // --- Роздільник ---
            sb.AppendLine("------------------------------------------------------");

            // Додаємо блок у файл, не перезаписуючи попередні
            File.AppendAllText(FilePath, sb.ToString(), Encoding.UTF8);
        }

        public static void LoadData(Customer customer)
        {
            if (!File.Exists(FilePath)) return;

            var lines = File.ReadAllLines(FilePath, Encoding.UTF8);

            bool readingOrders = false;

            foreach (var line in lines)
            {
                if (line.StartsWith("#Orders")) { readingOrders = true; continue; }
                if (line.StartsWith("#OrderedGoods") || line.StartsWith("------------------------------------------------------"))
                {
                    readingOrders = false;
                    continue;
                }

                if (readingOrders)
                {
                    var parts = line.Split('|');
                    if (parts.Length == 6 &&
                        int.TryParse(parts[0], out int orderId) &&
                        DateTime.TryParse(parts[1], out DateTime date) &&
                        int.TryParse(parts[4], out int qty) &&
                        decimal.TryParse(parts[5], out decimal total))
                    {
                        var item = new Goods(0, parts[3], total / qty, qty, "");
                        var order = customer.Orders.FirstOrDefault(o => o.Id == orderId);
                        if (order == null)
                            customer.Orders.Add(new Order(orderId, new List<Goods> { item }));
                        else
                            order.Items.Add(item);
                    }
                }
            }
        }
    }*/

    public static class FileManager
    {
        private const string FilePath = "marketplace_data.txt";
        private static bool _isFirstRun = true;

        // Зберігає тільки останнє замовлення у файл
        public static void SaveLastOrder(Customer customer)
        {
            if (!customer.Orders.Any()) return;

            var lastOrder = customer.Orders.Last(); // беремо тільки останнє замовлення
            var sb = new StringBuilder();

            // Очищення файлу при першому запуску програми
            if (_isFirstRun)
            {
                File.WriteAllText(FilePath, string.Empty, Encoding.UTF8);
                _isFirstRun = false;
            }

            // --- OrderedGoods ---
            sb.AppendLine("#OrderedGoods");
            foreach (var g in lastOrder.Items.GroupBy(i => i.Name)
                                             .Select(g => new { Name = g.Key, Quantity = g.Sum(x => x.Quantity) }))
            {
                sb.AppendLine($"{g.Name}|{g.Quantity}");
            }

            // --- Orders ---
            sb.AppendLine("#Orders");
            foreach (var item in lastOrder.Items)
            {
                sb.AppendLine($"{lastOrder.Id}|{lastOrder.Date:yyyy-MM-dd HH:mm}|{customer.Name}|{item.Name}|{item.Quantity}|{item.Price * item.Quantity}");
            }

            // Роздільник після замовлення
            sb.AppendLine("------------------------------------------------------");

            // Додаємо блок у кінець файлу
            File.AppendAllText(FilePath, sb.ToString(), Encoding.UTF8);
        }
    }


    // 12. UI УТИЛІТИ
    public static class UI
    {
        public static void ShowHeader(string title)
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"\n=== {title} ===");
            Console.ResetColor();
        }

        public static int GetChoice(int min, int max)
        {
            while (true)
            {
                Console.Write("\nВаш вибір: ");
                if (int.TryParse(Console.ReadLine(), out int choice) && choice >= min && choice <= max)
                    return choice;
                Console.WriteLine($"Потрібно число від {min} до {max}");
            }
        }

        public static void ShowProduct(Goods g)
        {
            Console.ForegroundColor = g.Quantity <= 3 ? ConsoleColor.Yellow : ConsoleColor.Green;
            Console.WriteLine($"[{g.Id}] {g.Name,-25} | {g.Category,-15} | {g.Price,8} грн | {g.Quantity,3} шт.");
            Console.ResetColor();
        }
    }

    // 13. ПРОГРАМА
    class Program
    {
        static void Main()
        {
            Console.Title = "🏪 MarketPlace";
            Console.OutputEncoding = Encoding.UTF8;

            var shop = ShopManager.Instance;
            var customer = Customer.Instance;

            shop.LowStockAlert += (sender, msg) =>
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"\n[!] {msg}");
                Console.ResetColor();
            };

            customer.Cart.CartChanged += (sender, e) =>
            {
                Console.ForegroundColor = ConsoleColor.Magenta;
                Console.WriteLine($"\n[Корзина] {e.Message}");
                Console.ResetColor();
            };

            MainMenu(shop, customer);
        }

        static void MainMenu(ShopManager shop, Customer customer)
        {
            while (true)
            {
                UI.ShowHeader("Головне меню");
                Console.WriteLine($"Користувач: {customer.Name}");
                Console.WriteLine($"Корзина: {customer.Cart.Items.Count} товарів на {customer.Cart.TotalPrice} грн\n");

                Console.WriteLine("1. 🛍️  Каталог товарів");
                Console.WriteLine("2. 🔍  Пошук");
                Console.WriteLine("3. 🛒  Корзина");
                Console.WriteLine("4. 📦  Замовлення");
                Console.WriteLine("5. ⚙️  Налаштування");
                Console.WriteLine("6. 💾  Файли");
                Console.WriteLine("7. 🚪  Вийти");

                switch (UI.GetChoice(1, 7))
                {
                    case 1: ShowCatalog(shop, customer); break;
                    case 2: SearchProducts(shop, customer); break;
                    case 3: ShowCart(customer); break;
                    case 4: ShowOrders(customer); break;
                    case 5: Settings(customer); break;
                    case 6: FileOperations(customer); break;
                    case 7: return;
                }
            }
        }
        //---------------------------------------
        static void ShowCatalog(ShopManager shop, Customer customer)
        {
            while (true)
            {
                UI.ShowHeader("Каталог товарів");

                var groups = shop.Goods.GroupBy(g => g.Category);
                foreach (var group in groups)
                {
                    Console.WriteLine($"\n📁 {group.Key}:");
                    foreach (var g in group) UI.ShowProduct(g);
                }

                Console.WriteLine("\n0. Назад");
                Console.WriteLine("ID товару - додати в корзину");

                int id = UI.GetChoice(0, shop.Goods.Max(g => g.Id));
                if (id == 0) break;

                var product = shop.Goods[id];
                if (product != null) AddToCart(product, customer);
            }
        }
        //---------------------------------------
        static void AddToCart(Goods product, Customer customer)
        {
            Console.Write($"Кількість (до {product.Quantity}): ");
            if (int.TryParse(Console.ReadLine(), out int qty) && qty > 0)
            {
                try
                {
                    customer.Cart.AddItem(product, qty);
                    Console.WriteLine("✅ Додано до корзини");
                }
                catch (Exception ex) { Console.WriteLine($"❌ {ex.Message}"); }
            }
        }
        //---------------------------------------
        static void SearchProducts(ShopManager shop, Customer customer)
        {
            UI.ShowHeader("Пошук товарів");
            Console.Write("Пошук: ");
            string query = Console.ReadLine();

            var results = shop.Search(query).ToList();
            if (results.Any())
            {
                foreach (var g in results) UI.ShowProduct(g);
                Console.Write("\nДодати товар до корзини (ID або 0 для виходу): ");
                if (int.TryParse(Console.ReadLine(), out int id) && id > 0)
                {
                    var product = results.FirstOrDefault(g => g.Id == id);
                    if (product != null) AddToCart(product, customer);
                }
            }
            else Console.WriteLine("Нічого не знайдено");
        }
        //---------------------------------------
        static void ShowCart(Customer customer)
        {
            while (true)
            {
                UI.ShowHeader("Ваша корзина");

                if (!customer.Cart.Items.Any())
                    Console.WriteLine("Корзина порожня");
                else
                {
                    foreach (var item in customer.Cart.Items)
                        Console.WriteLine($"{item.Name} x{item.Quantity} = {item.Price * item.Quantity} грн");

                    Console.WriteLine($"\n💵 Загальна сума: {customer.Cart.TotalPrice} грн");
                }

                Console.WriteLine("\n1. Оформити замовлення");
                Console.WriteLine("2. Очистити корзину");
                Console.WriteLine("0. Назад");

                switch (UI.GetChoice(0, 2))
                {
                    case 1: Checkout(customer); return;
                    case 2: customer.Cart.Clear(); Console.WriteLine("✅ Корзина очищена"); break;
                    case 0: return;
                }
            }
        }
        //---------------------------------------
        /*static void Checkout(Customer customer)
        {
            if (!customer.Cart.Items.Any())
            {
                Console.WriteLine("Корзина порожня!");
                return;
            }

            Console.Write("Ваше ім'я: ");
            customer.Name = Console.ReadLine();

            var order = new Order(customer.Orders.Count + 1, customer.Cart.Items.ToList());
            customer.Orders.Add(order);
            customer.Cart.Clear();

            Console.WriteLine($"\n✅ Замовлення #{order.Id} оформлено!");
            Console.WriteLine($"💰 Сума: {order.Total} грн");
            Console.ReadKey();
        }*/

        static void Checkout(Customer customer)
        {
            if (!customer.Cart.Items.Any())
            {
                Console.WriteLine("Корзина порожня!");
                return;
            }

            Console.Write("Ваше ім'я: ");
            customer.Name = Console.ReadLine();

            // Створюємо нове замовлення
            var order = new Order(customer.Orders.Count + 1, customer.Cart.Items.ToList());
            customer.Orders.Add(order);
            customer.Cart.Clear();

            Console.WriteLine($"\n✅ Замовлення #{order.Id} оформлено!");
            Console.WriteLine($"💰 Сума: {order.Total} грн");

            // --- Пропозиція зберегти замовлення у файл ---
            Console.Write("Бажаєте зберегти замовлення у файл? (T - так/F - ні): ");
            var key = Console.ReadKey();
            Console.WriteLine();
            if (key.Key == ConsoleKey.T)
            {
                FileManager.SaveLastOrder(customer);
                Console.WriteLine("✅ Замовлення збережене у файл marketplace_data.txt");
            }
            else
            {
                Console.WriteLine("⚠️ Замовлення не збережене у файл");
            }

            Console.ReadKey();
        }

        //---------------------------------------
        static void ShowOrders(Customer customer)
        {
            UI.ShowHeader("Мої замовлення");

            if (!customer.Orders.Any())
                Console.WriteLine("У вас ще немає замовлень");
            else
                foreach (var order in customer.Orders)
                    Console.WriteLine(order);

            Console.ReadKey();
        }
        //---------------------------------------
        static void Settings(Customer customer)
        {
            UI.ShowHeader("Налаштування");
            Console.Write("Нове ім'я: ");
            customer.Name = Console.ReadLine();
            Console.WriteLine($"✅ Ім'я змінено на: {customer.Name}");
            Console.ReadKey();
        }
        //---------------------------------------
        /*static void FileOperations(Customer customer)
        {
            UI.ShowHeader("Робота з файлами");

            // Зберігаємо замовлені товари та замовлення
            FileManager.SaveData(customer);
            Console.WriteLine("✅ Дані збережено у файл marketplace_data.txt");

            // Завантаження даних з файлу
            FileManager.LoadData(customer);
            Console.WriteLine($"📂 Завантажено {customer.Orders.Count} збережених замовлень");

            Console.ReadKey();
        }*/

        static void FileOperations(Customer customer)
        {
            UI.ShowHeader("Робота з файлами");

            // Зберігаємо тільки останнє замовлення
            FileManager.SaveLastOrder(customer);

            // Просте повідомлення користувачу
            Console.WriteLine("✅ Замовлення збережене у файл marketplace_data.txt");

            Console.ReadKey();
        }


    }
}
