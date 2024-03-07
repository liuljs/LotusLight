using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Admin.Models
{
    public class DonateRecord
    {
        public string OrderId { get; set; }
        public int OrderTypeId { get; set; }
        public string OrderType { get; set; }
        public string BuyerType { get; set; }
        public string BuyerName { get; set; }
        public string BuyerId { get; set; }
        public string BuyerPhone { get; set; }
        public string BuyerMail { get; set; }
        public string Branch { get; set; }
        public int DonateAmount { get; set; }
        public string DonateUnit { get; set; }
        public string DonateFor { get; set; }
        public string NeedReceipt { get; set; }
        public string NeedAnonymous { get; set; }
        public string ReceiptAddress { get; set; }
        public string PickupMethod { get; set; }
        public string Remark { get; set; }
        public int OrderStatusId { get; set; }
        public string OrderStatus { get; set; }
        public int PayStatusId { get; set; }
        public string PayStatus { get; set; }
        public int PayTypeId { get; set; }
        public string PayType { get; set; }
        public string PayDate { get; set; }
        public string BranchName { get; set; }
        public string IsManual { get; set; }
        public List<Product> Products { get; set; }
    }

    public class Product
    {
        public string Name { get; set; }
        public int Qty { get; set; }
    }

        public class Carousel
    {
        public int Sn { get; set; }
        public string ImgName { get; set; }
        public string TargetLink { get; set; }
        public int Sort { get; set; }
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
}