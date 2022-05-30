using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Text.RegularExpressions;
using System.Net.Http;
using System.Runtime.Serialization.Json;
using System.IO;
using System.Runtime.Serialization;

namespace Teachermate
{
    class TMURL
    {
        public const string URL_CHECKIN = "https://v18.teachermate.cn/wechat-api/v1/class-attendance/student-sign-in";
        public const string URL_COURSES = "https://v18.teachermate.cn/wechat-api/v1/students/courses";
        public const string URL_ACTIVESIGNS = "https://v18.teachermate.cn/wechat-api/v1/class-attendance/student/active_signs";
        public const string URL_REFERER_T = "https://v18.teachermate.cn/wechat-pro-ssr/student/sign?openid={0}";
        public const string URL_GETNAME_T = "https://v18.teachermate.cn/wechat-pro-ssr/?openid={0}&from=wzj";
        public const string URL_CHECKINREFER_T = "https://v18.teachermate.cn/wechat-pro-ssr/student/sign/list/{0}";
        public const string CODE_REPEATSIGNIN = "305";
    }
    class Student
    {
        public string name { get; set; }
        public string openid { get; set; }
        public Student(string name, string openid) { this.name = name; this.openid = openid; }
        public Student(string openid) { this.openid = openid; this.name = null; }
        static public string OpenidSizer(string originalOpenId)
        {
            if (originalOpenId is null)
            {
                throw new ArgumentNullException(nameof(originalOpenId));
            }
            string teachermate = "teachermate";
            Match teachermateMatcher = Regex.Match(originalOpenId, teachermate);
            if (teachermateMatcher.Success)
            {
                string re_ptn = "openid=([^&]*)";
                Match match = Regex.Match(originalOpenId, re_ptn);
                string matcherFinished = match.Value;
                return matcherFinished.Replace("openid=", "");
            }
            return originalOpenId;
        }
    }
    class Signer
    {
        private Student stu;
        public Signer(Student stu)
        {
            this.stu = stu;
        }
        [DataContract]
        public class SignEvent
        {
            [DataMember]
            public string courseId { get; set; }
            [DataMember]
            public string signId { get; set; }
            [DataMember]
            public bool isGPS { get; set; }
            [DataMember]
            public bool isQR { get; set; }
            [DataMember]
            public string name { get; set; }
            [DataMember]
            public string code { get; set; }
            [DataMember]
            public int startYear { get; set; }
            [DataMember]
            public string term { get; set; }
            [DataMember]
            public string cover { get; set; }
        }
        [DataContract]
        public class SignSuccessEvent
        {
            [DataMember]
            public string signRank { get; set; }
            [DataMember]
            public string studentRank { get; set; }
        }
        private List<SignEvent> signs { get; set; }
        public enum SignMode
        {
            Sign,
            Common
        }
        public static HttpRequestMessage GenReq(SignMode mode, HttpMethod m, string url, string openid = null)
        {
            var req = new HttpRequestMessage(m, url);
            req.Headers.Add("User-Agent", "Mozilla/5.0 (Linux; Android 11; Mi 10 Build/RKQ1.200826.002; wv) AppleWebKit/537.36 (KHTML, like Gecko) Version/4.0 Chrome/86.0.4240.99 XWEB/3165 MMWEBSDK/20210902 Mobile Safari/537.36 MMWEBID/3949");
            req.Headers.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.9");
            req.Headers.Add("Host", "v18.teachermate.cn");
            req.Headers.Add("Accept-Language", "zh-CN,zh;q=0.9,en-US;q=0.8,en;q=0.7");
            if (openid != null)
            {
                if (mode == SignMode.Common) req.Headers.Add("Referer", String.Format(TMURL.URL_REFERER_T, openid));
                req.Headers.Add("openid", openid);
            }
            return req;
        }
        public async Task<string> GetUserName()
        {
            var client = new HttpClient();
            var req = GenReq(SignMode.Common, HttpMethod.Get, String.Format(TMURL.URL_GETNAME_T, stu.openid), stu.openid);

            // Use Regex to find UserName in the HTML file
            var res = await client.SendAsync(req);
            var resS = await res.Content.ReadAsStringAsync();
            var pattern = @"name"":""([^""]+)";
            var match = Regex.Match(resS, pattern);
            if (!match.Success) return "";
            this.stu.name = match.Value.Replace("name\":\"", "");
            return match.Value.Replace("name\":\"", "");
        }
        public async Task<List<SignEvent>> GetSignList()
        {
            var client = new HttpClient();
            var req = GenReq(SignMode.Common, HttpMethod.Get, TMURL.URL_ACTIVESIGNS, stu.openid);
            var res = await client.SendAsync(req);
            var jsonstr = await res.Content.ReadAsStringAsync();
            using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(jsonstr)))
            {
                var serializer = new DataContractJsonSerializer(typeof(List<SignEvent>));
                signs = (List<SignEvent>)serializer.ReadObject(ms);
            }
            return signs;
        }
        public async Task<KeyValuePair<bool, string>> Sign()
        {
            var client = new HttpClient();
            var req = GenReq(SignMode.Sign, HttpMethod.Post, TMURL.URL_CHECKIN, stu.openid);
            var body = new Dictionary<string, string>();
            if (signs == null) await GetSignList();
            if (signs.Count == 0) return new KeyValuePair<bool, string>(false, "暂无签到");
            foreach (var sign in signs)
            {
                Console.WriteLine($"课堂：{sign.name}");
                if (sign.isQR)
                {
                    Console.WriteLine("不支持二维码签到");
                    return new KeyValuePair<bool, string>(false, $"{sign.name}不支持二维码签到");
                }

                body.Add("courseId", sign.courseId);
                body.Add("signId", sign.signId);
                req.Headers.Add("Referer", String.Format(TMURL.URL_CHECKINREFER_T, sign.courseId));
                req.Content = new FormUrlEncodedContent(body);
                var res = await client.SendAsync(req);
                var jsonstr = await res.Content.ReadAsStringAsync();
                var serializer = new DataContractJsonSerializer(typeof(SignSuccessEvent));
                var ms = new MemoryStream(Encoding.UTF8.GetBytes(jsonstr));
                var signResult = (SignSuccessEvent)serializer.ReadObject(ms);
                if (signResult.signRank == null)
                {
                    if (jsonstr.Contains(TMURL.CODE_REPEATSIGNIN))
                    {
                        Console.WriteLine("您已经签到成功！");
                        return new KeyValuePair<bool, string>(true, $"{sign.name}:已经签到成功。");
                    }
                    else return new KeyValuePair<bool, string>(false, $"{sign.name}:异常。");
                }
                Console.WriteLine(signResult);
                Console.WriteLine(await res.Content.ReadAsStringAsync());
                Console.WriteLine(res.StatusCode);
                return new KeyValuePair<bool, string>(true, $"{sign.name}:排名{signResult.studentRank}");


            }
            return new KeyValuePair<bool, string>(true, "");
        }

    }
}