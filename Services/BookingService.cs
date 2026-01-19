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
            // Lấy TẤT CẢ sân trước, sau đó filter trong memory để xử lý Unicode đúng
            var allPitches = await _context.Pitches
                .Include(p => p.Category)
                .ToListAsync();

            // Debug: Log tất cả sân
            Console.WriteLine("=== TẤT CẢ SÂN TRONG DB ===");
            foreach (var p in allPitches)
            {
                Console.WriteLine($"ID: {p.PitchId}, Tên: {p.PitchName}, Status: [{p.Status}], Length: {p.Status?.Length}");
            }

            // Filter CHỈ LẤY sân "Sẵn sàng" - so sánh trong memory (C#)
            var pitches = allPitches
                .Where(p => p.Status != null && p.Status.Trim() == "Sẵn sàng")
                .ToList();

            Console.WriteLine($"Số sân SAU KHI LỌC 'Sẵn sàng': {pitches.Count}");
            Console.WriteLine("========================");

            // Lọc theo danh mục
            if (categoryIds != null && categoryIds.Any())
            {
                pitches = pitches.Where(p => categoryIds.Contains(p.CategoryId)).ToList();
            }

            // Lọc theo khoảng giá
            if (minPrice.HasValue)
            {
                pitches = pitches.Where(p => p.PricePerHour >= minPrice.Value).ToList();
            }

            if (maxPrice.HasValue)
            {
                pitches = pitches.Where(p => p.PricePerHour <= maxPrice.Value).ToList();
            }

            var totalCount = pitches.Count;

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
                // Bỏ qua sân đang bảo trì (double check)
                if (pitch.Status == "Bảo trì")
                {
                    continue;
                }

                foreach (var timeSlot in timeSlots)
                {
                    bool isAvailable = true;
                    string status = "available";
                    string statusText = "✓ Còn trống";

                    // Kiểm tra trạng thái của sân trong khung giờ này
                    var pitchSlot = pitchSlots.FirstOrDefault(ps =>
                        ps.PitchId == pitch.PitchId &&
                        ps.SlotId == timeSlot.SlotId);

                    // Xử lý trạng thái cụ thể của slot
                    if (pitchSlot != null)
                    {
                        // Chỉ có 2 trạng thái: Trống (0) hoặc Đã đặt (1)
                        if (pitchSlot.Status == 1)
                        {
                            // Đã đặt
                            status = "booked";
                            statusText = "✕ Đã đặt";
                            isAvailable = false;
                        }
                        else
                        {
                            // Còn trống (Status = 0 hoặc bất kỳ giá trị nào khác)
                            status = "available";
                            statusText = "✓ Còn trống";
                            isAvailable = true;
                        }
                    }
                    else
                    {
                        // Không có dữ liệu trong PitchSlots → Mặc định còn trống
                        status = "available";
                        statusText = "✓ Còn trống";
                        isAvailable = true;
                    }

                    // Tính thời gian của slot
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

            var displayCount = result.Count;

            return new FilterPitchesResponse
            {
                PitchSlots = result,
                TotalCount = pitches.Count, // Số sân thực (không phải số kết quả)
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
            }

            response.DisplayCount = response.PitchSlots.Count;

            return response;
        }

        public async Task<FilterPitchesResponse> GetFilteredPitchesWithPaginationAsync(
            DateTime date,
            int? slotId,
            List<int>? categoryIds,
            decimal? minPrice,
            decimal? maxPrice,
            List<string>? statusFilter,
            int page,
            int pageSize)
        {
            // Lấy tất cả kết quả
            var response = await GetFilteredPitchesAsync(date, slotId, categoryIds, minPrice, maxPrice, statusFilter);

            // Tính phân trang
            var totalItems = response.PitchSlots.Count;
            var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            // Lấy dữ liệu theo trang
            response.PitchSlots = response.PitchSlots
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            response.TotalPages = totalPages;
            response.CurrentPage = page;
            response.DisplayCount = response.PitchSlots.Count;

            return response;
        }
    }
}