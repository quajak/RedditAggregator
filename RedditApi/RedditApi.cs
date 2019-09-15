using RedditSharp;
using RedditSharp.Things;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SourceApi
{

    public class RedditApi
    {
        readonly static Reddit reddit = new Reddit();

        public static List<Post> GetPosts(string subReddit, int count = 10)
        {
            List<Post> posts = new List<Post>();
            for (int i = 0; i < 5; i++)
            {
                try
                {
                    List<Post> _p = reddit.GetSubreddit(subReddit).Posts.Take(count).ToList();
                    if (_p.Count > posts.Count)
                        posts = _p;
                }
                catch (Exception e)
                {
                    Trace.TraceInformation(e.Message);
                }
                if (posts.Count == count)
                    return posts;
            }
            return posts;
        }

        public static List<Controller.Comment> GetComments(Post post)
        {
            int startId = Controller.Comment.GetId();
            List<Controller.Comment> comments = new List<Controller.Comment>();
            foreach (var comment in post.Comments)
            {
                comments.AddRange(GetComments(comment, -1, ref startId));
            }
            return comments;
        }

        static List<Controller.Comment> GetComments(RedditSharp.Things.Comment comment, int parentId, ref int id)
        {
            if (comment.Body is null || comment.Body == "" || comment.Body == "[Removed]")
                return new List<Controller.Comment>();

            int commentId = ++id;
            List<Controller.Comment> comments = new List<Controller.Comment>() { new Controller.Comment(commentId, comment, parentId) };
            foreach (var subComment in comment.Comments)
            {
                comments.AddRange(GetComments(subComment, commentId, ref id));
            }
            return comments;
        }
    }
}
