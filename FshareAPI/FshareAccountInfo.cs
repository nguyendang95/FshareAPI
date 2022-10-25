using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace FshareAPI
{
    public class FshareAccountInfo
    {
        public string Id { get; set; }
        public string Level { get; set; }
        public string Name { get; set; }
        public string Phone { get; set; }
        public string Birthday { get; set; }
        public string Gender { get; set; }
        public string Address { get; set; }
        public string IdCard { get; set; }
        public string City { get; set; }
        public string Email { get; set; }
        public string JoinDate { get; set; }
        public long TotalPoints { get; set; }
        public string ExpireVip { get; set; }
        public long Traffic { get; set; }
        public long TrafficUsed { get; set; }
        public long Webspace { get; set; }
        public long WebspaceUsed { get; set; }
        public long WebspaceSecure { get; set; }
        public long WebspaceSecureUsed { get; set; }
        public long Amount { get; set; }
        public long DownloadTimeAvailable { get; set; }
        public string AccountType { get; set; }
        public string Occupation { get; set; }
        public string JobName { get; set; }
        public string StatusTelesalePrepaid { get; set; }
        public string Country { get; set; }
        public string CountryName { get; set; }
        public HttpClient ActiveService { get; set; }
        internal string Msg { get; set; }
        internal string Code { get; set; }
        public void ExportToTextFile(string DestinationFolder)
        {
            StreamWriter objStreamWriter = new StreamWriter(new FileStream(DestinationFolder + "\\" + "FshareAccountInfo-" + Id + ".txt", FileMode.OpenOrCreate), Encoding.Unicode);
            objStreamWriter.WriteLine("Thông tin tài khoản Fshare:");
            objStreamWriter.WriteLine();
            objStreamWriter.WriteLine("Id: " + Id);
            objStreamWriter.WriteLine("Loại tài khoản: " + AccountType);
            objStreamWriter.WriteLine("Tên: " + Name);
            objStreamWriter.WriteLine("Số điện thoại: " + Phone);
            objStreamWriter.WriteLine("Ngày sinh: " + Birthday);
            objStreamWriter.WriteLine("Giới tính: " + Gender);
            objStreamWriter.WriteLine("Địa chỉ: " + Address);
            objStreamWriter.WriteLine("Thành phố:" + City);
            objStreamWriter.WriteLine("Số CMND: " + IdCard);
            objStreamWriter.WriteLine("E-mail: " + Email);
            objStreamWriter.WriteLine("Nghề nghiệp: " + Occupation);
            objStreamWriter.WriteLine("Tên nghề nghiệp: " + JobName);
            objStreamWriter.WriteLine("Quốc gia: " + Country);
            objStreamWriter.WriteLine("Ngôn ngữ: " + CountryName);
            objStreamWriter.WriteLine("Ngày tạo tài khoản: " + JoinDate);
            objStreamWriter.WriteLine("Tổng điểm Fshare: " + TotalPoints);
            objStreamWriter.WriteLine("Ngày hết hạn trạng thái VIP: " + ExpireVip);
            objStreamWriter.WriteLine("Lưu lượng: " + Traffic.ToString());
            objStreamWriter.WriteLine("Lưu lượng đã dùng: " + TrafficUsed.ToString());
            objStreamWriter.WriteLine("Dung lượng lưu trữ: " + Webspace.ToString());
            objStreamWriter.WriteLine("Dung lượng lưu trữ đã dùng: " + WebspaceUsed.ToString());
            objStreamWriter.WriteLine("Dung lượng lưu trữ đảm bảo: " + WebspaceSecure.ToString());
            objStreamWriter.WriteLine("Dung lượng lưu trữ đảm bảo đã dùng: " + WebspaceSecureUsed.ToString());
            objStreamWriter.WriteLine("Fxu: " + Amount.ToString());
            objStreamWriter.WriteLine("Thời gian tải xuống hiện có: " + DownloadTimeAvailable.ToString());
            objStreamWriter.WriteLine("Tình trạng thanh toán trước: " + StatusTelesalePrepaid);
            objStreamWriter.Close();
        }
    }
}
