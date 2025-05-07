namespace EBISX_POS.API.Services.DTO.Order
{
    public class FinalizeOrderResponseDTO
    {
        public required string InvoiceNumber { get; set; }
        public required string PosSerialNumber { get; set; } // serial number of the POS machine.
        public required string MinNumber { get; set; } // MIN (Machine Identifier Number) of the POS machine.
        public required string AccreditationNumber { get; set; } // accreditation number of the POS machine.
        public required string PtuNumber { get; set; } // PTU (Point of Transaction Unit) number of the POS machine.
        public required string DateIssued { get; set; } // date when the machine was issued.
        public required string ValidUntil { get; set; } // date until the machine is valid.

        // Business details
        public required string RegisteredName { get; set; } // registered name of the business.
        public required string Address { get; set; } // address of the business.
        public required string VatTinNumber { get; set; } // VAT (Value Added Tax) TIN (Tax Identification Number) of the business.

        public required string InvoiceDate { get; set; }
        public required bool IsTrainMode { get; set; }
    }
}
