using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using WebCrawler.Domain.DTO;
using WebCrawler.Domain.Interfaces.Repositories;
using WebCrawler.Domain.Models;
using WebCrawler.Infra.Data;

namespace WebCrawler.Infra.Repositories
{
    public class PageRepository : IPageRepository
    {
        private readonly WebCrawlerDbContext _context;
        public PageRepository(WebCrawlerDbContext context) => _context = context;

        public async Task<Page?> SavePageAsync(PageDTO page)
        {
            if (await _context.Pages.AnyAsync(p => p.Url == page.Url))
                return null;

            var pageEntity = new Page
            {
                Url = page.Url,
                Title = page.Title,
                Content = page.Content
            };

            _context.Pages.Add(pageEntity);

            await _context.SaveChangesAsync();
            return pageEntity;
        }


        public async Task<bool> IsPageAlreadyVisited(string url)
        {
            return await _context.Pages.AnyAsync(p => p.Url == url);
        }
        
        public async Task<List<PageDTO>> GetAllPagesAsync()
        {
            return await _context.Pages
                .Select(p => new PageDTO { Url = p.Url, Title = p.Title, Content = p.Content })
                .ToListAsync();
        }

        public async Task<List<PageListDTO>> GetPagesListAsync()
        {
            return await _context.Pages
                .Select(p => new PageListDTO { Id = p.Id, Url = p.Url, Title = p.Title })
                .ToListAsync();
        }

        public async Task<PageDTO?> GetPageByIdAsync(long id)
        {
            return await _context.Pages
                .Where(p => p.Id == id)
                .Select(p => new PageDTO { Url = p.Url, Title = p.Title, Content = p.Content })
                .FirstOrDefaultAsync();
        }
    }
}
