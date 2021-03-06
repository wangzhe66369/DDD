using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuthApp.Application.Common
{
    public class PagedList<T> : List<T>
    {
        public PagedList(List<T> items, int totalCount, int pageNumber, int pageSize)
        {
            TotalCount = totalCount;
            CurrentPage = pageNumber;
            PageSize = pageSize;

            TotalPages = (int)Math.Ceiling((double)totalCount / PageSize);
            AddRange(items);
        }

        public int CurrentPage { get; private set; }

        public bool HasNext => CurrentPage < TotalPages;

        public bool HasPrevious => CurrentPage > 1;

        public int PageSize { get; private set; }

        public int TotalCount { get; private set; }

        public int TotalPages { get; private set; }

        public static async Task<PagedList<T>> CreateAsync(IQueryable<T> source, int pageNumber, int pageSize)
        {
            var totalCount = source.Count();
            var items = source.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();
            var list = new PagedList<T>(items, totalCount, pageNumber, pageSize);
            return await Task.FromResult(list);
        }
    }
}
