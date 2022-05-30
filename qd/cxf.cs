using Android.Support.V4.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AndroidX.Fragment.App;

using ChaoXing;
using Xamarin.Essentials;
using System.Threading.Tasks;

namespace qd
{
    public class Cxf : AndroidX.Fragment.App.Fragment
    {
        private Student stu;
        private EditText eUsername;
        private EditText ePassword;
        private CheckBox cAutologin;
        private CheckBox cSavePswd;
        private TextView loginStat;
        private EditText editOpenCode;
        private TextView signStat;

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            return inflater.Inflate(Resource.Layout.cx, container, false);
        }

        public override async void OnActivityCreated(Bundle savedInstanceState)
        {
            base.OnActivityCreated(savedInstanceState);

            stu = new Student();

            eUsername = View.FindViewById<EditText>(Resource.Id.cxTxtUserName);
            ePassword = View.FindViewById<EditText>(Resource.Id.cxTxtPassword);
            cAutologin = View.FindViewById<CheckBox>(Resource.Id.cxChkAutoLogin);
            cSavePswd = View.FindViewById<CheckBox>(Resource.Id.cxChkRememberPswd);
            loginStat = View.FindViewById<TextView>(Resource.Id.cxTxtStatus);
            editOpenCode = View.FindViewById<EditText>(Resource.Id.cxTxtCode);
            signStat = View.FindViewById<TextView>(Resource.Id.cxTxtSignStat);


            Button cxLogin = View.FindViewById<Button>(Resource.Id.cxBtnLogin);
            cxLogin.Click += CxLogin_Click;
            Button cxPaste = View.FindViewById<Button>(Resource.Id.cxBtnPaste);
            cxPaste.Click += CxPaste_Click;
            Button cxSign = View.FindViewById<Button>(Resource.Id.cxBtnsign);
            cxSign.Click += CxQRSign_Click;
            Button cxGenSign = View.FindViewById<Button>(Resource.Id.cxBtnSignLoc);
            cxGenSign.Click += CxGenSign_Click;
            Button cxXcan = View.FindViewById<Button>(Resource.Id.cxBtnScan);
            cxXcan.Click += CxXcan_Click;
            // 创建用户文件
            await UserFile.ExistOrCreate();
            // 如果用户已经登录 则跳过登录这一步骤
            if (stu.logined) return;
            // 使用本地用户缓存直接尝试登录
            Task<bool> localLoginTask = CxLoginLocal();
            loginStat.Text = "缓存登录中..";
            var vs = await UserFile.GetArrayFromFile();
            if (vs == null || vs.Length != 3) return;
            eUsername.Text = vs[0];// 用户名
            ePassword.Text = vs[1];// 密码
            if (vs[1].Length != 0) cSavePswd.Checked = true;
            if (vs[2] == "a")
            {
                cAutologin.Checked = true;
                cSavePswd.Checked = true;
                if (await localLoginTask)
                {
                    stu.logined = true;
                }
                if (!stu.logined)
                {
                    await CxLogin(vs[0], vs[1]);
                }
            }
        }

        private async void CxXcan_Click(object sender, EventArgs e)
        {
            try
            {
                var s = await GetQRCodeFromCameraByZXingUsingUsersCamera();
                if (s != null)
                {
                    editOpenCode.Text = s;
                    //CxQRSign_Click(null, null);
                }
            }
            catch(Exception err)
            {
                signStat.Text += err.Message;
                signStat.Text += err.StackTrace;
            }
            
        }

        private async void CxGenSign_Click(object sender, EventArgs e)
        {
            if (!stu.logined) { loginStat.Text = "您还没有登录"; return; }
            signStat.Text = "开始检测并签到...\n";
            try
            {
                var courselist = await Signer.GetCourses(stu.loginInfo);
                signStat.Text += $"共检测到{courselist.Length}个签到\n";
                foreach (var course in courselist)
                {
                    course.signActives = await Signer.GetSignActives(stu.loginInfo, course.courseId, course.clazzId);
                    foreach (var signActive in course.signActives)
                    {
                        if (signActive.status == 1)
                        {
                            signStat.Text += $"检测到 {course.name} 有一个 {signActive.nameOne} 签到。\n";

                            KeyValuePair<bool, string> signRes;

                            // 目前仅仅支持手势签到和普通签到
                            if (signActive.nameOne.Contains("手势") || signActive.nameOne == "签到")
                            {
                                signRes = await 
                                    Signer.Sign(stu.loginInfo, signActive.id.ToString(), stu.name);
                            }

                            // 位置签到支持
                            else if (signActive.nameOne.Contains("位置"))
                            {
                                signRes = await
                                    Signer.Sign(stu.loginInfo, signActive.id.ToString(), stu.name, 0, 0, null);
                            }
                            else
                            {
                                signStat.Text += $"----{signActive.nameOne} 暂时不支持。你已经死了。\n";
                                continue;
                            }
                            // 签到结果的显示操作
                            switch (signRes.Key)
                            {
                                case true:
                                    signStat.Text += $"----{course.name} 的 {signActive.nameOne}->签到成功。\n";
                                    break;
                                case false:
                                    signStat.Text += $"----{course.name} 的 {signActive.nameOne}->签到失败。:{signRes.Value}\n";
                                    break;
                            }
                            
                        }
                    }
                    signStat.Text += $"{course.name}:检测完毕\n";
                }
            }
            catch(Exception err)
            {
                signStat.Text = err.Message + err.StackTrace;
                signStat.Text += "你已经死了。";
            }
            signStat.Text += "检测终了。";
        }

        private async void CxQRSign_Click(object sender, EventArgs e)
        {
            if (!stu.logined) { loginStat.Text = "您还没有登录"; return; }
            var login = stu.loginInfo;
            var code = Signer.SignIdSizer(editOpenCode.Text);
            if (code == null)
            {
                signStat.Text = "复制过来的东西解析有误";
                return;
            }
            try
            {
                var name = await stu.GetAccountName();
                var res = await Signer.Sign(login, code, name);
                if (res.Key)
                {
                    // Success to Sign.
                    signStat.Text = $"签到成功！{res.Value}";
                }
                else
                {
                    // Fail to Sign.
                    signStat.Text = $"签到失败！{res.Value}";
                }
            }
            catch (Exception e1)
            {
                signStat.Text = $"签到失败！{e1.Message}";
            }

        }

        private async void CxPaste_Click(object sender, EventArgs e)
        {
            var hasText = Clipboard.HasText;
            if (hasText)
            {
                var text = await Clipboard.GetTextAsync();
                editOpenCode.Text = text;
            }
        }

        private async void CxLogin_Click(object sender, EventArgs e)
        {
            await CxLogin(eUsername.Text, ePassword.Text);
        }
        private async Task<bool> CxLoginLocal()
        {
            try
            {
                var loginLocal = await UserFile.GetCxCodeFromFile();
                var nameGet = await Student.GetAccountName(loginLocal);
                stu.loginInfo = loginLocal;
                loginStat.Text = nameGet ?? throw new Exception("无法获取姓名");
                stu.name = nameGet;
                stu.logined = true;
                return true;
            }
            catch
            {
                return false;
            }
        }
        private async Task CxLogin(string username, string password)
        {
            if (username.Length == 0 || password.Length == 0) return;
            // use localfile to login
            //var localLoginSucceed = CxLoginLocal();
            loginStat.Text = "登录中...";
            //if (await localLoginSucceed) return;
            bool savepswd = cSavePswd.Checked;
            bool autologin = cAutologin.Checked;
            var tSaveFile = UserFile.SetArrayToFile(username, savepswd ? password : "", autologin);
            try
            {
                await stu.Login(username, password);
            }
            catch (Java.IO.IOException)
            {
                loginStat.Text = $"网络连接错误。";
                return;

            }
            catch (InvalidOperationException)
            {
                loginStat.Text = "用户名或密码错误.";
                return;
            }
            catch (Exception err)
            {
                loginStat.Text = $"登录失败{err.Message}";
                return;
            }
            finally
            {
                await tSaveFile;
            }
            var name = await stu.GetAccountName();
            await UserFile.SetCxCodeToFile(stu.loginInfo);
            loginStat.Text = name;
            stu.name = name;
        }

        public async Task<string> GetQRCodeFromCameraByZXingUsingUsersCamera()
        {
            var scanner = new ZXing.Mobile.MobileBarcodeScanner();
            scanner.TopText = "114514";
            scanner.BottomText = "1919810";
            var result = await scanner.Scan();
            return result.Text;
        }
    }
}