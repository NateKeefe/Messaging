namespace CDK
{
    using System.Collections.ObjectModel;

    using Scribe.Core.ConnectorApi;
    using Scribe.Core.ConnectorApi.Actions;
    using Scribe.Core.ConnectorApi.ConnectionUI;
    using Scribe.Core.ConnectorApi.Query;

    using System;
    using System.Collections.Generic;
    using System.Text;
    using HttpUtils;
    using Newtonsoft;
    using Newtonsoft.Json;
    using Scribe.Core.ConnectorApi.Cryptography;
    using Newtonsoft.Json.Linq;
    using CDK.Objects;
    using Scribe.Core.ConnectorApi.Exceptions;
    using System.Linq;
    using System.Reflection;
    using System.Data;
    using System.IO;
    using Objects.Messages;
    [ScribeConnector(
        ConnectorSettings.ConnectorTypeId,
        ConnectorSettings.Name,
        ConnectorSettings.Description,
        typeof(Connector),
        StandardConnectorSettings.SettingsUITypeName,
        StandardConnectorSettings.SettingsUIVersion,
        StandardConnectorSettings.ConnectionUITypeName,
        StandardConnectorSettings.ConnectionUIVersion,
        StandardConnectorSettings.XapFileName,
        new[] { "Scribe.IS2.Target", "Scribe.IS2.Message" },
        ConnectorSettings.SupportsCloud,
        ConnectorSettings.ConnectorVersion
        )]

    public class Connector : IConnector, ISupportProcessNotifications, ISupportMessage
    {
        public bool IsConnected { get; private set; }
        private readonly Guid connectorTypeId = new Guid(ConnectorSettings.ConnectorTypeId);
        public MetadataProvider metadataProvider;
        public readonly ConnectionInfo info = new ConnectionInfo();
        public RestClient client = new RestClient();
        public string assemblyPrefix = "CDK.Objects.";
        public List<DataSet> allData;

        public string PreConnect(IDictionary<string, string> properties)
        {
            var form = new FormDefinition
            {
                CompanyName = "ScribeLabs",
                CryptoKey = ConnectorSettings.cryptoKey,
                HelpUri = new Uri("http://www.scribesoft.com"),
                Entries =
                        new Collection<EntryDefinition>
                            {
                                new EntryDefinition
                                    {
                                        InputType = InputType.Text,
                                        IsRequired = true,
                                        Label = "API URL",
                                        PropertyName = "BaseURL"
                                    },
                            }
            };

            return form.Serialize();
        }

        public void Connect(IDictionary<string, string> properties)
        {
            //Get Connection Info
            info.BaseURL = properties["BaseURL"];
            if (info.BaseURL.ToString().EndsWith("/"))
            {
                info.BaseURL = info.BaseURL.Remove(info.BaseURL.Length - 1);
            }

            //Finish up Connect
            GetMetadataProvider();
            reconnect();
        }

        public void reconnect()
        {
            //Add Connection validation here
            this.IsConnected = true;
        }

        public void Disconnect()
        {
            this.IsConnected = false;
            this.metadataProvider = null;
        }

        public IMetadataProvider GetMetadataProvider()
        {
            this.metadataProvider = new MetadataProvider(this);

            return this.metadataProvider;
        }

        public IEnumerable<DataEntity> ProcessMessage(string entityName, string message)
        {
            switch (entityName)
            {
                case "IncomingMessage":
                var results = JsonConvert.DeserializeObject<Objects.IncomingMessages.Rootobject>(message);
                    foreach (var webhook in results.Property1)
                    {
                        DataEntity dataEntity = new DataEntity(entityName);
                        PropertyInfo[] messageProperties = typeof(Objects.IncomingMessages.Message).GetProperties();

                        //add payload properties to data entity using reflection
                        foreach (var customerProps in messageProperties)
                        {
                            if (webhook != null)
                            {
                                if (customerProps.PropertyType.Namespace.ToString().StartsWith("System") && !customerProps.PropertyType.IsArray)
                                {
                                    dataEntity.Properties.Add(customerProps.Name.ToString(), customerProps.GetValue(webhook));
                                }
                            }
                        }
                        //return a dataEntity for each record in the payload
                        yield return dataEntity;
                    }
                    break;
                default:
                    throw new InvalidExecuteOperationException($"There is no support for processing '{entityName} messages.");
            }
        }
        public IEnumerable<DataEntity> ExecuteQuery(Query query)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<DataEntity> GetData(string entityName, string json, Query query)
        {
            //Take the data returned from a payload, and deserialize it as an object
            var results = JsonConvert.DeserializeObject<Objects.Customers.Rootobject>(json);

            //      //should be deserializing objects and adding props dynamically based on entityName input
            //      Type RootObjectType = Type.GetType(assemblyPrefix + entityName);
            //      var resultsElement = Activator.CreateInstance(RootObjectType);
            //      resultsElement = JsonConvert.DeserializeObject(json, RootObjectType

            foreach (var cust in results.customers)
            {
                DataEntity dataEntity = new DataEntity(query.RootEntity.ObjectDefinitionFullName);
                PropertyInfo[] customerProperties = typeof(Objects.Customers.Customer).GetProperties();

                //add payload properties to data entity using reflection
                foreach (var customerProps in customerProperties)
                {
                    if (cust != null)
                    {
                        if (customerProps.PropertyType.Namespace.ToString().StartsWith("System") && !customerProps.PropertyType.IsArray)
                        {
                            dataEntity.Properties.Add(customerProps.Name.ToString(), customerProps.GetValue(cust));
                        }
                    }
                }
                //return a dataEntity for each record in the payload
                yield return dataEntity;
            }
        }

        public MethodResult ExecuteMethod(MethodInput input)
        {
            throw new NotImplementedException();
        }

        public OperationResult ExecuteOperation(OperationInput input)
        {
            var operation = input.Name;
            var datas = input.Input;
            var count = datas.Length;
            var entityName = datas[0].ObjectDefinitionFullName;
            var results = new OperationResult();

            this.allData = new List<DataSet>();

            var newDataTable = new DataTable();
            newDataTable.ExtendedProperties.Add("FileName", "JSONData");
            newDataTable.TableName = "data";                            //create a default tablename

            var newDataSet = new DataSet();
            newDataSet.ExtendedProperties.Add("Name", "JSONData");
            newDataSet.Tables.Add(newDataTable);

            this.allData.Add(newDataSet);

            foreach (var ds in allData)
            {
                foreach (DataTable dt in ds.Tables)
                {
                    dt.TableName = entityName;
                    if (dt.TableName == entityName)
                    {
                        switch (operation)
                        {
                            case "Create":
                            case "Upsert":
                                foreach (var data in datas)
                                {
                                    foreach (var inputProps in data.Properties)
                                    {
                                        if (!dt.Columns.Contains(inputProps.Key))
                                        {
                                            dt.Columns.Add(inputProps.Key, typeof(string));
                                        }
                                    }
                                    var newrow = dt.NewRow();
                                    foreach (var inputProps in data.Properties)
                                    {
                                        if (inputProps.Value == null)
                                        {
                                            newrow[inputProps.Key.ToString()] = " ";
                                        }
                                        else newrow[inputProps.Key.ToString()] = inputProps.Value.ToString();
                                    }
                                    dt.Rows.Add(newrow);
                                }

                                Uri url = new Uri(info.BaseURL);
                                client.EndPoint = url.ToString();
                                client.Method = HttpVerb.POST;
                                client.ContentType = "application/json";
                                client.PostData = JsonConvert.SerializeObject(allData, Formatting.Indented);
                                var jsonResponse = client.MakeRequest("");

                                // It always succeeds!
                                var size = input.Input.Length;
                                return new OperationResult
                                {
                                    ErrorInfo = Enumerable.Repeat<ErrorResult>(null, size).ToArray(),
                                    ObjectsAffected = Enumerable.Repeat(1, size).ToArray(),
                                    Output = input.Input,
                                    Success = Enumerable.Repeat(true, size).ToArray()

                                };
                            default:
                                var sizes = input.Input.Length;
                                return new OperationResult
                                {
                                    ErrorInfo = Enumerable.Repeat<ErrorResult>(null, sizes).ToArray(),
                                    ObjectsAffected = Enumerable.Repeat(1, sizes).ToArray(),
                                    Output = input.Input,
                                    Success = Enumerable.Repeat(true, sizes).ToArray()
                                };

                        }

                    }
                }
            }
            return results;
        }

        public Guid ConnectorTypeId
        {
            get
            {
                return connectorTypeId;
            }
        }

        public void ProcessEnded(Guid processId, bool success)
        {

        }

        public void ProcessStarted(Guid processId)
        {

        }

        private void ParseWhereClause(StringBuilder whereClause, Expression lookupCondition)
        {
            if (lookupCondition != null)
            {

                switch (lookupCondition.ExpressionType)
                {
                    case ExpressionType.Comparison:
                        var comparisonExpression = lookupCondition as ComparisonExpression;

                        //validate
                        if (comparisonExpression == null)
                        {
                            throw new InvalidOperationException("Invalid Comparision Expression");
                        }
                        // build up the expression
                        var expressionBuilder = new StringBuilder();

                        expressionBuilder.Append(GetLeftFormattedComparisonValue(comparisonExpression.LeftValue));

                        //change the operator if a null value is referenced
                        //valid queries may allow [Column Name] = NULL
                        ParseNullOperators(comparisonExpression);

                        expressionBuilder.AppendFormat(" {0} ", ParseComparisionOperator(comparisonExpression.Operator));

                        if (OperatorHasRightValue(comparisonExpression.Operator))
                        {
                            expressionBuilder.Append(GetRightFormattedComparisonValue(comparisonExpression.LeftValue, comparisonExpression.RightValue));
                        }

                        var sqlFormattedExpression = expressionBuilder.ToString();

                        whereClause.Append(sqlFormattedExpression);
                        break;

                    case ExpressionType.Logical:
                        var logicalExpression = lookupCondition as LogicalExpression;

                        if (logicalExpression == null)
                        {
                            throw new InvalidOperationException("Invalid Logical Expression");
                        }

                        ParseWhereClause(whereClause, logicalExpression.LeftExpression);

                        switch (logicalExpression.Operator)
                        {
                            case LogicalOperator.And:
                                whereClause.Append(" %26%26 ");
                                break;
                            case LogicalOperator.Or:
                                whereClause.Append(" || ");
                                break;
                            default:
                                throw new NotSupportedException(string.Format("UNSUPPORTED LOGICAL OPERATION: {0}", logicalExpression.Operator));
                        }

                        ParseWhereClause(whereClause, logicalExpression.RightExpression);
                        break;
                }

            }

            return;
        }

        private bool OperatorHasRightValue(ComparisonOperator @operator)
        {
            var onlyLeft = @operator == ComparisonOperator.IsNull || @operator == ComparisonOperator.IsNotNull;

            return !onlyLeft;
        }

        private static string GetLeftFormattedComparisonValue(ComparisonValue comparisonValue)
        {
            var formattedComparisonValue = new StringBuilder();

            //Check if value is of type Property, which would indicate a reference to a column name in sql
            if (comparisonValue.ValueType == ComparisonValueType.Property)
            {
                //split property hierarchy
                var comparisonHierarchy = comparisonValue.Value.ToString().Split('.')[1].ToString();
                formattedComparisonValue.Append(comparisonHierarchy);
            }
            else
            {
                // Values that are constant need to be enclosed in single quotes.
                formattedComparisonValue.Append(
                    string.Format(comparisonValue.ValueType == ComparisonValueType.Constant ? "'{0}'" : "{0}",
                                  comparisonValue.Value));
            }

            return formattedComparisonValue.ToString();
        }
            
        private string GetRightFormattedComparisonValue(ComparisonValue leftValue, ComparisonValue rightValue)
        {
            object comparisonValue;

            ////check if the left value is a property, which in this case would be a column name
            //if (leftValue.ValueType == ComparisonValueType.Property && _columnDefinitions != null)
            //{
            //    //retrieve the name of the column from the right value.
            //    //The Incomming format is [Column Name]
            //    //use the datatypes stored in the column definitions to propery convert the data stored in the right value
            //    comparisonValue = DataTypeConverter.ToSqlValue(leftValue.Value.ToString().Split('.').Last(), rightValue.Value, _columnDefinitions);
            //}
            //else
            //{
                comparisonValue = rightValue.Value;
            //}

            //bool valueIsDate = (comparisonValue is DateTime);
            string value = comparisonValue.ToString();
            string result;

            //if (valueIsDate)
            //{
            //    DateTime dateTimeValue = ((DateTime)(comparisonValue));

            //    if (dateTimeValue.Kind != DateTimeKind.Utc)
            //    {
            //        dateTimeValue = dateTimeValue.ToUniversalTime();
            //    }

            //    value = dateTimeValue.ToString("s");

            //    result = string.Format("{0}({1}, '{2}')", "CONVERT", "DATETIME", value);
            //}
            //else
            //{
                result = string.Format(rightValue.ValueType == ComparisonValueType.Constant ? "'{0}'" : "{0}", value);
            //}

            return result;
        }

        private static string ParseComparisionOperator(ComparisonOperator comparisonOperator)
        {
            string comparisonString;

            switch (comparisonOperator)
            {
                case ComparisonOperator.Equal:
                    comparisonString = "==";
                    break;
                case ComparisonOperator.Greater:
                    comparisonString = ">";
                    break;
                case ComparisonOperator.GreaterOrEqual:
                    comparisonString = ">=";
                    break;
                case ComparisonOperator.IsNotNull:
                    comparisonString = "!= NULL";
                    break;
                case ComparisonOperator.IsNull:
                    comparisonString = "== NULL";
                    break;
                case ComparisonOperator.Less:
                    comparisonString = "<";
                    break;
                case ComparisonOperator.LessOrEqual:
                    comparisonString = "<=";
                    break;
                case ComparisonOperator.Like:
                    comparisonString = "=~";
                    break;
                case ComparisonOperator.NotLike:
                    comparisonString = "!=~";
                    break;
                case ComparisonOperator.NotEqual:
                    comparisonString = "!=";
                    break;
                default:
                    throw new NotSupportedException(string.Format("The comparison operator {0} is not supported.", comparisonOperator));
            }

            return comparisonString;
        }

        private void ParseNullOperators(ComparisonExpression comparisonExpression)
        {
            //check for any form of a null right expression
            if (comparisonExpression.RightValue == null ||
                comparisonExpression.RightValue.Value == null)
            {
                //set the appropriate operator
                switch (comparisonExpression.Operator)
                {
                    case ComparisonOperator.Equal:
                        comparisonExpression.Operator = ComparisonOperator.IsNull;
                        break;
                    case ComparisonOperator.NotEqual:
                        comparisonExpression.Operator = ComparisonOperator.IsNotNull;
                        break;
                    case ComparisonOperator.IsNull:
                    case ComparisonOperator.IsNotNull:
                        break;
                    //default:
                        //throw new NotSupportedException(string.Format(ErrorCodes.NullOperatorNotValid.Description, comparisonExpression.Operator));

                }
            }
        }

    }
}
