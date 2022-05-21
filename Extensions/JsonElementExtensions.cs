using System.Text.Json;

namespace HnsExplorer.Extensions
{
    public static class JsonElementExtensions
    {
        public static string GetJsonDataAsString(this JsonElement element, string properties)
        {
            try
            {
                var multiProperty = properties.Split(',');
                var resultText = new List<string>();
                foreach (var property in multiProperty)
                {
                    if(property.StartsWith("'") && property.EndsWith("'"))
                    {
                        resultText.Add(property[1..^1]);
                        continue;
                    }
                    var thisPropertyName = property;
                    string remainingProperties = string.Empty;
                    if (property.Contains('.'))
                    {
                        thisPropertyName = property.Split('.').First();
                        remainingProperties = string.Join('.', property.Split('.').Skip(1));
                    }

                    if (element.ValueKind == JsonValueKind.Array)
                    {
                        var result = new List<string>();
                        var arrayItems = element.EnumerateArray();
                        foreach (var arrayItem in arrayItems)
                        {
                            result.Add(arrayItem.GetJsonDataAsString(property));
                        }
                        resultText.Add(string.Join(", ", result.Where(s => !string.IsNullOrEmpty(s))));
                    }
                    else
                    {
                        if (element.TryGetProperty(thisPropertyName, out JsonElement innerElement))
                        {
                            if (remainingProperties.Equals(string.Empty))
                            {
                                if (innerElement.ValueKind == JsonValueKind.Array)
                                {
                                    var elements = innerElement.EnumerateArray();
                                    foreach (var elementItem in elements)
                                    {
                                        if (elementItem.ValueKind == JsonValueKind.Number)
                                        {
                                            resultText.Add(elementItem.GetInt32().ToString() ?? "No data found");
                                        }
                                        else
                                        {
                                            resultText.Add(elementItem.GetString() ?? "No data found");
                                        }
                                    }
                                }
                                else
                                {
                                    if (innerElement.ValueKind == JsonValueKind.Number)
                                    {
                                        resultText.Add(innerElement.GetInt32().ToString() ?? "No data found");
                                    }
                                    else
                                    {
                                        resultText.Add(innerElement.GetString() ?? "No data found");
                                    }
                                }
                            }
                            else
                            {
                                resultText.Add(GetJsonDataAsString(innerElement, remainingProperties));
                            }
                        }
                        else
                        {
                            // no data
                        }
                    }
                }
                return string.Join("", resultText);
            }
            catch (Exception ex)
            {
                return $"Failed to get property [{properties}] detail: {ex.Message}";
            }
        }

        public static bool HasJsonData(this JsonElement element, string properties)
        {
            try
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
            catch
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
            try
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
                        if (result.Contains(expectedValue, StringComparison.InvariantCultureIgnoreCase))
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
            catch
            {
                return null;
            }
        }
    }
}
