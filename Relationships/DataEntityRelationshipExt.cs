using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Scribe.Core.ConnectorApi;
using Scribe.Core.ConnectorApi.Metadata;

namespace Relationships
{
    /// <summary>
    /// This class provides extension methods that facilitate adding related data to Data Entities.
    /// </summary>
    public static class DataEntityRelationshipExt
    {
        /// <summary>
        /// AddChildren takes a relationship name and an enumerable of DataEntities, and adds them to the current data entity.
        /// </summary>
        /// <param name="source">The DataEntity to add child relationships to.</param>
        /// <param name="relationshipName">The name of the relationship that must match one of the relationships defined in metadata.</param>
        /// <param name="children">The child DataEntities to add.</param>
        /// <returns>The DataEntity with the children added.</returns>
        public static DataEntity AddChildren(this DataEntity source, string relationshipName, IEnumerable<DataEntity> children)
        {
            EnsureDataEntityIsInitialized(source);

            // No need to convert IEnumerable to List if it is already a List
            var childList = children as List<DataEntity>;

            List<DataEntity> value;
            if (source.Children.TryGetValue(relationshipName, out value))
            {
                if (value.Count > 0)
                {
                    throw new InvalidOperationException(string.Format("This data entity already has a non-empty set of data included for the '{0}' relationship.", relationshipName));
                }
            }

            source.Children.Add(relationshipName, childList ?? new List<DataEntity>(children));
            return source;
        }

        /// <summary>
        /// AddParent takes a relationship name and a single DataEntity, and adds it to the current data entity.
        /// NOTE: Notice that this adds the parent to the (counterintuitive)Children property as a (counterintuitive) set rather than as a single.
        /// </summary>
        /// <param name="source">The DataEntity to add child relationships to.</param>
        /// <param name="relationshipName">The name of the relationship that must match one of the relationships defined in metadata.</param>
        /// <param name="parent">The DataEntity to add.</param>
        /// <returns>The DataEntity with the parent added.</returns>
        public static DataEntity AddParent(this DataEntity source, string relationshipName,
            DataEntity parent)
        {
            EnsureDataEntityIsInitialized(source);

            // This is the most confusing aspect -- to add a parent you must create a list of One, then add that list to the Children property
            source.Children.Add(relationshipName, new List<DataEntity>(new[] { parent }));

            return source;
        }

        /// <summary>
        /// Adds children DataEntities to the current DataEntity.
        /// </summary>
        /// <typeparam name="T">A type that is convertible to a DataEntity.</typeparam>
        /// <param name="source">The DataEntity.</param>
        /// <param name="relationshipName">The name of the relationship.</param>
        /// <param name="children">The Children to add.</param>
        /// <param name="converter">A function that converts a T into a DataEntity.</param>
        /// <returns></returns>
        public static DataEntity AddChildren<T>(this DataEntity source, string relationshipName, IEnumerable<T> children, Func<T, DataEntity> converter)
        {
            var convertedChildren = children.Select(converter);
            return source.AddChildren(relationshipName, convertedChildren);
        }

        /// <summary>
        /// Adds a parent to a DataEntity.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="relationshipName"></param>
        /// <param name="parent"></param>
        /// <param name="converter"></param>
        /// <returns></returns>
        public static DataEntity AddParent<T>(this DataEntity source, string relationshipName, T parent, Func<T, DataEntity> converter)
        {
            var convertedParent = converter(parent);
            return source.AddParent(relationshipName, convertedParent);
        }

        public static DataEntity AddChildren(this DataEntity source, IRelationshipDefinition rel,
    IEnumerable<DataEntity> children)
        {
            ValidateChildRelationship(source, rel);
            var childList = children as List<DataEntity> ?? new List<DataEntity>(children);

            foreach (var entity in childList)
            {
                if (entity.ObjectDefinitionFullName != rel.RelatedObjectDefinitionFullName)
                {
                    throw new InvalidOperationException(string.Format("Each entity in the collection of children must be of type {0}, but at least one was of type {1}.", rel.RelatedObjectDefinitionFullName, entity.ObjectDefinitionFullName));
                }
            }

            return source.AddChildren(rel.FullName, childList);
        }

        public static DataEntity AddParent(this DataEntity source, IRelationshipDefinition rel,
            DataEntity parent)
        {
            ValidateParentRelationship(source, parent, rel);

            var relationshipName = rel.FullName;
            return source.AddParent(relationshipName, parent);
        }

        public static DataEntity AddChildren<T>(this DataEntity source, IRelationshipDefinition rel, IEnumerable<T> children, Func<T, DataEntity> converter)
        {
            var dataEntites = children.Select(converter);
            return source.AddChildren(rel, dataEntites);
        }

        public static DataEntity AddParent<T>(this DataEntity source, IRelationshipDefinition rel, T parent, Func<T, DataEntity> converter)
        {
            var converted = converter(parent);

            return source.AddParent(rel, converted);
        }

        private static void EnsureDataEntityIsInitialized(DataEntity entity)
        {
            if (string.IsNullOrEmpty(entity.ObjectDefinitionFullName))
            {
                throw new InvalidOperationException("A DataEntity must have an ObjectDefinitionFullName to be used properly.");
            }

            if (entity.Properties == null)
            {
                throw new InvalidOperationException("A DataEntity's Properties must not be null.");
            }

            if (entity.Children == null)
            {
                throw new InvalidOperationException("A DataEntity's Children property must not be null.");
            }
        }

        private static void ValidateRelationship(IRelationshipDefinition relationshipDefinition)
        {
            if (relationshipDefinition == null)
            {
                throw new ArgumentNullException("relationshipDefinition", "RelationshipDefinition cannot be null.");
            }

            if (string.IsNullOrWhiteSpace(relationshipDefinition.FullName))
            {
                throw new InvalidOperationException("RelationshipDefinition's FullName must not be null or whitespace.");
            }

            if (string.IsNullOrWhiteSpace(relationshipDefinition.Name))
            {
                throw new InvalidOperationException("RelationshipDefinition's Name must not be null or whitespace.");
            }

            if (relationshipDefinition.FullName.Contains("."))
            {
                throw new InvalidOperationException("The Relationship's name must not contain a '.'.");
            }
        }

        private static void ValidateChildRelationship(DataEntity source, IRelationshipDefinition rel)
        {
            ValidateRelationship(rel);
            if (rel.RelationshipType != RelationshipType.Child)
            {
                throw new InvalidOperationException("Adding children must be to a relationship that is defined as type Child.");
            }

            var sourceType = source.ObjectDefinitionFullName;

            if (rel.ThisObjectDefinitionFullName != sourceType)
            {
                throw new InvalidOperationException(string.Format("Attempting to add a relationship to type {0}, but the relationship is defined for type {1}.", sourceType, rel.ThisObjectDefinitionFullName));
            }
        }

        private static DataEntity AddChildrenWithLazyValidation(this DataEntity source, IRelationshipDefinition rel,
    IEnumerable<DataEntity> children)
        {
            ValidateChildRelationship(source, rel);
            var validatingAction = ValidateNames(rel.RelatedObjectDefinitionFullName);
            var validatingEnumerable = children.ValidateEnumerable(validatingAction);

            return source.AddChildren(rel.FullName, validatingEnumerable);
        }

        private static Action<DataEntity> ValidateNames(string validName)
        {
            return (dataEntity) =>
            {
                if (dataEntity.ObjectDefinitionFullName != validName)
                {
                    throw new InvalidOperationException(string.Format("The ObjectDefinitionFullName must be '{0}'.", validName));
                }
            };
        }

        private static IEnumerable<T> ValidateEnumerable<T>(this IEnumerable<T> source, Action<T> validationgAction)
        {
            foreach (var x in source)
            {
                validationgAction(x);
                yield return x;
            }
        }

        private static void ValidateParentRelationship(DataEntity source, DataEntity parent, IRelationshipDefinition rel)
        {
            ValidateRelationship(rel);
            if (rel.RelationshipType != RelationshipType.Parent)
            {
                throw new InvalidOperationException("Adding a parent must be to a relationship that is defined as type Parent.");
            }

            var sourceType = source.ObjectDefinitionFullName;

            if (rel.ThisObjectDefinitionFullName != sourceType)
            {
                throw new InvalidOperationException(string.Format("Attempting to add a relationship to type {0}, but the relationship is defined for type {1}.", sourceType, rel.ThisObjectDefinitionFullName));
            }
            var relatedType = rel.RelatedObjectDefinitionFullName;
            var parentType = parent.ObjectDefinitionFullName;

            if (parentType != relatedType)
            {
                throw new InvalidOperationException(string.Format("When adding a parent using a relationship, the type of the parent [{0}] must match the type defined on the relationship [{1}]", parentType, relatedType));
            }
        }
    }
}