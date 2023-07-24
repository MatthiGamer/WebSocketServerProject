using System.Text.Json;
using System.Text.Json.Nodes;

namespace WebSocketServer
{
    public static class WebSocketServerDataHandler
    {
        private static Dictionary<int, DateTime> dataModifiedDictionary = new Dictionary<int, DateTime>();
        private static List<BlogPost> modifiedBlogPostsList = new List<BlogPost>();
        private static List<BlogPost> blogPostsList = new List<BlogPost>();

        public static BlogPost[] GetAllBlogPosts() => blogPostsList.ToArray();

        public static BlogPost[] GetModifiedPosts()
        {
            BlogPost[] blogPosts = modifiedBlogPostsList.ToArray();
            modifiedBlogPostsList.Clear();
            return blogPosts;
        }

        public static async Task<bool> CheckForNewData()
        {
            HttpContent? responseContent = await GetHttpContentFromLink("https://thekey.academy/wp-json/wp/v2/posts");

            if (responseContent == null)
                return false;

            JsonArray blogPostArray = GetJsonArray(await responseContent.ReadAsByteArrayAsync());
            bool isNewDataAvailable = CheckForNewPosts(blogPostArray);
            return isNewDataAvailable;
        }

        private static async Task<HttpContent?> GetHttpContentFromLink(string? contentURI)
        {
            HttpContent? responseContent = null;

            try
            {
                // May throw SSL error

                HttpClient httpClient = new HttpClient();
                HttpRequestMessage httpGetRequest = new HttpRequestMessage(new HttpMethod("GET"), contentURI);
                HttpResponseMessage httpResponse = await httpClient.SendAsync(httpGetRequest);
                responseContent = httpResponse.Content;
            }
            catch (Exception exception)
            {
                PrintError(exception);
            }

            return responseContent;
        }

        private static JsonArray GetJsonArray(ReadOnlySpan<byte> httpContent)
        {
            Utf8JsonReader utf8JSONReader = new Utf8JsonReader(httpContent);
            JsonArray? jsonArray = JsonArray.Create(JsonElement.ParseValue(ref utf8JSONReader));
            return jsonArray!;
        }

        public static bool CheckForNewPosts(JsonArray blogPostArray)
        {
            bool isNewDataAvailable = false;

            BlogPost[] blogPosts = new BlogPost[blogPostArray.Count];

            for (int i = 0; i < blogPostArray.Count; i++)
            {
                JsonNode blogPost = blogPostArray[i]!;
                isNewDataAvailable = CheckBlogPost(blogPost, ref blogPosts[i]);
            }

            return isNewDataAvailable;
        }

        private static bool CheckBlogPost(JsonNode blogPostToCheck, ref BlogPost blogPost)
        {
            int _id;
            DateTime _modifiedAt;
            string _title;
            string _content;

            try
            {
                // May throw a FormatException

                _id = blogPostToCheck!["id"]!.GetValue<int>();
                _modifiedAt = blogPostToCheck!["modified"]!.GetValue<DateTime>();

                // Data already exists and hasn't been modified
                if (dataModifiedDictionary.ContainsKey(_id) && dataModifiedDictionary[_id].Equals(_modifiedAt))
                    return false;

                _title = blogPostToCheck!["title"]!["rendered"]!.GetValue<string>();
                _content = blogPostToCheck!["content"]!["rendered"]!.GetValue<string>();

                if (!dataModifiedDictionary.ContainsKey(_id))
                {
                    // New blog post
                    blogPost = new BlogPost(_id, _modifiedAt, _title, HTMLFormatter.GetWordMapFromHTML(_content));
                    dataModifiedDictionary.Add(_id, _modifiedAt);
                    blogPostsList.Add(blogPost);
                }
                else
                {
                    // Updated blog post
                    blogPost.ModifiedAt = _modifiedAt;
                    blogPost.Title = _title;
                    blogPost.Content = HTMLFormatter.GetWordMapFromHTML(_content);
                    dataModifiedDictionary[_id] = _modifiedAt;
                }

                modifiedBlogPostsList.Add(blogPost);
                return true;
            }
            catch (FormatException exception)
            {
                PrintError(exception);
                return false;
            }
        }

        private static void PrintError(Exception exception) => Console.Error.WriteLine($"WebSocketServerDataHandlerError: {exception.Message}\n{exception.HelpLink}\n{exception.StackTrace}");
    }
}
