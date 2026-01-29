using QRCoder;
using System;
using SportBookingSystem.Models.EF;
using SportBookingSystem.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace SportBookingSystem.Services
{
    public class QrService : IQrService
    {
        private readonly ApplicationDbContext _context;

        public QrService(ApplicationDbContext context)
        {
            _context = context;
        }

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

        public async Task<Transactions?> GetTransactionByOrderIdAsync(int orderId)
        {
            return await _context.Transactions
                .FirstOrDefaultAsync(t => t.OrderId == orderId);
        }
    }
}
