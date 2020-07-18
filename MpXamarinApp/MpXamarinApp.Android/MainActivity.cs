using System;

using Android.App;
using Android.Content.PM;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Amazon;
using Amazon.S3;
using Amazon.CognitoIdentity;
using System.Security.Cryptography;
using Android;

namespace MpXamarinApp.Droid {
    [Activity(Label = "MpXamarinApp", Icon = "@mipmap/icon", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity {
        const int RequestLocationId = 0;

        readonly string[] LocationPermissions = {
            Manifest.Permission.AccessCoarseLocation,
            Manifest.Permission.AccessFineLocation
        };

        protected override void OnCreate(Bundle savedInstanceState) {
            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;

            base.OnCreate(savedInstanceState);

            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            global::Xamarin.Forms.Forms.Init(this, savedInstanceState);
            Xamarin.FormsMaps.Init(this, savedInstanceState);
            LoadApplication(new App());
        }

        protected override void OnStart() {
            base.OnStart();

            if ((int)Build.VERSION.SdkInt >= 23) {
                if (CheckSelfPermission(Manifest.Permission.AccessFineLocation) != Permission.Granted) {
                    RequestPermissions(LocationPermissions, RequestLocationId);
                } else {
                    // Permissions already granted - display a message.
                }
            }
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Permission[] grantResults) {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);
            if (requestCode == RequestLocationId) {
                if ((grantResults.Length == 1) && (grantResults[0] == (int)Permission.Granted)) {
                    // Permissions granted - display a message.
                } else {
                    // Permissions denied - display a message.
                }                
            } else {
                base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
            }
        }
        //public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults) {
        //    Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

        //    base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        //}
        private void InitAmazon() {
            /*
            When you log to SystemDiagnostics, the framework internally prints the 
            output to the System.Console. If you want to log HTTP responses, set the 
            LogResponses flag. The values can be Always, Never, or OnError.

            You can also log performance metrics for HTTP requests by using the
            LogMetrics property. The log format can be specified by using 
            LogMetricsFormat property. Valid values are JSON or standard. 
            */
            var loggingConfig = AWSConfigs.LoggingConfig;
            loggingConfig.LogMetrics = true;
            loggingConfig.LogResponses = ResponseLoggingOption.Always;
            loggingConfig.LogMetricsFormat = LogMetricsFormatOption.JSON;
            loggingConfig.LogTo = LoggingOptions.SystemDiagnostics;

            AWSConfigs.AWSRegion = "us-east-1";

            // Initialize the Amazon Cognito credentials provider
            CognitoAWSCredentials credentials = new CognitoAWSCredentials(
                "us-east-1:ba854715-37ed-4fd1-aae5-374764cf3414", // Identity pool ID
                RegionEndpoint.USEast1 // Region
            );

            IAmazonS3 s3Client = new AmazonS3Client(credentials, RegionEndpoint.USEast1);

            AWSConfigs.CorrectForClockSkew = true;

            //This field is set if a service call resulted in an exception and the SDK 
            //has determined that there is a difference between local and server times.
            var offset = AWSConfigs.ClockOffset;
        }
    }

}
