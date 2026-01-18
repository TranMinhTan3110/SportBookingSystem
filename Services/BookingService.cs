using Microsoft.EntityFrameworkCore;
using SportBookingSystem.Models.EF;
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

        public async Task<FilterPitchesResponse> GetAvailablePitchesAsync(
            DateTime date,
            int? slotId,
            List<int>? categoryIds,
            decimal? minPrice,
            decimal? maxPrice)
        {
            // Lấy tất cả sân với Category
            var pitchesQuery = _context.Pitches
                .Include(p => p.Category)
                .AsQueryable();

            // Lọc theo danh mục
            if (categoryIds != null && categoryIds.Any())
            {
                pitchesQuery = pitchesQuery.Where(p => categoryIds.Contains(p.CategoryId));
            }

            // Lọc theo khoảng giá
            if (minPrice.HasValue)
            {
                pitchesQuery = pitchesQuery.Where(p => p.PricePerHour >= minPrice.Value);
            }

            if (maxPrice.HasValue)
            {
                pitchesQuery = pitchesQuery.Where(p => p.PricePerHour <= maxPrice.Value);
            }

            var pitches = await pitchesQuery.ToListAsync();

            // Lấy tất cả TimeSlots hoạt động
            var timeSlotsQuery = _context.TimeSlots
                .Where(ts => ts.IsActive)
                .OrderBy(ts => ts.StartTime)
                .AsQueryable();

            // Nếu có lọc theo slotId thì chỉ lấy slot đó
            if (slotId.HasValue)
            {
                timeSlotsQuery = timeSlotsQuery.Where(ts => ts.SlotId == slotId.Value);
            }

            var timeSlots = await timeSlotsQuery.ToListAsync();

            // Lấy trạng thái đặt sân cho ngày được chọn
            var pitchSlots = await _context.PitchSlots
                .Where(ps => ps.PlayDate.Date == date.Date)
                .ToListAsync();

            // Tạo danh sách kết quả - MỖI SÂN x MỖI KHUNG GIỜ
            var result = new List<PitchSlotViewModel>();

            foreach (var pitch in pitches)
            {
                foreach (var timeSlot in timeSlots)
                {
                    // Kiểm tra trạng thái của sân trong khung giờ này
                    var pitchSlot = pitchSlots.FirstOrDefault(ps =>
                        ps.PitchId == pitch.PitchId &&
                        ps.SlotId == timeSlot.SlotId);

                    bool isAvailable = true;
                    string status = "available";
                    string statusText = "✓ Còn trống";

                    if (pitchSlot != null)
                    {
                        if (pitchSlot.Status == 1)
                        {
                            // Đã đặt
                            isAvailable = false;
                            status = "booked";
                            statusText = "✕ Đã hết sân";
                        }
                        else if (pitchSlot.Status == 2)
                        {
                            // Đang giữ chỗ
                            status = "limited";
                            statusText = "⚠️ Sắp hết";
                        }
                    }

                    // Tính thời gian của slot (giả sử mỗi slot là 1.5 giờ)
                    var duration = (timeSlot.EndTime - timeSlot.StartTime).TotalHours;
                    decimal fullPrice = pitch.PricePerHour * (decimal)duration;
                    decimal depositPrice = fullPrice * 0.3m;

                    var viewModel = new PitchSlotViewModel
                    {
                        PitchId = pitch.PitchId,
                        PitchName = pitch.PitchName,
                        CategoryName = pitch.Category?.CategoryName ?? "Không xác định",
                        Capacity = pitch.Capacity,
                        ImageUrl = pitch.ImageUrl ?? "/images/default-pitch.jpg",
                        Description = pitch.Description ?? "",

                        SlotId = timeSlot.SlotId,
                        SlotName = timeSlot.SlotName,
                        TimeRange = $"{timeSlot.StartTime:hh\\:mm} - {timeSlot.EndTime:hh\\:mm}",

                        PricePerHour = pitch.PricePerHour,
                        FullPrice = fullPrice,
                        DepositPrice = depositPrice,

                        Status = status,
                        StatusText = statusText,
                        IsAvailable = isAvailable
                    };

                    result.Add(viewModel);
                }
            }

            var totalCount = result.Count;
            var displayCount = result.Count;

            return new FilterPitchesResponse
            {
                PitchSlots = result,
                TotalCount = totalCount,
                DisplayCount = displayCount
            };
        }

        public async Task<FilterPitchesResponse> GetFilteredPitchesAsync(
            DateTime date,
            int? slotId,
            List<int>? categoryIds,
            decimal? minPrice,
            decimal? maxPrice,
            List<string>? statusFilter)
        {
            // Gọi method chính để lấy dữ liệu
            var response = await GetAvailablePitchesAsync(date, slotId, categoryIds, minPrice, maxPrice);

            // Nếu có filter theo trạng thái
            if (statusFilter != null && statusFilter.Any())
            {
                response.PitchSlots = response.PitchSlots
                    .Where(p => statusFilter.Contains(p.Status))
                    .ToList();

                response.DisplayCount = response.PitchSlots.Count;
            }

            return response;
        }
    }
}