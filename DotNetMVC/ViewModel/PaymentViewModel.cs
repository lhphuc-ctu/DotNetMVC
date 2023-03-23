namespace DotNetMVC.ViewModel
{
    public class PaymentViewModel
    {
        public string orderId { get; set; }
        public long vnpayTranId { get; set; }
        public string vnp_ResponseCode { get; set; }
        public string vnp_TransactionStatus { get; set; }
        public String vnp_SecureHash { get; set; }
        public String TerminalID { get; set; }
        public long vnp_Amount { get; set; }
        public String bankCode { get; set; }
        public bool Status { get; set; }
    }
}
