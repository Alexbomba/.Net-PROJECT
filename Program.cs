using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;

// 1. Клас для даних товарів
namespace MarketPlaceProject // ІМ'Я ПРОСТОРУ ІМЕН
{
    //---------------------------------------------------------------
    public static class GoodsData // Статичний клас для зберігання початкових даних товарів
    {
        private static int _nextId = 1; // Лічильник для унікальних ID товарів

        public static List<Goods> GetAllGoods() // Метод для отримання списку всіх товарів
        {
            return new List<Goods> // Повертаємо новий список товарів
            {
                // Смартфони, ТВ та електроніка
                new Goods(_nextId++, "iPhone 15 Pro", 45999, 10, "Смартфони, ТВ та електроніка"), // ID автоматично інкрементується
                new Goods(_nextId++, "Samsung Galaxy S23", 34999, 15, "Смартфони, ТВ та електроніка"),
                new Goods(_nextId++, "Xiaomi 13 Pro", 28999, 8, "Смартфони, ТВ та електроніка"),
                new Goods(_nextId++, "Sony Bravia 55\"", 25999, 5, "Смартфони, ТВ та електроніка"),
                new Goods(_nextId++, "LG OLED 65\"", 59999, 3, "Смартфони, ТВ та електроніка"),

                // Ноутбуки та комп'ютери
                new Goods(_nextId++, "MacBook Pro M3", 74999, 4, "Ноутбуки та комп'ютери"), // ID автоматично інкрементується
                new Goods(_nextId++, "Dell XPS 15", 69999, 6, "Ноутбуки та комп'ютери"),
                new Goods(_nextId++, "Lenovo ThinkPad", 45999, 8, "Ноутбуки та комп'ютери"),
                new Goods(_nextId++, "HP Spectre x360", 52999, 5, "Ноутбуки та комп'ютери"),
                new Goods(_nextId++, "Asus ROG Strix", 64999, 3, "Ноутбуки та комп'ютери"),

                // Товари для геймерів
                new Goods(_nextId++, "PlayStation 5", 20999, 3, "Товари для геймерів"), // ID автоматично інкрементується
                new Goods(_nextId++, "Xbox Series X", 19999, 4, "Товари для геймерів"),
                new Goods(_nextId++, "Nintendo Switch", 13999, 6, "Товари для геймерів"),
                new Goods(_nextId++, "Razer Gaming Mouse", 2999, 10, "Товари для геймерів"),
                new Goods(_nextId++, "Logitech Gaming Keyboard", 3999, 8, "Товари для геймерів"),

                // Побутова техніка
                new Goods(_nextId++, "LG Холодильник", 48999, 7, "Побутова техніка"), //    
                new Goods(_nextId++, "Dyson Пилосос", 25999, 9, "Побутова техніка"),
                new Goods(_nextId++, "Bosch Пральна машина", 32999, 6, "Побутова техніка"),
                new Goods(_nextId++, "Philips Мікрохвильова", 6999, 12, "Побутова техніка"),
                new Goods(_nextId++, "Redmond Кавоварка", 4999, 15, "Побутова техніка")
            };
        }

        public static List<string> GetCategories() // Метод для отримання списку категорій товарів
        {
            return new List<string> // Повертаємо новий список категорій
            {
                "Смартфони, ТВ та електроніка",
                "Ноутбуки та комп'ютери",
                "Товари для геймерів",
                "Побутова техніка"
            };
        }
    }

        public void ShowInfo()
        {
            Console.WriteLine($"ID:{Id}, Назва:{Name}, Ціна:{Price} грн, К-сть:{Quantity}, Категорія:{Category}");
        }
    }

    /// <summary>
    /// Інтерфейс оплати
    /// </summary>
    public interface IPay
    {
        void Pay();
    }

    /// <summary>
    /// Клас замовлення
    /// </summary>
    public class Basket : IPay
    {
        public Guid BasketId { get; set; }
        public Customer Customer { get; set; }
        public List<Goods> Items { get; set; } = new List<Goods>();
        public decimal TotalPrice { get; set; }
        public string Status { get; set; }

        public void CalcTotal()
        {
            TotalPrice = Items.Sum(x => x.Price * x.Quantity);
        }

        public void Pay()
        {
            Status = "Оплачено";
            Console.WriteLine($"Замовлення {BasketId} оплачено.");
        }
    }

    /// <summary>
    /// Клас клієнта
    /// </summary>
    public class Customer
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public List<Goods> Cart { get; set; } = new List<Goods>();

        public void AddToCart(Goods g, int qty)
        {
            if (g == null)
            {
                Console.WriteLine("Товар не знайдено!");
                return;
            }

            if (g.Quantity >= qty)
            {
                Cart.Add(new Goods { Id = g.Id, Name = g.Name, Price = g.Price, Quantity = qty, Category = g.Category });
                g.Quantity -= qty;
                Console.WriteLine($"{qty} шт. {g.Name} додано до корзини.");
            }
            else
            {
                Console.WriteLine($"Недостатньо товару {g.Name} на складі!");
            }
        }

        public Basket Checkout()
        {
            if (Cart.Count == 0)
            {
                Console.WriteLine("Корзина порожня!");
                return null;
            }
            var basket = new Basket { BasketId = Guid.NewGuid(), Customer = this, Items = new List<Goods>(Cart), Status = "Обробляється" };
            basket.CalcTotal();
            Cart.Clear();
            Console.WriteLine($"Замовлення {basket.BasketId} оформлено. Загальна сума: {basket.TotalPrice} грн.");
            return basket;
        }
    }

    /// <summary>
    /// Магазин (Singleton)
    /// </summary>
    public class Shop
    {
        private static Shop _instance;
        public static Shop Instance => _instance ??= new Shop();

        public List<Goods> GoodsList { get; set; } = new List<Goods>();
        public List<Basket> Baskets { get; set; } = new List<Basket>();

        public delegate void StockAlert(Goods g);
        public event StockAlert OnStockEmpty;

        public Goods this[int id]
        {
            get { return GoodsList.FirstOrDefault(x => x.Id == id); }
            set
            {
                var idx = GoodsList.FindIndex(x => x.Id == id);
                if (idx != -1) GoodsList[idx] = value;
                else GoodsList.Add(value);
            }
        }

        public void AddGoods(Goods g)
        {
            if (g != null)
            {
                GoodsList.Add(g);
                Console.WriteLine($"Товар {g.Name} додано до магазину.");
            }
        }

        public void RemoveGoods(Goods g)
        {
            if (g != null && GoodsList.Contains(g))
            {
                GoodsList.Remove(g);
                Console.WriteLine($"Товар {g.Name} видалено з магазину.");
            }
        }

    // 13. ПРОГРАМА
    class Program // ГОЛОВНИЙ КЛАС ПРОГРАМИ
    {
        static void Main() // ГОЛОВНИЙ МЕТОД ПРОГРАМИ
        {
            Console.Title = "🏪🛒 MarketPlace";
            Console.OutputEncoding = Encoding.UTF8; // Підтримка UTF-8 для емодзі

            var shop = ShopManager.Instance; // Отримуємо єдиний екземпляр ShopManager
            var customer = Customer.Instance; //    

            shop.LowStockAlert += (sender, msg) => // Обробник події низьких запасів
            {
                Console.ForegroundColor = ConsoleColor.Yellow; // Встановлюємо колір
                Console.WriteLine($"\n[!] {msg}"); // Виводимо повідомлення про низькі запаси
                Console.ResetColor(); // Скидаємо колір
            }; // Підписуємося на подію низьких запасів

            customer.Cart.CartChanged += (sender, e) => // Обробник події зміни корзини
            {
                Console.ForegroundColor = ConsoleColor.Magenta; // Встановлюємо колір 
                Console.WriteLine($"\n[Корзина] {e.Message}"); // Виводимо повідомлення про зміну корзини
                Console.ResetColor(); // Скидаємо колір
            }; // Підписуємося на подію зміни корзини

            MainMenu(shop, customer); // Викликаємо головне меню
        }

        public void CheckStock()
        {
            foreach (var g in GoodsList)
            {
                if (g.Quantity <= 0)
                    OnStockEmpty?.Invoke(g);
            }
        }

        public void Save(string path)
        {
            File.WriteAllText(path, JsonSerializer.Serialize(GoodsList, new JsonSerializerOptions { WriteIndented = true }));
        }

        public void Load(string path)
        {
            if (File.Exists(path))
            {
                var json = File.ReadAllText(path);
                var list = JsonSerializer.Deserialize<List<Goods>>(json);
                if (list != null) GoodsList = list;
            }
        }
    }

    /// <summary>
    /// Фабрика товарів
    /// </summary>
    public static class GoodsFactory
    {
        public static Goods Create(string category, int id, string name, decimal price, int qty)
        {
            if (string.IsNullOrEmpty(name)) name = "Без назви";
            switch (category?.ToLower())
            {
                case "electronics": return new Goods { Id = id, Name = name, Price = price, Quantity = qty, Category = "Електроніка" };
                case "clothing": return new Goods { Id = id, Name = name, Price = price, Quantity = qty, Category = "Одяг" };
                default: return new Goods { Id = id, Name = name, Price = price, Quantity = qty, Category = category ?? "Інше" };
            }
        }
    }

    class Program
    {
        static void Main()
        {
            Console.OutputEncoding = Encoding.UTF8;

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
        }*/

        static void Checkout(Customer customer) // МЕТОД ОФОРМЛЕННЯ ЗАМОВЛЕННЯ
        {
            if (!customer.Cart.Items.Any())
            {
                Console.WriteLine("Корзина порожня!");
                return;
            } // Перевірка чи корзина порожня

            Console.WriteLine("\nВикористати існуючі дані профілю або ввести нові?");
            Console.WriteLine("1. Використати існуючі");
            Console.WriteLine("2. Ввести нові");

            int choice = UI.GetChoice(1, 2); // Отримуємо вибір користувача
            if (choice == 2)
            {
                Console.Write("Ваше ім'я: ");
                customer.Name = Console.ReadLine();

                Console.Write("Телефон: ");
                customer.Phone = Console.ReadLine();

                Console.Write("Адреса: ");
                customer.Address = Console.ReadLine();

                Console.Write("Аккаунт: ");
                customer.Email = Console.ReadLine();
            }

            var order = new Order(customer.Orders.Count + 1, customer.Cart.Items.ToList()); // Створюємо нове замовлення
            customer.Orders.Add(order); // Додаємо замовлення до списку замовлень покупця
            customer.Cart.Clear(); // Очищаємо корзину

            Console.WriteLine($"\n✅ Замовлення #{order.Id} оформлено для {customer.Name}!");
            Console.WriteLine($"💰 Сума: {order.Total} грн");

            // Пропозиція зберегти замовлення у файл
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
        static void ShowOrders(Customer customer) // МЕНЮ ЗАМОВЛЕНЬ
        {
            UI.ShowHeader("Мої замовлення");

            if (!customer.Orders.Any()) // Перевірка чи є замовлення
                Console.WriteLine("У вас ще немає замовлень");
            else
                foreach (var order in customer.Orders) // Відображення кожного замовлення
                    Console.WriteLine(order);

            Console.ReadKey();
        }

        //---------------------------------------
        static void Settings(Customer customer) // МЕНЮ НАЛАШТУВАНЬ
        {
            while (true) // Цикл для відображення налаштувань
            {
                UI.ShowHeader("Налаштування профілю");
                Console.WriteLine($"1. Ім'я: {customer.Name}");
                Console.WriteLine($"2. Телефон: {customer.Phone}");
                Console.WriteLine($"3. Адреса: {customer.Address}");
                Console.WriteLine($"4. Аккаунт: {customer.Email}");
                Console.WriteLine("0. Назад");

                int choice = UI.GetChoice(0, 4); // Отримуємо вибір користувача
                if (choice == 0) break; // Повертаємося до головного меню

                Console.Write("Введіть нове значення: "); //
                //string input = Console.ReadLine();
                switch (choice) // Оновлюємо відповідне поле
                {
                    case 1: customer.Name = Console.ReadLine(); break;
                    case 2: customer.Phone = Console.ReadLine(); break;
                    case 3: customer.Address = Console.ReadLine(); break;
                    case 4: customer.Email = Console.ReadLine(); break;
                }

                Console.WriteLine("✔️ Значення оновлено");
                Console.ReadKey();
            }
        }

        //---------------------------------------

        static void FileOperations(Customer customer) // МЕНЮ РОБОТИ З ФАЙЛАМИ
        {
            UI.ShowHeader("Робота з файлами");

            // Зберігаємо тільки останнє замовлення
            FileManager.SaveLastOrder(customer); // Виклик методу для збереження останнього замовлення

            // Просте повідомлення користувачу
            Console.WriteLine("✅ Замовлення збережене у файл marketplace_data.txt");

            Console.ReadKey(); // Очікуємо натискання клавіші
        }


    }
}