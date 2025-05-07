namespace EBISX_POS.API.Services.DTO.FetchEBISX
{
    public class FetchUsersDTO : FetchResponseDTO<UserDto> { }

    public class UserDto
    {
        public int UserId { get; set; }
        public required string Username { get; set; }
        public required string Firstname { get; set; }
        public required string Lastname { get; set; }
        public required string Email { get; set; }
        public bool Active { get; set; }
        public required string UserPosition { get; set; }
        public required string UserPass { get; set; }
    }
}
