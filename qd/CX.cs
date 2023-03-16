using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ChaoXing
{
    public class GlobalVariables
    {
        public static string g_cookie { get; set; }
    }
    public class Utils
    {
        /// <summary>
        /// 内置的加密字符串
        /// </summary>
        public static string sKey = "u2oh6Vu^HWe40fj";
        public static string Base64Encode(string plainText)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
        }
        /// <summary>
        /// DES使用内置密钥进行加密
        /// </summary>
        /// <param name="Text">需要加密的文字</param>
        /// <returns>返回大写字符串</returns>
        public static string DESEncrypt(string Text)
        {
            return DESEncrypt(Text, sKey);
        }
        /// <summary>
        /// 2022年5月26日更新了DES
        /// </summary>
        /// <param name="Text"></param>
        /// <param name="sKey">就是key</param>
        /// <returns>返回大写字符串</returns>
        public static string DESEncrypt(string Text, string sKey)
        {
            var des = DES.Create();
            var md5 = MD5.Create();
            var keymd5 = md5.ComputeHash(System.Text.Encoding.ASCII.GetBytes(sKey));
            //DESCryptoServiceProvider des = new DESCryptoServiceProvider();
            byte[] inputByteArray;
            inputByteArray = Encoding.Default.GetBytes(Text);

            var k3 = BitConverter.ToString(keymd5).Replace("-", "").ToLower();
            des.Key = Encoding.ASCII.GetBytes(sKey[..8]);
            des.IV = Encoding.ASCII.GetBytes(sKey[..8]);
            des.Mode = CipherMode.ECB;
            des.Padding = PaddingMode.PKCS7;
            MemoryStream ms = new MemoryStream();
            CryptoStream cs = new CryptoStream(ms, des.CreateEncryptor(), CryptoStreamMode.Write);
            cs.Write(inputByteArray, 0, inputByteArray.Length);
            cs.FlushFinalBlock();
            StringBuilder ret = new StringBuilder();
            foreach (byte b in ms.ToArray())
            {
                ret.AppendFormat("{0:X2}", b);
            }
            return ret.ToString();
        }

        public static Signer.PositionWithName RandomizePosition(Signer.PositionWithName original, float precision = 0.000080f)
        {
            Random r = new Random();
            Console.WriteLine($"{original.lat},{original.lon}");

            float num1 = precision * r.Next(1000) / 1000;
            float flag1 = r.Next(2) * 2 - 1;

            float num2 = precision * r.Next(1000) / 1000;
            float flag2 = r.Next(2) * 2 - 1;

            original.lat += flag1 * num1;
            original.lon += flag2 * num2;
            Console.WriteLine($"{original.lat},{original.lon}");

            return original;
        }
    }
    public class CXURL
    {
        public const string LOGIN_PAGE = "https://passport2.chaoxing.com/mlogin?fid=&newversion=true&refer=http%3A%2F%2Fi.chaoxing.com";
        public const string LOGIN = "https://passport2.chaoxing.com/fanyalogin";
        public const string PPTSIGN = "https://mobilelearn.chaoxing.com/pptSign/stuSignajax";
        public const string COURSELIST = "https://mooc1-1.chaoxing.com/visit/courselistdata";
        public const string ACTIVELIST = "https://mobilelearn.chaoxing.com/v2/apis/active/student/activelist";
        public const string ACCOUNTMANAGE = "https://passport2.chaoxing.com/mooc/accountManage";
        public const string PANCHAOXING = "https://pan-yz.chaoxing.com";
        public const string PANLIST = "https://pan-yz.chaoxing.com/opt/listres";
        public const string PRESIGN = "https://mobilelearn.chaoxing.com/newsign/preSign";
        public const string UA = @"Dalvik/2.1.0 (Linux; U; Android 10; MI 8 MIUI/V12.0.3.0.QEACNXM) com.chaoxing.mobile/ChaoXingStudy_3_4.7.4_android_phone_593_53";
        public const string PC_UA = @"Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/111.0.0.0 Safari/537.36";
    }
    public class Student
    {
        public string name { get; set; }
        public override string ToString()
        {
            return $"<class ChaoXing.Student> name={name}";

        }
        public bool logined = false;
        public LoginParams loginInfo { get; internal set; } = new LoginParams();
        public class LoginParams
        {
            public string fid { get; set; }
            public string pid { get; set; }
            public string refer { get; set; }
            public string _blank { get; set; }
            public string t { get; set; }
            public string vc3 { get; set; }
            public string _uid { get; set; }
            public string _d { get; set; }
            public string uf { get; set; }
            public override string ToString()
            {
                return $"<class ChaoXing.Student.LoginParams> uid={_uid}";
            }
        }

        /// <summary>
        /// Default Constructor to init LoginParams
        /// </summary>
        public Student()
        {
            loginInfo.fid = "-1";
            loginInfo.pid = "-1";
            loginInfo.refer = "http%3A%2F%2Fi.chaoxing.com";
            loginInfo._blank = "1";
            loginInfo.t = "true";
            loginInfo.vc3 = "null";
            loginInfo._uid = "null";
            loginInfo._d = "null";
            loginInfo.uf = "null";
        }

        /// <summary>
        /// Constructor with username and password
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        public Student(string username, string password) : this()
        {
            this.Login(username, password).Wait();
        }
        /// <summary>
        /// When get a Cookie, you can use this method to fill params' blanks
        /// </summary>
        /// <param name="coo">A single cookie</param>
        /// <param name="index">To recognize</param>
        /// <returns></returns>
        string CookieHandler(string coo, int index)
        {
            // TODO: 重写Cookie Handler
            // 以适应更改的Cookie结构
            string ret = null;
            switch (index)
            {
                case 1:
                    ret = coo[4..coo.IndexOf(";")];
                    loginInfo.fid = ret;
                    break;
                case 8:
                    ret = coo[4..coo.IndexOf(";")];
                    loginInfo.vc3 = ret;
                    break;
                case 2:
                    ret = coo[5..coo.IndexOf(";")];
                    loginInfo._uid = ret;
                    break;
                case 4:
                    ret = coo[3..coo.IndexOf(";")];
                    loginInfo._d = ret;
                    break;
                case 3:
                    ret = coo[3..coo.IndexOf(";")];
                    loginInfo.uf = ret;
                    break;
            }
            return ret;
        }


        /// <summary>
        /// Login to ChaoXing
        /// </summary>
        /// <param name="username">用户名</param>
        /// <param name="password">密码</param>
        /// <returns></returns>
        public async Task Login(string u, string p)
        {
            var client = new HttpClient();
            var req = new HttpRequestMessage(HttpMethod.Get, CXURL.LOGIN_PAGE);
            await client.SendAsync(req);
            var reqLogin = new HttpRequestMessage(HttpMethod.Post, CXURL.LOGIN);
            //reqL.Headers.Add("Content-Type", "application/x-www-form-urlencoded");
            reqLogin.Headers.Add("X-Requested-With", "XMLHttpRequest");
            var body = new Dictionary<string, string>
                {
                    { "uname", u },
                    { "password", Utils.DESEncrypt(p).ToLower() },
                    { "fid", "-1" },
                    { "t", "true" },
                    { "refer", "http://i.chaoxing.com" },
                    {"forbidotherlogin", "0" }
                };
            reqLogin.Content = new FormUrlEncodedContent(body);
            var res = await client.SendAsync(reqLogin);
            var cookie = res.Headers.GetValues("set-cookie");
            int y = 0;
            var original_cookie = "";
            foreach (var coo in cookie)
            {
                original_cookie += coo[..coo.IndexOf(";")];
                original_cookie += "; ";
                CookieHandler(coo, y);
                y += 1;
                if (y == 9) break;
            }
            logined = true;
            Console.WriteLine(original_cookie);
            GlobalVariables.g_cookie = original_cookie;
        }
        /// <summary>
        /// When you use this method, you must login first
        /// </summary>
        /// <returns></returns>
        public async Task<string> GetAccountName()
        {
            if (!logined) return null;
            return await GetAccountName(loginInfo.uf, loginInfo._d, loginInfo._uid, loginInfo.vc3);
        }
        /// <summary>
        /// 使用标准登录表进行登录
        /// </summary>
        /// <param name="login"></param>
        /// <returns></returns>
        public static async Task<string> GetAccountName(LoginParams login)
        {
            return await GetAccountName(login.uf, login._d, login._uid, login.vc3);
        }
        public static async Task<string> GetAccountName(string uf, string _d, string _uid, string vc3)
        {
            var client = new HttpClient();
            var req = new HttpRequestMessage(HttpMethod.Get, CXURL.ACCOUNTMANAGE);
            req.Headers.Add("Cookie", $"uf={uf}; _d={_d}; UID={_uid}; vc3={vc3};");
            var res = await client.SendAsync(req);
            var resS = await res.Content.ReadAsStringAsync();
            var match = Regex.Match(resS, @"messageName([^>]+)");
            if (!match.Success) return null;
            var name = match.Groups[1].Value[9..^1];
            return name;
        }

    }
    public class Signer
    {
        [Obsolete("This int enum is deprecated, please use Signer.SignType instead.")]
        public static readonly int TYPE_SIGN_CLOSE = 2;
        [Obsolete("This int enum is deprecated, please use Signer.SignType instead.")]
        public static readonly int TYPE_SIGN_OPEN = 1;
        public enum SignOpenType : int
        {
            CLOSE = 2,
            OPEN = 1
        }
        public struct PositionWithName
        {
            public double lat, lon; // lat是纬度，lon是经度
            public string showName, upName;
        }
        public static readonly Dictionary<string, PositionWithName> LocationDict = new Dictionary<string, PositionWithName>{
            {"南一", new PositionWithName{lon=114.420163,lat=30.515399,showName="南一",upName="中国湖北省武汉市洪山区关山街道华中路华中科技大学" }},

            {"西十二SE",new PositionWithName{lon=114.414461,lat=30.514471,showName="西十二SE",upName="中国湖北省武汉市洪山区关山街道西五路华中科技大学" }},
            {"西十二SW",new PositionWithName{lon=114.413239,lat=30.514366,showName="西十二SW",upName="中国湖北省武汉市洪山区关山街道西五路华中科技大学" }},
            {"西十二NE",new PositionWithName{lon=114.414299,lat=30.514871,showName="西十二NE",upName="中国湖北省武汉市洪山区关山街道西五路华中科技大学" }},
            {"西十二NW",new PositionWithName{lon=114.413123,lat=30.514720,showName="西十二NW",upName="中国湖北省武汉市洪山区关山街道西五路华中科技大学" }},

            {"东九A",new PositionWithName{lon=114.433650,lat=30.519738,showName="东九A",upName="中国湖北省武汉市洪山区关山街道喻家湖路辅路华中科技大学(东校区)" }},
            {"东九B",new PositionWithName{lon=114.433502,lat=30.519349,showName="东九B",upName="中国湖北省武汉市洪山区关山街道喻家湖路辅路华中科技大学(东校区)" }},
            {"东九C",new PositionWithName{lon=114.433448,lat=30.518987,showName="东九C",upName="中国湖北省武汉市洪山区关山街道喻家湖路辅路华中科技大学(东校区)" }},
            {"东九D",new PositionWithName{lon=114.433350,lat=30.518544,showName="东九D",upName="中国湖北省武汉市洪山区关山街道喻家湖路辅路华中科技大学(东校区)" }},
        };
        public enum SignActiveType : int
        {
            COMMON,   //普通签到
            QRSIGN,   //二维码签到
            LOCSIGN,  //位置签到
            UNKNOWN
        }
        /// <summary>
        /// 二维码签到的有效信息
        /// </summary>
        public class SignCode
        {
            public string aid { get; set; }
            public string source { get; set; }
            public string Code { get; set; }
            public string enc { get; set; }
            public override string ToString()
            {
                return $"<class ChaoXing.Signer.SignCode> aid={aid} enc={enc}";
            }
            public static SignCode Parse(string s)
            {
                // Origin string: https://mobilelearn.chaoxing.com/widget/sign/e?id=5000040360449&c=340848&enc=9B7581E2E4C6544D299F457084BDA701&DB_STRATEGY=PRIMARY_KEY&STRATEGY_PARA=id
                // match id,c,enc
                // at the same time let source=""
                var match = Regex.Match(s, @"id=(\d+)&c=(\d+)&enc=([0-9A-F]+)");
                if (!match.Success) throw new Exception("Invalid sign code");
                // aid = id, source = "", Code = c, enc = enc
                SignCode code = new SignCode()
                {
                    aid = match.Groups[1].Value,
                    source = "",
                    Code = match.Groups[2].Value,
                    enc = match.Groups[3].Value
                };
                return code;
            }
        }
        /// <summary>
        /// 对于每个用户复制的二维码签到码进行一个规整化
        /// </summary>
        /// <param name="sign">签到二维码字符串</param>
        /// <returns></returns>
        public static SignCode SignIdSizer(string sign)
        {
            try
            {
                sign = sign.Split("?")[1];
                var codes = sign.Split("&");
                var signcode = new SignCode
                {
                    aid = codes[0][3..],
                    Code = codes[1][2..],
                    enc = codes[2][4..],
                    source = ""
                };
                return signcode;
            }
            catch
            {
                Console.WriteLine("SignId Resize ERROR!");
                return null;
            }
        }
        #region 反序列化类型类
        /// <summary>
        /// 课堂
        /// </summary>
        [DataContract]
        public class Course
        {
            [DataMember]
            public string courseId { get; set; }
            [DataMember]
            public string clazzId { get; set; }
            public string name { get; set; }
            public SignActive[] signActives { get; set; }
            public Course(string courseId, string clazzId) { this.courseId = courseId; this.clazzId = clazzId; }
            public override string ToString()
            {
                return $"<class ChaoXing.Signer.Course> courseId={courseId} name={name}";
            }
        }
        /// <summary>
        /// 每个签到的有用信息
        /// </summary>
        [DataContract]
        public class SignActive
        {
            [DataMember]
            public string nameOne { get; set; }//活动名字

            [DataMember]
            public long id { get; set; }//activeId

            [DataMember]
            public int status { get; set; }//

            [DataMember(Name = "nameFour")]
            public string nameFour { get; set; }

            [DataMember]
            public int type { get; set; }

            [DataMember(Name = "otherId")]
            public int signType { get; set; }

            public override string ToString()
            {
                return $"<class ChaoXing.Signer.SignActive> aid={id} nameFour={nameFour} nameOne={nameOne} status={status}";
            }
            /// <summary>
            /// 用来获取签到的形式。 2023/3/14更改了判别格式
            /// 2：二维码
            /// 4：位置
            /// 5：签到码
            /// </summary>
            /// <returns></returns>
            public SignActiveType GetSignActiveType()
            {
                //if (nameOne.Contains("二维码"))
                if (2 == signType)
                {
                    return SignActiveType.QRSIGN;
                }
                //else if (nameOne.Contains("位置"))
                else if (4 == signType)
                {
                    return SignActiveType.LOCSIGN;
                }
                else
                {
                    return SignActiveType.COMMON;
                }
            }
        }
        /// <summary>
        /// 获取签到列表的反序列化对象
        /// </summary>
        [DataContract]
        public class SignEvent
        {
            [DataContract]
            public class Rootobject
            {
                [DataMember]
                public int result { get; set; }
                [DataMember]
                public Data data { get; set; }
            }
            [DataContract]
            public class Data
            {
                [DataMember]
                public SignActive[] activeList { get; set; }
            }

        }
        #endregion
        /// <summary>
        /// 通过课程id和课堂id来获取签到列表
        /// 注意：非签到类型已经过滤
        /// </summary>
        /// <param name="login">登录信息</param>
        /// <param name="courceId">课程id</param>
        /// <param name="clazzId">课堂id</param>
        /// <returns>签到列表</returns>
        public static async Task<SignActive[]> GetSignActives(Student.LoginParams login, string courceId, string clazzId)
        {
            var client = new HttpClient();
            var content = new FormUrlEncodedContent(new Dictionary<string, string>{
                //fid=0&courseId=${courses[i].courseId}&classId=${courses[i].classId}&_=${new Date().getTime()}
                {"fid","0" },
                {"courseId",courceId },
                {"classId",clazzId },
                {"_",DateTimeOffset.Now.ToUnixTimeMilliseconds().ToString() }
            });
            var getUrl = CXURL.ACTIVELIST + "?" + await content.ReadAsStringAsync();
            var req = new HttpRequestMessage(HttpMethod.Get, getUrl);
            req.Headers.Add("Cookie", $"uf={login.uf}; _d={login._d}; UID={login._uid}; vc3={login.vc3};");
            var res = await client.SendAsync(req);
            var resS = await res.Content.ReadAsStringAsync();
            var ms = new MemoryStream(Encoding.UTF8.GetBytes(resS));
            var serializer = new DataContractJsonSerializer(typeof(SignEvent.Rootobject));
            var rootobject = (SignEvent.Rootobject)serializer.ReadObject(ms);
            var activeList = new List<SignActive>();
            foreach (var a in rootobject.data.activeList)
            {
                if ((int)SignOpenType.OPEN == a.status) activeList.Add(a);
            }
            return activeList.ToArray();
        }
        /// <summary>
        /// 获得当前用户的所有课程
        /// </summary>
        /// <param name="login"></param>
        /// <returns></returns>
        public static async Task<Course[]> GetCourses(Student.LoginParams login)
        {
            var client = new HttpClient();
            var req = new HttpRequestMessage(HttpMethod.Post, CXURL.COURSELIST);
            var listCourse = new List<Course>();
            req.Headers.Add("Accept", "text/html, */*; q=0.01");
            req.Headers.Add("Accept-Encoding", "deflate");
            req.Headers.Add("Accept-Language", "zh-CN,zh;q=0.9,en;q=0.8,en-GB;q=0.7,en-US;q=0.6");
            req.Headers.Add("Accept", "text/html, */*; q=0.01");
            req.Headers.Add("Cookie", $"_uid={login._uid}; _d={login._d}; vc3={login.vc3}");
            req.Content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                { "courseType","1" },
                { "courseFolderId","0" },
                { "courseFolderSize","0" }
            });
            var res = await client.SendAsync(req);
            var resS = await res.Content.ReadAsStringAsync();
            for (var i = 1; ; i++)
            {
                i = resS.IndexOf("course_", i);
                if (i == -1) break;
                var end_of_courseid = resS.IndexOf('_', i + 7);
                var newCourse = new Course(resS[(i + 7)..end_of_courseid], resS[(end_of_courseid + 1)..resS.IndexOf('"', i + 1)]);
                i = resS.IndexOf("title=\"", end_of_courseid);
                end_of_courseid = resS.IndexOf('"', i + 7);
                newCourse.name = resS[(i + 7)..end_of_courseid];
                listCourse.Add(newCourse);
            }
            return listCourse.ToArray();
        }
        /// <summary>
        /// 获取完整的课程列表，包括所有签到。
        /// </summary>
        /// <param name="login"></param>
        /// <returns></returns>
        public static async Task<Course[]> GetCoursesFull(Student.LoginParams login)
        {
            var courses = await GetCourses(login);
            foreach (var c in courses)
            {
                c.signActives = await GetSignActives(login, c.courseId, c.clazzId);
            }
            return courses;
        }
        #region 各种签到的表面形式
        /// <summary>
        /// 地理位置签到
        /// </summary>
        /// <param name="login">stu类的登录参数</param>
        /// <param name="aid">签到活动id</param>
        /// <param name="name">用户姓名</param>
        /// <param name="lat">纬度</param>
        /// <param name="lon">经度</param>
        /// <param name="address">地理位置的描述</param>
        /// <returns>(awaitable)<成功与否，信息></returns>
        [Obsolete("请使用SignLoc")]
        public static async Task<KeyValuePair<bool, string>> Sign(Student.LoginParams login, string aid, string name, double lat, double lon, string address)
        {

            return await SignLoc(login, aid, name, new PositionWithName() { lat = lat, lon = lon, upName = address });
        }

        /// <summary>
        /// 地理位置签到，使用新街口
        /// </summary>
        /// <param name="login"></param>
        /// <param name="aid"></param>
        /// <param name="stuname"></param>
        /// <param name="pos"></param>
        /// <returns></returns>
        public static async Task<KeyValuePair<bool, string>> SignLoc(Student.LoginParams login, string aid, string stuname, PositionWithName pos)
        {
            if (login == null) throw new ArgumentNullException("login");
            // lat = lat == 0 ? 30.514471 : lat;
            // lon = lon == 0 ? 114.414461 : lon;
            // address = address ?? "中国湖北省武汉市洪山区关山街道西五路华中科技大学";
            var content = new FormUrlEncodedContent(new Dictionary<string, string> {
                {"address",pos.upName },
                {"activeId",aid },
                {"uid",login._uid },
                {"clientip","" },
                {"useragent","" },
                {"latitude",$"{pos.lat:F6}"},
                {"longitude",$"{pos.lon:F6}" },
                {"fid",login.fid },
                {"appType","15" },
                {"name",stuname }
            });
            return await SignRoot(login, content, aid);
        }
        /// <summary>
        /// 普通签到或手势签到
        /// </summary>
        /// <param name="login"></param>
        /// <param name="aid"></param>
        /// <param name="name">用户姓名</param>
        /// <returns></returns>
        public static async Task<KeyValuePair<bool, string>> SignCommon(Student.LoginParams login, string aid, string name)
        {

            if (login == null) throw new ArgumentNullException("login");
            var content = new FormUrlEncodedContent(new Dictionary<string, string> {
                {"activeId",aid },
                {"uid",login._uid },
                {"clientip","" },
                {"useragent","" },
                {"latitude","-1"},
                {"longitude","-1" },
                {"fid",login.fid },
                {"appType","15" },
                {"name",name }
            });
            return await SignRoot(login, content, aid);
        }
        /// <summary>
        /// 二维码签到
        /// </summary>
        /// <param name="login"></param>
        /// <param name="sign"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        [Obsolete("二维码签到请使用SignQR")]
        public static async Task<KeyValuePair<bool, string>> Sign(Student.LoginParams login, Signer.SignCode sign, string name)
        {
            return await SignQR(login, name, sign);
        }
        /// <summary>
        /// QR签到
        /// </summary>
        /// <param name="login"></param>
        /// <param name="name"></param>
        /// <param name="sign"></param>
        /// <returns></returns>
        public static async Task<KeyValuePair<bool, string>> SignQR(Student.LoginParams login, string name, Signer.SignCode sign)
        {
            if (login == null) throw new ArgumentNullException("login");
            if (sign == null) throw new ArgumentNullException("sign");
            FormUrlEncodedContent content = new FormUrlEncodedContent(new Dictionary<string, string> {
                {"enc",sign.enc},
                {"name",name },
                {"activeId",sign.aid },
                {"uid",login._uid },
                {"clientip","" },
                {"useragent","" },
                {"latitude","-1"},
                {"longitude","-1" },
                {"fid",login.fid },
                {"appType","15" }
            });
            return await SignRoot(login, content, sign.aid);
        }
        public static async Task<KeyValuePair<bool, string>> SignQR(Student.LoginParams login, string name, string sign)
        {
            return await SignQR(login, name, Signer.SignCode.Parse(sign));
        }
        #endregion

        /// <summary>
        /// 使用原始的cookie string进行签到
        /// </summary>
        /// <param name="cookie_string">cookie</param>
        /// <param name="content"></param>
        /// <param name="aid"></param>
        /// <returns></returns>
        protected static async Task<KeyValuePair<bool, string>> SignRoot(string cookie_string, HttpContent content, string aid)
        {
            var client = new HttpClient();
            // 20220914 debug found new interface
            // you must GET the presign url so that you can do anything else
            FormUrlEncodedContent pre_content = new FormUrlEncodedContent(new Dictionary<string, string> {
                {"activePrimaryId",aid},//aid
                {"general","1" },
                {"sys","1" },
                {"ls","1" },
                {"appType","15" },
                {"ut","s" }
            });
            var pre_getUrl = CXURL.PRESIGN + "?" + await pre_content.ReadAsStringAsync();

            var pre_req = new HttpRequestMessage(HttpMethod.Get, pre_getUrl);
            pre_req.Headers.Add("Cookie", cookie_string);
            pre_req.Headers.Add("User-Agent", CXURL.UA);
            try
            {
                var pres = await client.SendAsync(pre_req);
                //var presS = await pres.Content.ReadAsStringAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine("第一步签到发生错误！");
                Console.WriteLine(ex.Message);
                return new KeyValuePair<bool, string>(false, ex.Message);
            }

            // 由于setcookie的存在，之前的client的cookie被覆写。所以我们暂时的解决方法是另写一个client.
            var client2 = new HttpClient();
            client.Dispose();
            var paramGet = await content.ReadAsStringAsync();
            var getUrl = CXURL.PPTSIGN + "?" + paramGet;
            Console.WriteLine(getUrl);
            var req = new HttpRequestMessage(HttpMethod.Get, getUrl);
            req.Headers.Add("Cookie", cookie_string);
            req.Headers.Add("User-Agent", CXURL.PC_UA);
            req.Version = new System.Version(1, 1);
            try
            {
                var res = await client2.SendAsync(req);
                var resS = await res.Content.ReadAsStringAsync();
                Console.WriteLine(resS);
                if (resS.Contains("success")) return new KeyValuePair<bool, string>(true, resS);
                return new KeyValuePair<bool, string>(false, resS);
            }
            catch (Exception err)
            {
                return new KeyValuePair<bool, string>(false, err.Message);
            }
        }
        /// <summary>
        /// 签到的根函数
        /// </summary>
        /// <param name="login"></param>
        /// <param name="content">给服务器发送的参数</param>
        /// <returns></returns>
        protected static async Task<KeyValuePair<bool, string>> SignRoot(Student.LoginParams login, HttpContent content, string aid)
        {
            var cookie_string = $"uf={login.uf}; _d={login._d}; UID={login._uid}; vc3={login.vc3}; _uid={login._uid}";
            return await SignRoot(cookie_string, content, aid);
        }
    }
}
