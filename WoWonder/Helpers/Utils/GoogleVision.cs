using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace WoWonder.Helpers.Utils
{
    public class GoogleVision
    {
        static readonly string UrlVisionApi = "https://vision.googleapis.com/v1/images:annotate?key=";

        public static async Task<GoogleVisionData.ResponseJson.RootObject> CheckVisionImage(byte[] image)
        {
            // Create new record 
            var str = JsonConvert.SerializeObject(GoogleVisionData.RequestJson.GetNewRequest(image));
            var content = new StringContent(str, Encoding.UTF8, "application/json");

            try
            {
                using var cl = new HttpClient();
                var res = await cl.PostAsync(UrlVisionApi + ListUtils.SettingsSiteList?.VisionApiKey, content);
                var resStr = await res.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<GoogleVisionData.ResponseJson.RootObject>(resStr);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return null;
            }
        }

        public class GoogleVisionData
        {
            public class RequestJson
            {
                public static RootObject GetNewRequest(byte[] imageBytes)
                {
                    return new RootObject
                    {
                        Requests = new List<Request>
                        {
                            new Request
                            {
                                Image = new Image
                                {
                                    Content = Convert.ToBase64String(imageBytes)
                                },
                                Features = new List<Feature>
                                {
                                    new Feature
                                    {
                                        Type = "SAFE_SEARCH_DETECTION",
                                        MaxResults = 1
                                    },
                                    new Feature
                                    {
                                        Type = "WEB_DETECTION",
                                        MaxResults = 2
                                    }
                                }
                            }
                        }
                    };
                }

                public class Image
                {
                    [JsonProperty("source", NullValueHandling = NullValueHandling.Ignore)]
                    public Source Source { get; set; }

                    [JsonProperty("content", NullValueHandling = NullValueHandling.Ignore)]
                    public string Content { get; set; }
                }

                public class Source
                {
                    [JsonProperty("imageUri", NullValueHandling = NullValueHandling.Ignore)]
                    public string ImageUri { get; set; }
                }

                public class Feature
                {
                    /// <summary>
                    /// TEXT_DETECTION
                    /// SAFE_SEARCH_DETECTION
                    /// </summary>
                    [JsonProperty("type", NullValueHandling = NullValueHandling.Ignore)]
                    public string Type { get; set; } = "SAFE_SEARCH_DETECTION";

                    [JsonProperty("maxResults", NullValueHandling = NullValueHandling.Ignore)]
                    public long? MaxResults { get; set; } = 1;
                }

                public class RootObject
                {
                    [JsonProperty("requests", NullValueHandling = NullValueHandling.Ignore)]
                    public List<Request> Requests { get; set; }
                }

                public class Request
                {
                    [JsonProperty("image", NullValueHandling = NullValueHandling.Ignore)]
                    public Image Image { get; set; }

                    [JsonProperty("features", NullValueHandling = NullValueHandling.Ignore)]
                    public List<Feature> Features { get; set; }
                }

            }

            public class ResponseJson
            {
                public class RootObject
                {
                    [JsonProperty("responses", NullValueHandling = NullValueHandling.Ignore)]
                    public List<Response> Responses { get; set; }

                    [JsonProperty("error", NullValueHandling = NullValueHandling.Ignore)]
                    public Error Error { get; set; }
                }

                public class Error
                {
                    [JsonProperty("code", NullValueHandling = NullValueHandling.Ignore)]
                    public long? Code { get; set; }

                    [JsonProperty("message", NullValueHandling = NullValueHandling.Ignore)]
                    public string Message { get; set; }

                    [JsonProperty("status", NullValueHandling = NullValueHandling.Ignore)]
                    public string Status { get; set; }

                    [JsonProperty("details", NullValueHandling = NullValueHandling.Ignore)]
                    public List<Detail> Details { get; set; }
                }

                public class Detail
                {
                    [JsonProperty("@type", NullValueHandling = NullValueHandling.Ignore)]
                    public string Type { get; set; }

                    [JsonProperty("fieldViolations", NullValueHandling = NullValueHandling.Ignore)]
                    public List<FieldViolation> FieldViolations { get; set; }
                }

                public class FieldViolation
                {
                    [JsonProperty("description", NullValueHandling = NullValueHandling.Ignore)]
                    public string Description { get; set; }
                }


                public class Response
                {
                    [JsonProperty("safeSearchAnnotation", NullValueHandling = NullValueHandling.Ignore)]
                    public SafeSearchAnnotation SafeSearchAnnotation { get; set; }

                    [JsonProperty("webDetection", NullValueHandling = NullValueHandling.Ignore)]
                    public WebDetection WebDetection { get; set; }
                }

                public class SafeSearchAnnotation
                {
                    [JsonProperty("adult", NullValueHandling = NullValueHandling.Ignore)]
                    public string Adult { get; set; }

                    [JsonProperty("spoof", NullValueHandling = NullValueHandling.Ignore)]
                    public string Spoof { get; set; }

                    [JsonProperty("medical", NullValueHandling = NullValueHandling.Ignore)]
                    public string Medical { get; set; }

                    [JsonProperty("violence", NullValueHandling = NullValueHandling.Ignore)]
                    public string Violence { get; set; }

                    [JsonProperty("racy", NullValueHandling = NullValueHandling.Ignore)]
                    public string Racy { get; set; }
                }

                public class WebDetection
                {
                    [JsonProperty("webEntities", NullValueHandling = NullValueHandling.Ignore)]
                    public List<WebEntity> WebEntities { get; set; }

                    [JsonProperty("fullMatchingImages", NullValueHandling = NullValueHandling.Ignore)]
                    public List<FullMatchingImage> FullMatchingImages { get; set; }

                    [JsonProperty("pagesWithMatchingImages", NullValueHandling = NullValueHandling.Ignore)]
                    public List<PagesWithMatchingImage> PagesWithMatchingImages { get; set; }

                    [JsonProperty("bestGuessLabels", NullValueHandling = NullValueHandling.Ignore)]
                    public List<BestGuessLabel> BestGuessLabels { get; set; }
                }

                public class BestGuessLabel
                {
                    [JsonProperty("label", NullValueHandling = NullValueHandling.Ignore)]
                    public string Label { get; set; }

                    [JsonProperty("languageCode", NullValueHandling = NullValueHandling.Ignore)]
                    public string LanguageCode { get; set; }
                }

                public class FullMatchingImage
                {
                    [JsonProperty("url", NullValueHandling = NullValueHandling.Ignore)]
                    public Uri Url { get; set; }
                }

                public class PagesWithMatchingImage
                {
                    [JsonProperty("url", NullValueHandling = NullValueHandling.Ignore)]
                    public Uri Url { get; set; }

                    [JsonProperty("pageTitle", NullValueHandling = NullValueHandling.Ignore)]
                    public string PageTitle { get; set; }

                    [JsonProperty("fullMatchingImages", NullValueHandling = NullValueHandling.Ignore)]
                    public List<FullMatchingImage> FullMatchingImages { get; set; }
                }

                public class WebEntity
                {
                    [JsonProperty("entityId", NullValueHandling = NullValueHandling.Ignore)]
                    public string EntityId { get; set; }

                    [JsonProperty("score", NullValueHandling = NullValueHandling.Ignore)]
                    public double? Score { get; set; }

                    [JsonProperty("description", NullValueHandling = NullValueHandling.Ignore)]
                    public string Description { get; set; }
                }


            }
        }

    }
}