# User Management Sample Implementation

This implementation demonstrates route parameter substitution in the Blazing.Mvvm.Sample.Server application following DRY, KISS, SOLID, and MVVM principles.

## Architecture Overview

### Services Layer (Following SOLID - Single Responsibility & Dependency Inversion)

#### Models
- `User.cs` - User entity
- `Post.cs` - Post entity

#### Service Interfaces
- `IUsersService` - Contract for user operations
- `IPostsService` - Contract for post operations

#### Service Implementations
- `UsersService` - Mock implementation for user data
- `PostsService` - Mock implementation for post data

**Benefits:**
- **DRY**: Centralized data management, no duplication
- **SOLID**: Interfaces allow for easy testing and future implementation swapping
- **KISS**: Simple, focused services with clear responsibilities

### ViewModels Layer (Following MVVM)

#### UsersViewModel
- **Route**: `/users`
- **Responsibility**: Display list of all users
- **Navigation**: To `UserViewModel` with userId parameter

#### UserViewModel
- **Route**: `/users/{userId}`
- **Responsibility**: Display user details and their posts
- **Navigation**: 
  - To `UserPostsViewModel` with userId
  - To `UserPostViewModel` with userId/postId
  - Back to `UsersViewModel`

#### UserPostsViewModel
- **Route**: `/users/{userId}/posts`
- **Responsibility**: Display all posts for a specific user
- **Navigation**:
  - To `UserPostViewModel` with userId/postId
  - Back to `UserViewModel`

#### UserPostViewModel
- **Route**: `/users/{userId}/posts/{postId}`
- **Responsibility**: Display full post details
- **Navigation**:
  - Back to `UserPostsViewModel`
  - Back to `UserViewModel`
  - Back to `UsersViewModel`

**MVVM Benefits:**
- ViewModels contain no UI logic
- All business logic in services
- Commands for user actions
- Observable properties for UI binding

### Pages Layer (Blazor Components)

- `Users.razor` - List of users with table view
- `User.razor` - User profile with posts preview
- `UserPosts.razor` - All posts for a user
- `UserPost.razor` - Full post content with breadcrumb navigation

## Route Parameter Substitution Testing

### Test Scenarios

1. **Simple Parameter**: `/users/{userId}`
   - Navigate with: `NavigateTo<UserViewModel>("2503")`
   - Expected: `/users/2503`

2. **Multiple Parameters**: `/users/{userId}/posts/{postId}`
   - Navigate with: `NavigateTo<UserPostViewModel>("2503/789")`
   - Expected: `/users/2503/posts/789`

3. **Navigation Chain**:
   - Users List ? User Details (userId: "2503")
   - User Details ? User Posts (userId: "2503")
   - User Posts ? Post Detail (userId: "2503", postId: "789")

## Design Principles Applied

### DRY (Don't Repeat Yourself)
- ? Centralized data in services
- ? Reusable navigation commands
- ? Shared models across ViewModels

### KISS (Keep It Simple, Stupid)
- ? Simple service interfaces
- ? Clear ViewModel responsibilities
- ? Straightforward navigation flow

### SOLID Principles

#### Single Responsibility
- ? Each service handles one entity
- ? Each ViewModel handles one view
- ? Each page displays one concept

#### Open/Closed
- ? Services can be extended without modification
- ? New ViewModels can be added without changing existing ones

#### Liskov Substitution
- ? Service implementations can be swapped
- ? Mock services can replace real ones for testing

#### Interface Segregation
- ? Small, focused interfaces (IUsersService, IPostsService)
- ? No fat interfaces forcing unnecessary implementations

#### Dependency Inversion
- ? ViewModels depend on service abstractions, not implementations
- ? Dependency injection throughout

### MVVM Pattern
- ? **Model**: User, Post entities
- ? **View**: Razor pages (.razor files)
- ? **ViewModel**: *ViewModel classes with commands and properties
- ? **Services**: Data access abstraction

## Sample Data

### Users
- Alice Johnson (ID: 1)
- Bob Smith (ID: 2)
- Charlie Brown (ID: 3)
- Deep Thought (ID: 42)
- Test User (ID: 2503)

### Posts
- Each user has 2-4 posts with realistic content
- Post IDs include: 101, 102, 789 (commonly referenced in tests)

## Navigation Added to NavMenu

Updated `NavMenu.razor` to include:
```razor
<MvvmNavLink class="nav-link" TViewModel="UsersViewModel">
    <span class="oi oi-people" aria-hidden="true"></span> Users
</MvvmNavLink>
```

## Running the Sample

1. Build the solution
2. Run `Blazing.Mvvm.Sample.Server`
3. Navigate to "Users" in the sidebar
4. Test route parameter substitution by:
   - Clicking "View Details" for any user
   - Clicking "View All Posts" or individual post "Read More" buttons
   - Observing URL changes in browser address bar

## Expected URL Patterns

- Users list: `/users`
- User details: `/users/2503`
- User posts: `/users/2503/posts`
- Specific post: `/users/2503/posts/789`

All navigation uses route parameter substitution instead of URL appending!
