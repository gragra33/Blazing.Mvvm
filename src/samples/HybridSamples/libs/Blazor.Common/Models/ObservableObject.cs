using System.ComponentModel;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace ToggleDemo.Models;

public abstract class ObservableObject : INotifyPropertyChanged
{
    protected bool Set<T>(Expression<Func<T>> propertyExpression, ref T field, T newValue)
    {
        if (EqualityComparer<T>.Default.Equals(field, newValue)) return false;

        field = newValue;

        OnPropertyChanged(propertyExpression);

        return true;
    }

    protected bool Set<T>(string? propertyName, ref T field, T newValue)
    {
        if (EqualityComparer<T>.Default.Equals(field, newValue))
            return false;
        field = newValue;
        OnPropertyChanged(propertyName);
        return true;
    }

    protected bool Set<T>(ref T field, T newValue, [CallerMemberName] string? propertyName = null)
        => Set(propertyName, ref field, newValue);

    public event PropertyChangedEventHandler? PropertyChanged;

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

    public virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    public virtual void OnPropertyChanged<T>(Expression<Func<T>> propertyExpression)
    {
        if (PropertyChanged == null) return;

        string propertyName = GetPropertyName(propertyExpression);
        if (string.IsNullOrEmpty(propertyName)) return;

        OnPropertyChanged(propertyName);
    }
}
