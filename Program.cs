using NBitcoin;
using Nethereum.HdWallet;
using Nethereum.JsonRpc.Client;
using Nethereum.Web3;
using Newtonsoft.Json;
using System.Text;

namespace checkweb3api
{
    internal class Program
    {
        // Biến để cache dữ liệu
        private static List<string> cachedData = new List<string>();
        private static HashSet<string> addData = new HashSet<string>();
        private static Web3 web3 = new Web3("https://bsc-dataseed.binance.org/");
        static readonly HttpClient client = new HttpClient();
        static async Task Main(string[] args)
        {

            string currentDirectory = Environment.CurrentDirectory;
            string projectRootDirectory1 = Directory.GetParent(Directory.GetParent(Directory.GetParent(currentDirectory).FullName).FullName).FullName;
            string filePath1 = Path.Combine(currentDirectory, "words_alpha.txt");
            // string filePath1 = Path.Combine(projectRootDirectory1, "words_alpha.txt");
            // string filePath2 = Path.Combine(projectRootDirectory1, "eth-list-address.txt");
            string filePath2 = Path.Combine(currentDirectory, "eth-list-address.txt");
            Console.WriteLine(filePath2);
            List<string> data = await GetDataAsync(filePath1);
            List<string> rd = new List<string>();

            string mnemonicWords = "";
            int count = 0;
            int seedNum = 12;

            Random random = new Random();
            while (true)
            {

                rd = new List<string>();
                var listRd = new List<int>();
                mnemonicWords = string.Empty;
                for (int i = 0; i < seedNum; i++)
                {
                    bool b = true;
                    while (b)
                    {
                        int randomIndex = random.Next(2048);
                        var check = listRd.Where(x => x == randomIndex);
                        if ((check == null || !check.Any()))
                        {
                            rd.Add(randomIndex.ToString());
                            listRd.Add(randomIndex);
                            mnemonicWords = mnemonicWords + " " + data[randomIndex];
                            b = false;
                        }
                    }

                }
                mnemonicWords = mnemonicWords.Trim();
                if (!(!string.IsNullOrEmpty(mnemonicWords) && (mnemonicWords.Split(" ").Length == 12 || mnemonicWords.Split(" ").Length == 24))) continue;
                try
                {
                    count++;
                    var listAddress = new List<string>();

                    // Tạo một ví mới từ seed
                    Wallet wallet = new Wallet(mnemonicWords, null);
                    string accountAddress44 = wallet.GetAccount(0).Address;
                    if (!string.IsNullOrEmpty(accountAddress44))
                    {
                        listAddress.Add(accountAddress44);
                    }
                    Console.WriteLine($"[{count}]-{accountAddress44}");
                    // Tạo và kiểm tra các loại địa chỉ khác nhau
                    Task t1 = DeriveAndCheckBalance(accountAddress44, filePath2, mnemonicWords);
                    Task t2 = DeriveAndCheckBalanceBNB(accountAddress44, filePath2, mnemonicWords);
                    Task t3 = DeriveAndCheckBalancePolygon(accountAddress44, filePath2, mnemonicWords);
                    Task t4 = DeriveAndCheckBalanceAvax(accountAddress44, filePath2, mnemonicWords);
                    Task t5 = DeriveAndCheckBalanceFantom(accountAddress44, filePath2, mnemonicWords);
                    Task t6 = DeriveAndCheckBalanceOp(accountAddress44, filePath2, mnemonicWords);
                    await Task.WhenAll(t1, t2, t3, t4, t5, t6).ConfigureAwait(false);
                    //_ = Task.Run(async () =>
                    //{
                    //    await DeriveAndCheckBalance(accountAddress44, filePath2, mnemonicWords).ConfigureAwait(false);
                    //});
                }
                catch (Exception e)
                {
                    Console.WriteLine("\nException Caught!");
                    Console.WriteLine("Message :{0} ", e.Message);
                }
            }


        }

        public class KKK
        {
            public string JsonRpc { get; set; }
            public int Id { get; set; }
            public string Method { get; set; }
            public string Params { get; set; }
            public string Result { get; set; }
        }


        static async Task DeriveAndCheckBalance(string listAddress, string csvFilePath, string mnemonicWords)
        {
            try
            {
                Console.WriteLine("check eth !");
                // Dữ liệu bạn muốn gửi trong body của POST request
                string postData = "{\"jsonrpc\":\"2.0\",\"id\":0,\"method\":\"eth_getBalance\",\"params\":[\"" + listAddress + "\",\"latest\"]}";
                try
                {
                    HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "https://eth-mainnet.blastapi.io/62582083-2ed3-494d-9f28-981c8ba74b6a")
                    {
                        Content = new StringContent(postData, Encoding.UTF8, "application/json")
                    };
                    HttpResponseMessage response = await client.SendAsync(request);
                    response.EnsureSuccessStatusCode(); // Đảm bảo rằng phản hồi là mã trạng thái thành công
                    string responseBody = await response.Content.ReadAsStringAsync();

                    var result = JsonConvert.DeserializeObject<KKK>(responseBody); // Giả sử bạn có một hàm để làm điều này
                    if (result.Result != "0x0")
                    {
                        string output = $"12 Seed: {mnemonicWords} | address:{String.Join(", ", listAddress)}";

                        string currentDirectory = Environment.CurrentDirectory;
                        string filePath = Path.Combine(currentDirectory, "btc-wallet.txt");

                        await using (StreamWriter sw = File.AppendText(filePath))
                        {
                            await sw.WriteLineAsync(output);
                        }
                        Console.WriteLine($"Thông tin đã được ghi vào file cho địa chỉ: {String.Join(", ", listAddress)}");
                    }
                }
                catch (HttpRequestException e)
                {
                    Console.WriteLine("\nException Caught!");
                    Console.WriteLine("Message :{0} ", e.Message);
                }
















                //if (getBalanceTask > 0)
                //{
                //    string output = $"12 Seed: {mnemonicWords} | address:{String.Join(", ", listAddress)}";

                //    string currentDirectory = Environment.CurrentDirectory;
                //    string projectRootDirectory = Directory.GetParent(Directory.GetParent(Directory.GetParent(currentDirectory).FullName).FullName).FullName;
                //    string filePath = Path.Combine(projectRootDirectory, "btc-wallet.txt");

                //    await using (StreamWriter sw = File.AppendText(filePath))
                //    {
                //        await sw.WriteLineAsync(output);
                //    }
                //    Console.WriteLine($"Thông tin đã được ghi vào file cho địa chỉ: {String.Join(", ", listAddress)}");
                //}
            }
            catch (Exception e)
            {
                Console.WriteLine("\nException Caught!");
                Console.WriteLine("Message :{0} ", e.Message);
            }
        }

        static async Task DeriveAndCheckBalanceBNB(string listAddress, string csvFilePath, string mnemonicWords)
        {
            try
            {
                Console.WriteLine("check bnb !");
                // Dữ liệu bạn muốn gửi trong body của POST request
                string postData = "{\"jsonrpc\":\"2.0\",\"id\":0,\"method\":\"eth_getBalance\",\"params\":[\"" + listAddress + "\",\"latest\"]}";
                try
                {
                    HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "https://bsc-mainnet.blastapi.io/62582083-2ed3-494d-9f28-981c8ba74b6a")
                    {
                        Content = new StringContent(postData, Encoding.UTF8, "application/json")
                    };
                    HttpResponseMessage response = await client.SendAsync(request);
                    response.EnsureSuccessStatusCode(); // Đảm bảo rằng phản hồi là mã trạng thái thành công
                    string responseBody = await response.Content.ReadAsStringAsync();

                    var result = JsonConvert.DeserializeObject<KKK>(responseBody); // Giả sử bạn có một hàm để làm điều này
                    if (result.Result != "0x0")
                    {
                        string output = $"12 Seed: {mnemonicWords} | address:{String.Join(", ", listAddress)}";

                        string currentDirectory = Environment.CurrentDirectory;
                        string filePath = Path.Combine(currentDirectory, "btc-wallet.txt");

                        await using (StreamWriter sw = File.AppendText(filePath))
                        {
                            await sw.WriteLineAsync(output);
                        }
                        Console.WriteLine($"Thông tin đã được ghi vào file cho địa chỉ: {String.Join(", ", listAddress)}");
                    }
                }
                catch (HttpRequestException e)
                {
                    Console.WriteLine("\nException Caught!");
                    Console.WriteLine("Message :{0} ", e.Message);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("\nException Caught!");
                Console.WriteLine("Message :{0} ", e.Message);
            }
        }

        static async Task DeriveAndCheckBalancePolygon(string listAddress, string csvFilePath, string mnemonicWords)
        {
            try
            {
                Console.WriteLine("check Polygon !");
                // Dữ liệu bạn muốn gửi trong body của POST request
                string postData = "{\"jsonrpc\":\"2.0\",\"id\":0,\"method\":\"eth_getBalance\",\"params\":[\"" + listAddress + "\",\"latest\"]}";
                try
                {
                    HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "https://polygon-mainnet.blastapi.io/62582083-2ed3-494d-9f28-981c8ba74b6a")
                    {
                        Content = new StringContent(postData, Encoding.UTF8, "application/json")
                    };
                    HttpResponseMessage response = await client.SendAsync(request);
                    response.EnsureSuccessStatusCode(); // Đảm bảo rằng phản hồi là mã trạng thái thành công
                    string responseBody = await response.Content.ReadAsStringAsync();

                    var result = JsonConvert.DeserializeObject<KKK>(responseBody); // Giả sử bạn có một hàm để làm điều này
                    if (result.Result != "0x0")
                    {
                        string output = $"12 Seed: {mnemonicWords} | address:{String.Join(", ", listAddress)}";

                        string currentDirectory = Environment.CurrentDirectory;
                        string filePath = Path.Combine(currentDirectory, "btc-wallet.txt");

                        await using (StreamWriter sw = File.AppendText(filePath))
                        {
                            await sw.WriteLineAsync(output);
                        }
                        Console.WriteLine($"Thông tin đã được ghi vào file cho địa chỉ: {String.Join(", ", listAddress)}");
                    }
                }
                catch (HttpRequestException e)
                {
                    Console.WriteLine("\nException Caught!");
                    Console.WriteLine("Message :{0} ", e.Message);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("\nException Caught!");
                Console.WriteLine("Message :{0} ", e.Message);
            }
        }
        static async Task DeriveAndCheckBalanceAvax(string listAddress, string csvFilePath, string mnemonicWords)
        {
            try
            {
                Console.WriteLine("check Avax !");
                // Dữ liệu bạn muốn gửi trong body của POST request
                string postData = "{\"jsonrpc\":\"2.0\",\"id\":0,\"method\":\"eth_getBalance\",\"params\":[\"" + listAddress + "\",\"latest\"]}";
                try
                {
                    HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "https://ava-mainnet.blastapi.io/62582083-2ed3-494d-9f28-981c8ba74b6a/ext/bc/C/rpc")
                    {
                        Content = new StringContent(postData, Encoding.UTF8, "application/json")
                    };
                    HttpResponseMessage response = await client.SendAsync(request);
                    response.EnsureSuccessStatusCode(); // Đảm bảo rằng phản hồi là mã trạng thái thành công
                    string responseBody = await response.Content.ReadAsStringAsync();

                    var result = JsonConvert.DeserializeObject<KKK>(responseBody); // Giả sử bạn có một hàm để làm điều này
                    if (result.Result != "0x0")
                    {
                        string output = $"12 Seed: {mnemonicWords} | address:{String.Join(", ", listAddress)}";

                        string currentDirectory = Environment.CurrentDirectory;
                        string filePath = Path.Combine(currentDirectory, "btc-wallet.txt");

                        await using (StreamWriter sw = File.AppendText(filePath))
                        {
                            await sw.WriteLineAsync(output);
                        }
                        Console.WriteLine($"Thông tin đã được ghi vào file cho địa chỉ: {String.Join(", ", listAddress)}");
                    }
                }
                catch (HttpRequestException e)
                {
                    Console.WriteLine("\nException Caught!");
                    Console.WriteLine("Message :{0} ", e.Message);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("\nException Caught!");
                Console.WriteLine("Message :{0} ", e.Message);
            }
        }

        static async Task DeriveAndCheckBalanceFantom(string listAddress, string csvFilePath, string mnemonicWords)
        {
            try
            {
                Console.WriteLine("check Fantom !");
                // Dữ liệu bạn muốn gửi trong body của POST request
                string postData = "{\"jsonrpc\":\"2.0\",\"id\":0,\"method\":\"eth_getBalance\",\"params\":[\"" + listAddress + "\",\"latest\"]}";
                try
                {
                    HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "https://fantom-mainnet.blastapi.io/62582083-2ed3-494d-9f28-981c8ba74b6a")
                    {
                        Content = new StringContent(postData, Encoding.UTF8, "application/json")
                    };
                    HttpResponseMessage response = await client.SendAsync(request);
                    response.EnsureSuccessStatusCode(); // Đảm bảo rằng phản hồi là mã trạng thái thành công
                    string responseBody = await response.Content.ReadAsStringAsync();

                    var result = JsonConvert.DeserializeObject<KKK>(responseBody); // Giả sử bạn có một hàm để làm điều này
                    if (result.Result != "0x0")
                    {
                        string output = $"12 Seed: {mnemonicWords} | address:{String.Join(", ", listAddress)}";

                        string currentDirectory = Environment.CurrentDirectory;
                        string filePath = Path.Combine(currentDirectory, "btc-wallet.txt");

                        await using (StreamWriter sw = File.AppendText(filePath))
                        {
                            await sw.WriteLineAsync(output);
                        }
                        Console.WriteLine($"Thông tin đã được ghi vào file cho địa chỉ: {String.Join(", ", listAddress)}");
                    }
                }
                catch (HttpRequestException e)
                {
                    Console.WriteLine("\nException Caught!");
                    Console.WriteLine("Message :{0} ", e.Message);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("\nException Caught!");
                Console.WriteLine("Message :{0} ", e.Message);
            }
        }


        static async Task DeriveAndCheckBalanceOp(string listAddress, string csvFilePath, string mnemonicWords)
        {
            try
            {
                Console.WriteLine("check Op !");
                // Dữ liệu bạn muốn gửi trong body của POST request
                string postData = "{\"jsonrpc\":\"2.0\",\"id\":0,\"method\":\"eth_getBalance\",\"params\":[\"" + listAddress + "\",\"latest\"]}";
                try
                {
                    HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "https://optimism-mainnet.blastapi.io/62582083-2ed3-494d-9f28-981c8ba74b6a")
                    {
                        Content = new StringContent(postData, Encoding.UTF8, "application/json")
                    };
                    HttpResponseMessage response = await client.SendAsync(request);
                    response.EnsureSuccessStatusCode(); // Đảm bảo rằng phản hồi là mã trạng thái thành công
                    string responseBody = await response.Content.ReadAsStringAsync();

                    var result = JsonConvert.DeserializeObject<KKK>(responseBody); // Giả sử bạn có một hàm để làm điều này
                    if (result.Result != "0x0")
                    {
                        string output = $"12 Seed: {mnemonicWords} | address:{String.Join(", ", listAddress)}";

                        string currentDirectory = Environment.CurrentDirectory;
                        string filePath = Path.Combine(currentDirectory, "btc-wallet.txt");

                        await using (StreamWriter sw = File.AppendText(filePath))
                        {
                            await sw.WriteLineAsync(output);
                        }
                        Console.WriteLine($"Thông tin đã được ghi vào file cho địa chỉ: {String.Join(", ", listAddress)}");
                    }
                }
                catch (HttpRequestException e)
                {
                    Console.WriteLine("\nException Caught!");
                    Console.WriteLine("Message :{0} ", e.Message);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("\nException Caught!");
                Console.WriteLine("Message :{0} ", e.Message);
            }
        }
        static async Task<List<string>> GetDataAsync(string filePath)
        {
            // Nếu dữ liệu đã được cache, trả về dữ liệu từ cache
            if (cachedData != null && cachedData.Count > 0)
            {
                Console.WriteLine("Lấy dữ liệu từ cache.");
                return cachedData;
            }

            // Nếu chưa có dữ liệu trong cache, đọc từ file
            Console.WriteLine("Đọc dữ liệu từ file và cache nó.");
            cachedData = new List<string>();

            // Kiểm tra xem file có tồn tại không
            if (!File.Exists(filePath))
            {
                Console.WriteLine("File không tồn tại.");
                return cachedData;
            }

            // Đọc file và lưu vào cache
            using (StreamReader reader = new StreamReader(filePath))
            {
                string? line;
                while ((line = await reader.ReadLineAsync()) != null)
                {
                    cachedData.Add(line);
                }
            }

            return cachedData;
        }

    }
}
