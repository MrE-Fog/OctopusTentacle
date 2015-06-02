﻿using System;

namespace Octopus.Shared.Properties
{
    /// <summary>
    /// Indicates that the method is contained in a type that implements
    /// <see cref="System.ComponentModel.INotifyPropertyChanged" /> interface
    /// and this method is used to notify that some property value changed
    /// </summary>
    /// <remarks>
    /// The method should be non-static and conform to one of the supported signatures:
    /// <list>
    ///     <item>
    ///         <c>OnPropertyChanged(string)</c>
    ///     </item>
    ///     <item>
    ///         <c>OnPropertyChanged(params string[])</c>
    ///     </item>
    ///     <item>
    ///         <c>NotifyChanged{T}(Expression{Func{T}})</c>
    ///     </item>
    ///     <item>
    ///         <c>NotifyChanged{T,U}(Expression{Func{T,U}})</c>
    ///     </item>
    ///     <item>
    ///         <c>SetProperty{T}(ref T, T, string)</c>
    ///     </item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    ///  public class Foo : INotifyPropertyChanged {
    ///    public event PropertyChangedEventHandler PropertyChanged;
    ///    [NotifyPropertyChangedInvocator]
    ///    protected virtual void OnPropertyChanged(string propertyName) { ... }
    /// 
    ///    private string _name;
    ///    public string Name {
    ///      get { return _name; }
    ///      set { _name = value; OnPropertyChanged("LastName"); /* Warning */ }
    ///    }
    ///  }
    ///  </code>
    /// Examples of generated notifications:
    /// <list>
    ///     <item>
    ///         <c>OnPropertyChanged("Property")</c>
    ///     </item>
    ///     <item>
    ///         <c>OnPropertyChanged(() =&gt; Property)</c>
    ///     </item>
    ///     <item>
    ///         <c>OnPropertyChanged((VM x) =&gt; x.Property)</c>
    ///     </item>
    ///     <item>
    ///         <c>SetProperty(ref myField, value, "Property")</c>
    ///     </item>
    /// </list>
    /// </example>
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class NotifyPropertyChangedInvocatorAttribute : Attribute
    {
        public NotifyPropertyChangedInvocatorAttribute()
        {
        }

        public NotifyPropertyChangedInvocatorAttribute(string parameterName)
        {
            ParameterName = parameterName;
        }

        public string ParameterName { get; private set; }
    }
}