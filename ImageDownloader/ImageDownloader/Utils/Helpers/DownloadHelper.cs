using HtmlAgilityPack;
using ImageDownloader.Models;
using RestSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ImageDownloader.Utils.Helpers
{
    public class DownloadHelper
    {

        public DownloadHelper()
        {

        }

        public async Task<List<DownloadResult>> DownloadImages(string sourceUrl, string destinationPath)
        {
            if (!IsValid(sourceUrl, destinationPath))
                return null;

            var client = new RestClient(sourceUrl);
            var request = new RestRequest();

            var response = await Task.Run(() => client.Execute(request));
            if (response == null)
                return null;
            var downloadList = new List<DownloadResult>();

            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(response.Content);
            var nodes = htmlDoc.DocumentNode.SelectNodes("//img[@src]");
            var imgSrcList = nodes.Select(x => x.GetAttributeValue("src", string.Empty).TrimEnd('/')).ToList();

            foreach (var imgSrc in imgSrcList)
            {
                var name = Path.GetFileName(imgSrc);
                var rgx = new Regex("[^a-zA-Z0-9 - .]");
                var newName = rgx.Replace(name, "_");

                var localFilename = string.Format("{0}\\{1}", destinationPath, newName);
                var status = "Ok";
                try
                {
                    using (var webClient = new WebClient())
                    {
                        webClient.DownloadFile(imgSrc, localFilename);
                    }
                }
                catch (Exception ex)
                {
                    status = ex.Message;
                }
                var downloadResult = new DownloadResult() { Status = status, Url = imgSrc };
                downloadList.Add(downloadResult);

            }
            return downloadList;
        }

        private static bool IsValid(string sourceUrl, string destinationPath)
        {
            if (string.IsNullOrEmpty(sourceUrl) || string.IsNullOrEmpty(destinationPath))
                return false;
            return true;
        }
    }
}
