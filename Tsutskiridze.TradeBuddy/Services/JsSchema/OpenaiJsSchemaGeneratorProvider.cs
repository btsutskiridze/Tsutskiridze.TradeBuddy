using Newtonsoft.Json.Schema;
using Newtonsoft.Json.Schema.Generation;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Text.Json.Serialization;

namespace Tsutskiridze.TradeBuddy.Services.JsSchema
{
    public class OpenaiJSSchemaGenerationProvider : JSchemaGenerationProvider
    {
        // This provider will handle any class type.
        public override bool CanGenerateSchema(JSchemaTypeGenerationContext context)
        {
            return IsClass(context.ObjectType);
        }

        private bool IsClass(Type type)
        {
            if (type == typeof(string) || type.IsPrimitive
                || type.IsEnum || type.IsValueType || type.IsArray || type.IsInterface || type.IsAbstract || type.IsGenericType || type.IsGenericTypeDefinition)
                return false;
            return type.IsClass;
        }

        public override JSchema GetSchema(JSchemaTypeGenerationContext context)
        {
            return GenerateSchema(context.ObjectType.GetProperties(), context);
        }

        private JSchema GenerateSchema(PropertyInfo[] properties, JSchemaTypeGenerationContext context)
        {
            // Create a new schema for object types and disable additional properties.
            var schema = new JSchema
            {
                Type = JSchemaType.Object,
                AllowAdditionalProperties = false
            };

            foreach (var property in properties)
            {
                if (property.GetCustomAttributes(typeof(JsonIgnoreAttribute), true).Any())
                {
                    continue;
                }

                var propertySchema = IsClass(property.PropertyType)
                    ? GenerateSchema(property.PropertyType.GetProperties(), context)
                    : context.Generator.Generate(property.PropertyType);

                schema.Properties.Add(property.Name, propertySchema);

                if (property.GetCustomAttributes(typeof(RequiredAttribute), true).Any())
                {
                    schema.Required.Add(property.Name);
                }

                if (property.GetCustomAttributes(typeof(DescriptionAttribute), true).FirstOrDefault() is DescriptionAttribute description)
                {
                    schema.Properties[property.Name].Description = description.Description;
                }
            }
            return schema;
        }
    }
}

