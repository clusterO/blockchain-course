using NBitcoin;
using System;

namespace RSA
{
    public static class RSA
    {
        public static Wallet GenerateKey()
        {
            Key privateKey = new Key();
            var publicKey = privateKey.PubKey.GetAddress(ScriptPubKeyType.Legacy, Network.Main);
            BitcoinAddress.Create(publicKey.ToString(), Network.Main);

            return new Wallet { PublicKey = publicKey.ToString(), PrivateKey = privateKey.GetBitcoinSecret(Network.Main).ToString() };
        }
        
        public static string Sign(string privateKey, string message)
        {
            var secret = Network.Main.CreateBitcoinSecret(privateKey);
            var signature = secret.PrivateKey.SignMessage(message);
            secret.PubKey.VerifyMessage(message, signature);

            return signature;
        }
        
        public static bool Verify(string publicKey, string message, string signedMessage)
        {
            var address = BitcoinAddress.Create(publicKey, Network.Main);
            var publicKeyHash = (address as IPubkeyHashUsable);

            return publicKeyHash.VerifyMessage(message, signedMessage);
        }
    }
}
