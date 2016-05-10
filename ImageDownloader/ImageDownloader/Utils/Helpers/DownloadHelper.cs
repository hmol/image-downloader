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

            var response = await ExecuteRequest(sourceUrl);
            var imgSrcList = GetImageTagsSrc(response);
            var downloadList = new List<DownloadResult>();

            foreach (var imgSrc in imgSrcList)
            {
                var downloadResult = DownloadImageFile(imgSrc, destinationPath);
                downloadList.Add(downloadResult);
            }
            return downloadList;
        }

        public async Task<IRestResponse> ExecuteRequest(string url)
        {
            var client = new RestClient(url);
            var request = new RestRequest();

            return await Task.Run(() => client.Execute(request));
        }

        public List<string> GetImageTagsSrc(IRestResponse restResponse)
        {
            var content = restResponse.Content;
            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(content);
            var nodes = htmlDoc.DocumentNode.SelectNodes("//img[@src]");
            var imgSrcList = nodes.Select(x => x.GetAttributeValue("src", string.Empty).TrimEnd('/')).ToList();
            return imgSrcList;
        }

        public DownloadResult DownloadImageFile(string imgSrc, string destinationPath)
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
            return new DownloadResult() { Status = status, Url = imgSrc };
        }

        private static bool IsValid(string sourceUrl, string destinationPath)
        {
            if (string.IsNullOrEmpty(sourceUrl) || string.IsNullOrEmpty(destinationPath))
                return false;
            return true;
        }
    }
}
