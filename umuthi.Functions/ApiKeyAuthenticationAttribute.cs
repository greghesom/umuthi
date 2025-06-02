using System;
using Microsoft.Azure.Functions.Worker;

namespace umuthi.Functions
{
    /// <summary>
    /// Attribute for marking functions that require API key authentication
    /// This is a marker attribute that works alongside the ApiKeyValidator class
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class ApiKeyAuthenticationAttribute : Attribute
    {
        // This is just a marker attribute
        // The actual validation is performed in ApiKeyValidator.ValidateApiKey
    }
}
