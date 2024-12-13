namespace RedisMVCHomeWork2.Statics
{
    public static class RedisElementStatic
    {

        static public readonly string MyListName = "RPubSub";
        static public string SelectedCName = "";
        static public List<string> OldMessages = new();

    }
}
