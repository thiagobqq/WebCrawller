using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using HtmlAgilityPack;
using Microsoft.VisualBasic;
using WebCrawler.Domain.DTO;

namespace WebCrawler.Application.Manager
{
    public class CrawlerManager
    {
        
        private readonly SpiderManager _spiderManager;
        
        public CrawlerManager (SpiderManager spiderManager) => _spiderManager = spiderManager;
        public async Task<PageDTO?> ProcessPage(string url)
        {
            var web = new HtmlWeb();
            try
            {
                var htmlDoc = await web.LoadFromWebAsync(url);
                var node = htmlDoc.DocumentNode.SelectSingleNode("//head/title");
                var nodes = htmlDoc.DocumentNode.SelectNodes("//a[@href]");

                var content = Regex.Replace(htmlDoc.DocumentNode.InnerText.Trim(), @"\s+", " ");
                var urls = await ExtractUrls(nodes);
                return new PageDTO
                {
                    Url = url,
                    Title = node != null ? node.InnerText : string.Empty,
                    Content = content
                };
            }
            catch
            {
                return null;
            }
        }


        public async Task <List<string>> ExtractUrls(HtmlNodeCollection nodes)
        {            
            var urls = new List<string>();
            if (nodes != null)
            {
                foreach (var node in nodes)
                {
                    var hrefValue = node.GetAttributeValue("href", string.Empty);
                    if (!string.IsNullOrEmpty(hrefValue) && Uri.IsWellFormedUriString(hrefValue, UriKind.Absolute))
                    {
                        urls.Add(hrefValue);
                        _spiderManager.EnqueueUrl(hrefValue);
                    }
                }
            }

            return urls;
        }
        
    }
}