namespace Sol_Demo.DTO.Requests
{
    public class CreateUsersRequestDTO
    {
        public string? FirstName { get; set; }

        public string? LastName { get; set; }

        public CreateUserCommunicationRequestDTO? Communication { get; set; }
    }

    public class CreateUserCommunicationRequestDTO
    {
        public string? MobileNo { get; set; }

        public string? EmailId { get; set; }
    }
}