namespace IPBSyncAppNetCore.Jobs
{
    public class SyncStocksJob
    {
        public void Execute()
        {
            this.GetStocs().Wait();
        }

        private string RESTAPIURL = "http://iuliusrds.ipb.ro:8080/datasnap/rest/TServerMethods/";

        public async Task<string> GetStocs()
        {
            // Create an instance of HttpClient
            using var client = new HttpClient();

            // Set base address of the API
            client.BaseAddress = new Uri(RESTAPIURL);

            try
            {
                // Call the API asynchronously
                HttpResponseMessage response = await client.GetAsync("GetStocArticoleExt");

                // Check if the response is successful
                if (response.IsSuccessStatusCode)
                {
                    // Read the content of the response
                    string responseData = await response.Content.ReadAsStringAsync();

                    return responseData;
                }
                else
                {
                    return $"Failed to call the API. Status code: {response.StatusCode}";
                }
            }
            catch (Exception ex)
            {
                return $"An error occurred: {ex.Message}";
            }
        }
    }
}
