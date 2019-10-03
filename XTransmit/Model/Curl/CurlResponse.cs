namespace XTransmit.Model.Curl
{
    /**Updated: 2019-10-02
     */
    public class CurlResponse
    {
        public int Index { get; set; }
        public string Time { get; set; }
        public string Response { get; set; }

        public CurlResponse(int index, string time, string response)
        {
            Index = index;
            Time = time;
            Response = response;
        }
    }
}
