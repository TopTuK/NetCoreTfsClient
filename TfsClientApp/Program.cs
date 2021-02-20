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
            var tfsCollection = "DefaultCollection/Test TFS project";

            Console.Write("Personal access token: ");
            var personalAccessToken = Console.ReadLine();

            var tfsService = TfsServiceClientFactory.CreateTfsServiceClient(tfsServerUrl, tfsCollection,
                personalAccessToken);

            DisplayWiqlQueryByIdResult(tfsService);

            Console.WriteLine("Meow!");
        }

        private static void DisplayWiqlQueryByIdResult(ITfsServiceClient tfsClient)
        {
            var queryId = @"ff710598-9708-4339-9496-3fc53c929398";

            Console.WriteLine(LINE_SEPARATOR);
            Console.WriteLine("Wiql query by QueryId");

            var res = tfsClient.RunSavedQuery(queryId);
            if (res != null)
            {
                Console.WriteLine($"Wiql Result empty: {res.IsEmpty}");
                Console.WriteLine($"Items count: {res.Count}");

                foreach (var item in res.GetWorkitems())
                {
                    Console.WriteLine($"{item.Id} {item["System.Title"]}");
                }
            }
            else
            {
                Console.WriteLine("WIQL result is null");
            }

            Console.WriteLine(LINE_SEPARATOR);
        }

        private static void DisplayWiqlQueryResult(ITfsServiceClient tfsClient)
        {
            var query = @"Select [System.Id], [System.Title], [System.State] From WorkItems Where [System.WorkItemType] = 'Requirement' order by [Microsoft.VSTS.Common.Priority] asc, [System.CreatedDate] desc";

            Console.WriteLine(LINE_SEPARATOR);
            Console.WriteLine("Wiql query");

            var res = tfsClient.RunWiql(query);
            if (res != null)
            {
                Console.WriteLine($"Wiql Result empty: {res.IsEmpty}");
                Console.WriteLine($"Items count: {res.Count}");

                foreach(var item in res.GetWorkitems())
                {
                    Console.WriteLine($"{item.Id} {item["System.Title"]}");
                }
            }
            else
            {
                Console.WriteLine("WIQL result is null");
            }

            Console.WriteLine(LINE_SEPARATOR);
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
