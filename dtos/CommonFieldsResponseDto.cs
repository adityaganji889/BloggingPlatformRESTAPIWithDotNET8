namespace BloggingPlatform.dtos
{

    public class CommonFieldsResponseDto<T>
    {

        public bool Success { get; set; }
        public string Message { get; set; }

        public T? Response { get; set; }

        public IEnumerable<T>? ResponseList { get; set; }

        // Constructor
        public CommonFieldsResponseDto()
        {
            if (Message == null)
            {
                Message = "";
            }
        }

        // Method to create a filtered response with only non-null values
        public IDictionary<string, object?> GetFilteredResponse()
        {
            var result = new Dictionary<string, object?>
        {
            { nameof(Success), Success },
            { nameof(Message), Message },
            { nameof(Response), Response },
            { nameof(ResponseList), ResponseList }
        };

            // Remove entries where the value is null
            return result.Where(kv => kv.Value is not null).ToDictionary(kv => kv.Key, kv => kv.Value);
        }

        // Optional: Method to ensure that only one of the properties is set
        public bool IsValid()
        {
            return (Response != null && ResponseList == null) ||
                   (Response == null && ResponseList != null);
        }

    }
}