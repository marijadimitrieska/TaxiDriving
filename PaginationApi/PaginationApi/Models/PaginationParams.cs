namespace PaginationApi.Models
{
    public class PaginationParams
    {
        public int PageNumber {  get; set; }

        private int _pageSize;
        private readonly int _maxPageSize;

        public PaginationParams(int defaultPageSize, int maxPageSize)
        {
            PageNumber = 1;
            _pageSize = defaultPageSize;
            _maxPageSize = maxPageSize;
        }

        public int PageSize
        {
            get => _pageSize;
            set => _pageSize = (value > _maxPageSize) ? _maxPageSize : value;
        }
    }
}
