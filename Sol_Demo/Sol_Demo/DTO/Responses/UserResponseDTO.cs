namespace Sol_Demo.DTO.Responses
{
    public class UserResponseDTO
    {
        public decimal Id { get; set; }

        public string? FirstName { get; set; }

        public string? LastName { get; set; }

        public UserCommunicationResponseDTO? Communication { get; set; }
    }

    public class UserCommunicationResponseDTO
    {
        public decimal Id { get; set; }

        public string? MobileNo { get; set; }

        public string? EmailId { get; set; }
    }
}