using Algorand;
using Algorand.Algod.Api;
using Algorand.Client;
using Algorand.V2;
using Algorand.V2.Algod.Model;
using HandlebarsDotNet;
using Newtonsoft.Json;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace ContractTests
{
    [Parallelizable(ParallelScope.Children)]
    public class ContractTests
    {
        private AlgodApi? api;
        private Algorand.V2.Algod.DefaultApi? api2;
        private Algorand.V2.Indexer.SearchApi? indexerSearch;
        private ulong AssetSell = 0;
        private ulong AssetBuy = 0;
        private Algorand.Account Seller;
        private Algorand.Account Buyer;
        [OneTimeSetUp]
        public async Task SetUp()
        {

            string ALGOD_API_ADDR = "http://localhost:4001"; //find in algod.net
            string ALGOD_API_TOKEN = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa"; //find in algod.token
            string INDEXER_API_ADDR = "http://localhost:8980"; //find in algod.net
            string INDEXER_API_TOKEN = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa"; //find in algod.token
            //api = new AlgodApi(ALGOD_API_ADDR, ALGOD_API_TOKEN);

            api = new AlgodApi(ALGOD_API_ADDR, ALGOD_API_TOKEN);

            var algodHttpClient = HttpClientConfigurator.ConfigureHttpClient(ALGOD_API_ADDR, ALGOD_API_TOKEN);

            api2 = new Algorand.V2.Algod.DefaultApi(algodHttpClient)
            {
                BaseUrl = ALGOD_API_ADDR,
            };
            var configuration = JsonConvert.DeserializeObject<Model.Configuration>(File.ReadAllText("appsettings.json"));
            if (configuration == null || string.IsNullOrEmpty(configuration.SandboxAcc1Mnemonic) || string.IsNullOrEmpty(configuration.SandboxAcc2Mnemonic))
            {
                throw new Exception("Please setup appsettings mnemonic phrases according to your sandbox");
            }
            var httpClient = HttpClientConfigurator.ConfigureHttpClient(INDEXER_API_ADDR, INDEXER_API_TOKEN);
            indexerSearch = new Algorand.V2.Indexer.SearchApi(httpClient);
            string SRC_ACCOUNT = configuration.SandboxAcc1Mnemonic;
            Seller = new Algorand.Account(SRC_ACCOUNT);
            string ACC2 = configuration.SandboxAcc2Mnemonic;
            Buyer = new Algorand.Account(ACC2);

            if (configuration.AssetSell.HasValue)
            {
                AssetSell = configuration.AssetSell.Value;
            }
            if (configuration.AssetBuy.HasValue)
            {
                AssetBuy = configuration.AssetBuy.Value;
            }

            if (AssetSell == 0)
            {
                var tasks = new List<Task>();
                tasks.Add(Task.Run(() => IssueBuyAsa()));
                tasks.Add(Task.Run(() => IssueSellAsa()));
                await Task.WhenAll(tasks);
                tasks = new List<Task>();
                tasks.Add(Task.Run(() => TransferAlgo(Seller, Buyer, 1000000, null)));
                tasks.Add(Task.Run(() => TransferAsa(Buyer, Buyer, AssetSell, 0, null)));
                tasks.Add(Task.Run(() => TransferAsa(Buyer, Buyer, AssetBuy, 0, null)));
                await Task.WhenAll(tasks);
                tasks.Add(Task.Run(() => TransferAsa(Seller, Buyer, AssetSell, 1000000, null)));
                tasks.Add(Task.Run(() => TransferAsa(Seller, Buyer, AssetBuy, 1000000, null)));
                await Task.WhenAll(tasks);

            }
            Trace.WriteLine($"Using ASA SELL: {AssetSell}");
            Trace.WriteLine($"Using ASA BUY: {AssetBuy}");
        }
        private async Task IssueBuyAsa()
        {
            var assetBuy = new Algorand.Algod.Model.AssetParams(creator: Seller.Address.EncodeAsString(), assetname: "TestBuy", unitname: "b", total: 100000000);
            AssetBuy = await IssueAsa(assetBuy);
        }
        private async Task IssueSellAsa()
        {
            var assetSell = new Algorand.Algod.Model.AssetParams(creator: Seller.Address.EncodeAsString(), assetname: "TestSell", unitname: "s", total: 100000000);
            AssetSell = await IssueAsa(assetSell);
        }
        private async Task<ulong> IssueAsa(Algorand.Algod.Model.AssetParams asset)
        {
            var transParams = api.TransactionParamsAsync().Result;
            var optin = Algorand.Utils.GetCreateAssetTransaction(asset, transParams, "asset tx message");
            //var optin = Utils.GetCreateAssetTransaction(new Algorand.Algod.Model.AssetParams("Test", total: 1, creator: src.Address.ToString(), unitname: "t"), transParams, decimals: 6);
            var signedTx = Seller.SignTransaction(optin);
            var id = Utils.SubmitTransaction(api, signedTx);

            var wait = Utils.WaitTransactionToComplete(api, id.TxId);

            Trace.WriteLine("Successfully sent tx with id: " + id.TxId);
            Trace.WriteLine(wait);


            var resp = await api.PendingTransactionInformationAsync(id.TxId);
            Trace.WriteLine("WaitTransactionToComplete: " + resp);
            return resp.Txresults.Createdasset.Value;
        }

        private async Task TransferAsa(Algorand.Account from, Algorand.Account to, ulong assetId, ulong amount, Address? closeTo)
        {

            var trans = api.TransactionParamsAsync().Result;
            var optin = Algorand.Utils.GetTransferAssetTransaction(from.Address, to.Address, assetId, amount, trans, closeTo);
            //var optin = Utils.GetCreateAssetTransaction(new Algorand.Algod.Model.AssetParams("Test", total: 1, creator: src.Address.ToString(), unitname: "t"), transParams, decimals: 6);
            var signedTx = from.SignTransaction(optin);
            var id = Utils.SubmitTransaction(api, signedTx);

            var wait = Utils.WaitTransactionToComplete(api, id.TxId);
        }
        private async Task TransferAlgo(Algorand.Account from, Algorand.Account to, ulong amount, Address? closeTo)
        {

            var trans = api.TransactionParamsAsync().Result;
            var tx = Algorand.Utils.GetPaymentTransaction(from.Address, to.Address, amount, "", trans);
            var signedTx = from.SignTransaction(tx);
            var id = Utils.SubmitTransaction(api, signedTx);
            var wait = Utils.WaitTransactionToComplete(api, id.TxId);
        }

        [Test]
        public void AssetExists()
        {
            Assert.IsTrue(AssetBuy > 0);
            Assert.IsTrue(AssetSell > 0);
            Assert.AreNotEqual(AssetSell, AssetBuy);
        }
        [Test]
        public void SellerAddressIsOk()
        {
            Assert.AreEqual("GCOBIVIJIDA7ZWYYP7IBJBRR442PMCCFJF6UZ3CICO6IARZ2JMRKPEGTQQ", Seller.Address.EncodeAsString());
        }
        [Test]
        public void BuyerAddressIsOk()
        {
            Assert.AreEqual("TESTVEMQSHBBJZCBARUBGYGGXIZC6Z47VTT6T3HEBD45GA5RTYD3COHY2M", Buyer.Address.EncodeAsString());
        }

        private string GetContract(ulong price, int multiplier, ulong assetSell, ulong assetBuy, Address seller)
        {
            var file = File.ReadAllText("asa2asa.teal.hbs");
            var template = Handlebars.Compile(file);
            var data = new Dictionary<string, string>();
            data["price"] = price.ToString();
            data["multiplier"] = multiplier.ToString();
            data["assetSell"] = assetSell.ToString();
            data["assetBuy"] = assetBuy.ToString();
            data["seller"] = seller.ToHex();
            return template(data);
        }

        [Test]

        public async Task GetContractTest()
        {
            Assert.AreEqual("#pragma version 5", Contract1().Substring(0, 17));
            Assert.AreEqual("bytecblock 0x", Contract1().Substring(19, 13));
            Assert.AreEqual("bytecblock 0xA0B001", Contract1().Substring(19, 19));
            Assert.AreEqual("0x309C14550940C1FCDB187FD0148631E734F60845497D4CEC4813BC80473A4B22", Contract1().Substring(39, 66));
            CompileResponse? contract = null;
            string hexContract = null;
            try
            {
                using var ms = new MemoryStream(Encoding.UTF8.GetBytes(Contract1()));
                contract = await api2.CompileAsync(ms);
                Assert.IsTrue(ContractIsValidA0B001(Convert.FromBase64String(contract.Result)));

                var bytes = Convert.FromBase64String(contract.Result);
                hexContract = Convert.ToHexString(bytes);

                Assert.IsNotNull(contract);
            }
            catch (Algorand.V2.Algod.Model.ApiException<ErrorResponse> ex)
            {
                Assert.Fail($"{ex.Message} - {ex.Result.Message}");
            }
            catch (Algorand.V2.Algod.Model.ApiException ex)
            {
                Assert.Fail(ex.Message);
            }
            //Assert.AreEqual(
            //    "05260203A0B0012041DEA6E53207AB5FC2A919D7B80A88118CF205E92036DF436DB973BAE0637CC0200B010AE901EA01000102030405904E33002032031244320421051240003D33012032031244320421061240002E3302203203124432042107124000",
            //    hexContract.Substring(0, 200));
            try
            {
                using var ms = new MemoryStream(Encoding.UTF8.GetBytes(Contract2()));
                contract = await api2.CompileAsync(ms);
                Assert.IsTrue(ContractIsValidA0B001(Convert.FromBase64String(contract.Result)));

                var bytes = Convert.FromBase64String(contract.Result);
                hexContract = Convert.ToHexString(bytes);

                Assert.IsNotNull(contract);
            }
            catch (Algorand.V2.Algod.Model.ApiException<ErrorResponse> ex)
            {
                Assert.Fail($"{ex.Message} - {ex.Result.Message}");
            }
            catch (Algorand.V2.Algod.Model.ApiException ex)
            {
                Assert.Fail(ex.Message);
            }
            Trace.WriteLine(hexContract.Substring(0, 200));
            //Assert.AreEqual(
            //    "05260203A0B0012041DEA6E53207AB5FC2A919D7B80A88118CF205E92036DF436DB973BAE0637CC0200B0264E901EA01000102030405904E33002032031244320421051240003D33012032031244320421061240002E3302203203124432042107124000",
            //    hexContract.Substring(0, 200)
            //    );




        }
        public bool ContractIsValidA0B001(byte[] data)
        {
            var hexContract = Convert.ToHexString(data);
            if (!hexContract.StartsWith("05260203A0B001")) return false;
            var minOffset = 90;
            var index = hexContract.Substring(minOffset).IndexOf("0102030405");
            if (index < 0) return false;
            if (index > 20) return false;
            index += minOffset;

            using SHA256 sha = SHA256.Create();
            Trace.WriteLine(index);
            var hash = sha.ComputeHash(Encoding.ASCII.GetBytes(hexContract.Substring(index)));
            var hashStr = Convert.ToHexString(hash);
            Trace.WriteLine(hexContract.Substring(index));
            //Assert.AreEqual("3687250DDFD8BA46F37FCB96978B7B6C566A0A918D9C8B70CD4FA7FA2412F6E2", hashStr);
            //return hashStr == "3687250DDFD8BA46F37FCB96978B7B6C566A0A918D9C8B70CD4FA7FA2412F6E2";
            return true;
        }
        public string Contract1()
        {
            return GetContract(1, 10, AssetSell, AssetBuy, Seller.Address);
        }

        public string Contract2()
        {
            return GetContract(2, 100, AssetSell, AssetBuy, Seller.Address);
        }
        public string Contract3()
        {
            return GetContract(3, 100, AssetSell, AssetBuy, Seller.Address);
        }
        public string Contract4()
        {
            return GetContract(4, 10, AssetSell, AssetBuy, Seller.Address);
        }
        public string ContractWithPrice(ulong price)
        {
            return GetContract(price, 1, AssetSell, AssetBuy, Seller.Address);
        }
        [Test]

        public async Task CreateEscrowAndFirstDepositTest()
        {
            using var ms = new MemoryStream(Encoding.UTF8.GetBytes(Contract1()));
            CompileResponse? contract = null;
            try
            {
                contract = await api2.CompileAsync(ms);
                Assert.IsNotNull(contract);
            }
            catch (Algorand.V2.Algod.Model.ApiException<ErrorResponse> ex)
            {
                Assert.Fail($"{ex.Message} - {ex.Result.Message}");
            }
            catch (Algorand.V2.Algod.Model.ApiException ex)
            {
                Assert.Fail(ex.Message);
            }

            //            Assert.AreEqual("BSAEAQ", contract.Result.Substring(0, 6));

            var logsig = new LogicsigSignature(Convert.FromBase64String(contract.Result));
            var escrow = logsig.Address;
            Trace.WriteLine($"Escrow: {logsig.Address}");

            var txid = await Deposit(logsig, escrow);
            var wait = await Utils.WaitTransactionToComplete(api2, txid);
        }

        private async Task<string> Deposit(LogicsigSignature logsig, Address escrow, ulong amountToSell = 5000)
        {

            var trans = api.TransactionParamsAsync().Result;
            var tx0MinDeposit = Algorand.Utils.GetPaymentTransaction(Seller.Address, escrow, 300000, "", trans);
            var tx1optInSellAsset = Algorand.Utils.GetTransferAssetTransaction(escrow, escrow, AssetSell, 0, trans);
            var tx2optInBuyAsset = Algorand.Utils.GetTransferAssetTransaction(escrow, escrow, AssetBuy, 0, trans);
            var tx3SellAsset = Algorand.Utils.GetTransferAssetTransaction(Seller.Address, escrow, AssetSell, amountToSell, trans);


            tx0MinDeposit.fee = 4000;
            tx1optInSellAsset.fee = 0;
            tx2optInBuyAsset.fee = 0;
            tx3SellAsset.fee = 0;

            Digest gid = TxGroup.ComputeGroupID(new Transaction[] {
                tx0MinDeposit,
                tx1optInSellAsset,
                tx2optInBuyAsset,
                tx3SellAsset
            });

            tx0MinDeposit.AssignGroupID(gid);
            tx1optInSellAsset.AssignGroupID(gid);
            tx2optInBuyAsset.AssignGroupID(gid);
            tx3SellAsset.AssignGroupID(gid);


            var tx0MinDepositSigned = Seller.SignTransaction(tx0MinDeposit);
            var tx1optInSellAssetSigned = Algorand.Account.SignLogicsigTransaction(logsig, tx1optInSellAsset);
            var tx2optInBuyAssetSigned = Algorand.Account.SignLogicsigTransaction(logsig, tx2optInBuyAsset);
            var tx3SellAssetSigned = Seller.SignTransaction(tx3SellAsset);

            try
            {
                var txs = await Utils.SubmitTransactions(api2, new SignedTransaction[]
                {
                    tx0MinDepositSigned,
                    tx1optInSellAssetSigned,
                    tx2optInBuyAssetSigned,
                    tx3SellAssetSigned
                });
                return txs.TxId;


            }
            catch (Algorand.V2.Algod.Model.ApiException<ErrorResponse> ex)
            {
                Assert.Fail($"{ex.Message} - {ex.Result.Message}");
                throw ex;
            }
            catch (Algorand.V2.Algod.Model.ApiException ex)
            {
                Assert.Fail(ex.Message);
                throw ex;
            }
        }



        private async Task<string> Withdraw(LogicsigSignature logsig, Address escrow, ulong amount, Algorand.Account withdrawalAccount)
        {
            var trans = api.TransactionParamsAsync().Result;
            var tx0ProofPrivKeyPayFees = Algorand.Utils.GetPaymentTransaction(withdrawalAccount.Address, escrow, 0, "", trans);
            var tx1WithdrawAsset = Algorand.Utils.GetTransferAssetTransaction(escrow, Seller.Address, AssetSell, amount, trans);

            tx0ProofPrivKeyPayFees.fee = 2000;
            tx1WithdrawAsset.fee = 0;

            Digest gid = TxGroup.ComputeGroupID(new Transaction[] {
                tx0ProofPrivKeyPayFees,
                tx1WithdrawAsset,
            });

            tx0ProofPrivKeyPayFees.AssignGroupID(gid);
            tx1WithdrawAsset.AssignGroupID(gid);


            var tx0ProofPrivKeyPayFeesSigned = withdrawalAccount.SignTransaction(tx0ProofPrivKeyPayFees);
            var tx1WithdrawAssetSigned = Algorand.Account.SignLogicsigTransaction(logsig, tx1WithdrawAsset);

            try
            {
                var txs = await Utils.SubmitTransactions(api2, new SignedTransaction[]
                {
                    tx0ProofPrivKeyPayFeesSigned,
                    tx1WithdrawAssetSigned
                });
                return txs.TxId;


            }
            catch (Algorand.V2.Algod.Model.ApiException<ErrorResponse> ex)
            {
                Trace.WriteLine($"{ex.Message} - {ex.Result.Message}");
                throw ex;
            }
            catch (Algorand.V2.Algod.Model.ApiException ex)
            {
                Trace.WriteLine(ex.Message);
                throw ex;
            }
        }


        private Task<string> Pay(LogicsigSignature logsig, Address escrow, ulong amountAsaSell, ulong amountAsaBuy)
        {
            return Pay(logsig, escrow, amountAsaSell, amountAsaBuy, AssetBuy, AssetSell);
        }


        private async Task<string> Pay(LogicsigSignature logsig, Address escrow, ulong amountAsaSell, ulong amountAsaBuy, ulong AssetBuy, ulong AssetSell)
        {
            var trans = api.TransactionParamsAsync().Result;
            var tx0WithdrawAssetToBuyer = Algorand.Utils.GetTransferAssetTransaction(escrow, Buyer.Address, AssetSell, amountAsaSell, trans);
            var tx1PayToSeller = Algorand.Utils.GetTransferAssetTransaction(Buyer.Address, Seller.Address, AssetBuy, amountAsaBuy, trans);

            tx0WithdrawAssetToBuyer.fee = 0;
            tx1PayToSeller.fee = 2000;

            Digest gid = TxGroup.ComputeGroupID(new Transaction[] {
                tx0WithdrawAssetToBuyer,
                tx1PayToSeller,
            });

            tx0WithdrawAssetToBuyer.AssignGroupID(gid);
            tx1PayToSeller.AssignGroupID(gid);

            var tx0WithdrawAssetToBuyerSigned = Algorand.Account.SignLogicsigTransaction(logsig, tx0WithdrawAssetToBuyer);
            var tx1PayToSellerSigned = Buyer.SignTransaction(tx1PayToSeller);

            try
            {
                var txs = await Utils.SubmitTransactions(api2, new SignedTransaction[]
                {
                    tx0WithdrawAssetToBuyerSigned,
                    tx1PayToSellerSigned
                });
                return txs.TxId;
            }
            catch (Algorand.V2.Algod.Model.ApiException<ErrorResponse> ex)
            {
                Trace.WriteLine($"{ex.Message} - {ex.Result.Message}");
                throw ex;
            }
            catch (Algorand.V2.Algod.Model.ApiException ex)
            {
                Trace.WriteLine(ex.Message);
                throw ex;
            }
        }


        private async Task<string> CancelOrder(LogicsigSignature logsig, Algorand.Account acc, ulong AssetSell)
        {
            var trans = api.TransactionParamsAsync().Result;
            var tx0PaySellerProof = Algorand.Utils.GetPaymentTransaction(acc.Address, acc.Address, 0, "", trans);
            var tx1WithdrawAssets = Algorand.Utils.GetTransferAssetTransaction(logsig.Address, acc.Address, assetId: AssetSell, amount: 0L, trans);
            tx1WithdrawAssets.assetCloseTo = acc.Address;
            var tx2WithdrawAssets = Algorand.Utils.GetTransferAssetTransaction(logsig.Address, acc.Address, assetId: AssetBuy, amount: 0L, trans);
            tx2WithdrawAssets.assetCloseTo = acc.Address;
            var tx3WithdrawAlgo = Algorand.Utils.GetPaymentTransaction(logsig.Address, logsig.Address, 0, "", trans);
            tx3WithdrawAlgo.closeRemainderTo = acc.Address;


            tx0PaySellerProof.fee = 3000;
            tx1WithdrawAssets.fee = 0;
            tx3WithdrawAlgo.fee = 0;

            Digest gid = TxGroup.ComputeGroupID(new Transaction[] {
                tx0PaySellerProof,
                tx1WithdrawAssets,
                tx2WithdrawAssets,
                tx3WithdrawAlgo
            });

            tx0PaySellerProof.AssignGroupID(gid);
            tx1WithdrawAssets.AssignGroupID(gid);
            tx2WithdrawAssets.AssignGroupID(gid);
            tx3WithdrawAlgo.AssignGroupID(gid);

            var tx0PaySellerProofSigned = acc.SignTransaction(tx0PaySellerProof);
            var tx1WithdrawAssetsSigned = Algorand.Account.SignLogicsigTransaction(logsig, tx1WithdrawAssets);
            var tx2WithdrawAssetsSigned = Algorand.Account.SignLogicsigTransaction(logsig, tx2WithdrawAssets);
            var tx3WithdrawAlgoSigned = Algorand.Account.SignLogicsigTransaction(logsig, tx3WithdrawAlgo);


            try
            {
                var txs = await Utils.SubmitTransactions(api2, new SignedTransaction[]
                {
                    tx0PaySellerProofSigned,
                    tx1WithdrawAssetsSigned,
                    tx2WithdrawAssetsSigned,
                    tx3WithdrawAlgoSigned
                });
                return txs.TxId;
            }
            catch (Algorand.V2.Algod.Model.ApiException<ErrorResponse> ex)
            {
                Trace.WriteLine($"{ex.Message} - {ex.Result.Message}");
                throw ex;
            }
            catch (Algorand.V2.Algod.Model.ApiException ex)
            {
                Trace.WriteLine(ex.Message);
                throw ex;
            }
        }

        [Test]
        public async Task DepositMoreToTheEscrow()
        {
            using var ms = new MemoryStream(Encoding.UTF8.GetBytes(Contract2()));
            var contract = await api2.CompileAsync(ms);
            var logsig = new LogicsigSignature(Convert.FromBase64String(contract.Result));
            var escrow = logsig.Address;
            Trace.WriteLine($"DepositMoreToTheEscrow Escrow: {logsig.Address}");
            var txid = await Deposit(logsig, escrow);
            var wait = await Utils.WaitTransactionToComplete(api2, txid);

            var trans = api.TransactionParamsAsync().Result;
            var depositMore = Algorand.Utils.GetTransferAssetTransaction(Seller.Address, escrow, AssetSell, 1000, trans);
            var depositMoreSigned = Seller.SignTransaction(depositMore);

            var id = await Utils.SubmitTransaction(api2, depositMoreSigned);
            var wait2 = await Utils.WaitTransactionToComplete(api2, id.TxId);
            Trace.WriteLine(wait2);
        }

        [Test]
        public async Task WithdrawFromEscrowByOwner()
        {
            using var ms = new MemoryStream(Encoding.UTF8.GetBytes(Contract3()));
            var contract = await api2.CompileAsync(ms);
            var logsig = new LogicsigSignature(Convert.FromBase64String(contract.Result));
            var escrow = logsig.Address;
            Trace.WriteLine($"DepositMoreToTheEscrow Escrow: {logsig.Address}");
            var txid = await Deposit(logsig, escrow, 5000);
            var wait = await Utils.WaitTransactionToComplete(api2, txid);
            Trace.WriteLine(wait);
            txid = await Withdraw(logsig, escrow, 1000, Seller);
            wait = await Utils.WaitTransactionToComplete(api2, txid);
            Trace.WriteLine(wait);
        }
        [Test]
        public async Task IllegalWithdrawFromEscrow()
        {
            using var ms = new MemoryStream(Encoding.UTF8.GetBytes(Contract4()));
            var contract = await api2.CompileAsync(ms);
            var logsig = new LogicsigSignature(Convert.FromBase64String(contract.Result));
            var escrow = logsig.Address;
            Trace.WriteLine($"DepositMoreToTheEscrow Escrow: {logsig.Address}");
            var txid = await Deposit(logsig, escrow, 5000);
            var wait = await Utils.WaitTransactionToComplete(api2, txid);
            Trace.WriteLine(wait);
            try
            {
                txid = await Withdraw(logsig, escrow, 1000, Buyer);
                Assert.Fail("Buyer cannot withdraw from escrow of other person");
            }
            catch (ApiException<ErrorResponse> exc)
            {
                Trace.WriteLine(exc.Result.Message);
                Assert.IsTrue(exc.Result.Message.Contains("rejected by logic"));
            }
            wait = await Utils.WaitTransactionToComplete(api2, txid);
            Trace.WriteLine(wait);
        }


        [Test]
        public async Task PayCorrectAmount()
        {
            using var ms = new MemoryStream(Encoding.UTF8.GetBytes(ContractWithPrice(123)));
            var contract = await api2.CompileAsync(ms);
            var logsig = new LogicsigSignature(Convert.FromBase64String(contract.Result));
            var escrow = logsig.Address;
            Trace.WriteLine($"DepositMoreToTheEscrow Escrow: {logsig.Address}");
            var txid = await Deposit(logsig, escrow, 1000);
            var wait = await Utils.WaitTransactionToComplete(api2, txid);
            Trace.WriteLine(wait);
            try
            {
                txid = await Pay(logsig, escrow, 100, 12300);
                wait = await Utils.WaitTransactionToComplete(api2, txid);
                Trace.WriteLine(wait);
            }
            catch (ApiException<ErrorResponse> exc)
            {
                Trace.WriteLine(exc.Result.Message);
                Assert.Fail(exc.Result.Message);
            }
        }


        [Test]
        public async Task PayWrongAssetAmount()
        {
            using var ms = new MemoryStream(Encoding.UTF8.GetBytes(ContractWithPrice(12)));
            var contract = await api2.CompileAsync(ms);
            var logsig = new LogicsigSignature(Convert.FromBase64String(contract.Result));
            var escrow = logsig.Address;
            Trace.WriteLine($"DepositMoreToTheEscrow Escrow: {logsig.Address}");
            var txid = await Deposit(logsig, escrow, 1000);
            var wait = await Utils.WaitTransactionToComplete(api2, txid);
            Trace.WriteLine(wait);
            try
            {
                txid = await Pay(logsig, escrow, 100, 1200, AssetBuy, AssetBuy);
                Assert.Fail("User cannot pay with asset which he received");
            }
            catch (ApiException<ErrorResponse> exc)
            {
                Trace.WriteLine(exc.Result.Message);
                Assert.IsTrue(exc.Result.Message.Contains("rejected by logic"));
            }
            catch (Exception exc)
            {
                Assert.Fail("No other exception is allowed", exc);
            }
        }

        [Test]
        public async Task CreateOrderCancelOrderTest()
        {
            using var ms = new MemoryStream(Encoding.UTF8.GetBytes(ContractWithPrice(13)));
            var contract = await api2.CompileAsync(ms);
            var logsig = new LogicsigSignature(Convert.FromBase64String(contract.Result));
            var escrow = logsig.Address;
            Trace.WriteLine($"DepositMoreToTheEscrow Escrow: {logsig.Address}");
            var txid = await Deposit(logsig, escrow, 1);
            var wait = await Utils.WaitTransactionToComplete(api2, txid);
            var txid2 = await CancelOrder(logsig, Seller, AssetSell);
            Trace.WriteLine(wait);
        }
    }
}