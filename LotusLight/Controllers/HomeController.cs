using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ecSqlDBManager;
using ecDataParse;
using LotusLight.Models;
using System.IO;
using Newtonsoft.Json;
using System.Text.RegularExpressions;

namespace LotusLight.Controllers
{
    public class HomeController : Controller
    {
        protected static string ConnString = ConfigurationManager.AppSettings["DbConnection"];
        protected static string MimePath = ConfigurationManager.AppSettings["MimePath"];
        protected DataBase db = new DataBase(ConnString);

        // 首頁
        public ActionResult Index()
        {
            // 輪播
            SqlCommand sql = new SqlCommand("SELECT SN, IMGNAME, TARGETLINK FROM CAROUSEL ORDER BY SORT");
            DataTable dt = db.GetResult(sql);

            List<Carousel> carousel = new List<Carousel>();
            foreach (DataRow dr in dt.Rows)
            {
                carousel.Add(new Carousel()
                {
                    Sn = dr["SN"].ParseInt(),
                    ImgName = dr["IMGNAME"].ToString(),
                    TargetLink = dr["TARGETLINK"].ToString()
                });
            }

            ViewBag.CarouselList = carousel;

            // 最新消息
            sql = new SqlCommand("SELECT TOP 6 SN, TITLE, ISTOP, ARTICLEDATE, CREATEDATE FROM NEWS ORDER BY ISTOP DESC, ARTICLEDATE DESC");
            dt = db.GetResult(sql);

            List<News> news = new List<News>();
            foreach (DataRow dr in dt.Rows)
            {
                news.Add(new News()
                {
                    Sn = dr["SN"].ParseInt(),
                    Title = dr["TITLE"].ToString(),
                    IsTop = dr["ISTOP"].ToString(),
                    ArticleDate = dr["ARTICLEDATE"].ParseDateTime(),
                    CreateDate = dr["CREATEDATE"].ParseDateTime()
                });
            }

            ViewBag.NewsList = news;

            // 最新活動
            sql = new SqlCommand("SELECT TOP 3 SN, TITLE, ISTOP, ARTICLEDATE, CREATEDATE FROM ACTIVITY ORDER BY ISTOP DESC, ARTICLEDATE DESC");
            dt = db.GetResult(sql);

            List<Activity> activitys = new List<Activity>();
            foreach (DataRow dr in dt.Rows)
            {
                activitys.Add(new Activity()
                {
                    Sn = dr["SN"].ParseInt(),
                    Title = dr["TITLE"].ToString(),
                    IsTop = dr["ISTOP"].ToString(),
                    ArticleDate = dr["ARTICLEDATE"].ParseDateTime(),
                    CreateDate = dr["CREATEDATE"].ParseDateTime()
                });
            }

            ViewBag.ActivityList = activitys;

            SetBranchList();
            return View();
        }

        // 認識華光
        public ActionResult About()
        {
            // 會員列表
            SqlCommand sql = new SqlCommand("SELECT ID, AREAID, NAME FROM MEMBER ORDER BY ID");
            DataTable dt = db.GetResult(sql);

            List<Member> members = new List<Member>();
            foreach (DataRow dr in dt.Rows)
            {
                members.Add(new Member()
                {
                    AreaId = dr["AREAID"].ParseInt(),
                    Id = dr["ID"].ToString(),
                    Name = dr["NAME"].ToString()
                });
            }

            ViewBag.AList = members.Where(x => x.AreaId.Equals(0)).ToList();
            ViewBag.NList = members.Where(x => x.AreaId.Equals(1)).ToList();
            ViewBag.CList = members.Where(x => x.AreaId.Equals(2)).ToList();
            ViewBag.SList = members.Where(x => x.AreaId.Equals(3)).ToList();
            ViewBag.OList = members.Where(x => x.AreaId.Equals(4)).ToList();

            ViewBag.Obj = JsonConvert.SerializeObject(members);

            SetBranchList();
            return View();
        }

        // 華光服務
        public ActionResult Service()
        {
            SetBranchList();
            return View();
        }

        #region 華光事蹟
        public ActionResult Events(string id)
        {
            ViewBag.Anchor = "N0";

            if (!string.IsNullOrEmpty(id))
                ViewBag.Anchor = id;

            SetBranchList();
            return View();
        }

        public ActionResult News()
        {
            // 最新消息
            SqlCommand sql = new SqlCommand("SELECT SN, TITLE, ISTOP, ARTICLEDATE, CREATEDATE FROM NEWS ORDER BY ISTOP DESC, ARTICLEDATE DESC");
            DataTable dt = db.GetResult(sql);

            List<News> news = new List<News>();
            foreach (DataRow dr in dt.Rows)
            {
                news.Add(new News()
                {
                    Sn = dr["SN"].ParseInt(),
                    Title = dr["TITLE"].ToString(),
                    IsTop = dr["ISTOP"].ToString(),
                    ArticleDate = dr["ARTICLEDATE"].ParseDateTime(),
                    CreateDate = dr["CREATEDATE"].ParseDateTime()
                });
            }
            return View(news);
        }

        public ActionResult GetNews(string id)
        {
            // 最新消息
            SqlCommand sql = new SqlCommand("SELECT TITLE, ARTICLEDATE, CONTENT1, CONTENT2, VIDEOLINK1, VIDEOLINK2 FROM NEWS WHERE SN = @SN");
            sql.Parameters.AddWithValue("@SN", id);
            DataTable dt = db.GetResult(sql);

            ViewBag.IsNews = "Y";

            ViewBag.Title = dt.Rows[0]["TITLE"].ToString();
            ViewBag.ArticleDate = dt.Rows[0]["ARTICLEDATE"].ParseDateTime().ToString("yyyy/MM/dd");
            ViewBag.P1 = dt.Rows[0]["CONTENT1"].ToString().Replace("\r\n", "<br>");
            ViewBag.P2 = dt.Rows[0]["CONTENT2"].ToString().Replace("\r\n", "<br>");
            ViewBag.V1 = dt.Rows[0]["VIDEOLINK1"].ToString();
            ViewBag.V2 = dt.Rows[0]["VIDEOLINK2"].ToString();

            // 圖片SN
            sql = new SqlCommand("SELECT SN FROM NEWSIMAGE WHERE NEWSID = @SN ORDER BY SN");
            sql.Parameters.AddWithValue("@SN", id);
            dt = db.GetResult(sql);
            ViewBag.Images = "";
            if (dt.Rows.Count > 0)
            {
                ViewBag.Images = string.Join(";", dt.AsEnumerable().Select(x => x.Field<int>("SN").ToString()).ToArray());
            }

            // 上/下則SN
            sql = new SqlCommand(@"SELECT TOP 1 SN FROM NEWS WHERE ARTICLEDATE > @DATE ORDER BY ARTICLEDATE");
            sql.Parameters.AddWithValue("@DATE", ViewBag.ArticleDate);
            dt = db.GetResult(sql);

            if (dt.Rows.Count > 0)
                ViewBag.Prev = dt.Rows[0]["SN"].ToString();

            sql = new SqlCommand(@"SELECT TOP 1 SN FROM NEWS WHERE ARTICLEDATE < @DATE ORDER BY ARTICLEDATE DESC");
            sql.Parameters.AddWithValue("@DATE", ViewBag.ArticleDate);
            dt = db.GetResult(sql);

            if (dt.Rows.Count > 0)
                ViewBag.Next = dt.Rows[0]["SN"].ToString();

            return View("EventContent");
        }

        public ActionResult Activity()
        {
            // 最新活動
            SqlCommand sql = new SqlCommand("SELECT SN, TITLE, ISTOP, ARTICLEDATE, CREATEDATE FROM ACTIVITY ORDER BY ISTOP DESC, ARTICLEDATE DESC");
            DataTable dt = db.GetResult(sql);

            List<Activity> activitys = new List<Activity>();
            foreach (DataRow dr in dt.Rows)
            {
                activitys.Add(new Activity()
                {
                    Sn = dr["SN"].ParseInt(),
                    Title = dr["TITLE"].ToString(),
                    IsTop = dr["ISTOP"].ToString(),
                    ArticleDate = dr["ARTICLEDATE"].ParseDateTime(),
                    CreateDate = dr["CREATEDATE"].ParseDateTime()
                });
            }
            return View(activitys);
        }

        public ActionResult GetActivity(string id)
        {
            // 最新消息
            SqlCommand sql = new SqlCommand("SELECT TITLE, ARTICLEDATE, CONTENT1, CONTENT2 FROM ACTIVITY WHERE SN = @SN");
            sql.Parameters.AddWithValue("@SN", id);
            DataTable dt = db.GetResult(sql);

            ViewBag.IsNews = "N";

            ViewBag.Title = dt.Rows[0]["TITLE"].ToString();
            ViewBag.ArticleDate = dt.Rows[0]["ARTICLEDATE"].ParseDateTime().ToString("yyyy/MM/dd");
            ViewBag.P1 = dt.Rows[0]["CONTENT1"].ToString().Replace("\r\n", "<br>");
            ViewBag.P2 = dt.Rows[0]["CONTENT2"].ToString().Replace("\r\n", "<br>");
            ViewBag.V1 = "";
            ViewBag.V2 = "";

            // 圖片SN
            sql = new SqlCommand("SELECT SN FROM ACTIVITYIMAGE WHERE ACTIVITYID = @SN ORDER BY SN");
            sql.Parameters.AddWithValue("@SN", id);
            dt = db.GetResult(sql);
            ViewBag.Images = "";
            if (dt.Rows.Count > 0)
            {
                ViewBag.Images = string.Join(";", dt.AsEnumerable().Select(x => x.Field<int>("SN").ToString()).ToArray());
            }

            // 上/下則SN
            sql = new SqlCommand(@"SELECT TOP 1 SN FROM ACTIVITY WHERE ARTICLEDATE > @DATE ORDER BY ARTICLEDATE");
            sql.Parameters.AddWithValue("@DATE", ViewBag.ArticleDate);
            dt = db.GetResult(sql);

            if (dt.Rows.Count > 0)
                ViewBag.Prev = dt.Rows[0]["SN"].ToString();

            sql = new SqlCommand(@"SELECT TOP 1 SN FROM ACTIVITY WHERE ARTICLEDATE < @DATE ORDER BY ARTICLEDATE DESC");
            sql.Parameters.AddWithValue("@DATE", ViewBag.ArticleDate);
            dt = db.GetResult(sql);

            if (dt.Rows.Count > 0)
                ViewBag.Next = dt.Rows[0]["SN"].ToString();

            return View("EventContent");
        }
        #endregion 華光事蹟

        // 捐款方式
        public ActionResult Donate()
        {
            SetBranchList();
            return View();
        }

        // 結緣捐贈
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

            SetBranchList();
            return View();
        }

        // 華光招募
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
                    P1 = dr["CONTENT1"].ToString().Replace("\r\n", "<br>"),
                    P2 = dr["CONTENT2"].ToString().Replace("\r\n", "<br>"),
                    CreateDate = dr["CREATEDATE"].ParseDateTime()
                });
            }

            ViewBag.ObjList = result;

            SetBranchList();
            return View();
        }

        #region 影音專區
        public ActionResult Video()
        {
            SqlCommand sql = new SqlCommand("SELECT SN, NAME FROM VIDEOTYPE ORDER BY SN DESC");
            DataTable dt = db.GetResult(sql);

            List<VideoType> result = new List<VideoType>();
            foreach (DataRow dr in dt.Rows)
            {
                result.Add(new VideoType()
                {
                    Sn = dr["SN"].ParseInt(),
                    Name = dr["NAME"].ToString()
                });
            }

            ViewBag.ObjList = result;

            SetBranchList();
            return View();
        }

        public ActionResult VideoContent(string id)
        {
            SqlCommand sql = new SqlCommand("SELECT SN, NAME, CONTENT1, CONTENT2, LINK FROM VIDEO WHERE TYPEID = @TYPEID ORDER BY SN DESC");
            sql.Parameters.AddWithValue("@TYPEID", id);
            DataTable dt = db.GetResult(sql);

            List<Video> result = new List<Video>();
            foreach (DataRow dr in dt.Rows)
            {
                result.Add(new Video()
                {
                    Sn = dr["SN"].ParseInt(),
                    Name = dr["NAME"].ToString(),
                    P1 = dr["CONTENT1"].ToString().Replace("\r\n", "<br>"),
                    P2 = dr["CONTENT2"].ToString().Replace("\r\n", "<br>"),
                    Lnk = dr["LINK"].ToString()
                });
            }

            //ViewBag.ObjList = result;

            SetBranchList();
            return View(result);
        }
        #endregion 影音專區

        // 連絡我們
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

            SetBranchList();
            return View();
        }

        // 連絡我們
        public ActionResult FavouriteLink()
        {
            SqlCommand sql = new SqlCommand("SELECT SN, NAME, LINK, SORT, CREATEDATE FROM FAVOURITELINK ORDER BY SORT DESC, SN DESC");
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

            SetBranchList();
            return View();
        }

        // 善捐查詢Donations.cshtml已取消連ObjList
        public ActionResult Donations()
        {
            //SqlCommand sql = new SqlCommand("SELECT BUYERNAME, PAYDATE, DONATEAMOUNT, RECEIPTADDRESS FROM ORDERMASTER WHERE NEEDANONYMOUS = 'Y' AND PAYDATE IS NOT NULL ORDER BY PAYDATE DESC");
            //SqlCommand sql = new SqlCommand("SELECT BUYERNAME, PAYDATE, DONATEAMOUNT, RECEIPTADDRESS FROM ORDERMASTER WHERE ISMANUAL = 'Y' ORDER BY PAYDATE DESC");
           // SqlCommand sql = new SqlCommand("SELECT BUYERNAME, PAYDATE, DONATEAMOUNT, DonateFor FROM ORDERMASTER WHERE ISMANUAL = 'Y' ORDER BY LEN(BUYERNAME)");
             SqlCommand sql = new SqlCommand("SELECT BUYERNAME, PAYDATE, DONATEAMOUNT, DonateFor FROM ORDERMASTER WHERE ISMANUAL = 'Y' ORDER BY PAYDATE DESC");

            DataTable dt = db.GetResult(sql);

            List<Donations> result = new List<Donations>();
            foreach (DataRow dr in dt.Rows)
            {
                result.Add(new Donations()
                {
                    //Name = dr["BUYERNAME"].ToString().Substring(0, 1) + "○" + dr["BUYERNAME"].ToString().Substring(2),
                    //Name = HideName( dr["BUYERNAME"].ToString() ),
                    //Name = MaskValue (dr["BUYERNAME"].ToString()),
                    Name = MaskValue (dr["BUYERNAME"].ToString()),
                    Name_data = dr["BUYERNAME"].ToString(),
                    DonateFor = dr["DonateFor"].ToString(),
                    Amount = dr["DONATEAMOUNT"].ToString(),
                    //Addr = dr["RECEIPTADDRESS"].ToString(),
                    //Name = dr["BUYERNAME"].ToString(),
                    PayDate = dr["PAYDATE"].ParseDateTime()
                });
            }

            ViewBag.ObjList = result;

            SetBranchList();
            return View();
        }
        private string MaskName(string val)
        {
            string maskstr, maskchar;
            maskchar = null;
            //符合英文的模式
            if (Regex.IsMatch(val, "[A-Za-z]"))
            {
                if (val.IndexOf("-") > 1)
                {
                    maskstr = val.Split('-')[1];        //將第二個陣列取代*
                    val = val.Replace(maskstr, "*");
                }
                if (val.IndexOf(" ") > 1)
                {
                    maskstr = val.Split(' ')[1];
                    val = val.Replace(maskstr, "*");
                }
            }
            else if (val.Length > 1)
            {
                //中文
                int End = (int)(val.Length / 2);
                maskstr = val.Substring(1, End);        //算要取代的字數，6愈多6/2=3，Substring(1, 3)，第234個字取代O
                for (int i = 0; i < maskstr.Length; i++)
                {
                    maskchar = maskchar + "O";
                }
                val = val.Replace(maskstr, maskchar);  //將maskstr要取代的文字，取代例123456，234取代成OOO,1OOO56
            }
            return val;
        }
        private string MaskValue(string _val)
        {
            //string _val = "";
            //string[] Rows = _val.Split(new char[2] { '.', '、' }, StringSplitOptions.RemoveEmptyEntries); 可以將空的陣列排除
            string[] Rows = _val.Split(new char[2] { '.', '、' });  //利用string的split做法，但可以針對多個字串做分割
            string _result = string.Empty;
            foreach (string _i in Rows)
            {
                //Console.WriteLine(MaskName(_i).ToString());
                if (_result == string.Empty)
                {
                    _result = MaskName(_i.ToString());
                }
                else
                {
                    _result = _result + "、" + MaskName(_i.ToString());
                }
            }
            return _result;
        }


        //public static string HideName(string username)      //加*
        //{
        //    int strlen = username.Length / 2;
        //    return username.Replace(username.Substring(strlen, 1), "*");
        //}

        // 下載專區
        public ActionResult Download()
        {
            string MimePath = ConfigurationManager.AppSettings["MimePath"];
            SqlCommand sql = new SqlCommand("SELECT SN, FILENAME FROM DOWNLOAD ORDER BY SN");
            DataTable dt = db.GetResult(sql);

            List<Download> result = new List<Download>();
            foreach (DataRow dr in dt.Rows)
            {
                string fDir = MimePath.IndexOf("\\") > -1 ? $"{MimePath}\\Download\\{dr["SN"].ToString()}" : $"{MimePath}//Download//{dr["SN"].ToString()}";
                string fPdf = Directory.GetFiles(fDir, "p_*").FirstOrDefault();
                string fWord = Directory.GetFiles(fDir, "w_*").FirstOrDefault();
                result.Add(new Download()
                {
                    Sn = dr["SN"].ParseInt(),
                    FileName = dr["FILENAME"].ToString(),
                    ExistsPdf = !string.IsNullOrEmpty(fPdf),
                    ExistsWord = !string.IsNullOrEmpty(fWord)
                });
            }

            ViewBag.ObjList = result;

            SetBranchList();
            return View();
        }

        // 取得虛擬帳號
        public ActionResult Receive()
        {
            Receive model = new Receive();

            if (Request.Form.HasKeys())
            {
                string param = string.Empty;
                foreach (string key in Request.Form.AllKeys)
                {
                    param += $"{key}={Request.Form[key]};";
                }

                ecLog.Log.Record(ecLog.Log.InfoType.Normal, param, nameof(Receive));

                string payType = Request.Form["Pay_zg"];
                string orderNo = Request.Form["OrderNo"];
                string amount = Request.Form["Amount"];

                string atmBankNo = Request.Form["AtmBankNo"] ?? string.Empty;
                string atmNo = Request.Form["AtmNo"] ?? string.Empty;
                string cvsno = Request.Form["CvsNo"] ?? string.Empty;

                string payEndDate = Request.Form["PayEndDate"];
                string code = Request.Form["Code"];
                string authCode = Request.Form["AuthCode"];
                string message = string.Empty;

                string payCode = string.Empty;
                if (payType.Equals("2"))
                {
                    payCode = $"{atmBankNo}-{atmNo}";
                    message = Request.Form["Message"];
                }
                else
                {
                    payCode = cvsno;
                    message = Request.Form["Desc"];
                }

                SqlCommand sql = new SqlCommand("SELECT NAME FROM PAYTYPE WHERE ID = @ID");
                sql.Parameters.AddWithValue("@ID", payType);
                DataTable dt = db.GetResult(sql);

                model.PayTypeName = dt.Rows.Count > 0 ? dt.Rows[0][0].ToString() : string.Empty;

                if (code.Trim().Equals("000"))
                {
                    sql = new SqlCommand($"UPDATE ORDERMASTER SET PAYCODE = '{payCode}', PAYDEADLINE = '{payEndDate}' WHERE ORDERID = @ORDERID");
                    sql.Parameters.AddWithValue("@ORDERID", orderNo);
                    db.OpenConn();
                    db.Execute(sql);
                    db.CloseConn();
                }

                model.PayType = payType;
                model.OrderNo = orderNo;

                if (model.PayType.Equals("2"))
                {
                    model.BankNo = atmBankNo;
                    model.AtmNo = string.Empty;
                    for (int i = 0; i < atmNo.Length; i++)
                    {
                        if (i > 0 && i % 4 == 0)
                            model.AtmNo += " ";
                        model.AtmNo += atmNo[i];
                    }
                }
                else
                    model.PayCode = payCode;

                model.Amount = amount.ParseInt();
                model.Code = code;
                model.Message = message;
                model.PayEndDate = payEndDate;
            }

            return View(model);
        }

        // 付款結果
        public ActionResult Return()
        {
            Return model = new Return();

            if (Request.Form.HasKeys())
            {
                string param = string.Empty;
                foreach (string key in Request.Form.AllKeys)
                {
                    param += $"{key}={Request.Form[key]};";
                }

                ecLog.Log.Record(ecLog.Log.InfoType.Normal, param, nameof(Receive));

                string orderNo = Request.Form["OrderNo"];
                //string amount = Request.Form["Amount"];
                string authAmount = Request.Form["AuthAmount"];
                string authTime = Request.Form["AuthTime"];
                string code = Request.Form["Code"];
                string message = Request.Form["Message"];
                string paymentNo = Request.Form["Payment_no"];

                if (code.Trim().Equals("000"))
                {
                    SqlCommand sql = new SqlCommand("SELECT ORDERTYPEID FROM ORDERMASTER WHERE ORDERID = @ORDERID");
                    sql.Parameters.AddWithValue("@ORDERID", orderNo);
                    DataTable dt = db.GetResult(sql);

                    if (dt.Rows.Count > 0)
                    {
                        // 捐款不用寄送結緣品，所以直接轉結案
                        if (dt.Rows[0]["ORDERTYPEID"].ParseInt() == 1)
                            sql = new SqlCommand($"UPDATE ORDERMASTER SET ORDERSTATUS = 2, PAYSTATUS = 2, PAYAMOUNT = {authAmount}, PAYDATE = '{authTime}' WHERE ORDERID = @ORDERID");
                        else
                            sql = new SqlCommand($"UPDATE ORDERMASTER SET PAYSTATUS = 2, PAYAMOUNT = {authAmount}, PAYDATE = '{authTime}' WHERE ORDERID = @ORDERID");
                        //if (dt.Rows[0]["ORDERTYPEID"].ParseInt() == 1)
                        //    sql = new SqlCommand($"UPDATE ORDERMASTER SET ORDERSTATUS = 2, PAYSTATUS = 2, PAYDATE = '{authTime}' WHERE ORDERID = @ORDERID");
                        //else
                        //    sql = new SqlCommand($"UPDATE ORDERMASTER SET PAYSTATUS = 2, PAYDATE = '{authTime}' WHERE ORDERID = @ORDERID");

                        sql.Parameters.AddWithValue("@ORDERID", orderNo);
                        db.OpenConn();
                        db.Execute(sql);
                        db.CloseConn();
                    }
                }

                model.OrderNo = orderNo;
                model.Amount = authAmount.ParseInt();
                //model.AuthAmount = authAmount.ParseInt();
                model.AuthTime = authTime;
                model.Code = code;
                model.Message = message;
            }

            return View(model);
        }

        private void SetBranchList()
        {
            SqlCommand sql = new SqlCommand(@"
                    SELECT 
                        SN, NAME, ADDRESS, ADDRESSENG, TEL, FAX, FBLINK, 
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

            ViewBag.BranchList = result;
        }
    }
}