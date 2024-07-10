﻿// Copyright (c) Criteo Technology. All rights reserved.
// Licensed under the Apache 2.0 License. See LICENSE in the project root for license information.

using Criteo.OpenApi.Comparator.Comparators.Extensions;
using Microsoft.OpenApi.Models;

namespace Criteo.OpenApi.Comparator.Comparators
{
    internal class RequestBodyComparator
    {
        private readonly ContentComparator _contentComparator;

        internal RequestBodyComparator(ContentComparator contentComparator)
        {
            _contentComparator = contentComparator;
        }

        internal void Compare(ComparisonContext context,
            OpenApiRequestBody oldRequestBody, OpenApiRequestBody newRequestBody)
        {
            context.Direction = DataDirection.Request;

            if (oldRequestBody == null && newRequestBody == null)
                return;

            if (oldRequestBody == null)
            {
                context.LogBreakingChange(ComparisonRules.AddedRequestBody);
                return;
            }

            if (newRequestBody == null)
            {
                context.LogBreakingChange(ComparisonRules.RemovedRequestBody);
                return;
            }

            if (!string.IsNullOrWhiteSpace(oldRequestBody.Reference?.ReferenceV3))
            {
                oldRequestBody = oldRequestBody.Reference.Resolve(context.OldOpenApiDocument.Components.RequestBodies);
                if (oldRequestBody == null)
                    return;
            }

            if (!string.IsNullOrWhiteSpace(newRequestBody.Reference?.ReferenceV3))
            {
                newRequestBody = newRequestBody.Reference.Resolve(context.NewOpenApiDocument.Components.RequestBodies);
                if (newRequestBody == null)
                    return;
            }

            CompareRequired(context, oldRequestBody.Required, newRequestBody.Required);

            _contentComparator.Compare(context, oldRequestBody.Content, newRequestBody.Content);

            context.Direction = DataDirection.None;
        }

        private static void CompareRequired(ComparisonContext context,
            bool oldRequired, bool newRequired)
        {
            if (oldRequired != newRequired)
            {
                context.PushProperty("required");
                if (newRequired)
                {
                    context.LogBreakingChange(ComparisonRules.RequiredStatusChange, oldRequired, newRequired);
                }
                else
                {
                    context.LogInfo(ComparisonRules.RequiredStatusChange, oldRequired, newRequired);
                }
                context.Pop();
            }
        }
    }
}
