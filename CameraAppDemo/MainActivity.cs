namespace CameraAppDemo
{

    using System;
    using System.Collections.Generic;
    using Android.App;
    using Android.Content;
    using Android.Content.PM;
    using Android.Graphics;
    using Android.OS;
    using Android.Provider;
    using Android.Widget;
    using Java.IO;
    using Environment = Android.OS.Environment;
    using Uri = Android.Net.Uri;
    using System.Net;

    public static class App
    {
        public static File _file;
        public static File _dir;
        public static Bitmap bitmap;
    }

    [Activity(Label = "Camera App Demo", MainLauncher = true)]
    public class MainActivity : Activity
    {

        private ImageView _imageView;

        //Tätä kutsutaan kun StartActivityForResult on suoritettu eli kuva otettu
        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);

            // Make it available in the gallery

            Intent mediaScanIntent = new Intent(Intent.ActionMediaScannerScanFile);
            Uri contentUri = Uri.FromFile(App._file);
            mediaScanIntent.SetData(contentUri);

            //Tätä en ymmärrä.. Kuka tämän broadcastin kuulee? Käyttöjärjestelmäkö?
            SendBroadcast(mediaScanIntent);

            // Display in ImageView. We will resize the bitmap to fit the display
            // Loading the full sized image will consume to much memory 
            // and cause the application to crash.

            int height = Resources.DisplayMetrics.HeightPixels;
            int width = _imageView.Height;
            App.bitmap = App._file.Path.LoadAndResizeBitmap(width, height);
            if (App.bitmap != null)
            {
                _imageView.SetImageBitmap(App.bitmap);
                App.bitmap = null;
            }
            /*
            var url = "http://heinoar.azurewebsites.net/WebApi/UploadImage";
            
            WebClient myWebClient = new WebClient();
            try
            {
                myWebClient.UploadFile(url, App._file.ToString());
            }
            catch (Exception e)
            {
                Toast.MakeText(this, e.Message, ToastLength.Long).Show();
            }
            */

            // Dispose of the Java side bitmap.
            GC.Collect();
        }

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.Main);
            //jos laiteessa on kamera, luodaan hakemisto kuville
            //lisäksi käyttöliittymään tehdään nappi, jolla kuvan voi ottaa
            if (IsThereAnAppToTakePictures())
            {
                CreateDirectoryForPictures();

                Button button = FindViewById<Button>(Resource.Id.myButton);
                _imageView = FindViewById<ImageView>(Resource.Id.imageView1);
                button.Click += TakeAPicture;
            }

        }

        //Luodaan hakemisto kuville. Tätä kutsutaan OnCreate kohdassa
        private void CreateDirectoryForPictures()
        {
            App._dir = new File(
                Environment.GetExternalStoragePublicDirectory(
                    Environment.DirectoryPictures), "CameraAppDemo");
            if (!App._dir.Exists())
            {
                App._dir.Mkdirs();
            }
        }

        //tarkistetaan onko laitteessa kamera
        private bool IsThereAnAppToTakePictures()
        {
            Intent intent = new Intent(MediaStore.ActionImageCapture);
            IList<ResolveInfo> availableActivities =
                PackageManager.QueryIntentActivities(intent, PackageInfoFlags.MatchDefaultOnly);
            return availableActivities != null && availableActivities.Count > 0;
        }

        //Kutsutaan kun nappia painetaan
        private void TakeAPicture(object sender, EventArgs eventArgs)
        {
            //Intent tyyppiä käytetään käynnistämään androidissa muita sovelluksia
            Intent intent = new Intent(MediaStore.ActionImageCapture);
            //määritellään tiedosto, johon kuva tallennetaan
            App._file = new File(App._dir, String.Format("myPhoto_{0}.jpg", Guid.NewGuid()));
            //kerrotaan intentille mihin tiedostoon kuva tallennetaan
            intent.PutExtra(MediaStore.ExtraOutput, Uri.FromFile(App._file));
            //käynnistetään määritelty intent
            StartActivityForResult(intent, 0);
        }
    }
}