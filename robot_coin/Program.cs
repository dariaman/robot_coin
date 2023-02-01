using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using robot_coin;

//EnvironmentVariableTarget dihilangkan kalau deploy di linux
var INDODAX_PRICE_URL = Environment.GetEnvironmentVariable("INDODAX_PRICE_URL", EnvironmentVariableTarget.User);
var NICEHASH_PRICE_URL = Environment.GetEnvironmentVariable("NICEHASH_PRICE_URL", EnvironmentVariableTarget.User);

var AWS_ACCESS_KEY = Environment.GetEnvironmentVariable("AWS_ACCESS_KEY", EnvironmentVariableTarget.User);
var AWS_SECRET_KEY = Environment.GetEnvironmentVariable("AWS_SECRET_KEY", EnvironmentVariableTarget.User);

if (AWS_ACCESS_KEY == null || AWS_SECRET_KEY == null)
{
    Console.WriteLine("accessKey or secretKey is null");
    Environment.Exit(0);
}

if (INDODAX_PRICE_URL == null)
{
    Console.WriteLine("IndodaxPriceUrl is null");
    Environment.Exit(0);
}

if (NICEHASH_PRICE_URL == null)
{
    Console.WriteLine("Nicehash Env is null");
    Environment.Exit(0);
}

var PriceNicehash = await GetPriceNicehashAsync();
var PriceIndodax = await GetPriceIndodaxAsync();


var awsCredentials = new Amazon.Runtime.BasicAWSCredentials(AWS_ACCESS_KEY, AWS_SECRET_KEY);
var client = new AmazonDynamoDBClient(awsCredentials, RegionEndpoint.APSoutheast1); // singapore

if (PriceNicehash != null || PriceIndodax != null)
{
    string TableName;
    if (PriceNicehash?.AAVEBTC > 0 || PriceNicehash?.AAVEUSDT > 0 || PriceIndodax?.aave_idr > 0)
    {
        TableName = "AAVE";
        await CreateTableIfExist(client, TableName);
        ///// write data to table
        try
        {
            Table tabel_coin = Table.LoadTable(client, TableName);
            var price_usdt = new Document();
            price_usdt["dateString"] = DateTime.Now.ToString("yyyyMMddHHmmss");
            price_usdt["USDT"] = PriceNicehash?.AAVEUSDT;
            price_usdt["BTC"] = PriceNicehash?.AAVEBTC;
            price_usdt["IDR"] = PriceIndodax?.aave_idr;

            await tabel_coin.PutItemAsync(price_usdt);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
        }
    }

}

//Console.ReadLine();


// ===================================Baca/Tulis data pakai DBContext=================================================================
//try
//{
//    DynamoDBContext context = new DynamoDBContext(client);
//    NicehashUSDT new_price_usdt = new()
//    {
//        AAVEUSDT = 12344
//    };
//    await context.SaveAsync(new_price_usdt);

//    var LastPrice = await context.LoadAsync<NicehashUSDT>(new_price_usdt.dateString);
//    var range_price_2H = DateTime.Now.AddHours(-2).ToString("yyyyMMddHHmm");
//    var search_2hours_price = context.ScanAsync<NicehashUSDT>(
//        new[]
//        {
//            new ScanCondition(
//                nameof(NicehashUSDT.dateString),
//                ScanOperator.GreaterThanOrEqual,
//                range_price_2H
//                )
//        });

//    var ListPrice2Hours = await search_2hours_price.GetRemainingAsync();
//}
//catch (Exception ex) { Console.WriteLine(ex.ToString()); }
// ===================================END Baca/Tulis data pakai DBContext=================================================================

static async Task CreateTableIfExist(IAmazonDynamoDB client, string tableName)
{
    try
    {
        var res = await client.DescribeTableAsync(new DescribeTableRequest { TableName = tableName });
    }
    catch
    {
        Console.WriteLine("Create Table >> " + tableName);
        await CreateTable(client, tableName);
    }
}

static async Task<DescribeTableResponse> CreateTable(IAmazonDynamoDB client, string tableName)
{

    var response = await client.CreateTableAsync(new CreateTableRequest
    {
        TableName = tableName,
        AttributeDefinitions = new List<AttributeDefinition>()
          {
            new AttributeDefinition { AttributeName = "dateString", AttributeType = "S" },
          },
        KeySchema = new List<KeySchemaElement>()
          {
            new KeySchemaElement { AttributeName = "dateString", KeyType = "HASH" },
          },
        ProvisionedThroughput = new ProvisionedThroughput { ReadCapacityUnits = 10, WriteCapacityUnits = 5 },
    });

    var result = await WaitTillTableCreated(client, tableName, response);

    return result;
}

///// Must wait create table until status active(finish)
static async Task<DescribeTableResponse> WaitTillTableCreated(IAmazonDynamoDB client, string tableName, CreateTableResponse response)
{
    DescribeTableResponse resp = new DescribeTableResponse();
    var tableDescription = response.TableDescription;
    string status = tableDescription.TableStatus;
    int sleepDuration = 1000; // One second

    // Don't wait more than 10 seconds.
    while ((status != "ACTIVE") && (sleepDuration < 10000))
    {
        System.Threading.Thread.Sleep(sleepDuration);

        resp = await client.DescribeTableAsync(new DescribeTableRequest { TableName = tableName });

        status = resp.Table.TableStatus;
        sleepDuration *= 2;
    }

    return resp;
}

async Task<Nicehash_Price?> GetPriceNicehashAsync()
{
    try
    {
        HttpClient client = new();
        var resp = await client.GetAsync(NICEHASH_PRICE_URL).Result.Content.ReadAsStringAsync();
        var _data_price = JsonConvert.DeserializeObject<Nicehash_Price>(resp);

        return _data_price;
    }
    catch { throw; }
}

async Task<Indodax_Price?> GetPriceIndodaxAsync()
{
    try
    {
        HttpClient client = new();
        var resp = await client.GetAsync(INDODAX_PRICE_URL).Result.Content.ReadAsStringAsync();
        JObject jObject = JObject.Parse(resp);
        if (jObject["tickers"] == null) return null;
        var coin = new Indodax_Price();

        if (jObject["tickers"]?["aave_idr"]?["last"] != null) coin.aave_idr = (int)jObject["tickers"]?["aave_idr"]?["last"];
        if (jObject["tickers"]?["ada_idr"]?["last"] != null) coin.ada_idr = (int)jObject["tickers"]?["ada_idr"]?["last"];
        if (jObject["tickers"]?["btc_idr"]?["last"] != null) coin.btc_idr = (int)jObject["tickers"]?["btc_idr"]?["last"];




        return coin;
    }
    catch { throw; }
}
