using Momentary.Domain.Aggregates.Post;
using Momentary.Domain.ValueObjects;

namespace Momentary.Application.Posts;

public class PostApplicationService
{
    private readonly IPostRepository _postRepository;
    public event Action<List<Post>>? PostsUpdated;

    public PostApplicationService(IPostRepository postRepository)
    {
        _postRepository = postRepository;
        _postRepository.ListenForUpdates(posts => PostsUpdated?.Invoke(posts));
    }

    public async Task CreatePostAsync(CreatePostCommand command)
    {
        var author = new Author(command.AuthorUid, command.AuthorDisplayName, command.AuthorPhotoUrl);
        var photo = Photo.Create(command.PhotoContent, command.PhotoLastModified);
        var post = Post.Factory.CreateNew(author, photo);
        await _postRepository.SaveAsync(post);
    }

    public void Unsubscribe()
    {
        _postRepository.StopListening();
    }
}

public record CreatePostCommand(string AuthorUid, string AuthorDisplayName, string AuthorPhotoUrl, byte[] PhotoContent, DateTime PhotoLastModified);