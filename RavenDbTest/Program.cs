using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Threading.Tasks;
using Raven.Client.Documents;
using Raven.Client.Documents.Linq;
using Raven.Client.Documents.Session;
using Raven.Client.ServerWide;
using Raven.Embedded;

namespace RavenDbTest
{
  class Program
  {
    static async Task Main(string[] args)
    {
      Console.WriteLine("Hello World!");
      
      var serOptions = new ServerOptions
      {
        FrameworkVersion = "5.0.3",
        ServerDirectory = $"{Environment.CurrentDirectory}\\RavenDBServer"
      };

      EmbeddedServer.Instance.StartServer(serOptions);

      EmbeddedServer.Instance.OpenStudioInBrowser();

      Uri url = await EmbeddedServer.Instance.GetServerUriAsync();
      Console.WriteLine("Server started!");

      var databaseOptions = new DatabaseOptions(new DatabaseRecord
      {
        DatabaseName = "TestDb"
      });

      using IDocumentStore store = await EmbeddedServer.Instance.GetDocumentStoreAsync(databaseOptions);
      store.Initialize();

      Console.WriteLine("Store initialized!");
      using var session = store.OpenAsyncSession();
     
      Address testAddress = new Address
      {
        Number = 3,
        Street = "Factory st."
      };

      await session.StoreAsync(testAddress);

      foreach (Person x in LoadDummyObjects(100000))
      {
        x.AddressId = testAddress.Id;
        await session.StoreAsync(x);
      }

      Console.WriteLine("Dummy objects added to session!");

      await session.SaveChangesAsync();

      Console.WriteLine("Saved!");

      List<string> pros = await session
        .Query<Person>()
        .Select(x => x.FirstName)                          
        .ToListAsync();

      Console.WriteLine($"{pros.Count} queried!");

 

      Console.ReadLine();
    }


    private static List<Person> LoadDummyObjects(int count)
    {
      var result = new List<Person>();
      for (var i = 0; i <= count; i++)
      {
        result.Add(new Person { FirstName = "First name", LastName = "Surname", Age = 22 });
      }

      return result;
    }

    private class Person
    {
      public string Id { get; set; }
      public string FirstName { get; set; }
      public string LastName { get; set; }
      public int Age { get; set; }
      public string AddressId { get; set; }
    }

    private class Address
    {
      public string Id { get; set; }
      public string Street { get; set; }
      public int Number { get; set; }
    }
  }
}
