namespace XTransmit.Model.Curl
{
    public class CurlResponse
    {
        public int Index { get; }
        public string Time { get; }
        public string Response { get; }

        public CurlResponse(int index, string time, string response)
        {
            Index = index;
            Time = time;
            Response = response;
        }
    }
}
