using Microsoft.EntityFrameworkCore;

namespace Assig1.Helpers
{
    public class PaginatedList<T> : List<T>
    {
        public int PageIndex { get; private set; }
        public int TotalPages { get; private set; }

        public PaginatedList (List<T> Items, int Count, int PageIndex, int PageSize)
        {
            this.PageIndex = PageIndex;
            TotalPages = (int)Math.Ceiling(Count / (double)PageSize);

            this.AddRange(Items);
        }

        public bool HasPreviousPage
        {
            get
            {
                return (PageIndex > 1);
            }
        }

        public bool HasNextPage
        {
            get
            {
                return (PageIndex < TotalPages);
            }
        }

        public static async Task<PaginatedList<T>> CreateAsync(IQueryable<T> Source, int PageIndex, int PageSize)
        {
            var Count = await Source.CountAsync();
            var Items = await Source.Skip((PageIndex - 1) * PageSize).Take(PageSize).ToListAsync();
            return new PaginatedList<T>(Items, Count, PageIndex, PageSize);

        }
    }
}
