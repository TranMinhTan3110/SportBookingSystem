using SportBookingSystem.Models.ViewModels;
namespace SportBookingSystem.Services
{
    public interface IVnPayService
    {
        string CreatePaymentUrl(decimal amount, string txnRef, HttpContext context);
        VnPayResponseModel ProcessVnPayReturn(IQueryCollection queryParams);
    }
}