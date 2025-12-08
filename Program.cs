using System; 
using System.Collections.Generic; 
using System.IO;
using System.Linq; 
using System.Runtime.InteropServices;
using System.Text; 

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
                new Goods(_nextId++, "iPhone 15 Pro", 45999, 10, "Смартфони, ТВ та електроніка"), 
                new Goods(_nextId++, "Samsung Galaxy S23", 34999, 15, "Смартфони, ТВ та електроніка"),
                new Goods(_nextId++, "Xiaomi 13 Pro", 28999, 8, "Смартфони, ТВ та електроніка"),
                new Goods(_nextId++, "Sony Bravia 55\"", 25999, 5, "Смартфони, ТВ та електроніка"),
                new Goods(_nextId++, "LG OLED 65\"", 59999, 3, "Смартфони, ТВ та електроніка"),

                // Ноутбуки та комп'ютери
                new Goods(_nextId++, "MacBook Pro M3", 74999, 4, "Ноутбуки та комп'ютери"), 
                new Goods(_nextId++, "Dell XPS 15", 69999, 6, "Ноутбуки та комп'ютери"),
                new Goods(_nextId++, "Lenovo ThinkPad", 45999, 8, "Ноутбуки та комп'ютери"),
                new Goods(_nextId++, "HP Spectre x360", 52999, 5, "Ноутбуки та комп'ютери"),
                new Goods(_nextId++, "Asus ROG Strix", 64999, 3, "Ноутбуки та комп'ютери"),

                // Товари для геймерів
                new Goods(_nextId++, "PlayStation 5", 20999, 3, "Товари для геймерів"), 
                new Goods(_nextId++, "Xbox Series X", 19999, 4, "Товари для геймерів"),
                new Goods(_nextId++, "Nintendo Switch", 13999, 6, "Товари для геймерів"),
                new Goods(_nextId++, "Razer Gaming Mouse", 2999, 10, "Товари для геймерів"),
                new Goods(_nextId++, "Logitech Gaming Keyboard", 3999, 8, "Товари для геймерів"),

                // Побутова техніка
                new Goods(_nextId++, "LG Холодильник", 48999, 7, "Побутова техніка"),     
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

    //-------------------------------------------------------------------
    // 2. ІНТЕРФЕЙСИ та ООП
    public interface ISerializableEntity { string Serialize(); } // Інтерфейс для серіалізації

    //-------------------------------------------------------------------
    // 3. ДЕЛЕГАТИ та ПОДІЇ
    public delegate void StockEventHandler(object sender, string message); 
    public delegate void CartEventHandler(object sender, CartEventArgs e); 

    public class CartEventArgs : EventArgs // Клас для аргументів подій корзини
    {
        public string Message { get; } // Повідомлення події
        public CartEventArgs(string message) => Message = message; // Конструктор
    }

    //-------------------------------------------------------------------
    // 4. АБСТРАКТНИЙ КЛАС
    [Serializable] // Позначає клас як серіалізований
    public abstract class ProductBase // АБСТРАКТНИЙ КЛАС ДЛЯ ПРОДУКТІВ
    {
        public int Id { get; protected set; } // Унікальний ідентифікатор продукту
        public string Name { get; protected set; } // Назва продукту
        public decimal Price { get; protected set; } // Ціна продукту
        public string Category { get; protected set; } // Категорія продукту

        protected ProductBase(int id, string name, decimal price, string category) // Конструктор
        {
            Id = id; Name = name; Price = price; Category = category; //    
        }

        public abstract string GetDescription(); // Абстрактний метод для отримання опису продукту
    }

    //-------------------------------------------------------------------
    // 5. КЛАС ТОВАРУ
    [Serializable] // Позначає клас як серіалізований
    public class Goods : ProductBase, ICloneable, ISerializableEntity // КЛАС ТОВАРУ
    {
        private int _quantity; // Приватне поле для кількості товару
        public int Quantity // Властивість для кількості товару
        {
            get => _quantity; // Геттер
            set => _quantity = value >= 0 ? value : throw new ArgumentException("Кількість не може бути від'ємною"); // Сеттер з перевіркою
        }

        public Goods(int id, string name, decimal price, int quantity, string category)
            : base(id, name, price, category) => Quantity = quantity; // Конструктор

        public override string ToString() => $"{Name} - {Price} грн ({Quantity} шт.)"; // Переопределение метода ToString
        public override string GetDescription() => $"Товар: {Name}, Категорія: {Category}"; // Переопределение абстрактного метода
        public object Clone() => new Goods(Id, Name, Price, Quantity, Category); // Реалізація методу клонування
        public string Serialize() => $"{Id}|{Name}|{Price}|{Quantity}|{Category}"; //   

        public static Goods operator +(Goods g, int qty) // Перевантаження оператора +
        {
            var result = (Goods)g.Clone(); // Клонуємо об'єкт
            result.Quantity += qty; // Додаємо кількість
            return result; // Повертаємо новий об'єкт
        }

        public static bool operator ==(Goods a, Goods b) // 
        {
            if (ReferenceEquals(a, b)) return true; //  Перевірка на однакові посилання
            if (a is null || b is null) return false; // Перевірка на null
            return a.Id == b.Id; // Порівняння за ID
        }

        public static bool operator !=(Goods a, Goods b) => !(a == b); // Перевантаження оператора !=
        public override bool Equals(object? obj) => obj is Goods goods && this == goods; // Переопределение метода Equals
        public override int GetHashCode() => Id.GetHashCode(); // Переопределение метода GetHashCode
    }

    //-------------------------------------------------------------------
    // 6. GENERICS КОЛЕКЦІЯ
    public class GoodsCollection<T> : IEnumerable<T> where T : Goods // GENERICS КОЛЕКЦІЯ
    {
        private List<T> _items = new List<T>(); // Внутрішній список товарів
        public T? this[int id] => _items.FirstOrDefault(g => g.Id == id); // Індексатор за ID
        public IEnumerable<T> this[string name] => _items.Where(g => g.Name.Contains(name, StringComparison.OrdinalIgnoreCase)); // Індексатор за назвою

        public void Add(T item) => _items.Add(item); // Метод для додавання товару
        public void AddRange(IEnumerable<T> items) => _items.AddRange(items); // Метод для додавання колекції товарів
        public IEnumerator<T> GetEnumerator() => _items.GetEnumerator(); // Реалізація IEnumerable
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
        public int Count => _items.Count; // Властивість для отримання кількості товарів
        public decimal TotalValue => _items.Sum(g => g.Price * g.Quantity); //  Властивість для отримання загальної вартості товарів
        public IEnumerable<T> Where(Func<T, bool> predicate) => _items.Where(predicate); // Метод для фільтрації товарів
        public IEnumerable<T> OrderBy<TKey>(Func<T, TKey> keySelector) => _items.OrderBy(keySelector); // Метод для сортування товарів
    }

    //-------------------------------------------------------------------
    // 7. КОРЗИНА
    public class ShoppingCart // КЛАС КОРЗИНИ
    {
        private List<Goods> _items = new List<Goods>(); // Внутрішній список товарів у корзині
        public event CartEventHandler? CartChanged; // Подія зміни корзини
        public IReadOnlyList<Goods> Items => _items.AsReadOnly(); // Властивість для отримання товарів у корзині
        public decimal TotalPrice => _items.Sum(i => i.Price * i.Quantity); // Властивість для отримання загальної вартості корзини

        public void AddItem(Goods goods, int quantity) // Метод для додавання товару у корзину
        {
            if (goods.Quantity < quantity) // Перевірка на наявність достатньої кількості
                throw new InvalidOperationException($"Недостатньо товару. Доступно: {goods.Quantity}"); // Викидаємо виключення

            var existing = _items.FirstOrDefault(i => i.Id == goods.Id); // Перевіряємо чи товар вже є у корзині
            if (existing is not null)
                existing.Quantity += quantity;    // Якщо є, збільшуємо кількість
            else
                _items.Add((Goods)goods.Clone()); // Інакше додаємо новий товар

            goods.Quantity -= quantity; // Зменшуємо кількість товару на складі
            OnCartChanged($"Додано: {goods.Name} x{quantity}"); // Викликаємо подію
        }

        public void Clear() // Метод для очищення корзини
        {
            _items.Clear(); // Очищаємо список товарів
            OnCartChanged("Корзина очищена"); // Викликаємо подію
        }

        protected virtual void OnCartChanged(string message) =>
            CartChanged?.Invoke(this, new CartEventArgs(message)); // Викликаємо подію зміни корзини
    }

    //-------------------------------------------------------------------
    // 8. SINGLETON покупець
    public sealed class Customer // SINGLETON КЛАС ПОКУПЦЯ
    {
        private static Customer? _instance; // Приватне статичне поле для зберігання єдиного екземпляра
        public static Customer Instance => _instance ??= new Customer(); // Властивість для отримання єдиного екземпляра

        public string? Name { get; set; } // Властивість для імені покупця
        public string? Phone { get; set; } // Властивість для телефону покупця
        public string? Address { get; set; } // Властивість для адреси покупця
        public string? Email { get; set; } // Властивість для електронної пошти покупця
        public ShoppingCart Cart { get; } = new ShoppingCart(); // Властивість для корзини покупця
        public List<Order> Orders { get; } = new List<Order>(); // Властивість для списку замовлень покупця

        private Customer() // Приватний конструктор
        {
            // Значення за замовчуванням
            Name = "Гість";
            Phone = "+380-21184823313";
            Address = "Місто, вулиця, будинок";
            Email = "guest@example.com";
        }
    }

    //-------------------------------------------------------------------
    // 9. ЗАМОВЛЕННЯ
    [Serializable] // Позначає клас як серіалізований
    public class Order // КЛАС ЗАМОВЛЕННЯ
    {
        public int Id { get; } // Властивість для унікального ідентифікатора замовлення
        public DateTime Date { get; } // Властивість для дати замовлення
        public List<Goods> Items { get; } // Властивість для списку товарів у замовленні
        public decimal Total { get; } // Властивість для загальної вартості замовлення

        public Order(int id, List<Goods> items) // Конструктор
        {
            Id = id; // Присвоюємо унікальний ідентифікатор
            Date = DateTime.Now; // Присвоюємо поточну дату
            Items = items.Select(i => (Goods)i.Clone()).ToList(); // Клонуємо товари у замовленні
            Total = items.Sum(i => i.Price * i.Quantity); // Обчислюємо загальну вартість
        }

        public override string ToString() =>
            $"Замовлення #{Id} від {Date:dd.MM.yyyy} - {Total} грн"; // Переопределение метода ToString
    }

    //-------------------------------------------------------------------
    // 10. SHOP MANAGER
    public sealed class ShopManager // SINGLETON КЛАС МЕНЕДЖЕРА МАГАЗИНУ
    {
        private static ShopManager? _instance; // Приватне статичне поле для зберігання єдиного екземпляра
        public static ShopManager Instance => _instance ??= new ShopManager(); // Властивість для отримання єдиного екземпляра
        public GoodsCollection<Goods> Goods { get; } = new GoodsCollection<Goods>(); // Властивість для колекції товарів
        public event StockEventHandler? LowStockAlert; // Подія для сповіщення про низькі запаси

        private ShopManager() // Приватний конструктор
        {
            Goods.AddRange(GoodsData.GetAllGoods()); // Ініціалізуємо колекцію товарів
            CheckStock(); // Перевіряємо запаси при створенні екземпляра
        }

        public void CheckStock() // Метод для перевірки запасів
        {
            foreach (var g in Goods.Where(g => g.Quantity <= 3)) // Перевіряємо товари з кількістю менше або рівною 3
                LowStockAlert?.Invoke(this, $"Увага! Закінчується: {g.Name} (залишилось: {g.Quantity})"); // Викликаємо подію для кожного товару з низьким запасом
        }

        public IEnumerable<Goods> Search(string query) => Goods[query]; // Метод для пошуку товарів за назвою
        public IEnumerable<Goods> FilterByPrice(decimal min, decimal max) =>
            Goods.Where(g => g.Price >= min && g.Price <= max).OrderBy(g => g.Price); // Метод для фільтрації товарів за ціною
    }

    //-------------------------------------------------------------------
    // 11. FILE MANAGER
    public static class FileManager // СТАТИЧНИЙ КЛАС ДЛЯ РОБОТИ З ФАЙЛАМИ
    {
        private const string FilePath = "marketplace_data.txt";
        private static bool _isFirstRun = true; // Флаг для очищення файлу при першому запуску

        // Зберігає тільки останнє замовлення у файл
        public static void SaveLastOrder(Customer customer) // Метод для збереження останнього замовлення у файл
        {
            if (!customer.Orders.Any()) return; // Якщо немає замовлень, виходимо

            var lastOrder = customer.Orders.Last(); // беремо тільки останнє замовлення
            var sb = new StringBuilder(); // Використовуємо StringBuilder для ефективного формування рядка

            // Очищення файлу при першому запуску програми
            if (_isFirstRun) // Якщо це перший запуск
            {
                File.WriteAllText(FilePath, string.Empty, Encoding.UTF8); // Очищаємо файл
                _isFirstRun = false; // Встановлюємо флаг у false
            }

            // --- OrderedGoods ---
            sb.AppendLine("#OrderedGoods"); // Заголовок блоку
            foreach (var g in lastOrder.Items.GroupBy(i => i.Name) // Групуємо товари за назвою
                                             .Select(g => new { Name = g.Key, Quantity = g.Sum(x => x.Quantity) }))
            {
                sb.AppendLine($"{g.Name}|{g.Quantity}"); // Додаємо рядок з назвою та кількістю
            }

            // --- Orders ---
            sb.AppendLine("#Orders");
            foreach (var item in lastOrder.Items) // Додаємо кожен товар замовлення
            {
                sb.AppendLine($"{lastOrder.Id}|{lastOrder.Date:yyyy-MM-dd HH:mm}|{customer.Name}|{item.Name}|{item.Quantity}|{item.Price * item.Quantity}");
            }

            // Роздільник після замовлення
            sb.AppendLine("------------------------------------------------------");

            // Додаємо блок у кінець файлу
            File.AppendAllText(FilePath, sb.ToString(), Encoding.UTF8); // Записуємо у файл
        }
    }

    //-------------------------------------------------------------------
    // 12. UI УТИЛІТИ
    public static class UI // СТАТИЧНИЙ КЛАС ДЛЯ UI УТИЛІТІВ
    {
        public static void ShowHeader(string title) // Метод для відображення заголовка
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"\n=== {title} ===");
            Console.ResetColor();
        }

        public static int GetChoice(int min, int max) // Метод для отримання вибору користувача
        {
            while (true)
            {
                Console.Write("\nВаш вибір: ");
                if (int.TryParse(Console.ReadLine(), out int choice) && choice >= min && choice <= max) // Перевірка на коректність вводу
                    return choice;
                Console.WriteLine($"Потрібно число від {min} до {max}");
            }
        }

        public static void ShowProduct(Goods g) // Метод для відображення інформації про товар
        {
            Console.ForegroundColor = g.Quantity <= 3 ? ConsoleColor.Yellow : ConsoleColor.Green; // Колір залежно від кількості
            Console.WriteLine($"[{g.Id}] {g.Name,-25} | {g.Category,-15} | {g.Price,8} грн | {g.Quantity,3} шт."); // Відображення інформації
            Console.ResetColor(); // Скидання кольору
        }
    }

    // 13. ПРОГРАМА
    class Program // ГОЛОВНИЙ КЛАС ПРОГРАМИ
    {
        static void Main() // ГОЛОВНИЙ МЕТОД ПРОГРАМИ
        {
            Console.Title = "MarketPlace";
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

        static void MainMenu(ShopManager shop, Customer customer) // ГОЛОВНЕ МЕНЮ
        {
            while (true)
            {
                UI.ShowHeader("Головне меню");
                Console.WriteLine($"Користувач: {customer.Name}");
                Console.WriteLine($"Корзина: {customer.Cart.Items.Count} товарів на {customer.Cart.TotalPrice} грн\n");

                Console.WriteLine("1.Каталог товарів");
                Console.WriteLine("2.Корзина");
                Console.WriteLine("3.Замовлення");
                Console.WriteLine("4.Налаштування");
                Console.WriteLine("5.Файли");
                Console.WriteLine("6.Вийти");

                switch (UI.GetChoice(1, 6))
                {
                    case 1: ShowCatalog(shop, customer); break;
                    case 2: ShowCart(customer); break;
                    case 3: ShowOrders(customer); break;
                    case 4: Settings(customer); break;
                    case 5: FileOperations(customer); break;
                    case 6:
                        Console.Clear();
                        Console.ForegroundColor = ConsoleColor.Cyan; // Встановлюємо колір
                        Console.WriteLine("\n======================================");
                        Console.WriteLine("Дякуємо за покупки, до зустрічі!");
                        Console.WriteLine("======================================\n");
                        Console.ResetColor(); // Скидаємо колір
                        Console.WriteLine("Натисніть будь-яку клавішу для завершення...");
                        Console.ReadKey(); // Очікуємо натискання клавіші
                        return;

                }
            }
        }
       
        static void ShowCatalog(ShopManager shop, Customer customer) // МЕНЮ КАТАЛОГУ
        {
            while (true)
            {
                UI.ShowHeader("Каталог товарів");

                var categories = GoodsData.GetCategories();

                for (int i = 0; i < categories.Count; i++) // Відображаємо категорії
                {
                    Console.WriteLine($"{i + 1}. {categories[i]}"); // Відображення категорії з номером
                }
                Console.WriteLine("0. Назад");

                int choice = UI.GetChoice(0, categories.Count); // Отримуємо вибір користувача
                if (choice == 0) break;

                string selectedCategory = categories[choice - 1];
                var productsInCategory = shop.Goods.Where(g => g.Category == selectedCategory).ToList(); // Фільтруємо товари за вибраною категорією

                while (true)
                {
                    UI.ShowHeader($"Категорія: {selectedCategory}");
                    foreach (var g in productsInCategory) // Відображаємо товари у вибраній категорії
                        UI.ShowProduct(g); // Відображення інформації про товар

                    Console.WriteLine("\n0. Назад");
                    Console.WriteLine("Введіть ID товару, щоб додати у корзину");

                    int id = UI.GetChoice(0, shop.Goods.Max(g => g.Id)); // Отримуємо вибір користувача
                    if (id == 0) break; // Повертаємося до вибору категорії

                    var product = shop.Goods[id]; // Знаходимо товар за ID
                    if (product is not null && product.Category == selectedCategory) // Перевіряємо чи товар належить вибраній категорії
                        AddToCart(product, customer); // Додаємо товар у корзину
                    else
                    {
                        Console.WriteLine("Товар не знайдено у цій категорії");
                        Console.ReadKey();
                    }
                }
            }
        }


        //---------------------------------------
        static void AddToCart(Goods product, Customer customer) // МЕТОД ДОДАВАННЯ ТОВАРУ В КОРЗИНУ
        {
            Console.Write($"Кількість (до {product.Quantity}): ");
            if (int.TryParse(Console.ReadLine(), out int qty) && qty > 0) // Перевірка на коректність вводу
            {
                try
                {
                    customer.Cart.AddItem(product, qty);
                    Console.WriteLine("Додано до корзини");
                }
                catch (Exception ex) { Console.WriteLine($"{ex.Message}"); }
            }
        }
        //---------------------------------------
        static void ShowCart(Customer customer) // МЕНЮ КОРЗИНИ
        {
            while (true) // Цикл для відображення корзини
            {
                UI.ShowHeader("Ваша корзина");

                if (!customer.Cart.Items.Any())
                    Console.WriteLine("Корзина порожня");
                else
                {
                    foreach (var item in customer.Cart.Items)
                        Console.WriteLine($"{item.Name} x{item.Quantity} = {item.Price * item.Quantity} грн");

                    Console.WriteLine($"\nЗагальна сума: {customer.Cart.TotalPrice} грн");
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

            Console.WriteLine($"\nЗамовлення #{order.Id} оформлено для {customer.Name}!");
            Console.WriteLine($"Сума: {order.Total} грн");

            // Пропозиція зберегти замовлення у файл
            Console.Write("Бажаєте зберегти замовлення у файл? (T - так/F - ні): ");
            var key = Console.ReadKey();
            Console.WriteLine();
            if (key.Key == ConsoleKey.T)
            {
                FileManager.SaveLastOrder(customer);
                Console.WriteLine("Замовлення збережене у файл marketplace_data.txt");
            }
            else
            {
                Console.WriteLine("Замовлення не збережене у файл");
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

                Console.WriteLine("Значення оновлено");
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
            Console.WriteLine("Замовлення збережене у файл marketplace_data.txt");

            Console.ReadKey(); // Очікуємо натискання клавіші
        }


    }
}