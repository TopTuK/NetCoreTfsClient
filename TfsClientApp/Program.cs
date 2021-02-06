using System;
using TfsClient;

namespace TfsClientApp
{
    class Program
    {
        private static string LINE_SEPARATOR = new string('*', 30);

        static void Main(string[] args)
        {
            Console.WriteLine("Hello to .net Core Tfs Sample application!");

            /*
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
            */
            var tfsServerUrl = @"https://smartagency.visualstudio.com/";
            var tfsCollection = "DefaultCollection";

            Console.Write("Personal access token: ");
            var personalAccessToken = Console.ReadLine();

            var tfsService = TfsServiceClientFactory.CreateTfsServiceClient(tfsServerUrl, tfsCollection,
                personalAccessToken);

            Console.Write("Enter Workitem id: ");
            if(int.TryParse(Console.ReadLine(), out int workId))
            {
                var item = tfsService.GetSingleWorkitem(workId);
                if(item != null)
                {
                    DisplayTfsItemDetails(item);
                    DisplayTfsItemRelations(item);
                    //DisplayTfsItemChilds(item);

                    Console.WriteLine();
                    Console.WriteLine("*** CHANGE ITEM TITLE ***");
                    item["System.Title"] = string.Format("{0} - edited", item["System.Title"]);
                    item.UpdateFields();
                    DisplayTfsItemDetails(item);

                    Console.WriteLine();
                    Console.WriteLine("*** Manage relations ***");
                    item = tfsService.AddRelationLink(2, 3, WorkitemRelationType.Affects);
                    if (item != null)
                    {
                        DisplayTfsItemRelations(item);

                        item.RemoveRelationLinks(3);
                        DisplayTfsItemRelations(item);
                    }
                    else
                    {
                        Console.WriteLine("ITEM is null");
                    }
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
            Console.WriteLine(LINE_SEPARATOR);

            Console.WriteLine($"Found item: {workitem.Id}");
            Console.WriteLine($"Workitem type: {workitem.ItemType} - {workitem.ItemTypeName}");
            Console.WriteLine($"Workitem url: {workitem.Url}");
            Console.WriteLine($"Workitem fields: {string.Join(',', workitem.FieldNames)}");

            Console.WriteLine($"Workitem title: {workitem["System.Title"]}");

            Console.WriteLine(LINE_SEPARATOR);
        }

        private static void DisplayTfsItemRelations(ITfsWorkitem workitem)
        {
            Console.WriteLine(LINE_SEPARATOR);

            Console.WriteLine("Workitem relations");
            foreach(var rel in workitem.Relations)
            {
                Console.WriteLine($"[{rel.RelationTypeName}]: {rel.RelationType} -> {rel.WorkitemId}");
            }

            Console.WriteLine(LINE_SEPARATOR);
        }

        private static void DisplayTfsItemChilds(ITfsWorkitem workitem)
        {
            Console.WriteLine(LINE_SEPARATOR);

            Console.WriteLine("Workitem CHILD relations");
            var items = workitem.GetRelatedWorkitems(WorkitemRelationType.Child);

            foreach(var item in items)
            {
                Console.WriteLine($"{item.Id} [{item.ItemType} : {item.ItemTypeName}] {item["System.Title"]}");
            }

            Console.WriteLine(LINE_SEPARATOR);
        }
    }
}
