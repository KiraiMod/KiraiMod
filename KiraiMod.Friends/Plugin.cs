using BepInEx;
using BepInEx.Configuration;
using BepInEx.IL2CPP;
using System;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace KiraiMod.Friends
{
    [BepInPlugin(GUID, "KM.Friends", "0.0.0")]
    public class Plugin : BasePlugin
    {
        public const string GUID = "me.kiraihooks.KiraiMod.Friends";

        internal RSA rsa;

        public override void Load()
        {
            SetupRSA();
        }

        private void SetupRSA()
        {
            rsa = RSA.Create(4096);
            ConfigEntry<string> entry = Config.Bind("Identity", "PrivateKey", "MIICCgKCAgEA" + Convert.ToBase64String(rsa.ExportParameters(true).Modulus));
            rsa.ImportRSAPrivateKey(Convert.FromBase64String(entry.Value), out int _);
        }
    }
}
