using Microsoft.Extensions.Configuration;
using Microsoft.JSInterop;
using Momentary.Domain.Aggregates.Post;
using Momentary.Domain.ValueObjects;
using Momentary.Infrastructure.Dto;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace Momentary.Infrastructure.Repositories;

public class FirebasePostRepository : IPostRepository
{
    private readonly HttpClient _httpClient;
    private readonly IJSRuntime _jsRuntime;
    private readonly string _databaseUrl;

    public FirebasePostRepository(HttpClient httpClient, IJSRuntime jsRuntime, IConfiguration config)
    {
        _httpClient = httpClient;
        _jsRuntime = jsRuntime;
        _databaseUrl = config["Firebase:DatabaseUrl"] ?? throw new NullReferenceException();
    }

    public async Task SaveAsync(Post post)
    {
        var idToken = await _jsRuntime.InvokeAsync<string?>("authFunctions.getIdToken");

        if (string.IsNullOrEmpty(idToken))
        {
            // IDトークンが取得できない場合は、ユーザーがログインしていないか、何らかの問題がある
            // 例外をスローするか、エラーをログに記録して処理を中止する
            throw new UnauthorizedAccessException("ユーザーが認証されていません。Firebase IDトークンが取得できませんでした。");
        }
        var dto = new PostDto
        {
            Author = new AuthorDto { Uid = post.Author.Uid, DisplayName = post.Author.DisplayName, PhotoUrl = post.Author.PhotoUrl },
            ImageBase64 = post.Photo.Base64Content,
            LastModified = new DateTimeOffset(post.Photo.LastModified).ToUnixTimeMilliseconds()
        };
        var response = await _httpClient.PutAsJsonAsync($"{_databaseUrl}/posts/{post.Id.Value}.json?auth={idToken}", dto);
        response.EnsureSuccessStatusCode();
    }

    public void ListenForUpdates(Action<List<Post>> onUpdate)
    {
        var callback = DotNetObjectReference.Create(new CallbackHelper(onUpdate));
        _jsRuntime.InvokeVoidAsync("dbFunctions.listenToPosts", callback);
    }

    public void StopListening()
    {
        _jsRuntime.InvokeVoidAsync("dbFunctions.stopListeningToPosts");
    }

    // JSからのコールバックを受け取るためのヘルパークラス
    private class CallbackHelper
    {
        private readonly Action<List<Post>> _onUpdate;

        public CallbackHelper(Action<List<Post>> onUpdate)
        {
            _onUpdate = onUpdate;
        }

        [JSInvokable]
        public void OnPostsReceived(string json)
        {
            var posts = new List<Post>();
            if (string.IsNullOrEmpty(json))
            {
                _onUpdate(posts);
                return;
            }

            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var dtos = JsonSerializer.Deserialize<Dictionary<string, PostDto>>(json, options);

            if (dtos != null)
            {
                posts = dtos.Select(kvp => Post.Factory.Hydrate(
                        kvp.Key,
                        new Author(kvp.Value.Author.Uid, kvp.Value.Author.DisplayName, kvp.Value.Author.PhotoUrl),
                        Photo.Create(Convert.FromBase64String(kvp.Value.ImageBase64), DateTimeOffset.FromUnixTimeMilliseconds(kvp.Value.LastModified).UtcDateTime),
                        // timestampはサーバーで設定されるので、ここでは便宜上Nowを設定
                        DateTime.UtcNow
                    )).OrderByDescending(p => p.PostedAt) //仮のソート
                    .ToList();
            }
            _onUpdate(posts);
        }
    }
}