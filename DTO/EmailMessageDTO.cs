namespace SportBookingSystem.DTO{
public class EmailMessageDTO{
    public string ToEmail{get;set;} = string.Empty;
    public string BookingCode{get;set;} = string.Empty;
    public string CustomerName{get;set;} = string.Empty;
    public string PitchName {get;set;} = string.Empty;
    public string TotalAmount {get;set;} = string.Empty;
    public string QrCodeBase64 {get;set;} = string.Empty;
}

}