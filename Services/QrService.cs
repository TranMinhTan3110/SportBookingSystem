using QRCoder;
using System;

namespace SportBookingSystem.Services
{
    public class QrService : IQrService
    {
        public string GenerateQrCode(string data)
        {
            using (QRCodeGenerator qrGenerator = new QRCodeGenerator())
            using (QRCodeData qrCodeData = qrGenerator.CreateQrCode(data, QRCodeGenerator.ECCLevel.Q))
            using (PngByteQRCode qrCode = new PngByteQRCode(qrCodeData))
            {
                byte[] qrCodeAsPngByteArr = qrCode.GetGraphic(20);
                return Convert.ToBase64String(qrCodeAsPngByteArr);
            }
        }
    }
}
