// using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.App;
using Xamarin.Essentials;
using Google.Android.Material.BottomNavigation;
using System;
using System.Threading.Tasks;
using cx = ChaoXing;
using tm = Teachermate;
using System.IO;
using AndroidX.Fragment.App;

namespace qd
{
    [Android.App.Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = true)]
    public class MainActivity : AppCompatActivity
    {
        Cxf cxf;
        Tmf tmf;
        Fragment isFragment;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            SetContentView(Resource.Layout.activity_main);

            BottomNavigationView navigation = FindViewById<BottomNavigationView>(Resource.Id.navigation);
            BottomNavigationView bottomNavigation = FindViewById<BottomNavigationView>(Resource.Id.navigation);
            bottomNavigation.NavigationItemSelected += BottomNavigation_NavigationItemSelected;

            tmf = new Tmf();
            isFragment = tmf;
            InitMainFrame(savedInstanceState);
        }
        private void InitMainFrame(Bundle savedInstance)
        {
            if (savedInstance == null)
            {
                FragmentManager fm = SupportFragmentManager;
                FragmentTransaction ft = fm.BeginTransaction();
                ft.Add(Resource.Id.frameLayout1, tmf).Commit();
            }
        }


        private void BottomNavigation_NavigationItemSelected(object sender, BottomNavigationView.NavigationItemSelectedEventArgs e)
        {
            switch (e.Item.ItemId)
            {
                case Resource.Id.navigation_tm:
                    if (tmf == null) { tmf = new Tmf(); }
                    SwitchContent(isFragment, tmf);
                    break;
                case Resource.Id.navigation_cx:
                    if (cxf == null) { cxf = new Cxf(); }
                    SwitchContent(isFragment, cxf);
                    break;
            }
        }


        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }


        public void SwitchContent(Fragment from, Fragment to)
        {
            if (isFragment != to)
            {
                isFragment = to;
                FragmentManager fm = this.SupportFragmentManager;
                //添加渐隐渐现的动画
                FragmentTransaction ft = fm.BeginTransaction();
                if(!to.IsAdded) {    // 先判断是否被add过
                    ft.Hide(from).Add(Resource.Id.frameLayout1, to).Commit(); // 隐藏当前的fragment，add下一个到Activity中
                } else
                {
                    ft.Hide(from).Show(to).Commit(); // 隐藏当前的fragment，显示下一个
                }
            }
        }
    }
}

