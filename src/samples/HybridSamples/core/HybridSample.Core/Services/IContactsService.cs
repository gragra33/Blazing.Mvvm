using HybridSample.Core.Models;
using Refit;

namespace HybridSample.Core.Services;

/// <summary>
/// An interface for a simple contacts service.
/// </summary>
public interface IContactsService
{
    /// <summary>
    /// Get a list of contacts.
    /// </summary>
    /// <param name="count">The number of contacts to retrieve.</param>
    [Get("/api/?dataType=json&inc=name,email,picture")]
    Task<ContactsQueryResponse> GetContactsAsync([AliasAs("results")] int count);
}
