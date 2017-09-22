namespace CDK
{
    using System.Collections.Generic;
    using System.Linq;

    using Scribe.Core.ConnectorApi;
    using Scribe.Core.ConnectorApi.Metadata;
    using Newtonsoft;
    using Newtonsoft.Json;
    using System;
    using System.Reflection;
    using System.Data;
    using Objects.EntityInfo;
    using Relationships;
    using CDK;
    using CDK.Objects;
    using CDK.Objects.Customers;
    using CDK.Objects.Messages;

    public class MetadataProvider : IMetadataProvider
    {
        private IEnumerable<IActionDefinition> actionDefinitions;
        private IEnumerable<IObjectDefinition> objectDefinitions;
        private IEnumerable<IActionDefinition> ActionDefinitions { get { return this.actionDefinitions ?? (this.actionDefinitions = this.GetActionDefinitions()); } }
        private IEnumerable<IObjectDefinition> ObjectDefinitions { get { return this.objectDefinitions ?? (this.objectDefinitions = this.GetObjectDefinitions()); } }

        public Connector Connector;

        public Dictionary<string, string> headerInfo = new Dictionary<string, string>();
        public Dictionary<string, string> relInfo = new Dictionary<string, string>();
        public Dictionary<string, string> TxnInfo = new Dictionary<string, string>();

        public MetadataProvider(Connector connector)
        {
            Connector = connector;
        }

        public IEnumerable<IActionDefinition> RetrieveActionDefinitions()
        {
            return this.ActionDefinitions;
        }

        private IEnumerable<IActionDefinition> GetActionDefinitions()
        {
            return new List<IActionDefinition>
                   {
                        new ActionDefinition
                       {
                            Description = "Create",
                            FullName = "Create",
                            KnownActionType = KnownActions.Create,
                            Name = "Create",
                            SupportsBulk = true,
                            SupportsConstraints = true,
                            SupportsInput = true,
                            SupportsLookupConditions = true,
                            SupportsMultipleRecordOperations = true,
                            SupportsRelations = true,
                            SupportsSequences = true
                       },
                        new ActionDefinition
                        {
                            Description = "Query",
                            FullName = "Query",
                            KnownActionType = KnownActions.Query,
                            Name = "Query",
                            SupportsBulk = false,
                            SupportsConstraints = true,
                            SupportsInput = true,
                            SupportsLookupConditions = true,
                            SupportsMultipleRecordOperations = true,
                            SupportsRelations = true,
                            SupportsSequences = true
                        }
                   };
        }

        private IEnumerable<IObjectDefinition> GetObjectDefinitions()
        {
            var objects = new List<IObjectDefinition>();

            var IncomingMessages = new ObjectDefinition
            {
                FullName = "IncomingMessage",
                Description = "IncomingMessage",
                Hidden = false,
                Name = "IncomingMessage",
                SupportedActionFullNames = new List<string> { "Query" },
                PropertyDefinitions = new List<IPropertyDefinition> { }
            };
            PropertyInfo[] IncomingMessageProperties = typeof(Objects.IncomingMessages.Message).GetProperties();

            foreach (PropertyInfo prop in IncomingMessageProperties)
            {
                var property = new PropertyDefinition
                {
                    Description = prop.Name.ToString(),
                    FullName = prop.Name.ToString(),
                    IsPrimaryKey = false,
                    MaxOccurs = 1,
                    MinOccurs = 0,
                    Name = prop.Name.ToString(),
                    Nullable = true,
                    NumericPrecision = 0,
                    NumericScale = 0,
                    PresentationType = prop.PropertyType.ToString(),
                    PropertyType = prop.PropertyType.ToString(),
                    UsedInActionInput = true,
                    UsedInLookupCondition = true,
                    UsedInQueryConstraint = true,
                    UsedInActionOutput = true,
                    UsedInQuerySelect = true,
                    UsedInQuerySequence = true
                };
                IncomingMessages.PropertyDefinitions.Add(property);
            }
                var Messages = new ObjectDefinition
            {
                FullName = "Message",
                Description = "Message",
                Hidden = false,
                Name = "Message",
                SupportedActionFullNames = new List<string> { "Create" },
                PropertyDefinitions = new List<IPropertyDefinition> { }
            };
            PropertyInfo[] MessageProperties = typeof(Objects.Messages.Message).GetProperties();

            foreach(PropertyInfo prop in MessageProperties)
            {
                var property = new PropertyDefinition
                {
                    Description = prop.Name.ToString(),
                    FullName = prop.Name.ToString(),
                    IsPrimaryKey = false,
                    MaxOccurs = 1,
                    MinOccurs = 0,
                    Name = prop.Name.ToString(),
                    Nullable = true,
                    NumericPrecision = 0,
                    NumericScale = 0,
                    PresentationType = prop.PropertyType.ToString(),
                    PropertyType = prop.PropertyType.ToString(),
                    UsedInActionInput = true,
                    UsedInLookupCondition = true,
                    UsedInQueryConstraint = true,
                    UsedInActionOutput = true,
                    UsedInQuerySelect = true,
                    UsedInQuerySequence = true
                };
                Messages.PropertyDefinitions.Add(property);
            }

            objects.Add(Messages);
            objects.Add(IncomingMessages);

            return objects;

        }        

        public IEnumerable<IObjectDefinition> RetrieveObjectDefinitions(bool shouldGetProperties = false, bool shouldGetRelations = false)
        {
            return this.ObjectDefinitions;
        }

        public IObjectDefinition RetrieveObjectDefinition(string objectName, bool shouldGetProperties = false, bool shouldGetRelations = false)
        {
            return this.ObjectDefinitions.FirstOrDefault(od => od.FullName == objectName);
        }

        public void ResetMetadata()
        {
            // reset metadata
            this.actionDefinitions = this.GetActionDefinitions();
            this.objectDefinitions = this.GetObjectDefinitions();
        }

        public IEnumerable<IMethodDefinition> RetrieveMethodDefinitions(bool shouldGetParameters = false)
        {
            throw new System.NotImplementedException();
        }

        public IMethodDefinition RetrieveMethodDefinition(string objectName, bool shouldGetParameters = false)
        {
            throw new System.NotImplementedException();
        }
        
        public void Dispose()
        {
        }


    }
}