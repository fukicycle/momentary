using Momentary.Domain.ValueObjects;

namespace Momentary.Domain.Aggregates.Post;

public class Post
{
    public PostId Id { get; private set; }
    public Author Author { get; private set; }
    public Photo Photo { get; private set; }
    public DateTime PostedAt { get; private set; }

    internal Post(PostId id, Author author, Photo photo, DateTime postedAt)
    {
        Id = id;
        Author = author;
        Photo = photo;
        PostedAt = postedAt;
    }

    public static class Factory
    {
        public static Post CreateNew(Author author, Photo photo)
        {
            if (photo.IsExpiredForPosting())
                throw new InvalidOperationException("24時間以上前に撮影された写真は投稿できません。");

            return new Post(PostId.NewId(), author, photo, DateTime.UtcNow);
        }

        public static Post Hydrate(string id, Author author, Photo photo, DateTime postedAt)
        {
            return new Post(new PostId(id), author, photo, postedAt);
        }
    }
}