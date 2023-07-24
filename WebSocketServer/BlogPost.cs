namespace WebSocketServer
{
    public class BlogPost
    {
        public bool IsModifiedOrNew { get; set; }
        public int ID { get; private set; }
        public DateTime ModifiedAt { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }

        public BlogPost(int _id, DateTime _modifiedAt, string _title, string _content)
        {
            ID = _id;
            ModifiedAt = _modifiedAt;
            Title = _title;
            Content = _content;
            IsModifiedOrNew = true;
        }
    }
}
