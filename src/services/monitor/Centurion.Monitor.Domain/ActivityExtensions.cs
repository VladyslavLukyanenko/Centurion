// using System;
// using System.Diagnostics;
// using System.Net.Http;
// using System.Net.Http.Headers;
// using CSharpFunctionalExtensions;
// using OpenTelemetry.Trace;
//
// namespace Centurion.Monitor.Domain
// {
//   public static class ActivityExtensions
//   {
//     public static void SetStatusIfUnset<T>(this Activity self, Result<T> result)
//     {
//       if (self.GetStatus() != Status.Unset)
//       {
//         return;
//       }
//
//       self.SetStatus(result.IsSuccess ? Status.Ok : Status.Error.WithDescription(result.Error));
//       if (result.IsFailure)
//       {
//         self.RecordException(new InvalidOperationException(result.Error));
//       }
//     }
//
//     public static void SetStatusIfUnset(this Activity self, Result result)
//     {
//       if (self.GetStatus() != Status.Unset)
//       {
//         return;
//       }
//
//       self.SetStatus(result.IsSuccess ? Status.Ok : Status.Error.WithDescription(result.Error));
//     }
//
//     public static void SetHttpRequestMessage(this Activity self, HttpRequestMessage requestMessage)
//     {
//       self.SetTag("http.version", requestMessage.Version);
//       self.SetTag("http.method", requestMessage.Method);
//       self.SetTag("http.url", requestMessage.RequestUri);
//
//       self.AddHttpHeaders(requestMessage.Headers);
//       if (requestMessage.Content != null)
//       {
//         self.AddHttpHeaders(requestMessage.Content.Headers);
//       }
//     }
//
//     private static void AddHttpHeaders(this Activity self, HttpHeaders headers)
//     {
//       foreach (var header in headers)
//       {
//         self.AddTag("http.header." + header.Key, string.Join("\n", header.Value));
//       }
//     }
//
//     public static void SetHttpResponseMessage(this Activity self, HttpResponseMessage response)
//     {
//       self.SetTag("http.status_code", (int) response.StatusCode);
//       self.AddHttpHeaders(response.TrailingHeaders);
//       self.AddHttpHeaders(response.Headers);
//       self.AddHttpHeaders(response.Content.Headers);
//     }
//   }
// }