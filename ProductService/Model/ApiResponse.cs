namespace ProductService.Model
{
    public class ApiResponse<T>
    {
        public string Status { get; set; }
        public T Data { get; set; }
        public string Message { get; set; }
    }
}
