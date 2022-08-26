﻿using System.Collections.Generic;
using System.Linq;
using Microsoft.OpenApi.Models;

namespace Criteo.OpenApi.Comparator.Comparators
{
    public class ContentComparator
    {
        private readonly SchemaComparator _schemaComparator;

        public ContentComparator(SchemaComparator schemaComparator)
        {
            _schemaComparator = schemaComparator;
        }

        public IEnumerable<ComparisonMessage> Compare(ComparisonContext<OpenApiDocument> context,
            IDictionary<string, OpenApiMediaType> oldContent, IDictionary<string, OpenApiMediaType> newContent)
        {
            oldContent = oldContent ?? new Dictionary<string, OpenApiMediaType>();
            newContent = newContent ?? new Dictionary<string, OpenApiMediaType>();

            context.PushProperty("content");

            var removedMediaTypes = oldContent.Keys.Except(newContent.Keys);
            var addedMediaTypes = newContent.Keys.Except(oldContent.Keys);
            var commonMediaTypes = oldContent.Keys.Intersect(newContent.Keys);

            foreach (var removedMediaType in removedMediaTypes)
            {
                context.PushProperty(removedMediaType);
                var comparisonMessage = context.Direction == DataDirection.Request
                    ? ComparisonMessages.RequestBodyFormatNoLongerSupported
                    : ComparisonMessages.ResponseBodyInOperationFormatNoLongerSupported;
                context.LogBreakingChange(comparisonMessage, removedMediaType);
                context.Pop();
            }

            foreach (var addedMediaType in addedMediaTypes)
            {
                context.PushProperty(addedMediaType);
                var comparisonMessage = context.Direction == DataDirection.Request
                    ? ComparisonMessages.RequestBodyFormatNowSupported
                    : ComparisonMessages.ResponseBodyFormatNowSupported;
                context.LogInfo(comparisonMessage, addedMediaType);
                context.Pop();
            }

            foreach (var commonMediaType in commonMediaTypes)
            {
                oldContent.TryGetValue(commonMediaType, out var oldMediaType);
                newContent.TryGetValue(commonMediaType, out var newMediaType);

                context.PushProperty(commonMediaType);
                CompareMediaType(context, oldMediaType, newMediaType);
                context.Pop();
            }

            context.Pop();

            return context.Messages;
        }

        private void CompareMediaType(
            ComparisonContext<OpenApiDocument> context,
            OpenApiMediaType oldMediaType,
            OpenApiMediaType newMediaType)
        {
            context.PushProperty("schema");
            _schemaComparator.Compare(context, oldMediaType?.Schema, newMediaType?.Schema);
            context.Pop();
        }
    }
}