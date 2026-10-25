using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace ConsoleApplication1
{
    internal enum OrderStatus
    {
        New = 1,
        Processing = 2,
        Completed = 3,
        Cancelled = 4
    }

    internal struct ProductInfo
    {
        public ProductInfo(string name, decimal price, int quantity)
            : this()
        {
            Name = name;
            Price = price;
            Quantity = quantity;
        }

        public string Name { get; private set; }
        public decimal Price { get; private set; }
        public int Quantity { get; private set; }

        public decimal Total
        {
            get { return Price * Quantity; }
        }
    }

    internal class Order
    {
        public Order(int id, ProductInfo product)
        {
            Id = id;
            Product = product;
            Status = OrderStatus.New;
            CreatedAt = DateTime.Now;
        }

        public int Id { get; private set; }
        public ProductInfo Product { get; private set; }
        public OrderStatus Status { get; private set; }
        public DateTime CreatedAt { get; private set; }

        public bool CanChangeTo(OrderStatus newStatus)
        {
            switch (Status)
            {
                case OrderStatus.New:
                    return newStatus == OrderStatus.Processing || newStatus == OrderStatus.Cancelled;

                case OrderStatus.Processing:
                    return newStatus == OrderStatus.Completed || newStatus == OrderStatus.Cancelled;

                default:
                    return false;
            }
        }

        public void ChangeStatus(OrderStatus newStatus)
        {
            if (Status == newStatus)
                throw new InvalidOperationException("Фармоиш аллакай дар ҳолати «" + StatusText.Get(newStatus) + "» аст.");

            if (!CanChangeTo(newStatus))
                throw new InvalidOperationException(
                    "Гузариш аз «" + StatusText.Get(Status) + "» ба «" + StatusText.Get(newStatus) + "» мумкин нест.");

            Status = newStatus;
        }
    }

    internal static class StatusText
    {
        public static string Get(OrderStatus status)
        {
            switch (status)
            {
                case OrderStatus.New:
                    return "Нав";

                case OrderStatus.Processing:
                    return "Дар раванд";

                case OrderStatus.Completed:
                    return "Анҷомёфта";

                case OrderStatus.Cancelled:
                    return "Бекоршуда";

                default:
                    return "Номаълум";
            }
        }
    }

    internal class Program
    {
        private static List<Order> orders = new List<Order>();
        private static int nextId = 1;

        private static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;

            while (true)
            {
                ShowMenu();
                string choice = Console.ReadLine();
                choice = (choice == null) ? "" : choice.Trim();
                Console.WriteLine();

                switch (choice)
                {
                    case "1":
                        AddOrder();
                        break;

                    case "2":
                        ChangeOrderStatus();
                        break;

                    case "3":
                        ShowAll();
                        break;

                    case "4":
                        ShowByStatus();
                        break;

                    case "0":
                        Console.WriteLine("Хайр!");
                        return;

                    default:
                        Console.WriteLine("Хато: интихоби нодуруст.");
                        break;
                }

                Console.WriteLine();
            }
        }

        private static void ShowMenu()
        {
            Console.WriteLine("=== Системаи фармоиши маҳсулот ===");
            Console.WriteLine("1. Фармоиши нав");
            Console.WriteLine("2. Тағйир додани ҳолати фармоиш");
            Console.WriteLine("3. Ҳамаи фармоишҳо");
            Console.WriteLine("4. Фармоишҳо аз рӯи ҳолат");
            Console.WriteLine("0. Баромадан");
            Console.Write("Интихоб: ");
        }

        private static void AddOrder()
        {
            string name = ReadText("Номи маҳсулот: ");
            decimal price = ReadMoney("Нарх: ");
            int quantity = ReadInt("Миқдор: ", 1, 100000);

            ProductInfo product = new ProductInfo(name, price, quantity);
            Order order = new Order(nextId, product);
            nextId++;

            orders.Add(order);

            Console.WriteLine("Фармоиш №" + order.Id + " сохта шуд.");
            Console.WriteLine("Маблағи умумӣ: " + Format(product.Total));
        }

        private static void ChangeOrderStatus()
        {
            if (orders.Count == 0)
            {
                Console.WriteLine("Фармоише нест.");
                return;
            }

            ShowAll();

            int id = ReadInt("Рақами фармоиш: ", 1, nextId - 1);
            Order order = FindOrder(id);

            if (order == null)
            {
                Console.WriteLine("Хато: фармоиш ёфт нашуд.");
                return;
            }

            Console.WriteLine();
            Console.WriteLine("Ҳолати ҳозира: " + StatusText.Get(order.Status));
            Console.WriteLine("1. " + StatusText.Get(OrderStatus.New));
            Console.WriteLine("2. " + StatusText.Get(OrderStatus.Processing));
            Console.WriteLine("3. " + StatusText.Get(OrderStatus.Completed));
            Console.WriteLine("4. " + StatusText.Get(OrderStatus.Cancelled));

            int number = ReadInt("Ҳолати нав: ", 1, 4);
            OrderStatus newStatus = (OrderStatus)number;

            try
            {
                order.ChangeStatus(newStatus);
                Console.WriteLine("Ҳолати фармоиш №" + order.Id + ": " + StatusText.Get(order.Status));
            }
            catch (InvalidOperationException ex)
            {
                Console.WriteLine("Хато: " + ex.Message);
            }
        }

        private static void ShowAll()
        {
            if (orders.Count == 0)
            {
                Console.WriteLine("Фармоише нест.");
                return;
            }

            Console.WriteLine("--- Ҳамаи фармоишҳо (" + orders.Count + ") ---");
            for (int i = 0; i < orders.Count; i++)
            {
                PrintOrder(orders[i]);
            }
        }

        private static void ShowByStatus()
        {
            Console.WriteLine("1. " + StatusText.Get(OrderStatus.New));
            Console.WriteLine("2. " + StatusText.Get(OrderStatus.Processing));
            Console.WriteLine("3. " + StatusText.Get(OrderStatus.Completed));
            Console.WriteLine("4. " + StatusText.Get(OrderStatus.Cancelled));

            int number = ReadInt("Ҳолатро интихоб кунед: ", 1, 4);
            OrderStatus status = (OrderStatus)number;

            int found = 0;
            decimal sum = 0m;

            for (int i = 0; i < orders.Count; i++)
            {
                if (orders[i].Status == status)
                {
                    PrintOrder(orders[i]);
                    found++;
                    sum += orders[i].Product.Total;
                }
            }

            if (found == 0)
            {
                Console.WriteLine("Фармоиш бо ҳолати «" + StatusText.Get(status) + "» нест.");
            }
            else
            {
                Console.WriteLine("Шумора: " + found + ", маблағи умумӣ: " + Format(sum));
            }
        }

        private static void PrintOrder(Order order)
        {
            Console.WriteLine("№" + order.Id + " | " + order.Product.Name
                + " | " + Format(order.Product.Price) + " x " + order.Product.Quantity
                + " = " + Format(order.Product.Total)
                + " | " + StatusText.Get(order.Status)
                + " | " + order.CreatedAt.ToString("dd.MM.yyyy HH:mm"));
        }

        private static Order FindOrder(int id)
        {
            for (int i = 0; i < orders.Count; i++)
            {
                if (orders[i].Id == id)
                    return orders[i];
            }

            return null;
        }

        private static string Format(decimal value)
        {
            return value.ToString("0.00", CultureInfo.InvariantCulture);
        }

        private static string ReadText(string prompt)
        {
            while (true)
            {
                Console.Write(prompt);
                string input = Console.ReadLine();
                input = (input == null) ? "" : input.Trim();

                if (input.Length > 0)
                    return input;

                Console.WriteLine("Хато: майдон холӣ буда наметавонад.");
            }
        }

        private static int ReadInt(string prompt, int min, int max)
        {
            while (true)
            {
                Console.Write(prompt);
                string input = Console.ReadLine();

                int value;
                if (int.TryParse(input, out value) && value >= min && value <= max)
                    return value;

                Console.WriteLine("Хато: адади бутун аз " + min + " то " + max + " ворид кунед.");
            }
        }

        private static decimal ReadMoney(string prompt)
        {
            while (true)
            {
                Console.Write(prompt);
                string input = Console.ReadLine();
                input = (input == null) ? "" : input.Trim().Replace(',', '.');

                decimal value;
                if (decimal.TryParse(input, NumberStyles.Number, CultureInfo.InvariantCulture, out value)
                    && value > 0)
                {
                    return value;
                }

                Console.WriteLine("Хато: адади аз сифр зиёд ворид кунед.");
            }
        }
    }
}