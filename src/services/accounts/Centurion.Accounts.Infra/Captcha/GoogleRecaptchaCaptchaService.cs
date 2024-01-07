// using System.Collections.Generic;
// using System.Net.Http;
// using System.Threading.Tasks;
// using CSharpFunctionalExtensions;
// using Newtonsoft.Json;
// using Centurion.Accounts.App.Captcha;
// using Centurion.Accounts.App.Config;
//
// // todo: enable recaptcha
// namespace Centurion.Accounts.Infra.Captcha
// {
//   public class GoogleRecaptchaCaptchaService : ICaptchaService
//   {
//     private readonly ReCaptchaConfig _config;
//
//     public GoogleRecaptchaCaptchaService(ReCaptchaConfig config)
//     {
//       _config = config;
//     }
//
//     public async ValueTask<Result> ValidateAsync(string captchaResponse)
//     {
//       using var webClient = new HttpClient();
//       var content = new FormUrlEncodedContent(new[]
//       {
//         new KeyValuePair<string?, string?>("secret", _config.SecretKey),
//         new KeyValuePair<string?, string?>("response", captchaResponse)
//       });
//       var response = await webClient.PostAsync(_config.VerificationUrl, content);
//       var json = await response.Content.ReadAsStringAsync();
//       var reCaptchaResponse = JsonConvert.DeserializeObject<ReCaptchaResponse?>(json);
//       if (reCaptchaResponse == null)
//       {
//         return Result.Failure("ReCaptcha verification unreachable");
//       }
//
//       return !reCaptchaResponse.Success
//         ? Result.Failure(string.Join("\n", reCaptchaResponse.ErrorCodes))
//         : Result.Success();
//     }
//
//     private record ReCaptchaResponse(
//       [property: JsonProperty("success")] bool Success,
//       [property: JsonProperty("challenge_ts")] string Challenge,
//       [property: JsonProperty("hostname")] string HostName,
//       [property: JsonProperty("error-codes")] string[] ErrorCodes);
//   }
// }