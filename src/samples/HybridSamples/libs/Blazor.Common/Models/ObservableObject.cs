using System.ComponentModel;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace ToggleDemo.Models;

/// <summary>
/// Provides base functionality for objects that notify property changes.
/// </summary>
public abstract class ObservableObject : INotifyPropertyChanged
{
    /// <summary>
    /// Sets the field to a new value and raises the PropertyChanged event if the value has changed.
    /// </summary>
    /// <typeparam name="T">The type of the property.</typeparam>
    /// <param name="propertyExpression">An expression representing the property.</param>
    /// <param name="field">A reference to the field backing the property.</param>
    /// <param name="newValue">The new value to set.</param>
    /// <returns>True if the value was changed; otherwise, false.</returns>
    protected bool Set<T>(Expression<Func<T>> propertyExpression, ref T field, T newValue)
    {
        if (EqualityComparer<T>.Default.Equals(field, newValue)) return false;

        field = newValue;

        OnPropertyChanged(propertyExpression);

        return true;
    }

    /// <summary>
    /// Sets the field to a new value and raises the PropertyChanged event if the value has changed.
    /// </summary>
    /// <typeparam name="T">The type of the property.</typeparam>
    /// <param name="propertyName">The name of the property.</param>
    /// <param name="field">A reference to the field backing the property.</param>
    /// <param name="newValue">The new value to set.</param>
    /// <returns>True if the value was changed; otherwise, false.</returns>
    protected bool Set<T>(string? propertyName, ref T field, T newValue)
    {
        if (EqualityComparer<T>.Default.Equals(field, newValue))
            return false;
        field = newValue;
        OnPropertyChanged(propertyName);
        return true;
    }

    /// <summary>
    /// Sets the field to a new value and raises the PropertyChanged event if the value has changed.
    /// </summary>
    /// <typeparam name="T">The type of the property.</typeparam>
    /// <param name="field">A reference to the field backing the property.</param>
    /// <param name="newValue">The new value to set.</param>
    /// <param name="propertyName">The name of the property. Automatically provided by the compiler.</param>
    /// <returns>True if the value was changed; otherwise, false.</returns>
    protected bool Set<T>(ref T field, T newValue, [CallerMemberName] string? propertyName = null)
        => Set(propertyName, ref field, newValue);

    /// <summary>
    /// Occurs when a property value changes.
    /// </summary>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// Gets the property name from a property expression.
    /// </summary>
    /// <typeparam name="T">The type of the property.</typeparam>
    /// <param name="propertyExpression">An expression representing the property.</param>
    /// <returns>The name of the property.</returns>
    protected static string GetPropertyName<T>(Expression<Func<T>> propertyExpression)
    {
        if (propertyExpression == null)
            throw new ArgumentNullException(nameof(propertyExpression));

        if (propertyExpression.Body is not MemberExpression body)
            throw new ArgumentException("Invalid argument", nameof(propertyExpression));

        if (body.Member is not PropertyInfo member)
            throw new ArgumentException("Argument is not a property", nameof(propertyExpression));

        return member.Name;
    }

    /// <summary>
    /// Raises the PropertyChanged event for the specified property.
    /// </summary>
    /// <param name="propertyName">The name of the property. Automatically provided by the compiler.</param>
    public virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    /// <summary>
    /// Raises the PropertyChanged event for the property specified by the expression.
    /// </summary>
    /// <typeparam name="T">The type of the property.</typeparam>
    /// <param name="propertyExpression">An expression representing the property.</param>
    public virtual void OnPropertyChanged<T>(Expression<Func<T>> propertyExpression)
    {
        if (PropertyChanged == null) return;

        string propertyName = GetPropertyName(propertyExpression);
        if (string.IsNullOrEmpty(propertyName)) return;

        OnPropertyChanged(propertyName);
    }
}
