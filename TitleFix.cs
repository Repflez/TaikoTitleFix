using HarmonyLib;
using Microsoft.Xbox;
using System.Reflection;
using XGamingRuntime;

namespace TaikoTitleFix
{
    internal class TitleFix
    {
        private static GdkHelpers instancedClass;

        [HarmonyPatch(typeof(GdkHelpers), "SignInImpl")]
        [HarmonyPrefix]
        static bool PrefixSignIn()
        {
            return false;
        }

        [HarmonyPatch(typeof(GdkHelpers), "SignInImpl")]
        [HarmonyPrefix]
        static void CustomSignInImpl(GdkHelpers __instance, bool isSilently)
        {
            Traverse gdkHelpers = Traverse.CreateWithType("Microsoft.Xbox.GdkHelpers");
            instancedClass = __instance;

            // A copy of the original method, but reflected back as is intended
            if (!gdkHelpers.Field<bool>("_isSingInRunning").Value)
            {
                gdkHelpers.Field<bool>("_isSingInRunning").Value = true;
                gdkHelpers.Field<bool>("_isSingInFailure").Value = false;
                gdkHelpers.Field<bool>("_isUserChangeCancel").Value = false;
                gdkHelpers.Field<bool>("_isReady").Value = false;

                // This is where the actual fix is in. Namco's intention was to do silent sign in, so they used XUserAddOptions.AddDefaultUserSilently.
                // All good and stuff, however, there's a flaw. To login for the first time, you have to agree to the login, because of Microsoft reasons.
                // Which means this well intended action backfires, because AddDefaultUserSilently doesn't show the "Welcome to Xbox" dialog thing, even if
                // there's a need to show it, such as the "Do you agree to give T Tablet(what is this name????????) access to your Xbox Live data?" dialog,
                // which is needed to login in the first place!
                //
                // The actual fix is a literal one-liner: Change AddDefaultUserSilently to AddDefaultUserAllowingUI, which allows showing the dialog, AND
                // does the silent login Namco wanted _AFTER_ the initial login. Everything else here is there to replicate the original method's coding.

                SDK.XUserAddAsync(isSilently ? XUserAddOptions.AddDefaultUserAllowingUI : XUserAddOptions.None, new XUserAddCompleted(customXAddUserCompleted));
            }
        }

        private static void customXAddUserCompleted(int hresult, XUserHandle userHandle)
        {
            // Call the original method here, to make sure the game does return properly to the title screen.
            // I couldn't do this with just Traverse, so I used good old reflection.
            typeof(GdkHelpers).GetMethod("AddUserComplete", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(instancedClass, new object[] { hresult, userHandle });
        }
    }
}
