using System.Text.Json;

namespace HnsExplorer.Extensions
{
    public static class JsonElementExtensions
    {
        public static string GetJsonDataAsString(this JsonElement element, string properties)
        {
            var thisPropertyName = properties;
            string remainingProperties = string.Empty;
            if (properties.Contains('.'))
            {
                thisPropertyName = properties.Split('.').First();
                remainingProperties = string.Join('.', properties.Split('.').Skip(1));
            }
            if (element.ValueKind == JsonValueKind.Array)
            {
                var result = new List<string>();
                var arrayItems = element.EnumerateArray();
                foreach (var arrayItem in arrayItems)
                {
                    result.Add(arrayItem.GetJsonDataAsString(properties));
                }
                return string.Join(", ", result);
            }
            else
            {
                if (element.TryGetProperty(thisPropertyName, out JsonElement innerElement))
                {
                    if (remainingProperties.Equals(string.Empty))
                    {
                        if(innerElement.ValueKind == JsonValueKind.Number)
                        {
                            return innerElement.GetInt32().ToString() ?? "No data found";
                        }
                        else
                        {
                            return innerElement.GetString() ?? "No data found";
                        }
                    }
                    else
                    {
                        return GetJsonDataAsString(innerElement, remainingProperties);
                    }
                }
                else
                {
                    return $"No data at [{thisPropertyName}]";
                }
            }
        }

        public static bool HasJsonData(this JsonElement element, string properties)
        {
            var thisPropertyName = properties;
            string remainingProperties = string.Empty;
            if (properties.Contains('.'))
            {
                thisPropertyName = properties.Split('.').First();
                remainingProperties = string.Join('.', properties.Split('.').Skip(1));
            }
            if (element.TryGetProperty(thisPropertyName, out JsonElement innerElement))
            {
                if (remainingProperties.Equals(string.Empty))
                {
                    return true;
                }
                else
                {
                    return HasJsonData(innerElement, remainingProperties);
                }
            }
            else
            {
                return false;
            }
        }

        public static JsonElement? FirstElementMatchingQuery(this IEnumerable<JsonElement> elements, string properties, string expectedValue)
        {
            foreach(var element in elements)
            {
                var match = element.FirstElementMatchingQuery(properties, expectedValue);
                if(match is not null)
                {
                    return match;
                }
            }
            return null;
        }

        public static JsonElement? FirstElementMatchingQuery(this JsonElement element, string properties, string expectedValue)
        {
            var thisPropertyName = properties;
            string remainingProperties = string.Empty;
            if (properties.Contains('.'))
            {
                thisPropertyName = properties.Split('.').First();
                remainingProperties = string.Join('.', properties.Split('.').Skip(1));
            }
            if (element.TryGetProperty(thisPropertyName, out JsonElement innerElement))
            {
                if (remainingProperties.Equals(string.Empty))
                {
                    string result;
                    if (innerElement.ValueKind == JsonValueKind.Number)
                    {
                        result = innerElement.GetInt32().ToString();
                    }
                    else if (innerElement.ValueKind == JsonValueKind.Array)
                    {
                        var arrayValues = innerElement.EnumerateArray();
                        result = string.Join(", ", arrayValues);
                    }
                    else
                    {
                        result = innerElement.GetString() ?? string.Empty;
                    }
                    if(result.Contains(expectedValue))
                    {
                        return element;
                    }
                    else
                    {
                        return null;
                    }
                }
                else
                {
                    return element.FirstElementMatchingQuery(remainingProperties, expectedValue);
                }
            }
            else
            {
                return null;
            }
        }
    }
}
