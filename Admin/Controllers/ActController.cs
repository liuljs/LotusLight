using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ecSqlDBManager;
using ecDataParse;
using System.Configuration;
using Newtonsoft.Json;
using Admin.Models;
using System.Text;
using NPOI;
using NPOI.HSSF.UserModel;
using NPOI.XSSF.UserModel;
using NPOI.SS.UserModel;
using ecNPOI;
using System.Web.Security;
using System.Globalization;

namespace Admin.Controllers
{
    [Authorize]
    public class ActController : Controller
    {
        protected static string ConnString = ConfigurationManager.AppSettings["DbConnection"];
        protected DataBase db = new DataBase(ConnString);

        #region 輪播管理
        [HttpPost]
        public ActionResult AddCarousel(FormCollection form)
        {
            if (Request.Files.Count > 0)
            {
                HttpPostedFileBase file = Request.Files[0];
                string url = form["addLinkURL"];
                string dir = Server.MapPath("~/Mime/Carousel");
                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);

                try
                {
                    db.OpenConn();

                    SqlCommand sql = new SqlCommand("INSERT INTO CAROUSEL (TARGETLINK) OUTPUT INSERTED.SN VALUES (@TARGETLINK);");
                    sql.Parameters.AddWithValue("@TARGETLINK", url);
                    DataTable dt = db.GetResult(sql);

                    string sn = dt.Rows[0]["SN"].ToString();
                    string fileName = $"{sn}{Path.GetExtension(file.FileName)}";

                    sql = new SqlCommand("UPDATE CAROUSEL SET IMGNAME = @IMGNAME WHERE SN = @SN");
                    sql.Parameters.AddWithValue("@IMGNAME", fileName);
                    sql.Parameters.AddWithValue("@SN", sn);
                    db.Execute(sql);

                    sql = new SqlCommand("INSERT EVENTLOG ([MODULE], [ACTION], [REMARK]) VALUES (@MODULE, @ACTION, @REMARK);");
                    sql.Parameters.AddWithValue("@MODULE", "輪播管理");
                    sql.Parameters.AddWithValue("@ACTION", "新增");
                    sql.Parameters.AddWithValue("@REMARK", $"{fileName},{url}");
                    db.Execute(sql);

                    db.CloseConn();

                    fileName = Path.Combine(dir, fileName);

                    if (System.IO.File.Exists(fileName))
                        System.IO.File.Delete(fileName);

                    file.SaveAs(fileName);
                }
                catch (Exception e)
                {
                    if (db.SqlConn.State != ConnectionState.Closed)
                        db.CloseConn();

                    ecLog.Log.Record(ecLog.Log.InfoType.Error, $"{e.Message};{e.StackTrace}", nameof(AddCarousel));
                }
            }
            return RedirectToAction("Carousel", "Home");
        }

        [HttpPost]
        public ActionResult EditCarousel(string id, FormCollection form)
        {
            try
            {
                string link = form["lnk"];
                string sort = form["sort"];

                db.OpenConn();

                SqlCommand sql = new SqlCommand("UPDATE CAROUSEL SET TARGETLINK = @TARGETLINK , SORT = @SORT WHERE SN = @SN");
                sql.Parameters.AddWithValue("@TARGETLINK", link);
                sql.Parameters.AddWithValue("@SORT", sort);
                sql.Parameters.AddWithValue("@SN", id);
                db.Execute(sql);

                sql = new SqlCommand("INSERT EVENTLOG ([MODULE], [ACTION], [REMARK]) VALUES (@MODULE, @ACTION, @REMARK);");
                sql.Parameters.AddWithValue("@MODULE", "輪播管理");
                sql.Parameters.AddWithValue("@ACTION", "修改");
                sql.Parameters.AddWithValue("@REMARK", $"sn={id},url={link},sort={sort}");
                db.Execute(sql);

                db.CloseConn();

                return Content("OK");
            }
            catch (Exception e)
            {
                if (db.SqlConn.State != ConnectionState.Closed)
                    db.CloseConn();

                ecLog.Log.Record(ecLog.Log.InfoType.Error, $"{e.Message};{e.StackTrace}", nameof(EditCarousel));

                return Content("NG");
            }
        }

        [HttpPost]
        public ActionResult DeleteCarousel(string id)
        {
            try
            {
                db.OpenConn();

                SqlCommand sql = new SqlCommand("DELETE FROM CAROUSEL OUTPUT DELETED.IMGNAME WHERE SN = @SN");
                sql.Parameters.AddWithValue("@SN", id);
                DataTable dt = db.GetResult(sql);

                sql = new SqlCommand("INSERT EVENTLOG ([MODULE], [ACTION], [REMARK]) VALUES (@MODULE, @ACTION, @REMARK);");
                sql.Parameters.AddWithValue("@MODULE", "輪播管理");
                sql.Parameters.AddWithValue("@ACTION", "刪除");
                sql.Parameters.AddWithValue("@REMARK", $"{id}");
                db.Execute(sql);

                db.CloseConn();

                // 刪除目錄
                if (!string.IsNullOrEmpty(dt.Rows[0]["IMGNAME"].ToString()))
                {
                    string dir = Server.MapPath($"~/Mime/Carousel/{dt.Rows[0]["IMGNAME"].ToString()}");
                    if (System.IO.File.Exists(dir))
                        System.IO.File.Delete(dir);
                }

                return Content("OK");
            }
            catch (Exception e)
            {
                if (db.SqlConn.State != ConnectionState.Closed)
                    db.CloseConn();

                ecLog.Log.Record(ecLog.Log.InfoType.Error, $"{e.Message};{e.StackTrace}", nameof(DeleteCarousel));

                return Content("NG");
            }
        }
        #endregion

        #region 消息管理
        [HttpPost]
        public ActionResult AddNews(FormCollection form)
        {
            // 消息標題
            if (form.HasKeys())
            {
                string title = form["addNewsTitle"];
                string date = form["addDate"];
                string p1 = form["addNewsContent1"];
                string p2 = form["addNewsContent2"];
                string v1 = form["addVideo1"];
                string v2 = form["addVideo2"];

                v1 = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(v1));
                v2 = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(v2));

                date = date.Equals("0001-01-01") ? string.Empty : date;

                // 消息標題圖片
                HttpPostedFileBase titleImg = null;
                // 消息內文圖片(可多)
                List<HttpPostedFileBase> newsImg = new List<HttpPostedFileBase>();
                if (Request.Files.Count > 0)
                {
                    for (int i = 0; i < Request.Files.AllKeys.Length; i++)
                    {
                        if (!Request.Files[i].ContentType.Equals("application/octet-stream"))
                        {
                            if (Request.Files.Keys[i].Equals("titleImage"))
                                titleImg = Request.Files[i];
                            else
                                newsImg.Add(Request.Files[i]);
                        }
                    }
                }

                // 新增消息
                try
                {
                    db.OpenConn();

                    SqlCommand sql = new SqlCommand("INSERT INTO NEWS (TITLE, CONTENT1, CONTENT2, ARTICLEDATE, VIDEOLINK1, VIDEOLINK2) OUTPUT INSERTED.SN VALUES (@TITLE, @CONTENT1, @CONTENT2, @ARTICLEDATE, @VIDEOLINK1, @VIDEOLINK2);");
                    sql.Parameters.AddWithValue("@TITLE", title);
                    sql.Parameters.AddWithValue("@CONTENT1", p1);
                    sql.Parameters.AddWithValue("@CONTENT2", p2);

                    if (string.IsNullOrEmpty(date))
                        sql.Parameters.AddWithValue("@ARTICLEDATE", DBNull.Value);
                    else
                        sql.Parameters.AddWithValue("@ARTICLEDATE", date);

                    sql.Parameters.AddWithValue("@VIDEOLINK1", v1);
                    sql.Parameters.AddWithValue("@VIDEOLINK2", v2);
                    DataTable dt = db.GetResult(sql);

                    string sn = dt.Rows[0]["SN"].ToString();

                    sql = new SqlCommand("INSERT EVENTLOG ([MODULE], [ACTION], [REMARK]) VALUES (@MODULE, @ACTION, @REMARK);");
                    sql.Parameters.AddWithValue("@MODULE", "消息管理");
                    sql.Parameters.AddWithValue("@ACTION", "新增");
                    sql.Parameters.AddWithValue("@REMARK", $"sn={sn};title={title};p1={p1};p2={p2};date={date};v1={v1};v2={v2}");
                    db.ExecuteNonCommit(sql);

                    // 建立檔案目錄
                    string dir = Server.MapPath($"~/Mime/News/{sn}");
                    if (!Directory.Exists(dir))
                        Directory.CreateDirectory(dir);

                    // 封面圖
                    if (titleImg != null)
                    {
                        string fileName = $"t_{sn}{Path.GetExtension(titleImg.FileName)}";

                        sql = new SqlCommand("UPDATE NEWS SET TITLEIMGNAME = @IMGNAME WHERE SN = @SN");
                        sql.Parameters.AddWithValue("@IMGNAME", fileName);
                        sql.Parameters.AddWithValue("@SN", sn);
                        db.ExecuteNonCommit(sql);

                        sql = new SqlCommand("INSERT EVENTLOG ([MODULE], [ACTION], [REMARK]) VALUES (@MODULE, @ACTION, @REMARK);");
                        sql.Parameters.AddWithValue("@MODULE", "消息管理");
                        sql.Parameters.AddWithValue("@ACTION", "新增");
                        sql.Parameters.AddWithValue("@REMARK", $"sn={sn};title_file={fileName}");
                        db.ExecuteNonCommit(sql);

                        fileName = Path.Combine(dir, fileName);

                        if (System.IO.File.Exists(fileName))
                            System.IO.File.Delete(fileName);

                        titleImg.SaveAs(fileName);
                    }

                    // 內容圖片
                    foreach (HttpPostedFileBase img in newsImg)
                    {
                        sql = new SqlCommand("INSERT INTO NEWSIMAGE (NEWSID) OUTPUT INSERTED.SN VALUES (@NEWSID);");
                        sql.Parameters.AddWithValue("@NEWSID", sn);
                        dt = db.GetResult(sql);
                        string fileSn = dt.Rows[0]["SN"].ToString();

                        string fileName = $"c_{fileSn}{Path.GetExtension(img.FileName)}";
                        sql = new SqlCommand("UPDATE NEWSIMAGE SET IMGNAME = @IMGNAME WHERE SN = @SN");
                        sql.Parameters.AddWithValue("@IMGNAME", fileName);
                        sql.Parameters.AddWithValue("@SN", fileSn);
                        db.ExecuteNonCommit(sql);

                        sql = new SqlCommand("INSERT EVENTLOG ([MODULE], [ACTION], [REMARK]) VALUES (@MODULE, @ACTION, @REMARK);");
                        sql.Parameters.AddWithValue("@MODULE", "消息管理");
                        sql.Parameters.AddWithValue("@ACTION", "新增");
                        sql.Parameters.AddWithValue("@REMARK", $"sn={sn};content_file={fileName}");
                        db.ExecuteNonCommit(sql);

                        fileName = Path.Combine(dir, fileName);

                        if (System.IO.File.Exists(fileName))
                            System.IO.File.Delete(fileName);

                        img.SaveAs(fileName);
                    }

                    db.Commit();

                    db.CloseConn();
                }
                catch (Exception e)
                {
                    if (db.SqlConn.State != ConnectionState.Closed)
                        db.CloseConn();

                    ecLog.Log.Record(ecLog.Log.InfoType.Error, $"{e.Message};{e.StackTrace}", nameof(AddNews));
                }

            }
            return RedirectToAction("News", "Home");
        }

        [HttpPost]
        public ActionResult EditNews(string id, FormCollection form)
        {
            try
            {
                string title = form["editTitle"];
                string date = form["editDate"];
                string p1 = form["editNewsContent1"];
                string p2 = form["editNewsContent2"];
                string v1 = form["editVideo1"];
                string v2 = form["editVideo2"];

                v1 = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(v1));
                v2 = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(v2));

                date = date.Equals("0001-01-01") ? string.Empty : date;

                // 建立檔案目錄
                string dir = Server.MapPath($"~/Mime/News/{id}");
                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);

                db.OpenConn();

                SqlCommand sql = new SqlCommand("UPDATE NEWS SET TITLE = @TITLE, CONTENT1 = @CONTENT1, CONTENT2 = @CONTENT2, ARTICLEDATE = @ARTICLEDATE, VIDEOLINK1 = @VIDEOLINK1, VIDEOLINK2 = @VIDEOLINK2 WHERE SN = @SN");
                sql.Parameters.AddWithValue("@TITLE", title);
                sql.Parameters.AddWithValue("@CONTENT1", p1);
                sql.Parameters.AddWithValue("@CONTENT2", p2);

                if (string.IsNullOrEmpty(date))
                    sql.Parameters.AddWithValue("@ARTICLEDATE", DBNull.Value);
                else
                    sql.Parameters.AddWithValue("@ARTICLEDATE", date);

                sql.Parameters.AddWithValue("@VIDEOLINK1", v1);
                sql.Parameters.AddWithValue("@VIDEOLINK2", v2);
                sql.Parameters.AddWithValue("@SN", id);
                db.Execute(sql);

                sql = new SqlCommand("INSERT EVENTLOG ([MODULE], [ACTION], [REMARK]) VALUES (@MODULE, @ACTION, @REMARK);");
                sql.Parameters.AddWithValue("@MODULE", "輪播管理");
                sql.Parameters.AddWithValue("@ACTION", "修改");
                sql.Parameters.AddWithValue("@REMARK", $"sn={id};title={title};p1={p1};p2={p2};date={date};v1={v1};v2={v2}");
                db.Execute(sql);

                // 消息標題圖片
                HttpPostedFileBase titleImg = null;
                // 消息內文圖片(可多)
                List<HttpPostedFileBase> newsImg = new List<HttpPostedFileBase>();
                if (Request.Files.Count > 0)
                {
                    for (int i = 0; i < Request.Files.AllKeys.Length; i++)
                    {
                        if (!Request.Files[i].ContentType.Equals("application/octet-stream"))
                        {
                            if (Request.Files.Keys[i].Equals("editTitleImage"))
                                titleImg = Request.Files[i];
                            else
                                newsImg.Add(Request.Files[i]);
                        }
                    }
                }

                DataTable dt = new DataTable();

                if (titleImg != null)
                {
                    sql = new SqlCommand("SELECT TITLEIMGNAME FROM NEWS WHERE SN = @SN");
                    sql.Parameters.AddWithValue("@SN", id);
                    dt = db.GetResult(sql);

                    sql = new SqlCommand("INSERT EVENTLOG ([MODULE], [ACTION], [REMARK]) VALUES (@MODULE, @ACTION, @REMARK);");
                    sql.Parameters.AddWithValue("@MODULE", "消息管理");
                    sql.Parameters.AddWithValue("@ACTION", "修改");
                    sql.Parameters.AddWithValue("@REMARK", $"sn={id};重新上傳封面圖");
                    db.ExecuteNonCommit(sql);

                    string fileName = dt.Rows[0]["TITLEIMGNAME"].ToString();
                    fileName = string.IsNullOrEmpty(fileName) ? $"t_{id}{Path.GetExtension(titleImg.FileName)}" : fileName;

                    sql = new SqlCommand("UPDATE NEWS SET TITLEIMGNAME = @TITLEIMGNAME WHERE SN = @SN");
                    sql.Parameters.AddWithValue("@TITLEIMGNAME", fileName);
                    sql.Parameters.AddWithValue("@SN", id);
                    db.ExecuteNonCommit(sql);

                    fileName = Path.Combine(dir, fileName);

                    if (System.IO.File.Exists(fileName))
                        System.IO.File.Delete(fileName);

                    titleImg.SaveAs(fileName);
                }

                // 內容圖片
                foreach (HttpPostedFileBase img in newsImg)
                {
                    sql = new SqlCommand("INSERT INTO NEWSIMAGE (NEWSID) OUTPUT INSERTED.SN VALUES (@NEWSID);");
                    sql.Parameters.AddWithValue("@NEWSID", id);
                    dt = db.GetResult(sql);
                    string fileSn = dt.Rows[0]["SN"].ToString();

                    string fileName = $"c_{fileSn}{Path.GetExtension(img.FileName)}";
                    sql = new SqlCommand("UPDATE NEWSIMAGE SET IMGNAME = @IMGNAME WHERE SN = @SN");
                    sql.Parameters.AddWithValue("@IMGNAME", fileName);
                    sql.Parameters.AddWithValue("@SN", fileSn);
                    db.ExecuteNonCommit(sql);

                    sql = new SqlCommand("INSERT EVENTLOG ([MODULE], [ACTION], [REMARK]) VALUES (@MODULE, @ACTION, @REMARK);");
                    sql.Parameters.AddWithValue("@MODULE", "消息管理");
                    sql.Parameters.AddWithValue("@ACTION", "修改");
                    sql.Parameters.AddWithValue("@REMARK", $"sn={id};新增content_file={fileName}");
                    db.ExecuteNonCommit(sql);

                    fileName = Path.Combine(dir, fileName);

                    if (System.IO.File.Exists(fileName))
                        System.IO.File.Delete(fileName);

                    img.SaveAs(fileName);
                }

                // 刪除內容圖片
                string[] fileIds = JsonConvert.DeserializeObject<string[]>(form["hidDelIds"]);
                foreach (string fileId in fileIds)
                {
                    sql = new SqlCommand("DELETE FROM NEWSIMAGE WHERE NEWSID = @NEWSID AND SN = @SN");
                    sql.Parameters.AddWithValue("@NEWSID", id);
                    sql.Parameters.AddWithValue("@SN", fileId);
                    db.ExecuteNonCommit(sql);

                    sql = new SqlCommand("INSERT EVENTLOG ([MODULE], [ACTION], [REMARK]) VALUES (@MODULE, @ACTION, @REMARK);");
                    sql.Parameters.AddWithValue("@MODULE", "消息管理");
                    sql.Parameters.AddWithValue("@ACTION", "修改");
                    sql.Parameters.AddWithValue("@REMARK", $"sn={id};刪除content_file_sn={fileId}");
                    db.ExecuteNonCommit(sql);
                }

                db.Commit();
                db.CloseConn();

                return Content("OK");
            }
            catch (Exception e)
            {
                if (db.SqlConn.State != ConnectionState.Closed)
                    db.CloseConn();

                ecLog.Log.Record(ecLog.Log.InfoType.Error, $"{e.Message};{e.StackTrace}", nameof(EditNews));

                return Content("NG");
            }
        }

        [HttpPost]
        public ActionResult DeleteNews(string id)
        {
            try
            {
                db.OpenConn();

                SqlCommand sql = new SqlCommand("DELETE FROM NEWS WHERE SN = @SN");
                sql.Parameters.AddWithValue("@SN", id);
                db.Execute(sql);

                sql = new SqlCommand("DELETE FROM NEWSIMAGE WHERE NEWSID = @NEWSID");
                sql.Parameters.AddWithValue("@NEWSID", id);
                db.Execute(sql);

                sql = new SqlCommand("INSERT EVENTLOG ([MODULE], [ACTION], [REMARK]) VALUES (@MODULE, @ACTION, @REMARK);");
                sql.Parameters.AddWithValue("@MODULE", "消息管理");
                sql.Parameters.AddWithValue("@ACTION", "刪除");
                sql.Parameters.AddWithValue("@REMARK", $"{id}");
                db.Execute(sql);

                db.CloseConn();

                // 刪除目錄
                string dir = Server.MapPath($"~/Mime/News/{id}");
                if (!Directory.Exists(dir))
                    Directory.Delete(dir);

                return Content("OK");
            }
            catch (Exception e)
            {
                if (db.SqlConn.State != ConnectionState.Closed)
                    db.CloseConn();

                ecLog.Log.Record(ecLog.Log.InfoType.Error, $"{e.Message};{e.StackTrace}", nameof(DeleteNews));

                return Content("NG");
            }
        }

        [HttpPost]
        public ActionResult TopNews(string id, FormCollection form)
        {
            try
            {
                db.OpenConn();

                SqlCommand sql = new SqlCommand("UPDATE NEWS SET ISTOP = @ISTOP WHERE SN = @SN");
                sql.Parameters.AddWithValue("@SN", id);
                sql.Parameters.AddWithValue("@ISTOP", form["top"]);
                db.Execute(sql);

                sql = new SqlCommand("INSERT EVENTLOG ([MODULE], [ACTION], [REMARK]) VALUES (@MODULE, @ACTION, @REMARK);");
                sql.Parameters.AddWithValue("@MODULE", "消息管理");
                sql.Parameters.AddWithValue("@ACTION", "修改");
                sql.Parameters.AddWithValue("@REMARK", $"sn={id};isTop={form["top"]}");
                db.Execute(sql);

                db.CloseConn();

                return Content("OK");
            }
            catch (Exception e)
            {
                if (db.SqlConn.State != ConnectionState.Closed)
                    db.CloseConn();

                ecLog.Log.Record(ecLog.Log.InfoType.Error, $"{e.Message};{e.StackTrace}", nameof(DeleteNews));

                return Content("NG");
            }
        }

        [HttpPost]
        public ActionResult GetNews(string id)
        {
            News obj = new News();
            try
            {
                SqlCommand sql = new SqlCommand("SELECT SN, TITLE, CONTENT1, CONTENT2, TITLEIMGNAME, ARTICLEDATE, VIDEOLINK1, VIDEOLINK2 FROM NEWS WHERE SN = @SN");
                sql.Parameters.AddWithValue("@SN", id);
                DataTable dt = db.GetResult(sql);
                DataRow dr = dt.Rows[0];

                obj = new News()
                {
                    Sn = dr["SN"].ParseInt(),
                    Title = dr["TITLE"].ToString(),
                    P1 = dr["CONTENT1"].ToString(),
                    P2 = dr["CONTENT2"].ToString(),
                    V1 = dr["VIDEOLINK1"].ToString(),
                    V2 = dr["VIDEOLINK2"].ToString(),
                    ArticleDate = dr["ARTICLEDATE"].ParseDateTime()
                };

                sql = new SqlCommand("SELECT SN, IMGNAME FROM NEWSIMAGE WHERE NEWSID = @NEWSID ORDER BY SN");
                sql.Parameters.AddWithValue("@NEWSID", id);
                dt = db.GetResult(sql);

                // 內容圖片
                foreach (DataRow r in dt.Rows)
                {
                    obj.Imgs.Add(new NewsImage()
                    {
                        Sn = r["SN"].ParseInt()
                    });
                }

                return Content(JsonConvert.SerializeObject(obj));
            }
            catch (Exception e)
            {
                if (db.SqlConn.State != ConnectionState.Closed)
                    db.CloseConn();

                ecLog.Log.Record(ecLog.Log.InfoType.Error, $"{e.Message};{e.StackTrace}", nameof(EditNews));
            }
            return Content(JsonConvert.SerializeObject(obj));
        }

        public FileResult GetNewsImage(string id)
        {
            SqlCommand sql = new SqlCommand("SELECT TITLEIMGNAME FROM NEWS WHERE SN = @SN");
            sql.Parameters.AddWithValue("@SN", id);
            DataTable dt = db.GetResult(sql);
            DataRow dr = dt.Rows[0];

            string file = Server.MapPath($"~/Mime/News/{id}/{dr["TITLEIMGNAME"].ToString()}");

            return new FileStreamResult(new FileStream(file, FileMode.Open), "image/jpeg");
        }

        public FileResult GetNewsImages(string id)
        {
            SqlCommand sql = new SqlCommand("SELECT NEWSID, IMGNAME FROM NEWSIMAGE WHERE SN = @SN");
            sql.Parameters.AddWithValue("@SN", id);
            DataTable dt = db.GetResult(sql);
            DataRow dr = dt.Rows[0];

            string file = Server.MapPath($"~/Mime/News/{dr["NEWSID"].ToString()}/{dr["IMGNAME"].ToString()}");

            return new FileStreamResult(new FileStream(file, FileMode.Open), "image/jpeg");
        }
        #endregion

        #region 活動管理
        [HttpPost]
        public ActionResult AddActivity(FormCollection form)
        {
            // 消息標題
            if (form.HasKeys())
            {
                string title = form["addNewsTitle"];
                string p1 = form["addNewsContent1"];
                string p2 = form["addNewsContent2"];
                string date = form["addDate"];

                date = date.Equals("0001-01-01") ? string.Empty : date;

                // 消息標題圖片
                HttpPostedFileBase titleImg = null;
                // 消息內文圖片(可多)
                List<HttpPostedFileBase> newsImg = new List<HttpPostedFileBase>();
                if (Request.Files.Count > 0)
                {
                    for (int i = 0; i < Request.Files.AllKeys.Length; i++)
                    {
                        if (!Request.Files[i].ContentType.Equals("application/octet-stream"))
                        {
                            if (Request.Files.Keys[i].Equals("titleImage"))
                                titleImg = Request.Files[i];
                            else
                                newsImg.Add(Request.Files[i]);
                        }
                    }
                }

                // 新增消息
                try
                {
                    db.OpenConn();

                    SqlCommand sql = new SqlCommand("INSERT INTO ACTIVITY (TITLE, CONTENT1, CONTENT2, ARTICLEDATE) OUTPUT INSERTED.SN VALUES (@TITLE, @CONTENT1, @CONTENT2, @ARTICLEDATE);");
                    sql.Parameters.AddWithValue("@TITLE", title);
                    sql.Parameters.AddWithValue("@CONTENT1", p1);
                    sql.Parameters.AddWithValue("@CONTENT2", p2);

                    if (string.IsNullOrEmpty(date))
                        sql.Parameters.AddWithValue("@ARTICLEDATE", DBNull.Value);
                    else
                        sql.Parameters.AddWithValue("@ARTICLEDATE", date);

                    DataTable dt = db.GetResult(sql);

                    string sn = dt.Rows[0]["SN"].ToString();

                    sql = new SqlCommand("INSERT EVENTLOG ([MODULE], [ACTION], [REMARK]) VALUES (@MODULE, @ACTION, @REMARK);");
                    sql.Parameters.AddWithValue("@MODULE", "活動管理");
                    sql.Parameters.AddWithValue("@ACTION", "新增");
                    sql.Parameters.AddWithValue("@REMARK", $"sn={sn};title={title};p1={p1};p2={p2};date={date}");
                    db.ExecuteNonCommit(sql);

                    // 建立檔案目錄
                    string dir = Server.MapPath($"~/Mime/Activity/{sn}");
                    if (!Directory.Exists(dir))
                        Directory.CreateDirectory(dir);

                    // 封面圖
                    if (titleImg != null)
                    {
                        string fileName = $"t_{sn}{Path.GetExtension(titleImg.FileName)}";

                        sql = new SqlCommand("UPDATE ACTIVITY SET TITLEIMGNAME = @IMGNAME WHERE SN = @SN");
                        sql.Parameters.AddWithValue("@IMGNAME", fileName);
                        sql.Parameters.AddWithValue("@SN", sn);
                        db.ExecuteNonCommit(sql);

                        sql = new SqlCommand("INSERT EVENTLOG ([MODULE], [ACTION], [REMARK]) VALUES (@MODULE, @ACTION, @REMARK);");
                        sql.Parameters.AddWithValue("@MODULE", "活動管理");
                        sql.Parameters.AddWithValue("@ACTION", "新增");
                        sql.Parameters.AddWithValue("@REMARK", $"sn={sn};title_file={fileName}");
                        db.ExecuteNonCommit(sql);

                        fileName = Path.Combine(dir, fileName);

                        if (System.IO.File.Exists(fileName))
                            System.IO.File.Delete(fileName);

                        titleImg.SaveAs(fileName);
                    }

                    // 內容圖片
                    foreach (HttpPostedFileBase img in newsImg)
                    {
                        sql = new SqlCommand("INSERT INTO ACTIVITYIMAGE (ACTIVITYID) OUTPUT INSERTED.SN VALUES (@ACTIVITYID);");
                        sql.Parameters.AddWithValue("@ACTIVITYID", sn);
                        dt = db.GetResult(sql);
                        string fileSn = dt.Rows[0]["SN"].ToString();

                        string fileName = $"c_{fileSn}{Path.GetExtension(titleImg.FileName)}";
                        sql = new SqlCommand("UPDATE ACTIVITYIMAGE SET IMGNAME = @IMGNAME WHERE SN = @SN");
                        sql.Parameters.AddWithValue("@IMGNAME", fileName);
                        sql.Parameters.AddWithValue("@SN", fileSn);
                        db.ExecuteNonCommit(sql);

                        sql = new SqlCommand("INSERT EVENTLOG ([MODULE], [ACTION], [REMARK]) VALUES (@MODULE, @ACTION, @REMARK);");
                        sql.Parameters.AddWithValue("@MODULE", "活動管理");
                        sql.Parameters.AddWithValue("@ACTION", "新增");
                        sql.Parameters.AddWithValue("@REMARK", $"sn={sn};content_file={fileName}");
                        db.ExecuteNonCommit(sql);

                        fileName = Path.Combine(dir, fileName);

                        if (System.IO.File.Exists(fileName))
                            System.IO.File.Delete(fileName);

                        img.SaveAs(fileName);
                    }

                    db.Commit();

                    db.CloseConn();
                }
                catch (Exception e)
                {
                    if (db.SqlConn.State != ConnectionState.Closed)
                        db.CloseConn();

                    ecLog.Log.Record(ecLog.Log.InfoType.Error, $"{e.Message};{e.StackTrace}", nameof(AddActivity));
                }

            }
            return RedirectToAction("Activity", "Home");
        }

        [HttpPost]
        public ActionResult EditActivity(string id, FormCollection form)
        {
            try
            {
                string title = form["editTitle"];
                string p1 = form["editNewsContent1"];
                string p2 = form["editNewsContent2"];
                string date = form["editDate"];

                date = date.Equals("0001-01-01") ? string.Empty : date;

                // 建立檔案目錄
                string dir = Server.MapPath($"~/Mime/Activity/{id}");
                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);

                db.OpenConn();

                SqlCommand sql = new SqlCommand("UPDATE ACTIVITY SET TITLE = @TITLE, CONTENT1 = @CONTENT1, CONTENT2 = @CONTENT2, ARTICLEDATE = @ARTICLEDATE WHERE SN = @SN");
                sql.Parameters.AddWithValue("@TITLE", title);
                sql.Parameters.AddWithValue("@CONTENT1", p1);
                sql.Parameters.AddWithValue("@CONTENT2", p2);

                if (string.IsNullOrEmpty(date))
                    sql.Parameters.AddWithValue("@ARTICLEDATE", DBNull.Value);
                else
                    sql.Parameters.AddWithValue("@ARTICLEDATE", date);

                sql.Parameters.AddWithValue("@SN", id);
                db.Execute(sql);

                sql = new SqlCommand("INSERT EVENTLOG ([MODULE], [ACTION], [REMARK]) VALUES (@MODULE, @ACTION, @REMARK);");
                sql.Parameters.AddWithValue("@MODULE", "活動管理");
                sql.Parameters.AddWithValue("@ACTION", "修改");
                sql.Parameters.AddWithValue("@REMARK", $"sn={id};title={title};p1={p1};p2={p2};date={date}");
                db.Execute(sql);

                // 消息標題圖片
                HttpPostedFileBase titleImg = null;
                // 消息內文圖片(可多)
                List<HttpPostedFileBase> newsImg = new List<HttpPostedFileBase>();
                if (Request.Files.Count > 0)
                {
                    for (int i = 0; i < Request.Files.AllKeys.Length; i++)
                    {
                        if (!Request.Files[i].ContentType.Equals("application/octet-stream"))
                        {
                            if (Request.Files.Keys[i].Equals("editTitleImage"))
                                titleImg = Request.Files[i];
                            else
                                newsImg.Add(Request.Files[i]);
                        }
                    }
                }

                DataTable dt = new DataTable();

                if (titleImg != null)
                {
                    sql = new SqlCommand("SELECT TITLEIMGNAME FROM ACTIVITY WHERE SN = @SN");
                    sql.Parameters.AddWithValue("@SN", id);
                    dt = db.GetResult(sql);

                    sql = new SqlCommand("INSERT EVENTLOG ([MODULE], [ACTION], [REMARK]) VALUES (@MODULE, @ACTION, @REMARK);");
                    sql.Parameters.AddWithValue("@MODULE", "活動管理");
                    sql.Parameters.AddWithValue("@ACTION", "修改");
                    sql.Parameters.AddWithValue("@REMARK", $"sn={id};重新上傳封面圖");
                    db.ExecuteNonCommit(sql);

                    string fileName = dt.Rows[0]["TITLEIMGNAME"].ToString();
                    fileName = string.IsNullOrEmpty(fileName) ? $"t_{id}{Path.GetExtension(titleImg.FileName)}" : fileName;

                    sql = new SqlCommand("UPDATE ACTIVITY SET TITLEIMGNAME = @TITLEIMGNAME WHERE SN = @SN");
                    sql.Parameters.AddWithValue("@TITLEIMGNAME", fileName);
                    sql.Parameters.AddWithValue("@SN", id);
                    db.ExecuteNonCommit(sql);

                    fileName = Path.Combine(dir, fileName);

                    if (System.IO.File.Exists(fileName))
                        System.IO.File.Delete(fileName);

                    titleImg.SaveAs(fileName);
                }

                // 內容圖片
                foreach (HttpPostedFileBase img in newsImg)
                {
                    sql = new SqlCommand("INSERT INTO ACTIVITYIMAGE (ACTIVITYID) OUTPUT INSERTED.SN VALUES (@ACTIVITYID);");
                    sql.Parameters.AddWithValue("@ACTIVITYID", id);
                    dt = db.GetResult(sql);
                    string fileSn = dt.Rows[0]["SN"].ToString();

                    string fileName = $"c_{fileSn}{Path.GetExtension(img.FileName)}";
                    sql = new SqlCommand("UPDATE ACTIVITYIMAGE SET IMGNAME = @IMGNAME WHERE SN = @SN");
                    sql.Parameters.AddWithValue("@IMGNAME", fileName);
                    sql.Parameters.AddWithValue("@SN", fileSn);
                    db.ExecuteNonCommit(sql);

                    sql = new SqlCommand("INSERT EVENTLOG ([MODULE], [ACTION], [REMARK]) VALUES (@MODULE, @ACTION, @REMARK);");
                    sql.Parameters.AddWithValue("@MODULE", "活動管理");
                    sql.Parameters.AddWithValue("@ACTION", "修改");
                    sql.Parameters.AddWithValue("@REMARK", $"sn={id};新增content_file={fileName}");
                    db.ExecuteNonCommit(sql);

                    fileName = Path.Combine(dir, fileName);

                    if (System.IO.File.Exists(fileName))
                        System.IO.File.Delete(fileName);

                    img.SaveAs(fileName);
                }

                // 刪除內容圖片
                string[] fileIds = JsonConvert.DeserializeObject<string[]>(form["hidDelIds"]);
                foreach (string fileId in fileIds)
                {
                    sql = new SqlCommand("DELETE FROM ACTIVITYIMAGE WHERE ACTIVITYID = @ACTIVITYID AND SN = @SN");
                    sql.Parameters.AddWithValue("@ACTIVITYID", id);
                    sql.Parameters.AddWithValue("@SN", fileId);
                    db.ExecuteNonCommit(sql);

                    sql = new SqlCommand("INSERT EVENTLOG ([MODULE], [ACTION], [REMARK]) VALUES (@MODULE, @ACTION, @REMARK);");
                    sql.Parameters.AddWithValue("@MODULE", "活動管理");
                    sql.Parameters.AddWithValue("@ACTION", "修改");
                    sql.Parameters.AddWithValue("@REMARK", $"sn={id};刪除content_file_sn={fileId}");
                    db.ExecuteNonCommit(sql);
                }

                db.Commit();
                db.CloseConn();

                return Content("OK");
            }
            catch (Exception e)
            {
                if (db.SqlConn.State != ConnectionState.Closed)
                    db.CloseConn();

                ecLog.Log.Record(ecLog.Log.InfoType.Error, $"{e.Message};{e.StackTrace}", nameof(EditActivity));

                return Content("NG");
            }
        }

        [HttpPost]
        public ActionResult DeleteActivity(string id)
        {
            try
            {
                db.OpenConn();

                SqlCommand sql = new SqlCommand("DELETE FROM ACTIVITY WHERE SN = @SN");
                sql.Parameters.AddWithValue("@SN", id);
                db.Execute(sql);

                sql = new SqlCommand("DELETE FROM ACTIVITYIMAGE WHERE ACTIVITYID = @ACTIVITYID");
                sql.Parameters.AddWithValue("@ACTIVITYID", id);
                db.Execute(sql);

                sql = new SqlCommand("INSERT EVENTLOG ([MODULE], [ACTION], [REMARK]) VALUES (@MODULE, @ACTION, @REMARK);");
                sql.Parameters.AddWithValue("@MODULE", "活動管理");
                sql.Parameters.AddWithValue("@ACTION", "刪除");
                sql.Parameters.AddWithValue("@REMARK", $"{id}");
                db.Execute(sql);

                db.CloseConn();

                // 刪除目錄
                string dir = Server.MapPath($"~/Mime/Activity/{id}");
                if (!Directory.Exists(dir))
                    Directory.Delete(dir);

                return Content("OK");
            }
            catch (Exception e)
            {
                if (db.SqlConn.State != ConnectionState.Closed)
                    db.CloseConn();

                ecLog.Log.Record(ecLog.Log.InfoType.Error, $"{e.Message};{e.StackTrace}", nameof(DeleteActivity));

                return Content("NG");
            }
        }

        [HttpPost]
        public ActionResult TopActivity(string id, FormCollection form)
        {
            try
            {
                db.OpenConn();

                SqlCommand sql = new SqlCommand("UPDATE ACTIVITY SET ISTOP = @ISTOP WHERE SN = @SN");
                sql.Parameters.AddWithValue("@SN", id);
                sql.Parameters.AddWithValue("@ISTOP", form["top"]);
                db.Execute(sql);

                sql = new SqlCommand("INSERT EVENTLOG ([MODULE], [ACTION], [REMARK]) VALUES (@MODULE, @ACTION, @REMARK);");
                sql.Parameters.AddWithValue("@MODULE", "活動管理");
                sql.Parameters.AddWithValue("@ACTION", "修改");
                sql.Parameters.AddWithValue("@REMARK", $"sn={id};isTop={form["top"]}");
                db.Execute(sql);

                db.CloseConn();

                return Content("OK");
            }
            catch (Exception e)
            {
                if (db.SqlConn.State != ConnectionState.Closed)
                    db.CloseConn();

                ecLog.Log.Record(ecLog.Log.InfoType.Error, $"{e.Message};{e.StackTrace}", nameof(TopActivity));

                return Content("NG");
            }
        }

        [HttpPost]
        public ActionResult GetActivity(string id)
        {
            Activity obj = new Activity();
            try
            {
                SqlCommand sql = new SqlCommand("SELECT SN, TITLE, CONTENT1, CONTENT2, TITLEIMGNAME, ARTICLEDATE FROM ACTIVITY WHERE SN = @SN");
                sql.Parameters.AddWithValue("@SN", id);
                DataTable dt = db.GetResult(sql);
                DataRow dr = dt.Rows[0];

                obj = new Activity()
                {
                    Sn = dr["SN"].ParseInt(),
                    Title = dr["TITLE"].ToString(),
                    P1 = dr["CONTENT1"].ToString(),
                    P2 = dr["CONTENT2"].ToString(),
                    ArticleDate = dr["ARTICLEDATE"].ParseDateTime()
                };

                sql = new SqlCommand("SELECT SN, IMGNAME FROM ACTIVITYIMAGE WHERE ACTIVITYID = @ACTIVITYID ORDER BY SN");
                sql.Parameters.AddWithValue("@ACTIVITYID", id);
                dt = db.GetResult(sql);

                // 內容圖片
                foreach (DataRow r in dt.Rows)
                {
                    obj.Imgs.Add(new ActivityImage()
                    {
                        Sn = r["SN"].ParseInt()
                    });
                }

                return Content(JsonConvert.SerializeObject(obj));
            }
            catch (Exception e)
            {
                if (db.SqlConn.State != ConnectionState.Closed)
                    db.CloseConn();

                ecLog.Log.Record(ecLog.Log.InfoType.Error, $"{e.Message};{e.StackTrace}", nameof(GetActivity));
            }
            return Content(JsonConvert.SerializeObject(obj));
        }

        public FileResult GetActivityImage(string id)
        {
            SqlCommand sql = new SqlCommand("SELECT TITLEIMGNAME FROM ACTIVITY WHERE SN = @SN");
            sql.Parameters.AddWithValue("@SN", id);
            DataTable dt = db.GetResult(sql);
            DataRow dr = dt.Rows[0];

            string file = Server.MapPath($"~/Mime/Activity/{id}/{dr["TITLEIMGNAME"].ToString()}");

            if (!System.IO.File.Exists(file))
                return null;
            else
                return new FileStreamResult(new FileStream(file, FileMode.Open), "image/jpeg");
        }

        public FileResult GetActivityImages(string id)
        {
            SqlCommand sql = new SqlCommand("SELECT ACTIVITYID, IMGNAME FROM ACTIVITYIMAGE WHERE SN = @SN");
            sql.Parameters.AddWithValue("@SN", id);
            DataTable dt = db.GetResult(sql);
            DataRow dr = dt.Rows[0];

            string file = Server.MapPath($"~/Mime/Activity/{dr["ACTIVITYID"].ToString()}/{dr["IMGNAME"].ToString()}");

            return new FileStreamResult(new FileStream(file, FileMode.Open), "image/jpeg");
        }
        #endregion

        #region 招募管理
        [HttpPost]
        public ActionResult AddRecruit(FormCollection form)
        {
            if (form.HasKeys())
            {
                string title = form["addTitle"];
                string subTitle = form["addSubTitle"];
                string p1 = form["addContent1"];
                string p2 = form["addContent2"] ?? string.Empty;

                // 標題圖片
                HttpPostedFileBase titleImg = null;

                if (Request.Files.Count > 0 && !Request.Files[0].ContentType.Equals("application/octet-stream"))
                    titleImg = Request.Files[0];

                // 新增消息
                try
                {
                    db.OpenConn();

                    SqlCommand sql = new SqlCommand("INSERT INTO RECRUIT (TITLE, SUBTITLE, CONTENT1, CONTENT2) OUTPUT INSERTED.SN VALUES (@TITLE, @SUBTITLE, @CONTENT1, @CONTENT2);");
                    sql.Parameters.AddWithValue("@TITLE", title);
                    sql.Parameters.AddWithValue("@SUBTITLE", subTitle);
                    sql.Parameters.AddWithValue("@CONTENT1", p1);
                    sql.Parameters.AddWithValue("@CONTENT2", p2);
                    DataTable dt = db.GetResult(sql);

                    string sn = dt.Rows[0]["SN"].ToString();

                    sql = new SqlCommand("INSERT EVENTLOG ([MODULE], [ACTION], [REMARK]) VALUES (@MODULE, @ACTION, @REMARK);");
                    sql.Parameters.AddWithValue("@MODULE", "招募管理");
                    sql.Parameters.AddWithValue("@ACTION", "新增");
                    sql.Parameters.AddWithValue("@REMARK", $"sn={sn};title={title};sub_title={subTitle};p1={p1};p2={p2}");
                    db.ExecuteNonCommit(sql);

                    // 建立檔案目錄
                    string dir = Server.MapPath($"~/Mime/Recruit/{sn}");
                    if (!Directory.Exists(dir))
                        Directory.CreateDirectory(dir);

                    // 封面圖
                    if (titleImg != null)
                    {
                        string fileName = $"t_{sn}{Path.GetExtension(titleImg.FileName)}";

                        sql = new SqlCommand("UPDATE RECRUIT SET TITLEIMGNAME = @IMGNAME WHERE SN = @SN");
                        sql.Parameters.AddWithValue("@IMGNAME", fileName);
                        sql.Parameters.AddWithValue("@SN", sn);
                        db.ExecuteNonCommit(sql);

                        sql = new SqlCommand("INSERT EVENTLOG ([MODULE], [ACTION], [REMARK]) VALUES (@MODULE, @ACTION, @REMARK);");
                        sql.Parameters.AddWithValue("@MODULE", "招募管理");
                        sql.Parameters.AddWithValue("@ACTION", "新增");
                        sql.Parameters.AddWithValue("@REMARK", $"sn={sn};title_file={fileName}");
                        db.ExecuteNonCommit(sql);

                        fileName = Path.Combine(dir, fileName);

                        if (System.IO.File.Exists(fileName))
                            System.IO.File.Delete(fileName);

                        titleImg.SaveAs(fileName);
                    }

                    db.Commit();

                    db.CloseConn();
                }
                catch (Exception e)
                {
                    if (db.SqlConn.State != ConnectionState.Closed)
                        db.CloseConn();

                    ecLog.Log.Record(ecLog.Log.InfoType.Error, $"{e.Message};{e.StackTrace}", nameof(AddRecruit));
                }

            }
            return RedirectToAction("Recruit", "Home");
        }
        [HttpPost]
        public ActionResult EditRecruit(string id, FormCollection form)
        {
            try
            {
                // 建立檔案目錄
                string dir = Server.MapPath($"~/Mime/Recruit/{id}");
                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);

                db.OpenConn();

                string title = form["editTitle"];
                string subTitle = form["editSubTitle"];
                string p1 = form["editContent1"];
                string p2 = form["editContent2"] ?? string.Empty;

                SqlCommand sql = new SqlCommand("UPDATE RECRUIT SET TITLE = @TITLE, SUBTITLE = @SUBTITLE, CONTENT1 = @CONTENT1, CONTENT2 = @CONTENT2 WHERE SN = @SN");
                sql.Parameters.AddWithValue("@TITLE", title);
                sql.Parameters.AddWithValue("@SUBTITLE", subTitle);
                sql.Parameters.AddWithValue("@CONTENT1", p1);
                sql.Parameters.AddWithValue("@CONTENT2", p2);
                sql.Parameters.AddWithValue("@SN", id);
                db.Execute(sql);

                sql = new SqlCommand("INSERT EVENTLOG ([MODULE], [ACTION], [REMARK]) VALUES (@MODULE, @ACTION, @REMARK);");
                sql.Parameters.AddWithValue("@MODULE", "招募管理");
                sql.Parameters.AddWithValue("@ACTION", "修改");
                sql.Parameters.AddWithValue("@REMARK", $"sn={id},title={title},sub_title={subTitle},p1={p1},p2={p2}");
                db.Execute(sql);

                // 消息標題圖片
                HttpPostedFileBase titleImg = null;

                if (Request.Files.Count > 0 && !Request.Files[0].ContentType.Equals("application/octet-stream"))
                    titleImg = Request.Files[0];

                DataTable dt = new DataTable();

                if (titleImg != null)
                {
                    sql = new SqlCommand("SELECT TITLEIMGNAME FROM RECRUIT WHERE SN = @SN");
                    sql.Parameters.AddWithValue("@SN", id);
                    dt = db.GetResult(sql);

                    sql = new SqlCommand("INSERT EVENTLOG ([MODULE], [ACTION], [REMARK]) VALUES (@MODULE, @ACTION, @REMARK);");
                    sql.Parameters.AddWithValue("@MODULE", "招募管理");
                    sql.Parameters.AddWithValue("@ACTION", "修改");
                    sql.Parameters.AddWithValue("@REMARK", $"sn={id};重新上傳封面圖");
                    db.ExecuteNonCommit(sql);

                    string fileName = Path.Combine(dir, dt.Rows[0]["TITLEIMGNAME"].ToString());

                    if (System.IO.File.Exists(fileName))
                        System.IO.File.Delete(fileName);

                    titleImg.SaveAs(fileName);
                }

                db.Commit();
                db.CloseConn();

                return Content("OK");
            }
            catch (Exception e)
            {
                if (db.SqlConn.State != ConnectionState.Closed)
                    db.CloseConn();

                ecLog.Log.Record(ecLog.Log.InfoType.Error, $"{e.Message};{e.StackTrace}", nameof(EditRecruit));

                return Content("NG");
            }
        }

        [HttpPost]
        public ActionResult DeleteRecruit(string id)
        {
            try
            {
                db.OpenConn();

                SqlCommand sql = new SqlCommand("DELETE FROM RECRUIT WHERE SN = @SN");
                sql.Parameters.AddWithValue("@SN", id);
                db.Execute(sql);

                sql = new SqlCommand("INSERT EVENTLOG ([MODULE], [ACTION], [REMARK]) VALUES (@MODULE, @ACTION, @REMARK);");
                sql.Parameters.AddWithValue("@MODULE", "招募管理");
                sql.Parameters.AddWithValue("@ACTION", "刪除");
                sql.Parameters.AddWithValue("@REMARK", $"{id}");
                db.Execute(sql);

                db.CloseConn();

                // 刪除目錄
                string dir = Server.MapPath($"~/Mime/Recruit/{id}");
                if (!Directory.Exists(dir))
                    Directory.Delete(dir);

                return Content("OK");
            }
            catch (Exception e)
            {
                if (db.SqlConn.State != ConnectionState.Closed)
                    db.CloseConn();

                ecLog.Log.Record(ecLog.Log.InfoType.Error, $"{e.Message};{e.StackTrace}", nameof(DeleteRecruit));

                return Content("NG");
            }
        }
        [HttpPost]
        public ActionResult GetRecruit(string id)
        {
            Recruit obj = new Recruit();
            try
            {
                SqlCommand sql = new SqlCommand("SELECT SN, TITLE, SUBTITLE, CONTENT1, CONTENT2, TITLEIMGNAME FROM RECRUIT WHERE SN = @SN");
                sql.Parameters.AddWithValue("@SN", id);
                DataTable dt = db.GetResult(sql);
                DataRow dr = dt.Rows[0];

                obj = new Recruit()
                {
                    Sn = dr["SN"].ParseInt(),
                    Title = dr["TITLE"].ToString(),
                    SubTitle = dr["SUBTITLE"].ToString(),
                    P1 = dr["CONTENT1"].ToString(),
                    P2 = dr["CONTENT2"].ToString()
                };

                return Content(JsonConvert.SerializeObject(obj));
            }
            catch (Exception e)
            {
                if (db.SqlConn.State != ConnectionState.Closed)
                    db.CloseConn();

                ecLog.Log.Record(ecLog.Log.InfoType.Error, $"{e.Message};{e.StackTrace}", nameof(GetRecruit));
            }
            return Content(JsonConvert.SerializeObject(obj));
        }

        public FileResult GetRecruitImage(string id)
        {
            SqlCommand sql = new SqlCommand("SELECT TITLEIMGNAME FROM RECRUIT WHERE SN = @SN");
            sql.Parameters.AddWithValue("@SN", id);
            DataTable dt = db.GetResult(sql);
            DataRow dr = dt.Rows[0];

            string file = Server.MapPath($"~/Mime/Recruit/{id}/{dr["TITLEIMGNAME"].ToString()}");

            return new FileStreamResult(new FileStream(file, FileMode.Open), "image/jpeg");
        }
        #endregion

        #region 請購管理
        [HttpPost]
        public ActionResult AddRelated(FormCollection form)
        {
            if (form.HasKeys())
            {
                string title = form["addTitle"];
                string amount = form["addPurchasePrice"];
                string p1 = form["addContent1"];
                string p2 = form["addContent2"];
                string notes = form["addNotes"];

                // 標題圖片
                HttpPostedFileBase titleImg = null;

                if (Request.Files.Count > 0 && !Request.Files[0].ContentType.Equals("application/octet-stream"))
                    titleImg = Request.Files[0];

                // 新增消息
                try
                {
                    db.OpenConn();

                    SqlCommand sql = new SqlCommand("INSERT INTO RELATED (TITLE, AMOUNT, CONTENT1, CONTENT2, NOTES) OUTPUT INSERTED.SN VALUES (@TITLE, @AMOUNT, @CONTENT1, @CONTENT2, @NOTES);");
                    sql.Parameters.AddWithValue("@TITLE", title);
                    sql.Parameters.AddWithValue("@AMOUNT", amount);
                    sql.Parameters.AddWithValue("@CONTENT1", p1);
                    sql.Parameters.AddWithValue("@CONTENT2", p2);
                    sql.Parameters.AddWithValue("@NOTES", notes);
                    DataTable dt = db.GetResult(sql);

                    string sn = dt.Rows[0]["SN"].ToString();

                    sql = new SqlCommand("INSERT EVENTLOG ([MODULE], [ACTION], [REMARK]) VALUES (@MODULE, @ACTION, @REMARK);");
                    sql.Parameters.AddWithValue("@MODULE", "請購管理");
                    sql.Parameters.AddWithValue("@ACTION", "新增");
                    sql.Parameters.AddWithValue("@REMARK", $"sn={sn};title={title};amount={amount};p1={p1};p2={p2};notes={notes}");
                    db.ExecuteNonCommit(sql);

                    // 建立檔案目錄
                    string dir = Server.MapPath($"~/Mime/Related/{sn}");
                    if (!Directory.Exists(dir))
                        Directory.CreateDirectory(dir);

                    // 封面圖
                    if (titleImg != null)
                    {
                        string fileName = $"t_{sn}{Path.GetExtension(titleImg.FileName)}";

                        sql = new SqlCommand("UPDATE RELATED SET TITLEIMGNAME = @IMGNAME WHERE SN = @SN");
                        sql.Parameters.AddWithValue("@IMGNAME", fileName);
                        sql.Parameters.AddWithValue("@SN", sn);
                        db.ExecuteNonCommit(sql);

                        sql = new SqlCommand("INSERT EVENTLOG ([MODULE], [ACTION], [REMARK]) VALUES (@MODULE, @ACTION, @REMARK);");
                        sql.Parameters.AddWithValue("@MODULE", "請購管理");
                        sql.Parameters.AddWithValue("@ACTION", "新增");
                        sql.Parameters.AddWithValue("@REMARK", $"sn={sn};title_file={fileName}");
                        db.ExecuteNonCommit(sql);

                        fileName = Path.Combine(dir, fileName);

                        if (System.IO.File.Exists(fileName))
                            System.IO.File.Delete(fileName);

                        titleImg.SaveAs(fileName);
                    }

                    db.Commit();

                    db.CloseConn();
                }
                catch (Exception e)
                {
                    if (db.SqlConn.State != ConnectionState.Closed)
                        db.CloseConn();

                    ecLog.Log.Record(ecLog.Log.InfoType.Error, $"{e.Message};{e.StackTrace}", nameof(AddRelated));
                }

            }
            return RedirectToAction("Related", "Home");
        }
        [HttpPost]
        public ActionResult EditRelated(string id, FormCollection form)
        {
            try
            {
                string title = form["editTitle"];
                string amount = form["editPurchasePrice"];
                string p1 = form["editContent1"];
                string p2 = form["editContent2"];
                string notes = form["editNotes"];

                // 建立檔案目錄
                string dir = Server.MapPath($"~/Mime/Related/{id}");
                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);

                db.OpenConn();

                SqlCommand sql = new SqlCommand("UPDATE RELATED SET TITLE = @TITLE, AMOUNT = @AMOUNT, CONTENT1 = @CONTENT1, CONTENT2 = @CONTENT2, NOTES = @NOTES WHERE SN = @SN");
                sql.Parameters.AddWithValue("@TITLE", title);
                sql.Parameters.AddWithValue("@AMOUNT", amount);
                sql.Parameters.AddWithValue("@CONTENT1", p1);
                sql.Parameters.AddWithValue("@CONTENT2", p2);
                sql.Parameters.AddWithValue("@NOTES", notes);
                sql.Parameters.AddWithValue("@SN", id);
                db.Execute(sql);

                sql = new SqlCommand("INSERT EVENTLOG ([MODULE], [ACTION], [REMARK]) VALUES (@MODULE, @ACTION, @REMARK);");
                sql.Parameters.AddWithValue("@MODULE", "請購管理");
                sql.Parameters.AddWithValue("@ACTION", "修改");
                sql.Parameters.AddWithValue("@REMARK", $"sn={id};title={title};amount={amount};p1={p1};p2={p2};notes={notes}");
                db.Execute(sql);

                // 消息標題圖片
                HttpPostedFileBase titleImg = null;

                if (Request.Files.Count > 0 && !Request.Files[0].ContentType.Equals("application/octet-stream"))
                    titleImg = Request.Files[0];

                DataTable dt = new DataTable();

                if (titleImg != null)
                {
                    sql = new SqlCommand("SELECT TITLEIMGNAME FROM RELATED WHERE SN = @SN");
                    sql.Parameters.AddWithValue("@SN", id);
                    dt = db.GetResult(sql);

                    sql = new SqlCommand("INSERT EVENTLOG ([MODULE], [ACTION], [REMARK]) VALUES (@MODULE, @ACTION, @REMARK);");
                    sql.Parameters.AddWithValue("@MODULE", "請購管理");
                    sql.Parameters.AddWithValue("@ACTION", "修改");
                    sql.Parameters.AddWithValue("@REMARK", $"sn={id};重新上傳封面圖");
                    db.ExecuteNonCommit(sql);

                    string fileName = Path.Combine(dir, dt.Rows[0]["TITLEIMGNAME"].ToString());

                    if (System.IO.File.Exists(fileName))
                        System.IO.File.Delete(fileName);

                    titleImg.SaveAs(fileName);
                }

                db.Commit();
                db.CloseConn();

                return Content("OK");
            }
            catch (Exception e)
            {
                if (db.SqlConn.State != ConnectionState.Closed)
                    db.CloseConn();

                ecLog.Log.Record(ecLog.Log.InfoType.Error, $"{e.Message};{e.StackTrace}", nameof(EditRelated));

                return Content("NG");
            }
        }

        [HttpPost]
        public ActionResult DeleteRelated(string id)
        {
            try
            {
                db.OpenConn();

                SqlCommand sql = new SqlCommand("DELETE FROM RELATED WHERE SN = @SN");
                sql.Parameters.AddWithValue("@SN", id);
                db.Execute(sql);

                sql = new SqlCommand("INSERT EVENTLOG ([MODULE], [ACTION], [REMARK]) VALUES (@MODULE, @ACTION, @REMARK);");
                sql.Parameters.AddWithValue("@MODULE", "請購管理");
                sql.Parameters.AddWithValue("@ACTION", "刪除");
                sql.Parameters.AddWithValue("@REMARK", $"{id}");
                db.Execute(sql);

                db.CloseConn();

                // 刪除目錄
                string dir = Server.MapPath($"~/Mime/Related/{id}");
                if (!Directory.Exists(dir))
                    Directory.Delete(dir);

                return Content("OK");
            }
            catch (Exception e)
            {
                if (db.SqlConn.State != ConnectionState.Closed)
                    db.CloseConn();

                ecLog.Log.Record(ecLog.Log.InfoType.Error, $"{e.Message};{e.StackTrace}", nameof(DeleteRelated));

                return Content("NG");
            }
        }
        [HttpPost]
        public ActionResult GetRelated(string id)
        {
            Related obj = new Related();
            try
            {
                SqlCommand sql = new SqlCommand("SELECT SN, TITLE, AMOUNT, CONTENT1, CONTENT2, TITLEIMGNAME, NOTES FROM RELATED WHERE SN = @SN");
                sql.Parameters.AddWithValue("@SN", id);
                DataTable dt = db.GetResult(sql);
                DataRow dr = dt.Rows[0];

                obj = new Related()
                {
                    Sn = dr["SN"].ParseInt(),
                    Title = dr["TITLE"].ToString(),
                    Amount = dr["AMOUNT"].ParseDecimal(),
                    P1 = dr["CONTENT1"].ToString(),
                    P2 = dr["CONTENT2"].ToString(),
                    Notes = dr["NOTES"].ToString()
                };

                return Content(JsonConvert.SerializeObject(obj));
            }
            catch (Exception e)
            {
                if (db.SqlConn.State != ConnectionState.Closed)
                    db.CloseConn();

                ecLog.Log.Record(ecLog.Log.InfoType.Error, $"{e.Message};{e.StackTrace}", nameof(GetRelated));
            }
            return Content(JsonConvert.SerializeObject(obj));
        }

        public FileResult GetRelatedImage(string id)
        {
            SqlCommand sql = new SqlCommand("SELECT TITLEIMGNAME FROM RELATED WHERE SN = @SN");
            sql.Parameters.AddWithValue("@SN", id);
            DataTable dt = db.GetResult(sql);
            DataRow dr = dt.Rows[0];

            string file = Server.MapPath($"~/Mime/Related/{id}/{dr["TITLEIMGNAME"].ToString()}");

            return new FileStreamResult(new FileStream(file, FileMode.Open), "image/jpeg");
        }
        #endregion

        #region 聯絡資訊管理
        [HttpPost]
        public ActionResult AddContact(FormCollection form)
        {
            if (form.HasKeys())
            {
                string name = form["addContactTitle"];
                string address = form["addContactAddress"];
                string addressEng = form["addContactAddressEng"];
                string tel = form["addContactTel"];
                string fax = form["addContactFax"];
                string fbLink = form["addContactFBLink"];
                string merchantId = form["addMerchantId"];
                string terminalId = form["addTerminalId"];
                string googleMap = form["addGoogleMap"];
                string bankCode = form["addBankCode"];
                string bankName = form["addBankName"];
                string bankNameEng = form["addBankNameEng"];
                string accountName = form["addAccountName"];
                string accountNameEng = form["addAccountNameEng"];
                string accountNum = form["addAccountNum"];
                string mail = form["addMail"];

                googleMap = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(googleMap));
                addressEng = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(addressEng));
                bankNameEng = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(bankNameEng));
                accountNameEng = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(accountNameEng));

                // 新增
                try
                {
                    db.OpenConn();

                    SqlCommand sql = new SqlCommand("INSERT INTO BRANCH (NAME, ADDRESS, ADDRESSENG, TEL, FAX, FBLINK, MERCHANTID, TERMINALID, GOOGLEMAP, BANKNAME, BANKNAMEENG, BANKCODE, ACCOUNTNAME, ACCOUNTNAMEENG, ACCOUNTNUM, MAIL) OUTPUT INSERTED.SN VALUES (@NAME, @ADDRESS, @ADDRESSENG, @TEL, @FAX, @FBLINK, @MERCHANTID, @TERMINALID, @GOOGLEMAP, @BANKNAME, @BANKNAMEENG, @BANKCODE, @ACCOUNTNAME, @ACCOUNTNAMEENG, @ACCOUNTNUM, @MAIL);");
                    sql.Parameters.AddWithValue("@NAME", name);
                    sql.Parameters.AddWithValue("@ADDRESS", address);
                    sql.Parameters.AddWithValue("@ADDRESSENG", addressEng);
                    sql.Parameters.AddWithValue("@TEL", tel);
                    sql.Parameters.AddWithValue("@FAX", fax);
                    sql.Parameters.AddWithValue("@FBLINK", fbLink);
                    sql.Parameters.AddWithValue("@MERCHANTID", merchantId);
                    sql.Parameters.AddWithValue("@TERMINALID", terminalId);
                    sql.Parameters.AddWithValue("@GOOGLEMAP", googleMap);
                    sql.Parameters.AddWithValue("@BANKCODE", bankCode);
                    sql.Parameters.AddWithValue("@BANKNAME", bankName);
                    sql.Parameters.AddWithValue("@BANKNAMEENG", bankNameEng);
                    sql.Parameters.AddWithValue("@ACCOUNTNAME", accountName);
                    sql.Parameters.AddWithValue("@ACCOUNTNAMEENG", accountNameEng);
                    sql.Parameters.AddWithValue("@ACCOUNTNUM", accountNum);
                    sql.Parameters.AddWithValue("@MAIL", mail);
                    DataTable dt = db.GetResult(sql);

                    string sn = dt.Rows[0]["SN"].ToString();

                    sql = new SqlCommand("INSERT EVENTLOG ([MODULE], [ACTION], [REMARK]) VALUES (@MODULE, @ACTION, @REMARK);");
                    sql.Parameters.AddWithValue("@MODULE", "聯絡資訊管理");
                    sql.Parameters.AddWithValue("@ACTION", "新增");
                    sql.Parameters.AddWithValue("@REMARK", $"sn={sn};name={name};address={address};addressEng={addressEng};tel={tel};fax={fax};fbLink={fbLink};merchantId={merchantId};terminalId={terminalId};googleMap={googleMap};bankCode={bankCode};bankName={bankName};bankNameEng={bankNameEng};accountName={accountName};accountNameEng={accountNameEng};accountNum={accountNum};mail={mail}");
                    db.Execute(sql);

                    db.CloseConn();
                }
                catch (Exception e)
                {
                    if (db.SqlConn.State != ConnectionState.Closed)
                        db.CloseConn();

                    ecLog.Log.Record(ecLog.Log.InfoType.Error, $"{e.Message};{e.StackTrace}", nameof(AddContact));
                }

            }
            return RedirectToAction("Contact", "Home");
        }
        [HttpPost]
        public ActionResult EditContact(string id, FormCollection form)
        {
            try
            {
                string name = form["editTitle"];
                string address = form["editAddress"];
                string addressEng = form["editAddressEng"];
                string tel = form["editTel"];
                string fax = form["editFax"];
                string fbLink = form["editFBLink"];
                string merchantId = form["editMerchantId"];
                string terminalId = form["editTerminalId"];
                string googleMap = form["editGoogleMap"];
                string bankCode = form["editBankCode"];
                string bankName = form["editBankName"];
                string bankNameEng = form["editBankNameEng"];
                string accountName = form["editAccountName"];
                string accountNameEng = form["editAccountNameEng"];
                string accountNum = form["editAccountNum"];
                string mail = form["editMail"];

                googleMap = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(googleMap));
                addressEng = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(addressEng));
                bankNameEng = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(bankNameEng));
                accountNameEng = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(accountNameEng));

                db.OpenConn();

                SqlCommand sql = new SqlCommand("UPDATE BRANCH SET NAME = @NAME, ADDRESS = @ADDRESS, ADDRESSENG = @ADDRESSENG, TEL = @TEL, FAX = @FAX, FBLINK = @FBLINK, MERCHANTID = @MERCHANTID, TERMINALID = @TERMINALID, GOOGLEMAP = @GOOGLEMAP, BANKNAME = @BANKNAME, BANKNAMEENG = @BANKNAMEENG, BANKCODE = @BANKCODE, ACCOUNTNAME = @ACCOUNTNAME, ACCOUNTNAMEENG = @ACCOUNTNAMEENG, ACCOUNTNUM = @ACCOUNTNUM, MAIL = @MAIL WHERE SN = @SN");
                sql.Parameters.AddWithValue("@NAME", name);
                sql.Parameters.AddWithValue("@ADDRESS", address);
                sql.Parameters.AddWithValue("@ADDRESSENG", addressEng);
                sql.Parameters.AddWithValue("@TEL", tel);
                sql.Parameters.AddWithValue("@FAX", fax);
                sql.Parameters.AddWithValue("@FBLINK", fbLink);
                sql.Parameters.AddWithValue("@MERCHANTID", merchantId);
                sql.Parameters.AddWithValue("@TERMINALID", terminalId);
                sql.Parameters.AddWithValue("@GOOGLEMAP", googleMap);
                sql.Parameters.AddWithValue("@BANKCODE", bankCode);
                sql.Parameters.AddWithValue("@BANKNAME", bankName);
                sql.Parameters.AddWithValue("@BANKNAMEENG", bankNameEng);
                sql.Parameters.AddWithValue("@ACCOUNTNAME", accountName);
                sql.Parameters.AddWithValue("@ACCOUNTNAMEENG", accountNameEng);
                sql.Parameters.AddWithValue("@ACCOUNTNUM", accountNum);
                sql.Parameters.AddWithValue("@MAIL", mail);
                sql.Parameters.AddWithValue("@SN", id);
                db.Execute(sql);

                sql = new SqlCommand("INSERT EVENTLOG ([MODULE], [ACTION], [REMARK]) VALUES (@MODULE, @ACTION, @REMARK);");
                sql.Parameters.AddWithValue("@MODULE", "聯絡資訊管理");
                sql.Parameters.AddWithValue("@ACTION", "修改");
                sql.Parameters.AddWithValue("@REMARK", $"sn={id};name={name};address={address};addressEng={addressEng};tel={tel};fax={fax};fbLink={fbLink};merchantId={merchantId};terminalId={terminalId};googleMap={googleMap};bankCode={bankCode};bankName={bankName};bankNameEng={bankNameEng};accountName={accountName};accountNameEng={accountNameEng};accountNum={accountNum};mail={mail}");
                db.Execute(sql);

                db.CloseConn();

                return Content("OK");
            }
            catch (Exception e)
            {
                if (db.SqlConn.State != ConnectionState.Closed)
                    db.CloseConn();

                ecLog.Log.Record(ecLog.Log.InfoType.Error, $"{e.Message};{e.StackTrace}", nameof(EditContact));

                return Content("NG");
            }
        }
        [HttpPost]
        public ActionResult DeleteContact(string id)
        {
            try
            {
                db.OpenConn();

                SqlCommand sql = new SqlCommand("DELETE FROM BRANCH WHERE SN = @SN");
                sql.Parameters.AddWithValue("@SN", id);
                db.Execute(sql);

                sql = new SqlCommand("INSERT EVENTLOG ([MODULE], [ACTION], [REMARK]) VALUES (@MODULE, @ACTION, @REMARK);");
                sql.Parameters.AddWithValue("@MODULE", "聯絡資訊管理");
                sql.Parameters.AddWithValue("@ACTION", "刪除");
                sql.Parameters.AddWithValue("@REMARK", $"{id}");
                db.Execute(sql);

                db.CloseConn();

                return Content("OK");
            }
            catch (Exception e)
            {
                if (db.SqlConn.State != ConnectionState.Closed)
                    db.CloseConn();

                ecLog.Log.Record(ecLog.Log.InfoType.Error, $"{e.Message};{e.StackTrace}", nameof(DeleteContact));

                return Content("NG");
            }
        }
        [HttpPost]
        public ActionResult GetContact(string id)
        {
            Contact obj = new Contact();
            try
            {
                SqlCommand sql = new SqlCommand(@"
                    SELECT 
                        SN, NAME, ADDRESS, ADDRESSENG, TEL, FAX, FBLINK, MAIL,
                        MERCHANTID, TERMINALID, GOOGLEMAP, 
                        BANKNAME, BANKNAMEENG, BANKCODE, ACCOUNTNAME, ACCOUNTNAMEENG, ACCOUNTNUM,
                        CREATEDATE 
                    FROM BRANCH WHERE SN = @SN");
                sql.Parameters.AddWithValue("@SN", id);
                DataTable dt = db.GetResult(sql);
                DataRow dr = dt.Rows[0];

                obj = new Contact()
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
                };

                return Content(JsonConvert.SerializeObject(obj));
            }
            catch (Exception e)
            {
                if (db.SqlConn.State != ConnectionState.Closed)
                    db.CloseConn();

                ecLog.Log.Record(ecLog.Log.InfoType.Error, $"{e.Message};{e.StackTrace}", nameof(GetContact));
            }
            return Content(JsonConvert.SerializeObject(obj));
        }
        #endregion

        #region 友善連結
        [HttpPost]
        public ActionResult AddFavouriteLink(FormCollection form)
        {
            if (form.HasKeys())
            {
                string name = form["addContactTitle"];
                string link = form["addFriendlyLink"];

                // 新增
                try
                {
                    db.OpenConn();

                    SqlCommand sql = new SqlCommand("INSERT INTO FAVOURITELINK (NAME, LINK) OUTPUT INSERTED.SN VALUES (@NAME, @LINK);");
                    sql.Parameters.AddWithValue("@NAME", name);
                    sql.Parameters.AddWithValue("@LINK", link);
                    DataTable dt = db.GetResult(sql);

                    string sn = dt.Rows[0]["SN"].ToString();

                    sql = new SqlCommand("INSERT EVENTLOG ([MODULE], [ACTION], [REMARK]) VALUES (@MODULE, @ACTION, @REMARK);");
                    sql.Parameters.AddWithValue("@MODULE", "友善連結");
                    sql.Parameters.AddWithValue("@ACTION", "新增");
                    sql.Parameters.AddWithValue("@REMARK", $"sn={sn};name={name};link={link}");
                    db.Execute(sql);

                    db.CloseConn();
                }
                catch (Exception e)
                {
                    if (db.SqlConn.State != ConnectionState.Closed)
                        db.CloseConn();

                    ecLog.Log.Record(ecLog.Log.InfoType.Error, $"{e.Message};{e.StackTrace}", nameof(AddFavouriteLink));
                }

            }
            return RedirectToAction("FavouriteLink", "Home");
        }
        [HttpPost]
        public ActionResult EditFavouriteLink(string id, FormCollection form)
        {
            try
            {
                string name = form["editTitle"];
                string link = form["editLinkURL"];
                string sort = form["editSort"];

                db.OpenConn();

                SqlCommand sql = new SqlCommand("UPDATE FAVOURITELINK SET NAME = @NAME, LINK = @LINK, SORT = @SORT WHERE SN = @SN");
                sql.Parameters.AddWithValue("@NAME", name);
                sql.Parameters.AddWithValue("@LINK", link);
                sql.Parameters.AddWithValue("@SORT", sort);
                sql.Parameters.AddWithValue("@SN", id);
                db.Execute(sql);

                sql = new SqlCommand("INSERT EVENTLOG ([MODULE], [ACTION], [REMARK]) VALUES (@MODULE, @ACTION, @REMARK);");
                sql.Parameters.AddWithValue("@MODULE", "友善連結");
                sql.Parameters.AddWithValue("@ACTION", "修改");
                sql.Parameters.AddWithValue("@REMARK", $"sn={id};name={name};link={link};sort={sort}");
                db.Execute(sql);

                db.CloseConn();

                return Content("OK");
            }
            catch (Exception e)
            {
                if (db.SqlConn.State != ConnectionState.Closed)
                    db.CloseConn();

                ecLog.Log.Record(ecLog.Log.InfoType.Error, $"{e.Message};{e.StackTrace}", nameof(EditFavouriteLink));

                return Content("NG");
            }
        }
        [HttpPost]
        public ActionResult DeleteFavouriteLink(string id)
        {
            try
            {
                db.OpenConn();

                SqlCommand sql = new SqlCommand("DELETE FROM FAVOURITELINK WHERE SN = @SN");
                sql.Parameters.AddWithValue("@SN", id);
                db.Execute(sql);

                sql = new SqlCommand("INSERT EVENTLOG ([MODULE], [ACTION], [REMARK]) VALUES (@MODULE, @ACTION, @REMARK);");
                sql.Parameters.AddWithValue("@MODULE", "友善連結");
                sql.Parameters.AddWithValue("@ACTION", "刪除");
                sql.Parameters.AddWithValue("@REMARK", $"{id}");
                db.Execute(sql);

                db.CloseConn();

                return Content("OK");
            }
            catch (Exception e)
            {
                if (db.SqlConn.State != ConnectionState.Closed)
                    db.CloseConn();

                ecLog.Log.Record(ecLog.Log.InfoType.Error, $"{e.Message};{e.StackTrace}", nameof(DeleteFavouriteLink));

                return Content("NG");
            }
        }
        #endregion

        #region 訂閱管理
        [HttpPost]
        public ActionResult AddSubscription(FormCollection form)
        {
            if (form.HasKeys())
            {
                string mail = form["addGenre"];

                // 新增
                try
                {
                    db.OpenConn();

                    SqlCommand sql = new SqlCommand("INSERT INTO SUBSCRIPTION (MAIL) OUTPUT INSERTED.SN VALUES (@MAIL);");
                    sql.Parameters.AddWithValue("@MAIL", mail);
                    DataTable dt = db.GetResult(sql);

                    string sn = dt.Rows[0]["SN"].ToString();

                    sql = new SqlCommand("INSERT EVENTLOG ([MODULE], [ACTION], [REMARK]) VALUES (@MODULE, @ACTION, @REMARK);");
                    sql.Parameters.AddWithValue("@MODULE", "訂閱管理");
                    sql.Parameters.AddWithValue("@ACTION", "新增");
                    sql.Parameters.AddWithValue("@REMARK", $"sn={sn};mail={mail}");
                    db.Execute(sql);

                    db.CloseConn();

                    return Content("OK");
                }
                catch (Exception e)
                {
                    if (db.SqlConn.State != ConnectionState.Closed)
                        db.CloseConn();

                    ecLog.Log.Record(ecLog.Log.InfoType.Error, $"{e.Message};{e.StackTrace}", nameof(AddSubscription));

                    return Content("NG");
                }
            }
            else
                return Content("NG");
        }
        [HttpPost]
        public ActionResult EditSubscription(string id, FormCollection form)
        {
            try
            {
                string mail = form["editMail"];

                db.OpenConn();

                SqlCommand sql = new SqlCommand("UPDATE SUBSCRIPTION SET MAIL = @MAIL WHERE SN = @SN");
                sql.Parameters.AddWithValue("@MAIL", mail);
                sql.Parameters.AddWithValue("@SN", id);
                db.Execute(sql);

                sql = new SqlCommand("INSERT EVENTLOG ([MODULE], [ACTION], [REMARK]) VALUES (@MODULE, @ACTION, @REMARK);");
                sql.Parameters.AddWithValue("@MODULE", "訂閱管理");
                sql.Parameters.AddWithValue("@ACTION", "修改");
                sql.Parameters.AddWithValue("@REMARK", $"sn={id};mail={mail}");
                db.Execute(sql);

                db.CloseConn();

                return Content("OK");
            }
            catch (Exception e)
            {
                if (db.SqlConn.State != ConnectionState.Closed)
                    db.CloseConn();

                ecLog.Log.Record(ecLog.Log.InfoType.Error, $"{e.Message};{e.StackTrace}", nameof(EditSubscription));

                return Content("NG");
            }
        }
        [HttpPost]
        public ActionResult DeleteSubscription(string id)
        {
            try
            {
                db.OpenConn();

                SqlCommand sql = new SqlCommand("DELETE FROM SUBSCRIPTION WHERE SN = @SN");
                sql.Parameters.AddWithValue("@SN", id);
                db.Execute(sql);

                sql = new SqlCommand("INSERT EVENTLOG ([MODULE], [ACTION], [REMARK]) VALUES (@MODULE, @ACTION, @REMARK);");
                sql.Parameters.AddWithValue("@MODULE", "訂閱管理");
                sql.Parameters.AddWithValue("@ACTION", "刪除");
                sql.Parameters.AddWithValue("@REMARK", $"{id}");
                db.Execute(sql);

                db.CloseConn();

                return Content("OK");
            }
            catch (Exception e)
            {
                if (db.SqlConn.State != ConnectionState.Closed)
                    db.CloseConn();

                ecLog.Log.Record(ecLog.Log.InfoType.Error, $"{e.Message};{e.StackTrace}", nameof(DeleteSubscription));

                return Content("NG");
            }
        }
        #endregion

        #region 檔案下載管理
        [HttpPost]
        public ActionResult AddDownload(FormCollection form)
        {
            if (form.HasKeys())
            {
                string fileName = form["addContactTitle"];

                // 檔案
                HttpPostedFileBase fileWord = null;
                HttpPostedFileBase filePdf = null;
                if (Request.Files.Count > 0)
                {
                    for (int i = 0; i < Request.Files.AllKeys.Length; i++)
                    {
                        if (!Request.Files[i].ContentType.Equals("application/octet-stream"))
                        {
                            if (Request.Files.Keys[i].Equals("addFilesWord"))
                                fileWord = Request.Files[i];
                            else if (Request.Files.Keys[i].Equals("addFilesPDF"))
                                filePdf = Request.Files[i];
                        }
                    }
                }

                // 新增
                try
                {
                    db.OpenConn();

                    SqlCommand sql = new SqlCommand("INSERT INTO DOWNLOAD (FILENAME) OUTPUT INSERTED.SN VALUES (@FILENAME);");
                    sql.Parameters.AddWithValue("@FILENAME", fileName);
                    DataTable dt = db.GetResult(sql);

                    string sn = dt.Rows[0]["SN"].ToString();

                    sql = new SqlCommand("INSERT EVENTLOG ([MODULE], [ACTION], [REMARK]) VALUES (@MODULE, @ACTION, @REMARK);");
                    sql.Parameters.AddWithValue("@MODULE", "檔案下載管理");
                    sql.Parameters.AddWithValue("@ACTION", "新增");
                    sql.Parameters.AddWithValue("@REMARK", $"sn={sn};fileName={fileName}");
                    db.ExecuteNonCommit(sql);

                    // 建立檔案目錄
                    string dir = Server.MapPath($"~/Mime/Download/{sn}");
                    if (!Directory.Exists(dir))
                        Directory.CreateDirectory(dir);

                    if (fileWord != null)
                    {
                        string fileNameW = $"w_{sn}{Path.GetExtension(fileWord.FileName)}";

                        sql = new SqlCommand("INSERT EVENTLOG ([MODULE], [ACTION], [REMARK]) VALUES (@MODULE, @ACTION, @REMARK);");
                        sql.Parameters.AddWithValue("@MODULE", "檔案下載管理");
                        sql.Parameters.AddWithValue("@ACTION", "新增");
                        sql.Parameters.AddWithValue("@REMARK", $"sn={sn};word_file={fileNameW}");
                        db.ExecuteNonCommit(sql);

                        fileNameW = Path.Combine(dir, fileNameW);

                        if (System.IO.File.Exists(fileNameW))
                            System.IO.File.Delete(fileNameW);

                        fileWord.SaveAs(fileNameW);
                    }

                    if (filePdf != null)
                    {
                        string fileNameP = $"p_{sn}{Path.GetExtension(filePdf.FileName)}";

                        sql = new SqlCommand("INSERT EVENTLOG ([MODULE], [ACTION], [REMARK]) VALUES (@MODULE, @ACTION, @REMARK);");
                        sql.Parameters.AddWithValue("@MODULE", "檔案下載管理");
                        sql.Parameters.AddWithValue("@ACTION", "新增");
                        sql.Parameters.AddWithValue("@REMARK", $"sn={sn};pdf_file={fileNameP}");
                        db.ExecuteNonCommit(sql);

                        fileNameP = Path.Combine(dir, fileNameP);

                        if (System.IO.File.Exists(fileNameP))
                            System.IO.File.Delete(fileNameP);

                        filePdf.SaveAs(fileNameP);
                    }

                    db.Commit();
                    db.CloseConn();
                }
                catch (Exception e)
                {
                    if (db.SqlConn.State != ConnectionState.Closed)
                        db.CloseConn();

                    ecLog.Log.Record(ecLog.Log.InfoType.Error, $"{e.Message};{e.StackTrace}", nameof(AddDownload));
                }
            }
            return RedirectToAction("Download", "Home");
        }
        [HttpPost]
        public ActionResult EditDownload(string id, FormCollection form)
        {
            try
            {
                string fileName = form["editTitle"];

                db.OpenConn();

                SqlCommand sql = new SqlCommand("UPDATE Download SET FILENAME = @FILENAME WHERE SN = @SN");
                sql.Parameters.AddWithValue("@FILENAME", fileName);
                sql.Parameters.AddWithValue("@SN", id);
                db.Execute(sql);

                sql = new SqlCommand("INSERT EVENTLOG ([MODULE], [ACTION], [REMARK]) VALUES (@MODULE, @ACTION, @REMARK);");
                sql.Parameters.AddWithValue("@MODULE", "檔案下載管理");
                sql.Parameters.AddWithValue("@ACTION", "修改");
                sql.Parameters.AddWithValue("@REMARK", $"sn={id};fileName={fileName}");
                db.Execute(sql);

                db.CloseConn();

                return Content("OK");
            }
            catch (Exception e)
            {
                if (db.SqlConn.State != ConnectionState.Closed)
                    db.CloseConn();

                ecLog.Log.Record(ecLog.Log.InfoType.Error, $"{e.Message};{e.StackTrace}", nameof(EditDownload));

                return Content("NG");
            }
        }
        [HttpPost]
        public ActionResult DeleteDownload(string id)
        {
            try
            {
                db.OpenConn();

                SqlCommand sql = new SqlCommand("DELETE FROM DOWNLOAD WHERE SN = @SN");
                sql.Parameters.AddWithValue("@SN", id);
                db.Execute(sql);

                sql = new SqlCommand("INSERT EVENTLOG ([MODULE], [ACTION], [REMARK]) VALUES (@MODULE, @ACTION, @REMARK);");
                sql.Parameters.AddWithValue("@MODULE", "檔案下載管理");
                sql.Parameters.AddWithValue("@ACTION", "刪除");
                sql.Parameters.AddWithValue("@REMARK", $"{id}");
                db.Execute(sql);

                db.CloseConn();

                // 刪除目錄
                string dir = Server.MapPath($"~/Mime/Download/{id}");
                if (!Directory.Exists(dir))
                    Directory.Delete(dir);

                return Content("OK");
            }
            catch (Exception e)
            {
                if (db.SqlConn.State != ConnectionState.Closed)
                    db.CloseConn();

                ecLog.Log.Record(ecLog.Log.InfoType.Error, $"{e.Message};{e.StackTrace}", nameof(DeleteDownload));

                return Content("NG");
            }
        }

        public FileResult GetDownloadDoc(string id)
        {
            string file = Directory.GetFiles(Server.MapPath($"~/Mime/Download/{id}"), "w_*").FirstOrDefault();
            if (!string.IsNullOrEmpty(file))
            {
                string mimeType = Path.GetExtension(file).ToLower().Equals(".doc") ? "application/msword" : "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
                return new FileStreamResult(new FileStream(file, FileMode.Open), mimeType);
            }
            else
                return null;
        }
        public FileResult GetDownloadPdf(string id)
        {
            string file = Directory.GetFiles(Server.MapPath($"~/Mime/Download/{id}"), "p_*").FirstOrDefault();
            if (!string.IsNullOrEmpty(file))
            {
                string mimeType = "application/pdf";
                return new FileStreamResult(new FileStream(file, FileMode.Open), mimeType);
            }
            else
                return null;
        }
        #endregion

        #region 名單管理
        [HttpPost]
        public ActionResult AddMember(FormCollection form)
        {
            if (form.HasKeys())
            {
                string id = form["addMemNum"];
                string name = form["addTitle"];
                string areaId = form["addArea"];

                // 新增
                try
                {
                    SqlCommand sql = new SqlCommand("SELECT COUNT(*) CNT FROM MEMBER WHERE ID = @ID");
                    sql.Parameters.AddWithValue("@ID", id);
                    DataTable dt = db.GetResult(sql);

                    if (dt.Rows[0]["CNT"].ParseInt() == 0)
                    {
                        db.OpenConn();

                        sql = new SqlCommand("INSERT INTO MEMBER (ID, NAME, AREAID) VALUES (@ID, @NAME, @AREAID);");
                        sql.Parameters.AddWithValue("@ID", id);
                        sql.Parameters.AddWithValue("@NAME", name);
                        sql.Parameters.AddWithValue("@AREAID", areaId);
                        db.Execute(sql);

                        sql = new SqlCommand("INSERT EVENTLOG ([MODULE], [ACTION], [REMARK]) VALUES (@MODULE, @ACTION, @REMARK);");
                        sql.Parameters.AddWithValue("@MODULE", "名單管理");
                        sql.Parameters.AddWithValue("@ACTION", "新增");
                        sql.Parameters.AddWithValue("@REMARK", $"id={id};name={name};areaId={areaId}");
                        db.Execute(sql);

                        db.CloseConn();

                        return Content("OK");
                    }
                    else
                        return Content("NG;此會員編號已經存在！");
                }
                catch (Exception e)
                {
                    if (db.SqlConn.State != ConnectionState.Closed)
                        db.CloseConn();

                    ecLog.Log.Record(ecLog.Log.InfoType.Error, $"{e.Message};{e.StackTrace}", nameof(AddMember));

                    return Content("NG");
                }
            }
            else
                return Content("NG");
        }
        [HttpPost]
        public ActionResult EditMember(string id, FormCollection form)
        {
            try
            {
                string name = form["editTitle"];
                string areaId = form["editArea"];

                db.OpenConn();

                SqlCommand sql = new SqlCommand("UPDATE MEMBER SET NAME = @NAME, AREAID = @AREAID WHERE ID = @ID");
                sql.Parameters.AddWithValue("@NAME", name);
                sql.Parameters.AddWithValue("@AREAID", areaId);
                sql.Parameters.AddWithValue("@ID", id);
                db.Execute(sql);

                sql = new SqlCommand("INSERT EVENTLOG ([MODULE], [ACTION], [REMARK]) VALUES (@MODULE, @ACTION, @REMARK);");
                sql.Parameters.AddWithValue("@MODULE", "名單管理");
                sql.Parameters.AddWithValue("@ACTION", "修改");
                sql.Parameters.AddWithValue("@REMARK", $"id={id};name={name};areaId={areaId}");
                db.Execute(sql);

                db.CloseConn();

                return Content("OK");
            }
            catch (Exception e)
            {
                if (db.SqlConn.State != ConnectionState.Closed)
                    db.CloseConn();

                ecLog.Log.Record(ecLog.Log.InfoType.Error, $"{e.Message};{e.StackTrace}", nameof(EditMember));

                return Content("NG");
            }
        }
        [HttpPost]
        public ActionResult DeleteMember(string id)
        {
            try
            {
                db.OpenConn();

                SqlCommand sql = new SqlCommand("DELETE FROM MEMBER WHERE ID = @ID");
                sql.Parameters.AddWithValue("@ID", id);
                db.Execute(sql);

                sql = new SqlCommand("INSERT EVENTLOG ([MODULE], [ACTION], [REMARK]) VALUES (@MODULE, @ACTION, @REMARK);");
                sql.Parameters.AddWithValue("@MODULE", "名單管理");
                sql.Parameters.AddWithValue("@ACTION", "刪除");
                sql.Parameters.AddWithValue("@REMARK", $"{id}");
                db.Execute(sql);

                db.CloseConn();

                return Content("OK");
            }
            catch (Exception e)
            {
                if (db.SqlConn.State != ConnectionState.Closed)
                    db.CloseConn();

                ecLog.Log.Record(ecLog.Log.InfoType.Error, $"{e.Message};{e.StackTrace}", nameof(DeleteSubscription));

                return Content("NG");
            }
        }

        [HttpPost]
        public ActionResult ImportMember()
        {
            try
            {
                HttpPostedFileBase file = null;
                file = Request.Files.Count > 0 ? Request.Files[0] : null;

                if (file == null || !new string[] { "application/vnd.ms-excel", "text/csv" }.Contains(file.ContentType))
                    return Content("NG;檔案類型錯誤，請下載範例檔編輯後上傳！");

                DataTable dt = CsvToDataTable(file, ",");

                SqlCommand sql = new SqlCommand();
                DataTable rDt = new DataTable();
                
                // 欄位防呆
                foreach (DataColumn dc in dt.Columns)
                {
                    if (!new string[] { "會員編號", "會員名稱", "地區" }.Contains(dc.ColumnName))
                        return Content($"NG;欄位不齊全，請參考範例檔！");
                }
                // 防呆
                foreach (DataRow dr in dt.Rows)
                {
                    string id = dr["會員編號"].ToString().Trim();
                    string name = dr["會員名稱"].ToString().Trim();
                    string area = dr["地區"].ToString().Trim();

                    sql.CommandText = "SELECT COUNT(*) CNT FROM MEMBER WHERE ID = @ID";
                    sql.Parameters.Clear();
                    sql.Parameters.AddWithValue("@ID", id);
                    rDt = db.GetResult(sql);
                    if (rDt.Rows[0][0].ParseInt() > 0)
                    {
                        return Content($"NG;會員編號 {id} 已經存在名單內！");
                    }

                    sql.CommandText = "SELECT COUNT(*) CNT FROM AREA WHERE NAME = @NAME";
                    sql.Parameters.Clear();
                    sql.Parameters.AddWithValue("@NAME", area);
                    rDt = db.GetResult(sql);
                    if (rDt.Rows[0][0].ParseInt() == 0)
                    {
                        return Content($"NG;地區 {area} 不在地區選項內！");
                    }
                }

                db.OpenConn();

                foreach (DataRow dr in dt.Rows)
                {
                    string id = dr["會員編號"].ToString().Trim();
                    string name = dr["會員名稱"].ToString().Trim();
                    string area = dr["地區"].ToString().Trim();

                    sql.CommandText = "SELECT ID FROM AREA WHERE NAME = @NAME";
                    sql.Parameters.Clear();
                    sql.Parameters.AddWithValue("@NAME", area);
                    rDt = db.GetResult(sql);

                    int areaId = rDt.Rows[0][0].ParseInt();

                    sql = new SqlCommand("INSERT INTO MEMBER (ID, NAME, AREAID) VALUES (@ID, @NAME, @AREAID);");
                    sql.Parameters.AddWithValue("@ID", id);
                    sql.Parameters.AddWithValue("@NAME", name);
                    sql.Parameters.AddWithValue("@AREAID", areaId);
                    db.ExecuteNonCommit(sql);

                    sql = new SqlCommand("INSERT EVENTLOG ([MODULE], [ACTION], [REMARK]) VALUES (@MODULE, @ACTION, @REMARK);");
                    sql.Parameters.AddWithValue("@MODULE", "名單管理");
                    sql.Parameters.AddWithValue("@ACTION", "新增");
                    sql.Parameters.AddWithValue("@REMARK", $"id={id};name={name};areaId={areaId}");
                    db.ExecuteNonCommit(sql);
                }

                db.Commit();
                db.CloseConn();

                return Content("OK");
            }
            catch (Exception e)
            {
                if (db.SqlConn.State != ConnectionState.Closed)
                    db.CloseConn();

                ecLog.Log.Record(ecLog.Log.InfoType.Error, $"{e.Message};{e.StackTrace}", nameof(ImportMember));

                return Content("NG");
            }
        }
        #endregion

        #region 影片管理
        [HttpPost]
        public ActionResult AddVideoType(FormCollection form)
        {
            if (form.HasKeys())
            {
                string name = form["addGenre"];

                // 新增
                try
                {
                    SqlCommand sql = new SqlCommand("SELECT COUNT(*) CNT FROM VIDEOTYPE WHERE NAME = @NAME");
                    sql.Parameters.AddWithValue("@NAME", name);
                    DataTable dt = db.GetResult(sql);

                    if (dt.Rows[0]["CNT"].ParseInt() == 0)
                    {
                        db.OpenConn();

                        sql = new SqlCommand("INSERT INTO VIDEOTYPE (NAME) OUTPUT INSERTED.SN VALUES (@NAME);");
                        sql.Parameters.AddWithValue("@NAME", name);
                        dt = db.GetResult(sql);

                        string sn = dt.Rows[0]["SN"].ToString();

                        sql = new SqlCommand("INSERT EVENTLOG ([MODULE], [ACTION], [REMARK]) VALUES (@MODULE, @ACTION, @REMARK);");
                        sql.Parameters.AddWithValue("@MODULE", "影片管理");
                        sql.Parameters.AddWithValue("@ACTION", "新增");
                        sql.Parameters.AddWithValue("@REMARK", $"sn={sn};name={name}");
                        db.Execute(sql);

                        db.CloseConn();

                        return Content($"OK;{sn}");
                    }
                    else
                        return Content("NG;此分類名稱已經存在！");
                }
                catch (Exception e)
                {
                    if (db.SqlConn.State != ConnectionState.Closed)
                        db.CloseConn();

                    ecLog.Log.Record(ecLog.Log.InfoType.Error, $"{e.Message};{e.StackTrace}", nameof(AddVideoType));

                    return Content("NG");
                }
            }
            else
                return Content("NG");
        }
        [HttpPost]
        public ActionResult EditVideoType(string id, FormCollection form)
        {
            try
            {
                string name = form["editGenre"];

                db.OpenConn();

                SqlCommand sql = new SqlCommand("UPDATE VIDEOTYPE SET NAME = @NAME WHERE SN = @SN");
                sql.Parameters.AddWithValue("@NAME", name);
                sql.Parameters.AddWithValue("@SN", id);
                db.Execute(sql);

                sql = new SqlCommand("INSERT EVENTLOG ([MODULE], [ACTION], [REMARK]) VALUES (@MODULE, @ACTION, @REMARK);");
                sql.Parameters.AddWithValue("@MODULE", "影片管理");
                sql.Parameters.AddWithValue("@ACTION", "修改");
                sql.Parameters.AddWithValue("@REMARK", $"sn={id};name={name}");
                db.Execute(sql);

                db.CloseConn();

                return Content($"OK;{id}");
            }
            catch (Exception e)
            {
                if (db.SqlConn.State != ConnectionState.Closed)
                    db.CloseConn();

                ecLog.Log.Record(ecLog.Log.InfoType.Error, $"{e.Message};{e.StackTrace}", nameof(EditVideoType));

                return Content("NG");
            }
        }
        [HttpPost]
        public ActionResult DeleteVideoType(string id)
        {
            try
            {
                db.OpenConn();

                SqlCommand sql = new SqlCommand("DELETE FROM VIDEOTYPE WHERE SN = @SN");
                sql.Parameters.AddWithValue("@SN", id);
                db.ExecuteNonCommit(sql);

                sql = new SqlCommand("DELETE FROM VIDEO WHERE TYPEID = @TYPEID");
                sql.Parameters.AddWithValue("@TYPEID", id);
                db.ExecuteNonCommit(sql);

                sql = new SqlCommand("INSERT EVENTLOG ([MODULE], [ACTION], [REMARK]) VALUES (@MODULE, @ACTION, @REMARK);");
                sql.Parameters.AddWithValue("@MODULE", "影片管理");
                sql.Parameters.AddWithValue("@ACTION", "刪除");
                sql.Parameters.AddWithValue("@REMARK", $"{id}");
                db.ExecuteNonCommit(sql);

                db.Commit();
                db.CloseConn();

                return Content($"OK;{id}");
            }
            catch (Exception e)
            {
                if (db.SqlConn.State != ConnectionState.Closed)
                    db.CloseConn();

                ecLog.Log.Record(ecLog.Log.InfoType.Error, $"{e.Message};{e.StackTrace}", nameof(DeleteVideoType));

                return Content("NG");
            }
        }
        [HttpPost]
        public ActionResult AddVideo(FormCollection form)
        {
            if (form.HasKeys())
            {
                string id = form["typeid"];
                string title = form["addTitle"];
                string p1 = form["addMoviesContent1"];
                string p2 = form["addMoviesContent2"];
                string lnk = form["addMovieURL"];

                lnk = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(lnk));

                // 標題圖片
                HttpPostedFileBase titleImg = null;

                if (Request.Files.Count > 0 && !Request.Files[0].ContentType.Equals("application/octet-stream"))
                    titleImg = Request.Files[0];

                // 新增
                try
                {
                    db.OpenConn();

                    SqlCommand sql = new SqlCommand("INSERT INTO VIDEO (TYPEID, NAME, CONTENT1, CONTENT2, LINK) OUTPUT INSERTED.SN VALUES (@TYPEID, @NAME, @CONTENT1, @CONTENT2, @LINK);");
                    sql.Parameters.AddWithValue("@TYPEID", id);
                    sql.Parameters.AddWithValue("@NAME", title);
                    sql.Parameters.AddWithValue("@CONTENT1", p1);
                    sql.Parameters.AddWithValue("@CONTENT2", p2);
                    sql.Parameters.AddWithValue("@LINK", lnk);
                    DataTable dt = db.GetResult(sql);

                    string sn = dt.Rows[0]["SN"].ToString();

                    sql = new SqlCommand("INSERT EVENTLOG ([MODULE], [ACTION], [REMARK]) VALUES (@MODULE, @ACTION, @REMARK);");
                    sql.Parameters.AddWithValue("@MODULE", "影片管理");
                    sql.Parameters.AddWithValue("@ACTION", "新增影片");
                    sql.Parameters.AddWithValue("@REMARK", $"sn={sn};typeid={id};name={title};p1={p1};p2={p2};lnk={lnk}");
                    db.Execute(sql);

                    // 建立檔案目錄
                    string dir = Server.MapPath($"~/Mime/Video/{sn}");
                    if (!Directory.Exists(dir))
                        Directory.CreateDirectory(dir);

                    // 封面圖
                    if (titleImg != null)
                    {
                        string fileName = $"t_{sn}{Path.GetExtension(titleImg.FileName)}";

                        sql = new SqlCommand("UPDATE VIDEO SET TITLEIMGNAME = @IMGNAME WHERE SN = @SN");
                        sql.Parameters.AddWithValue("@IMGNAME", fileName);
                        sql.Parameters.AddWithValue("@SN", sn);
                        db.ExecuteNonCommit(sql);

                        sql = new SqlCommand("INSERT EVENTLOG ([MODULE], [ACTION], [REMARK]) VALUES (@MODULE, @ACTION, @REMARK);");
                        sql.Parameters.AddWithValue("@MODULE", "影片管理");
                        sql.Parameters.AddWithValue("@ACTION", "新增影片");
                        sql.Parameters.AddWithValue("@REMARK", $"sn={sn};title_file={fileName}");
                        db.ExecuteNonCommit(sql);

                        fileName = Path.Combine(dir, fileName);

                        if (System.IO.File.Exists(fileName))
                            System.IO.File.Delete(fileName);

                        titleImg.SaveAs(fileName);
                    }

                    db.CloseConn();

                    return Content($"OK;{sn}");
                }
                catch (Exception e)
                {
                    if (db.SqlConn.State != ConnectionState.Closed)
                        db.CloseConn();

                    ecLog.Log.Record(ecLog.Log.InfoType.Error, $"{e.Message};{e.StackTrace}", nameof(AddVideo));

                    return Content("NG");
                }
            }
            else
                return Content("NG");
        }
        [HttpPost]
        public ActionResult EditVideo(string id, FormCollection form)
        {
            if (form.HasKeys())
            {
                string title = form["editTitle"];
                string p1 = form["editContent1"];
                string p2 = form["editContent2"];
                string lnk = form["editLinkURL"];

                lnk = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(lnk));

                // 標題圖片
                HttpPostedFileBase titleImg = null;

                if (Request.Files.Count > 0 && !Request.Files[0].ContentType.Equals("application/octet-stream"))
                    titleImg = Request.Files[0];

                // 新增
                try
                {
                    db.OpenConn();

                    SqlCommand sql = new SqlCommand("UPDATE VIDEO SET NAME = @NAME, CONTENT1 = @CONTENT1, CONTENT2 = @CONTENT2, LINK = @LINK WHERE SN = @SN");
                    sql.Parameters.AddWithValue("@NAME", title);
                    sql.Parameters.AddWithValue("@CONTENT1", p1);
                    sql.Parameters.AddWithValue("@CONTENT2", p2);
                    sql.Parameters.AddWithValue("@LINK", lnk);
                    sql.Parameters.AddWithValue("@SN", id);
                    db.ExecuteNonCommit(sql);

                    sql = new SqlCommand("INSERT EVENTLOG ([MODULE], [ACTION], [REMARK]) VALUES (@MODULE, @ACTION, @REMARK);");
                    sql.Parameters.AddWithValue("@MODULE", "影片管理");
                    sql.Parameters.AddWithValue("@ACTION", "修改影片");
                    sql.Parameters.AddWithValue("@REMARK", $"sn={id};name={title};p1={p1};p2={p2};lnk={lnk}");
                    db.ExecuteNonCommit(sql);

                    // 建立檔案目錄
                    string dir = Server.MapPath($"~/Mime/Video/{id}");
                    if (!Directory.Exists(dir))
                        Directory.CreateDirectory(dir);

                    // 封面圖
                    if (titleImg != null)
                    {
                        string fileName = $"t_{id}{Path.GetExtension(titleImg.FileName)}";

                        sql = new SqlCommand("UPDATE VIDEO SET TITLEIMGNAME = @IMGNAME WHERE SN = @SN");
                        sql.Parameters.AddWithValue("@IMGNAME", fileName);
                        sql.Parameters.AddWithValue("@SN", id);
                        db.ExecuteNonCommit(sql);

                        sql = new SqlCommand("INSERT EVENTLOG ([MODULE], [ACTION], [REMARK]) VALUES (@MODULE, @ACTION, @REMARK);");
                        sql.Parameters.AddWithValue("@MODULE", "影片管理");
                        sql.Parameters.AddWithValue("@ACTION", "修改影片");
                        sql.Parameters.AddWithValue("@REMARK", $"sn={id};重新上傳封面圖");
                        db.ExecuteNonCommit(sql);

                        fileName = Path.Combine(dir, fileName);

                        if (System.IO.File.Exists(fileName))
                            System.IO.File.Delete(fileName);

                        titleImg.SaveAs(fileName);
                    }

                    db.Commit();

                    db.CloseConn();

                    return Content($"OK");
                }
                catch (Exception e)
                {
                    if (db.SqlConn.State != ConnectionState.Closed)
                        db.CloseConn();

                    ecLog.Log.Record(ecLog.Log.InfoType.Error, $"{e.Message};{e.StackTrace}", nameof(EditVideo));

                    return Content("NG");
                }
            }
            else
                return Content("NG");
        }
        [HttpPost]
        public ActionResult DeleteVideo(string id)
        {
            try
            {
                db.OpenConn();

                SqlCommand sql = new SqlCommand("DELETE FROM VIDEO WHERE SN = @SN");
                sql.Parameters.AddWithValue("@SN", id);
                db.Execute(sql);

                sql = new SqlCommand("INSERT EVENTLOG ([MODULE], [ACTION], [REMARK]) VALUES (@MODULE, @ACTION, @REMARK);");
                sql.Parameters.AddWithValue("@MODULE", "影片管理");
                sql.Parameters.AddWithValue("@ACTION", "刪除影片");
                sql.Parameters.AddWithValue("@REMARK", $"{id}");
                db.Execute(sql);

                db.CloseConn();

                // 刪除目錄
                string dir = Server.MapPath($"~/Mime/Video/{id}");
                if (!Directory.Exists(dir))
                    Directory.Delete(dir);

                return Content("OK");
            }
            catch (Exception e)
            {
                if (db.SqlConn.State != ConnectionState.Closed)
                    db.CloseConn();

                ecLog.Log.Record(ecLog.Log.InfoType.Error, $"{e.Message};{e.StackTrace}", nameof(DeleteVideo));

                return Content("NG");
            }
        }

        public ActionResult VideoTypeTab(string id)
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
                    CreateDate = dr["CREATEDATE"].ParseDateTime(),
                    Focus = (id == dr["SN"].ToString())
                });
            }

            return View(result);
        }
        public ActionResult VideoTypeTabContent(string id)
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
                    CreateDate = dr["CREATEDATE"].ParseDateTime(),
                    Focus = (id == dr["SN"].ToString())
                });
            }

            return View(result);
        }
        public ActionResult VideoBlock(string id)
        {
            SqlCommand sql = new SqlCommand("SELECT SN, NAME, CONTENT1, CONTENT2, LINK, CREATEDATE FROM VIDEO WHERE TYPEID = @TYPEID ORDER BY SN DESC");
            sql.Parameters.AddWithValue("@TYPEID", id);
            DataTable dt = db.GetResult(sql);

            List<Video> result = new List<Video>();
            foreach (DataRow dr in dt.Rows)
            {
                result.Add(new Video()
                {
                    Sn = dr["SN"].ParseInt(),
                    Name = dr["NAME"].ToString(),
                    P1 = dr["CONTENT1"].ToString(),
                    P2 = dr["CONTENT2"].ToString(),
                    Lnk = dr["LINK"].ToString(),
                    CreateDate = dr["CREATEDATE"].ParseDateTime()
                });
            }

            return View(result);
        }
        public FileResult GetVideoImage(string id)
        {
            SqlCommand sql = new SqlCommand("SELECT TITLEIMGNAME FROM VIDEO WHERE SN = @SN");
            sql.Parameters.AddWithValue("@SN", id);
            DataTable dt = db.GetResult(sql);
            DataRow dr = dt.Rows[0];

            if (!string.IsNullOrEmpty(dr["TITLEIMGNAME"].ToString()))
            {
                string file = Server.MapPath($"~/Mime/Video/{id}/{dr["TITLEIMGNAME"].ToString()}");
                return new FileStreamResult(new FileStream(file, FileMode.Open), "image/jpeg");
            }
            else
                return null;
        }
        #endregion

        #region 捐款紀錄
        [HttpPost]
        public ActionResult DeleteDonateRecord()
        {
            if (Request.Form.HasKeys())
            {
                try
                {
                    string Ids = Request.Form["Ids"];

                    db.OpenConn();
                    SqlCommand sql = new SqlCommand();
                    foreach (string id in Ids.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries))
                    {
                        sql = new SqlCommand("DELETE FROM ORDERMASTER WHERE ORDERID = @ORDERID");
                        sql.Parameters.AddWithValue("@ORDERID", id);
                        db.ExecuteNonCommit(sql);

                        sql = new SqlCommand("DELETE FROM ORDERDETAIL WHERE ORDERID = @ORDERID");
                        sql.Parameters.AddWithValue("@ORDERID", id);
                        db.ExecuteNonCommit(sql);

                        sql = new SqlCommand("INSERT EVENTLOG ([MODULE], [ACTION], [REMARK]) VALUES (@MODULE, @ACTION, @REMARK);");
                        sql.Parameters.AddWithValue("@MODULE", "捐款紀錄");
                        sql.Parameters.AddWithValue("@ACTION", "刪除");
                        sql.Parameters.AddWithValue("@REMARK", $"{id}");
                        db.ExecuteNonCommit(sql);
                    }
                    db.Commit();
                    db.CloseConn();

                    return Content($"OK");
                }
                catch (Exception e)
                {
                    if (db.SqlConn.State != ConnectionState.Closed)
                        db.CloseConn();

                    ecLog.Log.Record(ecLog.Log.InfoType.Error, $"{e.Message};{e.StackTrace}", nameof(DeleteDonateRecord));

                    return Content("NG");
                }
            }
            else
            {
                return Content("NG");
            }
        }
        [HttpPost]
        public ActionResult GetDonateRecord(string id)
        {
            DonateRecord obj = new DonateRecord();
            try
            {
                SqlCommand sql = new SqlCommand(@"
                SELECT 
	                BT.NAME BUYERTYPE,
	                O.BUYERNAME, 
	                O.BUYERID,
	                O.BUYERPHONE,
	                O.BUYEREMAIL,
	                PT.NAME PAYTYPE,
	                B.NAME BRANCH,
	                O.DONATEAMOUNT,
	
	                O.DONATEFOR,
	                O.NEEDRECEIPT,
	                O.NEEDANONYMOUS,
	                O.RECEIPTADDRESS,

	                O.PICKUPMETHOD,
	                O.REMARK
                FROM ORDERMASTER O
                JOIN ORDERTYPE OT ON O.ORDERTYPEID = OT.ID
                JOIN ORDERSTATUS OS ON O.ORDERSTATUS = OS.ID
                JOIN PAYSTATUS PS ON O.PAYSTATUS = PS.ID
                JOIN BUYERTYPE BT ON O.BUYERTYPEID = BT.ID
                JOIN BRANCH B ON O.DONATEUNIT = B.SN
                LEFT JOIN PAYTYPE PT ON O.PAYTYPE = PT.ID
                WHERE O.ORDERID = @ORDERID");
                sql.Parameters.AddWithValue("@ORDERID", id);
                DataTable dt = db.GetResult(sql);
                DataRow dr = dt.Rows[0];

                obj = new DonateRecord()
                {
                    BuyerType = dr["BUYERTYPE"].ToString(),
                    BuyerName = dr["BUYERNAME"].ToString(),
                    BuyerId = dr["BUYERID"].ToString(),
                    BuyerPhone = dr["BUYERPHONE"].ToString(),
                    BuyerMail = dr["BUYEREMAIL"].ToString(),
                    PayType = dr["PAYTYPE"].ToString(),
                    Branch = dr["BRANCH"].ToString(),
                    DonateAmount = dr["DONATEAMOUNT"].ParseInt(),
                    DonateUnit = dr["BRANCH"].ToString(),

                    DonateFor = dr["DONATEFOR"].ToString(),
                    NeedReceipt = dr["NEEDRECEIPT"].ToString(),
                    NeedAnonymous = dr["NEEDANONYMOUS"].ToString(),
                    ReceiptAddress = dr["RECEIPTADDRESS"].ToString(),

                    PickupMethod = dr["PICKUPMETHOD"].ToString(),
                    Remark = dr["REMARK"].ToString()
                };

                sql = new SqlCommand("SELECT ITEMNAME, QTY FROM ORDERDETAIL WHERE ORDERID = @ORDERID");
                sql.Parameters.AddWithValue("@ORDERID", id);
                dt = db.GetResult(sql);

                // 商品
                obj.Products = new List<Product>();
                foreach (DataRow r in dt.Rows)
                {
                    obj.Products.Add(new Product()
                    {
                        Name = r["ITEMNAME"].ToString(),
                        Qty = r["QTY"].ParseInt()
                    });
                }

                return Content(JsonConvert.SerializeObject(obj));
            }
            catch (Exception e)
            {
                if (db.SqlConn.State != ConnectionState.Closed)
                    db.CloseConn();

                ecLog.Log.Record(ecLog.Log.InfoType.Error, $"{e.Message};{e.StackTrace}", nameof(GetDonateRecord));
            }
            return Content(JsonConvert.SerializeObject(obj));
        }
        [HttpPost]
        public ActionResult ToClose(string id)
        {
            try
            {
                db.OpenConn();

                SqlCommand sql = new SqlCommand("UPDATE ORDERMASTER SET ORDERSTATUS = 2 WHERE ORDERID = @ORDERID");
                sql.Parameters.AddWithValue("@ORDERID", id);
                db.Execute(sql);

                sql = new SqlCommand("INSERT EVENTLOG ([MODULE], [ACTION], [REMARK]) VALUES (@MODULE, @ACTION, @REMARK);");
                sql.Parameters.AddWithValue("@MODULE", "捐款紀錄");
                sql.Parameters.AddWithValue("@ACTION", "修改");
                sql.Parameters.AddWithValue("@REMARK", $"orderStatus=2");
                db.Execute(sql);

                db.CloseConn();

                return Content("OK");
            }
            catch (Exception e)
            {
                if (db.SqlConn.State != ConnectionState.Closed)
                    db.CloseConn();

                ecLog.Log.Record(ecLog.Log.InfoType.Error, $"{e.Message};{e.StackTrace}", nameof(ToClose));

                return Content("NG");
            }
        }
        [HttpPost]
        public ActionResult ToPayEnd(string id)
        {
            try
            {
                db.OpenConn();

                DateTime now = DateTime.Now;

                SqlCommand sql = new SqlCommand("UPDATE ORDERMASTER SET PAYSTATUS = 2, PAYDATE = @PAYDATE WHERE ORDERID = @ORDERID");
                sql.Parameters.AddWithValue("@PAYDATE", now.ToString("yyyy/MM/dd HH:mm:ss"));
                sql.Parameters.AddWithValue("@ORDERID", id);
                db.Execute(sql);

                sql = new SqlCommand("INSERT EVENTLOG ([MODULE], [ACTION], [REMARK]) VALUES (@MODULE, @ACTION, @REMARK);");
                sql.Parameters.AddWithValue("@MODULE", "捐款紀錄");
                sql.Parameters.AddWithValue("@ACTION", "修改");
                sql.Parameters.AddWithValue("@REMARK", $"payStatus=2");
                db.Execute(sql);

                db.CloseConn();

                return Content($"OK;{now.ToString("yyyy/MM/dd HH:mm:ss")}");
            }
            catch (Exception e)
            {
                if (db.SqlConn.State != ConnectionState.Closed)
                    db.CloseConn();

                ecLog.Log.Record(ecLog.Log.InfoType.Error, $"{e.Message};{e.StackTrace}", nameof(ToPayEnd));

                return Content("NG");
            }
        }
        public ActionResult ExportDonateRecord(string gp, string opt, string name, string date1, string date2)
        {
            #region 篩選設定
            DataTable queryCondition = new DataTable();
            queryCondition.Columns.Add("篩選設定");
            string gpDesc = string.Empty;
            string optDesc = string.Empty;

            if (gp.Equals("1"))
            {
                gpDesc = "捐款/結緣";
                optDesc = opt.Equals("1") ? "一般捐款" : "結緣捐贈";
            }

            if (gp.Equals("2"))
            {
                gpDesc = "交易狀態";
                optDesc = opt.Equals("1") ? "未結案" : "已結案";
            }

            if (gp.Equals("3"))
            {
                gpDesc = "繳款狀態";
                optDesc = opt.Equals("1") ? "待付款" : "已付款";
            }

            queryCondition.Rows.Add($"捐款人：{name}");
            queryCondition.Rows.Add($"狀態分類：{gpDesc}");
            queryCondition.Rows.Add($"狀態：{optDesc}");
            queryCondition.Rows.Add($"付款日期：{date1} ~ {date2}");
            #endregion

            SqlCommand sql = new SqlCommand(@"
            SELECT 
	            O.ORDERID,
                CONVERT(NVARCHAR, O.ORDERDATE, 120) ORDERDATE,
                CONVERT(NVARCHAR, O.PAYDATE, 120) PAYDATE,
	            BT.NAME BUYERTYPE,
	            O.BUYERNAME, 
	            O.BUYERID,
	            O.BUYERPHONE,
	            O.BUYEREMAIL,
	            PT.NAME PAYTYPE,
	            B.NAME BRANCH,
	            O.DONATEAMOUNT,

	            OT.NAME ORDERTYPE,

	            O.DONATEFOR,
	            O.NEEDRECEIPT,
	            O.RECEIPTADDRESS,
	            O.NEEDANONYMOUS,

	            O.PICKUPMETHOD,
	            O.REMARK
            FROM ORDERMASTER O
            JOIN ORDERTYPE OT ON O.ORDERTYPEID = OT.ID
            JOIN ORDERSTATUS OS ON O.ORDERSTATUS = OS.ID
            JOIN PAYSTATUS PS ON O.PAYSTATUS = PS.ID
            JOIN BUYERTYPE BT ON O.BUYERTYPEID = BT.ID
            JOIN BRANCH B ON O.DONATEUNIT = B.SN
            JOIN PAYTYPE PT ON O.PAYTYPE = PT.ID
            WHERE O.BUYERNAME LIKE @BUYERNAME");

            sql.Parameters.AddWithValue("@BUYERNAME", $"%{name}%");

            if (gp.Equals("1"))
            {
                sql.CommandText += " AND O.ORDERTYPEID = @ORDERTYPEID";
                sql.Parameters.AddWithValue("@ORDERTYPEID", opt);
            }
            if (gp.Equals("2"))
            {
                sql.CommandText += " AND O.ORDERSTATUS = @ORDERSTATUS";
                sql.Parameters.AddWithValue("@ORDERSTATUS", opt);
            }
            if (gp.Equals("3"))
            {
                sql.CommandText += " AND O.PAYSTATUS = @PAYSTATUS";
                sql.Parameters.AddWithValue("@PAYSTATUS", opt);
            }

            if (!string.IsNullOrEmpty(date1) && !string.IsNullOrEmpty(date2))
            {
                sql.CommandText += " AND O.PAYDATE >= @DATE1 AND O.PAYDATE <= @DATE2";
                sql.Parameters.AddWithValue("@DATE1", date1);
                sql.Parameters.AddWithValue("@DATE2", date2);
            }

            DataTable dt = db.GetResult(sql);

            //sql = new SqlCommand(@"SELECT NAME, QTY FROM ORDERDETAIL WHERE ORDERID = @ORDERID");
            //sql
            // 表頭
            for (int i = 0; i < dt.Columns.Count; i++)
            {
                switch (dt.Columns[i].ColumnName)
                {
                    case "ORDERID": dt.Columns[i].ColumnName = "捐款編號"; break;
                    case "ORDERDATE": dt.Columns[i].ColumnName = "捐款日期"; break;
                    case "PAYDATE": dt.Columns[i].ColumnName = "付款日期"; break;
                    case "BUYERTYPE": dt.Columns[i].ColumnName = "個人/公司"; break;
                    case "BUYERNAME": dt.Columns[i].ColumnName = "捐贈者(姓名)"; break;
                    case "BUYERID": dt.Columns[i].ColumnName = "身分證號"; break;
                    case "BUYERPHONE": dt.Columns[i].ColumnName = "電話"; break;
                    case "BUYEREMAIL": dt.Columns[i].ColumnName = "信箱"; break;
                    case "PAYTYPE": dt.Columns[i].ColumnName = "捐款方式"; break;
                    case "DONATEAMOUNT": dt.Columns[i].ColumnName = "捐款金額"; break;
                    case "BRANCH": dt.Columns[i].ColumnName = "贈與單位"; break;

                    case "ORDERTYPE": dt.Columns[i].ColumnName = "捐款/結緣"; break;

                    case "DONATEFOR": dt.Columns[i].ColumnName = "項目"; break;
                    case "NEEDRECEIPT": dt.Columns[i].ColumnName = "是否需要收據"; break;
                    case "RECEIPTADDRESS": dt.Columns[i].ColumnName = "收據地址"; break;
                    case "NEEDANONYMOUS": dt.Columns[i].ColumnName = "是否同意公開捐款姓名及金額"; break;

                    case "PICKUPMETHOD": dt.Columns[i].ColumnName = "取貨方式"; break;
                    case "REMARK": dt.Columns[i].ColumnName = "寄件地址或備註"; break;
                }
            }

            Dictionary<string, DataTable> datas = new Dictionary<string, DataTable>();
            datas.Add("過濾條件設定", queryCondition);
            datas.Add("查詢結果", dt);

            MemoryStream MS = ecNPOI.Excel.Export(datas, true);
            //MS.Position = 0;
            MS.Seek(0, SeekOrigin.Begin);
            //Response.ContentType = "application/vnd.ms-excel";
            //Response.AddHeader("Content-Disposition", "attachment; filename=" + $"訂單列表_{DateTime.Now.Date.ToString("yyyyMMdd")}.xls");
            //Response.BinaryWrite(MS.ToArray());
            //Response.End();

            return File(MS, "application/vnd.ms-excel", "DonateRecord.xls");
        }
        [HttpPost]
        public ActionResult ImportDonateRecord()
        {
            try
            {
                HttpPostedFileBase file = null;
                file = Request.Files.Count > 0 ? Request.Files[0] : null;

                if (file == null || !new string[] { "application/vnd.ms-excel", "text/csv","application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" }.Contains(file.ContentType))
                    return Content("NG;檔案類型錯誤，請下載範例檔編輯後上傳！");

                // DataTable dt = CsvToDataTable(file, ",");
                //Equals若指定的物件等於目前的物件，則為 true；否則為 false
                DataTable dt = ExcelToDataTable(file, (file.ContentType.Equals("application/vnd.ms-excel"))); ; //預設.xls,若不是則用xlsx

                SqlCommand sql = new SqlCommand();
                DataTable rDt = new DataTable();

                // 欄位防呆
                foreach (DataColumn dc in dt.Columns)
                {
                    if (!new string[] {"No.", "收據編號", "收據日期", "收款單號", "收款日期", "捐款專戶名稱", "會員編號", "捐贈者(單位)", "身分證號", "地址", "捐款方式", "捐款金額", "類別", "用途/事由" }.Contains(dc.ColumnName))
                            return Content($"NG;欄位不齊全，請參考範例檔！");
                 }
                // ==============資料防呆開始============
                foreach (DataRow dr in dt.Rows)
                {
                    string number = dr["身分證號"].ToString().Trim();
                    string name = dr["捐贈者(單位)"].ToString().Trim();  //捐款姓名

                    string donateTo_s = dr["捐款方式"].ToString().Trim();  //捐款方式是xls的名稱，實際是贈予單位Branch裏的1234總會與北中南
                    string DonateUnits_result = DonateUnits(donateTo_s);
                    int sn = 0;
                    if (!int.TryParse(DonateUnits_result, out sn))  //辨斷左邊的字串是輸入的值是否能轉成數字，右邊是輸出
                        sn = 0; //轉換失敗
                    sql = new SqlCommand("SELECT SN FROM BRANCH WHERE SN = @SN");
                    sql.Parameters.AddWithValue("@SN", sn);
                    rDt = db.GetResult(sql);
                    if (rDt.Rows.Count == 0)
                        return Content($"NG;欄位 贈予單位 請填寫正確總分會名稱！");

                    string amount = dr["捐款金額"].ToString().Trim().Replace(",", "");  //客戶提供的資料有豆點
                    int amt = 0;
                    if (!int.TryParse(amount, out amt))     //辨斷是不是純數字，若有任何符號則不是，不是則設為0
                        amt = 0;

                    if (amt == 0)
                        return Content($"NG;欄位 捐款金額 需大於0元，並請填寫半形數字（不含任何標點符號）！");

                    string ReceiptNum = dr["收據編號"].ToString().Trim();  //收據編號目前資料庫6字元需加到變7
                    if (ReceiptNum.Length > 15)
                        return Content($"NG;欄位 收據編號不能超過15位數！");
                    if (ReceiptNum.Length == 0)
                        return Content($"NG;欄位 收據編號不能空值!");
                }
                // ===============資料防呆結束===============


                db.OpenConn();

                // 訂單編號
                DateTime now = DateTime.Now;
                string orderId = string.Empty;
                sql = new SqlCommand("SELECT MAX(ORDERID) FROM ORDERMASTER WHERE ORDERID LIKE @ORDERID");
                sql.Parameters.AddWithValue("@ORDERID", $"{now.ToString("yyyyMMdd")}%");
                rDt = db.GetResult(sql);
                               
                if (string.IsNullOrEmpty(rDt.Rows[0][0].ToString()))    //找不到今天資料時
                    orderId = $"{now.ToString("yyyyMMdd")}000";
                else
                    orderId = $"{now.ToString("yyyyMMdd")}{rDt.Rows[0][0].ToString().Substring(rDt.Rows[0][0].ToString().Length - 3)}";
                // 20210519   Substring(0)開始算，20210519220共11碼，11-3=8 Substring(8)=220


                //int newId = 0;
                foreach (DataRow dr in dt.Rows)
                {
                    //目前暫時是用xls匯入，但若重覆資料庫會重覆錯誤
                    //newId = orderId.Substring(orderId.Length - 3).ParseInt() + 1;       //Substring(8)=220+1
                    //orderId = $"{now.ToString("yyyyMMdd")}{newId.ToString().PadLeft(3, '0')}";  // PadLeft是220前面要補0，3是代表3位數，220剛好3位所以沒補若是 22則會變022
                    orderId = dr["收款單號"].ToString().Trim();  //訂單編號

                    string number = dr["身分證號"].ToString().Trim();

                    int buyertypeid;
                    if (number.Length == 8)
                    {
                        buyertypeid = 2;        //2公司,統編8碼
                    }
                    else
                    {
                        buyertypeid = 1;        //1個人,10碼或空
                    }
                    string name = dr["捐贈者(單位)"].ToString().Trim();  //捐款姓名
                   // if (string.IsNullOrEmpty(name)) continue;  //捐款姓名是空值就不加入資料庫，還有資料庫最後一欄若用雙格線時，不知道為什麼會多出一行空資料，所以要跳過不加入

                    //string tel = dr["聯絡電話xx目前xls並無提供"].ToString().Trim();
                    string tel = string.Empty;

                    int payType = 0; //捐款方式 目前沒有對應的欄位與要如何設定什麼付款，資料庫是存123..
                    //xls沒有此欄，此欄目前資料庫所存的資料為數字代號，代號內容如右說明1=線上刷卡; 2=虛擬帳號; 4=7-11代碼; 6=全家代碼; 9=OK代碼; 10=萊爾富代碼; 83=銀行匯款
                    //測試若是string.Empty, 則為0因為資料庫設定smallint, 也不允許NULL, 所以就自動變成0，而且後端是INNER JOIN, 所以資料都不會出現

                    string donateTo_s = dr["捐款方式"].ToString().Trim();  //K欄:捐款方式  贈予單位 四個會
                    string donateTo = DonateUnits(donateTo_s);
            
                    //string amount = dr["捐款金額"].ToString().Trim().Replace(",", "");  //客戶提供的資料有豆點
                    int amount = dr["捐款金額"].ToString().Trim().ParseInt();  //客戶提供的資料有豆點
                     // ecDataParse.Extension
                     // ParseInt內容，先??看是不是null是就指定0, 處理,取代, 辨斷是不是純數字，是就回傳，不是就指定0
                    //public static int ParseInt(this object obj)
                    //{
                    //    object obj2 = obj ?? ((object)0);
                    //    int result = 0;
                    //    return int.TryParse(obj2.ToString().Replace(",", ""), out result) ? result : 0;
                    //}

                    string donateDesc = dr["捐款專戶名稱"].ToString().Trim();  //F:捐款專戶名稱-資料庫DonateFor捐款用途
                    string needReceipt = "Y" ;  //是否開收據沒有資料時資料庫預設N此欄xls並沒有提供

                    string ReceiptNum = dr["收據編號"].ToString().Trim();  //收據編號目前資料庫6字元需加到變7

                    string addr = dr["地址"].ToString().Trim();  //地址

                    string remark = dr["用途/事由"].ToString().Trim();  // 備註;

                    string sampleDate = dr["收款日期"].ToString().Trim(); ;  // xls收款日期 sql付款日期;

                    //string sampleDate = "0110.02.09";
                    CultureInfo culture = new CultureInfo("zh-TW");
                    culture.DateTimeFormat.Calendar = new TaiwanCalendar();
                    DateTime paydate = DateTime.Parse(sampleDate, culture);




                    sql = new SqlCommand(@"INSERT INTO ORDERMASTER 
                    (
                        ORDERID, ORDERDATE, ORDERSTATUS, PAYSTATUS, ORDERTYPEID, 
                        BUYERTYPEID, BUYERNAME, BUYERID, BUYERPHONE, BUYEREMAIL, 
                        PAYTYPE, DONATEUNIT, DONATEAMOUNT, DONATEFOR, 
                        NEEDRECEIPT, RECEIPTNUM, RECEIPTPOSTMETHOD, NEEDANONYMOUS, PICKUPMETHOD, REMARK, RECEIPTTITLE, RECEIPTADDRESS,
                        ISMANUAL, PAYDATE
                    ) VALUES (
                        @ORDERID, @ORDERDATE, @ORDERSTATUS, @PAYSTATUS, @ORDERTYPEID, 
                        @BUYERTYPEID, @BUYERNAME, @BUYERID, @BUYERPHONE, @BUYEREMAIL, 
                        @PAYTYPE, @DONATEUNIT, @DONATEAMOUNT, @DONATEFOR, 
                        @NEEDRECEIPT, @RECEIPTNUM, @RECEIPTPOSTMETHOD, @NEEDANONYMOUS, @PICKUPMETHOD, @REMARK, @RECEIPTTITLE, @RECEIPTADDRESS,
                        @ISMANUAL, @PAYDATE
                    )");
                   // sql.Parameters.AddWithValue("@ORDERID", orderId);   // 訂單編號No Null用程式編入
                    sql.Parameters.AddWithValue("@ORDERID", ReceiptNum);   // 訂單編號No Null用收據編號xls欄加入
                    sql.Parameters.AddWithValue("@ORDERDATE", now.ToString("yyyy/MM/dd HH:mm:ss")); //訂單日期No Null
                    //sql.Parameters.AddWithValue("@PAYDATE", now.ToString("yyyy/MM/dd HH:mm:ss")); //付款日期 E欄:收款日期
                    sql.Parameters.AddWithValue("@PAYDATE", paydate); //付款日期 E欄:收款日期
                    sql.Parameters.AddWithValue("@ORDERSTATUS", 2); //交易狀態2已結案No Null
                    sql.Parameters.AddWithValue("@PAYSTATUS", 2);   //繳款狀態 2已付款No Null
                    sql.Parameters.AddWithValue("@ORDERTYPEID", 1); // 捐款/請購舊官網僅捐贈，沒有結緣No Null
                    sql.Parameters.AddWithValue("@BUYERTYPEID", buyertypeid);    //捐款單位1個人2公司改先預設1 No Null
                    sql.Parameters.AddWithValue("@BUYERNAME", name); //捐款姓名 : H欄:捐贈者(單位)
                    sql.Parameters.AddWithValue("@BUYERID", number);    //身分證號統編 : I欄:身分證號
                    sql.Parameters.AddWithValue("@BUYERPHONE", tel);    //聯絡電話
                    sql.Parameters.AddWithValue("@BUYEREMAIL", DBNull.Value); //電子郵件NULL沒資料
                    sql.Parameters.AddWithValue("@PAYTYPE", payType);   //捐款方式1線上刷卡,2虛擬帳號,47-11,6全家,9OK,10萊爾富,83銀行匯款
                    sql.Parameters.AddWithValue("@DONATEUNIT", donateTo);   //贈予單位 K欄:捐款方式 smallint 1,2,3,4
                    sql.Parameters.AddWithValue("@DONATEAMOUNT", amount);   //捐款金額 L欄:捐款金額

                    sql.Parameters.AddWithValue("@DONATEFOR", donateDesc); //捐款用途 F欄:捐款專戶名稱
                    sql.Parameters.AddWithValue("@NEEDRECEIPT", needReceipt); //是否開收據

                    sql.Parameters.AddWithValue("@RECEIPTNUM", string.Empty); //收據編號

                    sql.Parameters.AddWithValue("@RECEIPTPOSTMETHOD", string.Empty); //是否寄送空值
                    sql.Parameters.AddWithValue("@NEEDANONYMOUS", "Y");  // 是否開公開捐款姓名及金額
                    sql.Parameters.AddWithValue("@RECEIPTTITLE", string.Empty);     // 收據抬頭空值
                    sql.Parameters.AddWithValue("@RECEIPTADDRESS", addr);       //地址 J欄:地址

                    sql.Parameters.AddWithValue("@PICKUPMETHOD", string.Empty);   // 取貨方式空值
                    sql.Parameters.AddWithValue("@REMARK", remark);     //備註                                                           
                    sql.Parameters.AddWithValue("@ISMANUAL", "Y");  // 是否用匯入 Y是匯入
                    db.ExecuteNonCommit(sql);

                    sql = new SqlCommand("INSERT EVENTLOG ([MODULE], [ACTION], [REMARK]) VALUES (@MODULE, @ACTION, @REMARK);");
                    sql.Parameters.AddWithValue("@MODULE", "捐款紀錄");
                    sql.Parameters.AddWithValue("@ACTION", "新增捐款");
                    sql.Parameters.AddWithValue("@REMARK", $"orderId={orderId}");
                    db.ExecuteNonCommit(sql);
                }

                db.Commit();
                db.CloseConn();
                return Content("OK");  //代表整個ImportDonateRecord執行完成所以設定一個ok回傳讓responseText接收

            }
            catch (Exception e)
            {
                if (db.SqlConn.State != ConnectionState.Closed)
                    db.CloseConn();

                ecLog.Log.Record(ecLog.Log.InfoType.Error, $"{e.Message};{e.StackTrace}", nameof(ImportMember));

                return Content("NG");
            }
        }
        // 目前沒這個需求，寫一半先留著
        //[HttpPost]
        //public ActionResult ImportDonateRecord()
        //{
        //    try
        //    {
        //        HttpPostedFileBase file = null;
        //        file = Request.Files.Count > 0 ? Request.Files[0] : null;

        //        if (file == null || !new string[] { "application/vnd.ms-excel", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" }.Contains(file.ContentType))
        //            return Content("NG;檔案類型錯誤，請下載範例檔編輯後上傳！");

        //        DataTable dt = ExcelToDataTable(file, (file.ContentType.Equals("application/vnd.ms-excel")));

        //        SqlCommand sql = new SqlCommand();
        //        DataTable rDt = new DataTable();

        //        db.OpenConn();

        //        foreach (DataRow dr in dt.Rows)
        //        {
        //            string receiptNum = dr["收據編號"].ToString().Trim();
        //            string receiptDate = dr["收據日期"].ToString().Trim();
        //            string payDate = dr["收款日期"].ToString().Trim();
        //            string donateFor = dr["捐款專戶名稱"].ToString().Trim();
        //            string buyerName = dr["捐贈者(姓名)"].ToString().Trim();
        //            string buyerTel = dr["電話"].ToString().Trim();
        //            string buyerId = dr["身分證號"].ToString().Trim();
        //            string receiptAddr = dr["地址"].ToString().Trim();
        //            string payType = dr["捐款方式"].ToString().Trim();
        //            string amount = dr["捐款金額"].ToString().Trim();
        //        }

        //        db.Commit();
        //        db.CloseConn();

        //        return Content("OK");
        //    }
        //    catch (Exception e)
        //    {
        //        if (db.SqlConn.State != ConnectionState.Closed)
        //            db.CloseConn();

        //        ecLog.Log.Record(ecLog.Log.InfoType.Error, $"{e.Message};{e.StackTrace}", nameof(ImportDonateRecord));

        //        return Content("NG");
        //    }
        //}
        #endregion

        [HttpPost]
        public ActionResult UpdPassword()
        {
            try
            {
                if (Request.Form.HasKeys())         //有鑰匙
                {
                    string oldPsw = Request.Form["oldPsw"];             // 您輸入的舊密碼
                    string newPsw = Request.Form["newPsw01"];       // 您輸入的新密碼

                    SqlCommand sql = new SqlCommand("SELECT SALT, PASSWORD FROM USERS WHERE ACCOUNT = @ACCOUNT");
                    sql.Parameters.AddWithValue("@ACCOUNT", System.Web.HttpContext.Current.User.Identity.Name);  //調出目前登入的帳號
                    DataTable dt = db.GetResult(sql);

                    string salt = dt.Rows[0]["SALT"].ToString();                          //資料第0行的salt欄
                    string oldPwd = dt.Rows[0]["PASSWORD"].ToString();      //資料庫裏你的舊密碼有加密過

                    oldPsw = FormsAuthentication.HashPasswordForStoringInConfigFile(oldPsw + salt, "SHA1");  //加密後的字串是大寫
   
                    if (oldPsw != oldPwd)
                        return Content("NG");

                    string pwd = FormsAuthentication.HashPasswordForStoringInConfigFile(newPsw + salt, "SHA1");

                    sql = new SqlCommand("UPDATE USERS SET PASSWORD = @PASSWORD WHERE ACCOUNT = @ACCOUNT");
                    sql.Parameters.AddWithValue("@ACCOUNT", System.Web.HttpContext.Current.User.Identity.Name);
                    sql.Parameters.AddWithValue("@PASSWORD", pwd);
                    db.OpenConn();
                    db.Execute(sql);
                    db.CloseConn();

                    return Content("OK");
                }
                return Content("NG");
            }
            catch (Exception e)
            {
                return Content("NG");
            }
        }

        public static DataTable CsvToDataTable(HttpPostedFileBase file, string seperator)
        {
            DataTable dt = new DataTable();
            bool isFirst = true;


            using (StreamReader sr = new StreamReader(file.InputStream, Encoding.Default, true))
            {
                while (!sr.EndOfStream)
                {
                    var eachLineStr = sr.ReadLine();

                    string[] rows = eachLineStr.Split(new string[] { seperator }, StringSplitOptions.None);

                    // exclude first line
                    if (isFirst)
                    {
                        for (int i = 0; i < rows.Length; i++)
                        {
                            dt.Columns.Add(rows[i].Trim());
                        }
                        isFirst = false;
                        continue;
                    }

                    // split string to array of string use seperator
                    DataRow dr = dt.NewRow();
                    for (int i = 0; i < rows.Length; i++)
                    {
                        dr[i] = rows[i].Trim();
                    }
                    dt.Rows.Add(dr);
                }
            }

            return dt;
        }

        public static DataTable ExcelToDataTable(HttpPostedFileBase file, bool isXls)
        {
            DataTable dt = new DataTable();

            IWorkbook workbook = null;
            if (isXls)
                workbook = new HSSFWorkbook(file.InputStream);
                //HSSF － 提供讀寫Microsoft Excel XLS格式檔案的功能97-2003。
            else
                workbook = new XSSFWorkbook(file.InputStream);
            //XSSF － 提供讀寫Microsoft Excel OOXML XLSX格式檔案的功能2007。

            ISheet sheet = workbook.GetSheetAt(0);      //得到第一張表
            IRow headerRow = sheet.GetRow(7);            //第一行為標題行
            int cellCount = headerRow.LastCellNum;      //LastCellNum = PhysicalNumberOfCells欄位數量
            int rowCount = sheet.LastRowNum;            //LastRowNum = PhysicalNumberOfRows - 1 xls資料最後一行

            //IRow row;
            //int Preset_i = 7;
            for (int i = headerRow.FirstCellNum ; i < cellCount  ; i++)        //表頭列從0開始
            {
                //新增行標題
                //寫法1
                //string cellValue = sheet.GetRow(7).GetCell(i).ToString(); 這樣也行
                string cellValue = headerRow.GetCell(i).ToString();
                dt.Columns.Add(cellValue);
                //寫法2
                //DataColumn column = new DataColumn(headerRow.GetCell(i).StringCellValue);
                //dt.Columns.Add(column); 
            }

           // for (int i = (sheet.FirstRowNum + 1); i <= rowCount; i++)       //豎一行一行往下
           //for (int i = (sheet.FirstRowNum + 1); i <= sheet.LastRowNum; i++)       //豎一行一行往下
            for (int i = (sheet.FirstRowNum + 8); i <= sheet.LastRowNum; i++)       //豎一行一行往下從0行開始，若0行是標題行就要加1
             {
                IRow row = sheet.GetRow(i);
                DataRow dr = dt.NewRow();
                if (row != null)
                {
                    for (int j = row.FirstCellNum; j < cellCount; j++)      //由左至右欄調出DataRow表示 DataTable 中的一行資料0欄開始，到最後一欄是第3欄，FirstCellNum就是4
                    {
                            if (row.GetCell(j) != null)
                            dr[j] = row.GetCell(j);
                    }
                    dt.Rows.Add(dr);
                }
             }
            //for (int i = 0; i <= sheet.LastRowNum; i++)     //0是表頭，每一row列做迴圈
            //{
            //row = sheet.GetRow(i);      //(0)Excel 表頭列
            //if (row != null)
            //{
            //for (int colIdx = 0; colIdx <= row.LastCellNum; colIdx++) //表頭列，共有幾個 "欄位"?（取得最後一欄的數字） 
            //{
            //    if (row.GetCell(colIdx) != null)
            //    {
            //        string cellValue = row.GetCell(colIdx).ToString();
            //        dt.Columns.Add(cellValue);
            //        //dt.Columns.Add(new DataColumn(row.GetCell(colIdx).StringCellValue));
            //    }
            //}
            //        if (row.GetCell(0).ToString().Trim().Equals("No"))     //表頭第0列第0行設定No.，橫為行，列為豎Row[0列往下][0行往右]
            //        {
            //            for (int j = 0; j < row.LastCellNum; j++)       //表頭列，往右共有幾個 "欄位"?（取得最後一欄的數字） 
            //            {
            //                string cellValue = row.GetCell(j).ToString();
            //                dt.Columns.Add(cellValue);
            //            }
            //        }
            //        else
            //        {
            //            DataRow dr = dt.NewRow(); 
            //            for (int j = 0; j < row.LastCellNum; j++)   //不包含 Excel表頭列的 "其他資料列"一樣由左往右欄
            //            {
            //                string cellValue = row.GetCell(j).ToString();       ////每一個欄位，都加入同一列 DataRow
            //                dr[j] = cellValue;
            //            }
            //            dt.Rows.Add(dr);
            //        }
            //    }
            //}

            return dt;
        }


        private string DonateUnits(string donateTo_s)
        {
            string donateTo_sn = "0";
            switch (donateTo_s)
            {  
                case "日盛-總會":
                case "合作金庫-總會":
                case "富邦":
                case "郵局劃撥":
                    donateTo_sn = "1";     //1華光功德會總會
                    return donateTo_sn;
                case "日盛-中區":
                case "合作金庫-中區":
                case "現金":
                case "藍新金流(國外刷卡)":
                    donateTo_sn = "3";     //3華光功德會中區分會
                    return donateTo_sn;
                   //break;
                case "三信-南區":
                case "支票":
                case "日盛-南區":
                case "合作金庫-南區":
                case "現金-南區":
                    donateTo_sn = "4";     //4華光功德會南區分會
                    return donateTo_sn;
                default:
                    donateTo_sn = "0";     //沒有比對到任何區會
                    return donateTo_sn;
                    //https://docs.microsoft.com/zh-tw/dotnet/csharp/language-reference/keywords/return
                    //https://docs.microsoft.com/zh-tw/dotnet/csharp/language-reference/keywords/switch
            }

        }



    }
}