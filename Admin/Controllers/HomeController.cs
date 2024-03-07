using ecSqlDBManager;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ecDataParse;
using Admin.Models;
using System.Web.Security;
using System.IO;

namespace Admin.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        protected static string ConnString = ConfigurationManager.AppSettings["DbConnection"];
        protected DataBase db = new DataBase(ConnString);

        [AllowAnonymous]
        public ActionResult Login()
        {
            if (!Request.Form.HasKeys())
            {
                ViewBag.Message = "";
            }
            else
            {
                string a = Request.Form["managerAcc"];
                string p = Request.Form["managerPsw"];

                SqlCommand sql = new SqlCommand(@"SELECT PASSWORD, SALT FROM USERS WHERE ACCOUNT = @ACCOUNT");
                sql.Parameters.AddWithValue("@ACCOUNT", a);
                DataTable dt = db.GetResult(sql);
                if (dt.Rows.Count > 0)
                {
                    string pwd = FormsAuthentication.HashPasswordForStoringInConfigFile(p + dt.Rows[0]["SALT"].ToString(), "SHA1");
                    //if (1 == 1)
                    if (dt.Rows[0]["PASSWORD"].ToString() == pwd)
                    {
                        var ticket = new FormsAuthenticationTicket(
                        version: 1,
                        name: a, //可以放使用者Id
                        issueDate: DateTime.UtcNow,//現在UTC時間
                        expiration: DateTime.UtcNow.AddMinutes(60),//Cookie有效時間=現在時間往後+30分鐘
                        isPersistent: true,// 是否要記住我 true or false
                        userData: a, //可以放使用者角色名稱
                        cookiePath: FormsAuthentication.FormsCookiePath);

                        var encryptedTicket = FormsAuthentication.Encrypt(ticket); //把驗證的表單加密
                        var cookie = new HttpCookie(FormsAuthentication.FormsCookieName, encryptedTicket);
                        Response.Cookies.Add(cookie);
                        return RedirectToAction("Carousel");
                    }
                    else
                        ViewBag.Message = "帳號或密碼錯誤！";
                }
                else
                    ViewBag.Message = "帳號或密碼錯誤！";
            }
            return View();
        }

        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();      
            return RedirectToAction("Login", "Home");
        }

        /// <summary>
        /// 輪播管理
        /// </summary>
        /// <returns></returns>
        public ActionResult Carousel()
        {
            SqlCommand sql = new SqlCommand("SELECT SN, IMGNAME, TARGETLINK, SORT FROM CAROUSEL ORDER BY SORT");
            DataTable dt = db.GetResult(sql);

            List<Carousel> result = new List<Carousel>();
            foreach (DataRow dr in dt.Rows)
            {
                result.Add(new Carousel()
                {
                    Sn = dr["SN"].ParseInt(),
                    ImgName = dr["IMGNAME"].ToString(),
                    TargetLink = dr["TARGETLINK"].ToString(),
                    Sort = dr["SORT"].ParseInt()
                });
            }

            ViewBag.ObjList = result;

            return View();
        }
        /// <summary>
        /// 消息管理
        /// </summary>
        /// <returns></returns>
        public ActionResult News()
        {
            SqlCommand sql = new SqlCommand("SELECT SN, TITLE, ISTOP, ARTICLEDATE, CREATEDATE FROM NEWS ORDER BY ISTOP DESC, ARTICLEDATE DESC");
            DataTable dt = db.GetResult(sql);


            List<News> result = new List<News>();
            foreach (DataRow dr in dt.Rows)
            {
                result.Add(new News()
                {
                    Sn = dr["SN"].ParseInt(),
                    Title = dr["TITLE"].ToString(),
                    IsTop = dr["ISTOP"].ToString(),
                    ArticleDate = dr["ARTICLEDATE"].ParseDateTime(),
                    CreateDate = dr["CREATEDATE"].ParseDateTime()
                });
            }

            ViewBag.ObjList = result;

            return View();
        }
        /// <summary>
        /// 活動管理
        /// </summary>
        /// <returns></returns>
        public ActionResult Activity()
        {
            SqlCommand sql = new SqlCommand("SELECT SN, TITLE, ISTOP, ARTICLEDATE, CREATEDATE FROM ACTIVITY ORDER BY ISTOP DESC, ARTICLEDATE DESC");
            DataTable dt = db.GetResult(sql);

            List<Activity> result = new List<Activity>();
            foreach (DataRow dr in dt.Rows)
            {
                result.Add(new Activity()
                {
                    Sn = dr["SN"].ParseInt(),
                    Title = dr["TITLE"].ToString(),
                    IsTop = dr["ISTOP"].ToString(),
                    ArticleDate = dr["ARTICLEDATE"].ParseDateTime(),
                    CreateDate = dr["CREATEDATE"].ParseDateTime()
                });
            }

            ViewBag.ObjList = result;

            return View();
        }
        /// <summary>
        /// 招募管理
        /// </summary>
        /// <returns></returns>
        public ActionResult Recruit()
        {
            SqlCommand sql = new SqlCommand("SELECT SN, TITLE, SUBTITLE, CONTENT1, CONTENT2, CREATEDATE FROM RECRUIT ORDER BY SN DESC");
            DataTable dt = db.GetResult(sql);

            List<Recruit> result = new List<Recruit>();
            foreach (DataRow dr in dt.Rows)
            {
                result.Add(new Recruit()
                {
                    Sn = dr["SN"].ParseInt(),
                    Title = dr["TITLE"].ToString(),
                    SubTitle = dr["SUBTITLE"].ToString(),
                    P1 = dr["CONTENT1"].ToString(),
                    P2 = dr["CONTENT2"].ToString(),
                    CreateDate = dr["CREATEDATE"].ParseDateTime()
                });
            }

            ViewBag.ObjList = result;

            return View();
        }
        /// <summary>
        /// 請購管理
        /// </summary>
        /// <returns></returns>
        public ActionResult Related()
        {
            SqlCommand sql = new SqlCommand("SELECT SN, TITLE, AMOUNT, CONTENT1, CONTENT2, NOTES, CREATEDATE FROM RELATED ORDER BY SN DESC");
            DataTable dt = db.GetResult(sql);

            List<Related> result = new List<Related>();
            foreach (DataRow dr in dt.Rows)
            {
                result.Add(new Related()
                {
                    Sn = dr["SN"].ParseInt(),
                    Title = dr["TITLE"].ToString(),
                    Amount = dr["AMOUNT"].ParseDecimal(),
                    P1 = dr["CONTENT1"].ToString(),
                    P2 = dr["CONTENT2"].ToString(),
                    Notes = dr["NOTES"].ToString(),
                    CreateDate = dr["CREATEDATE"].ParseDateTime()
                });
            }

            ViewBag.ObjList = result;

            return View();
        }
        /// <summary>
        /// 影片管理
        /// </summary>
        /// <returns></returns>
        public ActionResult Video()
        {
            SqlCommand sql = new SqlCommand("SELECT SN, NAME, CREATEDATE FROM VIDEOTYPE ORDER BY SN DESC");
            DataTable dt = db.GetResult(sql);

            List<VideoType> result = new List<VideoType>();
            foreach (DataRow dr in dt.Rows)
            {
                result.Add(new VideoType()
                {
                    Sn = dr["SN"].ParseInt(),
                    Name = dr["NAME"].ToString(),
                    CreateDate = dr["CREATEDATE"].ParseDateTime()
                });
            }

            ViewBag.ObjList = result;

            return View();
        }
        /// <summary>
        /// 聯絡資訊
        /// </summary>
        /// <returns></returns>
        public ActionResult Contact()
        {
            SqlCommand sql = new SqlCommand(@"
                    SELECT 
                        SN, NAME, ADDRESS, ADDRESSENG, TEL, FAX, FBLINK, MAIL,
                        MERCHANTID, TERMINALID, GOOGLEMAP, 
                        BANKNAME, BANKNAMEENG, BANKCODE, ACCOUNTNAME, ACCOUNTNAMEENG, ACCOUNTNUM,
                        CREATEDATE 
                    FROM BRANCH
                    ORDER BY SN");
            DataTable dt = db.GetResult(sql);

            List<Contact> result = new List<Contact>();
            foreach (DataRow dr in dt.Rows)
            {
                result.Add(new Contact()
                {
                    Sn = dr["SN"].ParseInt(),
                    Name = dr["NAME"].ToString(),
                    Address = dr["ADDRESS"].ToString(),
                    AddressEng = dr["ADDRESSENG"].ToString(),
                    Mail = dr["MAIL"].ToString(),
                    Tel = dr["TEL"].ToString(),
                    Fax = dr["FAX"].ToString(),
                    FbLink = dr["FBLINK"].ToString(),
                    MerchantId = dr["MERCHANTID"].ToString(),
                    TerminalId = dr["TERMINALID"].ToString(),
                    GoogleMap = dr["GOOGLEMAP"].ToString(),
                    BankName = dr["BANKNAME"].ToString(),
                    BankNameEng = dr["BANKNAMEENG"].ToString(),
                    BankCode = dr["BANKCODE"].ToString(),
                    AccountName = dr["ACCOUNTNAME"].ToString(),
                    AccountNameEng = dr["ACCOUNTNAMEENG"].ToString(),
                    AccountNum = dr["ACCOUNTNUM"].ToString(),
                    CreateDate = dr["CREATEDATE"].ParseDateTime()
                });
            }

            ViewBag.ObjList = result;

            return View();
        }
        /// <summary>
        /// 友善連結
        /// </summary>
        /// <returns></returns>
        public ActionResult FavouriteLink()
        {
            SqlCommand sql = new SqlCommand("SELECT SN, NAME, LINK, SORT, CREATEDATE FROM FAVOURITELINK ORDER BY SN DESC");
            DataTable dt = db.GetResult(sql);

            List<FavouriteLink> result = new List<FavouriteLink>();
            foreach (DataRow dr in dt.Rows)
            {
                result.Add(new FavouriteLink()
                {
                    Sn = dr["SN"].ParseInt(),
                    Name = dr["NAME"].ToString(),
                    Link = dr["LINK"].ToString(),
                    Sort = dr["SORT"].ParseInt(),
                    CreateDate = dr["CREATEDATE"].ParseDateTime()
                });
            }

            ViewBag.ObjList = result;

            return View();
        }
        /// <summary>
        /// 訂閱管理
        /// </summary>
        /// <returns></returns>
        public ActionResult Subscription()
        {
            SqlCommand sql = new SqlCommand("SELECT SN, MAIL, CREATEDATE FROM SUBSCRIPTION ORDER BY SN DESC");
            DataTable dt = db.GetResult(sql);

            List<Subscription> result = new List<Subscription>();
            foreach (DataRow dr in dt.Rows)
            {
                result.Add(new Subscription()
                {
                    Sn = dr["SN"].ParseInt(),
                    Mail = dr["MAIL"].ToString(),
                    CreateDate = dr["CREATEDATE"].ParseDateTime()
                });
            }

            ViewBag.ObjList = result;

            return View();
        }
        /// <summary>
        /// 檔案下載
        /// </summary>
        /// <returns></returns>
        public ActionResult Download()
        {
            SqlCommand sql = new SqlCommand("SELECT SN, FILENAME, CREATEDATE FROM DOWNLOAD ORDER BY SN DESC");
            DataTable dt = db.GetResult(sql);

            List<Download> result = new List<Download>();
            foreach (DataRow dr in dt.Rows)
            {
                string fPdf = Directory.GetFiles(Server.MapPath($"~/Mime/Download/{dr["SN"].ParseInt()}"), "p_*").FirstOrDefault();
                string fWord = Directory.GetFiles(Server.MapPath($"~/Mime/Download/{dr["SN"].ParseInt()}"), "w_*").FirstOrDefault();
                result.Add(new Download()
                {
                    Sn = dr["SN"].ParseInt(),
                    FileName = dr["FILENAME"].ToString(),
                    CreateDate = dr["CREATEDATE"].ParseDateTime(),
                    ExistsPdf = !string.IsNullOrEmpty(fPdf),
                    ExistsWord = !string.IsNullOrEmpty(fWord)
                });
            }

            ViewBag.ObjList = result;

            return View();
        }
        /// <summary>
        /// 名單管理
        /// </summary>
        /// <returns></returns>
        public ActionResult Member()
        {
            SqlCommand sql = new SqlCommand("SELECT ID, AREAID, NAME FROM MEMBER ORDER BY ID");
            DataTable dt = db.GetResult(sql);

            List<Member> result = new List<Member>();
            foreach (DataRow dr in dt.Rows)
            {
                result.Add(new Member()
                {
                    Id = dr["ID"].ToString(),
                    AreaId = dr["AREAID"].ParseInt(),
                    Name = dr["NAME"].ToString()
                });
            }
            ViewBag.ObjList = result;

            sql = new SqlCommand("SELECT ID, NAME FROM AREA ORDER BY ID");
            dt = db.GetResult(sql);

            List<Area> areas = new List<Area>();
            foreach (DataRow dr in dt.Rows)
            {
                areas.Add(new Area()
                {
                    Id = dr["ID"].ParseInt(),
                    Name = dr["NAME"].ToString()
                });
            }
            ViewBag.AreaList = areas;

            return View();
        }
        /// <summary>
        /// 捐款紀錄
        /// </summary>
        /// <returns></returns>
        public ActionResult DonateRecord()
        {
            SqlCommand sql = new SqlCommand(@"
            SELECT 
	            O.ORDERID, 
                O.ORDERTYPEID,
	            OT.NAME ORDERTYPE, 
	            O.BUYERNAME, 
                O.ORDERSTATUS ORDERSTATUSID,
	            OS.NAME ORDERSTATUS,
                O.PAYSTATUS PAYSTATUSID,
	            PS.NAME PAYSTATUS,
	            O.PAYTYPE PAYTYPEID,
	            PT.NAME PAYTYPE,
                O.PAYDATE,
				Br.[Name] BranchName,
                O.ISMANUAL
            FROM ORDERMASTER O
            JOIN ORDERTYPE OT ON O.ORDERTYPEID = OT.ID
            JOIN ORDERSTATUS OS ON O.ORDERSTATUS = OS.ID
            JOIN PAYSTATUS PS ON O.PAYSTATUS = PS.ID
            LEFT JOIN PAYTYPE PT ON O.PAYTYPE = PT.ID
            JOIN Branch Br ON O.[DonateUnit] = Br.Sn
            WHERE IsManual = 'N'
            ORDER BY O.ORDERID DESC, O.PayDate DESC");
            DataTable dt = db.GetResult(sql);

            List<DonateRecord> result = new List<DonateRecord>();
            foreach (DataRow dr in dt.Rows)
            {
                result.Add(new DonateRecord()
                {
                    OrderId = dr["ORDERID"].ToString(),
                    OrderTypeId = dr["ORDERTYPEID"].ParseInt(),
                    OrderType = dr["ORDERTYPE"].ToString(),
                    BuyerName = dr["BUYERNAME"].ToString(),
                    OrderStatusId = dr["ORDERSTATUSID"].ParseInt(),
                    OrderStatus = dr["ORDERSTATUS"].ToString(),
                    PayStatusId = dr["PAYSTATUSID"].ParseInt(),
                    PayStatus = dr["PAYSTATUS"].ToString(),
                    PayTypeId = dr["PAYTYPEID"].ParseInt(),
                    PayType = dr["PAYTYPE"].ToString(),
                    PayDate = dr["PAYDATE"].ParseDateTime().ToString("yyyy/MM/dd").Replace("0001/01/01",""),
                    BranchName = dr["BranchName"].ToString(),
                    IsManual = dr["ISMANUAL"].ToString()
                });
            }
            ViewBag.ObjList = result;

            return View();
        }

        //public void GetUser()
        //{
        //    if (HttpContext.Current.User.Identity.IsAuthenticated)
        //    {
        //        FormsIdentity id = (FormsIdentity)HttpContext.Current.User.Identity;
        //    }
        //}
    }
}