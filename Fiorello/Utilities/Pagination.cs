using Fiorello.DAL;
using Fiorello.Models;
using Microsoft.EntityFrameworkCore;

namespace Fiorello.Utilities
{
    public static class Pagination
    {
        public static async Task<List<T>> PaginateAsync<T>(IQueryable<T> source, int currentPage, int take)
        {
            return await source.Skip((currentPage - 1) * take).Take(take).ToListAsync();
        }

        public static int GetTotalPage(int totalCount, int take)
        {
            return (int)Math.Ceiling((decimal)totalCount / take);
        }
    }
}
