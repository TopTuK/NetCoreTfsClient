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

            Console.Write("Enter Workitem id: ");
            if(int.TryParse(Console.ReadLine(), out int workId))
            {
                var item = tfsService.GetSingleWorkitem(workId);
                if(item != null)
                {
                    DisplayTfsItemDetails(item);
                }
                else
                {
                    Console.WriteLine($"Item {workId} is not found!");
                }
            }
            else
            {
                Console.WriteLine("Invalid workitem id");
            }

            Console.WriteLine("Meow!");
        }

        private static void DisplayTfsItemDetails(ITfsWorkitem workitem)
        {
            Console.WriteLine($"Found item: {workitem.Id}");
            Console.WriteLine($"Workitem type: {workitem.ItemType} - {workitem.ItemTypeName}");
            Console.WriteLine($"Workitem url: {workitem.Url}");
            Console.WriteLine($"Workitem fields: {string.Join(',', workitem.FieldNames)}");

            Console.WriteLine($"Workitem title: {workitem["System.Title"]}");
        }
    }
}
