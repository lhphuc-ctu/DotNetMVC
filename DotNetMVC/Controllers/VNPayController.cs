using DotNetMVC.ViewModel;
using MailKit.Search;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Org.BouncyCastle.Asn1.Ocsp;
using System.Collections.Generic;
using VNPayment;

namespace DotNetMVC.Controllers
{
    public class VNPayController : Controller
    {
        private readonly IVNPayPayment _vnpay;

        public VNPayController(IVNPayPayment vnpay)
        {
            _vnpay = vnpay;
        }
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Pay(string OrderId, string Amount)
        {
            var vnpayPaymentRequest = new VnPayRequest
            {
                OrderId = OrderId,
                Amount = Amount,
                OrderInfo = "Thông tin đơn hàng #"+OrderId,
                IpAddress = Request.HttpContext.Connection.RemoteIpAddress.ToString()
            };
            var vnpayPaymentUrl = _vnpay.CreatePaymentUrl(vnpayPaymentRequest);
            return Redirect(vnpayPaymentUrl);
        }

        public IActionResult PayReturn()
        {
            if (Request.Query.Count > 0)
            {
                var vnpayData = Request.Query;
                SortedList<String, String> data = new SortedList<String, String>();

                foreach (KeyValuePair<string, StringValues> kv in vnpayData)
                {
                    data.Add(kv.Key, kv.Value);
                }
                _vnpay.AddResponse(data);

                PaymentViewModel paymentViewModel = new()
                {
                    orderId = _vnpay.GetResponseData("vnp_TxnRef"),
                    vnpayTranId = Convert.ToInt64(_vnpay.GetResponseData("vnp_TransactionNo")),
                    vnp_ResponseCode = _vnpay.GetResponseData("vnp_ResponseCode"),
                    vnp_TransactionStatus = _vnpay.GetResponseData("vnp_TransactionStatus"),
                    vnp_SecureHash = Request.Query["vnp_SecureHash"].ToString(),
                    TerminalID = Request.Query["vnp_TmnCode"].ToString(),
                    vnp_Amount = Convert.ToInt64(_vnpay.GetResponseData("vnp_Amount")) / 100,
                    bankCode = Request.Query["vnp_BankCode"].ToString()
                };

                bool checkSignature = _vnpay.ValidateSignature(paymentViewModel.vnp_SecureHash);

                if (checkSignature)
                {
                    if (paymentViewModel.vnp_ResponseCode == "00" && paymentViewModel.vnp_TransactionStatus == "00")
                    {
                        paymentViewModel.Status = true;
                    }
                    else
                    {
                        paymentViewModel.Status = false;
                    }
                }

                return View(paymentViewModel);

            }

            return View();
        }
    }
}
