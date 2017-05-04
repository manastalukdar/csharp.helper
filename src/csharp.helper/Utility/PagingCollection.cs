using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace csharp.helper.Utility
{
    ///-------------------------------------------------------------------------------------------------
    /// <summary>Collection of pagings.</summary>
    /// <remarks>http://www.csharpdeveloping.net/Snippet/how_to_make_paging_in_collections</remarks>
    ///
    /// <typeparam name="T">Generic type parameter.</typeparam>
    ///-------------------------------------------------------------------------------------------------

    public class PagingCollection<T> : IEnumerable<T>
    {
        #region fields

        private const int DefaultPageSize = 10;
        private readonly IEnumerable<T> _collection;
        private int _pageSize = DefaultPageSize;

        #endregion fields

        #region properties

        /// <summary>
        /// Gets pages count
        /// </summary>
        public int PagesCount => (int)Math.Ceiling(_collection.Count() / (decimal)PageSize);

        /// <summary>
        /// Gets or sets page size
        /// </summary>
        public int PageSize
        {
            get
            {
                return _pageSize;
            }
            set
            {
                if (value <= 0)
                {
                    throw new ArgumentException();
                }
                _pageSize = value;
            }
        }

        #endregion properties

        #region ctor

        /// <summary>
        /// Creates paging collection and sets page size
        /// </summary>
        public PagingCollection(IEnumerable<T> collection, int pageSize)
        {
            if (collection == null)
            {
                throw new ArgumentNullException(nameof(collection));
            }
            PageSize = pageSize;
            _collection = collection.ToArray();
        }

        /// <summary>
        /// Creates paging collection
        /// </summary>
        public PagingCollection(IEnumerable<T> collection)
            : this(collection, DefaultPageSize)
        { }

        #endregion ctor

        #region public methods

        /// <summary>
        /// Returns number of items on page by number
        /// </summary>
        public int GetCount(int pageNumber)
        {
            return GetData(pageNumber).Count();
        }

        /// <summary>
        /// Returns data by page number
        /// </summary>
        public IEnumerable<T> GetData(int pageNumber)
        {
            if (pageNumber < 0 || pageNumber > PagesCount)
            {
                return new T[] { };
            }

            var offset = (pageNumber - 1) * PageSize;

            return _collection.Skip(offset).Take(PageSize);
        }

        #endregion public methods

        #region static methods

        /// <summary>
        /// Returns data by page number and page size
        /// </summary>
        public static IEnumerable<T> GetPaging(IEnumerable<T> collection, int pageNumber, int pageSize)
        {
            return new PagingCollection<T>(collection, pageSize).GetData(pageNumber);
        }

        /// <summary>
        /// Returns data by page number
        /// </summary>
        public static IEnumerable<T> GetPaging(IEnumerable<T> collection, int pageNumber)
        {
            return new PagingCollection<T>(collection, DefaultPageSize).GetData(pageNumber);
        }

        #endregion static methods

        #region IEnumerable<T> Members

        /// <summary>
        /// Returns an enumerator that iterates through collection
        /// </summary>
        public IEnumerator<T> GetEnumerator()
        {
            return _collection.GetEnumerator();
        }

        #endregion IEnumerable<T> Members

        #region IEnumerable Members

        /// <summary>
        /// Returns an enumerator that iterates through collection
        /// </summary>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion IEnumerable Members
    }
}