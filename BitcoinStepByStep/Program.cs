
using System;
using System.Linq;

namespace BitcoinStepByStep
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello BitCoin Step By Step :) \r\n");

            ShowMenu();
        }

        static void ShowMenu()
        {
            Console.WriteLine("");

            Console.WriteLine("1) Create Your Own Wallet");
            Console.WriteLine("2) Restore Your Wallet");
            Console.WriteLine("3) Check Wallet : mvJh993ZpnVFgX4Bs9jacjFLsKPhXj3nkj Balance");
            Console.WriteLine("4) Check Wallet : mvJh993ZpnVFgX4Bs9jacjFLsKPhXj3nkj Receive");
            Console.WriteLine("5) Start Transaction : mvJh993ZpnVFgX4Bs9jacjFLsKPhXj3nkj (0.01 BTC)=> mshr22VWpq7XTTA3EhAoqoizPuqRAvZfvi");
            Console.WriteLine("6) Check Wallet : mvJh993ZpnVFgX4Bs9jacjFLsKPhXj3nkj Send Records");
            Console.WriteLine("e) Exit");

            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("");
            Console.Write("Choose Command \\>");

            var res = Console.ReadLine();

            switch (res.Trim())
            {
                case "1":
                    CreateWallet();
                    break;
                case "2":
                    StoreWallet();
                    break;
                case "3":
                    CheckAmountWalletA();
                    break;
                case "4":
                    CheckWalletAllReceiveCoin("mvJh993ZpnVFgX4Bs9jacjFLsKPhXj3nkj");
                    break;
                case "5":
                    StartTransation();
                    break;
                case "6":
                    SendRecords("mvJh993ZpnVFgX4Bs9jacjFLsKPhXj3nkj");
                    break;
                case "e":
                    Environment.Exit(-1);
                    break;
                default:
                    Console.WriteLine("\r\nPlease Select Command..\r\n");

                    break;
            }

            ShowMenu();

        }

        private static void SendRecords(string walletAddress) {
            QBitNinja.Client.QBitNinjaClient client = new QBitNinja.Client.QBitNinjaClient(NBitcoin.Network.TestNet);
            var balance = client.GetBalance(new NBitcoin.BitcoinPubKeyAddress(walletAddress), false).Result;

            foreach (var operation in balance.Operations)
            {
                var transSum = operation.SpentCoins.Select(coin => coin as NBitcoin.Coin).Sum(x => x.Amount.ToDecimal(NBitcoin.MoneyUnit.BTC));

                if (transSum > 0) {
                    Console.WriteLine(operation.TransactionId + ":" + transSum);
                }
               

            }
        }

        private static void StartTransation()
        {

            //還原主錢包

            var walletA = NBitcoin.Key.Parse("6PYQBZhumqnrhETBXNvqW61XHpwYVefbTmZtM2BZcekPjunUbPgUPGe8H2", "your_pass_word", NBitcoin.Network.TestNet).GetWif(NBitcoin.Network.TestNet);


            //準備被接收端的錢包(目的地)
            var reveiveAddress = NBitcoin.BitcoinAddress.Create("mshr22VWpq7XTTA3EhAoqoizPuqRAvZfvi", NBitcoin.Network.TestNet);

            var tx = NBitcoin.Transaction.Create(NBitcoin.Network.TestNet);

            var input = new NBitcoin.TxIn();

            // 帶入來源端的 Trasaction Id 還有 Index 
            // Source from : https://no2don.blogspot.com/2019/02/c-bitcoin_96.html
            // https://live.blockcypher.com/btc-testnet/tx/d3425d5f912552a47358df8b6647330e914019b1745c88b89d376896d35864e5/
            input.PrevOut = new NBitcoin.OutPoint(new NBitcoin.uint256("d3425d5f912552a47358df8b6647330e914019b1745c88b89d376896d35864e5"), 0);

            input.ScriptSig = walletA.GetAddress().ScriptPubKey;
            tx.Inputs.Add(input);

            var output = new NBitcoin.TxOut();
            //這是 GAS Fee 
            var gasFee = NBitcoin.Money.Coins(0.0001M);
            output.Value = NBitcoin.Money.Coins(0.01M) - gasFee;
            //設定轉出到指定的錢包至 Output
            output.ScriptPubKey = reveiveAddress.ScriptPubKey;
            tx.Outputs.Add(output);


            tx.Sign(walletA.PrivateKey, false);


            Console.WriteLine("========== TXINO ===========");
            Console.WriteLine(tx.ToString());


            var txBuilder = NBitcoin.Network.TestNet.CreateTransactionBuilder();
            var res = txBuilder.Verify(tx);


            //對一個節點傳送交易 ，使其進行廣播

            var node = NBitcoin.Protocol.Node.Connect(NBitcoin.Network.TestNet, "testnet-seed.bitcoin.jonasschnelli.ch");
            node.VersionHandshake();
            node.SendMessage(new NBitcoin.Protocol.InvPayload(tx));
            node.SendMessage(new NBitcoin.Protocol.TxPayload(tx));

            System.Threading.Thread.Sleep(2000);
            node.Disconnect();


            Console.WriteLine("========== NODE TX ===========");
            Console.WriteLine("TXID:" + tx.GetHash().ToString());


        }


        private static void CreateWallet()
        {

            var Key1 = new NBitcoin.Key();
            var bitcoinPrivateKey = Key1.GetWif(NBitcoin.Network.TestNet);
            NBitcoin.BitcoinEncryptedSecret encryptedBitcoinPrivateKey = bitcoinPrivateKey.Encrypt("your_pass_word");

            Console.WriteLine("Your Address : " + bitcoinPrivateKey.GetAddress());
            Console.WriteLine("Encrypted PrivateKey : " + bitcoinPrivateKey.ScriptPubKey.ToString());

            //Result : 

            //Wallet A
            //Your Address : mvJh993ZpnVFgX4Bs9jacjFLsKPhXj3nkj
            //Encrypted PrivateKey: 6PYQBZhumqnrhETBXNvqW61XHpwYVefbTmZtM2BZcekPjunUbPgUPGe8H2

            //Wallet B
            //Your Address : mshr22VWpq7XTTA3EhAoqoizPuqRAvZfvi
            //Encrypted PrivateKey: 6PYTsdNrmWyNTBJVMd7LqXnAM7ApFkTtG1GMA5RdqczYW8kuAaTwbvJENw

        }

        private static void CheckWalletAllReceiveCoin(string walletAddress)
        {

            QBitNinja.Client.QBitNinjaClient client = new QBitNinja.Client.QBitNinjaClient(NBitcoin.Network.TestNet);
            var balance = client.GetBalance(new NBitcoin.BitcoinPubKeyAddress(walletAddress), false).Result;

            Console.WriteLine("");
            Console.WriteLine("TransactionId                                                    : BitCoin , Index");
            Console.WriteLine("-------------------------------------------");

            foreach (var operation in balance.Operations)
            {
                Console.Write(operation.TransactionId + " : " + operation.ReceivedCoins.Select(coin => coin as NBitcoin.Coin).Sum(x => x.Amount.ToDecimal(NBitcoin.MoneyUnit.BTC)) + ",");


                //To highline INDEX so chnage color .
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine(string.Join(";", operation.ReceivedCoins.Select(coin => coin as NBitcoin.Coin).Select(x => x.Outpoint.N)));
                Console.ForegroundColor = ConsoleColor.White;
            }
        }


        private static void StoreWallet()
        {

            var walletA = NBitcoin.Key.Parse("6PYQBZhumqnrhETBXNvqW61XHpwYVefbTmZtM2BZcekPjunUbPgUPGe8H2", "your_pass_word", NBitcoin.Network.TestNet).GetWif(NBitcoin.Network.TestNet);

            Console.WriteLine("Your Address : " + walletA.GetAddress());


        }



        private static void CheckAmountWalletA()
        {

            decimal confirmsBalance = 0;
            decimal unConfirmsBalance = 0;

            GetWalletBalance("mvJh993ZpnVFgX4Bs9jacjFLsKPhXj3nkj", out confirmsBalance, out unConfirmsBalance);

            Console.WriteLine("Wallet A : " + "mvJh993ZpnVFgX4Bs9jacjFLsKPhXj3nkj");
            Console.WriteLine("Confirm Balance : " + confirmsBalance);
            Console.WriteLine("UnConfirm Balance : " + unConfirmsBalance);
        }



        /// <summary>
        /// 取的該錢包有多少餘額
        /// </summary>
        /// <param name="ssAddress">錢包地址</param>
        /// <param name="confirmBalance">已確認的</param>
        /// <param name="unConfirmBalance">未確認的</param>
        public static void GetWalletBalance(string ssAddress, out decimal confirmBalance, out decimal unConfirmBalance)
        {
            //幾個確認判斷為已經確認完成
            //預設你可以設定為 5 
            //但是因為測試所以>0 , 我就判斷已經當作確認過了
            var confirmThres = 4;



            QBitNinja.Client.QBitNinjaClient client = new QBitNinja.Client.QBitNinjaClient(NBitcoin.Network.TestNet);
            var balance = client.GetBalance(new NBitcoin.BitcoinPubKeyAddress(ssAddress), false).Result;

            confirmBalance = 0;
            unConfirmBalance = 0;
            if (balance.Operations.Count > 0)
            {
                var unspentCoins = new System.Collections.Generic.List<NBitcoin.Coin>();
                var unspentCoinsConfirmed = new System.Collections.Generic.List<NBitcoin.Coin>();
                foreach (var operation in balance.Operations)
                {
                    unspentCoins.AddRange(operation.ReceivedCoins.Select(coin => coin as NBitcoin.Coin));
                    if (operation.Confirmations > confirmThres)
                    {
                        unspentCoinsConfirmed.AddRange(operation.ReceivedCoins.Select(coin => coin as NBitcoin.Coin));

                    }
                    unConfirmBalance = unspentCoins.Sum(x => x.Amount.ToDecimal(NBitcoin.MoneyUnit.BTC));
                    confirmBalance = unspentCoinsConfirmed.Sum(x => x.Amount.ToDecimal(NBitcoin.MoneyUnit.BTC));
                }

            }
        }



    }
}
