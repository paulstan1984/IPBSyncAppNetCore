namespace IPBSyncAppNetCore.Jobs
{
    public class SyncStocksJob
    {
        public void Execute()
        {
            this.Exdecute().Wait();
        }

        public async Task<string> Exdecute()
        {
            // Create an instance of HttpClient
            using var client = new HttpClient();

            // Set base address of the API
            client.BaseAddress = new Uri(Config.WMERESTAPIURL);

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
