using Newtonsoft.Json;
using RSA;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Blockchain.Models
{
    public class Cryptocurrency
    {
        private List<Transaction> currentTransactions = new List<Transaction>();
        private List<Block> chain = new List<Block>();
        private List<Node> nodes = new List<Node>();
        private Block lastBlock => chain.Last();
        
        public string NodeId { get; private set; }
        static int blockLength = 0;
        static decimal reward = 100;

        static string minderPrivateKey = "";
        static Wallet minderWallet = RSA.RSA.GenerateKey();
        
        public Cryptocurrency()
        {
            minderPrivateKey = minderWallet.PrivateKey;
            NodeId = minderWallet.PublicKey;

            var transaction = new Transaction { Sender = "0", Recipient = NodeId, Amount = 100, Fees = 0, Signature = "" };
            currentTransactions.Add(transaction);

            CreateNewBlock(proof: 100, previousHash: "1");
        }

        private void RegisterNode(string address)
        {
            nodes.Add(new Node { Address = new Uri(address) });
        }

        private Block CreateNewBlock(int proof, string previousHash = null)
        {
            var block = new Block
            {
                Index = chain.Count,
                Timestamp = DateTime.UtcNow,
                Transactions = currentTransactions.ToList(),
                Proof = proof,
                PreviousHash = previousHash ?? GetHash(chain.Last())
            };

            currentTransactions.Clear();
            chain.Add(block);
            return block;
        }

        private string GetHash(Block block)
        {
            string blockText = JsonConvert.SerializeObject(block);
            return GetSha256(blockText);
        }

        private string GetSha256(string data)
        {
            var sha256 = new SHA256Managed();
            var hashBuilder = new StringBuilder();
            byte[] bytes = Encoding.Unicode.GetBytes(data);
            byte[] hash = sha256.ComputeHash(bytes);

            foreach (byte x in hash)
                hashBuilder.Append($"{x:x2}");

            return hashBuilder.ToString();
        }

        private int CreatePOW(string previousHash)
        {
            int proof = 0;
            while (!IsValidProof(currentTransactions, proof, previousHash))
                proof++;

            if(blockLength == 10)
            {
                blockLength = 0;
                reward = reward / 2;
            }

            var transaction = new Transaction { Sender = "0", Recipient = NodeId, Amount = reward, Fees = 0, Signature = "" };
            currentTransactions.Add(transaction);
            blockLength++;

            return proof;
        }

        private bool IsValidProof(List<Transaction> transactions, int proof, string previousHash)
        {
            var signatures = transactions.Select(x => x.Signature).ToArray();
            string guess = $"{signatures}{proof}{previousHash}";
            string result = GetSha256(guess);

            return result.StartsWith("00");
        }

        public bool VerifyTransactionSignature(Transaction transaction, string signedMessage, string publicKey)
        {
            string message = transaction.ToString();
            bool verified = RSA.RSA.Verify(publicKey, message, signedMessage);

            return verified;
        }

        private List<Transaction> TransationByAddress(string address)
        {
            List<Transaction> transactions = new List<Transaction>();
            foreach(var block in chain.OrderByDescending(x => x.Index))
            {
                var t = block.Transactions.Where(x => x.Sender == address || x.Recipient == address);
                transactions.AddRange(t);
            }

            return transactions;
        }

        public bool HasBalance(Transaction transaction)
        {
            var t = TransationByAddress(transaction.Sender);
            decimal balance = 0;
            foreach(var item in t)
            {
                if (item.Recipient == transaction.Sender)
                    balance += item.Amount;
                else
                    balance -= item.Amount;
            }

            return balance >= (transaction.Amount + transaction.Fees);
        }

        private void AddTransaction(Transaction transaction)
        {
            currentTransactions.Add(transaction);

            if (transaction.Sender != NodeId)
                currentTransactions.Add(new Transaction
                {
                    Sender = transaction.Sender,
                    Recipient = NodeId,
                    Amount = transaction.Fees,
                    Signature = "",
                    Fees = 0
                });
        }

        internal Block Mine()
        {
            int proof = CreatePOW(lastBlock.PreviousHash);
            Block block = CreateNewBlock(proof);

            return block;
        }

        internal string GetChain()
        {
            var response = new
            {
                Chain = chain.ToArray(),
                length = chain.Count
            };

            return JsonConvert.SerializeObject(response);
        }

        internal string RegisterNodes(string[] nodes)
        {
            var builder = new StringBuilder();
            foreach(string node in nodes)
            {
                string url = node;
                RegisterNode(url);
                builder.Append($"{url}, ");
            }

            builder.Insert(0, $"{nodes.Count()} new nodes have been added: ");
            string result = builder.ToString();

            return result.Substring(0, result.Length - 2);
        }

        internal Object Consensus()
        {
            bool replaced = ResolveConflicts();
            string message = replaced ? "was replaced" : "is authoritive";

            var response = new
            {
                message = $"Our chain {message}",
                chain
            };

            return response;
        }

        internal Object CreateTransaction(Transaction transaction)
        {
            var response = new Object();
            var verifed = VerifyTransactionSignature(transaction, transaction.Signature, transaction.Sender);

            if(!verifed || transaction.Sender == transaction.Recipient)
            {
                response = new { message = $"Invalid transaction" };
                return response;
            }

            if(!HasBalance(transaction))
            {
                response = new { message = $"Insufficient balance" };
                return response;
            }

            AddTransaction(transaction);

            var blockIndex = lastBlock != null ? lastBlock.Index + 1 : 0;

            response = new { message = $"Transaction will be added to block {blockIndex}" };
            return response;
        }

        internal List<Transaction> GetTransactions()
        {
            return currentTransactions;
        }

        internal List<Block> GetBlocks()
        {
            return chain;
        }

        internal List<Node> GetNodes()
        {
            return nodes;
        }

        internal Wallet GetMinderWaller()
        {
            return minderWallet;
        }

        private bool ResolveConflicts()
        {
            List<Block> newChain = null;
            int maxLength = chain.Count;

            foreach(Node node in nodes)
            {
                var url = new Uri(node.Address, "/chain");
                var request = (HttpWebRequest)WebRequest.Create(url);
                var response = (HttpWebResponse)request.GetResponse();

                if(response.StatusCode == HttpStatusCode.OK)
                {
                    var model = new
                    {
                        chain = new List<Block>(),
                        length = 0
                    };

                    string json = new StreamReader(response.GetResponseStream()).ReadToEnd();
                    var data = JsonConvert.DeserializeAnonymousType(json, model);

                    if(data.chain.Count > chain.Count && IsValidChain(data.chain))
                    {
                        maxLength = data.chain.Count;
                        newChain = data.chain;
                    }
                }
            }

            if (newChain != null)
            {
                chain = newChain;
                return true;
            }

            return false;
        }

        private bool IsValidChain(List<Block> chain)
        {
            Block block = null;
            Block lastBlock = chain.First();
            int currentIndex = 1;

            while(currentIndex < chain.Count)
            {
                block = chain.ElementAt(currentIndex);

                if (block.PreviousHash != GetHash(lastBlock))
                    return false;
                if (!IsValidProof(block.Transactions, block.Proof, lastBlock.PreviousHash))
                    return false;

                lastBlock = block;
                currentIndex++;
            }

            return true;
        }
    }
}
