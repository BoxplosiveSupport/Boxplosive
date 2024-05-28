using nl.boxplosive.Data.Web.Webhook.Models;
using System;
using System.Collections.Generic;

namespace nl.boxplosive.BackOffice.Mvc.Models.Webhook
{
    public class WebhookEventModel
    {
        public string Id { get; }
        public DateTime PostedDate { get; }
        public string Version { get; }
        public string RequestId { get; }
        public int HttpResponseCode { get; }
        public string HttpResponseErrorMessage { get; }
        public DateTime? ReplayedDate { get; }
        public string EventType { get; }
        public IDictionary<string, object> Data { get; }

        public string State =>
            HttpResponseCode >= 200 && HttpResponseCode <= 299 ? WebhookEventState.Succeeded.ToString() : WebhookEventState.Failed.ToString();

        public WebhookEventModel()
        {
        }

        public WebhookEventModel(WebhookEventGetResult webhookEvent)
        {
            Id = webhookEvent.Id;
            PostedDate = webhookEvent.PostedDate;
            Version = webhookEvent.Version;
            RequestId = webhookEvent.RequestId;
            HttpResponseCode = webhookEvent.HttpResponseCode;
            HttpResponseErrorMessage = webhookEvent.HttpResponseErrorMessage;
            ReplayedDate = webhookEvent.ReplayedDate;
            EventType = webhookEvent.EventType;
            Data = webhookEvent.Data;
        }
    }
}