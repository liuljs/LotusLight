using ecSqlDBManager;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Web;
using System.Web.Mvc;
using ecDataParse;
using LotusLight.Models;
using Newtonsoft.Json;
using System.Text.RegularExpressions;

namespace LotusLight.Controllers
{
    public class ActController : Controller
    {
        protected static string ConnString = ConfigurationManager.AppSettings["DbConnection"];
        protected static string MimePath = ConfigurationManager.AppSettings["MimePath"];
        protected DataBase db = new DataBase(ConnString);

        #region 各種取圖
        public FileResult GetCarouselImage(string id)
        {
            SqlCommand sql = new SqlCommand("SELECT IMGNAME FROM CAROUSEL WHERE SN = @SN");
            sql.Parameters.AddWithValue("@SN", id);
            DataTable dt = db.GetResult(sql);
            DataRow dr = dt.Rows[0];

            string file = MimePath.IndexOf("\\") > -1 ? $"{MimePath}\\Carousel\\{dr["IMGNAME"].ToString()}" : $"{MimePath}//Carousel//{dr["IMGNAME"].ToString()}";

            if (!System.IO.File.Exists(file))
                return null;
            else
                return new FileStreamResult(new FileStream(file, FileMode.Open), "image/jpeg");
        }

        public FileResult GetRelatedImage(string id)
        {
            SqlCommand sql = new SqlCommand("SELECT TITLEIMGNAME FROM RELATED WHERE SN = @SN");
            sql.Parameters.AddWithValue("@SN", id);
            DataTable dt = db.GetResult(sql);
            DataRow dr = dt.Rows[0];

            string file = MimePath.IndexOf("\\") > -1 ? $"{MimePath}\\Related\\{id}\\{dr["TITLEIMGNAME"].ToString()}" : $"{MimePath}//Related//{id}//{dr["TITLEIMGNAME"].ToString()}";

            if (!System.IO.File.Exists(file))
                return null;
            else
                return new FileStreamResult(new FileStream(file, FileMode.Open), "image/jpeg");
        }

        public FileResult GetNewsImage(string id)
        {
            SqlCommand sql = new SqlCommand("SELECT TITLEIMGNAME FROM NEWS WHERE SN = @SN");
            sql.Parameters.AddWithValue("@SN", id);
            DataTable dt = db.GetResult(sql);
            DataRow dr = dt.Rows[0];

            string file = MimePath.IndexOf("\\") > -1 ? $"{MimePath}\\News\\{id}\\{dr["TITLEIMGNAME"].ToString()}" : $"{MimePath}//News//{id}//{dr["TITLEIMGNAME"].ToString()}";

            if (!System.IO.File.Exists(file))
                return null;
            else
                return new FileStreamResult(new FileStream(file, FileMode.Open), "image/jpeg");
        }

        public FileResult GetNewsContentImage(string id)
        {
            SqlCommand sql = new SqlCommand("SELECT NEWSID, IMGNAME FROM NEWSIMAGE WHERE SN = @SN");
            sql.Parameters.AddWithValue("@SN", id);
            DataTable dt = db.GetResult(sql);
            DataRow dr = dt.Rows[0];

            string file = MimePath.IndexOf("\\") > -1 ? $"{MimePath}\\News\\{dr["NEWSID"].ToString()}\\{dr["IMGNAME"].ToString()}" : $"{MimePath}//News//{dr["NEWSID"].ToString()}//{dr["IMGNAME"].ToString()}";

            if (!System.IO.File.Exists(file))
                return null;
            else
                return new FileStreamResult(new FileStream(file, FileMode.Open), "image/jpeg");
        }

        public FileResult GetActivityImage(string id)
        {
            SqlCommand sql = new SqlCommand("SELECT TITLEIMGNAME FROM ACTIVITY WHERE SN = @SN");
            sql.Parameters.AddWithValue("@SN", id);
            DataTable dt = db.GetResult(sql);
            DataRow dr = dt.Rows[0];

            string file = MimePath.IndexOf("\\") > -1 ? $"{MimePath}\\Activity\\{id}\\{dr["TITLEIMGNAME"].ToString()}" : $"{MimePath}//Activity//{id}//{dr["TITLEIMGNAME"].ToString()}";

            if (!System.IO.File.Exists(file))
                return null;
            else
                return new FileStreamResult(new FileStream(file, FileMode.Open), "image/jpeg");
        }

        public FileResult GetActivityContentImage(string id)
        {
            SqlCommand sql = new SqlCommand("SELECT ACTIVITYID, IMGNAME FROM ACTIVITYIMAGE WHERE SN = @SN");
            sql.Parameters.AddWithValue("@SN", id);
            DataTable dt = db.GetResult(sql);
            DataRow dr = dt.Rows[0];

            string file = MimePath.IndexOf("\\") > -1 ? $"{MimePath}\\Activity\\{dr["ACTIVITYID"].ToString()}\\{dr["IMGNAME"].ToString()}" : $"{MimePath}//Activity//{dr["ACTIVITYID"].ToString()}//{dr["IMGNAME"].ToString()}";

            if (!System.IO.File.Exists(file))
                return null;
            else
                return new FileStreamResult(new FileStream(file, FileMode.Open), "image/jpeg");
        }

        public FileResult GetRecruitImage(string id)
        {
            SqlCommand sql = new SqlCommand("SELECT TITLEIMGNAME FROM RECRUIT WHERE SN = @SN");
            sql.Parameters.AddWithValue("@SN", id);
            DataTable dt = db.GetResult(sql);
            DataRow dr = dt.Rows[0];

            string file = MimePath.IndexOf("\\") > -1 ? $"{MimePath}\\Recruit\\{id}\\{dr["TITLEIMGNAME"].ToString()}" : $"{MimePath}//Recruit//{id}//{dr["TITLEIMGNAME"].ToString()}";

            if (!System.IO.File.Exists(file))
                return null;
            else
                return new FileStreamResult(new FileStream(file, FileMode.Open), "image/jpeg");
        }

        public FileResult GetVideoImage(string id)
        {
            SqlCommand sql = new SqlCommand("SELECT TITLEIMGNAME FROM VIDEO WHERE SN = @SN");
            sql.Parameters.AddWithValue("@SN", id);
            DataTable dt = db.GetResult(sql);
            DataRow dr = dt.Rows[0];

            string file = MimePath.IndexOf("\\") > -1 ? $"{MimePath}\\Video\\{id}\\{dr["TITLEIMGNAME"].ToString()}" : $"{MimePath}//Video//{id}//{dr["TITLEIMGNAME"].ToString()}";

            if (!System.IO.File.Exists(file))
                return null;
            else
                return new FileStreamResult(new FileStream(file, FileMode.Open), "image/jpeg");
        }
        #endregion

        /// <summary>
        /// 連絡我們
        /// </summary>
        /// <param name="form"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult SendMailToBranch(FormCollection form)
        {
            try
            {
                string unit = form["contactUnit"];
                string name = form["contactName"];
                string tel = form["contactTel"];
                string mail = form["contactMail"];
                string area = form["contactArea"];
                string content = form["contactContent"];

                SqlCommand sql = new SqlCommand("SELECT NAME, MAIL FROM BRANCH WHERE SN = @SN");
                sql.Parameters.AddWithValue("@SN", area);
                DataTable dt = db.GetResult(sql);

                string branchName = dt.Rows[0]["NAME"].ToString();
                string branchMail = dt.Rows[0]["MAIL"].ToString();

                ecMail.MailAccount sender = new ecMail.MailAccount(branchMail, name);
                ecMail.MailAccount recver = new ecMail.MailAccount(branchMail, branchName);

                string mailHost = ConfigurationManager.AppSettings["MailHost"];
                string mailUser = ConfigurationManager.AppSettings["MailUser"];
                string mailPass = ConfigurationManager.AppSettings["MailPass"];

                // Init Mail STMP & Sender
                SmtpClient smtp = new SmtpClient(mailHost, 25)
                {
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = false,
                    Credentials = new System.Net.NetworkCredential(mailUser, mailPass),
                    Host = mailHost
                };

                string html = string.Format(@"
                <p style='font-weight: 800;'><strong>親愛的 {0} 您好，您有一封來自官方網站「聯絡我們」的訊息：</strong></p>
                <br>
                <p><strong>聯絡單位：</strong>{1}</p>
                <p><strong>姓名（公司）：</strong>{2}</p>
                <p><strong>聯絡電話：</strong>{3}</p>
                <p><strong>電子信箱：</strong>{4}</p>
                <p><strong>聯絡內容：</strong></p>
                <p>{5}</p>
                ", branchName, unit, name, tel, mail, content.Replace("\r\n", "<br>"));

                ecMail.Mail.SendMail(smtp, sender, new ecMail.MailAccount[] { recver }, new ecMail.MailAccount[0], "聯絡我們", html);

                return Content("OK");
            }
            catch (Exception e)
            {
                ecLog.Log.Record(ecLog.Log.InfoType.Error, $"{e.Message};{e.StackTrace}", nameof(SendMailToBranch));
                return Content("NG");
            }
        }

        /// <summary>
        /// 訂閱
        /// </summary>
        /// <param name="form"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult AddSubscription(FormCollection form)
        {
            string mail = form["subMmail"];
            try
            {
                if (!string.IsNullOrEmpty(mail))
                {
                    SqlCommand sql = new SqlCommand("SELECT TOP 1 SN FROM SUBSCRIPTION WHERE MAIL = @MAIL");
                    sql.Parameters.AddWithValue("@MAIL", mail);
                    DataTable dt = db.GetResult(sql);

                    if (dt.Rows.Count > 0)
                        return Content("EX");
                    else
                    {
                        db.OpenConn();
                        sql = new SqlCommand("INSERT INTO SUBSCRIPTION (MAIL) VALUES (@MAIL)");
                        sql.Parameters.AddWithValue("@MAIL", mail);
                        db.Execute(sql);
                        db.CloseConn();
                    }
                }
                return Content("OK");
            }
            catch (Exception e)
            {
                ecLog.Log.Record(ecLog.Log.InfoType.Error, $"{e.Message};{e.StackTrace}", nameof(AddSubscription));
                return Content("NG");
            }

        }

        public FileContentResult GetDownload(string sn, string opt)
        {
            string name = opt.Equals("word") ? $"w_{sn}" : opt.Equals("pdf") ? $"p_{sn}" : "";
            string fileDir = MimePath.IndexOf("\\") > -1 ? $"{MimePath}\\Download\\{sn}" : $"{MimePath}//Download//{sn}";

            if (string.IsNullOrEmpty(name))
                return File(new byte[0], "application/octet-stream");

            string[] file = Directory.GetFiles(fileDir, $"{name}.*", SearchOption.TopDirectoryOnly);

            if (file.Length == 0)
                return File(new byte[0], "application/octet-stream");
            else
            {
                string downloadName = Path.GetFileName(file[0]);
                FileStream fs = new FileStream(file[0], FileMode.Open, FileAccess.Read);
                byte[] f = new byte[fs.Length];
                fs.Read(f, 0, f.Length);
                return File(f, (opt.Equals("pdf") ? "application/pdf" : opt.Equals("word") && Path.GetExtension(file[0]).ToLower().Contains("docx") ? "application/vnd.openxmlformats-officedocument.wordprocessing" : "application/msword"), downloadName);
            }
        }

        /// <summary>
        /// 產生捐贈訂單
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult GoDonate()
        {
            string orderId = string.Empty;

            try
            {
                if (Request.Form.HasKeys())
                {
                    DateTime now = DateTime.Now;
                    DataBase db = new DataBase(ConfigurationManager.AppSettings["DbConnection"]);
                    SqlCommand sql = new SqlCommand("SELECT MAX(ORDERID) FROM ORDERMASTER WHERE ORDERID LIKE @ORDERID");
                    sql.Parameters.AddWithValue("@ORDERID", $"{now.ToString("yyyyMMdd")}%");
                    DataTable dt = db.GetResult(sql);

                    // 訂單編號
                    if (string.IsNullOrEmpty(dt.Rows[0][0].ToString()))
                        orderId = $"{now.ToString("yyyyMMdd")}001";
                    else
                    {
                        int newId = dt.Rows[0][0].ToString().Substring(dt.Rows[0][0].ToString().Length - 3).ParseInt() + 1;
                        orderId = $"{now.ToString("yyyyMMdd")}{newId.ToString().PadLeft(3, '0')}";
                    }

                    string orderTypeId = Request.Form["OrderTypeId"];
                    string buyerTypeId = Request.Form["BuyerTypeId"];
                    string buyerName = Request.Form["BuyerName"];
                    string buyerId = Request.Form["BuyerId"];
                    string buyerPhone = Request.Form["BuyerPhone"];
                    string buyerEmail = Request.Form["BuyerEmail"];
                    string payType = Request.Form["PayType"];
                    string donateUnit = Request.Form["DonateUnit"];
                    string amount = Request.Form["DonateAmount"];

                    string donatePurpose = Request.Form["DonateFor"] ?? string.Empty;
                    string needReceipt = Request.Form["NeedReceipt"] ?? string.Empty;
                    string receiptPostMethod = Request.Form["ReceiptPostMethod"] ?? string.Empty;
                    string receiptTitle = Request.Form["ReceiptTitle"] ?? string.Empty;

                    string pickupMethod = Request.Form["PickupMethod"] ?? string.Empty;
                    string remark = Request.Form["Remark"] ?? string.Empty;

                    string addr = Request.Form["ReceiptAddress"] ?? string.Empty;
                    string needAnonymous = Request.Form["NeedAnonymous"] ?? "Y";

                    sql = new SqlCommand(@"INSERT INTO ORDERMASTER 
                    (
                        ORDERID, ORDERDATE, ORDERSTATUS, PAYSTATUS, ORDERTYPEID, 
                        BUYERTYPEID, BUYERNAME, BUYERID, BUYERPHONE, BUYEREMAIL, 
                        PAYTYPE, DONATEUNIT, DONATEAMOUNT, DONATEFOR, 
                        NEEDRECEIPT, RECEIPTPOSTMETHOD, NEEDANONYMOUS, PICKUPMETHOD, REMARK, RECEIPTTITLE, RECEIPTADDRESS
                    ) VALUES (
                        @ORDERID, @ORDERDATE, @ORDERSTATUS, @PAYSTATUS, @ORDERTYPEID, 
                        @BUYERTYPEID, @BUYERNAME, @BUYERID, @BUYERPHONE, @BUYEREMAIL, 
                        @PAYTYPE, @DONATEUNIT, @DONATEAMOUNT, @DONATEFOR, 
                        @NEEDRECEIPT, @RECEIPTPOSTMETHOD, @NEEDANONYMOUS, @PICKUPMETHOD, @REMARK, @RECEIPTTITLE, @RECEIPTADDRESS
                    )");
                    sql.Parameters.AddWithValue("@ORDERID", orderId);
                    sql.Parameters.AddWithValue("@ORDERDATE", now.ToString("yyyy/MM/dd HH:mm:ss"));

                    if(orderTypeId.Equals("1"))
                        sql.Parameters.AddWithValue("@ORDERSTATUS", 2);
                    else
                        sql.Parameters.AddWithValue("@ORDERSTATUS", 1);

                    sql.Parameters.AddWithValue("@PAYSTATUS", 1);
                    sql.Parameters.AddWithValue("@ORDERTYPEID", orderTypeId);
                    sql.Parameters.AddWithValue("@BUYERTYPEID", buyerTypeId);
                    sql.Parameters.AddWithValue("@BUYERNAME", buyerName);
                    sql.Parameters.AddWithValue("@BUYERID", buyerId);
                    sql.Parameters.AddWithValue("@BUYERPHONE", buyerPhone);
                    sql.Parameters.AddWithValue("@BUYEREMAIL", buyerEmail);
                    sql.Parameters.AddWithValue("@PAYTYPE", payType.ParseInt());
                    sql.Parameters.AddWithValue("@DONATEUNIT", donateUnit);
                    sql.Parameters.AddWithValue("@DONATEAMOUNT", amount);

                    sql.Parameters.AddWithValue("@DONATEFOR", donatePurpose);
                    sql.Parameters.AddWithValue("@NEEDRECEIPT", needReceipt);
                    sql.Parameters.AddWithValue("@RECEIPTPOSTMETHOD", receiptPostMethod);
                    sql.Parameters.AddWithValue("@NEEDANONYMOUS", needAnonymous);
                    sql.Parameters.AddWithValue("@RECEIPTTITLE", receiptTitle);
                    sql.Parameters.AddWithValue("@RECEIPTADDRESS", addr);

                    sql.Parameters.AddWithValue("@PICKUPMETHOD", pickupMethod);
                    sql.Parameters.AddWithValue("@REMARK", remark);

                    db.OpenConn();

                    db.ExecuteNonCommit(sql);

                    // 結緣會有商品
                    if (orderTypeId.Equals("2"))
                    {
                        string products = Request.Form["Products"];

                        sql = new SqlCommand(@"INSERT INTO ORDERDETAIL ( ORDERID, ITEMNAME, QTY ) VALUES ( @ORDERID, @ITEMNAME, @QTY )");
                        foreach (var item in products.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries))
                        {
                            string itemName = item.Split('=')[0];
                            string itemQty = item.Split('=')[1];

                            sql.Parameters.Clear();
                            sql.Parameters.AddWithValue("@ORDERID", orderId);
                            sql.Parameters.AddWithValue("@ITEMNAME", itemName);
                            sql.Parameters.AddWithValue("@QTY", itemQty);

                            db.ExecuteNonCommit(sql);
                        }
                    }

                    db.Commit();
                    db.CloseConn();

                    return Content($"OK;{orderId}");
                }
                else
                    return Content($"NG");
            }
            catch (Exception e)
            {
                if (db.SqlConn.State != ConnectionState.Closed)
                    db.CloseConn();

                ecLog.Log.Record(ecLog.Log.InfoType.Error, $"{e.Message};{e.StackTrace}", nameof(SendMailToBranch));
                return Content("NG");
            }

        }



        
        [HttpPost]
        public ActionResult GetDonations(FormCollection form)
        {
            List<Donations> result = new List<Donations>();
            try
            {
                string BUYERNAME = string.Empty;
                //BUYERNAME = form["BUYERNAME"];
                //BUYERNAME = Request.Form["BUYERNAME"];
                if (form.HasKeys())
                {
                    BUYERNAME = form["search[value]"];      //傳送搜尋欄
                }
                if (BUYERNAME == string.Empty)
                {
                    BUYERNAME = "XX預設沒有資料XX";
                }
                //只是用來測jQuery DataTables傳什麼欄位
                string param = string.Empty;
                foreach (string key in Request.Form.AllKeys)
                {
                    param += $"{key}={Request.Form[key]};";
                }
                ecLog.Log.Record(ecLog.Log.InfoType.Normal, param, nameof(Receive));

                SqlCommand sql = new SqlCommand("SELECT Top 100 BUYERNAME, PAYDATE, DONATEAMOUNT, DonateFor FROM ORDERMASTER WHERE BUYERNAME Like @BUYERNAME And ISMANUAL = 'Y' ORDER BY PAYDATE DESC");
                sql.Parameters.AddWithValue("@BUYERNAME", "%" + BUYERNAME + "%");
                DataTable dt = db.GetResult(sql);

                foreach (DataRow dr in dt.Rows)
                {
                    result.Add(new Donations()
                    {
                        Name = MaskValue(dr["BUYERNAME"].ToString()),
                        PayDate = dr["PAYDATE"].ParseDateTime(),
                        Amount = dr["DONATEAMOUNT"].ParseInt().ToString("#,###"),
                        DonateFor = dr["DonateFor"].ToString()
                    });
                }
                string JSON = JsonConvert.SerializeObject(result);
                string str = "{\n";
                str += @"""data"": " ;
                str +=  JSON;
                str += "\n} ";

                return Content(str);
            }
            catch (Exception e)
            {
                if (db.SqlConn.State != ConnectionState.Closed)
                    db.CloseConn();

                ecLog.Log.Record(ecLog.Log.InfoType.Error, $"{e.Message};{e.StackTrace}", nameof(GetDonations));
            }
            return Content(JsonConvert.SerializeObject(result));
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


    }



}