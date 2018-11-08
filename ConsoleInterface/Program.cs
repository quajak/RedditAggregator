using RedditAggregator.Model;
using RedditSharp;
using RedditSharp.Things;
using System;
using System.Collections.Generic;
using System.Linq;
using Comment = RedditAggregator.Model.Comment;

namespace ConsoleInterface
{
    class Program
    {
        static Reddit reddit = new Reddit();
        static void Main(string[] args)
        {
            CreateRawComments();
        }

        static void CreateRawComments()
        {
            Post post = GetPost();
            List<Comment> comments = RedditApi.GetComments(post);
            foreach (var comment in comments)
            {
                Console.WriteLine(comment.text + $"\n\r {comment.score1} {comment.score2}");
                string s = Console.ReadLine();
                if (s.ToLower() == "break")
                    break;
                comment.userScore = float.Parse(s);
            }
            Comment.SaveComments(comments);
        }

        static Post GetPost()
        {
            Console.Write("Subreddit: r/");
            string subRedditString = Console.ReadLine();
            var subReddit = reddit.GetSubreddit(subRedditString);
            var post = subReddit.Posts.Take(1).ToList()[0];
            foreach (var pPost in subReddit.Posts)
            {
                Console.WriteLine(pPost.Title + pPost.CommentCount.ToString());
                var r = Console.ReadLine();
                if (r == "y")
                {
                    post = pPost;
                    break;
                }
            }
            return post;
        }
    }
}
