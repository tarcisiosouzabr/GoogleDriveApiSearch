using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoleTestes
{
    class Program
    {
        // If modifying these scopes, delete your previously saved credentials
        // at ~/.credentials/drive-dotnet-quickstart.json
        static string[] Scopes = { DriveService.Scope.DriveReadonly };
        static string ApplicationName = "Drive API .NET Quickstart";

        static void Main(string[] args)
        {
            UserCredential credential;

            using (var stream =
                new FileStream("credentials.json", FileMode.Open, FileAccess.Read))
            {
                // The file token.json stores the user's access and refresh tokens, and is created
                // automatically when the authorization flow completes for the first time.
                string credPath = "cred.json";
                credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.Load(stream).Secrets,
                    Scopes,
                    "user",
                    CancellationToken.None,
                    new FileDataStore(credPath, true)).Result;
            }
            Console.WriteLine("Digite o termo de consulta:");
            var searchTerms = Console.ReadLine();

            // Create Drive API service.
            var service = new DriveService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = ApplicationName,
            });

            // Define parameters of request.
            FilesResource.ListRequest listRequest = service.Files.List();
            listRequest.PageSize = 10;
            listRequest.Q = $"fullText contains '{searchTerms}' and mimeType='application/vnd.google-apps.document'";
            listRequest.Fields = "nextPageToken, files(id, name,parents,mimeType,description)";

            // List files.
            IList<Google.Apis.Drive.v3.Data.File> files = listRequest.Execute()
                .Files;
            Console.WriteLine("Files:");
            if (files != null && files.Count > 0)
            {
                foreach (var file in files)
                {
                    FilesResource.ExportRequest exportRequest = service.Files.Export(file.Id, "text/plain");
                    var s = exportRequest.ExecuteAsStream();
                    StreamReader reader = new StreamReader(s);
                    string text = reader.ReadToEnd();
                    var contentWhereFound = text.IndexOf(searchTerms);
                    var resultText = text.Substring(contentWhereFound, 100);
                    Console.WriteLine("{0} - Trecho encontrado : {1}", file.Name, resultText);
                }
                Console.WriteLine("---");
                Console.WriteLine("Consulta finalizada.");
            }
            else
            {
                Console.WriteLine("No files found.");
            }
            Console.Read();

        }
    }
}
