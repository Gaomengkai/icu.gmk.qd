using AndroidX.Fragment.App;
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
using System.Threading.Tasks;
using Xamarin.Essentials;

using Teachermate;

namespace qd
{
    public class Tmf : Fragment
    {

        private Student stu;
        private bool tmLogined;
        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            // Use this to return your custom view for this Fragment
            // return inflater.Inflate(Resource.Layout.YourFragment, container, false);
            return inflater.Inflate(Resource.Layout.tm, container, false);
        }
        public override void OnActivityCreated(Bundle savedInstanceState)
        {
            base.OnActivityCreated(savedInstanceState);

            Button tmLogin = View.FindViewById<Button>(Resource.Id.tmBtnLogin);
            tmLogin.Click += OnTmLoginClicked;

            Button tmChk = View.FindViewById<Button>(Resource.Id.tmBtnCheckSigns);
            tmChk.Click += OnTmBtnChkSignsClicked;

            Button tmOnekey = View.FindViewById<Button>(Resource.Id.tmBtnSign);
            tmOnekey.Click += OnTmBtnOnekeySignClicked;

            Button tmPaste = View.FindViewById<Button>(Resource.Id.tmBtnPaste);
            tmPaste.Click += OnTmBtnPasteClicked;
        }
        public async Task TmLogin()
        {
            TextView textName = View.FindViewById<TextView>(Resource.Id.tmTxtLoginStatus);
            textName.Text = "登录中...";
            EditText editOpenid = View.FindViewById<EditText>(Resource.Id.tmTxtOpenid);
            string openid = editOpenid.Text;
            try
            {
                openid = Student.OpenidSizer(openid);
            }
            catch
            {
                textName.Text = "openid解析失败";
                return;
            }
            stu = new Student(openid);
            Signer signer = new Signer(stu);
            try
            {
                string name = await signer.GetUserName();
                if (name.Length == 0)
                {
                    textName.Text = "登录失败。";
                    return;
                }
                // Openid Valid
                textName.Text = name;
                tmLogined = true;
            }
            catch (Java.IO.IOException)
            {
                textName.Text = "网络连接错误。";
                return;
            }
            // Enable "btnChkSigns:
            Button btnChkSigns = View.FindViewById<Button>(Resource.Id.tmBtnCheckSigns);
            btnChkSigns.Enabled = true;
        }
        public async void OnTmLoginClicked(object sender, EventArgs e)
        {
            await TmLogin();
        }
        public async void OnTmBtnOnekeySignClicked(object o, EventArgs e)
        {
            if (stu == null || tmLogined == false)
            {
                await TmLogin();
            }
            if (!tmLogined)
            {
                return;
            }
            Signer signer = new Signer(stu);
            TextView tmTxtSignStatus = View.FindViewById<TextView>(Resource.Id.tmTxtSignStatus);

            try
            {
                var signResult = await signer.Sign();
                bool signSuccess = signResult.Key;
                if (signSuccess) { tmTxtSignStatus.Text = $"签到成功！{signResult.Value}"; }
                else { tmTxtSignStatus.Text = $"签到失败！{signResult.Value}"; }
            }
            catch (Java.IO.IOException)
            {
                tmTxtSignStatus.Text = "网络错误";
            }
        }
        /// <summary>
        /// 检测签到的按钮
        /// </summary>
        /// <param name="o"></param>
        /// <param name="e"></param>
        public async void OnTmBtnChkSignsClicked(object o, EventArgs e)
        {
            // 这里已经假定用户已经登录
            TextView tmTxtSignStatus = View.FindViewById<TextView>(Resource.Id.tmTxtSignStatus);
            Signer signer = new Signer(stu);
            try
            {
                var list = await signer.GetSignList();
                if (list.Count == 0)
                {
                    tmTxtSignStatus.Text = "暂无签到";
                }
                else
                {
                    string ss = $"共{list.Count}个签到\n";
                    foreach (var sign in list)
                    {
                        ss += sign.name;
                        if (sign.isGPS) { ss += " --定位"; }
                        if (sign.isQR) { ss += " --二维码（不支持）"; }
                        ss += "\n";
                    }
                    tmTxtSignStatus.Text = ss;
                }
            }
            catch (Java.IO.IOException)
            {
                tmTxtSignStatus.Text = "网络错误。";
                return;
            }
            
        }
        public async void OnTmBtnPasteClicked(object o, EventArgs e)
        {
            var hasText = Clipboard.HasText;
            if (hasText)
            {
                var text = await Clipboard.GetTextAsync();
                EditText editOpenid = View.FindViewById<EditText>(Resource.Id.tmTxtOpenid);
                editOpenid.Text = text;
            }

        }
    }
}