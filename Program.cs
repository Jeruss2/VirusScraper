using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;
using CoronavirusModels;
using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Virus.Data.Layer;

namespace VirusScraper
{
    internal class Program
    {
        private static void Main(string[] args)
        {


            VirusDAL virusDal = new VirusDAL();

            try
            {
                string url = "https://www.worldometers.info/coronavirus/";

                Task<IHtmlDocument> documentTask = GetHtmlDocument(url);

                documentTask.Wait();

                var document = documentTask.Result;

                CaseBlobs caseBlobs = GetCasesFromHtmlDoc(document);

                virusDal.SaveDataCaseBlob(caseBlobs);

            }
            catch (Exception e)
            {
                var error = new VirusErrorLog();

                error.ApplicationName = "VirusScraper";
                error.ExceptionMessage = e.Message;
                error.StackTrace = e.StackTrace;
                error.DateAdded = DateTime.Now;

                virusDal.SaveErrorLog(error);


            }



        }

        private static async Task<IHtmlDocument> GetHtmlDocument(string url)
        {
            CancellationTokenSource cancellationToken = new CancellationTokenSource();
            HttpClient httpClient = new HttpClient();
            HttpResponseMessage request = await httpClient.GetAsync(url);
            cancellationToken.Token.ThrowIfCancellationRequested();

            Stream response = await request.Content.ReadAsStreamAsync();
            cancellationToken.Token.ThrowIfCancellationRequested();

            HtmlParser parser = new HtmlParser();
            IHtmlDocument document = parser.ParseDocument(response);

            return document;
        }

        private static CaseBlobs GetCasesFromHtmlDoc(IHtmlDocument document)
        {
            CaseBlobs c = new CaseBlobs();

            var stats = document.GetElementsByClassName("maincounter-number");


            c.DateScraped = DateTime.Now;
            c.TotalCasesBlob = stats[0].InnerHtml;
            c.TotalDeathsBlob = stats[1].InnerHtml;
            c.TotalRecoveriesBlob = stats[2].InnerHtml;
            c.Processed = false;


            return c;
        }
    }
}