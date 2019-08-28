namespace XTransmit.Model.Curl
{
    /**
     * Updated: 2019-08-02
     */
    public class CurlResponse
    {
        public int Index { get; set; }
        public string Response { get; set; }

        public CurlResponse(int index, string response)
        {
            Index = index;
            Response = response;
        }
    }
}
