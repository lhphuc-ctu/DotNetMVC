using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace VNPayment
{
    public class VNPayPayment : IVNPayPayment
    {
        private readonly VNPayConfig _config;
        public const string VERSION = "2.1.0";
        private SortedList<String, String> _requestData = new SortedList<String, String>(new VnPayCompare());
        private SortedList<String, String> _responseData = new SortedList<String, String>(new VnPayCompare());

        public VNPayPayment(VNPayConfig config)
        {
            _config = config;
        }
        #region Request

        //Them du lieu vao Request
        public void AddRequestData(VnPayRequest vnPayRequest)
        {
            if (!String.IsNullOrEmpty(vnPayRequest.OrderId)) _requestData.Add("vnp_TxnRef", vnPayRequest.OrderId);
            if (!String.IsNullOrEmpty(vnPayRequest.OrderInfo)) _requestData.Add("vnp_OrderInfo", vnPayRequest.OrderInfo);
            if (!String.IsNullOrEmpty(vnPayRequest.OrderType)) _requestData.Add("vnp_OrderType", vnPayRequest.OrderType);
            if (!String.IsNullOrEmpty(vnPayRequest.Amount)) _requestData.Add("vnp_Amount", vnPayRequest.Amount);
            if (!String.IsNullOrEmpty(vnPayRequest.IpAddress)) _requestData.Add("vnp_IpAddr", vnPayRequest.IpAddress);
            _requestData.Add("vnp_Version", VERSION);
            _requestData.Add("vnp_Command", "pay");
            _requestData.Add("vnp_TmnCode", _config.Vnp_TmnCode);
            _requestData.Add("vnp_CreateDate", DateTime.Now.ToString("yyyyMMddHHmmss"));
            _requestData.Add("vnp_CurrCode", "VND");
            _requestData.Add("vnp_Locale", "vn");
            _requestData.Add("vnp_ReturnUrl", _config.Vnp_ReturnUrl);
        }

        //Tao url thanh toan
        public string CreatePaymentUrl(VnPayRequest vnPayRequest)
        {
            AddRequestData(vnPayRequest);
            string baseUrl = _config.Vnp_Url;
            StringBuilder data = new StringBuilder();
            foreach (KeyValuePair<string, string> kv in _requestData)
            {
                if (!String.IsNullOrEmpty(kv.Value))
                {
                    data.Append(WebUtility.UrlEncode(kv.Key) + "=" + WebUtility.UrlEncode(kv.Value) + "&");
                }
            }
            string queryString = data.ToString();

            baseUrl += "?" + queryString;
            String signData = queryString;
            if (signData.Length > 0)
            {

                signData = signData.Remove(data.Length - 1, 1);
            }
            string vnp_SecureHash = Utils.HmacSHA512(_config.Vnp_HashSecret, signData);
            baseUrl += "vnp_SecureHash=" + vnp_SecureHash;

            return baseUrl;
        }

        #endregion

        #region Response

        //Them du lieu vao Response
        public void AddResponse(SortedList<String, String> responseData)
        {
            _responseData = responseData;
        }

        //Kiem tra chu ky dien tu
        public bool ValidateSignature(string inputHash)
        {
            string rspRaw = GetResponseData();
            string myChecksum = Utils.HmacSHA512(_config.Vnp_HashSecret, rspRaw);
            return myChecksum.Equals(inputHash, StringComparison.InvariantCultureIgnoreCase);
        }

        //Lay du lieu theo gia tri
        public string GetResponseData(string key)
        {
            string retValue;
            if (_responseData.TryGetValue(key, out retValue))
            {
                return retValue;
            }
            else
            {
                return string.Empty;
            }
        }

        //Lay du lieu tra ve
        private string GetResponseData()
        {

            StringBuilder data = new StringBuilder();
            if (_responseData.ContainsKey("vnp_SecureHashType"))
            {
                _responseData.Remove("vnp_SecureHashType");
            }
            if (_responseData.ContainsKey("vnp_SecureHash"))
            {
                _responseData.Remove("vnp_SecureHash");
            }
            foreach (KeyValuePair<string, string> kv in _responseData)
            {
                if (!String.IsNullOrEmpty(kv.Value))
                {
                    data.Append(WebUtility.UrlEncode(kv.Key) + "=" + WebUtility.UrlEncode(kv.Value) + "&");
                }
            }
            //remove last '&'
            if (data.Length > 0)
            {
                data.Remove(data.Length - 1, 1);
            }
            return data.ToString();
        }

        #endregion
    }
}
