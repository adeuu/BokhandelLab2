using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Linq;

namespace BokhandelLab2
{
     

    public class BokhandelDbContext : DbContext
    {
        
        public BokhandelDbContext() : base("BokhandelDb")
        {
            this.Configuration.LazyLoadingEnabled = false;
        }

       
        public DbSet<Butik> Butiker { get; set; }
      
        public DbSet<Bok> Bocker { get; set; }
      
        public DbSet<LagerSaldo> Lagersaldo { get; set; }
    }

    

    [Table("Butiker")]
    public class Butik
    {
        [Key]
         public int ButikID { get; set; }

        [Required, StringLength(100)]
     public string Namn { get; set; }

        [Required, StringLength(200)]
      public string Adress { get; set; }

           [Required, StringLength(10)]
      public string Postnummer { get; set; }

        [Required, StringLength(100)]
           public string Stad { get; set; }
    }

    [Table("Bocker")]
    public class Bok
    {
       [Key, StringLength(13)]
        public string ISBN13 { get; set; }

          [Required, StringLength(200)]
        public string Titel { get; set; }

     [Required, StringLength(50)]
        public string Sprak { get; set; }

          public decimal Pris { get; set; }

      public DateTime? Utgivningsdatum { get; set; }

        public int ForfattareID { get; set; }
    }

    [Table("Lagersaldo")]
    public class LagerSaldo
    {
       [Key, Column(Order = 0)]
        public int ButikID { get; set; }

          [Key, Column(Order = 1), StringLength(13)]
      public string ISBN13 { get; set; }

       public int Antal { get; set; }
    }

    internal class Program
    {
        static void Main(string[] args)
        {
            using (var db = new BokhandelDbContext())
            {
              while (true)
                {
                  Console.Clear();
                    Console.WriteLine("Abdul Bokhandel");
                   Console.WriteLine("1. Visa lagersaldo");
                  Console.WriteLine("2. Lägg till bok i butik");
                    Console.WriteLine("3. Ta bort bok från butik");
                  Console.WriteLine("0. Avsluta");
                    Console.Write("Välj alternativ: ");

                    var choice = Console.ReadLine();

                    if (choice == "1") ShowInventory(db);
                  else if (choice == "2") AddBookToStore(db);
                    else if (choice == "3") RemoveBookFromStore(db);
                  else if (choice == "0") break;
                    else
                    {
                        Console.WriteLine("Fel val. Tryck valfri tangent...");
                        Console.ReadKey();
                    }
                }
            }
        }

      

        static int ReadInt(string text)
        {
            while (true)
            {
                Console.Write(text);
                var s = Console.ReadLine();

                if (int.TryParse(s, out int value))
                    return value;

                Console.WriteLine("Ogiltigt tal, försök igen.");
            }
        }

        static void ListStores(BokhandelDbContext db)
        {
            Console.WriteLine("Butiker:");

            var stores = db.Butiker
                           .OrderBy(b => b.Namn)
                           .ToList();

            foreach (var butik in stores)
            {
                Console.WriteLine($"{butik.ButikID}: {butik.Namn}");
            }
        }

        static void ListBooks(BokhandelDbContext db)
        {
            Console.WriteLine("Böcker:");

            var books = db.Bocker
                          .OrderBy(b => b.Titel)
                          .ToList();

            foreach (var bok in books)
            {
                Console.WriteLine($"{bok.ISBN13}: {bok.Titel}");
            }
        }


        static void ShowInventory(BokhandelDbContext db)
        {
            Console.Clear();
            Console.WriteLine("Lagersaldo per butik");

            var saldon =
                from ls in db.Lagersaldo
                join bok in db.Bocker on ls.ISBN13 equals bok.ISBN13
                join butik in db.Butiker on ls.ButikID equals butik.ButikID
                orderby butik.Namn, bok.Titel
                select new
                {
                    Butik = butik.Namn,
                    Titel = bok.Titel,
                    Antal = ls.Antal
                };

            string currentStore = null;

            foreach (var row in saldon)
            {
                if (currentStore != row.Butik)
                {
                    currentStore = row.Butik;
                   Console.WriteLine();
                    Console.WriteLine("Butik: " + currentStore);
                }

                Console.WriteLine($"  - {row.Titel} (Antal: {row.Antal})");
            }

            Console.WriteLine();
          Console.WriteLine("Tryck valfri tangent för att fortsätta...");
            Console.ReadKey();
        }


        static void AddBookToStore(BokhandelDbContext db)
        {
            Console.Clear();
            Console.WriteLine("Lägg till bok i butik");

            ListStores(db);
            int butikId = ReadInt("Ange ButikID: ");

            var butik = db.Butiker.SingleOrDefault(b => b.ButikID == butikId);
            if (butik == null)
            {
                Console.WriteLine("Butik hittades inte.");
             Console.ReadKey();
                return;
            }

            ListBooks(db);
          Console.Write("Ange ISBN13: ");
            string isbn = Console.ReadLine();

            var bok = db.Bocker.SingleOrDefault(b => b.ISBN13 == isbn);
            if (bok == null)
            {
                Console.WriteLine("Bok hittades inte.");
             Console.ReadKey();
                return;
            }

            int antal = ReadInt("Hur många exemplar vill du lägga till? ");

            var saldo = db.Lagersaldo
                          .SingleOrDefault(x => x.ButikID == butikId && x.ISBN13 == isbn);

            if (saldo == null)
            {
                saldo = new LagerSaldo
                {
                    ButikID = butikId,
                   ISBN13 = isbn,
                    Antal = antal
                };
                db.Lagersaldo.Add(saldo);
            }
            else
            {
                saldo.Antal += antal;
            }

            db.SaveChanges();
           Console.WriteLine("Lagersaldo uppdaterat!");
            Console.ReadKey();
        }

       

        static void RemoveBookFromStore(BokhandelDbContext db)
        {
            Console.Clear();
            Console.WriteLine("Ta bort bok från butik");

            ListStores(db);
            int butikId = ReadInt("Ange ButikID: ");

            var butik = db.Butiker.SingleOrDefault(b => b.ButikID == butikId);
            if (butik == null)
            {
                Console.WriteLine("Butik hittades inte.");
              Console.ReadKey();
                return;
            }

            ListBooks(db);
          Console.Write("Ange ISBN13: ");
            string isbn = Console.ReadLine();

            var saldo = db.Lagersaldo
                          .SingleOrDefault(x => x.ButikID == butikId && x.ISBN13 == isbn);

            if (saldo == null)
            {
                Console.WriteLine("Denna butik har inga exemplar av den boken.");
              Console.ReadKey();
                return;
            }

            int antal = ReadInt("Hur många exemplar vill du ta bort? ");

            if (antal <= 0)
            {
                Console.WriteLine("Antal måste vara > 0.");
               
                
              Console.ReadKey();
                return;
            }

            if (antal > saldo.Antal)
            {
                Console.WriteLine($"Det finns bara {saldo.Antal} exemplar.");
                Console.ReadKey();
                return;
            }

            saldo.Antal -= antal;

            if (saldo.Antal == 0)
            {
                db.Lagersaldo.Remove(saldo);
            }

            db.SaveChanges();
          
           Console.WriteLine("Lagersaldo uppdaterat!");
            Console.ReadKey();
        }
    }
}
