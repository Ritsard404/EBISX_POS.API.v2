namespace EBISX_POS.API.Services.DTO.FetchEBISX
{
    public class FetchResponseDTO<T>
    {
        public required List<T> AllUsers { get; set; }
    }
}
