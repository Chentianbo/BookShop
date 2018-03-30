using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Net;
using System.Drawing;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Net.Mail;
using System.Text;

using System.Linq;
using System.Threading.Tasks;
using System.Web;


namespace JN.Services.Tool
{
    public static class SMSHelper
    {


        #region 网建接口，支持批量
        /// <summary>
        /// 发送手机短信（网建接口，支持批量） http://www.smschinese.cn/
        /// </summary>
        /// <param name="mobile">手机号码,多个手机号以,号相隔</param>
        /// <param name="body">短信内容</param>
        public static bool WebChineseMSM(string mobile, string body)
        {
            var sys = MvcCore.Unity.Get<JN.Data.Service.ISysSettingService>().Single(1);
            if (!string.IsNullOrEmpty(mobile))
            {
                string url = "http://utf8.sms.webchinese.cn/?Uid=" + sys.SMSUid + "&Key=" + sys.SMSKey + "&smsMob=" + mobile + "&smsText=" + body;
                string targeturl = url.Trim();
                try
                {
                    bool result = false;
                    HttpWebRequest hr = (HttpWebRequest)WebRequest.Create(targeturl);
                    hr.UserAgent = "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.1)";
                    hr.Method = "GET";
                    hr.Timeout = 30 * 60 * 1000;
                    WebResponse hs = hr.GetResponse();
                    Stream sr = hs.GetResponseStream();
                    StreamReader ser = new StreamReader(sr, Encoding.Default);
                    string content = ser.ReadToEnd();
                    string msg = "";
                    if (content.Substring(0, 1) == "0" || content.Substring(0, 1) == "1")
                    {
                        result = true;
                        msg = "发送成功";
                    }
                    else
                    {
                        if (content.Substring(0, 1) == "2") //余额不足
                        {
                            //"手机短信余额不足";
                        }
                        else
                        {
                            //短信发送失败的其他原因，请参看官方API
                        }
                        result = false;
                        msg = "发送失败，结果：" + content;
                    }
                    MvcCore.Unity.Get<JN.Data.Service.ISMSLogService>().Add(new Data.SMSLog {  Mobile = mobile, SMSContent = body, CreateTime = DateTime.Now, ReturnValue = msg });
                    MvcCore.Unity.Get<JN.Data.Service.ILogDBTool>().Commit();

                    return result;
                }
                catch
                {
                    return false;
                }
            }
            else
                return false;
        }
        #endregion

        #region  C123接口
        /// <summary>
        /// C123接口 http://www.c123.cn/
        /// </summary>
        /// <param name="mobile"></param>
        /// <param name="body"></param>
        /// <returns></returns>
        public static bool C123SMS(string mobile, string body)
        {
            var sys = MvcCore.Unity.Get<JN.Data.Service.ISysSettingService>().Single(1);
            if (!string.IsNullOrEmpty(mobile))
            {
                string url = "http://wapi.c123.cn/tx/?uid=" + sys.SMSUid + "&pwd=" + sys.SMSKey + "&mobile=" + mobile + "&content=" + System.Web.HttpUtility.UrlEncode(body, Encoding.GetEncoding("GB2312")).ToUpper();
                string targeturl = url.Trim();
                try
                {
                    bool result = false;
                    HttpWebRequest hr = (HttpWebRequest)WebRequest.Create(targeturl);
                    hr.UserAgent = "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.1)";
                    hr.Method = "GET";
                    hr.Timeout = 30 * 60 * 1000;
                    WebResponse hs = hr.GetResponse();
                    Stream sr = hs.GetResponseStream();
                    StreamReader ser = new StreamReader(sr, Encoding.Default);
                    string content = ser.ReadToEnd();
                    string msg = "";
                    if (content.Substring(0, 1) == "100")
                    {
                        result = true;
                        msg = "发送成功";
                    }
                    else
                    {
                        if (content.Substring(0, 1) == "102") //余额不足
                        {
                            //"手机短信余额不足";
                        }
                        else
                        {
                            //短信发送失败的其他原因，请参看官方API
                        }
                        result = false;
                        msg = "发送失败，结果：" + content;
                    }
                    MvcCore.Unity.Get<JN.Data.Service.ISMSLogService>().Add(new Data.SMSLog { Mobile = mobile, SMSContent = body, CreateTime = DateTime.Now, ReturnValue = msg });
                    MvcCore.Unity.Get<JN.Data.Service.ILogDBTool>().Commit();

                    return result;

                }
                catch
                {
                    return false;
                }
            }
            else
                return false;
        }

        #endregion

        #region 容联接口 (支持短信,语言验证码)
        ///// <summary>
        ///// C123接口 http://www.yuntongxun.com/
        ///// </summary>
        ///// <param name="mobile"></param>
        ///// <param name="body"></param>
        ///// <returns></returns>
        //public static bool VoiceVerify(string mobile, string yzm, string displayNum)
        //{
        //    string ret = null;
        //    CCPRestSDK.CCPRestSDK api = new CCPRestSDK.CCPRestSDK();
        //    bool isInit = api.init("sandboxapp.cloopen.com", "8883");
        //    api.setAccount(ConfigHelper.GetConfigString("Voice_Uid"), ConfigHelper.GetConfigString("Voice_Key"));
        //    api.setAppId(ConfigHelper.GetConfigString("Voice_Appid"));
        //    try
        //    {
        //        if (isInit)
        //        {
        //            Dictionary<string, object> retData = api.VoiceVerify(mobile, yzm, displayNum, "2", "");
        //            ret = getDictionaryData(retData);
        //        }
        //        else
        //        {
        //            ret = "初始化失败";
        //        }
        //        return true;
        //    }
        //    catch (Exception exc)
        //    {
        //        ret = exc.Message;
        //        return false;
        //    }
        //}

        //private static string getDictionaryData(Dictionary<string, object> data)
        //{
        //    string ret = null;
        //    foreach (KeyValuePair<string, object> item in data)
        //    {
        //        if (item.Value != null && item.Value.GetType() == typeof(Dictionary<string, object>))
        //        {
        //            ret += item.Key.ToString() + "={";
        //            ret += getDictionaryData((Dictionary<string, object>)item.Value);
        //            ret += "};";
        //        }
        //        else
        //        {
        //            ret += item.Key.ToString() + "=" +
        //            (item.Value == null ? "null" : item.Value.ToString()) + ";";
        //        }
        //    }
        //    return ret;
        //}
        #endregion

        #region 叮咚云接口，不支持批量
        /// <summary>
        /// 发送手机短信（叮咚云接口，不支持批量） http://www.dingdongcloud.com/platui/newplatIndex
        /// </summary>
        /// <param name="mobile">手机号码</param>
        /// <param name="body">短信内容</param>
        public static bool DDYmsm(string mobile, string body)
        {
            var sys = MvcCore.Unity.Get<JN.Data.Service.ISysSettingService>().Single(1);
            if (!string.IsNullOrEmpty(mobile))
            {
                try
                {
                    bool result = false;
                    string URL_SEND_YZM = "https://api.dingdongcloud.com/v1/sms/sendyzm";
                    string content = HttpUtility.UrlEncode(body, Encoding.UTF8);
                    string data_send_yzm = "apikey=" + sys.SMSKey + "&mobile=" + mobile + "&content=" + content;
                    HttpPost(URL_SEND_YZM, data_send_yzm);
                    //string content = HttpPost(URL_SEND_YZM, data_send_yzm);
                    string msg = "";
                    //if (content.Substring(0, 1) == "0" || content.Substring(0, 1) == "1")
                    //{
                    //    result = true;
                    //    msg = "发送成功";
                    //}
                    //else
                    //{
                    //    if (content.Substring(0, 1) == "2") //余额不足
                    //    {
                    //        //"手机短信余额不足";
                    //    }
                    //    else
                    //    {
                    //        //短信发送失败的其他原因，请参看官方API
                    //    }
                    //    result = false;
                    //    msg = "发送失败，结果：" + content;
                    //}
                    MvcCore.Unity.Get<JN.Data.Service.ISMSLogService>().Add(new Data.SMSLog { Mobile = mobile, SMSContent = body, CreateTime = DateTime.Now, ReturnValue = msg });
                    MvcCore.Unity.Get<JN.Data.Service.ILogDBTool>().Commit();

                    return result;
                }
                catch
                {
                    return false;
                }
            }
            else
                return false;
        }
        public static void HttpPost(string Url, string postDataStr)
        {
            byte[] dataArray = Encoding.UTF8.GetBytes(postDataStr);
            // Console.Write(Encoding.UTF8.GetString(dataArray));

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(Url);
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            request.ContentLength = dataArray.Length;
            //request.CookieContainer = cookie;
            Stream dataStream = request.GetRequestStream();
            dataStream.Write(dataArray, 0, dataArray.Length);
            dataStream.Close();
            try
            {
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8);
                String res = reader.ReadToEnd();
                reader.Close();
                Console.Write("\nResponse Content:\n" + res + "\n");
            }
            catch (Exception e)
            {
                Console.Write(e.Message + e.ToString());
            }
        }
        #endregion

        #region 创瑞接口
        /// <summary>
        /// 发送手机短信（创瑞接口） http://sms.cr6868.com
        /// </summary>
        /// <param name="mobile">手机号码</param>
        /// <param name="body">短信内容</param>
        public static bool CYmsm(string mobile, string body)
        {
            var sys = MvcCore.Unity.Get<JN.Data.Service.ISysSettingService>().Single(1);
            if (!string.IsNullOrEmpty(mobile))
            {
                try
                {
                    bool result = false;
                    StringBuilder sms = new StringBuilder();
                    sms.AppendFormat("name={0}", sys.SMSUid);
                    sms.AppendFormat("&pwd={0}", sys.SMSKey);//登陆平台，管理中心--基本资料--接口密码（28位密文）；复制使用即可。
                    sms.AppendFormat("&content={0}", body);
                    sms.AppendFormat("&mobile={0}", mobile);
                    sms.AppendFormat("&sign={0}", "天地人");// 公司的简称或产品的简称都可以
                    sms.Append("&type=pt");
                    string resp = PushToWeb("http://web.cr6868.com/asmx/smsservice.aspx", sms.ToString(), Encoding.UTF8);
                    string[] content = resp.Split(',');
                    string msg = "";
                    if (content[0] == "0")
                    {
                        result = true;
                        msg = "发送成功";
                    }
                    else
                    {
                        if (content[0] == "2") //余额不足
                        {
                            //"手机短信余额不足";
                        }
                        else
                        {
                            //短信发送失败的其他原因，请参看官方API
                        }
                        result = false;
                        msg = "发送失败，结果：" + content;
                    }
                    MvcCore.Unity.Get<JN.Data.Service.ISMSLogService>().Add(new Data.SMSLog { Mobile = mobile, SMSContent = body, CreateTime = DateTime.Now, ReturnValue = msg });
                    MvcCore.Unity.Get<JN.Data.Service.ILogDBTool>().Commit();

                    return result;
                }
                catch
                {
                    return false;
                }
            }
            else
                return false;
        }
        public static string PushToWeb(string weburl, string data, Encoding encode)
        {
            byte[] byteArray = encode.GetBytes(data);

            HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(new Uri(weburl));
            webRequest.Method = "POST";
            webRequest.ContentType = "application/x-www-form-urlencoded";
            webRequest.ContentLength = byteArray.Length;
            Stream newStream = webRequest.GetRequestStream();
            newStream.Write(byteArray, 0, byteArray.Length);
            newStream.Close();

            //接收返回信息：
            HttpWebResponse response = (HttpWebResponse)webRequest.GetResponse();
            StreamReader aspx = new StreamReader(response.GetResponseStream(), encode);
            return aspx.ReadToEnd();
        }
        #endregion

    }
}
