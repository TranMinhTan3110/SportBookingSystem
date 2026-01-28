using Microsoft.EntityFrameworkCore;
using QRCoder;
using SportBookingSystem.Models.EF;
using SportBookingSystem.Models.Entities;
using SportBookingSystem.Models.ViewModels;
using System.Drawing;
using System.Drawing.Imaging;

namespace SportBookingSystem.Services
{
    public class BookingService : IBookingService
    {
        private readonly ApplicationDbContext _context;

        public BookingService(ApplicationDbContext context)
        {
            _context = context;
        }

        // CÁC HÀM LẤY DANH SÁCH SÂN

        public async Task<FilterPitchesResponse> GetAvailablePitchesAsync(
            DateTime date,
            List<int>? slotIds,
            List<int>? categoryIds,
            List<int>? specificPitchIds,
            List<int>? capacities)
        {
            var allPitches = await _context.Pitches
                .Include(p => p.Category)
                .ToListAsync();

            var priceSettings = await _context.PitchPriceSettings.ToListAsync();

            var pitches = allPitches
                .Where(p => p.Status != null && p.Status.Trim() == "Sẵn sàng")
                .ToList();

            if (categoryIds != null && categoryIds.Any())
            {
                pitches = pitches.Where(p => categoryIds.Contains(p.CategoryId)).ToList();
            }

            if (specificPitchIds != null && specificPitchIds.Any())
            {
                pitches = pitches.Where(p => specificPitchIds.Contains(p.PitchId)).ToList();
            }

            // Lọc theo sức chứa
            if (capacities != null && capacities.Any())
            {
                pitches = pitches.Where(p => capacities.Contains(p.Capacity)).ToList();
            }

            var timeSlotsQuery = _context.TimeSlots
                .Where(ts => ts.IsActive)
                .OrderBy(ts => ts.StartTime)
                .AsQueryable();

            if (slotIds != null && slotIds.Any())
            {
                timeSlotsQuery = timeSlotsQuery.Where(ts => slotIds.Contains(ts.SlotId));
            }

            var timeSlots = await timeSlotsQuery.ToListAsync();

            var bookedSlots = await _context.PitchSlots
                .Where(ps => ps.PlayDate.Date == date.Date)
                .ToListAsync();

            var groupedResult = new List<PitchGroupViewModel>();

            foreach (var pitch in pitches)
            {
                if (pitch.Status == "Bảo trì") continue;

                var pitchGroup = new PitchGroupViewModel
                {
                    PitchId = pitch.PitchId,
                    PitchName = pitch.PitchName,
                    CategoryName = pitch.Category?.CategoryName ?? "Sân bóng",
                    Capacity = pitch.Capacity,
                    ImageUrl = pitch.ImageUrl ?? "/asset/img/default-pitch.jpg",
                    Description = pitch.Description ?? "",
                    PricePerHour = pitch.PricePerHour,
                    Slots = new List<SlotInfoViewModel>()
                };

                var currentPitchSettings = priceSettings.Where(s => s.PitchId == pitch.PitchId).ToList();

                foreach (var timeSlot in timeSlots)
                {
                    decimal currentPricePerHour = pitch.PricePerHour;
                    var specialPrice = currentPitchSettings.FirstOrDefault(s =>
                        timeSlot.StartTime >= s.StartTime && timeSlot.StartTime < s.EndTime);

                    if (specialPrice != null)
                    {
                        currentPricePerHour = specialPrice.Price;
                    }

                    var duration = (timeSlot.EndTime - timeSlot.StartTime).TotalHours;
                    decimal fullPrice = currentPricePerHour * (decimal)duration;
                    decimal depositPrice = fullPrice * 0.3m;

                    bool isAvailable = true;
                    string status = "available";
                    string statusText = "Còn trống";

                    var bookingInfo = bookedSlots.FirstOrDefault(ps =>
                        ps.PitchId == pitch.PitchId && ps.SlotId == timeSlot.SlotId);

                    if (bookingInfo != null && bookingInfo.Status == 1)
                    {
                        status = "booked";
                        statusText = "Đã đặt";
                        isAvailable = false;
                    }
                    //xét khi thời gian đã qua 
                    DateTime slotEndTime = date.Date.Add(timeSlot.EndTime);

                    // So sánh với thời gian hiện tại
                    if (slotEndTime < DateTime.Now)
                    {
                        
                        if (status != "booked")
                        {
                            status = "expired"; 
                            statusText = "Đã qua";
                            isAvailable = false; 
                        }
                    }

                    var slotModel = new SlotInfoViewModel
                    {
                        SlotId = timeSlot.SlotId,
                        SlotName = timeSlot.SlotName,
                        TimeRange = $"{timeSlot.StartTime:hh\\:mm} - {timeSlot.EndTime:hh\\:mm}",
                        FullPrice = fullPrice,
                        DepositPrice = depositPrice,
                        Status = status,
                        StatusText = statusText,
                        IsAvailable = isAvailable
                    };

                    pitchGroup.Slots.Add(slotModel);
                }

                if (pitchGroup.Slots.Any())
                {
                    groupedResult.Add(pitchGroup);
                }
            }

            return new FilterPitchesResponse
            {
                Pitches = groupedResult,
                TotalCount = pitches.Count,
                DisplayCount = groupedResult.Count
            };
        }

        public async Task<FilterPitchesResponse> GetFilteredPitchesAsync(
            DateTime date,
            List<int>? slotIds,
            List<int>? categoryIds,
            List<int>? specificPitchIds,
            List<int>? capacities,
            List<string>? statusFilter)
        {
            var response = await GetAvailablePitchesAsync(date, slotIds, categoryIds, specificPitchIds, capacities);

            if (statusFilter != null && statusFilter.Any())
            {
                response.Pitches = response.Pitches
                    .Where(p => p.Slots.Any(s => statusFilter.Contains(s.Status)))
                    .ToList();
            }

            response.DisplayCount = response.Pitches.Count;
            return response;
        }

        public async Task<FilterPitchesResponse> GetFilteredPitchesWithPaginationAsync(
            DateTime date,
            List<int>? slotIds,
            List<int>? categoryIds,
            List<int>? specificPitchIds,
            List<int>? capacities,
            List<string>? statusFilter,
            int page,
            int pageSize)
        {
            var response = await GetFilteredPitchesAsync(date, slotIds, categoryIds, specificPitchIds, capacities, statusFilter);

            var totalItems = response.Pitches.Count;
            var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            response.Pitches = response.Pitches
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            response.TotalPages = totalPages;
            response.CurrentPage = page;
            response.DisplayCount = response.Pitches.Count;

            return response;
        }

        // --- HÀM ĐẶT SÂN DUY NHẤT ---

        public async Task<(bool Success, string Message, string? QrBase64, string? BookingCode)> BookPitchAsync(int userId, int pitchId, int slotId, DateTime date)
        {
            using var transaction = _context.Database.BeginTransaction();
            try
            {
                // 1. Kiểm tra Slot có trống không
                var slot = await _context.PitchSlots
                    .FirstOrDefaultAsync(ps => ps.PitchId == pitchId && ps.SlotId == slotId && ps.PlayDate.Date == date.Date);

                if (slot != null && slot.Status == 1)
                {
                    return (false, "Sân này đã có người đặt rồi!", null, null);
                }

                // 2. Tính tiền
                var pitch = await _context.Pitches.FindAsync(pitchId);
                decimal amount = pitch.PricePerHour * 1.5m; 

                // 3. Kiểm tra ví
                var user = await _context.Users.FindAsync(userId);
                if (user.WalletBalance < amount)
                {
                    return (false, "Số dư không đủ. Vui lòng nạp thêm!", null, null);
                }

                // 4. TRỪ TIỀN VÍ
                user.WalletBalance -= amount;
                _context.Users.Update(user);

                // 5. Tạo Mã Booking
                string bookingCode = $"BKG-{DateTime.Now:yyMMdd}-{Guid.NewGuid().ToString().Substring(0, 4).ToUpper()}";

                // 6. Cập nhật PitchSlot
                if (slot == null)
                {
                    slot = new PitchSlots
                    {
                        PitchId = pitchId,
                        SlotId = slotId,
                        PlayDate = date,
                        Status = 1,
                        BookingCode = bookingCode
                    };
                    _context.PitchSlots.Add(slot);
                }
                else
                {
                    slot.Status = 1;
                    slot.BookingCode = bookingCode;
                    _context.PitchSlots.Update(slot);
                }

                // 7. Lưu Giao Dịch
                var trans = new Transactions
                {
                    UserId = userId,
                    Amount = -amount, 
                    TransactionType = "Thanh toán Booking",
                    Status = "Thành công",
                    Source = "Ví nội bộ",
                    TransactionDate = DateTime.Now,
                    TransactionCode = bookingCode,
                    Message = $"Đặt sân {pitch.PitchName} - {date:dd/MM/yyyy}"
                };
                _context.Transactions.Add(trans);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                // 8. Tạo QR
                string qrContent = $"BOOKING:{bookingCode}";
                string qrBase64 = GenerateQrCode(qrContent);

                return (true, "Đặt sân thành công!", qrBase64, bookingCode);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return (false, "Lỗi: " + ex.Message, null, null);
            }
        }

        // hàm gen mã qr

        private string GenerateQrCode(string content)
        {
            using (QRCodeGenerator qrGenerator = new QRCodeGenerator())
            using (QRCodeData qrCodeData = qrGenerator.CreateQrCode(content, QRCodeGenerator.ECCLevel.Q))
            using (PngByteQRCode qrCode = new PngByteQRCode(qrCodeData))
            {
                byte[] qrCodeBytes = qrCode.GetGraphic(20);
                return "data:image/png;base64," + Convert.ToBase64String(qrCodeBytes);
            }
        }
    }
}