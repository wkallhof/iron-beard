using Raud.Core.Extensions;

namespace Raud.Core.Features.Shared
{
    public class ServiceResult
    {
        public string Error { get; set; }
        public bool Success => !this.Error.IsSet();
        public ServiceResult() { }
        public ServiceResult(string error) { this.Error = error; }
    }

    public class ServiceResult<T> : ServiceResult
    {
        public T Data { get; set; }
        public ServiceResult() : base() { }
        public ServiceResult(string error) : base(error) { }
    }
}