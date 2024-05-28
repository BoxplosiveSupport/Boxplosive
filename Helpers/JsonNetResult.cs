using System;
using System.Text;
using System.Web.Mvc;
using nl.boxplosive.Utilities.Json;
using Newtonsoft.Json;

namespace nl.boxplosive.BackOffice.Mvc.Helpers
{
    public class JsonNetResult : ActionResult
    {
        public Encoding ContentEncoding { get; set; }
        public string ContentType { get; set; }
        public object Data { get; set; }

        public JsonSerializerSettings SerializerSettings { get; set; }
        public Formatting Formatting { get; set; }

        public JsonNetResult(object data, Formatting formatting)
            : this(data)
        {
            Formatting = formatting;
        }

        public JsonNetResult(object data)
            : this()
        {
            Data = data;
        }

        public JsonNetResult()
        {
            Formatting = Formatting.None;
            SerializerSettings = new JsonSerializerSettings();
        }

        public override void ExecuteResult(ControllerContext context)
        {
            if (context == null)
                throw new ArgumentNullException("context");
            var response = context.HttpContext.Response;
            response.ContentType = !string.IsNullOrEmpty(ContentType)
              ? ContentType
              : "application/json";
            if (ContentEncoding != null)
                response.ContentEncoding = ContentEncoding;

            if (Data == null) return;

            var writer = new JsonTextWriter(response.Output) { Formatting = Formatting };
            JsonWrapper.Serialize(writer, Data, SerializerSettings);
            writer.Flush();
        }
    }
}