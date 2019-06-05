using System;
using Xamarin.UITest;
using Xamarin.UITest.Queries;

namespace Nester.Deploy.Tests
{
    public class AppInitializer
    {
        public static IApp StartApp(Platform platform)
        {
            if (platform == Platform.Android)
            {
                return ConfigureApp.Android.StartApp();
            }
            else if (platform == Platform.)

            return ConfigureApp.iOS.StartApp();
        }
    }
}