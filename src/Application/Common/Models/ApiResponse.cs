using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Common.Models
{
    public class PaginationMeta
    {
        public int Page { get; init; }
        public int PageSize { get; init; }
        public int TotalCount { get; init; }
        public int TotalPages => PageSize == 0 ? 0 : (int)Math.Ceiling(TotalCount / (double)PageSize);
    }

    public class ApiError
    {
        public string Code { get; init; } = string.Empty;
        public string Message { get; init; } = string.Empty;
        public IDictionary<string, string[]>? Details { get; init; }
    }

    public class ApiResponse<T>
    {
        public T? Data { get; init; }
        public PaginationMeta? Pagination { get; init; }
        public ApiError? Error { get; init; }

        public static ApiResponse<T> Ok(T data, PaginationMeta? pagination = null) =>
            new() { Data = data, Pagination = pagination };

        public static ApiResponse<T> Fail(string code, string message, IDictionary<string, string[]>? details = null) =>
            new() { Error = new ApiError { Code = code, Message = message, Details = details } };
    }
}
