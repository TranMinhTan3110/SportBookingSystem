using Microsoft.EntityFrameworkCore;
using QRCoder;
using SportBookingSystem.Constants;
using SportBookingSystem.Models.EF;
using SportBookingSystem.Models.DTOs;
using SportBookingSystem.Models.Entities;
using SportBookingSystem.Models.ViewModels;

namespace SportBookingSystem.Services
{
    public class BookingService : IBookingService
    {
        private readonly ApplicationDbContext _context;

        public BookingService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<FilterPitchesResponse> GetAvailablePitchesAsync(DateTime date, List<int>? slotIds, List<int>? categoryIds, List<int>? specificPitchIds, List<int>? capacities)
        {
            var allPitches = await _context.Pitches.Include(p => p.Category).ToListAsync();
            var priceSettings = await _context.PitchPriceSettings.ToListAsync();
            var pitches = allPitches.Where(p => p.Status != null && p.Status.Trim() == "Sẵn sàng").ToList();

            if (categoryIds != null && categoryIds.Any()) pitches = pitches.Where(p => categoryIds.Contains(p.CategoryId)).ToList();
            if (specificPitchIds != null && specificPitchIds.Any()) pitches = pitches.Where(p => specificPitchIds.Contains(p.PitchId)).ToList();
            if (capacities != null && capacities.Any()) pitches = pitches.Where(p => capacities.Contains(p.Capacity)).ToList();

            var timeSlotsQuery = _context.TimeSlots.Where(ts => ts.IsActive).OrderBy(ts => ts.StartTime).AsQueryable();
            if (slotIds != null && slotIds.Any()) timeSlotsQuery = timeSlotsQuery.Where(ts => slotIds.Contains(ts.SlotId));

            var timeSlots = await timeSlotsQuery.ToListAsync();
            var bookedSlots = await _context.PitchSlots.Where(ps => ps.PlayDate.Date == date.Date).ToListAsync();

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
                    var specialPrice = currentPitchSettings.FirstOrDefault(s => timeSlot.StartTime >= s.StartTime && timeSlot.StartTime < s.EndTime);
                    if (specialPrice != null) currentPricePerHour = specialPrice.Price;

                    var duration = (timeSlot.EndTime - timeSlot.StartTime).TotalHours;
                    decimal fullPrice = currentPricePerHour * (decimal)duration;
                    decimal depositPrice = fullPrice * 0.3m;

                    bool isAvailable = true;
                    string status = "available";
                    string statusText = "Còn trống";

                    var bookingInfo = bookedSlots.FirstOrDefault(ps => ps.PitchId == pitch.PitchId && ps.SlotId == timeSlot.SlotId);
                    if (bookingInfo != null && bookingInfo.Status >= 1)
                    {
                        status = "booked";
                        statusText = "Đã đặt";
                        isAvailable = false;
                    }

                    DateTime slotStartTime = date.Date.Add(timeSlot.StartTime);
                    DateTime slotEndTime = date.Date.Add(timeSlot.EndTime);
                    DateTime now = DateTime.Now; 

                    if (status != "booked")
                    {
                        double minutesPassed = (now - slotStartTime).TotalMinutes;

                        if (now >= slotEndTime) 
                        {
                            status = "expired";
                            statusText = "Đã qua";
                            isAvailable = false;
                        }
                        else if (now >= slotStartTime) 
                        {
                            if (minutesPassed > 30)
                            {
                                status = "expired";
                                statusText = "Đã qua";
                                isAvailable = false;
                            }
                            else if (minutesPassed > 15) 
                            {
                                fullPrice = fullPrice * 0.75m;
                                depositPrice = fullPrice * 0.3m;
                                statusText = "Muộn -25%";
                            }
                        }
                    }

                    pitchGroup.Slots.Add(new SlotInfoViewModel
                    {
                        SlotId = timeSlot.SlotId,
                        SlotName = timeSlot.SlotName,
                        TimeRange = $"{timeSlot.StartTime:hh\\:mm} - {timeSlot.EndTime:hh\\:mm}",
                        FullPrice = fullPrice,
                        DepositPrice = depositPrice,
                        Status = status,
                        StatusText = statusText,
                        IsAvailable = isAvailable
                    });
                }

                if (pitchGroup.Slots.Any()) groupedResult.Add(pitchGroup);
            }

            return new FilterPitchesResponse { Pitches = groupedResult, TotalCount = pitches.Count, DisplayCount = groupedResult.Count };
        }

        public async Task<FilterPitchesResponse> GetFilteredPitchesAsync(DateTime date, List<int>? slotIds, List<int>? categoryIds, List<int>? specificPitchIds, List<int>? capacities, List<string>? statusFilter)
        {
            var response = await GetAvailablePitchesAsync(date, slotIds, categoryIds, specificPitchIds, capacities);
            if (statusFilter != null && statusFilter.Any()) response.Pitches = response.Pitches.Where(p => p.Slots.Any(s => statusFilter.Contains(s.Status))).ToList();
            response.DisplayCount = response.Pitches.Count;
            return response;
        }

        public async Task<FilterPitchesResponse> GetFilteredPitchesWithPaginationAsync(DateTime date, List<int>? slotIds, List<int>? categoryIds, List<int>? specificPitchIds, List<int>? capacities, List<string>? statusFilter, int page, int pageSize)
        {
            var response = await GetFilteredPitchesAsync(date, slotIds, categoryIds, specificPitchIds, capacities, statusFilter);
            var totalItems = response.Pitches.Count;
            var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            response.Pitches = response.Pitches.Skip((page - 1) * pageSize).Take(pageSize).ToList();
            response.TotalPages = totalPages;
            response.CurrentPage = page;
            response.DisplayCount = response.Pitches.Count;
            return response;
        }

        public async Task<(bool Success, string Message, string? QrBase64, string? BookingCode)> BookPitchAsync(int userId, int pitchId, int slotId, DateTime date)
        {
            using var transaction = _context.Database.BeginTransaction();
            try
            {
                // Tạo Lock (Giữ nguyên logic lock của bạn để tránh đặt trùng)
                string lockResource = $"Booking_Lock_{pitchId}_{slotId}_{date:yyyyMMdd}";
                await _context.Database.ExecuteSqlRawAsync($@"
        DECLARE @res INT;
        EXEC @res = sp_getapplock @Resource = '{lockResource}', @LockMode = 'Exclusive', @LockOwner = 'Transaction', @LockTimeout = 5000;
        IF @res < 0 THROW 50000, 'Không thể lấy khóa đặt sân', 1;
    ");

                // 2. Kiểm tra trạng thái sân trong DB
                var slot = await _context.PitchSlots.FirstOrDefaultAsync(ps => ps.PitchId == pitchId && ps.SlotId == slotId && ps.PlayDate.Date == date.Date);
                if (slot != null && slot.Status >= 1) return (false, "Sân này vừa được người khác đặt rồi!", null, null);

                var pitch = await _context.Pitches.FindAsync(pitchId);
                var timeSlot = await _context.TimeSlots.FindAsync(slotId);

                //  LOGIC THỜI GIAN VÀ GIÁ 
                DateTime slotStartTime = date.Date.Add(timeSlot.StartTime);
                DateTime slotEndTime = date.Date.Add(timeSlot.EndTime);
                DateTime now = DateTime.Now; 

                // Tính giá gốc
                decimal currentPricePerHour = pitch.PricePerHour;
                var specialPrice = await _context.PitchPriceSettings
                    .FirstOrDefaultAsync(s => s.PitchId == pitchId && timeSlot.StartTime >= s.StartTime && timeSlot.StartTime < s.EndTime);
                if (specialPrice != null) currentPricePerHour = specialPrice.Price;

                var duration = (timeSlot.EndTime - timeSlot.StartTime).TotalHours;
                decimal amount = currentPricePerHour * (decimal)duration;

                // Chặn đặt sân nếu đã quá giờ
                if (now >= slotEndTime) return (false, "Ca đá này đã kết thúc, không thể đặt!", null, null);

                string discountNote = "";
                if (now >= slotStartTime)
                {
                    double minutesPassed = (now - slotStartTime).TotalMinutes;
                    if (minutesPassed > 30) return (false, "Đã quá 30 phút từ khi bắt đầu ca, hệ thống đã khóa đặt sân!", null, null);

                    if (minutesPassed > 15)
                    {
                        amount = amount * 0.75m; // Giảm giá 25% vào Transaction
                        discountNote = " (Giảm 25% vào muộn)";
                    }
                }

                // Kiểm tra ví và trừ tiền (Dùng amount đã tính toán lại)
                var user = await _context.Users.FindAsync(userId);
                if (user.WalletBalance < amount) return (false, "Số dư không đủ. Vui lòng nạp thêm!", null, null);

                user.WalletBalance -= amount;
                _context.Users.Update(user);

                //  Lưu thông tin đặt sân và Giao dịch
                string bookingCode = $"BKG-{DateTime.Now:yyMMddHHmmss}{userId}";

                if (slot == null)
                {
                    slot = new PitchSlots { PitchId = pitchId, SlotId = slotId, PlayDate = date, Status = BookingStatus.PendingConfirm, BookingCode = bookingCode, UserId = userId };
                    _context.PitchSlots.Add(slot);
                }
                else
                {
                    slot.Status = BookingStatus.PendingConfirm; slot.BookingCode = bookingCode; slot.UserId = userId;
                    _context.PitchSlots.Update(slot);
                }

                var trans = new Transactions
                {
                    UserId = userId,
                    Amount = amount, // Lưu số tiền thực tế đã trừ
                    TransactionType = TransactionTypes.Booking,
                    Status = TransactionStatus.PendingConfirm,
                    Source = TransactionSources.Wallet,
                    TransactionDate = DateTime.Now,
                    TransactionCode = bookingCode,
                    Message = $"Đặt sân {pitch.PitchName} - {date:dd/MM/yyyy}{discountNote}",
                    BalanceAfter = user.WalletBalance
                };
                _context.Transactions.Add(trans);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

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

        public async Task<(bool Success, string Message, object? Data)> CheckInBookingAsync(string bookingCode)
        {
            var slot = await _context.PitchSlots
                .Include(p => p.Pitch)
                .Include(p => p.TimeSlot)
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.BookingCode == bookingCode);

            if (slot == null) return (false, "Mã đặt sân không tồn tại!", null);

            if (slot.Status == BookingStatus.CheckedIn || slot.Status == BookingStatus.Completed)
            {
                return (false, $"Vé này đã được sử dụng lúc {slot.UpdatedAt:HH:mm dd/MM}!", null);
            }

            DateTime matchStartTime = slot.PlayDate.Date.Add(slot.TimeSlot.StartTime);
            DateTime now = DateTime.Now;
            DateTime allowedCheckInTime = matchStartTime.AddMinutes(-30);

            if (now < allowedCheckInTime)
            {
                var minutesWait = (allowedCheckInTime - now).TotalMinutes;
                return (false, $"Chưa đến giờ nhận sân! Vui lòng quay lại sau {Math.Ceiling(minutesWait)} phút nữa.", null);
            }

            DateTime matchEndTime = slot.PlayDate.Date.Add(slot.TimeSlot.EndTime);
            if (now > matchEndTime)
            {
                return (false, "Ca đá đã kết thúc hoàn toàn, mã QR không còn hiệu lực!", null);
            }

            return (true, "Mã hợp lệ", new
            {
                BookingCode = slot.BookingCode,
                PitchName = slot.Pitch.PitchName,
                CustomerName = slot.User?.FullName ?? "Khách vãng lai",
                Date = slot.PlayDate.ToString("dd/MM/yyyy"),
                Time = $"{slot.TimeSlot.StartTime:hh\\:mm} - {slot.TimeSlot.EndTime:hh\\:mm}",
                Status = "Chờ xác nhận"
            });
        }

        public async Task<(bool Success, string Message)> ConfirmCheckInAsync(string bookingCode)
        {
            var slot = await _context.PitchSlots.FirstOrDefaultAsync(p => p.BookingCode == bookingCode);
            if (slot == null || slot.Status >= BookingStatus.CheckedIn) return (false, "Lỗi xác nhận");

            slot.Status = BookingStatus.CheckedIn;
            slot.UpdatedAt = DateTime.Now;

            var trans = await _context.Transactions.FirstOrDefaultAsync(t => t.TransactionCode == bookingCode);
            if (trans != null)
            {
                trans.Status = TransactionStatus.Success;
                _context.Transactions.Update(trans);
            }

            await _context.SaveChangesAsync();
            return (true, "Check-in thành công!");
        }

        public async Task<(bool Success, string Message, decimal RefundAmount)> CancelBookingAsync(string bookingCode, bool isAutoCancel = false)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var slot = await _context.PitchSlots
                    .Include(ps => ps.TimeSlot)
                    .Include(ps => ps.Pitch)
                    .FirstOrDefaultAsync(ps => ps.BookingCode == bookingCode);

                if (slot == null) return (false, "Mã đặt sân không tồn tại!", 0);

                if (slot.Status == BookingStatus.Cancelled || slot.Status == BookingStatus.RefundBooking)
                {
                    return (false, "Đơn đặt sân này đã được hủy trước đó.", 0);
                }

                if (slot.Status >= BookingStatus.CheckedIn)
                {
                    return (false, "Không thể hủy sân đã check-in hoặc hoàn thành.", 0);
                }

                DateTime matchStartTime = slot.PlayDate.Date.Add(slot.TimeSlot.StartTime);
                decimal refundAmount = 0;
                bool isRefunded = false;

                // Logic hoàn tiền: Nếu hủy trước > 24h
                if (!isAutoCancel && DateTime.Now < matchStartTime.AddHours(-24))
                {
                    var booking = await _context.Transactions.FirstOrDefaultAsync(t => t.TransactionCode == bookingCode && t.TransactionType == TransactionTypes.Booking);
                    if (booking != null)
                    {
                        refundAmount = booking.Amount * 1m; // Hoàn 100

                        refundAmount = booking.Amount;

                        isRefunded = true;

                        var user = await _context.Users.FindAsync(slot.UserId);
                        if (user != null)
                        {
                            user.WalletBalance += refundAmount;
                            _context.Users.Update(user);

                            // Tạo giao dịch hoàn tiền
                            var refundTrans = new Transactions
                            {
                                UserId = user.UserId,
                                Amount = refundAmount,
                                TransactionType = TransactionTypes.RefundBooking,
                                Status = TransactionStatus.Success,
                                Source = TransactionSources.Wallet,
                                TransactionDate = DateTime.Now,
                                TransactionCode = $"REF-{DateTime.Now:yyMMddHHmmss}{user.UserId}",
                              
                                Message = $"Hoàn tiền hủy sân {slot.Pitch?.PitchName} ({bookingCode})",
                                BalanceAfter = user.WalletBalance
                            };
                            _context.Transactions.Add(refundTrans);
                        }
                    }
                }

                // Cập nhật trạng thái sân
                slot.Status = BookingStatus.RefundBooking;
                slot.UpdatedAt = DateTime.Now;
                _context.PitchSlots.Update(slot);

                // Cập nhật trạng thái giao dịch gốc
                var originalTrans = await _context.Transactions.FirstOrDefaultAsync(t => t.TransactionCode == bookingCode && t.TransactionType == TransactionTypes.Booking);
                if (originalTrans != null)
                {
                    originalTrans.Status = TransactionStatus.CancelBooking;
                    _context.Transactions.Update(originalTrans);
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                string msg = isRefunded ? $"Hủy sân thành công. Bạn được hoàn lại {refundAmount:N0}₫ (100%)." : "Hủy sân thành công. Giao dịch không đủ điều kiện hoàn tiền.";
                if (isAutoCancel) msg = "Hệ thống tự động hủy sân do quá hạn check-in.";

                return (true, msg, refundAmount);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return (false, "Lỗi khi hủy sân: " + ex.Message, 0);
            }
        }

        public async Task<List<BookingManagementDTO>> GetPendingBookingsAsync(DateTime? fromDate, DateTime? toDate, int? timeSlotId, int? categoryId, string? search)
        {
            var query = _context.PitchSlots
                .Include(ps => ps.Pitch)
                    .ThenInclude(p => p.Category)
                .Include(ps => ps.TimeSlot)
                .Include(ps => ps.User)
                .Where(ps => ps.Status == BookingStatus.PendingConfirm);

            if (fromDate.HasValue)
                query = query.Where(ps => ps.PlayDate == fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(ps => ps.PlayDate <= toDate.Value);

            if (timeSlotId.HasValue)
                query = query.Where(ps => ps.SlotId == timeSlotId.Value);

            if (categoryId.HasValue)
                query = query.Where(ps => ps.Pitch.CategoryId == categoryId.Value);

            if (!string.IsNullOrEmpty(search))
            {
                search = search.ToLower();
                query = query.Where(ps => ps.BookingCode.ToLower().Contains(search) ||
                                          (ps.User.FullName != null && ps.User.FullName.ToLower().Contains(search)) ||
                                          (ps.User.Phone != null && ps.User.Phone.Contains(search)));
            }
            
            var bookings = await query.OrderByDescending(ps => ps.PlayDate).ThenBy(ps => ps.TimeSlot.StartTime).ToListAsync();
            var result = new List<BookingManagementDTO>();
            
            foreach (var b in bookings)
            {
                var trans = await _context.Transactions
                    .Where(t => t.TransactionCode == b.BookingCode && t.TransactionType == TransactionTypes.Booking)
                    .FirstOrDefaultAsync();

                result.Add(new BookingManagementDTO
                {
                    Id = b.PitchSlotId,
                    Code = b.BookingCode,
                    CustomerName = b.User?.FullName ?? "N/A",
                    CustomerPhone = b.User?.Phone ?? "N/A",
                    PitchName = b.Pitch?.PitchName ?? "N/A",
                    Date = b.PlayDate.ToString("dd/MM/yyyy"),
                    Time = $"{b.TimeSlot.StartTime:hh\\:mm} - {b.TimeSlot.EndTime:hh\\:mm}",
                    SlotId = b.SlotId,
                    TotalPrice = trans?.Amount ?? 0
                });
            }
            
            return result;
        }

        public async Task<(bool Success, string Message)> ApproveBookingAsync(int bookingId)
        {
            var booking = await _context.PitchSlots.FindAsync(bookingId);
            if (booking == null) return (false, "Booking not found");
            if (booking.Status != BookingStatus.PendingConfirm) return (false, "Status invalid");

            booking.Status = BookingStatus.Completed; 
            return (true, "Đã duyệt đơn.");
        }

        public async Task<(bool Success, string Message)> RejectBookingAsync(int bookingId)
        {
             return await CancelBooking(bookingId); 
        }
        
        private async Task<(bool Success, string Message)> CancelBooking(int bookingId)
        {
            var booking = await _context.PitchSlots.FindAsync(bookingId);
            if (booking == null) return (false, "Not found");
            var result = await CancelBookingAsync(booking.BookingCode, false);
            return (result.Success, result.Message);
        }

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