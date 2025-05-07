namespace EBISX_POS.API.Services.DTO.FetchEBISX
{
    public class FetchSaleTypeDTO: FetchResponseDTO<SaleTypeInfo> { }
    public class SaleTypeInfo
    {
        public required string TransNo { get; set; }
        public required string TenderName { get; set; }
        public required string Account { get; set; }
        public required string SaleType { get; set; }
    }
}
