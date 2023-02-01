using Amazon.DynamoDBv2.DataModel;

namespace robot_coin
{
    [DynamoDBTable("nicehash_usdt")]
    internal class NicehashUSDT
    {
        [DynamoDBHashKey] //Partition key
        public string dateString { get; set; } = DateTime.Now.ToString("yyyyMMddHHmmss");

        public int AAVEUSDT { get; set; } = 0;
    }
}
