namespace HybridSample.Blazor.Core;

public static class StaticStrings
{
    public static class ObservableObject
    {
        public static string Sample1Razor =
            """
            <div class="mb-3">
              <input @bind-value=ViewModel.Name
                     @bind-value:event="oninput"
                     type="text"
                     class="form-control"
                     placeholder="Type here to update the textbelow">
              <p class="sample--output">@ViewModel.Name</p>
            </div>
            """;

        public static string Sample1Csharp =
            """
            private string name;

            /// <summary>
            /// Gets or sets the name to display.
            /// </summary>
            public string Name
            {
                get => name;
                set => SetProperty(ref name, value);
            }
            """;

        public static string Sample2Razor =
            """
            <div class="mb-3">
                <button type="button"
                        class="btn btn-primary" @onclick="() => ViewModel.ReloadTaskCommand.Execute(null)">
                    Click me to load a Task to await
                </button>
                <p class="sample--output">@ViewModel.MyTask?.Status</p>
            </div>
            """;

        public static string Sample2Csharp =
            """
            public ObservableObjectPageViewModel()
            {
                ReloadTaskCommand = new RelayCommand(ReloadTask);
            }

            public ICommand ReloadTaskCommand { get; }

            private string name;

            public string Name
            {
                get => name;
                set => SetProperty(ref name, value);
            }

            private TaskNotifier myTask;

            public Task MyTask
            {
                get => myTask;
                private set => SetPropertyAndNotifyOnCompletion(ref myTask, value);
            }

            public void ReloadTask()
            {
                MyTask = Task.Delay(3000);
            }
            """;
    }

    public static class RelayCommand
    {
        public static string Sample1Razor =
            """
            ```html
            <div class="mb-3">
                <p class="sample--output">@ViewModel.Counter</p>
                <button type="button"
                        class="btn btn-primary"
                        @onclick="() => ViewModel.IncrementCounterCommand.Execute(null)">
                    Click me!
                </button>
            </div>
            ```
            """;

        public static string Sample1Csharp =
            """
            ```csharp
            public class MyViewModel : ObservableObject
            {
                public MyViewModel()
                {
                    IncrementCounterCommand = new RelayCommand(IncrementCounter);
                }
            
                /// <summary>
                /// Gets the <see cref="ICommand"/> responsible for incrementing <see cref="Counter"/>.
                /// </summary>
                public ICommand IncrementCounterCommand { get; }
            
                private int counter;
            
                /// <summary>
                /// Gets the current value of the counter.
                /// </summary>
                public int Counter
                {
                    get => counter;
                    private set => SetProperty(ref counter, value);
                }
            
                /// <summary>
                /// Increments <see cref="Counter"/>.
                /// </summary>
                private void IncrementCounter() => Counter++;
            }
            ```
            """;
    }

    public static class AsyncRelayCommand
    {
        public static string Sample1Razor =
            """
            ```html
            <div class="mb-3">
                <p class="sample--output">Status: @ViewModel.DownloadTextCommand.ExecutionTask?.Status</p>
                @{
                    string? GetResult(object? executionTask)
                    {
                        if (executionTask is Task<string> task)
                            return task.Status == TaskStatus.RanToCompletion ? task.Result : default;
            
                        return default;
                    }
                    <p class="sample--output">Result: @GetResult(ViewModel.DownloadTextCommand.ExecutionTask)</p>
                }
                <button type="button"
                        class="btn btn-primary" @onclick="async () => 
                        {
                            await ViewModel.DownloadTextCommand.ExecuteAsync(null);
                            await InvokeAsync(StateHasChanged);
                        }">
                    Click me!
                </button>
            </div>
            ```
            """;

        public static string Sample1Csharp =
            """
            ```csharp
            public MyViewModel()
            {
                DownloadTextCommand = new AsyncRelayCommand(DownloadTextAsync);
            }

            public IAsyncRelayCommand DownloadTextCommand { get; }

            private async Task<string> DownloadTextAsync()
            {
                await Task.Delay(3000); // Simulate a web request
            
                return "Hello world!";
            }
            ```
            """;
    }

    public static class MessengerSend
    {
        public static string Sample1Razor =
            """
            ```html
            <!--Sender module-->
            <div class="mb-3 shadow-none bg-light rounded">
                <p class="sample--output">Status: @ViewModel.SenderViewModel.Username</p>
                <button type="button"
                        class="btn btn-primary" @onclick="() =>
                            ViewModel.SenderViewModel.SendUserMessageCommand.Execute(null)">
                    Click me!
                </button>
            </div>

            <!--Receiver module-->
            <div class="mb-3 shadow-none bg-light rounded">
                <p class="sample--output">Status: @ViewModel.ReceiverViewModel.Username</p>
            </div>

            @code {
                protected override async Task OnAfterRenderAsync(bool firstRender)
                {
                    if (firstRender)
                        ViewModel.ReceiverViewModel.IsActive = true; // enable
                }
            }
            ```
            """;

        public static string Sample1Csharp =
            """
            ```csharp
            public class MessengerPageViewModel : SamplePageViewModel
            {
            	public MessengerPageViewModel()
            	{
                    RequestCurrentUsernameCommand = new RelayCommand(RequestCurrentUsername);
                    ResetCurrentUsernameCommand = new RelayCommand(ResetCurrentUsername);
                }
            
                public ICommand RequestCurrentUsernameCommand { get; }
                public ICommand ResetCurrentUsernameCommand { get; }
            
                public UserSenderViewModel SenderViewModel { get; } = new UserSenderViewModel();
            
                public UserReceiverViewModel ReceiverViewModel { get; } = new UserReceiverViewModel();
            
                // Simple viewmodel for a module sending a username message
                public class UserSenderViewModel : ObservableRecipient
                {
                    public UserSenderViewModel()
                    {
                        SendUserMessageCommand = new RelayCommand(SendUserMessage);
                    }
            
                    public ICommand SendUserMessageCommand { get; }
            
                    private string username = "Bob";
            
                    public string Username
                    {
                        get => username;
                        private set => SetProperty(ref username, value);
                    }
            
                    protected override void OnActivated()
                    {
                        Messenger.Register
                            <CurrentUsernameRequestMessage>(this, m => m.Reply(Username));
                    }
            
                    public void SendUserMessage()
                    {
                        Username = Username == "Bob" ? "Alice" : "Bob";
            
                        Messenger.Send(new UsernameChangedMessage(Username));
                    }
                }
            
                // Simple viewmodel for a module receiving a username message
                public class UserReceiverViewModel : ObservableRecipient
                {
                    private string username = "";
            
                    public string Username
                    {
                        get => username;
                        private set => SetProperty(ref username, value);
                    }
            
                    protected override void OnActivated()
                    {
                        Messenger.Register
                            <UsernameChangedMessage>(this, m => Username = m.Value);
                    }
                }

            // A sample message with a username value
            public sealed class UsernameChangedMessage : ValueChangedMessage<string>
            {
                public UsernameChangedMessage(string value) : base(value)
                {
                }
            }
            ```
            """;
    }

    public static class MessengerRequest
    {
        public static string Sample1Razor =
            """
            ```html
                <div class="mb-3 shadow-none bg-light rounded">
                    <p class="sample--output">Status: @ViewModel.Username</p>
                    <button type="button"
                            class="btn btn-primary" @onclick="() =>
                            ViewModel.RequestCurrentUsernameCommand.Execute(null)">
                        Click to request the username!
                    </button>
                    <button type="button"
                            class="btn btn-primary" @onclick="() =>
                            ViewModel.ResetCurrentUsernameCommand.Execute(null)">
                        Click to reset the local username!
                    </button>
                </div>

            @code {
                protected override async Task OnAfterRenderAsync(bool firstRender)
                {
                    if (firstRender)
                        ViewModel.SenderViewModel.IsActive = true; // enable
                }
            }
            ```
            """;

        public static string Sample1Csharp =
            """
            ```csharp
            public class UserSenderViewModel : ObservableRecipient
            {
                public UserSenderViewModel()
                {
                    SendUserMessageCommand = new RelayCommand(SendUserMessage);
                }
            
                public ICommand SendUserMessageCommand { get; }
            
                private string username = "Bob";
            
                public string Username
                {
                    get => username;
                    private set => SetProperty(ref username, value);
                }
            
                protected override void OnActivated()
                {
                    Messenger.Register
                        <CurrentUsernameRequestMessage>(this, m => m.Reply(Username));
                }
            
                public void SendUserMessage()
                {
                    Username = Username == "Bob" ? "Alice" : "Bob";
            
                    Messenger.Send(new UsernameChangedMessage(Username));
                }
            }

            // A sample request message to get the current username
            public sealed class CurrentUsernameRequestMessage : RequestMessage<string>
            {
            }
            ```
            """;
    }

    public static class RedditBrowser
    {
public static string Sample1Razor =
    """
    ## SubredditWidget.razor
    ```html
    @using System.Collections.Specialized
    @inherits MvvmComponentBase<SubredditWidgetViewModel>

    <div class="subreddit__container">
        <h2>Subreddit:</h2>
        <div class="subreddit__filter">
            <ListBox TItem=String ItemSource="ViewModel?.Subreddits"
                     SelectedItem=@ViewModel?.SelectedSubreddit
                     SelectionChanged="@ChangeSubredditAsync">
                <ItemTemplate>
                    @context
                </ItemTemplate>
            </ListBox>
            <div class="button-refresh__container">
                <button class="btn btn-primary" onclick="@(async()
                    => await GetPostsAsync())">REFRESH</button>
            </div>
        </div>
    
        <h2>Posts:</h2>
        <div class="subreddit_posts">
            <ListBox TItem=Post ItemSource="ViewModel!.Posts"
                     SelectedItem=@ViewModel.SelectedPost
                     SelectionChanged="@(e => ViewModel.SelectedPost = e.Item)">
                <ItemTemplate Context="post">
                    <div class="list-post">
                        <h3 class="list-post__title">@post.Title</h3>
                        @if (post.Thumbnail is not null && post.Thumbnail != "self")
                        {
                            <img src="@post.Thumbnail"
                                 onerror="this.onerror=null; this.style='display:none';"
                                 alt="@post.Title" class="list-post__image" />
                        }
                    </div>
                </ItemTemplate>
            </ListBox>
        </div>
    </div>

    @code {
        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
                await GetPostsAsync();
        }
    
        private async Task GetPostsAsync()
        {
            if (ViewModel is null) return;
    
            // manually flush
            ViewModel.SelectedPost = null;
            ViewModel.Posts.Clear();
    
            // update UI
            await InvokeAsync(StateHasChanged);
    
            // get posts
            await ViewModel.LoadPostsCommand.ExecuteAsync(null);
    
            // ObservableCollection change events not tracked,
            //   so we need to manually indicate binding update
            await InvokeAsync(StateHasChanged);
        }
    
        private async Task ChangeSubredditAsync(ListBoxEventArgs<string> e)
        {
            if (ViewModel is null) return;
    
            ViewModel.SelectedSubreddit = e.Item;
            await GetPostsAsync();
        }
    }
    ```
    ## PostWidget.razor
    ```html
    @inherits MvvmComponentBase<PostWidgetViewModel>

    <div class="card__container">
        <h2>Selected Post:</h2>
        <div class="card-post">
        @if (ViewModel is null || ViewModel.Post is null)
        {
            <p>No selection</p>
        }
        else
        {
            Post post = ViewModel!.Post;
            string title = post?.Title ?? "No Title";
    
            <h3 class="card-post__title">@title</h3>
            @if (post!.Thumbnail is not null && post.Thumbnail != "self")
            {
                <img src="@post!.Thumbnail"
                         onerror="this.onerror=null; this.style='display:none';"
                     alt="@post.Title" class="card-post__image" />
            }
            <p class="card-post__body">
                @(new MarkupString(post.SelfText.Replace("laborum.", "laborum.<br /><br />")))
            </p>
        }
        </div>
    </div>
    @code {
        protected override void OnAfterRender(bool firstRender)
        {
            if (firstRender)
                ViewModel!.IsActive = true;
        }
    }                
    ```
    """;

        public static string Sample1Csharp =
            """
            ## SubredditWidgetViewModel.cs
            ```csharp
            /// <summary>
            /// A viewmodel for a subreddit widget.
            /// </summary>
            public sealed class SubredditWidgetViewModel : RecipientViewModelBase
            {
                /// <summary>
                /// Gets the <see cref="IRedditService"/> instance to use.
                /// </summary>
                private readonly IRedditService RedditService;
            
                /// <summary>
                /// Gets the <see cref="ISettingsService"/> instance to use.
                /// </summary>
                private readonly ISettingsService SettingsService;
            
                /// <summary>
                /// An <see cref="AsyncLock"/> instance to avoid concurrent requests.
                /// </summary>
                private readonly AsyncLock LoadingLock = new();
            
                /// <summary>
                /// Creates a new <see cref="SubredditWidgetViewModel"/> instance.
                /// </summary>
                public SubredditWidgetViewModel(IRedditService redditService, ISettingsService settingsService)
                {
                    RedditService = redditService;
                    SettingsService = settingsService;
            
                    LoadPostsCommand = new AsyncRelayCommand(LoadPostsAsync);
            
                    selectedSubreddit = SettingsService.GetValue<string>(nameof(SelectedSubreddit)) ?? Subreddits[0];
                }
            
                /// <summary>
                /// Gets the <see cref="IAsyncRelayCommand"/> instance responsible for loading posts.
                /// </summary>
                public IAsyncRelayCommand LoadPostsCommand { get; }
            
                /// <summary>
                /// Gets the collection of loaded posts.
                /// </summary>
                public ObservableCollection<Post> Posts { get; } = new();
            
                /// <summary>
                /// Gets the collection of available subreddits to pick from.
                /// </summary>
                public IReadOnlyList<string> Subreddits { get; } = new[]
                {
                    "microsoft",
                    "windows",
                    "surface",
                    "windowsphone",
                    "dotnet",
                    "csharp"
                };
            
                private string selectedSubreddit;
            
                /// <summary>
                /// Gets or sets the currently selected subreddit.
                /// </summary>
                public string SelectedSubreddit
                {
                    get => selectedSubreddit;
                    set
                    {
                        SetProperty(ref selectedSubreddit, value);
            
                        SettingsService.SetValue(nameof(SelectedSubreddit), value);
                    }
                }
            
                private Post? selectedPost;
            
                /// <summary>
                /// Gets or sets the currently selected post, if any.
                /// </summary>
                public Post? SelectedPost
                {
                    get => selectedPost;
                    set => SetProperty(ref selectedPost, value, true);
                }
            
                /// <summary>
                /// Loads the posts from a specified subreddit.
                /// </summary>
                private async Task LoadPostsAsync()
                {
                    using (await LoadingLock.LockAsync())
                    {
                        try
                        {
                            PostsQueryResponse? response = await RedditService.GetSubredditPostsAsync(SelectedSubreddit);
            
                            Posts.Clear();
            
                            foreach (PostData? item in response.Data!.Items!)
                            {
                                Posts.Add(item.Data!);
                            }
                        }
                        catch
                        {
                            Debugger.Break();
                            // Whoops!
                        }
                    }
                }
            }
            ```
            ## PostWidgetViewModel.cs
            ```csharp
            /// <summary>
            /// A viewmodel for a post widget.
            /// </summary>
            public sealed class PostWidgetViewModel : RecipientViewModelBase<PropertyChangedMessage<Post>>
            {
                private Post? post;
            
                /// <summary>
                /// Gets the currently selected post, if any.
                /// </summary>
                public Post? Post
                {
                    get => post;
                    private set => SetProperty(ref post, value);
                }
            
                /// <inheritdoc/>
                public override void Receive(PropertyChangedMessage<Post> message)
                {
                    if (message is
                        {
                            Sender: SubredditWidgetViewModel,
                            PropertyName: nameof(SubredditWidgetViewModel.SelectedPost)
                        })
                    {
                        Post = message.NewValue;
                    }
                }
            }
            ```
            """;    }
}
