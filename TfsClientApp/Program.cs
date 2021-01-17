using System;
using TfsClient;

namespace TfsClientApp
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello to .net Core Tfs Sample application!");

            Console.Write("Enter TFS server URl: ");
            var tfsServerUrl = Console.ReadLine();
            Console.Write("Enter TFS project: ");
            var tfsCollection = Console.ReadLine();

            Console.Write("Username: ");
            var userName = Console.ReadLine();
            Console.Write("Password: ");
            var userPassword = Console.ReadLine();

            var tfsService = TfsServiceClientFactory.CreateTfsServiceClient(tfsServerUrl, tfsCollection,
                userName, userPassword);

            var item = tfsService.GetSingleWorkitem(100);

            Console.WriteLine("Meow!");
        }
    }
}
