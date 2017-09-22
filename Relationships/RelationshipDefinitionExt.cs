using Scribe.Core.ConnectorApi.Metadata;

namespace Relationships
{
    public static class RelationshipDefinitionExt
    {
        public static IRelationshipDefinition DescribeOneToManyRelationship(this IObjectDefinition source,
            IObjectDefinition that, string thisProperty, string thatProperty, string name = "", string description = "")
        {
            var n = name == string.Empty ? string.Format("{0}_{1}", source.FullName, that.FullName) : name;
            var d = description == string.Empty ? string.Format("Child Relationship from {0}  to {1}.", source.FullName, that.FullName) : description;

            var rel = new RelationshipDefinition();
            rel.Description = d;
            rel.FullName = n;
            rel.Name = n;
            rel.RelatedObjectDefinitionFullName = that.FullName;
            rel.ThisObjectDefinitionFullName = source.FullName;
            rel.RelationshipType = RelationshipType.Child;
            rel.ThisProperties = thisProperty;
            rel.RelatedProperties = thatProperty;

            return rel;
        }

        public static IRelationshipDefinition DescribeManyToOneRelationship(this IObjectDefinition source,
            IObjectDefinition that, string thisProperty, string thatProperty, string name = "", string description = "")
        {
            var n = name == string.Empty ? string.Format("{0}_{1}", source.FullName, that.FullName) : name;
            var d = description == string.Empty ? string.Format("Child Relationship from {0}  to {1}.", source.FullName, that.FullName) : description;

            var rel = new RelationshipDefinition();
            rel.Description = d;
            rel.FullName = n;
            rel.Name = n;
            rel.RelatedObjectDefinitionFullName = that.FullName;
            rel.ThisObjectDefinitionFullName = source.FullName;
            rel.RelationshipType = RelationshipType.Parent;
            rel.ThisProperties = thisProperty;
            rel.RelatedProperties = thatProperty;

            return rel;

        }
    }
}