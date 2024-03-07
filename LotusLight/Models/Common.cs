using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LotusLight.Models
{
    public class Return
    {
        public string OrderNo { get; set; } = string.Empty;
        public int Amount { get; set; } = -1;
        public int AuthAmount { get; set; } = -1;
        public string AuthTime { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string PaymentNo { get; set; } = string.Empty;
    }

    public class Receive {
        public string OrderNo { get; set; } = string.Empty;
        public int Amount { get; set; } = -1;
        public string BankNo { get; set; } = string.Empty;
        public string AtmNo { get; set; } = string.Empty;
        public string PayEndDate { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string PayCode { get; set; } = string.Empty;
        public string PayType { get; set; } = string.Empty;
        public string PayTypeName { get; set; } = string.Empty;
    }

    public class Carousel
    {
        public int Sn { get; set; }
        public string ImgName { get; set; }
        public string TargetLink { get; set; }
    }

    public class News
    {
        public int Sn { get; set; }
        public string Title { get; set; }
        public DateTime ArticleDate { get; set; }
        public string P1 { get; set; }
        public string P2 { get; set; }
        public string V1 { get; set; }
        public string V2 { get; set; }
        public string TitleFileName { get; set; }
        public string IsTop { get; set; }
        public DateTime CreateDate { get; set; }
        public List<NewsImage> Imgs { get; set; } = new List<NewsImage>();
    }

    public class NewsImage
    {
        public int Sn { get; set; }
    }

    public class Activity
    {
        public int Sn { get; set; }
        public string Title { get; set; }
        public string P1 { get; set; }
        public string P2 { get; set; }
        public string TitleFileName { get; set; }
        public string IsTop { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime ArticleDate { get; set; }
        public List<ActivityImage> Imgs { get; set; } = new List<ActivityImage>();
    }

    public class ActivityImage
    {
        public int Sn { get; set; }
    }

    public class Related
    {
        public int Sn { get; set; }
        public string Title { get; set; }
        public decimal Amount { get; set; }
        public string P1 { get; set; }
        public string P2 { get; set; }
        public string TitleFileName { get; set; }
        public string Notes { get; set; }
        public DateTime CreateDate { get; set; }
    }

    public class Recruit
    {
        public int Sn { get; set; }
        public string Title { get; set; }
        public string SubTitle { get; set; }
        public string P1 { get; set; }
        public string P2 { get; set; }
        public string TitleFileName { get; set; }
        public DateTime CreateDate { get; set; }
    }

    public class Contact
    {
        public int Sn { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public string AddressEng { get; set; }
        public string Mail { get; set; }
        public string Tel { get; set; }
        public string Fax { get; set; }
        public string FbLink { get; set; }
        public string MerchantId { get; set; }
        public string TerminalId { get; set; }
        public string GoogleMap { get; set; }
        public string BankName { get; set; }
        public string BankNameEng { get; set; }
        public string BankCode { get; set; }
        public string AccountName { get; set; }
        public string AccountNameEng { get; set; }
        public string AccountNum { get; set; }
        public DateTime CreateDate { get; set; }
    }

    public class FavouriteLink
    {
        public int Sn { get; set; }
        public string Name { get; set; }
        public string Link { get; set; }
        public int Sort { get; set; }
        public DateTime CreateDate { get; set; }
    }

    public class Subscription
    {
        public int Sn { get; set; }
        public string Mail { get; set; }
        public DateTime CreateDate { get; set; }
    }

    public class Download
    {
        public int Sn { get; set; }
        public string FileName { get; set; }
        public DateTime CreateDate { get; set; }
        public bool ExistsPdf { get; set; }
        public bool ExistsWord { get; set; }
    }

    public class Member
    {
        public string Id { get; set; }
        public int AreaId { get; set; }
        public string Name { get; set; }
    }

    public class Area
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class VideoType
    {
        public int Sn { get; set; }
        public string Name { get; set; }
        public DateTime CreateDate { get; set; }
        public bool Focus { get; set; }
    }

    public class Video
    {
        public int Sn { get; set; }
        public string Name { get; set; }
        public string P1 { get; set; }
        public string P2 { get; set; }
        public string Lnk { get; set; }
        public DateTime CreateDate { get; set; }
    }

    public class Donations
    {
        public string Name { get; set; }
        public string Addr { get; set; }
        public DateTime PayDate { get; set; }
        public string Amount { get; set; }
        public string DonateFor { get; internal set; }
        public string Name_data { get; set; }
    }
}