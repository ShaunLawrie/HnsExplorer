using System.Text.Json;
namespace HnsExplorer.Extensions
{
    public static class TreeNodeCollectionExtensions
    {
        private readonly static int MAX_DEPTH = 5;

        public static void InsertChildren(this TreeNodeCollection element,
            IEnumerable<JsonElement> elementsToInsert,
            string identifier,
            string nodeTextPrefix,
            string nodeTextSuffix)
        {
            foreach (var item in elementsToInsert)
            {
                var id = item.GetJsonDataAsString(identifier);
                var nodeText = nodeTextPrefix;
                var suffix = item.GetJsonDataAsString(nodeTextSuffix);
                if (!suffix.Contains("No data"))
                {
                    nodeText += $" [{suffix}]";
                }
                element.Add(new TreeNode()
                {
                    Text = nodeText,
                    Name = id.ToLower(),
                    Tag = item
                });
            }
        }

        public static IEnumerable<JsonElement> InsertNestedChildren(this TreeNodeCollection element,
            IEnumerable<JsonElement> elementsToInsert,
            string identifier,
            string parentIdentifier,
            string nodeTextPrefix,
            string nodeTextSuffix,
            int depth = 0)
        {
            var orphans = new List<JsonElement>();
            foreach (var item in elementsToInsert)
            {
                var id = item.GetJsonDataAsString(identifier);
                var nodeText = nodeTextPrefix;
                var suffix = item.GetJsonDataAsString(nodeTextSuffix);
                if (!suffix.Contains("No data"))
                {
                    nodeText += $" [{suffix}]";
                }
                var hasParent = item.HasJsonData(parentIdentifier);
                
                if (!hasParent)
                {
                    element.Add(new TreeNode()
                    {
                        Text = nodeText,
                        Name = id.ToLower(),
                        Tag = item
                    });
                }
                else
                {
                    var parentId = item.GetJsonDataAsString(parentIdentifier).ToLower();
                    var parent = element.Find(parentId, true).FirstOrDefault();
                    if (parent is not null)
                    {
                        parent.Nodes.Add(new TreeNode()
                        {
                            Text = nodeText,
                            Name = id.ToLower(),
                            Tag = item
                        });
                    }
                    else
                    {
                        orphans.Add(item);
                    }
                }
            }

            if(orphans.Count() > 0)
            {
                if (depth >= MAX_DEPTH)
                {
                    return orphans;
                }
                else
                {
                    return element.InsertNestedChildren(orphans, identifier, parentIdentifier, nodeTextPrefix, nodeTextSuffix, ++depth);
                }
            }
            else
            {
                return new List<JsonElement>();
            }
        }

        public static IEnumerable<JsonElement> InsertChildrenWithMatchingParentReference(this TreeNodeCollection element,
            IEnumerable<JsonElement> elementsToInsert,
            IEnumerable<JsonElement> elementsToSearchForParentId,
            string identifier,
            string parentIdentifier,
            string parentFieldReferencingIdentifier,
            string nodeTextPrefix,
            string nodeTextSuffix,
            int depth = 0)
        {
            var orphans = new List<JsonElement>();
            foreach (var item in elementsToInsert)
            {
                var id = item.GetJsonDataAsString(identifier);
                var nodeText = nodeTextPrefix;
                var suffix = item.GetJsonDataAsString(nodeTextSuffix);
                if(!suffix.Contains("No data"))
                {
                    nodeText += $" [{suffix}]";
                }
                
                var parentData = elementsToSearchForParentId.FirstElementMatchingQuery(parentFieldReferencingIdentifier, id);
                var parentId = parentData?.GetJsonDataAsString(parentIdentifier);
                if (parentId is not null)
                {
                    var parent = element.Find(parentId, true).FirstOrDefault();
                    if (parent is not null)
                    {
                        parent.Nodes.Add(new TreeNode()
                        {
                            Text = nodeText,
                            Name = id.ToLower(),
                            Tag = item
                        });
                    }
                }
                else
                {
                    orphans.Add(item);
                }
            }

            if (orphans.Count() > 0)
            {
                if (depth >= MAX_DEPTH)
                {
                    return orphans;
                }
                else
                {
                    return element.InsertChildrenWithMatchingParentReference(orphans,
                        elementsToSearchForParentId,
                        identifier,
                        parentIdentifier,
                        parentFieldReferencingIdentifier,
                        nodeTextPrefix,
                        nodeTextSuffix,
                        ++depth);
                }
            }
            else
            {
                return new List<JsonElement>();
            }
        }
    }
}
