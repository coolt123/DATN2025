    namespace DATN.Dtos
{
    public class ResponseDto<T>
    {
        public int statusCode { get; set; }
        public string Message { get; set; }
        public T Data { get; set; }
        public object? Meta {  get; set; }
    }
}
