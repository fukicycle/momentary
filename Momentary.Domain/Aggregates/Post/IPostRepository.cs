namespace Momentary.Domain.Aggregates.Post;

public interface IPostRepository
{
    Task SaveAsync(Post post);
    void ListenForUpdates(Action<List<Post>> onUpdate);
    void StopListening();
}